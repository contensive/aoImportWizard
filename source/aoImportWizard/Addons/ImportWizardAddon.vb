
Imports System.Linq
Imports System.Text
Imports Contensive.ImportWizard.Controllers
Imports Contensive.ImportWizard.Controllers.GenericController
Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Contensive.ImportWizard.Addons
    ''' <summary>
    ''' The addon that runs on the page -- setup the import files
    ''' </summary>
    Public Class ImportWizardAddon
        Inherits AddonBaseClass
        Private Property sourceFieldCnt As Integer
        Private Property sourceFields As String()
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
        '
        '=====================================================================================
        ''' <summary>
        ''' The addon that runs on the page -- setup the import files
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Dim result As String = ""
                Dim GetForm As String = ""
                Using app As New ApplicationController(CP, False)
                    Dim PeopleContentID As Integer = CP.Content.GetRecordID("content", "people")
                    Dim ProcessError As Boolean = False
                    Dim Content As String = ""
                    Dim Button As String = CP.Doc.GetText(RequestNameButton)
                    If Button = ButtonCancel Then
                        '
                        ' Cancel
                        '
                        Call WizardController.clearWizardValues(CP)
                        Call CP.Response.Redirect("\")
                    End If
                    If True Then
                        '

                        Dim ImportMap As New ImportMapType
                        Dim Ptr As Integer
                        Dim ImportContentID As Integer
                        Dim Filename As String
                        Dim SourceFieldPtr As Integer
                        Dim ImportMapFile As String
                        Dim ImportMapData As String
                        Dim useNewContentName As Boolean
                        Dim newContentName As String = ""
                        '
                        Dim viewId As Integer = CP.Doc.GetInteger(RequestNameSubForm)
                        If viewId = 0 Then
                            '
                            ' Set defaults and go to first form
                            '
                            Call WizardController.clearWizardValues(CP)
                            ImportContentID = CP.Doc.GetInteger("cid")
                            If ImportContentID <> 0 Then
                                Call WizardController.saveWizardValue(CP, RequestNameImportContentID, CStr(ImportContentID))
                            End If
                            viewId = viewIdSelectSource
                            Call WizardController.loadWizardPath(app)
                        End If
                        '
                        ' Load the importmap with what we have so far
                        '
                        ImportMapFile = WizardController.getWizardValue(CP, RequestNameImportMapFile, app.getDefaultImportMapFile(CP))
                        ImportMapData = CP.CdnFiles.Read(ImportMapFile)
                        ImportMap = loadImportMap(CP, ImportMapData)
                        '
                        ' Process incoming form
                        '
                        Select Case viewId
                            Case viewIdSelectSource
                                '
                                ' Source and ContentName
                                '
                                viewId = SelectSourceView.processView(app)
                            Case viewIdUpload
                                '
                                ' Upload
                                '
                                viewId = UploadView.processView(app)
                            Case viewIdSelectFile
                                '
                                '
                                '
                                Filename = CP.Doc.GetText("SelectFile")
                                If Left(Filename, 1) = "\" Then
                                    Filename = Mid(Filename, 2)
                                End If
                                Call WizardController.saveWizardValue(CP, RequestNameImportUpload, Filename)
                                WizardController.loadWizardPath(app)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonContinue2
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdResourceLibrary
                                '
                                '
                                '
                                ProcessError = True
                                Call CP.UserError.Add("Under Construction")
                                WizardController.loadWizardPath(app)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonContinue2
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdSelectDestination
                                '
                                ' Source and ContentName
                                '
                                useNewContentName = CP.Doc.GetBoolean("useNewContentName")
                                If useNewContentName Then
                                    newContentName = CP.Doc.GetText("newContentName")
                                    ImportMap.ContentName = newContentName
                                    ImportMap.importToNewContent = True
                                    ImportMap.SkipRowCnt = 1
                                    Call MapController.saveImportMap(app, ImportMap)
                                    Call WizardController.saveWizardRequestInteger(CP, RequestNameImportContentID)
                                    Select Case Button
                                        Case ButtonFinish
                                            viewId = 1
                                        Case ButtonBack2
                                            viewId = viewIdUpload
                                        Case ButtonContinue2
                                            viewId = viewIdFinish
                                        Case Else
                                            viewId = viewIdSelectDestination
                                    End Select
                                Else
                                    Dim ContentID As Integer = CP.Doc.GetInteger(RequestNameImportContentID)
                                    Dim ContentName As String = CP.Content.GetRecordName("content", ContentID)
                                    ImportMap.ContentName = ContentName
                                    ImportMap.importToNewContent = False
                                    Call MapController.saveImportMap(app, ImportMap)
                                    Call WizardController.saveWizardRequestInteger(CP, RequestNameImportContentID)
                                    WizardController.loadWizardPath(app)

                                    Select Case Button
                                        Case ButtonBack2
                                            viewId = previousSubFormID(app, viewId)
                                        Case ButtonContinue2
                                            viewId = nextSubFormID(app, viewId)
                                    End Select
                                End If
                            Case viewIdNewMapping
                                '
                                ' Mapping - Save Values to the file pointed to by RequestNameImportMapFile
                                '
                                If CP.Doc.GetBoolean(RequestNameImportSkipFirstRow) Then
                                    ImportMap.SkipRowCnt = 1
                                Else
                                    ImportMap.SkipRowCnt = 0
                                End If
                                Dim FieldCnt As Integer = CP.Doc.GetInteger("ccnt")
                                ImportMap.MapPairCnt = FieldCnt
                                If FieldCnt > 0 Then
                                    ReDim ImportMap.MapPairs(FieldCnt - 1)
                                    If FieldCnt > 0 Then
                                        For Ptr = 0 To FieldCnt - 1
                                            SourceFieldPtr = CP.Doc.GetInteger("SOURCEFIELD" & Ptr)
                                            Dim DbField As String = CP.Doc.GetText("DBFIELD" & Ptr)
                                            ImportMap.MapPairs(Ptr) = New MapPairType With {
                                                    .SourceFieldPtr = SourceFieldPtr,
                                                    .DbField = DbField
                                                }
                                            Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(DbField) & ")and(contentid=" & CP.Content.GetID(ImportMap.ContentName) & ")")
                                            If (fieldList.Count > 0) Then
                                                ImportMap.MapPairs(Ptr).DbFieldType = fieldList.First().type
                                            End If
                                        Next
                                    End If
                                End If
                                Call MapController.saveImportMap(app, ImportMap)
                                WizardController.loadWizardPath(app)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonContinue2
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdSelectKey
                                '
                                ' Select Key Field
                                '
                                ImportMap.KeyMethodID = CP.Doc.GetInteger(RequestNameImportKeyMethodID)
                                ImportMap.SourceKeyField = CP.Doc.GetText(RequestNameImportSourceKeyFieldPtr)
                                ImportMap.DbKeyField = CP.Doc.GetText(RequestNameImportDbKeyField)
                                If Not String.IsNullOrEmpty(ImportMap.DbKeyField) Then
                                    Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(ImportMap.DbKeyField) & ")and(contentid=" & CP.Content.GetID(ImportMap.ContentName) & ")")
                                    If (fieldList.Count > 0) Then
                                        ImportMap.DbKeyFieldType = fieldList.First().type
                                    End If
                                End If
                                Call MapController.saveImportMap(app, ImportMap)
                                WizardController.loadWizardPath(app)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonContinue2
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdSelectGroup
                                '
                                ' Add to group
                                '
                                Dim newGroupName As String
                                Dim newGroupID As Integer
                                newGroupName = CP.Doc.GetText(RequestNameImportGroupNew)
                                If Not String.IsNullOrEmpty(newGroupName) Then
                                    Dim groupmodellist As List(Of GroupModel) = DbBaseModel.createList(Of GroupModel)(CP, "(name=" & CP.Db.EncodeSQLText(newGroupName) & ")")
                                    If (groupmodellist.Count <> 0) Then
                                        Dim newGroup As GroupModel = groupmodellist.First
                                        newGroupID = newGroup.id
                                    End If
                                    If newGroupID = 0 Then
                                        Dim newGroup = DbBaseModel.addDefault(Of GroupModel)(CP)
                                        newGroup.name = newGroupName
                                        newGroup.caption = newGroupName
                                        newGroupID = newGroup.id
                                        newGroup.save(CP)
                                    End If

                                End If
                                If newGroupID <> 0 Then
                                    ImportMap.GroupID = newGroupID
                                Else
                                    ImportMap.GroupID = CP.Doc.GetInteger(RequestNameImportGroupID)
                                End If
                                ImportMap.GroupOptionID = CP.Doc.GetInteger(RequestNameImportGroupOptionID)
                                Call MapController.saveImportMap(app, ImportMap)
                                Call WizardController.loadWizardPath(app)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonContinue2
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdFinish
                                '
                                ' Determine next or previous form
                                '
                                Call WizardController.loadWizardPath(app)
                                Call WizardController.saveWizardRequestText(CP, RequestNameImportEmail)

                                Select Case Button
                                    Case ButtonBack2
                                        viewId = previousSubFormID(app, viewId)
                                    Case ButtonFinish
                                        Dim ImportWizardTasks = DbBaseModel.addDefault(Of ImportWizardTaskModel)(CP)
                                        If (ImportWizardTasks IsNot Nothing) Then
                                            ImportWizardTasks.name = Now() & " CSV Import" 'Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                                            ImportWizardTasks.uploadFilename = WizardController.getWizardValue(CP, RequestNameImportUpload, "")
                                            ImportWizardTasks.notifyEmail = WizardController.getWizardValue(CP, RequestNameImportEmail, "")
                                            ImportWizardTasks.importMapFilename = WizardController.getWizardValue(CP, RequestNameImportMapFile, app.getDefaultImportMapFile(CP))
                                            ImportWizardTasks.save(CP)
                                        End If
                                        CP.Addon.ExecuteAsProcess(guidAddonImportTask)
                                        '
                                        'Dim addon As New ProcessClass()
                                        'addon.Execute(CP)
                                        Call WizardController.clearWizardValues(CP)
                                        viewId = nextSubFormID(app, viewId)
                                End Select
                            Case viewIdDone
                                '
                                ' nothing to do, keep same form
                                viewId = viewId
                        End Select
                        '
                        ' Get Next Form
                        '
                        Content &= CP.Html.Hidden(RequestNameSubForm, viewId.ToString)
                        Dim SourceFieldSelect As String
                        Dim Description As String
                        Dim body As String = ""
                        Dim ImportContentName As String
                        Select Case viewId
                            Case viewIdSelectSource, 0
                                '
                                ' -- data source
                                body = SelectSourceView.getView(app)
                            Case viewIdUpload
                                '
                                ' Upload file to Upload folder
                                '
                                body = UploadView.getView(app)
                            Case viewIdSelectFile
                                '
                                ' Select a file from the upload folder
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Description = CP.Html.h4("Select a file from your Upload folder") & CP.Html.p("Select the upload file you wish to import")
                                Call CP.Doc.AddRefreshQueryString(RequestNameSubForm, viewIdSelectFile.ToString)
                                Dim fileList2 As New StringBuilder()
                                Dim uploadPtr As Integer = 0
                                For Each file In CP.CdnFiles.FileList("upload")
                                    Dim uploadId As String = "upload" & uploadPtr
                                    Dim input As String = "<label for=""" & uploadId & """>" & CP.Html.RadioBox("selectfile", "upload\" & file.Name, "", "mr-2", uploadId) & file.Name & "</label>"
                                    fileList2.Append(CP.Html.div(input, "", "pb-2"))
                                    uploadPtr += 1
                                Next
                                Content = fileList2.ToString() & CP.Html.Hidden(RequestNameSubForm, viewId.ToString)


                                Call CP.Doc.AddRefreshQueryString(RequestNameSubForm, CType("", String))
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case viewIdSelectDestination
                                '
                                ' Destination
                                '
                                Dim headerCaption As String = "Import Wizard"
                                ImportContentID = CP.Utils.EncodeInteger(WizardController.getWizardValue(CP, RequestNameImportContentID, CP.Utils.EncodeText(PeopleContentID)))
                                If ImportContentID = 0 Then
                                    ImportContentID = CP.Content.GetID("People")
                                End If
                                Dim inputRadioNewContent As String
                                Dim inputRadioExistingContent As String
                                If useNewContentName Then
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""1"" checked>"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""0"">"
                                Else
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""1"">"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""0"" checked>"
                                End If
                                Description = CP.Html.h4("Select the destination for your data") & CP.Html.p("For example, to import a list in to people, select People.")
                                Dim contentSelect As String = CP.Html.SelectContent(RequestNameImportContentID, ImportContentID.ToString, "Content", "", "", "form-control")
                                contentSelect = contentSelect.Replace("<select ", "<select style=""max-width:300px; display:inline;"" ")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                                    & "<tr><td colspan=""2"">Import into an existing content table</td></tr>" _
                                    & "<tr><td colspan=""2"">" & inputRadioExistingContent & contentSelect & "</td></tr>" _
                                    & "<tr><td colspan=""2"">Create a new content table</td></tr>" _
                                    & "<tr><td colspan=""2"">" & inputRadioNewContent & "<input style=""max-width:300px; display:inline;"" type=""text"" name=""newContentName"" value=""" & newContentName & """ class=""form-control""></td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case viewIdNewMapping
                                '
                                ' Get Mapping fields
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Dim FileData As String = ""
                                Description = CP.Html.h4("Create a New Mapping") & CP.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.")
                                Filename = WizardController.getWizardValue(CP, RequestNameImportUpload, "")
                                If Not String.IsNullOrEmpty(Filename) Then
                                    If Left(Filename, 1) = "\" Then
                                        Filename = Mid(Filename, 2)
                                    End If
                                    FileData = CP.CdnFiles.Read(Filename)
                                End If
                                If String.IsNullOrEmpty(FileData) Then
                                    '
                                    ' no data in upload
                                    '
                                    Content &= "<P>The file you are importing is empty. Please go back and select a different file.</P>"
                                    body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, "", Description, Content)
                                Else
                                    '
                                    ' Skip first Row checkbox
                                    '
                                    Content &= CP.Html.CheckBox(RequestNameImportSkipFirstRow, (ImportMap.SkipRowCnt <> 0)) & "&nbsp;First row contains field names"
                                    Content &= "<div>&nbsp;</div>"
                                    '
                                    ' Build FileColumns
                                    '
                                    Dim DefaultSourceFieldSelect As String = getSourceFieldSelect(CP, Filename, "Ignore")
                                    '
                                    ' Build the Database field list
                                    '
                                    ImportContentID = CInt(WizardController.getWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString))
                                    ImportContentName = CP.Content.GetRecordName("content", ImportContentID)
                                    Dim DBFields() As String = Split(getDbFieldList(CP, ImportContentName, False), ",")
                                    '
                                    ' Output the table
                                    '
                                    Content &= "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>"
                                    Content &= "" _
                                        & "<TR>" _
                                        & "<TD align=left>Data From</TD>" _
                                        & "<TD align=left width=200>Set Value</TD>" _
                                        & "<TD align=center width=10></TD>" _
                                        & "<TD align=left width=200>Save Data To</TD>" _
                                        & "<TD align=left width=200>Type</TD>" _
                                        & "</TR>"
                                    ImportMapFile = WizardController.getWizardValue(CP, RequestNameImportMapFile, app.getDefaultImportMapFile(CP))
                                    ImportMapData = CP.CdnFiles.Read(ImportMapFile)
                                    ImportMap = loadImportMap(CP, ImportMapData)
                                    For Ptr = 0 To UBound(DBFields)
                                        '
                                        ' -- classes for each column
                                        Dim cell0Style As String = ""
                                        Dim cell1Style As String = ""
                                        Dim cell2Style As String = ""
                                        Dim cell3Style As String = ""
                                        Dim cell4Style As String = ""
                                        '
                                        ' -- get field data
                                        Dim DBFieldName As String = DBFields(Ptr)
                                        Dim field As ContentFieldModel = Nothing
                                        Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(DBFieldName) & ")and(contentid=" & CP.Content.GetID(ImportContentName) & ")")
                                        If fieldList.Count > 0 Then
                                            field = fieldList.First()
                                        End If
                                        '
                                        ' -- get row data specific to field type
                                        Dim DbFieldType As String
                                        Dim dataEditor As String = ""
                                        If field Is Nothing Then
                                            DbFieldType = "Text (255 char)"
                                            dataEditor = "<input name="""" type=""text"">"
                                        Else
                                            Dim setValueInput As String = "setValueField" & field.id
                                            Select Case field.type
                                                Case FieldTypeBoolean
                                                    DbFieldType = "true/false"
                                                    dataEditor = "<input type=""checkbox"" name=""" & setValueInput & """ class=""js-import-manual-data"" style=""display:none;"">"
                                                    cell1Style &= "vertical-align:middle;text-align:center;"
                                                Case FieldTypeCurrency, FieldTypeFloat
                                                    DbFieldType = "Number"
                                                    dataEditor = "<input type=""number"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case FieldTypeDate
                                                    DbFieldType = "Date"
                                                    dataEditor = "<input type=""date"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                                    DbFieldType = "Filename"
                                                    dataEditor = "<input type=""file"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case FieldTypeInteger
                                                    DbFieldType = "Integer"
                                                    dataEditor = "<input type=""number"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case FieldTypeLongText, FieldTypeHTML
                                                    DbFieldType = "Text (8000 char)"
                                                    dataEditor = "<input type=""text"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case FieldTypeLookup
                                                    DbFieldType = "Integer ID"
                                                    dataEditor = "<select name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;""><option name=""""></option></select>"
                                                Case FieldTypeManyToMany
                                                    DbFieldType = "Integer ID"
                                                    dataEditor = ""
                                                Case FieldTypeMemberSelect
                                                    DbFieldType = "Integer ID"
                                                    dataEditor = "<select name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;""><option name=""""></option></select>"
                                                Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                                    DbFieldType = "Text (255 char)"
                                                    dataEditor = "<input type=""text"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                                                Case Else
                                                    DbFieldType = "Invalid [" & field.type & "]"
                                                    dataEditor = ""
                                            End Select
                                        End If
                                        SourceFieldSelect = DefaultSourceFieldSelect.Replace("{{fieldId}}", field.id.ToString()).Replace("{{inputName}}", "SourceField" & Ptr)
                                        SourceFieldPtr = -1
                                        If Not String.IsNullOrEmpty(DBFieldName) Then
                                            '
                                            ' Find match in current ImportMap
                                            '
                                            Dim lCaseDbFieldName As String = LCase(DBFieldName)
                                            With ImportMap
                                                If .MapPairCnt > 0 Then
                                                    Dim MapPtr As Integer
                                                    For MapPtr = 0 To .MapPairCnt - 1
                                                        If lCaseDbFieldName = LCase(.MapPairs(MapPtr).DbField) Then
                                                            SourceFieldPtr = .MapPairs(MapPtr).SourceFieldPtr
                                                            Exit For
                                                        End If
                                                    Next
                                                End If
                                            End With
                                            If SourceFieldPtr = -1 Then
                                                '
                                                ' Find a default field - one that matches the DBFieldName if possible
                                                '
                                                Dim TestName As String
                                                If sourceFieldCnt > 0 Then
                                                    For SourceFieldPtr = 0 To sourceFieldCnt - 1
                                                        TestName = sourceFields(SourceFieldPtr)
                                                        TestName = LCase(TestName)
                                                        TestName = Replace(TestName, " ", "")
                                                        '
                                                        ' check for exact match
                                                        '
                                                        If lCaseDbFieldName = TestName Then
                                                            Exit For
                                                        End If
                                                        '
                                                        ' check for pseudo match
                                                        '
                                                        Select Case lCaseDbFieldName
                                                            Case "zip"
                                                                If (TestName = "postalcode") Or (TestName = "zip/postalcode") Then
                                                                    Exit For
                                                                End If
                                                            Case "firstname"
                                                                If TestName = "first" Then
                                                                    Exit For
                                                                End If
                                                            Case "lastname"
                                                                If TestName = "last" Then
                                                                    Exit For
                                                                End If
                                                            Case "email"
                                                                If (TestName = "e-mail") Or (TestName = "emailaddress") Or (TestName = "e-mailaddress") Then
                                                                    Exit For
                                                                End If
                                                            Case "address"
                                                                If (TestName = "address1") Or (TestName = "addressline1") Then
                                                                    Exit For
                                                                End If
                                                            Case "address2"
                                                                If (TestName = "addressline2") Then
                                                                    Exit For
                                                                End If
                                                        End Select
                                                    Next
                                                End If
                                            End If
                                            If SourceFieldPtr >= 0 Then
                                                SourceFieldSelect = Replace(SourceFieldSelect, "value=""" & SourceFieldPtr & """>", "selected value=""" & SourceFieldPtr & """>", , , vbTextCompare)
                                            Else
                                                SourceFieldSelect = Replace(SourceFieldSelect, "value=""-1"">", "selected value=""-1"">", , , vbTextCompare)
                                            End If
                                        Else
                                            DBFieldName = "[blank]"
                                        End If
                                        '
                                        ' Now customize the caption for the DBField a little
                                        '
                                        Dim DBFieldCaption As String = DBFieldName
                                        If Not CP.User.IsDeveloper Then
                                            Select Case LCase(DBFieldCaption)
                                                Case "id"
                                                    DBFieldCaption = "Contensive ID"
                                            End Select
                                        End If
                                        Dim rowStyle As String
                                        Dim cellClass As String = "text-align:center;vertical-align:middle;"
                                        If Ptr Mod 2 = 0 Then
                                            rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;"
                                        Else
                                            rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;"
                                        End If
                                        Content = Content _
                                            & vbCrLf _
                                            & "<TR>" _
                                            & "<TD style=""" & cell0Style & rowStyle & """ align=left>" & SourceFieldSelect & "</td>" _
                                            & "<TD style=""" & cell1Style & rowStyle & """ align=left>" & dataEditor & "</td>" _
                                            & "<TD style=""" & cell2Style & rowStyle & """ align=center>&gt;&gt;</TD>" _
                                            & "<TD style=""" & cell3Style & rowStyle & """ align=left>&nbsp;" & DBFieldCaption & "<input type=hidden name=DbField" & Ptr & " value=""" & DBFieldName & """></td>" _
                                            & "<TD style=""" & cell4Style & rowStyle & """ align=left>&nbsp;" & DbFieldType & "</td>" _
                                            & "</TR>"
                                    Next
                                    Content &= "<input type=hidden name=Ccnt value=" & Ptr & ">"
                                    Content &= "</TABLE>"
                                    body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                                End If
                            Case viewIdSelectKey
                                '
                                ' Select Key
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Dim SourceKeyFieldPtr As Integer
                                Dim DbKeyField As String = ""
                                Dim KeyMethodID As Integer = CP.Utils.EncodeInteger(ImportMap.KeyMethodID)
                                If KeyMethodID = 0 Then
                                    KeyMethodID = KeyMethodUpdateOnMatchInsertOthers
                                End If
                                '
                                If Not String.IsNullOrEmpty(ImportMap.SourceKeyField) Then
                                    SourceKeyFieldPtr = CP.Utils.EncodeInteger(ImportMap.SourceKeyField)
                                Else
                                    SourceKeyFieldPtr = -1
                                End If
                                Filename = WizardController.getWizardValue(CP, RequestNameImportUpload, "")
                                SourceFieldSelect = Replace(getSourceFieldSelect(CP, Filename, "Select One"), "xxxx", RequestNameImportSourceKeyFieldPtr)
                                SourceFieldSelect = Replace(SourceFieldSelect, "value=" & SourceKeyFieldPtr, "value=" & SourceKeyFieldPtr & " selected", , , vbTextCompare)
                                '
                                ImportContentID = CP.Utils.EncodeInteger(WizardController.getWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString))
                                ImportContentName = CP.Content.GetRecordName("content", ImportContentID)
                                Dim note As String

                                Dim DBFieldSelect As String
                                If True Then
                                    'If (Not Main.IsWithinContent(ImportContentName, "People")) Or Main.IsDeveloper Then
                                    '
                                    ' Pick any field for key if developer or not the ccMembers table
                                    '
                                    DbKeyField = ImportMap.DbKeyField
                                    Dim LookupContentName As String
                                    LookupContentName = CP.Content.GetRecordName("content", CP.Utils.EncodeInteger(WizardController.getWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString)))
                                    ' LookupContentName = Main.GetContentNamebyid(kmaEncodeInteger(wizardcontroller.getWizardValue(RequestNameImportContentID, CStr(PeopleContentID))))
                                    DBFieldSelect = Replace(getDbFieldSelect(CP, LookupContentName, "Select One", True), "xxxx", RequestNameImportDbKeyField)
                                    DBFieldSelect = Replace(DBFieldSelect, ">" & DbKeyField & "<", " selected>" & DbKeyField & "<", , , vbTextCompare)
                                    note = ""
                                Else
                                    '
                                    ' non-developer in ccMembers table - limit key fields
                                    '
                                    DBFieldSelect = "" _
                                    & "<select name=" & RequestNameImportDbKeyField & " class=""form-control"">" _
                                    & "<Option value="""">Select One</Option>" _
                                    & "<Option value=ID>Contensive ID</Option>" _
                                    & "<Option value=email>Email</Option>" _
                                    & "<Option value=username>Username</Option>" _
                                    & "" _
                                    & "</select>"
                                    DBFieldSelect = Replace(DBFieldSelect, "value=" & DbKeyField & ">", "value=" & DbKeyField & " selected>", , , vbTextCompare)
                                    note = "<p>note: As a non-developer, your Database key options are limited to id, email and username.</p>"
                                End If
                                '
                                Description = CP.Html.h4("Update Control") & CP.Html.p("When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted.")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                                    & "<TR><TD colspan=2>Key Fields</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" _
                                        & "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>" _
                                        & "<tr><td>Imported&nbsp;Key&nbsp;</td><td>" & SourceFieldSelect & "</td></tr>" _
                                        & "<tr><td>Database&nbsp;Key&nbsp;</td><td>" & DBFieldSelect & "</td></tr>" _
                                        & "</table>" _
                                        & "</td></tr>" _
                                    & "<TR><TD colspan=2>Update Options</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodInsertAll.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Insert all imported data, regardless of key field.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatchInsertOthers.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Update database records when the data in the key fields match. Insert all other imported data.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatch.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Update database records when the data in the key fields match. Ignore all other imported data.</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case viewIdSelectGroup
                                '
                                ' Select a group to add
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Dim GroupOptionID = ImportMap.GroupOptionID
                                If GroupOptionID = 0 Then
                                    GroupOptionID = GroupOptionNone
                                End If
                                Description = CP.Html.h4("Group Membership") & CP.Html.p("When your data is imported, people can be added to a group automatically. Select the option below, and a group.")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                                    & "<TR><TD colspan=2>Add to Existing Group</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.SelectContent(RequestNameImportGroupID, ImportMap.GroupID.ToString, "Groups", "", "", "form-control") & "</td></tr>" _
                                    & "<TR><TD colspan=2>Create New Group</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.InputText(RequestNameImportGroupNew, "", 100, "form-control") & "</td></tr>" _
                                    & "<TR><TD colspan=2>Group Options</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionNone.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Do not add to a group.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionAll.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add everyone to the the group.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys match.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnNoMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys do NOT match.</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case viewIdFinish
                                '
                                ' Ask for an email address to notify when the list is complete
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Description = CP.Html.h4("Finish") & CP.Html.p("Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete.")
                                Content &= "<div Class=""p-2""><label for=""name381"">Email</label><div class=""ml-5"">" & CP.Html5.InputText(RequestNameImportEmail, 255, CP.User.Email) & "</div><div class=""ml-5""><small class=""form-text text-muted""></small></div></div>"
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Case viewIdDone
                                '
                                ' Thank you
                                '
                                Dim headerCaption As String = "Import Wizard"
                                Description = CP.Html.h4("Import Requested") & CP.Html.p("Your import is underway and should only take a moment.")
                                body = WizardController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Case Else
                        End Select
                        '
                        body = CP.Html.Form(body)
                        GetForm = body
                    End If
                    result = GetForm
                End Using
                Return result
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '=====================================================================================
        ''' <summary>
        ''' Wrap the wizard content in a form
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="wizardContent"></param>
        ''' <returns></returns>
        Private ReadOnly Property getAdminFormBody(cp As CPBaseClass, wizardContent As String) As String
            Get
                Try
                    Return cp.Html.Form(cp.Html.div(wizardContent))
                Catch ex As Exception
                    cp.Site.ErrorReport(ex)
                    Throw
                End Try
            End Get
        End Property
        '
        '=====================================================================================
        ''' <summary>
        ''' Get next wizard form
        ''' </summary>
        ''' <param name="SubformID"></param>
        ''' <returns></returns>
        Private Function nextSubFormID(app As ApplicationController, SubformID As Integer) As Integer
            Try
                Dim Ptr As Integer = 0
                Do While Ptr < viewIdMax
                    If SubformID = app.wizard.Path(Ptr) Then
                        nextSubFormID = app.wizard.Path(Ptr + 1)
                        Exit Do
                    End If
                    Ptr += 1
                Loop
            Catch ex As Exception
                Throw
            End Try
        End Function
        '
        '=====================================================================================
        ''' <summary>
        ''' get previous wizard form
        ''' </summary>
        ''' <param name="SubformID"></param>
        ''' <returns></returns>
        Private Function previousSubFormID(app As ApplicationController, SubformID As Integer) As Integer
            Try
                Dim Ptr As Integer
                '
                Ptr = 1
                Do While Ptr < viewIdMax
                    If SubformID = app.wizard.Path(Ptr) Then
                        previousSubFormID = app.wizard.Path(Ptr - 1)
                        Exit Do
                    End If
                    Ptr += 1
                Loop
            Catch ex As Exception
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get the database field list for this content
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ContentName"></param>
        ''' <param name="AllowID"></param>
        ''' <returns></returns>
        Private Function getDbFieldList(cp As CPBaseClass, ContentName As String, AllowID As Boolean) As String
            Try
                Dim result As String = "," & cp.Content.GetProperty(ContentName, "SELECTFIELDLIST") & ","
                If Not AllowID Then
                    result = Replace(result, ",ID,", ",", , , vbTextCompare)
                End If
                result = Replace(result, ",CONTENTCONTROLID,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITSOURCEID,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITBLANK,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITARCHIVE,", ",", , , vbTextCompare)
                result = Replace(result, ",DEVELOPER,", ",", , , vbTextCompare)
                result = Mid(result, 2, Len(result) - 2)
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get an html select with teh current content's fields
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ContentName"></param>
        ''' <param name="NoneCaption"></param>
        ''' <param name="AllowID"></param>
        ''' <returns></returns>
        Private Function getDbFieldSelect(cp As CPBaseClass, ContentName As String, NoneCaption As String, AllowID As Boolean) As String
            Try
                '
                Dim result As String = "" _
                & "<select class=""form-control"" name=xxxx><option value="""" style=""Background-color:#E0E0E0;"">" & NoneCaption & "</option>" _
                & "<option>" & Replace(getDbFieldList(cp, ContentName, AllowID), ",", "</option><option>") & "</option>" _
                & "</select>"
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Load the sourceField and sourceFieldCnt from a wizard file
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="Filename"></param>
        Private Sub loadSourceFields(cp As CPBaseClass, Filename As String)
            Try
                Dim FileData As String
                Dim ignoreLong As Integer
                Dim ignoreBoolean As Boolean
                Dim foundFirstName As Boolean = False
                Dim foundLastName As Boolean = False
                Dim foundName As Boolean = False
                '
                If Not String.IsNullOrEmpty(Filename) Then
                    If sourceFieldCnt = 0 Then
                        FileData = cp.CdnFiles.Read(Filename)
                        If Not String.IsNullOrEmpty(FileData) Then
                            '
                            ' Build FileColumns
                            '
                            Call parseLine(FileData, 1, sourceFields, ignoreLong, ignoreBoolean)
                            '
                            ' todo - implement new fields to allow name/firstname/lastname population
                            'For Each field As String In sourceFields
                            '    foundFirstName = foundFirstName Or field.ToLowerInvariant().Equals("firstname") Or field.ToLowerInvariant().Equals("first name")
                            '    foundLastName = foundLastName Or field.ToLowerInvariant().Equals("lastname") Or field.ToLowerInvariant().Equals("last name")
                            '    foundName = foundName Or field.ToLowerInvariant().Equals("name")
                            'Next
                            'If (foundName And Not foundFirstName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("Name-first-half]")
                            'End If
                            'If (foundName And Not foundLastName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("Name-last-half")
                            'End If
                            'If (Not foundName And foundFirstName And foundLastName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("First-Name Last-Name")
                            'End If
                            sourceFieldCnt = UBound(sourceFields) + 1
                        End If
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
        ''' get an html select with all the fields from the uploaded source data
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="filename"></param>
        ''' <param name="noneCaption"></param>
        ''' <returns></returns>
        Private Function getSourceFieldSelect(cp As CPBaseClass, filename As String, noneCaption As String) As String
            Try
                If String.IsNullOrEmpty(filename) Then Return String.Empty
                Call loadSourceFields(cp, filename)
                If sourceFieldCnt.Equals(0) Then Return String.Empty
                '
                ' Build FileColumns
                '
                Dim result As String = ""
                result = "<select name={{inputName}} class=""form-control js-import-select"" id=""js-import-select-{{fieldId}}"">"
                result &= "<option value=-1>" & noneCaption & "</option>"
                result &= "<option value=-2>Set Value</option>"
                For Ptr As Integer = 0 To sourceFieldCnt - 1
                    result &= "<option value=""" & Ptr & """>column " & (Ptr + 1) & " (" & If(String.IsNullOrEmpty(sourceFields(Ptr)), "[blank]", sourceFields(Ptr)) & ")</option>"
                Next
                result &= "</select>"
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function

    End Class
End Namespace
