
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
                If Button = ButtonRestart Then
                    '
                    ' Restart
                    ImportConfigModel.clear(app)
                    Return viewIdSelectSource
                End If
                '
                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back to select source
                        Return viewIdSelectSource
                    Case ButtonContinue
                        '
                        ' -- continue to select source file
                        Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                        If (importConfig.privateUploadPathFilename <> app.cp.Doc.GetText("SelectFile")) Then
                            '
                            ' -- there was a change in the file, reset the map
                            importConfig.newImportMap(app)
                            importConfig.privateUploadPathFilename = app.cp.Doc.GetText("SelectFile")
                            importConfig.save(app)
                        End If
                        If String.IsNullOrEmpty(importConfig.privateUploadPathFilename) Then
                            '
                            ' -- no file selected
                            app.cp.UserError.Add("You must select a file to continue")
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
                Dim Description As String = cp.Html.h4("Select a file to import.") & cp.Html.p("Select a file that will be imported. This is a list of files you have previously uploaded.")
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
                Return HtmlController.createLayout(cp, headerCaption, Description, Content, True, True, True, True)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
