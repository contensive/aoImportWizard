
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports System.Reflection

Namespace Models
    Public MustInherit Class baseModel
        '
        '====================================================================================================
        '-- const must be set in derived clases
        '
        'Public Const contentName As String = "" '<------ set content name
        'Public Const contentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
        'Public Const contentDataSource As String = "" '<----- set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property id  as Integer
        Public Property name As String
        Public Property ccguid As String
        Public Property Active As Boolean
        Public Property ContentControlID  as Integer
        Public Property CreatedBy  as Integer
        Public Property CreateKey  as Integer
        Public Property DateAdded As Date
        Public Property ModifiedBy  as Integer
        Public Property ModifiedDate As Date
        Public Property SortOrder As String
        '
        '====================================================================================================
        Private Shared Function derivedContentName(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
        '
        '====================================================================================================
        Private Shared Function derivedContentTableName(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentTableName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentTableName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
        '
        '====================================================================================================
        Private Shared Function contentDataSource(derivedType As Type) As String
            Dim fieldInfo As FieldInfo = derivedType.GetField("contentTableName")
            If (fieldInfo Is Nothing) Then
                Throw New ApplicationException("Class [" & derivedType.Name & "] must declare constant [contentTableName].")
            Else
                Return fieldInfo.GetRawConstantValue().ToString()
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        ''' include callersCacheNameList to get a list of cacheNames used to assemble this response
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Protected Shared Function add(Of T As baseModel)(cp As CPBaseClass) As T
            Dim result As T = Nothing
            Try
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                result = create(Of T)(cp, cp.Content.AddRecord(contentName))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        Protected Shared Function create(Of T As baseModel)(cp As CPBaseClass, recordId  as Integer) As T
            Dim result As T = Nothing
            Try
                If recordId > 0 Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If cs.Open(contentName, "(id=" & recordId.ToString() & ")") Then
                        result = loadRecord(Of T)(cp, cs)
                    End If
                    cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Protected Shared Function create(Of T As baseModel)(cp As CPBaseClass, recordGuid As String) As T
            Dim result As T = Nothing
            Try
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                Dim cs As CPCSBaseClass = cp.CSNew()
                If cs.Open(contentName, "(ccGuid=" & cp.Db.EncodeSQLText(recordGuid) & ")") Then
                    result = loadRecord(Of T)(cp, cs)
                End If
                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordName"></param>
        Protected Shared Function createByName(Of T As baseModel)(cp As CPBaseClass, recordName As String) As T
            Dim result As T = Nothing
            Try
                If Not String.IsNullOrEmpty(recordName) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If cs.Open(contentName, "(name=" & cp.Db.EncodeSQLText(recordName) & ")", "id") Then
                        result = loadRecord(Of T)(cp, cs)
                    End If
                    cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="cs"></param>
        Private Shared Function loadRecord(Of T As baseModel)(cp As CPBaseClass, cs As CPCSBaseClass, Optional listOfLowerCaseFields As List(Of String) = Nothing) As T
            Dim modelInstance As T = Nothing
            Try
                If cs.OK() Then
                    Dim instanceType As Type = GetType(T)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    modelInstance = DirectCast(Activator.CreateInstance(instanceType), T)
                    For Each modelProperty As PropertyInfo In modelInstance.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)

                        Dim includeField As Boolean = True
                        If listOfLowerCaseFields IsNot Nothing Then
                            includeField = listOfLowerCaseFields.Contains(modelProperty.Name.ToLower())
                        End If
                        If includeField Then
                            Select Case modelProperty.Name.ToLower()
                                Case "specialcasefield"
                                Case "sortorder"
                                    '
                                    ' -- customization for pc, could have been in default property, db default, etc.
                                    Dim sortOrder As String = cs.GetText(modelProperty.Name)
                                    If (String.IsNullOrEmpty(sortOrder)) Then
                                        sortOrder = "9999"
                                    End If
                                    modelProperty.SetValue(modelInstance, sortOrder, Nothing)
                                Case Else
                                    '
                                    ' -- get the underlying type if this is nullable
                                    Dim targetNullable As Boolean = IsNullable(modelProperty.PropertyType)
                                    Dim prpertyValueText As String = cs.GetText(modelProperty.Name)
                                    If (targetNullable And (String.IsNullOrEmpty(prpertyValueText))) Then
                                        '
                                        ' -- load a blank value to a nullable property as null
                                        modelProperty.SetValue(modelInstance, Nothing, Nothing)
                                    Else
                                        '
                                        ' -- not nullable or value is not null
                                        Dim targetType As Type = If(targetNullable, Nullable.GetUnderlyingType(modelProperty.PropertyType), modelProperty.PropertyType)
                                        Select Case targetType.Name
                                            Case "Int32"
                                                modelProperty.SetValue(modelInstance, cs.GetInteger(modelProperty.Name), Nothing)
                                            Case "Boolean"
                                                modelProperty.SetValue(modelInstance, cs.GetBoolean(modelProperty.Name), Nothing)
                                            Case "DateTime"
                                                modelProperty.SetValue(modelInstance, cs.GetDate(modelProperty.Name), Nothing)
                                            Case "Double"
                                                modelProperty.SetValue(modelInstance, cs.GetNumber(modelProperty.Name), Nothing)
                                            Case "String"
                                                modelProperty.SetValue(modelInstance, cs.GetText(modelProperty.Name), Nothing)
                                            Case "fieldTypeTextFile"
                                                '
                                                ' -- cdn files
                                                Dim instanceFileType As New fieldTypeTextFile
                                                instanceFileType.filename = cs.GetFilename(modelProperty.Name)
                                                modelProperty.SetValue(modelInstance, instanceFileType, Nothing)
                                            Case "fieldTypeJavascriptFile"
                                                '
                                                ' -- cdn files
                                                Dim instanceFileType As New fieldTypeJavascriptFile
                                                instanceFileType.filename = cs.GetFilename(modelProperty.Name)
                                                modelProperty.SetValue(modelInstance, instanceFileType, Nothing)
                                            Case "fieldTypeCSSFile"
                                                '
                                                ' -- cdn files
                                                Dim instanceFileType As New fieldTypeCSSFile
                                                instanceFileType.filename = cs.GetFilename(modelProperty.Name)
                                                modelProperty.SetValue(modelInstance, instanceFileType, Nothing)
                                            Case "fieldTypeHTMLFile"
                                                '
                                                ' -- private files
                                                Dim instanceFileType As New fieldTypeHTMLFile
                                                instanceFileType.filename = cs.GetFilename(modelProperty.Name)
                                                modelProperty.SetValue(modelInstance, instanceFileType, Nothing)
                                            Case Else
                                                modelProperty.SetValue(modelInstance, cs.GetText(modelProperty.Name), Nothing)
                                        End Select
                                    End If
                            End Select
                        End If
                    Next
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return modelInstance
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Protected Function save(Of T As baseModel)(cp As CPBaseClass)  as Integer
            Dim cs As CPCSBaseClass = cp.CSNew()
            Try
                Dim instanceType As Type = Me.GetType()
                Dim contentName As String = derivedContentName(instanceType)
                Dim tableName As String = derivedContentTableName(instanceType)
                If (id > 0) Then
                    If Not cs.Open(contentName, "id=" & id) Then
                        Dim message As String = "Unable to open record in content [" & contentName & "], with id [" & id & "]"
                        cs.Close()
                        id = 0
                        Throw New ApplicationException(message)
                    End If
                Else
                    If Not cs.Insert(contentName) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & contentName & "]")
                    End If
                End If
                'Dim instanceType As Type = Me.GetType()
                For Each instanceProperty As PropertyInfo In Me.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                    Select Case instanceProperty.Name.ToLower()
                        Case "id"
                            id = cs.GetInteger("id")
                        Case "ccguid"
                            If (String.IsNullOrEmpty(ccguid)) Then
                                ccguid = "{" & Guid.NewGuid().ToString() & "}"
                            End If
                            Dim value As String
                            value = instanceProperty.GetValue(Me, Nothing).ToString()
                            cs.SetField(instanceProperty.Name, value)
                        Case Else

                            '
                            ' -- get the underlying type if this is nullable
                            Dim targetNullable As Boolean = IsNullable(instanceProperty.PropertyType)
                            Dim propertyValueText As String = cs.GetText(instanceProperty.Name)
                            If (targetNullable And (String.IsNullOrEmpty(propertyValueText))) Then
                                '
                                ' -- null value in a nullable property - save a blank value to a Db field
                                cs.SetField(instanceProperty.Name, Nothing)
                            Else
                                '
                                ' -- not nullable or value is not null
                                Dim targetType As Type = If(targetNullable, Nullable.GetUnderlyingType(instanceProperty.PropertyType), instanceProperty.PropertyType)
                                Select Case targetType.Name
                                    Case "Int32"
                                        Dim value  as Integer
                                        Integer.TryParse(instanceProperty.GetValue(Me, Nothing).ToString(), value)
                                        cs.SetField(instanceProperty.Name, value.ToString())
                                    Case "Boolean"
                                        Dim value As Boolean
                                        Boolean.TryParse(instanceProperty.GetValue(Me, Nothing).ToString(), value)
                                        cs.SetField(instanceProperty.Name, value.ToString())
                                    Case "DateTime"
                                        Dim value As Date
                                        Date.TryParse(instanceProperty.GetValue(Me, Nothing).ToString(), value)
                                        cs.SetField(instanceProperty.Name, value.ToString())
                                    Case "Double"
                                        Dim value As Double
                                        Double.TryParse(instanceProperty.GetValue(Me, Nothing).ToString(), value)
                                        cs.SetField(instanceProperty.Name, value.ToString())
                                    Case "fieldTypeTextFile", "fieldTypeJavascriptFile", "fieldTypeCSSFile", "fieldTypeHTMLFile"
                                        Dim fieldTypeId  as Integer = 0
                                        Dim contentProperty As PropertyInfo = Nothing
                                        Dim contentUpdatedProperty As PropertyInfo
                                        Dim contentUpdated As Boolean
                                        Dim content As String = ""
                                        Select Case instanceProperty.PropertyType.Name
                                            Case "fieldTypeJavascriptFile"
                                                fieldTypeId = FieldTypeIdFileJavascript
                                                Dim fileProperty As fieldTypeJavascriptFile = DirectCast(instanceProperty.GetValue(Me, Nothing), fieldTypeJavascriptFile)
                                                fileProperty.internalCp = cp
                                                contentProperty = instanceProperty.PropertyType.GetProperty("content")
                                                contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated")
                                                contentUpdated = DirectCast(contentUpdatedProperty.GetValue(fileProperty, Nothing), Boolean)
                                                content = DirectCast(contentProperty.GetValue(fileProperty, Nothing), String)
                                            Case "fieldTypeCSSFile"
                                                fieldTypeId = FieldTypeIdFileCSS
                                                Dim fileProperty As fieldTypeCSSFile = DirectCast(instanceProperty.GetValue(Me, Nothing), fieldTypeCSSFile)
                                                fileProperty.internalCp = cp
                                                contentProperty = instanceProperty.PropertyType.GetProperty("content")
                                                contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated")
                                                contentUpdated = DirectCast(contentUpdatedProperty.GetValue(fileProperty, Nothing), Boolean)
                                                content = DirectCast(contentProperty.GetValue(fileProperty, Nothing), String)
                                            Case "fieldTypeHTMLFile"
                                                fieldTypeId = FieldTypeIdFileHTML
                                                Dim fileProperty As fieldTypeHTMLFile = DirectCast(instanceProperty.GetValue(Me, Nothing), fieldTypeHTMLFile)
                                                fileProperty.internalCp = cp
                                                contentProperty = instanceProperty.PropertyType.GetProperty("content")
                                                contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated")
                                                contentUpdated = DirectCast(contentUpdatedProperty.GetValue(fileProperty, Nothing), Boolean)
                                                content = DirectCast(contentProperty.GetValue(fileProperty, Nothing), String)
                                            Case Else
                                                fieldTypeId = FieldTypeIdFileText
                                                Dim fileProperty As fieldTypeTextFile = DirectCast(instanceProperty.GetValue(Me, Nothing), fieldTypeTextFile)
                                                fileProperty.internalCp = cp
                                                contentProperty = instanceProperty.PropertyType.GetProperty("content")
                                                contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated")
                                                contentUpdated = DirectCast(contentUpdatedProperty.GetValue(fileProperty, Nothing), Boolean)
                                                content = DirectCast(contentProperty.GetValue(fileProperty, Nothing), String)
                                        End Select
                                        If (contentUpdated) Then
                                            Dim filename As String = cs.GetFilename(instanceProperty.Name)
                                            If (String.IsNullOrEmpty(content)) Then
                                                '
                                                ' -- empty content
                                                If (Not String.IsNullOrEmpty(filename)) Then
                                                    cs.SetField(instanceProperty.Name, "")
                                                    cp.File.Delete(filename)
                                                End If
                                            Else
                                                '
                                                ' -- save content
                                                If (String.IsNullOrEmpty(filename)) Then
                                                    filename = getUploadPath(Of T)(instanceProperty.Name.ToLower())
                                                End If
                                                cs.SetFile(instanceProperty.Name, content, contentName)
                                            End If
                                        End If

                                    Case Else
                                        Dim value As String
                                        value = instanceProperty.GetValue(Me, Nothing).ToString()
                                        cs.SetField(instanceProperty.Name, value)
                                End Select


                            End If
                    End Select
                Next
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            Finally
                cs.Close()
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Protected Shared Sub delete(Of T As baseModel)(cp As CPBaseClass, recordId  as Integer)
            Try
                If (recordId > 0) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    cp.Content.Delete(contentName, "id=" & recordId.ToString)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by guid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccguid"></param>
        Protected Shared Sub delete(Of T As baseModel)(cp As CPBaseClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    Dim instanceType As Type = GetType(T)
                    Dim contentName As String = derivedContentName(instanceType)
                    Dim instance As baseModel = create(Of baseModel)(cp, ccguid)
                    If (instance IsNot Nothing) Then
                        cp.Content.Delete(contentName, "(ccguid=" & cp.Db.EncodeSQLText(ccguid) & ")")
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <returns></returns>
        Protected Shared Function createList(Of T As baseModel)(cp As CPBaseClass, sqlCriteria As String, sqlOrderBy As String) As List(Of T)
            Dim result As New List(Of T)
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim ignoreCacheNames As New List(Of String)
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                If (cs.Open(contentName, sqlCriteria, sqlOrderBy)) Then
                    Dim instance As T
                    Do
                        instance = loadRecord(Of T)(cp, cs)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.GoNext()
                    Loop While cs.OK()
                End If
                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <returns></returns>
        Protected Shared Function createList(Of T As baseModel)(cp As CPBaseClass, sqlCriteria As String, sqlOrderBy As String, pageSize  as Integer, pageNumber  as Integer, Optional listOfLowerCaseFields As List(Of String) = Nothing) As List(Of T)
            Dim result As New List(Of T)
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim ignoreCacheNames As New List(Of String)
                Dim instanceType As Type = GetType(T)
                Dim contentName As String = derivedContentName(instanceType)
                If (cs.Open(contentName, sqlCriteria, sqlOrderBy,,, pageSize, pageNumber)) Then
                    Dim instance As T
                    Do
                        instance = loadRecord(Of T)(cp, cs, listOfLowerCaseFields)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.GoNext()
                    Loop While cs.OK()
                End If
                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Protected Shared Function getRecordName(Of T As baseModel)(cp As CPBaseClass, recordId  as Integer) As String
            Try
                If (recordId > 0) Then
                    Dim instanceType As Type = GetType(T)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If (cs.OpenSQL("select name from " & tableName & " where id=" & recordId.ToString())) Then
                        Return cs.GetText("name")
                    End If
                    cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return ""
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Protected Shared Function getRecordName(Of T As baseModel)(cp As CPBaseClass, ccGuid As String) As String
            Try
                If (Not String.IsNullOrEmpty(ccGuid)) Then
                    Dim instanceType As Type = GetType(T)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If (cs.OpenSQL("select name from " & tableName & " where ccguid=" & cp.Db.EncodeSQLText(ccGuid))) Then
                        Return cs.GetText("name")
                    End If
                    cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return ""
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the id of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Protected Shared Function getRecordId(Of T As baseModel)(cp As CPBaseClass, ccGuid As String)  as Integer
            Try
                If (Not String.IsNullOrEmpty(ccGuid)) Then
                    Dim instanceType As Type = GetType(T)
                    Dim tableName As String = derivedContentTableName(instanceType)
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If (cs.OpenSQL("select id from " & tableName & " where ccguid=" & cp.Db.EncodeSQLText(ccGuid))) Then
                        Return cs.GetInteger("id")
                    End If
                    cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return 0
        End Function
        '
        '====================================================================================================
        Protected Shared Function getCount(Of T As baseModel)(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Dim result  as Integer = 0
            Try
                Dim instanceType As Type = GetType(T)
                Dim tableName As String = derivedContentTableName(instanceType)
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim sql As String = "select count(id) as cnt from " & tableName
                If (Not String.IsNullOrEmpty(sqlCriteria)) Then
                    sql &= " where " & sqlCriteria
                End If
                If (cs.OpenSQL(sql)) Then
                    result = cs.GetInteger("cnt")
                End If
                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Temporary method to create a path for an uploaded. First, try the texrt value in the field. If it is empty, use this method to create the path,
        ''' append the filename to the end and save it to the field, and save the file there. This path starts with the tablename and ends with a slash.
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <returns></returns>
        Protected Function getUploadPath(Of T As baseModel)(fieldName As String) As String
            Dim instanceType As Type = GetType(T)
            Dim tableName As String = derivedContentTableName(instanceType)
            Return tableName.ToLower() & "/" & fieldName.ToLower() & "/" & id.ToString().PadLeft(12, CChar("0")) & "/"
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return true if the type is nullable
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function IsNullable(ByVal type As Type) As Boolean
            Return Nullable.GetUnderlyingType(type) IsNot Nothing
        End Function

        '
        '====================================================================================================
        ''' <summary>
        ''' field type to store file related fields. Copied from cpCore
        ''' </summary>
        Public MustInherit Class fieldCdnFile
            '
            ' -- 
            ' during load
            '   -- The filename is loaded into the model (blank or not). No content Is read from the file during load.
            '   -- the internalCpCore must be set
            '
            ' during a cache load, the internalCpCore must be set
            '
            ' content property read:
            '   -- If the filename Is blank, a blank Is returned
            '   -- if the filename exists, the content is read into the model and returned to the consumer
            '
            ' content property written:
            '   -- content is stored in the model until save(). contentUpdated is set.
            '
            ' filename property read: nothing special
            '
            ' filename property written:
            '   -- contentUpdated set true if it was previously set (content was written), or if the content is not empty
            '
            ' contentLoaded property means the content in the model is valid
            ' contentUpdated property means the content needs to be saved on the next save
            '
            Public Property filename As String
                Set(value As String)
                    _filename = value
                    '
                    ' -- mark content updated if the content was updated, or if the content is not blank (so old content is written to the new updated filename)
                    contentUpdated = contentUpdated Or (Not String.IsNullOrEmpty(_content))
                End Set
                Get
                    Return _filename
                End Get
            End Property
            Private _filename As String = ""
            '
            ' -- content in the file. loaded as needed, not during model create. 
            Public Property content As String
                Set(value As String)
                    _content = value
                    contentUpdated = True
                End Set
                Get
                    If (Not contentLoaded) Then
                        If (Not String.IsNullOrEmpty(filename)) And (internalCp IsNot Nothing) Then
                            contentLoaded = True
                            _content = internalCp.File.Read(filename)
                        End If
                    End If
                    Return _content
                End Get
            End Property
            '
            ' -- internal storage for content
            Private Property _content As String = ""
            '
            ' -- When field is deserialized from cache, contentLoaded flag is used to deferentiate between unloaded content and blank conent.
            Public Property contentLoaded As Boolean = False
            '
            ' -- When content is updated, the model.save() writes the file
            Public Property contentUpdated As Boolean = False
            '
            ' -- set by load(). Used by field to read content from filename when needed
            Public Property internalCp As CPBaseClass = Nothing
        End Class

        '
        Public Class fieldTypeTextFile
            Inherits fieldCdnFile
        End Class
        Public Class fieldTypeJavascriptFile
            Inherits fieldCdnFile
        End Class
        Public Class fieldTypeCSSFile
            Inherits fieldCdnFile
        End Class
        Public Class fieldTypeHTMLFile
            Inherits fieldCdnFile
        End Class
        '
        '-----------------------------------------------------------------------
        ' ----- Field type Definitions
        '       Field Types are numeric values that describe how to treat values
        '       stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
        '-----------------------------------------------------------------------
        '
        Public Const FieldTypeIdInteger = 1       ' An long number
        Public Const FieldTypeIdText = 2          ' A text field (up to 255 characters)
        Public Const FieldTypeIdLongText = 3      ' A memo field (up to 8000 characters)
        Public Const FieldTypeIdBoolean = 4       ' A yes/no field
        Public Const FieldTypeIdDate = 5          ' A date field
        Public Const FieldTypeIdFile = 6          ' A filename of a file in the files directory.
        Public Const FieldTypeIdLookup = 7        ' A lookup is a FieldTypeInteger that indexes into another table
        Public Const FieldTypeIdRedirect = 8      ' creates a link to another section
        Public Const FieldTypeIdCurrency = 9      ' A Float that prints in dollars
        Public Const FieldTypeIdFileText = 10     ' Text saved in a file in the files area.
        Public Const FieldTypeIdFileImage = 11        ' A filename of a file in the files directory.
        Public Const FieldTypeIdFloat = 12        ' A float number
        Public Const FieldTypeIdAutoIdIncrement = 13 'long that automatically increments with the new record
        Public Const FieldTypeIdManyToMany = 14    ' no database field - sets up a relationship through a Rule table to another table
        Public Const FieldTypeIdMemberSelect = 15 ' This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        Public Const FieldTypeIdFileCSS = 16      ' A filename of a CSS compatible file
        Public Const FieldTypeIdFileXML = 17      ' the filename of an XML compatible file
        Public Const FieldTypeIdFileJavascript = 18 ' the filename of a javascript compatible file
        Public Const FieldTypeIdLink = 19           ' Links used in href tags -- can go to pages or resources
        Public Const FieldTypeIdResourceLink = 20   ' Links used in resources, link <img or <object. Should not be pages
        Public Const FieldTypeIdHTML = 21           ' LongText field that expects HTML content
        Public Const FieldTypeIdFileHTML = 22       ' TextFile field that expects HTML content
        Public Const FieldTypeIdMax = 22
        '
        ' ----- Field Descriptors for these type
        '       These are what are publicly displayed for each type
        '       See GetFieldTypeNameByType and vise-versa to translater
        '
        Public Const FieldTypeNameInteger As String = "Integer"
        Public Const FieldTypeNameText As String = "Text"
        Public Const FieldTypeNameLongText As String = "LongText"
        Public Const FieldTypeNameBoolean As String = "Boolean"
        Public Const FieldTypeNameDate As String = "Date"
        Public Const FieldTypeNameFile As String = "File"
        Public Const FieldTypeNameLookup As String = "Lookup"
        Public Const FieldTypeNameRedirect As String = "Redirect"
        Public Const FieldTypeNameCurrency As String = "Currency"
        Public Const FieldTypeNameImage As String = "Image"
        Public Const FieldTypeNameFloat As String = "Float"
        Public Const FieldTypeNameManyToMany As String = "ManyToMany"
        Public Const FieldTypeNameTextFile As String = "TextFile"
        Public Const FieldTypeNameCSSFile As String = "CSSFile"
        Public Const FieldTypeNameXMLFile As String = "XMLFile"
        Public Const FieldTypeNameJavascriptFile As String = "JavascriptFile"
        Public Const FieldTypeNameLink As String = "Link"
        Public Const FieldTypeNameResourceLink As String = "ResourceLink"
        Public Const FieldTypeNameMemberSelect As String = "MemberSelect"
        Public Const FieldTypeNameHTML As String = "HTML"
        Public Const FieldTypeNameHTMLFile As String = "HTMLFile"
        '
        Public Const FieldTypeNameLcaseInteger As String = "integer"
        Public Const FieldTypeNameLcaseText As String = "text"
        Public Const FieldTypeNameLcaseLongText As String = "longtext"
        Public Const FieldTypeNameLcaseBoolean As String = "boolean"
        Public Const FieldTypeNameLcaseDate As String = "date"
        Public Const FieldTypeNameLcaseFile As String = "file"
        Public Const FieldTypeNameLcaseLookup As String = "lookup"
        Public Const FieldTypeNameLcaseRedirect As String = "redirect"
        Public Const FieldTypeNameLcaseCurrency As String = "currency"
        Public Const FieldTypeNameLcaseImage As String = "image"
        Public Const FieldTypeNameLcaseFloat As String = "float"
        Public Const FieldTypeNameLcaseManyToMany As String = "manytomany"
        Public Const FieldTypeNameLcaseTextFile As String = "textfile"
        Public Const FieldTypeNameLcaseCSSFile As String = "cssfile"
        Public Const FieldTypeNameLcaseXMLFile As String = "xmlfile"
        Public Const FieldTypeNameLcaseJavascriptFile As String = "javascriptfile"
        Public Const FieldTypeNameLcaseLink As String = "link"
        Public Const FieldTypeNameLcaseResourceLink As String = "resourcelink"
        Public Const FieldTypeNameLcaseMemberSelect As String = "memberselect"
        Public Const FieldTypeNameLcaseHTML As String = "html"
        Public Const FieldTypeNameLcaseHTMLFile As String = "htmlfile"
    End Class
End Namespace
