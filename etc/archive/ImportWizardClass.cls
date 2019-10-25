VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ImportWizardClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
'========================================================================
'
'========================================================================
'
' ----- global scope variables
'
Private Main As ccWeb3.MainClass
'
'
'
Const ButtonContinue2 = " Continue "
Const ButtonBack2 = " Back "
'
'
'
Private Wizard As WizardType
'
Private DefaultImportMapFile As String
'
' ----- Forms - these must be in the order that they are processed
'               GetnextForm() uses the Email Wizard row to skip the ones that do not apply
'               Except:
'                   1 is the first form
'                   2 or 3 always follow 1
'                   2 picks and email, and the email determines the wizard row to start
'                   3 picks the wizard row to start
'
'
' Source Fields
'
Dim SourceFieldCnt As Long
Dim SourceFields() As String
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
'=================================================================================
'   Aggregate Object Interface
'=================================================================================
'
Public Function GetForm(MainPointer As ccWeb3.MainClass) As String
    On Error GoTo ErrorTrap
    '
    'Dim FileView As FileViewClass
Dim ImportMap As ImportMapType
    Dim MapPtr As Long
    Dim UcaseDbFieldName As String
   ' Dim SourceFieldName As String
    Dim KeyMethodID As Long
    Dim SourceFieldSelect As String
    Dim DBFieldSelect As String
    Dim RowStart As String
    Dim RowStop As String
    Dim ValueText As String
    Dim Content As String
    Dim ContentID As Long
    Dim Description As String
    Dim WizardContent As String
    Dim SubformID As Long
    Dim HeaderCaption As String
    Dim ModifyType As Integer
    Dim Button As String
    Dim EmailID As Long
    Dim EMailName As String
    Dim CS As Long
    Dim ImportWizardID As Long
    Dim EMailTemplateID As Long
    Dim EmailConditionID As Long
    Dim EmailConditionPeriod As Long
    Dim SendMethodID As Long
    Dim ConditionID As Long
    Dim ProcessError As Boolean
    Dim TestMemberID As Long
    Dim EmailAddress As String
    Dim MemberName As String
    Dim HeadingContentID As Long
    Dim HeadingContentName As String
    Dim RecordName As String
    Dim RecordID As Long
    Dim IDList As String
    Dim CSList As Long
    Dim ButtonList As String
    Dim EmailScheduleStart As Date
    Dim EmailFinishID As Long
    Dim Cnt As Long
    Dim Ptr As Long
    Dim ImportSource As Long
    Dim ImportContentID As Long
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
    Dim SourceFieldPtr As Long
    Dim ImportMapFile As String
    Dim ImportMapData As String
    Dim FieldCnt As Long
    Dim DBFieldCaption As String
    '
    'Const ContentName = "People"
    Dim PeopleContentID As Long
    '
    Set Main = MainPointer
    '
    PeopleContentID = Main.GetContentID("people")
    ProcessError = False
    Content = ""
    Button = Main.GetStreamText(RequestNameButton)
    If Button = ButtonCancel Then
        '
        ' Cancel
        '
        Call ClearWizardValues
        Call Main.Redirect(Main.ServerAppRootPath & Main.ServerAppPath)
    Else
        SubformID = Main.GetStreamInteger(RequestNameSubForm)
        '
        If SubformID = 0 Then
            '
            ' Set defaults and go to first form
            '
            Call ClearWizardValues
            ImportContentID = Main.GetStreamInteger("cid")
            If ImportContentID <> 0 Then
                Call SaveWizardValue(RequestNameImportContentID, CStr(ImportContentID))
            End If
            SubformID = SubFormSource
            Call LoadWizardPath
        Else
            '
            ' Load the importmap with what we have so far
            '
            ImportMapFile = GetWizardValue(RequestNameImportMapFile, GetDefaultImportMapFile)
            ImportMapData = Main.ReadVirtualFile(ImportMapFile)
            ImportMap = LoadImportMap(ImportMapData)
            '
            ' Process incoming form
            '
            Select Case SubformID
                Case SubFormSource
                    '
                    ' Source and ContentName
                    '
                    'Call SaveWizardValue(RequestNameImportContentName, ContentName)
                    Call SaveWizardStreamInteger(RequestNameImportSource)
                Case SubFormSourceUpload
                    '
                    ' Upload
                    '
                    Filename = Main.ProcessFormInputFile(RequestNameImportUpload)
                    Call SaveWizardValue(RequestNameImportUpload, Filename)
                Case SubFormSourceUploadFolder
                    '
                    '
                    '
                    Filename = Main.GetStreamText("SelectFile")
                    Call SaveWizardValue(RequestNameImportUpload, Filename)
                Case SubFormSourceResourceLibrary
                    '
                    '
                    '
                    ProcessError = True
                    Call Main.AddUserError("Under Construction")
                Case SubFormDestination
                    '
                    ' Source and ContentName
                    '
                    Call SaveWizardStreamInteger(RequestNameImportContentID)
                Case SubFormNewMapping
                    '
                    ' Mapping - Save Values to the file pointed to by RequestNameImportMapFile
                    '
                    If Main.GetStreamBoolean(RequestNameImportSkipFirstRow) Then
                        ImportMap.SkipRowCnt = 1
                    Else
                        ImportMap.SkipRowCnt = 0
                    End If
                    FieldCnt = Main.GetStreamInteger("ccnt")
                    ImportMap.MapPairCnt = FieldCnt
                    If FieldCnt > 0 Then
                    ReDim ImportMap.MapPairs(FieldCnt - 1)
Dim DbField As String
                    If FieldCnt > 0 Then
                        For Ptr = 0 To FieldCnt - 1
                            SourceFieldPtr = Main.GetStreamInteger("SOURCEFIELD" & Ptr)
                            ImportMap.MapPairs(Ptr).SourceFieldPtr = SourceFieldPtr
                            DbField = Main.GetStreamText("DBFIELD" & Ptr)
                            ImportMap.MapPairs(Ptr).DbField = DbField
                            ImportMap.MapPairs(Ptr).DbFieldType = kmaEncodeInteger(Main.GetContentFieldProperty(ImportMap.ContentName, DbField, "Fieldtype"))
                        Next
                    End If
                    End If
                    Call SaveImportMap(ImportMap)
                Case SubFormKey
                    '
                    ' Select Key Field
                    '
                    ImportMap.KeyMethodID = Main.GetStreamInteger(RequestNameImportKeyMethodID)
                    ImportMap.SourceKeyField = Main.GetStreamText(RequestNameImportSourceKeyFieldPtr)
                    ImportMap.DbKeyField = Main.GetStreamText(RequestNameImportDbKeyField)
                    If ImportMap.DbKeyField <> "" Then
                        ImportMap.DbKeyFieldType = Main.GetContentFieldProperty(ImportMap.ContentName, ImportMap.DbKeyField, "FieldType")
                    End If
                    Call SaveImportMap(ImportMap)
                Case SubFormGroup
                    '
                    ' Add to group
                    '
                    ImportMap.GroupID = Main.GetStreamInteger(RequestNameImportGroupID)
                    ImportMap.GroupOptionID = Main.GetStreamInteger(RequestNameImportGroupOptionID)
                    Call SaveImportMap(ImportMap)
                Case SubFormFinish
                    '
                    ' Get email address for email
                    '
                    Call SaveWizardStream(RequestNameImportEmail)
                    If Button = ButtonFinish Then
                        CS = Main.InsertCSRecord("Tasks")
                        If Main.IsCSOK(CS) Then
                            Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                            Call Main.SetCS(CS, "Filename", GetWizardValue(RequestNameImportUpload, ""))
                            Call Main.SetCS(CS, "NotifyEmail", GetWizardValue(RequestNameImportEmail, ""))
                            Call Main.SetCS(CS, "ImportMapFilename", GetWizardValue(RequestNameImportMapFile, GetDefaultImportMapFile))
                            Call Main.SetCS(CS, "COMMAND", "IMPORTCSV")
                        End If
                        Call Main.CloseCS(CS)
                    End If
            End Select
            '
            ' Handle back and continue
            '
            If Not ProcessError Then
                If Button = ButtonFinish Then
                    '
                    ' Finished - exit here
                    '
                    Call ClearWizardValues
                    Call Main.Redirect(Main.ServerAppRootPath & Main.ServerAppPath)
                Else
                    '
                    ' Determine next or previous form
                    '
                    Call LoadWizardPath
                    Select Case Button
                        Case ButtonBack2
                            SubformID = PreviousSubFormID(SubformID)
                        Case ButtonContinue2
                            SubformID = NextSubFormID(SubformID)
                    End Select
                End If
            End If
        End If
        '
        ' Get Next Form
        '
        HeaderCaption = "Import Wizard"
        Content = Content & Main.GetFormInputHidden(RequestNameSubForm, SubformID)
        If Main.IsUserError Then
            Content = Content & Main.GetUserError()
        End If
        Select Case SubformID
            Case SubFormSource, 0
                '
                ' Source
                '
                ImportSource = kmaEncodeInteger(GetWizardValue(RequestNameImportSource, kmaEncodeText(ImportSourceUpload)))
                Description = "<B>Select the import source</B><BR><BR>There are several sources you can use for your data..."
                Content = Content _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<TR><TD width=1>" & Main.GetFormInputRadioBox(RequestNameImportSource, ImportSourceUpload, ImportSource) & "</td><td width=99% align=left>" & "Upload a comma delimited text file (up to 5 MBytes).</td></tr>" _
                    & "<TR><TD width=1>" & Main.GetFormInputRadioBox(RequestNameImportSource, ImportSourceUploadFolder, ImportSource) & "</td><td width=99% align=left>" & "Use a file already uploaded into your Upload Folder.</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel, ButtonContinue2, True, True, Description, Content)
            Case SubFormSourceUpload
                '
                ' Upload file to Upload folder
                '
                
                Description = "<B>Upload your File</B><BR><BR>Hit browse to locate the file you want to upload..."
                Content = Content _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" & Main.GetFormInputFile(RequestNameImportUpload) & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
'WizardContent = "hello world"
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
'GetForm = "Hello World"
'Exit Function
            Case SubFormSourceUploadFolder
                '
                ' Select a file from the upload folder
                '
                Description = "<B>Select a file from your Upload folder</B><BR><BR>Select the upload file you wish to import..."
                Call Main.AddRefreshQueryString("af", AdminFormImportWizard)
                Call Main.AddRefreshQueryString(RequestNameSubForm, SubFormSourceUploadFolder)
                'Set FileView = New FileViewClass
Dim OptionString As String
Const FileViewGuid = "{B966103C-DBF4-4655-856A-3D204DEF6B21}"
    OptionString = "" _
        & "AdminLayout=0" _
        & "&FileSystem=content files" _
        & "&BaseFolder=\upload\" _
        & "&AllowEdit=0" _
        & "&AllowNavigation=0" _
        & ""
Content = Content _
    & "<div>" _
    & Main.ExecuteAddon2(FileViewGuid, OptionString) _
    & "</div>"
'Content = Content & "<div>" & FileView.GetContentFileView2(Main, "upload", False, False, False, True, False, False) & "</div>"
                'Content = Content & "<div>" & FileView.GetContentFileView(Main, "upload", False, False, False, True, False) & "</div>"
                'Set FileView = Nothing
                Call Main.AddRefreshQueryString(RequestNameSubForm, "")
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
            Case SubFormDestination
                '
                ' Destination
                '
                ImportContentID = kmaEncodeInteger(GetWizardValue(RequestNameImportContentID, kmaEncodeText(PeopleContentID)))
                Description = "<B>Select the destination for your data</B><BR><BR>"
                Content = Content _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<TR><TD colspan=""2"">" & Main.GetFormInputSelect(RequestNameImportContentID, ImportContentID, "Content") & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
            Case SubFormNewMapping
                '
                ' Get Mapping fields
                '
                FileData = ""
                Description = "<B>Create a New Mapping</B><BR><BR>This step lets you select which fields in your database you would like each field in your upload to be assigned."
                Filename = GetWizardValue(RequestNameImportUpload, "")
                If Filename <> "" Then
                    FileData = Main.ReadVirtualFile(Filename)
                End If
                If FileData = "" Then
                    '
                    ' no data in upload
                    '
                    Content = Content & "<P>The file you are importing is empty. Please go back and select a different file.</P>"
                    WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, "", True, True, Description, Content)
                Else
                    '
                    ' Skip first Row checkbox
                    '
                    Content = Content & Main.GetFormInputCheckBox(RequestNameImportSkipFirstRow, (ImportMap.SkipRowCnt <> 0)) & "&nbsp;First row contains field names"
                    Content = Content & "<div>&nbsp;</div>"
                    '
                    ' Build FileColumns
                    '
                    DefaultSourceFieldSelect = GetSourceFieldSelect(Filename, "none")
                    '
                    ' Build the Database field list
                    '
                    ImportContentID = GetWizardValue(RequestNameImportContentID, CStr(PeopleContentID))
                    ImportContentName = Main.GetContentNameByID(ImportContentID)
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
                    ImportMapFile = GetWizardValue(RequestNameImportMapFile, GetDefaultImportMapFile)
                    ImportMapData = Main.ReadVirtualFile(ImportMapFile)
                    ImportMap = LoadImportMap(ImportMapData)
                    For Ptr = 0 To UBound(DBFields)
                        DBFieldName = DBFields(Ptr)
                        Select Case kmaEncodeInteger(Main.GetContentFieldProperty(ImportContentName, DBFieldName, "FieldType"))
                            Case FieldTypeBoolean
                                DbFieldType = "true/false"
                            Case FieldTypeCurrency, FieldTypeFloat
                                DbFieldType = "Number"
                            Case FieldTypeDate
                                DbFieldType = "Date"
                            Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile
                                DbFieldType = "Filename"
                            Case FieldTypeInteger
                                DbFieldType = "Integer"
                            Case FieldTypeLongText
                                DbFieldType = "Text (8000 chr)"
                            Case FieldTypeLookup
                                DbFieldType = "Integer ID"
                            Case FieldTypeManyToMany
                                DbFieldType = "Integer ID"
                            Case FieldTypeMemberSelect
                                DbFieldType = "Integer ID"
                            Case FieldTypeText
                                DbFieldType = "Text (255 chr)"
                            Case Else
                                DbFieldType = "Invalid"
                        End Select
                        SourceFieldSelect = DefaultSourceFieldSelect
                        SourceFieldSelect = Replace(SourceFieldSelect, "xxxx", "SourceField" & Ptr)
                        SourceFieldPtr = 0
                        If DBFieldName <> "" Then
                            '
                            ' Find match in current ImportMap
                            '
                            UcaseDbFieldName = UCase(DBFieldName)
                            With ImportMap
                                If .MapPairCnt > 0 Then
                                    For MapPtr = 0 To .MapPairCnt - 1
                                        If UcaseDbFieldName = UCase(.MapPairs(MapPtr).DbField) Then
                                            SourceFieldPtr = .MapPairs(MapPtr).SourceFieldPtr
                                            Exit For
                                        End If
                                    Next
                                End If
                            End With
                            If SourceFieldPtr = 0 Then
                                '
                                ' Find a default field - one that matches the DBFieldName if possible
                                '
Dim TestName As String
                                If SourceFieldCnt > 0 Then
                                For SourceFieldPtr = 0 To SourceFieldCnt - 1
                                    TestName = UCase(SourceFields(SourceFieldPtr))
                                    '
                                    ' check for exact match
                                    '
                                    If UcaseDbFieldName = TestName Then
                                        Exit For
                                    End If
                                    '
                                    ' check for pseudo match
                                    '
                                    Select Case UcaseDbFieldName
                                        Case "FIRSTNAME"
                                            If TestName = "FIRST" Then
                                                Exit For
                                            End If
                                        Case "LASTNAME"
                                            If TestName = "LAST" Then
                                                Exit For
                                            End If
                                        Case "EMAIL"
                                            If TestName = "E-MAIL" Then
                                                Exit For
                                            End If
                                        Case "ADDRESS"
                                            If TestName = "ADDRESS1" Then
                                                Exit For
                                            ElseIf TestName = "ADDRESS 1" Then
                                                Exit For
                                            End If
                                        Case "ADDRESS2"
                                            If TestName = "ADDRESS 2" Then
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
                        If Not Main.IsDeveloper Then
                            Select Case UCase(DBFieldCaption)
                                Case "ID"
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
                    WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
                End If
            Case SubFormKey
                '
                ' Select Key
                '
Dim SourceKeyFieldPtr As Long
Dim DbKeyField As String
                KeyMethodID = kmaEncodeInteger(ImportMap.KeyMethodID)
                If KeyMethodID = 0 Then
                    KeyMethodID = KeyMethodUpdateOnMatchInsertOthers
                End If
                '
                If ImportMap.SourceKeyField <> "" Then
                    SourceKeyFieldPtr = kmaEncodeInteger(ImportMap.SourceKeyField)
                Else
                    SourceKeyFieldPtr = -1
                End If
                Filename = GetWizardValue(RequestNameImportUpload, "")
                SourceFieldSelect = Replace(GetSourceFieldSelect(Filename, "Select One"), "xxxx", RequestNameImportSourceKeyFieldPtr)
                SourceFieldSelect = Replace(SourceFieldSelect, "value=" & SourceKeyFieldPtr, "value=" & SourceKeyFieldPtr & " selected", , , vbTextCompare)
                '
                ImportContentID = kmaEncodeInteger(GetWizardValue(RequestNameImportContentID, CStr(PeopleContentID)))
                ImportContentName = Main.GetContentNameByID(ImportContentID)
                If (Not Main.IsWithinContent(ImportContentName, "People")) Or Main.IsDeveloper Then
                    '
                    ' Pick any field for key if developer or not the ccMembers table
                    '
                    DbKeyField = ImportMap.DbKeyField
Dim LookupContentName As String
                    LookupContentName = Main.GetContentNameByID(kmaEncodeInteger(GetWizardValue(RequestNameImportContentID, CStr(PeopleContentID))))
                    DBFieldSelect = Replace(GetDbFieldSelect(LookupContentName, "Select One", True), "xxxx", RequestNameImportDbKeyField)
                    DBFieldSelect = Replace(DBFieldSelect, ">" & DbKeyField & "<", " selected>" & DbKeyField & "<", , , vbTextCompare)
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
                End If
                '
                Description = "<B>Update Control</B><BR><BR>When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted."
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
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportKeyMethodID, KeyMethodInsertAll, KeyMethodID) & "</td><td width=99% align=left>" & "Insert all imported data, regardless of key field.</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatchInsertOthers, KeyMethodID) & "</td><td width=99% align=left>" & "Update database records when the data in the key fields match. Insert all other imported data.</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatch, KeyMethodID) & "</td><td width=99% align=left>" & "Update database records when the data in the key fields match. Ignore all other imported data.</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
Dim GroupID As Long
Dim GroupOptionID As Long

            Case SubFormGroup
                '
                ' Select a group to add
                '
                GroupOptionID = ImportMap.GroupOptionID
                If GroupOptionID = 0 Then
                    GroupOptionID = GroupOptionNone
                End If
                Description = "<B>Group Membership</B><BR><BR>When your data is imported, people can be added to a group automatically. Select the option below, and a group."
                Content = Content _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                    & "<TR><TD colspan=2>Destination Group</td></tr>" _
                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & Main.GetFormInputSelect(RequestNameImportGroupID, ImportMap.GroupID, "Groups") & "</td></tr>" _
                    & "<TR><TD colspan=2>Group Options</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportGroupOptionID, GroupOptionNone, GroupOptionID) & "</td><td width=99% align=left>Do not add to a group.</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportGroupOptionID, GroupOptionAll, GroupOptionID) & "</td><td width=99% align=left>Add everyone to the selected group.</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportGroupOptionID, GroupOptionOnMatch, GroupOptionID) & "</td><td width=99% align=left>Add to the selected group if keys match.</td></tr>" _
                    & "<TR><TD width=10>" & Main.GetFormInputRadioBox(RequestNameImportGroupOptionID, GroupOptionOnNoMatch, GroupOptionID) & "</td><td width=99% align=left>Add to the selected group if keys do NOT match.</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonContinue2, True, True, Description, Content)
            Case SubFormFinish
                '
                ' Ask for an email address to notify when the list is complete
                '
                Description = "<B>Finish</B><BR><BR>Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete."
                Content = Content _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & Main.GetFormInputText(RequestNameImportEmail, GetWizardValue(RequestNameImportEmail, Main.MemberEmail)) & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                WizardContent = Main.GetWizardContent(HeaderCaption, ButtonCancel & "," & ButtonBack2, ButtonFinish, True, True, Description, Content)
            Case Else
        End Select
        '
'GetForm = WizardContent
        GetForm = Main.GetAdminFormBody("", "", "", True, True, "", "", 20, WizardContent)
        'GetForm = Replace(GetForm, "<form ", "<xform ", , , vbTextCompare)
        'GetForm = Replace(GetForm, "</form", "</xform", , , vbTextCompare)
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleClassTrapError("GetForm")
End Function
'
'
'
Private Sub Class_Initialize()
End Sub
'
'
'
Private Sub Class_Terminate()
    Set Main = Nothing
End Sub
'
'
'
Private Sub HandleClassTrapError(MethodName As String, Optional ignore0 As String)
    Set Main = Nothing
    Call HandleError("ImportWizardClass", MethodName, Err.Number, Err.Source, Err.Description, True, False)
End Sub
'
'
'
Private Sub HandleForumInternalError(MethodName As String, ErrDescription As String)
    Call Err.Raise(KmaErrorInternal, App.EXEName, ErrDescription)
End Sub
'
'
'
Private Sub ClearWizardValues()
    Call Main.ExecuteSQL("default", "delete from ccProperties where name like 'ImportWizard.%' and typeid=1 and keyid=" & Main.VisitID)
End Sub
'
'
'
Private Function NextSubFormID(SubformID As Long) As Long
    Dim Ptr As Long
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
Private Function PreviousSubFormID(SubformID As Long) As Long
    Dim Ptr As Long
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
Private Sub LoadWizardPath()
    On Error GoTo ErrorTrap
    '
    Dim CS As Long
    Dim EmailID As Long
    Dim CSEmail As Long
    Dim CSWizard As Long
    Dim ImportWizardID As Long
    Dim ValueString As String
    Dim EmailCID As Long
    '
    ' Get the saved ImportWizardID
    '
    ImportWizardID = kmaEncodeInteger(GetWizardValue(RequestNameImportWizardID, ""))
    '
    If ImportWizardID = 0 Then
        '
        ' Default Wizard, for any type of email, nothing disabled
        '
        EmailCID = Main.GetContentID("Email Templates")
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
        
        Select Case kmaEncodeInteger(GetWizardValue(RequestNameImportSource, ImportSourceUpload))
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
        If kmaEncodeInteger(GetWizardValue(RequestNameImportContentID, "0")) = Main.GetContentID("people") Then
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
    Exit Sub
ErrorTrap:
    Call HandleClassTrapError("LoadWizardPath")
End Sub
'
'
'
Private Sub SaveWizardValue(Name As String, Value As String)
    Call Main.SetVisitProperty("ImportWizard." & Name, Value)
End Sub
'
'
'
Private Function GetWizardValue(Name As String, DefaultValue As String) As String
    GetWizardValue = Main.GetVisitProperty("ImportWizard." & Name, DefaultValue)
End Function
'
'
'
Private Sub SaveWizardStreamInteger(RequestName As String)
    Call SaveWizardValue(RequestName, Main.GetStreamInteger(RequestName))
End Sub
'
'
'
Private Sub SaveWizardStream(RequestName As String)
    Call SaveWizardValue(RequestName, Main.GetStreamText(RequestName))
End Sub
'
'
'
Private Sub SaveWizardFileValue(Name As String, Value As String)
    Dim Filename As String
    '
    Filename = GetWizardValue(Name, "Temp/ImportWizard_Visit" & Main.VisitID & ".txt")
    If Filename = "" Then
        Filename = "Temp/ImportWizard_Visit" & Main.VisitID & ".txt"
        Call SaveWizardValue(Name, Filename)
    End If
    Call Main.SaveVirtualFile(Filename, Value)
End Sub
'
'
'
Private Sub SaveWizardFileStreamAC(RequestName As String)
    Dim Filename As String
    '
    Filename = GetWizardValue(RequestName, "Temp/ImportWizard_Visit" & Main.VisitID & ".txt")
    If Filename = "" Then
        Filename = "Temp/ImportWizard_Visit" & Main.VisitID & ".txt"
        Call SaveWizardValue(RequestName, Filename)
    End If
    Call Main.SaveVirtualFile(Filename, Main.GetStreamActiveContent(RequestName))
End Sub
'
'
'
Private Function GetWizardFileValue(Name As String, DefaultValue As String) As String
    Dim Filename As String
    '
    Filename = GetWizardValue(Name, "Temp/ImportWizard_Visit" & Main.VisitID & ".txt")
    If Filename <> "" Then
        GetWizardFileValue = Main.ReadVirtualFile(Filename)
    End If
    If GetWizardFileValue = "" Then
        GetWizardFileValue = DefaultValue
        Call SaveWizardFileValue(Name, GetWizardFileValue)
    End If
End Function
'
'
'
Private Sub LoadAllImportWizardValues(EmailID As Long)
'    On Error GoTo ErrorTrap
'    '
'    Dim ImportWizardID As Long
'    Dim CS As Long
'    Dim IDList As String
'    Dim ConditionID As Long
'    Dim CSList As Long
'    Dim ContentID As Long
'    Dim ContentName As String
'    Dim SendMethodID As Long
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
'            ContentName = Main.GetContentNameByID(ContentID)
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
Private Sub LoadWizard(ImportWizardID As Long)
'    On Error GoTo ErrorTrap
'    '
'    Dim CS As Long
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
Private Function GetMemberSelect(RequestName As String, MemberID As Long) As String
    On Error GoTo ErrorTrap
    '
    Dim CS As Long
    Dim RecordID As Long
    Dim Email As String
    Dim ContentID As Long
    Dim ContentName As String
    '
    ContentID = Main.GetContentFieldProperty("Email", "TestMemberID", "LookupContentID")
    ContentName = Main.GetContentNameByID(ContentID)
    If ContentName = "" Then
        ContentName = "Members"
    End If
    '
    CS = Main.OpenCSContent(ContentName, , "name", , , , "ID,Name,Email")
    If Not Main.IsCSOK(CS) Then
        GetMemberSelect = GetMemberSelect & "<div>There are no members to select</div>"
    Else
        GetMemberSelect = GetMemberSelect & "<select size=1 name=" & RequestName & "><option value=0>Select One</Option>"
        Do While Main.IsCSOK(CS)
            RecordID = Main.GetCSInteger(CS, "ID")
            Email = Main.GetCSText(CS, "email")
            GetMemberSelect = GetMemberSelect & "<option value=" & RecordID
            If RecordID = MemberID Then
                GetMemberSelect = GetMemberSelect & " selected "
            End If
            If Email = "" Then
                GetMemberSelect = GetMemberSelect & ">" & Main.GetCSText(CS, "name") & " &lt;no email address&gt;</option>"
            Else
                GetMemberSelect = GetMemberSelect & ">" & Main.GetCSText(CS, "name") & " &lt;" & Email & "&gt;</option>"
            End If
            Call Main.NextCSRecord(CS)
        Loop
        GetMemberSelect = GetMemberSelect & "</select>"
    End If
    Call Main.CloseCS(CS)

    '
    Exit Function
ErrorTrap:
    Call HandleClassTrapError("GetMemberSelect")
End Function
'
'
'
Private Function GetDbFieldList(ContentName As String, AllowID As Boolean) As String
    On Error GoTo ErrorTrap
    '
    GetDbFieldList = "," & Main.GetContentProperty(ContentName, "SELECTFIELDLIST") & ","
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
    Exit Function
ErrorTrap:
    Call HandleClassTrapError("GetDbFieldList")
End Function
'
'
'
Private Function GetDbFieldSelect(ContentName As String, NoneCaption As String, AllowID As Boolean) As String
    On Error GoTo ErrorTrap
    '
    GetDbFieldSelect = "" _
        & "<select name=xxxx><option value="""" style=""Background-color:#E0E0E0;"">" & NoneCaption & "</option>" _
        & "<option>" & Replace(GetDbFieldList(ContentName, AllowID), ",", "</option><option>") & "</option>" _
        & "</select>"
    '
    Exit Function
ErrorTrap:
    Call HandleClassTrapError("GetDbFieldSelect")
End Function
'
'
'
Private Sub LoadSourceFields(Filename As String)
    On Error GoTo ErrorTrap
    '
    Dim FileData As String
    Dim FileRows() As String
    Dim Ptr As Long
    Dim FileColumns() As String
    Dim ColumnName As String
    '
    If Filename <> "" Then
        If SourceFieldCnt = 0 Then
            FileData = Main.ReadVirtualFile(Filename)
            If FileData <> "" Then
                '
                ' Build FileColumns
                '
                If InStr(1, FileData, vbCrLf) <> 0 Then
                    FileRows = Split(FileData, vbCrLf)
                Else
                    FileRows = Split(FileData, vbLf)
                End If
                If UBound(FileRows) > 0 Then
                    FileColumns = Split(FileRows(0), ",")
                    SourceFieldCnt = UBound(FileColumns) + 1
                    ReDim Preserve SourceFields(SourceFieldCnt - 1)
                    For Ptr = 0 To SourceFieldCnt - 1
                        ColumnName = Trim(FileColumns(Ptr))
                        If Len(ColumnName) > 2 And Left(ColumnName, 1) = """" And Right(ColumnName, 1) = """" Then
                            ColumnName = Trim(Mid(ColumnName, 2, Len(ColumnName) - 2))
                        End If
                        SourceFields(Ptr) = ColumnName
                    Next
                End If
            End If
        End If
    End If
    '
    Exit Sub
ErrorTrap:
    Call HandleClassTrapError("LoadSourceFields")
End Sub
'
'
'
Private Function GetSourceFieldSelect(Filename As String, NoneCaption As String) As String
    On Error GoTo ErrorTrap
    '
    Dim FileData As String
    Dim FileRows() As String
    Dim Ptr As Long
    Dim FileColumns() As String
    Dim ColumnName As String
    '
    If Filename <> "" Then
        Call LoadSourceFields(Filename)
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
    Exit Function
ErrorTrap:
    Call HandleClassTrapError("GetSourceFieldSelect")
End Function
'
'
'
Private Sub SaveImportMap(ImportMap As ImportMapType)
    On Error GoTo ErrorTrap
    '
    Dim ImportMapFile As String
    Dim ImportMapData As String
    Dim Rows() As String
    Dim Pair() As String
    Dim Ptr As Long
    '
    ImportMapFile = GetWizardValue(RequestNameImportMapFile, GetDefaultImportMapFile)
    ImportMapData = "" _
        & ImportMap.KeyMethodID _
        & vbCrLf & ImportMap.SourceKeyField _
        & vbCrLf & ImportMap.DbKeyField _
        & vbCrLf & ImportMap.ContentName _
        & vbCrLf & ImportMap.GroupOptionID _
        & vbCrLf & ImportMap.GroupID _
        & vbCrLf & ImportMap.SkipRowCnt _
        & vbCrLf & ImportMap.DbKeyFieldType _
        & vbCrLf
    If ImportMap.MapPairCnt > 0 Then
        For Ptr = 0 To ImportMap.MapPairCnt - 1
            ImportMapData = ImportMapData & vbCrLf & ImportMap.MapPairs(Ptr).DbField & "=" & ImportMap.MapPairs(Ptr).SourceFieldPtr & "," & ImportMap.MapPairs(Ptr).DbFieldType
        Next
    End If
    Call Main.SaveVirtualFile(ImportMapFile, ImportMapData)
    Exit Sub
ErrorTrap:
    Call HandleClassTrapError("SaveImportMap")
End Sub

'
'
'
Private Function GetDefaultImportMapFile() As String
    If DefaultImportMapFile = "" Then
        DefaultImportMapFile = "ImportWizard\ImportMap" & GetRandomInteger & ".txt"
    End If
    GetDefaultImportMapFile = DefaultImportMapFile
    
End Function


