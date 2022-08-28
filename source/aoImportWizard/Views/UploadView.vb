
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class UploadView
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
                    ImportDataModel.clear(app)
                    Return viewIdReturnBlank
                End If
                '
                Select Case Button
                    Case ButtonBack2
                        '
                        ' -- back to select source
                        Return viewIdSelectSource
                    Case ButtonContinue2
                        '
                        ' -- upload the file and continue
                        Dim Filename As String = app.cp.Doc.GetText(RequestNameImportUpload)
                        If String.IsNullOrEmpty(Filename) Then Return viewIdSelectSource
                        '
                        Dim importData As ImportDataModel = ImportDataModel.create(app)
                        If Not app.cp.PrivateFiles.SaveUpload(RequestNameImportUpload, "importWizardUploads", importData.privateCsvPathFilename) Then
                            '
                            ' -- upload failed, stay on this view
                            Return srcViewId
                        End If
                        importData.privateCsvPathFilename = "importWizardUploads/" & importData.privateCsvPathFilename
                        importData.save(app)
                        '
                        Return viewIdSelectTable
                End Select
                Return viewIdUpload
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
                Dim description As String = cp.Html.h4("Upload your File") & cp.Html.p("Hit browse to locate the file you want to upload")
                Dim content As String = "" _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" & cp.Html.InputFile(RequestNameImportUpload) & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                content &= cp.Html5.Hidden(rnSrcViewId, viewIdUpload)
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, description, content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
