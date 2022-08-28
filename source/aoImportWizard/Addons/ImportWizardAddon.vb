
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
                            viewId = SelectKeyView.processView(app, viewId)
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
                            Dim importConfig = ImportConfigModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importConfig)
                            If newGroupID <> 0 Then
                                ImportMap.groupID = newGroupID
                            Else
                                ImportMap.groupID = CP.Doc.GetInteger(RequestNameImportGroupID)
                            End If
                            ImportMap.groupOptionID = CP.Doc.GetInteger(RequestNameImportGroupOptionID)
                            ImportMap.save(app, importConfig)

                            Dim Button As String = CP.Doc.GetText(RequestNameButton)
                            Select Case Button
                                Case ButtonBack2
                                    '
                                    ' -- back to select key
                                    viewId = viewIdSelectKey
                                Case ButtonContinue2
                                    '
                                    ' -- continue to finish
                                    viewId = viewIdFinish
                            End Select
                        Case viewIdFinish
                            '
                            ' Determine next or previous form
                            '
                            Dim Button As String = CP.Doc.GetText(RequestNameButton)
                            Select Case Button
                                Case ButtonBack2
                                    '
                                    ' -- back to select key
                                    viewId = viewIdSelectKey
                                Case ButtonFinish
                                    Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                                    Dim ImportWizardTasks = DbBaseModel.addDefault(Of ImportWizardTaskModel)(CP)
                                    If (ImportWizardTasks IsNot Nothing) Then
                                        ImportWizardTasks.name = Now() & " CSV Import" 'Call Main.SetCS(CS, "Name", Now() & " CSV Import")
                                        ImportWizardTasks.uploadFilename = importConfig.privateUploadPathFilename
                                        ImportWizardTasks.notifyEmail = importConfig.notifyEmail
                                        ImportWizardTasks.importMapFilename = importConfig.importMapPathFilename
                                        ImportWizardTasks.save(CP)
                                    End If
                                    CP.Addon.ExecuteAsProcess(guidAddonImportTask)
                                    '
                                    viewId = viewIdReturnBlank
                            End Select
                        Case viewIdDone
                            '
                            ' nothing to do, keep same form
                            viewId = viewId
                    End Select
                    '
                    ' Get Next Form
                    '
                    Dim Description As String
                    Dim body As String = ""
                    'Dim ImportContentName As String
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
                            body = SelectKeyView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectGroup
                            '
                            ' Select a group to add
                            '

                            Dim headerCaption As String = "Import Wizard"

                            Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                            Dim ImportMap As ImportMapModel = ImportMapModel.create(CP, importConfig)


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
        ''
        ''=====================================================================================
        '''' <summary>
        '''' Wrap the wizard content in a form
        '''' </summary>
        '''' <param name="cp"></param>
        '''' <param name="wizardContent"></param>
        '''' <returns></returns>
        'Private ReadOnly Property getAdminFormBody(cp As CPBaseClass, wizardContent As String) As String
        '    Get
        '        Try
        '            Return cp.Html.Form(cp.Html.div(wizardContent))
        '        Catch ex As Exception
        '            cp.Site.ErrorReport(ex)
        '            Throw
        '        End Try
        '    End Get
        'End Property
        ''
        ''=====================================================================================
        '''' <summary>
        '''' Get next wizard form
        '''' </summary>
        '''' <param name="SubformID"></param>
        '''' <returns></returns>
        'Private Function nextSubFormID(app As ApplicationModel, SubformID As Integer) As Integer
        '    Try
        '        Dim Ptr As Integer = 0
        '        Do While Ptr < viewIdMax
        '            If SubformID = app.wizard.Path(Ptr) Then
        '                nextSubFormID = app.wizard.Path(Ptr + 1)
        '                Exit Do
        '            End If
        '            Ptr += 1
        '        Loop
        '    Catch ex As Exception
        '        Throw
        '    End Try
        'End Function
        ''
        ''=====================================================================================
        '''' <summary>
        '''' get previous wizard form
        '''' </summary>
        '''' <param name="SubformID"></param>
        '''' <returns></returns>
        'Private Function previousSubFormID(app As ApplicationModel, SubformID As Integer) As Integer
        '    Try
        '        Dim Ptr As Integer
        '        '
        '        Ptr = 1
        '        Do While Ptr < viewIdMax
        '            If SubformID = app.wizard.Path(Ptr) Then
        '                previousSubFormID = app.wizard.Path(Ptr - 1)
        '                Exit Do
        '            End If
        '            Ptr += 1
        '        Loop
        '    Catch ex As Exception
        '        Throw
        '    End Try
        'End Function


    End Class
End Namespace
