
Imports System.Linq
Imports System.Text
Imports Contensive.ImportWizard.Models
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
                Using app As New ApplicationModel(CP, False)
                    '
                    ' Process incoming form
                    '
                    Dim viewId As Integer = CP.Doc.GetInteger(rnSrcViewId)
                    Select Case viewId
                        Case viewIdSelectSource
                            '
                            ' Source and ContentName
                            '
                            viewId = SelectSourceView.processView(app, viewId)
                        Case viewIdUpload
                            '
                            ' Upload
                            '
                            viewId = UploadView.processView(app, viewId)
                        Case viewIdSelectFile
                            '
                            ' Select file
                            '
                            viewId = SelectFileView.processView(app, viewId)
                        Case viewIdSelectTable
                            '
                            ' Source and ContentName
                            '
                            viewId = SelectTableView.processView(app, viewId)
                        Case viewIdNewMapping
                            '
                            ' Mapping - Save Values to the file pointed to by RequestNameImportMapFile
                            '
                            viewId = MappingView.processView(app, viewId)
                        Case viewIdSelectKey
                            '
                            ' Select Key Field
                            '
                            Dim importData = ImportDataModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importData)
                            ImportMap.keyMethodID = CP.Doc.GetInteger(RequestNameImportKeyMethodID)
                            ImportMap.sourceKeyField = CP.Doc.GetText(RequestNameImportSourceKeyFieldPtr)
                            ImportMap.dbKeyField = CP.Doc.GetText(RequestNameImportDbKeyField)
                            If Not String.IsNullOrEmpty(ImportMap.dbKeyField) Then
                                Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(CP, "(name=" & CP.Db.EncodeSQLText(ImportMap.dbKeyField) & ")and(contentid=" & CP.Content.GetID(ImportMap.contentName) & ")")
                                If (fieldList.Count > 0) Then
                                    ImportMap.dbKeyFieldType = fieldList.First().type
                                End If
                            End If
                            ImportMap.save(app)

                            Dim Button As String = CP.Doc.GetText(RequestNameButton)
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
                            Dim importData = ImportDataModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importData)
                            If newGroupID <> 0 Then
                                ImportMap.groupID = newGroupID
                            Else
                                ImportMap.groupID = CP.Doc.GetInteger(RequestNameImportGroupID)
                            End If
                            ImportMap.groupOptionID = CP.Doc.GetInteger(RequestNameImportGroupOptionID)
                            ImportMap.save(app)

                            Dim Button As String = CP.Doc.GetText(RequestNameButton)
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
                            Dim Button As String = CP.Doc.GetText(RequestNameButton)
                            Select Case Button
                                Case ButtonBack2
                                    viewId = previousSubFormID(app, viewId)
                                Case ButtonFinish
                                    Dim importData As ImportDataModel = ImportDataModel.create(app)
                                    Dim ImportWizardTasks = DbBaseModel.addDefault(Of ImportWizardTaskModel)(CP)
                                    If (ImportWizardTasks IsNot Nothing) Then
                                        ImportWizardTasks.name = Now() & " CSV Import" 'Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                                        ImportWizardTasks.uploadFilename = importData.privateCsvPathFilename
                                        ImportWizardTasks.notifyEmail = importData.notifyEmail
                                        ImportWizardTasks.importMapFilename = importData.importMapPathFilename
                                        ImportWizardTasks.save(CP)
                                    End If
                                    CP.Addon.ExecuteAsProcess(guidAddonImportTask)
                                    '
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
                    Dim SourceFieldSelect As String
                    Dim Description As String
                    Dim body As String = ""
                    Dim ImportContentName As String
                    Select Case viewId
                        Case viewIdUpload
                            '
                            ' Upload file to Upload folder
                            '
                            body = UploadView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectFile
                            '
                            ' Select a file from the upload folder
                            '
                            body = SelectFileView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectTable
                            '
                            ' Destination
                            '
                            body = SelectTableView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdNewMapping
                            '
                            '
                            '
                            body = MappingView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectKey
                            '
                            ' Select Key
                            '
                            Dim headerCaption As String = "Import Wizard"
                            Dim SourceKeyFieldPtr As Integer
                            Dim DbKeyField As String = ""
                            Dim importData As ImportDataModel = ImportDataModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importData)

                            Dim KeyMethodID As Integer = CP.Utils.EncodeInteger(ImportMap.keyMethodID)
                            If KeyMethodID = 0 Then
                                KeyMethodID = KeyMethodUpdateOnMatchInsertOthers
                            End If
                            '
                            If Not String.IsNullOrEmpty(ImportMap.sourceKeyField) Then
                                SourceKeyFieldPtr = CP.Utils.EncodeInteger(ImportMap.sourceKeyField)
                            Else
                                SourceKeyFieldPtr = -1
                            End If
                            Dim Filename As String = importData.privateCsvPathFilename
                            SourceFieldSelect = Replace(getSourceFieldSelect(app, Filename, "Select One"), "xxxx", RequestNameImportSourceKeyFieldPtr)
                            SourceFieldSelect = Replace(SourceFieldSelect, "value=" & SourceKeyFieldPtr, "value=" & SourceKeyFieldPtr & " selected", , , vbTextCompare)
                            '
                            Dim PeopleContentID As Integer = CP.Content.GetID("people")
                            Dim ImportContentID As Integer = importData.dstContentId
                            ImportContentName = CP.Content.GetRecordName("content", ImportContentID)
                            Dim note As String

                            Dim DBFieldSelect As String
                            '
                            ' Pick any field for key if developer or not the ccMembers table
                            '
                            DbKeyField = ImportMap.dbKeyField
                            Dim LookupContentName As String
                            LookupContentName = CP.Content.GetRecordName("content", importData.dstContentId)
                            DBFieldSelect = Replace(getDbFieldSelect(CP, LookupContentName, "Select One", True), "xxxx", RequestNameImportDbKeyField)
                            DBFieldSelect = Replace(DBFieldSelect, ">" & DbKeyField & "<", " selected>" & DbKeyField & "<", , , vbTextCompare)
                            note = ""
                            '
                            Description = CP.Html.h4("Update Control") & CP.Html.p("When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted.")
                            Dim Content As String = "" _
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
                            Content &= CP.Html.Hidden(rnSrcViewId, viewId.ToString)
                            body = HtmlController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Return CP.Html.Form(body)
                        Case viewIdSelectGroup
                            '
                            ' Select a group to add
                            '

                            Dim headerCaption As String = "Import Wizard"

                            Dim importData As ImportDataModel = ImportDataModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importData)


                            Dim GroupOptionID = ImportMap.groupOptionID
                            If GroupOptionID = 0 Then
                                GroupOptionID = GroupOptionNone
                            End If
                            Description = CP.Html.h4("Group Membership") & CP.Html.p("When your data is imported, people can be added to a group automatically. Select the option below, and a group.")
                            Dim Content As String = "" _
                                    & "<div>" _
                                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                                    & "<TR><TD colspan=2>Add to Existing Group</td></tr>" _
                                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & CP.Html.SelectContent(RequestNameImportGroupID, ImportMap.groupID.ToString, "Groups", "", "", "form-control") & "</td></tr>" _
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
                            Content &= CP.Html.Hidden(rnSrcViewId, viewId.ToString)
                            body = HtmlController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
                            Return CP.Html.Form(body)
                        Case viewIdFinish
                            '
                            ' Ask for an email address to notify when the list is complete
                            '
                            Dim headerCaption As String = "Import Wizard"
                            Description = CP.Html.h4("Finish") & CP.Html.p("Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete.")
                            Dim Content As String = "<div Class=""p-2""><label for=""name381"">Email</label><div class=""ml-5"">" & CP.Html5.InputText(RequestNameImportEmail, 255, CP.User.Email) & "</div><div class=""ml-5""><small class=""form-text text-muted""></small></div></div>"
                            Content &= CP.Html.Hidden(rnSrcViewId, viewId.ToString)
                            body = HtmlController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Return CP.Html.Form(body)
                        Case viewIdDone
                            '
                            ' Thank you
                            '
                            Dim headerCaption As String = "Import Wizard"
                            Description = CP.Html.h4("Import Requested") & CP.Html.p("Your import is underway and should only take a moment.")
                            Dim Content As String = CP.Html.Hidden(rnSrcViewId, viewId.ToString)
                            body = HtmlController.getWizardContent(CP, headerCaption, ButtonCancel, ButtonBack2, ButtonFinish, Description, Content)
                            Return CP.Html.Form(body)
                        Case viewIdReturnBlank
                            '
                            ' -- 
                            Return ""
                        Case viewIdSelectSource, 0
                            '
                            ' -- data source
                            body = SelectSourceView.getView(app)
                            Return CP.Html.Form(body)
                    End Select
                End Using
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
        Private Function nextSubFormID(app As ApplicationModel, SubformID As Integer) As Integer
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
        Private Function previousSubFormID(app As ApplicationModel, SubformID As Integer) As Integer
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

    End Class
End Namespace
