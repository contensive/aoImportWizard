
Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard.Controllers
    Public Class UploadView
        '
        ''' <summary>
        ''' return the next view. 0 goes to the first form (start over)
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function processView(app As ApplicationController) As Integer
            Try

                app.cp.Html.ProcessInputFile(RequestNameImportUpload, "upload")
                Dim Filename As String = app.cp.Doc.GetText(RequestNameImportUpload)
                Call WizardController.saveWizardValue(app.cp, RequestNameImportUpload, "upload/" & Filename)
                WizardController.loadWizardPath(app)

                Dim Button As String = app.cp.Doc.GetText(RequestNameButton)
                Select Case Button
                    Case ButtonBack2
                        '
                        ' -- back to select source
                        Return viewIdSelectSource
                    Case ButtonContinue2
                        '
                        ' -- comntinue to select destination
                        Return viewIdSelectDestination
                End Select
                Return viewIdUpload
                '
                'Call WizardController.saveWizardRequestInteger(app.cp, RequestNameImportSource)
                'Call WizardController.loadWizardPath(app)

                'Dim Button As String = app.cp.Doc.GetText(RequestNameButton)
                'Select Case Button
                '    Case ButtonCancel
                '        '
                '        ' Cancel
                '        '
                '        Call WizardController.clearWizardValues(app.cp)
                '        Return viewIdReturnBlank
                '    Case ButtonContinue2
                '        '
                'End Select
                'Return viewIdSelectSource
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
        Public Shared Function getView(app As ApplicationController) As String
            Try
                Dim cp As CPBaseClass = app.cp
                Dim headerCaption As String = "Import Wizard"
                Dim description As String = cp.Html.h4("Upload your File") & cp.Html.p("Hit browse to locate the file you want to upload")
                Dim content As String = "" _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" & cp.Html.InputFile(RequestNameImportUpload) & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                Return WizardController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, description, content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
