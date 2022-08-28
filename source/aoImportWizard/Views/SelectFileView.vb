
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectFileView
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
                '
                Select Case Button
                    Case ButtonBack2
                        '
                        ' -- back to select source
                        Return viewIdSelectSource
                    Case ButtonContinue2
                        '
                        ' -- continue to select source file
                        Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                        importConfig.privateUploadPathFilename = app.cp.Doc.GetText("SelectFile")
                        If String.IsNullOrEmpty(importConfig.privateUploadPathFilename) Then
                            '
                            ' -- no file selected
                            app.cp.UserError.Add("You must select an upload to continue")
                            Return srcViewId
                        End If
                        If Left(importConfig.privateUploadPathFilename, 1) = "\" Then importConfig.privateUploadPathFilename = Mid(importConfig.privateUploadPathFilename, 2)
                        importConfig.save(app)
                        '
                        Return viewIdSelectTable
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
                Dim Description As String = cp.Html.h4("Select a file from your Upload folder") & cp.Html.p("Select the upload file you wish to import")
                Call cp.Doc.AddRefreshQueryString(rnSrcViewId, viewIdSelectFile.ToString)
                Dim fileList2 As New StringBuilder()
                Dim uploadPtr As Integer = 0
                For Each file In cp.PrivateFiles.FileList(privateFilesUploadPath)
                    Dim uploadId As String = "upload" & uploadPtr
                    Dim input As String = "<label for=""" & uploadId & """>" & cp.Html.RadioBox("selectfile", privateFilesUploadPath & file.Name, "", "mr-2", uploadId) & file.Name & "</label>"
                    fileList2.Append(cp.Html.div(input, "", "pb-2"))
                    uploadPtr += 1
                Next
                Dim Content As String = fileList2.ToString()
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectFile.ToString)
                '
                Call cp.Doc.AddRefreshQueryString(rnSrcViewId, CType("", String))
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
