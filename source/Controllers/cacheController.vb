
Option Explicit On
Option Strict On

Imports System.Text.RegularExpressions
Imports Contensive.BaseClasses
Imports System.Linq
Imports Contensive.Addons.xxxxxCollectionNameSpaceGoesHerexxxxx


Namespace Controllers
    ''' <summary>
    ''' Ignore this class for now.
    ''' Best Practices
    ''' - do not cache sub-objects in objects, they are very difficult to know when to invalidate
    ''' - A cache object typically contains data from a primary database record. Use getobjectCacheName() to generate cache name
    ''' - If a cache object contains data from other records, write cache class methods to clearCacheObject_ for all objects effected
    ''' </summary>
    Public Class cacheController
        Implements IDisposable
        '
        ' store just for live of object. Cannot load in constructor without glocal code update
        Private Const invalidationDaysDefault As Double = 365
        Private cp As CPBaseClass
        Private mc As Enyim.Caching.MemcachedClient
        Private rightNow As Date = Now
        Private cacheLogFilename As String
        Private json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
        '
        <Serializable()>
        Public Class cacheDataClass
            Public tagList As New List(Of String)
            Public saveDate As Date
            Public invalidationDate As Date
            Public data As Object
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' new constructor. Initializes properties and cache client. For now, called from each public routine. Eventually this is contructor.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cp As CPBaseClass)
            Dim logMsg As String = "new performanceCache instance(" & cp.Doc.StartTime & ")"
            Dim hint As String = "0"
            Try
                rightNow = Now()
                cacheLogFilename = "cacheLog" & rightNow.Year.ToString() & rightNow.Month.ToString().PadLeft(2, "0"c) & rightNow.Day.ToString().PadLeft(2, "0"c) & ".txt"
                hint &= ",1"
                If True Then
                    Me.cp = cp
                    Dim serverPortSplit() As String
                    Dim port  as Integer
                    Dim awsElastiCacheConfigurationEndpoint As String
                    Dim cacheConfig As Amazon.ElastiCacheCluster.ElastiCacheClusterConfig
                    Dim testKey As String = encodeCacheKey("cacheTest")
                    Dim saveValue As String = "123"
                    '
                    ' setup cache
                    _cacheIsLocal = True
                    awsElastiCacheConfigurationEndpoint = cp.Site.GetText("performanceCloud ElastiCacheConfigurationEndpoint")
                    If String.IsNullOrEmpty(awsElastiCacheConfigurationEndpoint) Then
                        '
                        'logMsg &= ", elasticache disabled"
                        '
                    Else
                        '
                        ' site property set, attempt connection
                        serverPortSplit = awsElastiCacheConfigurationEndpoint.Split(CChar(":"))
                        If serverPortSplit.Count > 1 Then
                            port = cp.Utils.EncodeInteger(serverPortSplit(1))
                        Else
                            port = 11211
                        End If
                        hint &= ",7"
                        cacheConfig = New Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(serverPortSplit(0), port)
                        hint &= ",8"
                        cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary

                        hint &= ",9"
                        mc = New Enyim.Caching.MemcachedClient(cacheConfig)
                        _cacheIsLocal = False
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in cacheClass constructor, hint [" & hint & "]")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' false if elasticache has initialized successfully
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property cacheIsLocal As Boolean
            Get
                Return _cacheIsLocal
            End Get
        End Property
        Private _cacheIsLocal As Boolean = True
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the cached value of the site property AllowBake. Private because newNew must have already been called.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property allowCache As Boolean
            Get
                If Not _allowCacheLoaded Then
                    _allowCacheLoaded = True
                    _allowCache = cp.Site.GetBoolean("ALLOWBAKE")
                End If
                Return _allowCache
            End Get
        End Property
        Private _allowCacheLoaded As Boolean = False
        Private _allowCache As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property globalInvalidationDate As Date
            Get
                Dim dataObject As Object
                '
                If Not _globalInvalidationDateLoaded Then
                    _globalInvalidationDateLoaded = True
                    dataObject = readRaw("globalInvalidationDate")
                    If TypeOf (dataObject) Is Date Then
                        _globalInvalidationDate = DirectCast(dataObject, Date)
                    ElseIf TypeOf (dataObject) Is String Then
                        _globalInvalidationDate = cp.Utils.EncodeDate(dataObject)
                    Else
                        _globalInvalidationDate = New Date(1990, 8, 7)
                    End If
                End If
                Return _globalInvalidationDate
            End Get
        End Property
        Private _globalInvalidationDateLoaded As Boolean = False
        Private _globalInvalidationDate As Date
        '
        '====================================================================================================
        ''' <summary>
        ''' write object directly to cache, no tests. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="data">Either a string, a date, or a serializable object</param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub saveRaw(ByVal Key As String, ByVal data As Object, invalidationDate As Date)
            Try
                Dim dataString As String
                '
                If TypeOf (data) Is String Then
                    dataString = DirectCast(data, String)
                ElseIf TypeOf (data) Is Date Then
                    dataString = CDate(data).ToString
                Else
                    dataString = json_serializer.Serialize(data)
                    'dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data)
                End If
                If cacheIsLocal Then
                    cp.Cache.Save(encodeCacheKey(Key), dataString, "", invalidationDate)
                Else
                    Call mc.Store(Enyim.Caching.Memcached.StoreMode.Set, encodeCacheKey(Key), dataString, invalidationDate)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in performanceCache.saveRaw")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' write a cacheData object to ae.pcache. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="data"></param>
        ''' <param name="invalidationDate"></param>
        ''' <remarks></remarks>
        Private Sub saveCacheData(ByVal Key As String, ByVal data As cacheDataClass, Optional invalidationDate As Date = #12:00:00 AM#)
            Try
                '
                If invalidationDate <= Date.MinValue Then
                    ' add random component so everything does not clear all at once
                    invalidationDate = Now.AddDays(invalidationDaysDefault + Rnd())
                End If
                If (Key = "") Then
                    cp.Site.ErrorReport("Exception in cache_saveRaw, key cannot be empty")
                ElseIf (invalidationDate <= Now()) Then
                    cp.Site.ErrorReport("Exception in cache_saveRaw, invalidationDate must be > current date/time")
                Else
                    Dim allowSave As Boolean = False
                    If data Is Nothing Then
                        allowSave = True
                    ElseIf Not data.GetType.IsSerializable Then
                        cp.Site.ErrorReport("Exception in cache_saveRaw, data object must be serializable")
                    Else
                        allowSave = True
                    End If
                    If allowSave Then
                        saveRaw(encodeCacheKey(Key), data, invalidationDate)
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in performanceCache.saveCacheData")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a string to cache, for compatibility with existing site. (no objects, no invalidation, yet)
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="cacheValue"></param>
        ''' <remarks></remarks>
        Public Sub save(ByVal CP As CPBaseClass, cacheName As String, cacheValue As String)
            Try
                If allowCache Then
                    Dim invalidationDate As Date = rightNow.AddDays(invalidationDaysDefault + Rnd())
                    Dim cacheData As New cacheDataClass()
                    cacheData.data = cacheValue
                    cacheData.saveDate = Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = New List(Of String)
                    saveCacheData(cacheName, cacheData, invalidationDate)
                End If
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.write")
                Catch errObj As Exception
                End Try
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save an object to cache, with invalidation
        ''' 
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="cacheObject"></param>
        ''' <param name="invalidationDate"></param>
        ''' <param name="invalidationTagList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub save(ByVal CP As CPBaseClass, cacheName As String, cacheObject As Object, invalidationDate As Date, invalidationTagList As List(Of String))
            Try
                'If TypeOf cacheObject Is String Then
                '    appendCacheLog(vbTab & "save(" & CP.Doc.StartTime & "), cacheName(" & cacheName & "), length(" & DirectCast(cacheObject, String).Length() & ")")
                'Else
                '    appendCacheLog(vbTab & "save(" & CP.Doc.StartTime & "), cacheName(" & cacheName & "), length(not string)")
                'End If
                '
                If allowCache Then
                    Dim cacheData As New cacheDataClass()
                    cacheData.data = cacheObject
                    cacheData.saveDate = Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = invalidationTagList
                    saveCacheData(cacheName, cacheData, invalidationDate)
                End If
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.write2")
                Catch errObj As Exception
                End Try
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' save a cache value, compatible with legacy method signature.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <param name="cacheValue"></param>
        ''' <param name="invalidationTagCommaList">Comma delimited list of tags. Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        ''' <remarks></remarks>
        Public Sub save(ByVal CP As CPBaseClass, cacheName As String, cacheValue As String, invalidationTagCommaList As String)
            Try
                If allowCache Then
                    Dim invalidationList As New List(Of String)
                    Dim invalidationDate As Date = rightNow.AddDays(invalidationDaysDefault + Rnd())
                    Dim cacheData As New cacheDataClass()
                    invalidationList.AddRange(invalidationTagCommaList.Split(","c))
                    cacheData.data = cacheValue
                    cacheData.saveDate = Now()
                    cacheData.invalidationDate = invalidationDate
                    cacheData.tagList = invalidationList
                    saveCacheData(cacheName, cacheData, invalidationDate)
                End If
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.write2")
                Catch errObj As Exception
                End Try
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Read directly from ae.pcache. returns an object. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function readRaw(ByVal Key As String) As Object
            Dim returnValue As Object = Nothing
            Try
                If cacheIsLocal Then
                    returnValue = cp.Cache.Read(encodeCacheKey(Key))
                Else
                    returnValue = mc.Get(encodeCacheKey(Key))
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in performanceCache.readRaw")
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Read a cacheData object from ae.pcache. Private because newNew must have already been called.
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function readCacheData(ByVal Key As String) As cacheDataClass
            Dim returnCacheData As cacheDataClass = Nothing
            Dim dataRaw As Object
            Try
                Dim dataString As String
                If cacheIsLocal Then
                    dataString = DirectCast(readRaw(Key), String)
                    returnCacheData = json_serializer.Deserialize(Of cacheDataClass)(dataString)
                Else
                    dataRaw = readRaw(Key)
                    If Not (dataRaw Is Nothing) Then
                        Try
                            dataString = DirectCast(dataRaw, String)
                            'returnCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject(Of cacheDataClass)(dataString)
                            returnCacheData = json_serializer.Deserialize(Of cacheDataClass)(dataString)
                        Catch ex As Exception
                            cp.Site.ErrorReport(ex, "readCacheData error converting cache object to cacheDataClass object")
                        End Try
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in performanceCache.readCacheData")
            End Try
            Return returnCacheData
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' read a cache name and return it serialized as a string, for compatibility with existing site.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function read(ByVal CP As CPBaseClass, cacheName As String) As String
            Dim s As String = ""
            Dim logMsg As String = vbTab & "read(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")"
            Try
                Dim cacheData As cacheDataClass
                Dim tagInvalidated As Boolean
                If allowCache Then
                    cacheData = readCacheData(cacheName)
                    If (cacheData Is Nothing) Then
                        logMsg &= ", CACHE-MISS"
                    Else
                        If (globalInvalidationDate > cacheData.saveDate) Or (cacheData.invalidationDate < rightNow) Then
                            If (globalInvalidationDate > cacheData.saveDate) Then
                                logMsg &= ", CACHE-INVALIDATED by globalInvalidationDate"
                            Else
                                logMsg &= ", CACHE-INVALIDATED by data invalidationDate"
                            End If
                        Else
                            '
                            ' if this data is newer that the last glocal invalidation, continue
                            '
                            For Each tag As String In cacheData.tagList
                                If (getTagInvalidationDate(tag) > cacheData.saveDate) Then
                                    tagInvalidated = True
                                    logMsg &= ", CACHE-INVALIDATED by tag invalidationDate (" & tag & ")"
                                    Exit For
                                End If
                            Next
                            If Not tagInvalidated Then
                                '
                                ' no tags are invalidated, return the data
                                '
                                If TypeOf (cacheData.data) Is String Then
                                    s = DirectCast(cacheData.data, String)
                                Else
                                    s = json_serializer.Serialize(cacheData.data)
                                    's = Newtonsoft.Json.JsonConvert.SerializeObject(cacheData.data)
                                End If
                            End If
                        End If
                    End If
                End If
                '
                '
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.read")
                appendCacheLog(logMsg & ", length(" & s.Length & ")")
            End Try
            Return s
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' read a cache name and return the object.
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName">The cache entry name, a-z, 0-9, no spaces only</param>
        ''' <param name="return_cacheHit">Returns true if a valid cache entry found, false if there was no valid cache stored.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function read2(ByVal CP As CPBaseClass, cacheName As String, Optional ByRef return_cacheHit As Boolean = False) As Object
            Dim returnData As Object = Nothing
            Dim logMsg As String = vbTab & "read2(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")"
            Try
                'NewNew(CP)
                '
                Dim cacheData As cacheDataClass
                Dim tagInvalidated As Boolean
                return_cacheHit = False
                If allowCache Then
                    'addCacheNameToList(CP, cacheName)
                    cacheData = readCacheData(cacheName)
                    If (cacheData Is Nothing) Then
                        logMsg &= ", CACHE-MISS"
                    Else
                        If (globalInvalidationDate > cacheData.saveDate) Or (cacheData.invalidationDate < rightNow) Then
                            If (globalInvalidationDate > cacheData.saveDate) Then
                                logMsg &= ", CACHE-INVALIDATED by globalInvalidationDate"
                            Else
                                logMsg &= ", CACHE-INVALIDATED by data invalidationDate"
                            End If
                        Else
                            '
                            ' if this data is newer that the last glocal invalidation, continue
                            '
                            For Each tag As String In cacheData.tagList
                                If (getTagInvalidationDate(tag) > cacheData.saveDate) Then
                                    tagInvalidated = True
                                    logMsg &= ", CACHE-INVALIDATED by tag invalidationDate (" & tag & ")"
                                    Exit For
                                End If
                            Next
                            If Not tagInvalidated Then
                                '
                                ' no tags are invalidated, return the data
                                '
                                returnData = cacheData.data
                                return_cacheHit = True
                            End If
                        End If
                    End If
                End If
                ''NewNew(CP)
                ''
                'appendCacheLog(vbTab & "read2(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")")
                ''
                'Dim cacheData As cacheDataClass
                'Dim tagInvalidated As Boolean
                ''
                'If allowCache Then
                '    ' legacy
                '    addCacheNameToList(CP, cacheName)
                '    '
                '    cacheData = readCacheData(cacheName)
                '    If Not (cacheData Is Nothing) Then
                '        If (globalInvalidationDate < cacheData.saveDate) And (cacheData.invalidationDate > rightNow) Then
                '            '
                '            ' if this data is newer that the last glocal invalidation, continue
                '            '
                '            For Each tag As String In cacheData.tagList
                '                If (getTagInvalidationDate(tag) > cacheData.saveDate) Then
                '                    tagInvalidated = True
                '                    Exit For
                '                End If
                '            Next
                '            If Not tagInvalidated Then
                '                '
                '                ' no tags are invalidated, return the data
                '                '
                '                returnData = cacheData.data
                '            End If
                '        End If
                '    End If
                'End If
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.read2")
            End Try
            Return returnData
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' clear a cacheName
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="cacheName"></param>
        ''' <remarks></remarks>
        Public Sub clear(ByVal CP As CPBaseClass, cacheName As String)
            Try
                'NewNew(CP)
                '
                'appendCacheLog(vbTab & "clear(" & CP.Doc.StartTime & "), cacheName(" & cacheName & ")")
                '
                'Dim cachePath As String = cacheFolder & cacheName & ".cache"
                '
                '   built in cache clears on cDef - set to blank as a workaround
                '
                If cacheName <> "" Then
                    save(CP, cacheName, "")
                End If
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.performanceCloud.performanceCache.clear")
                Catch errObj As Exception
                End Try
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' clearUserCache - clears all the cache entries related to a userId
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="userID"></param>
        ''' <param name="clearQualifications"></param>
        ''' <param name="clearDegrees"></param>
        ''' <param name="clearCourses"></param>
        ''' <remarks></remarks>
        Public Sub clearObjectCache_Person(ByVal CP As CPBaseClass, userID  as Integer, clearQualifications As Boolean, clearDegrees As Boolean, clearCourses As Boolean)
            Try
                clear(CP, getObjectCacheName(CP, Models.PeopleModel.contentName, userID))
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' clear all cache entries with the globalInvalidationDate. Legacy code saved all cacheNames in a List() and iterated through the list, then did a contensive clearAll
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <remarks></remarks>
        Public Sub flushCache(ByVal CP As CPBaseClass)
            Try
                invalidateAll(CP)
                CP.Cache.ClearAll()
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' return the last dateTime this tag was modified
        ''' </summary>
        ''' <param name="tag">A tag that represents the source of the data in a cache entry. When that data source is modifed, the tag can be used to invalidate the cache entries based on it.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getTagInvalidationDate(ByVal tag As String) As Date
            Dim returnTagInvalidationDate As Date = New Date(1990, 8, 7)
            Try
                Dim cacheNameTagInvalidateDate As String = "tagInvalidationDate-" & tag
                Dim cacheObject As Object
                '
                If allowCache Then
                    If Not String.IsNullOrEmpty(tag) Then
                        '
                        ' get it from raw cache
                        '
                        cacheObject = readRaw(cacheNameTagInvalidateDate)
                        If TypeOf (cacheObject) Is String Then
                            returnTagInvalidationDate = cp.Utils.EncodeDate(cacheObject)
                        ElseIf TypeOf (cacheObject) Is Date Then
                            returnTagInvalidationDate = DirectCast(cacheObject, Date)
                        Else
                            returnTagInvalidationDate = New Date(1990, 8, 7)
                        End If
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in getTagInvalidationDate")
            End Try
            Return returnTagInvalidationDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates the entire cache (except those entires written with saveRaw)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub invalidateAll(cp As CPBaseClass)
            Try
                'NewNew(CP)
                '
                'appendCacheLog(vbTab & "invalidateAll(" & cp.Doc.StartTime & ")")
                '
                Call saveRaw("globalInvalidationDate", Now().ToString(), Now().AddYears(10))
                _globalInvalidationDateLoaded = False
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in invalidateAll")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a tag
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <remarks></remarks>
        Public Sub invalidateTag(cp As CPBaseClass, ByVal tag As String)
            Try
                Dim cacheName As String = "tagInvalidationDate-"
                '
                If allowCache Then
                    If tag <> "" Then
                        saveRaw(cacheName & tag, Now.ToString, Now.AddYears(10))
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in invalidateTag")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidates a list of tags 
        ''' </summary>
        ''' <param name="tagList"></param>
        ''' <remarks></remarks>
        Public Sub invalidateTagList(cp As CPBaseClass, ByVal tagList As List(Of String))
            Try
                If allowCache Then
                    For Each tag In tagList
                        Call invalidateTag(cp, tag)
                    Next
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Exception in invalidateTagList")
            End Try
        End Sub
        '
        '=======================================================================
        ''' <summary>
        ''' Encode a string to be memCacheD compatible, removing 0x00-0x20 and space
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function encodeCacheKey(key As String) As String
            Dim returnKey As String
            returnKey = Regex.Replace(key, "0x[a-fA-F\d]{2}", "_")
            returnKey = returnKey.Replace(" ", "_")
            Return returnKey
        End Function
        '
        '=======================================================================
        Private Sub appendCacheLog(line As String)
            Try
                cp.Utils.AppendLog(cacheLogFilename, "pid(" & Process.GetCurrentProcess().Id.ToString.PadLeft(4, "0"c) & "),thread(" & Threading.Thread.CurrentThread.ManagedThreadId.ToString.PadLeft(4, "0"c) & ")" & vbTab & line)
            Catch ex As Exception
                ' ignore logging errors
            End Try
        End Sub
        '
        Public Function getObjectCacheName(cp As CPBaseClass, contentname As String, recordId  as Integer, Optional suffixCacheType As String = "") As String
            Return (contentname & recordId.ToString & suffixCacheType).Replace(" ", "").ToLower()
        End Function
        '
        Public Sub clearObjectCache(cp As CPBaseClass, contentname As String, recordId  as Integer, Optional suffixCacheType As String = "")
            Try
                Dim cacheName As String = getObjectCacheName(cp, contentname, recordId, suffixCacheType)
                Call clear(cp, cacheName)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub
#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                '
                'appendCacheLog("disposing")
                '
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                    If Not (mc Is Nothing) Then
                        '
                        'appendCacheLog("dispose mc")
                        '
                        mc.Dispose()
                    Else
                        '
                        'appendCacheLog("dispose NO mc")
                        '
                    End If
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' finalize
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
    '
End Namespace