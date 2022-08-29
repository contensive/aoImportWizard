
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports C5BaseModel = Contensive.Models.Db.DbBaseModel

Namespace Contensive.ImportWizard.Controllers
    Public Class FinishView
        '
        ''' <summary>
        ''' return the next view. 0 goes to the first form (start over)
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function processView(app As ApplicationModel, srcViewId As Integer) As Integer
            Try
                Dim cp As CPBaseClass = app.cp
                Dim Button As String = app.cp.Doc.GetText(RequestNameButton)
                If String.IsNullOrEmpty(Button) Then Return srcViewId
                If Button = ButtonCancel Then
                    '
                    ' Cancel
                    ImportConfigModel.clear(app)
                    Return viewIdReturnBlank
                End If
                If Button = ButtonRestart Then
                    '
                    ' Restart
                    ImportConfigModel.clear(app)
                    Return viewIdSelectSource
                End If


                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back to select key
                        Return viewIdSelectKey
                    Case ButtonContinue
                        Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                        Dim ImportWizardTasks = C5BaseModel.addDefault(Of Contensive.Models.Db.ImportWizardTaskModel)(cp)
                        If (ImportWizardTasks IsNot Nothing) Then
                            ImportWizardTasks.name = Now() & " CSV Import"
                            ImportWizardTasks.uploadFilename = importConfig.privateUploadPathFilename
                            ImportWizardTasks.notifyEmail = importConfig.notifyEmail
                            ImportWizardTasks.importMapFilename = importConfig.importMapPathFilename
                            ImportWizardTasks.save(cp)
                        End If
                        '
                        ' -- queue the import tak
                        cp.Addon.ExecuteAsProcess(guidAddonImportTask)
                        '
                        ' -- clear the import wizard to start fresh
                        ImportConfigModel.clear(app)
                        '
                        ' -- thank you page, with reset button
                        Return viewIdDone
                End Select
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        ''' <summary>
        ''' return the html for this view
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function getView(app As ApplicationModel) As String
            Try
                Dim cp As CPBaseClass = app.cp
                Dim headerCaption As String = "Import Wizard"
                Dim description As String = ""
                Dim content As String = ""
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                '
                description = cp.Html.h4("Finish") & cp.Html.p("Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete.")
                content = "<div Class=""p-2""><label for=""name381"">Email</label><div class=""ml-5"">" & cp.Html5.InputText(RequestNameImportEmail, 255, cp.User.Email) & "</div><div class=""ml-5""><small class=""form-text text-muted""></small></div></div>"
                content &= cp.Html.Hidden(rnSrcViewId, viewIdFinish.ToString)
                Return HtmlController.createLayout(cp, headerCaption, description, content, True, True, True, True)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
