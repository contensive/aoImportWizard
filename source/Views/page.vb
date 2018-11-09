
Option Strict On
Option Explicit On

Imports Contensive.Addons.ImportWizard.Controllers
Imports Contensive.BaseClasses
Imports Contensive.Addons.ImportWizard.Models
Imports System.Linq
Imports System.Text

Namespace Views
    '
    Public Class pageClass
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
            'Dim sw As New Stopwatch : sw.Start()
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ae As New applicationController(CP, False)
                    '
                    ' -- your code
                    Const FileViewGuid = "{B966103C-DBF4-4655-856A-3D204DEF6B21}"
                    '
                    Dim OptionString As String
                    Dim FileView As Object
                    Dim ImportMap As ImportMapType
                    Dim MapPtr As Integer
                    Dim lCaseDbFieldName As String
                    Dim KeyMethodID As Integer
                    Dim SourceFieldSelect As String
                    Dim DBFieldSelect As String
                    Dim RowStart As String
                    Dim RowStop As String
                    Dim ValueText As String
                    Dim Content As String
                    Dim ContentID As Integer
                    Dim Description As String
                    Dim WizardContent As String
                    Dim SubformID As Integer
                    Dim HeaderCaption As String
                    Dim ModifyType As Integer
                    Dim Button As String
                    Dim EmailID As Integer
                    Dim EMailName As String
                    Dim CS As Integer
                    Dim ImportWizardID As Integer
                    Dim EMailTemplateID As Integer
                    Dim EmailConditionID As Integer
                    Dim EmailConditionPeriod As Integer
                    Dim SendMethodID As Integer
                    Dim ConditionID As Integer
                    Dim ProcessError As Boolean
                    Dim TestMemberID As Integer
                    Dim EmailAddress As String
                    Dim MemberName As String
                    Dim HeadingContentID As Integer
                    Dim HeadingContentName As String
                    Dim RecordName As String
                    Dim recordId As Integer
                    Dim IDList As String
                    Dim CSList As Integer
                    Dim ButtonList As String
                    Dim EmailScheduleStart As Date
                    Dim EmailFinishID As Integer
                    Dim cnt As Integer
                    Dim Ptr As Integer
                    Dim ImportSource As Integer
                    Dim ImportContentID As Integer
                    Dim ImportContentName As String
                    Dim Filename As String
                    Dim FileData As String
                    Dim FileRows() As String
                    Dim FileColumns() As String
                    Dim DBFieldName As String
                    Dim DefaultSourceFieldSelect As String
                    Dim RowStyle As String
                    Dim DbFieldList As String
                    Dim DBFields() As String
                    Dim DbFieldType As String
                    Dim SourceFieldPtr As Integer
                    Dim ImportMapFile As String
                    Dim ImportMapData As String
                    Dim FieldCnt As Integer
                    Dim DBFieldCaption As String
                    Dim useNewContentName As Boolean
                    Dim newContentName As String
                    Dim PeopleContentID As Integer
                    Dim ContentName As String
                    Dim DbField As String
                    Dim inputRadioNewContent As String
                    Dim inputRadioExistingContent As String
                    '
                    Dim PeopleModelList As List(Of PeopleModel) = PeopleModel.createList(CP, "")
                    PeopleContentID = CP.Content.GetRecordID("content", "people")
                    ProcessError = False
                    Content = ""
                    Button = CP.Doc.GetText(RequestNameButton)
                    If Button = ButtonCancel Then
                        '
                        ' Cancel
                        '
                        Call ClearWizardValues(CP)
                        Call CP.Response.Redirect(CP.Site.AppRootPath & CP.Site.AppPath)
                    Else
                        ' SubformID = Main.GetStreamInteger(RequestNameSubForm)
                        '
                        SubformID = CP.Doc.GetInteger(RequestNameSubForm)

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
                                    'Call SaveWizardValue(RequestNameImportContentName, ContentName)
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
                                    Call SaveWizardValue(CP, RequestNameImportUpload, Filename)
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
                                        ContentID = CP.Doc.GetInteger(RequestNameImportContentID)
                                        ContentName = CP.Content.GetRecordName("content", ContentID)
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
                                        '                        Select Case Button
                                        '                            Case ButtonFinish
                                        '                                SubformID = 1
                                        '                            Case ButtonBack2
                                        '                                SubformID = 1
                                        '                            Case ButtonContinue2
                                        '                                SubformID = 1
                                        '                            Case Else
                                        '                                SubformID = 1
                                        '                        End Select
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
                                    FieldCnt = CP.Doc.GetInteger("ccnt")
                                    ImportMap.MapPairCnt = FieldCnt
                                    If FieldCnt > 0 Then
                                        ReDim ImportMap.MapPairs(FieldCnt - 1)
                                        If FieldCnt > 0 Then
                                            For Ptr = 0 To FieldCnt - 1
                                                SourceFieldPtr = CP.Doc.GetInteger("SOURCEFIELD" & Ptr)
                                                ImportMap.MapPairs(Ptr).SourceFieldPtr = SourceFieldPtr
                                                DbField = CP.Doc.GetText("DBFIELD" & Ptr)
                                                ImportMap.MapPairs(Ptr).DbField = DbField
                                                ImportMap.MapPairs(Ptr).DbFieldType = CP.Utils.EncodeInteger(CP.Content.GetFieldProperty(ImportMap.ContentName, DbField, "Fieldtype"))
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
                                        ImportMap.DbKeyFieldType = CInt(CP.Content.GetFieldProperty(ImportMap.ContentName, ImportMap.DbKeyField, "FieldType"))
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
                                        Dim groupmodellist As List(Of GroupModel) = GroupModel.createList(CP, "name=" & CP.Db.EncodeSQLText(newGroupName))
                                        If (groupmodellist IsNot Nothing) Then
                                            Dim newGroup As GroupModel = groupmodellist.First
                                            newGroupID = newGroup.id
                                        End If
                                        If newGroupID = 0 Then
                                            Dim newGroup = GroupModel.add(CP)
                                            newGroup.name = newGroupName
                                            newGroup.Caption = newGroupName
                                            newGroupID = newGroup.id
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
                                    ' Get email address for email
                                    '
                                    Call SaveWizardStream(CP, RequestNameImportEmail)
                                    If Button = ButtonFinish Then
                                        Dim ImportWizardTasks = ImportWizardTaskModel.add(CP)
                                        ' CS = Main.InsertCSRecord("Import Wizard Tasks")
                                        If (ImportWizardTasks IsNot Nothing) Then
                                            ImportWizardTasks.name = Now() & " CSV Import" 'Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                                            ImportWizardTasks.uploadFilename = GetWizardValue(CP, RequestNameImportUpload, "")
                                            ImportWizardTasks.NotifyEmail = GetWizardValue(CP, RequestNameImportEmail, "")
                                            ImportWizardTasks.ImportMapFilename = GetWizardValue(CP, RequestNameImportMapFile, GetDefaultImportMapFile)
                                            ImportWizardTasks.save(CP)
                                        End If

                                    End If
                                    '
                                    ' Call the background process to do the import
                                    '
                                    If Button = ButtonFinish Then
                                        '
                                        Dim addon As New processClass()
                                        addon.Execute(CP)
                                        'Call CP.Utils.ExecuteAddonAsProcess(ImportProcessAddonGuid)
                                        Call ClearWizardValues(CP)
                                        ' Call CP.Response.Redirect(CP.Site.AppRootPath & CP.Site.AppPath)
                                    Else
                                        '
                                        ' Determine next or previous form
                                        '
                                        Call LoadWizardPath(CP)

                                        Select Case Button
                                            Case ButtonBack2
                                                SubformID = PreviousSubFormID(SubformID)
                                            Case ButtonContinue2
                                                SubformID = NextSubFormID(SubformID)
                                        End Select
                                    End If
                            End Select
                            '            '
                            '            ' Handle back and continue
                            '            '
                            '            If Not ProcessError Then
                            '                If Button = ButtonFinish Then
                            '                    '
                            '                    ' Finished - exit here
                            '                    '
                            '                    Call ClearWizardValues
                            '                    Call Main.Redirect(Main.ServerAppRootPath & Main.ServerAppPath)
                            '                Else
                            '                    '
                            '                    ' Determine next or previous form
                            '                    '
                            '                    Call LoadWizardPath
                            '                    Select Case Button
                            '                        Case ButtonBack2
                            '                            SubformID = PreviousSubFormID(SubformID)
                            '                        Case ButtonContinue2
                            '                            SubformID = NextSubFormID(SubformID)
                            '                    End Select
                            '                End If
                            '            End If
                        End If
                        '
                        ' Get Next Form
                        '
                        HeaderCaption = "Import Wizard"
                        Content = Content & CP.Html.Hidden(RequestNameSubForm, SubformID.ToString)
                        'If CP.UserError Then
                        '    Content = Content & Main.GetUserError()
                        'End If
                        Select Case SubformID
                            Case SubFormSource, 0
                                '
                                ' Source
                                '
                                ImportSource = CP.Utils.EncodeInteger(GetWizardValue(CP, RequestNameImportSource, CP.Utils.EncodeText(ImportSourceUpload)))
                                Description = "<B>Select the import source</B><BR><BR>There are several sources you can use for your data..."
                                Content = Content _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                                    & "<TR><TD width=1>" & CP.Html.RadioBox(RequestNameImportSource, ImportSourceUpload.ToString, ImportSource.ToString) & "</td>" _
                                    & "<td width=99% align=left>""Upload a comma delimited text file (up to 5 MBytes).</td></tr>" _
                                    & "<TR><TD width=1>" & CP.Html.RadioBox(RequestNameImportSource, ImportSourceUploadFolder.ToString, ImportSource.ToString) & "</td>" _
                                    & "<td width=99% align=left>""Use a file already uploaded into your Upload Folder.</td></tr>" _
                                    & "</table>" _
                                    & "</div>" _
                                    & ""
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, "", ButtonContinue2, Description, Content)
                            Case SubFormSourceUpload
                                '
                                ' Upload file to Upload folder
                                '

                                Description = "<B>Upload your File</B><BR><BR>Hit browse to locate the file you want to upload..."
                                Content = Content _
                            & "<div>" _
                            & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                            & "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" & CP.Html.InputFile(RequestNameImportUpload) & "</td></tr>" _
                            & "</table>" _
                            & "</div>" _
                            & ""
                                'WizardContent = "hello world"
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                                'GetForm = "Hello World"
                                'Exit Function
                            Case SubFormSourceUploadFolder
                                '
                                ' Select a file from the upload folder
                                '
                                Description = "<B>Select a file from your Upload folder</B><BR><BR>Select the upload file you wish to import..."
                                Dim AdminFormImportWizard As String = Nothing
                                Call CP.Doc.AddRefreshQueryString("af", AdminFormImportWizard)
                                Call CP.Doc.AddRefreshQueryString(RequestNameSubForm, SubFormSourceUploadFolder.ToString)

                                Dim fileList2 As New StringBuilder()
                                For Each file In System.IO.Directory.GetFiles(CP.Site.PhysicalFilePath & "upload")
                                    fileList2.Append(CP.Html.div(CP.Html.CheckBox(file.Replace(" ", "")) & "&nbsp;" & file))
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
                                If useNewContentName Then
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" value=""1"" checked>"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" value=""0"">"
                                Else
                                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" value=""1"">"
                                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" value=""0"" checked>"
                                End If
                                Description = "<B>Select the destination for your data</B>" _
                                    & "<BR><BR><p>For example, to import a list in to people, select People.</p>"
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
                                FileData = ""
                                Description = "<B>Create a New Mapping</B><BR><BR>This step lets you select which fields in your database you would like each field in your upload to be assigned."
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
                                    DefaultSourceFieldSelect = GetSourceFieldSelect(CP, Filename, "none")
                                    '
                                    ' Build the Database field list
                                    '
                                    ImportContentID = CInt(GetWizardValue(CP, RequestNameImportContentID, PeopleContentID.ToString))
                                    ImportContentName = CP.Content.GetRecordName("content", ImportContentID)
                                    DBFields = Split(GetDbFieldList(ImportContentName, False), ",")
                                    '
                                    ' Output the table
                                    '
                                    Content = Content & vbCrLf & "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>"
                                    Content = Content _
                                & vbCrLf _
                                & "<TR>" _
                                & "<TD width=99% align=left>Imported&nbsp;Field<BR><img src=/cclib/images/spacer.gif width=1 height=1></TD>" _
                                & "<TD width=10 align=center><img src=/cclib/images/spacer.gif width=10 height=1></TD>" _
                                & "<TD width=100 align=left>Database&nbsp;Field<BR><img src=/cclib/images/spacer.gif width=100 height=1></TD>" _
                                & "<TD width=100 align=left>Type<BR><img src=/cclib/images/spacer.gif width=100 height=1></TD>" _
                                & "</TR>"
                                    ImportMapFile = GetWizardValue(CP, RequestNameImportMapFile, GetDefaultImportMapFile)
                                    ImportMapData = CP.File.ReadVirtual(ImportMapFile)
                                    ImportMap = LoadImportMap(CP, ImportMapData)
                                    For Ptr = 0 To UBound(DBFields)
                                        DBFieldName = DBFields(Ptr)
                                        Select Case CP.Utils.EncodeInteger(CP.Content.GetFieldProperty(ImportContentName, DBFieldName, "FieldType"))
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
                                                DbFieldType = "Invalid"
                                        End Select
                                        SourceFieldSelect = DefaultSourceFieldSelect
                                        SourceFieldSelect = Replace(SourceFieldSelect, "xxxx", "SourceField" & Ptr)
                                        SourceFieldPtr = -1
                                        If DBFieldName <> "" Then
                                            '
                                            ' Find match in current ImportMap
                                            '
                                            lCaseDbFieldName = LCase(DBFieldName)
                                            With ImportMap
                                                If .MapPairCnt > 0 Then
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
                                        DBFieldCaption = DBFieldName
                                        If Not CP.User.IsDeveloper Then
                                            Select Case LCase(DBFieldCaption)
                                                Case "id"
                                                    DBFieldCaption = "Contensive ID"
                                            End Select
                                        End If
                                        If Ptr Mod 2 = 0 Then
                                            RowStyle = """border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;"""
                                        Else
                                            RowStyle = """border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;"""
                                        End If
                                        Content = Content _
                                    & vbCrLf _
                                    & "<TR>" _
                                    & "<TD style=" & RowStyle & " width=99% align=left>" & SourceFieldSelect & "</td>" _
                                    & "<TD style=" & RowStyle & " width=10 align=center>&gt;&gt;</TD>" _
                                    & "<TD style=" & RowStyle & " width=100 align=left>&nbsp;" & DBFieldCaption & "<input type=hidden name=DbField" & Ptr & " value=""" & DBFieldName & """></td>" _
                                    & "<TD style=" & RowStyle & " width=100 align=left>&nbsp;" & DbFieldType & "</td>" _
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
                                KeyMethodID = CP.Utils.EncodeInteger(ImportMap.KeyMethodID)
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

                                If True Then
                                    'If (Not Main.IsWithinContent(ImportContentName, "People")) Or Main.IsDeveloper Then
                                    '
                                    ' Pick any field for key if developer or not the ccMembers table
                                    '
                                    DbKeyField = ImportMap.DbKeyField
                                    Dim LookupContentName As String
                                    LookupContentName = CP.Content.GetRecordName(CType(CP.Utils.EncodeInteger(GetWizardValue(CP, CStr(RequestNameImportContentID), CStr(PeopleContentID))), String), 0)
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
                                Description = "<p><B>Update Control</B><BR><BR>When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted.</p>" & note
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
                            & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodInsertAll.ToString, KeyMethodID & "</td><td width=99% align=left>" & "Insert all imported data, regardless of key field.</td></tr>") _
                            & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatchInsertOthers.ToString, KeyMethodID & "</td><td width=99% align=left>" & "Update database records when the data in the key fields match. Insert all other imported data.</td></tr>") _
                            & "<TR><TD width=10>" & CP.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatch.ToString, KeyMethodID & "</td><td width=99% align=left>" & "Update database records when the data in the key fields match. Ignore all other imported data.</td></tr>") _
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
                                Description = "<B>Group Membership</B><BR><BR>When your data is imported, people can be added to a group automatically. Select the option below, and a group."
                                Content = Content _
                            & "<div>" _
                            & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                            & "<TR><TD colspan=2>Add to Existing Group</td></tr>" _
                            & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.SelectContent(RequestNameImportGroupID, ImportMap.GroupID.ToString, "Groups") & "</td></tr>" _
                            & "<TR><TD colspan=2>Create New Group</td></tr>" _
                            & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Doc.GetText(RequestNameImportGroupNew, "") & "</td></tr>" _
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
                                Description = "<B>Finish</B><BR><BR>Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete."
                                Content = Content _
                            & "<div>" _
                            & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                            & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Doc.GetText(RequestNameImportEmail, GetWizardValue(CP, RequestNameImportEmail, CP.User.Email)) & "</td></tr>" _
                            & "</table>" _
                            & "</div>" _
                            & ""
                                WizardContent = GetWizardContent(CP, HeaderCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Case Else
                        End Select
                        '
                        'GetForm = WizardContent
                        GetForm = GetAdminFormBody(CP, "", "", "", True, True, "", "", 20, WizardContent)
                        'GetForm = Replace(GetForm, "<form ", "<xform ", , , vbTextCompare)
                        'GetForm = Replace(GetForm, "</form", "</xform", , , vbTextCompare)
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
                Dim body As String = "<div class=""iWizWrapper"" style=""border:2px solid black;"">" _
                    & cp.Html.div(headerCaption,, "ccWizardHeader") _
                    & cp.Html.div(description) _
                    & cp.Html.div(WizardContent) _
                    & cp.Html.Button("button", buttonCancel) _
                    & cp.Html.Button("button", buttonback2) _
                    & cp.Html.Button("button", buttonContinue2)
                Return body
            End Get
        End Property

        Public Property GetForm As Object

        Private Sub ClearWizardValues(cp As CPBaseClass)
            Call cp.Db.ExecuteSQL("delete from ccProperties where name like 'ImportWizard.%' and typeid=1 and keyid=" & cp.Visit.Id)
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
                    '        Wizard.AllowSpamFooterDefault = True
                    '        '
                    '        Wizard.IncludeAllowSpamFooter = True
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
            Call cp.File.SaveVirtual(Filename, Value)
        End Sub
        '
        '
        '
        Private Sub SaveWizardFileStreamAC(cp As CPBaseClass, RequestName As String)
            Dim Filename As String
            '
            Filename = GetWizardValue(cp, RequestName, "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt")
            If Filename = "" Then
                Filename = "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt"
                Call SaveWizardValue(cp, RequestName, Filename)
            End If
            'Call Main.SaveVirtualFile(Filename, Main.GetStreamActiveContent(RequestName))
            Call cp.File.SaveVirtual(Filename, cp.Html.InputWysiwyg(RequestName))
        End Sub
        '
        '
        '
        Private Function GetWizardFileValue(cp As CPBaseClass, Name As String, DefaultValue As String) As String
            Dim Filename As String
            '
            Filename = GetWizardValue(cp, Name, "Temp/ImportWizard_Visit" & cp.Visit.Id & ".txt")
            If Filename <> "" Then
                GetWizardFileValue = cp.File.ReadVirtual(Filename)
            End If
            If GetWizardFileValue = "" Then
                GetWizardFileValue = DefaultValue
                Call SaveWizardFileValue(cp, Name, GetWizardFileValue)
            End If
        End Function
        '
        '
        '
        Private Sub LoadAllImportWizardValues(EmailID As Integer)
            '    On Error GoTo ErrorTrap
            '    '
            '    Dim ImportWizardID as Integer
            '    Dim CS as Integer
            '    Dim IDList As String
            '    Dim ConditionID as Integer
            '    Dim CSList as Integer
            '    Dim ContentID as Integer
            '    Dim ContentName As String
            '    Dim SendMethodID as Integer
            '    '
            '    ImportWizardID = 0
            '    CS = Main.OpenCSContentRecord("email", EmailID)
            '    If Main.IsCSOK(CS) Then
            '        ConditionID = Main.GetCSInteger(CS, "ConditionID")
            '        Call SaveWizardValue(RequestNameEmailName, Main.GetCSText(CS, "Name"))
            '        Call SaveWizardValue(RequestNameEmailSubject, Main.GetCSText(CS, "Subject"))
            '        Call SaveWizardValue(RequestNameEmailFromAddress, Main.GetCSText(CS, "FromAddress"))
            '        Call SaveWizardValue(RequestNameEmailTestMemberID, Main.GetCSText(CS, "TestMemberID"))
            '        Call SaveWizardFileValue(RequestNameEmailContent, Main.GetCS(CS, "CopyFilename"))
            '        Call SaveWizardValue(RequestNameEmailConditionID, CStr(ConditionID))
            '        Call SaveWizardValue(RequestNameEmailConditionPeriod, Main.GetCSText(CS, "ConditionPeriod"))
            '        Call SaveWizardValue(RequestNameEmailScheduleStart, Main.GetCSText(CS, "ScheduleDate"))
            '        Call SaveWizardValue(RequestNameEmailScheduleStop, Main.GetCSText(CS, "ConditionExpireDate"))
            '        Call SaveWizardValue(RequestNameEmailLinkAuthentication, Main.GetCSText(CS, "AddLinkEID"))
            '        Call SaveWizardValue(RequestNameEmailSpamFooter, Main.GetCSText(CS, "AllowSpamFooter"))
            '        If Main.SiteProperty_BuildVersion > "3.3.530" Then
            '            Call SaveWizardValue(RequestNameEmailTemplateID, Main.GetCSText(CS, "EmailTemplateID"))
            '        End If
            '        If Main.SiteProperty_BuildVersion > "3.3.531" Then
            '            Call SaveWizardValue(RequestNameImportWizardID, Main.GetCSText(CS, "ImportWizardID"))
            '        End If
            '        '
            '        IDList = ""
            '        CSList = Main.OpenCSContent("Email Groups", "emailid=" & EmailID)
            '        Do While Main.IsCSOK(CSList)
            '            IDList = IDList & "," & Main.GetCSText(CSList, "GroupID")
            '            Main.NextCSRecord (CSList)
            '        Loop
            '        Call Main.CloseCS(CSList)
            '        If IDList <> "" Then
            '            IDList = Mid(IDList, 2)
            '        End If
            '        Call SaveWizardValue(RequestNameEmailGroupIDList, IDList)
            '        '
            '        IDList = ""
            '        CSList = Main.OpenCSContent("Email Topics", "emailid=" & EmailID)
            '        Do While Main.IsCSOK(CSList)
            '            IDList = IDList & "," & Main.GetCSText(CSList, "Topicid")
            '            Main.NextCSRecord (CSList)
            '        Loop
            '        Call Main.CloseCS(CSList)
            '        If IDList <> "" Then
            '            IDList = Mid(IDList, 2)
            '        End If
            '        Call SaveWizardValue(RequestNameEmailTopicIDList, IDList)
            '        '
            '        'Call SaveWizardValue(RequestNameEmailTopicIDList, "")
            '        ContentID = Main.GetCSInteger(CS, "ContentControlID")
            '        If ContentID <> 0 Then
            '            ContentName = CP.Content.GetRecordName(ContentID)
            '            Select Case UCase(ContentName)
            '                Case "GROUP EMAIL"
            '                    SendMethodID = SendMethodIDGroup
            '                Case "SYSTEM EMAIL"
            '                    SendMethodID = SendMethodIDSystem
            '                Case "CONDITIONAL EMAIL"
            '                    Select Case ConditionID
            '                        Case EmailConditionAfterJoin
            '                            SendMethodID = SendMethodIDConditionalAfterJoin
            '                        Case EmailConditionBeforeExpire
            '                            SendMethodID = SendMethodIDConditionalBeforeExpire
            '                        Case EmailConditionBirthday
            '                            SendMethodID = SendMethodIDConditionalBirthday
            '                    End Select
            '            End Select
            '        End If
            '        Call SaveWizardValue(RequestNameEmailSendMethodID, CStr(SendMethodID))
            '    End If
            '    Call Main.CloseCS(CS)
            '
            '    '
            '    Exit Sub
            'ErrorTrap:
            '    Call HandleClassTrapError("LoadAllImportWizardValues")
        End Sub
        '
        '
        '
        Private Sub LoadWizard(ImportWizardID As Integer)
            '    On Error GoTo ErrorTrap
            '    '
            '    Dim CS as Integer
            '    '
            '    CS = Main.OpenCSContentRecord("Import Wizards", ImportWizardID)
            '    If Main.IsCSOK(CS) Then
            '        Wizard.ContentformInstructions = Main.GetCSText(CS, "ContentformInstructions")
            '        Wizard.TemplateFormInstructions = Main.GetCSText(CS, "TemplateFormInstructions")
            '        '
            '        Wizard.GroupFormConditionalAfterJoinInstructions = Main.GetCSText(CS, "GroupFormInstructions")
            '        Wizard.GroupFormConditionalBeforeExpireInstructions = Main.GetCSText(CS, "GroupFormInstructions")
            '        Wizard.GroupFormConditionalBirthdayInstructions = Main.GetCSText(CS, "GroupFormInstructions")
            '        Wizard.GroupFormGroupInstructions = Main.GetCSText(CS, "GroupFormInstructions")
            '        Wizard.GroupFormSystemInstructions = Main.GetCSText(CS, "GroupFormInstructions")
            '        '
            '        Wizard.IncludeAllowSpamFooter = Main.GetCSBoolean(CS, "IncludeAllowSpamFooter")
            '        Wizard.IncludeContentForm = Main.GetCSBoolean(CS, "IncludeContentForm")
            '        Wizard.IncludeSchedule = Main.GetCSBoolean(CS, "IncludeSchedule")
            '        Wizard.IncludeGroupForm = Main.GetCSBoolean(CS, "IncludeGroupForm")
            '        Wizard.IncludeLinkAuthentication = Main.GetCSBoolean(CS, "IncludeLinkAuthentication")
            '        Wizard.IncludeTemplateForm = Main.GetCSBoolean(CS, "IncludeTemplateForm")
            '        '
            '        Wizard.LinkAuthenticationDefault = Main.GetCSBoolean(CS, "LinkAuthenticationDefault")
            '        Wizard.AllowSpamFooterDefault = Main.GetCSBoolean(CS, "AllowSpamFooterDefault")
            '        Wizard.DefaultConditionPeriod = Main.GetCSInteger(CS, "DefaultConditionPeriod")
            '        Wizard.DefaultContent = Main.GetCSText(CS, "DefaultContent")
            '        Wizard.DefaultTemplateID = Main.GetCSInteger(CS, "DefaultTemplateID")
            '        Wizard.SendMethodID = Main.GetCSInteger(CS, "SendMethodID")
            '    End If
            '    Call Main.CloseCS(CS)
            '    '
            '    Exit Sub
            'ErrorTrap:
            '    Call HandleClassTrapError("LoadWizard")
        End Sub
        '
        '
        '
        Private Function GetMemberSelect(cp As CPBaseClass, RequestName As String, MemberID As Integer) As String
            Dim result As String = ""
            Try
                '
                Dim CS As Integer
                Dim cs1 As CPCSBaseClass = cp.CSNew()
                Dim recordId As Integer
                Dim Email As String
                Dim ContentID As Integer
                Dim ContentName As String
                '
                ContentID = CInt(cp.Content.GetFieldProperty("Email", "TestMemberID", "LookupContentID"))
                ContentName = cp.Content.GetRecordName("content", ContentID)
                If ContentName = "" Then
                    ContentName = "Members"
                End If
                '

                ' CS = Main.OpenCSContent(ContentName, , "name", , , , "ID,Name,Email")
                cs1.Open(ContentName)
                If cs1.OK Then
                    GetMemberSelect = GetMemberSelect & "<div>There are no members to select</div>"
                Else
                    GetMemberSelect = GetMemberSelect & "<select size=1 name=" & RequestName & "><option value=0>Select One</Option>"
                    Do While cs1.OK
                        recordId = cs1.GetInteger("ID")
                        Email = cs1.GetText("email")
                        GetMemberSelect = GetMemberSelect & "<option value=" & recordId
                        If recordId = MemberID Then
                            GetMemberSelect = GetMemberSelect & " selected "
                        End If
                        If Email = "" Then
                            GetMemberSelect = GetMemberSelect & ">" & cs1.GetText("name") & " &lt;no email address&gt;</option>"
                        Else
                            GetMemberSelect = GetMemberSelect & ">" & cs1.GetText("name") & " &lt;" & Email & "&gt;</option>"
                        End If

                        cs1.GoNext()

                    Loop
                    GetMemberSelect = GetMemberSelect & "</select>"
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
                & "<option>" & Replace(GetDbFieldList(ContentName, AllowID), ",", "</option><option>") & "</option>" _
                & "</select>"
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        Private Function GetDbFieldList(contentName As String, allowID As Boolean) As String
            Throw New NotImplementedException()
        End Function
        '
        '
        '
        Private Sub LoadSourceFields(cp As CPBaseClass, Filename As String)
            Dim result As String = ""
            Try
                '
                Dim FileData As String
                Dim FileRows() As String
                Dim Ptr As Integer
                Dim FileColumns() As String
                Dim ColumnName As String
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
                            '                If InStr(1, FileData, vbCrLf) <> 0 Then
                            '                    FileRows = Split(FileData, vbCrLf)
                            '                Else
                            '                    FileRows = Split(FileData, vbLf)
                            '                End If
                            '                If UBound(FileRows) > 0 Then
                            '                    If InStr(1, FileRows(0), ",") = 0 Then
                            '                        SourceFieldCnt = 1
                            '                        ReDim Preserve SourceFields(0)
                            '                        SourceFields(0) = Trim(FileRows(0))
                            '                    Else
                            '                        FileColumns = Split(FileRows(0), ",")
                            '                        SourceFieldCnt = UBound(FileColumns) + 1
                            '                        ReDim Preserve SourceFields(SourceFieldCnt - 1)
                            '                        For ptr = 0 To SourceFieldCnt - 1
                            '                            ColumnName = Trim(FileColumns(ptr))
                            '                            If Len(ColumnName) > 2 And Left(ColumnName, 1) = """" And Right(ColumnName, 1) = """" Then
                            '                                ColumnName = Trim(Mid(ColumnName, 2, Len(ColumnName) - 2))
                            '                            End If
                            '                            SourceFields(ptr) = ColumnName
                            '                        Next
                            '                    End If
                            '                End If
                        End If
                    End If
                End If

                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return
        End Sub

        Private Sub parseLine(fileData As String, v As Integer, sourceFields() As String, ignoreLong As Integer, ignoreBoolean As Boolean)
            Throw New NotImplementedException()
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
                    'GetSourceFieldSelect = vbCrLf & "<select name=xxxx>" & vbCrLf & "<option value=""-1"">" & NoneCaption & "</option>"
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
