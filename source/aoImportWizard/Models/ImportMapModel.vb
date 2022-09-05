
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Controllers
Imports C5BaseModel = Contensive.Models.Db.DbBaseModel

Namespace Contensive.ImportWizard.Models
    Public Class ImportMapModel

        'Public Shared Widening Operator CType(v As String) As ImportMapType
        '    Throw New NotImplementedException()
        'End Operator
        '
        ' Import Map file layout
        '
        ' row 0 - KeyMethodID
        ' row 1 - SourceKey Field
        ' row 2 - DbKey Field
        ' row 3 - blank
        ' row4+ SourceField,DbField mapping pairs
        '
        ' - if nuget references are not there, open nuget command line and click the 'reload' message at the top, or run "Update-Package -reinstall" - close/open
        ' - Verify project root name space is empty
        ' - Change the namespace (AddonCollectionVb) to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        Public Property importToNewContent As Boolean
        Public Property contentName As String
        ''' <summary>
        ''' see MapKeyEnum, 1=insert all, 2=update if match, 3=update if match, else insert
        ''' </summary>
        ''' <returns></returns>
        Public Property keyMethodID As Integer
        Public Property sourceKeyField As String
        Public Property dbKeyField As String
        Public Property dbKeyFieldType As Integer
        Public Property groupOptionID As Integer
        Public Property groupID As Integer
        Public Property skipRowCnt As Integer
        Public Property mapPairCnt As Integer
        Public Property mapPairs As ImportMapModel_MapPair()
        '
        Public Shared Function getMapPath(app As ApplicationModel, contentName As String) As String
            Return privateFilesMapFolder & "user" & app.cp.User.Id & "\" & contentName.Replace(" ", "-") & "\"
        End Function
        '
        ''' <summary>
        ''' create filename
        ''' YYYY-MM-DD-HH-MM-SS-normalizedFilename
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="contentName"></param>
        ''' <param name="mapName"></param>
        ''' <returns></returns>
        Public Shared Function createMapPathFilename(app As ApplicationModel, contentName As String, mapName As String) As String
            Dim dateCreated As Date = Now
            Return getMapPath(app, contentName) & dateCreated.Year & "-" & dateCreated.Month.ToString().PadLeft(2, "0"c) & "-" & dateCreated.Day.ToString().PadLeft(2, "0"c) & "_" & dateCreated.Hour.ToString().PadLeft(2, "0"c) & "-" & dateCreated.Minute.ToString().PadLeft(2, "0"c) & "-" & dateCreated.Second.ToString().PadLeft(2, "0"c) & "_" & GenericController.normalizeFilename(mapName) & ".txt"
        End Function
        '
        Public Shared Function decodeMapFileName(cp As CPBaseClass, mapFilename As String) As MapFilenameDataModel
            '
            If mapFilename.Length < 23 Then Return Nothing
            Dim yearPart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(0, 4))
            Dim monthPart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(5, 2))
            Dim dayPart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(8, 2))
            Dim hourPart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(11, 2))
            Dim minutePart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(14, 2))
            Dim secondPart As Integer = cp.Utils.EncodeInteger(mapFilename.Substring(17, 2))
            If yearPart = 0 Or monthPart = 0 Or dayPart = 0 Then Return Nothing
            '
            Dim filenamePart As String = mapFilename.Substring(20)
            If String.IsNullOrEmpty(filenamePart) Then Return Nothing
            '
            Return New MapFilenameDataModel With {
                .filename = mapFilename,
                .mapName = IO.Path.GetFileNameWithoutExtension(filenamePart),
                .dateCreated = New Date(yearPart, monthPart, dayPart, hourPart, minutePart, secondPart)
            }
        End Function

        '
        ''' <summary>
        ''' Return a list of filename plus dateadded
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="contentName"></param>
        ''' <returns></returns>
        Public Shared Function getMapFileList(app As ApplicationModel, contentName As String) As List(Of MapFilenameDataModel)
            Try
                Dim result As New List(Of MapFilenameDataModel)
                For Each fileDetail In app.cp.PrivateFiles.FileList(getMapPath(app, contentName))
                    Dim mapField As MapFilenameDataModel = decodeMapFileName(app.cp, fileDetail.Name)
                    If mapField Is Nothing Then Continue For
                    result.Add(mapField)
                    'Dim mapFilename As String = fileDetail.Name
                    ''
                    'If mapFilename.Length < 23 Then Continue For
                    'Dim yearPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(0, 4))
                    'Dim monthPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(5, 2))
                    'Dim dayPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(8, 2))
                    'Dim hourPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(11, 2))
                    'Dim minutePart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(14, 2))
                    'Dim secondPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(17, 2))
                    'If yearPart = 0 Or monthPart = 0 Or dayPart = 0 Then Continue For
                    ''
                    'Dim filenamePart As String = mapFilename.Substring(20)
                    'If String.IsNullOrEmpty(filenamePart) Then Continue For
                    ''
                    'result.Add(New MapFilenameDataModel With {
                    '    .filename = mapFilename,
                    '    .mapName = filenamePart,
                    '    .dateCreated = New Date(yearPart, monthPart, dayPart, hourPart, minutePart, secondPart)
                    '})
                Next
                Return result
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        ''' <summary>
        ''' Load Import Map
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="importMapPathFilename"></param>
        ''' <returns></returns>
        Public Shared Function create(cp As CPBaseClass, importMapPathFilename As String) As ImportMapModel
            Try
                Dim result As ImportMapModel = cp.JSON.Deserialize(Of ImportMapModel)(cp.PrivateFiles.Read(importMapPathFilename))
                If result IsNot Nothing Then Return result
                '
                result = New ImportMapModel() With {
                    .contentName = "People",
                    .groupID = 0,
                    .mapPairCnt = 0,
                    .mapPairs = {},
                    .skipRowCnt = 1,
                    .keyMethodID = 1
                }
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '

        '
        '====================================================================================================
        ''' <summary>
        ''' Save the import map data
        ''' </summary>
        ''' <param name="app"></param>
        Public Sub save(app As ApplicationModel, importConfig As ImportConfigModel)
            Try
                Call app.cp.PrivateFiles.Save(importConfig.importMapPathFilename, app.cp.JSON.Serialize(Me))
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
        '
        ''' <summary>
        ''' Create new import map based on contentName. contentName is explicitly requested instead of using importconfig.contentid so this
        ''' call must explicitly note the content is valid
        ''' 
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="importConfig"></param>
        ''' <param name="contentName"></param>
        Public Shared Sub buildNewImportMapForContent(app As ApplicationModel, importConfig As ImportConfigModel, contentName As String)
            Dim cp As CPBaseClass = app.cp
            '
            ' -- build a new map
            Dim mapName As String = "Import " & contentName
            importConfig.importMapPathFilename = ImportMapModel.createMapPathFilename(app, contentName, mapName)
            importConfig.dstContentId = cp.Content.GetID(contentName)
            importConfig.save(app)
            '
            Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
            ImportMap.contentName = contentName

            Dim fieldList As List(Of ContentFieldModel) = C5BaseModel.createList(Of ContentFieldModel)(cp, "(contentId=" & importConfig.dstContentId & ")and(active>0)", "caption")

            Dim dbFieldList As List(Of ContentFieldData) = ContentFieldModel.getDbFieldList(cp, ImportMap.contentName, False, False)
            'Dim dbFieldNames() As String = Split(ContentFieldModel.getDbFieldList(cp, ImportMap.contentName, False, False), ",")
            'Dim dbFieldNameCnt As Integer = UBound(dbFieldNames) + 1
            ' todo di-hack
            app.loadUploadFields(importConfig.privateUploadPathFilename)
            Dim uploadFields() As String = app.uploadFields
            ImportMap.mapPairCnt = dbFieldList.Count
            ReDim ImportMap.mapPairs(dbFieldList.Count - 1)
            Dim rowPtr As Integer = 0

            For Each dbField In dbFieldList
                Dim dbFieldName As String = dbField.name
                '
                ' -- setup mapPair
                Dim mapRow = New ImportMapModel_MapPair()
                ImportMap.mapPairs(rowPtr) = mapRow
                mapRow.dbFieldName = dbFieldName
                mapRow.setValue = Nothing
                mapRow.dbFieldType = ContentFieldModel.getFieldType(cp, dbFieldName, importConfig.dstContentId)
                mapRow.uploadFieldPtr = -1
                mapRow.uploadFieldName = ""
                '
                ' -- search uploadFields for matches to dbFields
                Dim dBFieldName_lower As String = LCase(dbFieldName)
                Dim uploadFieldPtr As Integer = 0
                For Each uploadField As String In uploadFields
                    Dim uploadField_lower As String = uploadField.ToLowerInvariant()
                    If uploadField_lower = dBFieldName_lower Then
                        mapRow.uploadFieldPtr = uploadFieldPtr
                        mapRow.uploadFieldName = uploadField
                        Exit For
                    End If
                    Select Case dBFieldName_lower
                        Case "company"
                            If (uploadField_lower = "companyname") OrElse (uploadField_lower = "company name") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "zip"
                            If (uploadField_lower = "zip code") OrElse (uploadField_lower = "zipcode") OrElse (uploadField_lower = "postal code") OrElse (uploadField_lower = "postalcode") OrElse (uploadField_lower = "zip/postalcode") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "firstname"
                            If (uploadField_lower = "first") OrElse (uploadField_lower = "first name") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "lastname"
                            If (uploadField_lower = "last") OrElse (uploadField_lower = "last name") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "email"
                            If (uploadField_lower = "e-mail") OrElse (uploadField_lower = "emailaddress") OrElse (uploadField_lower = "e-mailaddress") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "address"
                            If (uploadField_lower = "address1") OrElse (uploadField_lower = "addressline1") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "address2"
                            If (uploadField_lower = "addressline2") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                        Case "phone", "cellphone"
                            If (uploadField_lower = "phone number") OrElse (uploadField_lower = "phonenumber") Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                    End Select
                    uploadFieldPtr += 1
                Next
                rowPtr += 1
            Next
            ImportMap.save(app, importConfig)

        End Sub
    End Class
    '
    Public Class ImportMapModel_MapPair
        ''' <summary>
        ''' 0-based index to the column of the uploaded data file
        ''' -1 = ignore this fields
        ''' -2 = set the value from the setValue field
        ''' -3 = combine 'firstname lastname' (valid for people content only. All other tables will ignore this)
        ''' -4 = firstname from name field (valid for people content only. All other tables will ignore this)
        ''' -5 = lastname from name field (valid for people content only. All other tables will ignore this)
        ''' </summary>
        ''' <returns></returns>
        Public Property uploadFieldPtr As Integer
        ''' <summary>
        ''' the name of the column from the uploaded data file
        ''' </summary>
        ''' <returns></returns>
        Public Property uploadFieldName As String
        Public Property dbFieldName As String
        Public Property dbFieldType As Integer
        ''' <summary>
        ''' if not null, ignore dbfield and set the 
        ''' </summary>
        ''' <returns></returns>
        Public Property setValue As String
    End Class
    '
    Public Class MapFilenameDataModel
        Public Property filename As String
        Public Property mapName As String
        Public Property dateCreated As Date
    End Class
    '
    Public Enum MapKeyEnum
        KeyMethodInsertAll = 1
        KeyMethodUpdateOnMatch = 2
        KeyMethodUpdateOnMatchInsertOthers = 3
    End Enum

End Namespace