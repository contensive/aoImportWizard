
Option Strict On
Option Explicit On

Imports Contensive.Addons.ImportWizard.Controllers
Imports Contensive.BaseClasses
Imports Contensive.Models.Db
Imports System.Linq
Imports System.Text
Imports Contensive.Addons.ImportWizard.Controllers.genericController

Namespace Views
    '
    Public Class PageClass
        Inherits AddonBaseClass
        '
        Private Wizard As New WizardType
        Private DefaultImportMapFile As String
        '
        ' ----- Forms - these must be in the order that they are processed
        '               GetnextForm() uses the Email Wizard row to skip the ones that do not apply
        '               Except:
        '                   1 is the first form
        '                   2 or 3 always follow 1
        '                   2 picks and email, and the email determines the wizard row to start
        '                   3 picks the wizard row to start
        Private SourceFieldCnt As Integer
        Private SourceFields() As String
        '
        ' Import Map file layout
        '
        ' row 0 - KeyMethodID
        ' row 1 - SourceKey Field
        ' row 2 - DbKey Field
        ' row 3 - blank
        ' row4+ SourceField,DbField mapping pairs
        '
        '
        '
        '
        ' - if nuget references are not there, open nuget command line and click the 'reload' message at the top, or run "Update-Package -reinstall" - close/open
        ' - Verify project root name space is empty
        ' - Change the namespace (AddonCollectionVb) to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        '
        '=====================================================================================
        ''' <summary>
        ''' AddonDescription
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ae As New applicationController(CP, False)
                    Dim PeopleContentID As Integer = CP.Content.GetRecordID("content", "people")
                    Dim ProcessError As Boolean = False
                    Dim Content As String = ""
                    Dim Button As String = CP.Doc.GetText(RequestNameButton)
                    If Button = ButtonCancel Then
                        '
                        ' Cancel
                        '
                        Call ClearWizardValues(CP)
                        Call CP.Response.Redirect(CP.Site.AppRootPath & CP.Site.AppPath)
                    Else
                        '
                        Dim SubformID As Integer = CP.Doc.GetInteger(RequestNameSubForm)

                        Dim ImportMap As ImportMapType
                        Dim Ptr As Integer
                        Dim ImportContentID As Integer
                        Dim Filename As String
                        Dim SourceFieldPtr As Integer
                        Dim ImportMapFile As String
                        Dim ImportMapData As String
                        Dim useNewContentName As Boolean
                        Dim newContentName As String
                        If SubformID = 0 Then
                            '
                            ' Set defaults and go to first form
                            '
                            Call ClearWizardValues(CP)
                            ImportContentID = CP.Doc.GetInteger("cid")
                            If ImportContentID <> 0 Then
                                Call SaveWizardValue(CP, RequestNameImportContentID, CStr(ImportContentID))
                            End If
                            SubformID = SubFormSource
                            Call LoadWizardPath(CP)
                        Else
                            '
                            ' Load the importmap with what we have so far
                            '
                            ImportMapFile = GetWizardValue(CP, RequestNameImportMapFile, GetDefaultImportMapFile)
                            ImportMapData = CP.File.ReadVirtual(ImportMapFile)
                            ImportMap = LoadImportMap(CP, ImportMapData)
                            '
                            ' Process incoming form
                            '
                            Select Case SubformID
                                Case SubFormSource
                                    '
                                    ' Source and ContentName
                                    '
                                    Call SaveWizardStreamInteger(CP, RequestNameImportSource)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormSourceUpload
                                    '
                                    ' Upload
                                    '
                                    CP.Html.ProcessInputFile(RequestNameImportUpload, "upload")
                                    Filename = CP.Doc.GetText(RequestNameImportUpload)
                                    Call SaveWizardValue(CP, RequestNameImportUpload, "upload/" & Filename)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormSourceUploadFolder
                                    '
                                    '
                                    '
                                    Filename = CP.Doc.GetText("SelectFile")
                                    If Left(Filename, 1) = "\" Then
                                        Filename = Mid(Filename, 2)
                                    End If
                                    Call SaveWizardValue(CP, RequestNameImportUpload, Filename)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormSourceResourceLibrary
                                    '
                                    '
                                    '
                                    ProcessError = True
                                    Call CP.UserError.Add("Under Construction")
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormDestination
                                    '
                                    ' Source and ContentName
                                    '
                                    useNewContentName = CP.Doc.GetBoolean("useNewContentName")
                                    If useNewContentName Then
                                        newContentName = CP.Doc.GetText("newContentName")
                                        ImportMap.ContentName = newContentName
                                        ImportMap.importToNewContent = True
                                        ImportMap.SkipRowCnt = 1
                                        Call SaveImportMap(CP, ImportMap)
                                        Call SaveWizardStreamInteger(CP, RequestNameImportContentID)
                                        Select Case Button
                                            Case ButtonFinish
                                                SubformID = 1
                                            Case ButtonBack2
                                                SubformID = SubFormSourceUpload
                                            Case ButtonContinue2
                                                SubformID = SubFormFinish
                                            Case Else
                                                SubformID = SubFormDestination
                                        End Select
                                    Else
                                        Dim ContentID As Integer = CP.Doc.GetInteger(RequestNameImportContentID)
                                        Dim ContentName As String = CP.Content.GetRecordName("content", ContentID)
                                        ImportMap.ContentName = ContentName
                                        ImportMap.importToNewContent = False
                                        Call SaveImportMap(CP, ImportMap)
                                        Call SaveWizardStreamInteger(CP, RequestNameImportContentID)
                                        Call LoadWizardPath(CP)

                                        Select Case Button
                                            Case ButtonBack2
                                                SubformID = PreviousSubFormID(SubformID)
                                            Case ButtonContinue2
                                                SubformID = NextSubFormID(SubformID)
                                        End Select
                                    End If
                                Case SubFormNewMapping
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
                                                ImportMap.MapPairs(Ptr) = New MapPairType()
                                                ImportMap.MapPairs(Ptr).SourceFieldPtr = SourceFieldPtr
                                                Dim DbField As String = CP.Doc.GetText("DBFIELD" & Ptr)
                                                ImportMap.MapPairs(Ptr).DbField = DbField
                                                Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(DbField) & ")and(contentid=" & CP.Content.GetID(ImportMap.ContentName) & ")")
                                                If (fieldList.Count > 0) Then
                                                    ImportMap.MapPairs(Ptr).DbFieldType = fieldList.First().type
                                                End If
                                            Next
                                        End If
                                    End If
                                    Call SaveImportMap(CP, ImportMap)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormKey
                                    '
                                    ' Select Key Field
                                    '
                                    ImportMap.KeyMethodID = CP.Doc.GetInteger(RequestNameImportKeyMethodID)
                                    ImportMap.SourceKeyField = CP.Doc.GetText(RequestNameImportSourceKeyFieldPtr)
                                    ImportMap.DbKeyField = CP.Doc.GetText(RequestNameImportDbKeyField)
                                    If ImportMap.DbKeyField <> "" Then
                                        Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(ImportMap.DbKeyField) & ")and(contentid=" & CP.Content.GetID(ImportMap.ContentName) & ")")
                                        If (fieldList.Count > 0) Then
                                            ImportMap.DbKeyFieldType = fieldList.First().type
                                        End If
                                    End If
                                    Call SaveImportMap(CP, ImportMap)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormGroup
                                    '
                                    ' Add to group
                                    '
                                    Dim newGroupName As String
                                    Dim newGroupID As Integer
                                    newGroupName = CP.Doc.GetText(RequestNameImportGroupNew)
                                    If newGroupName <> "" Then
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
                                    Call SaveImportMap(CP, ImportMap)
                                    Call LoadWizardPath(CP)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonContinue2
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormFinish
                                    '
                                    ' Determine next or previous form
                                    '
                                    Call LoadWizardPath(CP)
                                    Call SaveWizardStream(CP, RequestNameImportEmail)

                                    Select Case Button
                                        Case ButtonBack2
                                            SubformID = PreviousSubFormID(SubformID)
                                        Case ButtonFinish
                                            Dim ImportWizardTasks = DbBaseModel.addDefault(Of ImportWizardTaskModel)(CP)
                                            If (ImportWizardTasks IsNot Nothing) Then
                                                ImportWizardTasks.name = Now() & " CSV Import" 'Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                                                ImportWizardTasks.uploadFilename = GetWizardValue(CP, RequestNameImportUpload, "")
                                                ImportWizardTasks.notifyEmail = GetWizardValue(CP, RequestNameImportEmail, "")
                                                ImportWizardTasks.importMapFilename = GetWizardValue(CP, RequestNameImportMapFile, GetDefaultImportMapFile)
                                                ImportWizardTasks.save(CP)
                                            End If
                                            '
                                            Dim addon As New processClass()
                                            addon.Execute(CP)
                                            Call ClearWizardValues(CP)
                                            SubformID = NextSubFormID(SubformID)
                                    End Select
                                Case SubFormDone
                                    '
                                    ' nothing to do, keep same form
                                    SubformID = SubformID
                            End Select
                        End If
                        '
                        ' Get Next Form
                        '
                        Dim HeaderCaption As String = "Import Wizard"
                        Content = Content & CP.Html.Hidden(RequestNameSubForm, SubformID.ToString)
                        Dim SourceFieldSelect As String
                        Dim Description As String
                        Dim WizardContent As String
                        Dim ImportContentName As String
                        Select Case SubformID
                            Case SubFormSource, 0
                                '
                                ' Source
                                '
                                Dim ImportSource As Integer = CP.Utils.EncodeInteger(GetWizardValue(CP, RequestNameImportSource, CP.Utils.EncodeText(ImportSourceUpload)))
                                Description = CP.Html.h4("Select the import source") & CP.Html.p("There are several sources you can use for your data")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                                    & "<TR><TD width=1>" & CP.Html.RadioBox(RequestNameImportSource, ImportSourceUpload.ToString, ImportSource.ToString) & "</td>" _
                                    & "<td width=99% align=left>Upload a comma delimited text file (up to 5 MBytes).</td></tr>" _
                                    & "<TR><TD width=1>" & CP.Html.RadioBox(RequestNameImportSource, ImportSourceUploadFolder.ToString, ImportSource.ToString) & "</td>" _
                                    & "<td width=99% align=left>Use a file already uploaded into your Upload Folder.</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, "", ButtonContinue2, Description, Content)
                            Case SubFormSourceUpload
                                '
                                ' Upload file to Upload folder
                                '

                                Description = CP.Html.h4("Upload your File") & CP.Html.p("Hit browse to locate the file you want to upload")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                                    & "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" & CP.Html.InputFile(RequestNameImportUpload) & "</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                'WizardContent = "hello world"
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case SubFormSourceUploadFolder
                                '
                                ' Select a file from the upload folder
                                '
                                Description = CP.Html.h4("Select a file from your Upload folder") & CP.Html.p("Select the upload file you wish to import")
                                Call CP.Doc.AddRefreshQueryString(RequestNameSubForm, SubFormSourceUploadFolder.ToString)

                                Dim fileList2 As New StringBuilder()
                                For Each file In System.IO.Directory.GetFiles(CP.Site.PhysicalFilePath & "upload")
                                    fileList2.Append(CP.Html.div(CP.Html.RadioBox("selectfile", file, "") & "&nbsp;" & file))
                                Next
                                Content = fileList2.ToString() & CP.Html.Hidden(RequestNameSubForm, SubformID.ToString)


                                Call CP.Doc.AddRefreshQueryString(RequestNameSubForm, CType("", String))
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case SubFormDestination
                                '
                                ' Destination
                                '
                                ImportContentID = CP.Utils.EncodeInteger(GetWizardValue(CP, RequestNameImportContentID, CP.Utils.EncodeText(PeopleContentID)))
                                If ImportContentID = 0 Then
                                    ImportContentID = CP.Content.GetID("People")
                                End If
                                Dim inputRadioNewContent As String
                                Dim inputRadioExistingContent As String
                                If useNewContentName Then
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" value=""1"" checked>"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" value=""0"">"
                                Else
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" value=""1"">"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" value=""0"" checked>"
                                End If
                                Description = CP.Html.h4("Select the destination for your data") & CP.Html.p("For example, to import a list in to people, select People.")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                                    & "<TR><TD colspan=""2"">Import into an existing content table</td></tr>" _
                                    & "<TR><TD colspan=""2"">" & inputRadioExistingContent & CP.Html.SelectContent(RequestNameImportContentID, ImportContentID.ToString, "Content") & "</td></tr>" _
                                    & "<TR><TD colspan=""2"">Create a new content table</td></tr>" _
                                    & "<TR><TD colspan=""2"">" & inputRadioNewContent & "<input type=""text"" name=""newContentName"" value=""" & newContentName & """></td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case SubFormNewMapping
                                '
                                ' Get Mapping fields
                                '
                                Dim FileData As String = ""
                                Description = CP.Html.h4("Create a New Mapping") & CP.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.")
                                Filename = GetWizardValue(CP, RequestNameImportUpload, "")
                                If Filename <> "" Then
                                    If Left(Filename, 1) = "\" Then
                                        Filename = Mid(Filename, 2)
                                    End If
                                    FileData = CP.File.ReadVirtual(Filename)
                                End If
                                If FileData = "" Then
                                    '
                                    ' no data in upload
                                    '
                                    Content = Content & "<P>The file you are importing is empty. Please go back and select a different file.</P>"
                                    WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, "", Description, Content)
                                Else
                                    '
                                    ' Skip first Row checkbox
                                    '
                                    Content = Content & CP.Html.CheckBox(RequestNameImportSkipFirstRow, (ImportMap.SkipRowCnt <> 0)) & "&nbsp;First row contains field names"
                                    Content = Content & "<div>&nbsp;</div>"
                                    '
                                    ' Build FileColumns
                                    '
                                    Dim DefaultSourceFieldSelect As String = GetSourceFieldSelect(CP, Filename, "none")
                                    '
                                    ' Build the Database field list
                                    '
                                    ImportContentID = CInt(GetWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString))
                                    ImportContentName = CP.Content.GetRecordName("content", ImportContentID)
                                    Dim DBFields() As String = Split(GetDbFieldList(CP, ImportContentName, False), ",")
                                    '
                                    ' Output the table
                                    '
                                    Content = Content & vbCrLf & "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>"
                                    Content = Content _
                                        & vbCrLf _
                                        & "<TR>" _
                                        & "<TD align=left>Imported&nbsp;Field</TD>" _
                                        & "<TD align=center width=10></TD>" _
                                        & "<TD align=left width=200>Database&nbsp;Field</TD>" _
                                        & "<TD align=left width=200>Type</TD>" _
                                        & "</TR>"
                                    ImportMapFile = GetWizardValue(CP, RequestNameImportMapFile, GetDefaultImportMapFile)
                                    ImportMapData = CP.File.ReadVirtual(ImportMapFile)
                                    ImportMap = LoadImportMap(CP, ImportMapData)
                                    For Ptr = 0 To UBound(DBFields)
                                        Dim DBFieldName As String = DBFields(Ptr)
                                        Dim field As ContentFieldModel = Nothing
                                        Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(DBFieldName) & ")and(contentid=" & CP.Content.GetID(ImportContentName) & ")")
                                        If (fieldList.Count > 0) Then
                                            field = fieldList.First()
                                        End If
                                        Dim DbFieldType As String
                                        If (field Is Nothing) Then
                                            DbFieldType = "Text (255 chr)"
                                        Else
                                            Select Case field.type
                                                Case FieldTypeBoolean
                                                    DbFieldType = "true/false"
                                                Case FieldTypeCurrency, FieldTypeFloat
                                                    DbFieldType = "Number"
                                                Case FieldTypeDate
                                                    DbFieldType = "Date"
                                                Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                                    DbFieldType = "Filename"
                                                Case FieldTypeInteger
                                                    DbFieldType = "Integer"
                                                Case FieldTypeLongText, FieldTypeHTML
                                                    DbFieldType = "Text (8000 chr)"
                                                Case FieldTypeLookup
                                                    DbFieldType = "Integer ID"
                                                Case FieldTypeManyToMany
                                                    DbFieldType = "Integer ID"
                                                Case FieldTypeMemberSelect
                                                    DbFieldType = "Integer ID"
                                                Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                                    DbFieldType = "Text (255 chr)"
                                                Case Else
                                                    DbFieldType = "Invalid [" & field.type & "]"
                                            End Select
                                        End If
                                        SourceFieldSelect = DefaultSourceFieldSelect
                                        SourceFieldSelect = Replace(SourceFieldSelect, "xxxx", "SourceField" & Ptr)
                                        SourceFieldPtr = -1
                                        If DBFieldName <> "" Then
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
                                                If SourceFieldCnt > 0 Then
                                                    For SourceFieldPtr = 0 To SourceFieldCnt - 1
                                                        TestName = SourceFields(SourceFieldPtr)
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
                                        Dim RowStyle As String
                                        If Ptr Mod 2 = 0 Then
                                            RowStyle = """border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;"""
                                        Else
                                            RowStyle = """border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;"""
                                        End If
                                        Content = Content _
                                    & vbCrLf _
                                    & "<TR>" _
                                    & "<TD style=" & RowStyle & " align=left>" & SourceFieldSelect & "</td>" _
                                    & "<TD style=" & RowStyle & " align=center>&gt;&gt;</TD>" _
                                    & "<TD style=" & RowStyle & " align=left>&nbsp;" & DBFieldCaption & "<input type=hidden name=DbField" & Ptr & " value=""" & DBFieldName & """></td>" _
                                    & "<TD style=" & RowStyle & " align=left>&nbsp;" & DbFieldType & "</td>" _
                                    & "</TR>"
                                    Next
                                    Content = Content & "<input type=hidden name=Ccnt value=" & Ptr & ">"
                                    Content = Content & "</TABLE>"
                                    WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                                End If
                            Case SubFormKey
                                '
                                ' Select Key
                                '
                                Dim SourceKeyFieldPtr As Integer
                                Dim DbKeyField As String = ""
                                Dim KeyMethodID As Integer = CP.Utils.EncodeInteger(ImportMap.KeyMethodID)
                                If KeyMethodID = 0 Then
                                    KeyMethodID = KeyMethodUpdateOnMatchInsertOthers
                                End If
                                '
                                If ImportMap.SourceKeyField <> "" Then
                                    SourceKeyFieldPtr = CP.Utils.EncodeInteger(ImportMap.SourceKeyField)
                                Else
                                    SourceKeyFieldPtr = -1
                                End If
                                Filename = GetWizardValue(CP, RequestNameImportUpload, "")
                                SourceFieldSelect = Replace(GetSourceFieldSelect(CP, Filename, "Select One"), "xxxx", RequestNameImportSourceKeyFieldPtr)
                                SourceFieldSelect = Replace(SourceFieldSelect, "value=" & SourceKeyFieldPtr, "value=" & SourceKeyFieldPtr & " selected", , , vbTextCompare)
                                '
                                ImportContentID = CP.Utils.EncodeInteger(GetWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString))
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
                                    LookupContentName = CP.Content.GetRecordName("content", CP.Utils.EncodeInteger(GetWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString)))
                                    ' LookupContentName = Main.GetContentNamebyid(kmaEncodeInteger(GetWizardValue(RequestNameImportContentID, CStr(PeopleContentID))))
                                    DBFieldSelect = Replace(GetDbFieldSelect(CP, LookupContentName, "Select One", True), "xxxx", RequestNameImportDbKeyField)
                                    DBFieldSelect = Replace(DBFieldSelect, ">" & DbKeyField & "<", " selected>" & DbKeyField & "<", , , vbTextCompare)
                                    note = ""
                                Else
                                    '
                                    ' non-developer in ccMembers table - limit key fields
                                    '
                                    DBFieldSelect = "" _
                                    & "<select name=" & RequestNameImportDbKeyField & ">" _
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
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                                Dim GroupID As Integer
                                Dim GroupOptionID As Integer

                            Case SubFormGroup
                                '
                                ' Select a group to add
                                '
                                Dim GroupOptionID = ImportMap.GroupOptionID
                                If GroupOptionID = 0 Then
                                    GroupOptionID = GroupOptionNone
                                End If
                                Description = CP.Html.h4("Group Membership") & CP.Html.p("When your data is imported, people can be added to a group automatically. Select the option below, and a group.")
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                                    & "<TR><TD colspan=2>Add to Existing Group</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.SelectContent(RequestNameImportGroupID, ImportMap.GroupID.ToString, "Groups") & "</td></tr>" _
                                    & "<TR><TD colspan=2>Create New Group</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.InputText(RequestNameImportGroupNew, "") & "</td></tr>" _
                                    & "<TR><TD colspan=2>Group Options</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionNone.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Do not add to a group.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionAll.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add everyone to the the group.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys match.</td></tr>" _
                                    & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnNoMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys do NOT match.</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Case SubFormFinish
                                '
                                ' Ask for an email address to notify when the list is complete
                                '
                                Description = CP.Html.h4("Finish") & CP.Html.p("Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete.")
                                Content &= "<div Class=""p-2""><label for=""name381"">Email</label><div class=""ml-5"">" & CP.Html5.InputText(RequestNameImportEmail, 255, CP.User.Email) & "</div><div class=""ml-5""><small class=""form-text text-muted""></small></div></div>"
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Case SubFormDone
                                '
                                ' Thank you
                                '
                                Description = CP.Html.h4("Import Requested") & CP.Html.p("Your import is underway and should only take a moment.")
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Case Else
                        End Select
                        '
                        'GetForm = WizardContent
                        GetForm = GetAdminFormBody(CP, "", "", "", True, True, "", "", 20, WizardContent)
                    End If
                    result = GetForm.ToString
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        Private ReadOnly Property GetAdminFormBody(cp As CPBaseClass, v1 As String, v2 As String, v3 As String, v4 As Boolean, v5 As Boolean, v6 As String, v7 As String, v8 As Integer, wizardContent As String) As Object
            Get
                Dim result As String = ""
                result = cp.Html.div(wizardContent)
                result = cp.Html.Form(result)
                Return result

                'If wizardContent = "" Then
                '    Return cp.Html.Form(My.Resources.importWizardSelectLayout)
                'Else
                '    Return cp.Html.Form(wizardContent)
                'End If
            End Get
        End Property


        Public Const RequestNameSubForm = "SubForm"

        Private ReadOnly Property GetWizardContent(cp As CPBaseClass, headerCaption As String, buttonCancel As String, buttonback2 As String, buttonContinue2 As String, description As String, WizardContent As String) As String
            Get
                Dim body As String = ""
                If buttonback2 = "" Then
                    body = "<div Class=""bg-white p-4"">" _
                            & cp.Html.h2(headerCaption) _
                            & cp.Html.div(description) _
                            & cp.Html.div(WizardContent) _
                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")

                Else
                    body = "<div Class=""bg-white p-4"">" _
                            & cp.Html.h2(headerCaption) _
                            & cp.Html.div(description) _
                            & cp.Html.div(WizardContent) _
                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonback2) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")
                End If
                Return body
            End Get
        End Property

        Public Property GetForm As Object

        Private Sub ClearWizardValues(cp As CPBaseClass)
            Call cp.Db.ExecuteSQL("delete from ccProperties where name Like 'ImportWizard.%' and typeid=1 and keyid=" & cp.Visit.Id)
        End Sub
        '
        '
        '
        Private Function NextSubFormID(SubformID As Integer) As Integer
            Dim Ptr As Integer
            '
            Ptr = 0
            Do While Ptr < SubFormMax
                If SubformID = Wizard.Path(Ptr) Then
                    NextSubFormID = Wizard.Path(Ptr + 1)
                    Exit Do
                End If
                Ptr = Ptr + 1
            Loop
        End Function
        '
        '
        '
        Private Function PreviousSubFormID(SubformID As Integer) As Integer
            Dim Ptr As Integer
            '
            Ptr = 1
            Do While Ptr < SubFormMax
                If SubformID = Wizard.Path(Ptr) Then
                    PreviousSubFormID = Wizard.Path(Ptr - 1)
                    Exit Do
                End If
                Ptr = Ptr + 1
            Loop
        End Function
        '
        '====================================================================================================
        ' Load the wizard variables, and build the .Path used for next and previous calls
        '
        ' If the WizardId property is set, load it and use it
        ' If no Wizard ID, use the Group wizard by default
        ' If during form processing, the wizard changes, the process must save the new wizardid
        '
        '====================================================================================================
        '
        Private Sub LoadWizardPath(cp As CPBaseClass)
            Dim result As String = ""
            Try
                '
                Dim CS As Integer
                Dim EmailID As Integer
                Dim CSEmail As Integer
                Dim CSWizard As Integer
                Dim ImportWizardID As Integer
                Dim ValueString As String
                Dim EmailCID As Integer
                '
                ' Get the saved ImportWizardID
                '
                ImportWizardID = cp.Utils.EncodeInteger(GetWizardValue(cp, RequestNameImportWizardID, ""))
                '
                If ImportWizardID = 0 Then
                    '
                    ' Default Wizard, for any type of email, nothing disabled
                    '
                    EmailCID = cp.Content.GetRecordID("content", "Email Templates")
                    Wizard.GroupFormInstructions = "Select Group"
                    Wizard.KeyFormInstructions = "Select the key field"
                    Wizard.MappingFormInstructions = "Set Mapping"
                    Wizard.SourceFormInstructions = "Select the source"
                    Wizard.UploadFormInstructions = "Upload the file"
                    '        '
                Else
                    Call LoadWizard(ImportWizardID)
                End If
                '
                ' Build Wizard path from path properties
                '
                With Wizard
                    .PathCnt = 0
                    ReDim .Path(SubFormMax)
                    '
                    .Path(.PathCnt) = SubFormSource
                    .PathCnt = .PathCnt + 1

                    Select Case cp.Utils.EncodeInteger(GetWizardValue(cp, RequestNameImportSource, CType(ImportSourceUpload, String)))
                        Case ImportSourceUpload
                            '
                            '
                            '
                            .Path(.PathCnt) = SubFormSourceUpload
                            .PathCnt = .PathCnt + 1
                        Case ImportSourceUploadFolder
                            '
                            '
                            '
                            .Path(.PathCnt) = SubFormSourceUploadFolder
                            .PathCnt = .PathCnt + 1
                        Case Else
                            '
                            '
                            '
                            .Path(.PathCnt) = SubFormSourceResourceLibrary
                            .PathCnt = .PathCnt + 1
                    End Select
                    '
                    '
                    '
                    .Path(.PathCnt) = SubFormDestination
                    .PathCnt = .PathCnt + 1
                    '
                    '
                    '
                    .Path(.PathCnt) = SubFormNewMapping
                    .PathCnt = .PathCnt + 1
                    '
                    '
                    '
                    .Path(.PathCnt) = SubFormKey
                    .PathCnt = .PathCnt + 1
                    '
                    '
                    '
                    If cp.Utils.EncodeInteger(GetWizardValue(cp, RequestNameImportContentID, "0")) = cp.Content.GetID("people") Then
                        '
                        ' if importing into people, get them the option of adding to a group
                        '
                        .Path(.PathCnt) = SubFormGroup
                        .PathCnt = .PathCnt + 1
                    End If
                    '
                    ' Finish
                    '
                    .Path(.PathCnt) = SubFormFinish
                    .PathCnt = .PathCnt + 1
                    '
                    ' Done, thank you
                    '
                    .Path(.PathCnt) = SubFormDone
                    .PathCnt = .PathCnt + 1
                End With
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return
        End Sub
        '
        '
        '
        Private Sub SaveWizardValue(cp As CPBaseClass, Name As String, Value As String)
            Call cp.Visit.SetProperty("ImportWizard." & Name, Value)
        End Sub
        '
        '
        '
        Private Function GetWizardValue(cp As CPBaseClass, Name As String, DefaultValue As String) As String
            GetWizardValue = cp.Visit.GetProperty("ImportWizard." & Name, DefaultValue)
        End Function
        '
        '
        '
        Private Sub SaveWizardStreamInteger(cp As CPBaseClass, RequestName As String)
            Call SaveWizardValue(cp, RequestName, cp.Doc.GetText(RequestName))
        End Sub
        '
        '
        '
        Private Sub SaveWizardStream(cp As CPBaseClass, RequestName As String)
            Call SaveWizardValue(cp, RequestName, cp.Doc.GetText(RequestName))
        End Sub
        '
        '
        '
        Private Sub SaveWizardFileValue(cp As CPBaseClass, Name As String, Value As String)
            Dim Filename As String
            '
            Filename = GetWizardValue(cp, Name, "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt")
            If Filename = "" Then
                Filename = "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt"
                Call SaveWizardValue(cp, Name, Filename)
            End If
            Call cp.CdnFiles.Save(Filename, Value)
        End Sub

        '
        '
        '
        Private Function GetWizardFileValue(cp As CPBaseClass, Name As String, DefaultValue As String) As String
            Try
                Dim result As String = ""
                Dim Filename As String = GetWizardValue(cp, Name, "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt")
                If Filename <> "" Then
                    result = cp.CdnFiles.Read(Filename)
                End If
                If result = "" Then
                    result = DefaultValue
                    Call SaveWizardFileValue(cp, Name, result)
                End If
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return String.Empty
            End Try
        End Function
        '
        '
        '
        Private Sub LoadAllImportWizardValues(EmailID As Integer)

        End Sub
        '
        '
        '
        Private Sub LoadWizard(ImportWizardID As Integer)
            '    On Error GoTo ErrorTrap
        End Sub
        '
        '
        '
        Private Function getMemberSelect(cp As CPBaseClass, RequestName As String, MemberID As Integer) As String
            Dim result As String = ""
            Try
                Dim ContentID As Integer = 0
                Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name='testmemberid')and(contentid=" & cp.Content.GetID("email") & ")")
                If (fieldList.Count > 0) Then
                    ContentID = fieldList.First().lookupContentId
                End If
                Dim ContentName As String = cp.Content.GetRecordName("content", ContentID)
                If ContentName = "" Then
                    ContentName = "Members"
                End If
                Dim cs1 As CPCSBaseClass = cp.CSNew()
                cs1.Open(ContentName)
                If cs1.OK Then
                    result &= "<div>There are no members to select</div>"
                Else
                    result &= "<select size=1 name=" & RequestName & "><option value=0>Select One</Option>"
                    Do While cs1.OK
                        Dim recordId As Integer = cs1.GetInteger("ID")
                        Dim Email As String = cs1.GetText("email")
                        result &= "<option value=" & recordId
                        If recordId = MemberID Then
                            result &= " selected "
                        End If
                        If Email = "" Then
                            result &= ">" & cs1.GetText("name") & " &lt;no email address&gt;</option>"
                        Else
                            result &= ">" & cs1.GetText("name") & " &lt;" & Email & "&gt;</option>"
                        End If
                        cs1.GoNext()
                    Loop
                    result &= "</select>"
                End If
                Call cs1.Close()
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function GetDbFieldList(cp As CPBaseClass, ContentName As String, AllowID As Boolean) As String
            Dim result As String = ""
            Try
                '
                GetDbFieldList = "," & cp.Content.GetProperty(ContentName, "SELECTFIELDLIST") & ","
                If Not AllowID Then
                    GetDbFieldList = Replace(GetDbFieldList, ",ID,", ",", , , vbTextCompare)
                End If
                GetDbFieldList = Replace(GetDbFieldList, ",CONTENTCONTROLID,", ",", , , vbTextCompare)
                GetDbFieldList = Replace(GetDbFieldList, ",EDITSOURCEID,", ",", , , vbTextCompare)
                GetDbFieldList = Replace(GetDbFieldList, ",EDITBLANK,", ",", , , vbTextCompare)
                GetDbFieldList = Replace(GetDbFieldList, ",EDITARCHIVE,", ",", , , vbTextCompare)
                GetDbFieldList = Replace(GetDbFieldList, ",DEVELOPER,", ",", , , vbTextCompare)
                GetDbFieldList = Mid(GetDbFieldList, 2, Len(GetDbFieldList) - 2)
                '
                result = GetDbFieldList
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function GetDbFieldSelect(cp As CPBaseClass, ContentName As String, NoneCaption As String, AllowID As Boolean) As String
            Dim result As String = ""
            Try
                '
                GetDbFieldSelect = "" _
                & "<select name=xxxx><option value="""" style=""Background-color:#E0E0E0;"">" & NoneCaption & "</option>" _
                & "<option>" & Replace(GetDbFieldList(cp, ContentName, AllowID), ",", "</option><option>") & "</option>" _
                & "</select>"
                '
                result = GetDbFieldSelect
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        '
        '
        '
        Private Sub LoadSourceFields(cp As CPBaseClass, Filename As String)
            Dim result As String = ""
            Try
                '
                Dim FileData As String
                Dim ignoreLong As Integer
                Dim ignoreBoolean As Boolean
                '
                If Filename <> "" Then
                    If SourceFieldCnt = 0 Then
                        FileData = cp.File.ReadVirtual(Filename)
                        If FileData <> "" Then
                            '
                            ' Build FileColumns
                            '
                            Call parseLine(FileData, 1, SourceFields, ignoreLong, ignoreBoolean)
                            SourceFieldCnt = UBound(SourceFields) + 1
                        End If
                    End If
                End If

                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return
        End Sub


        '
        '
        '
        Private Function GetSourceFieldSelect(cp As CPBaseClass, Filename As String, NoneCaption As String) As String
            Dim result As String = ""
            Try
                '
                Dim FileData As String
                Dim FileRows() As String
                Dim Ptr As Integer
                Dim FileColumns() As String
                Dim ColumnName As String
                '
                If Filename <> "" Then
                    Call LoadSourceFields(cp, Filename)
                    '
                    ' Build FileColumns
                    '
                    GetSourceFieldSelect = vbCrLf & "<select name=xxxx><option style=""Background-color:#E0E0E0;"" value=-1>" & NoneCaption & "</option>"
                    For Ptr = 0 To SourceFieldCnt - 1
                        ColumnName = SourceFields(Ptr)
                        If ColumnName = "" Then
                            ColumnName = "[blank]"
                        End If
                        GetSourceFieldSelect = GetSourceFieldSelect & vbCrLf & "<option value=""" & Ptr & """>" & (Ptr + 1) & " (" & ColumnName & ")</option>"
                    Next
                    GetSourceFieldSelect = GetSourceFieldSelect & vbCrLf & "</select>"
                End If

                '
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try

        End Function
        '
        '
        '
        Private Sub SaveImportMap(cp As CPBaseClass, ImportMap As ImportMapType)
            Dim result As String = ""
            Try
                '
                Dim ImportMapFile As String
                Dim ImportMapData As String
                Dim Rows() As String
                Dim Pair() As String
                Dim Ptr As Integer
                '
                ImportMapFile = GetWizardValue(cp, RequestNameImportMapFile, GetDefaultImportMapFile)
                ImportMapData = "" _
                & ImportMap.KeyMethodID _
                & vbCrLf & ImportMap.SourceKeyField _
                & vbCrLf & ImportMap.DbKeyField _
                & vbCrLf & ImportMap.ContentName _
                & vbCrLf & ImportMap.GroupOptionID _
                & vbCrLf & ImportMap.GroupID _
                & vbCrLf & ImportMap.SkipRowCnt _
                & vbCrLf & ImportMap.DbKeyFieldType _
                & vbCrLf & ImportMap.importToNewContent _
                & vbCrLf
                If ImportMap.MapPairCnt > 0 Then
                    For Ptr = 0 To ImportMap.MapPairCnt - 1
                        ImportMapData = ImportMapData & vbCrLf & ImportMap.MapPairs(Ptr).DbField & "=" & ImportMap.MapPairs(Ptr).SourceFieldPtr & "," & ImportMap.MapPairs(Ptr).DbFieldType
                    Next
                End If
                Call cp.File.SaveVirtual(ImportMapFile, ImportMapData)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub

        '
        '
        '
        Private Function GetDefaultImportMapFile() As String
            If DefaultImportMapFile = "" Then
                Dim GetRandomInteger As String = Nothing
                DefaultImportMapFile = "ImportWizard\ImportMap" & GetRandomInteger & ".txt"
            End If
            GetDefaultImportMapFile = DefaultImportMapFile

        End Function

    End Class
End Namespace
