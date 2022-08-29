
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectSourceView
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
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                importConfig.importSource = CType(app.cp.Doc.GetInteger(RequestNameImportSource), ImportDataModel_ImportTypeEnum)
                importConfig.save(app)
                '
                Select Case Button
                    Case ButtonBack
                        '
                        Return srcViewId
                    Case ButtonContinue
                        '
                        Select Case importConfig.importSource
                            Case ImportDataModel_ImportTypeEnum.UploadFile
                                '
                                ' -- upload a commad delimited file
                                Return viewIdUpload
                            Case Else
                                '
                                ' -- use a file uploaded previously
                                Return viewIdSelectFile
                        End Select
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
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim headerCaption As String = "Import Wizard"
                Dim importSource As Integer = Convert.ToInt32(importConfig.importSource)
                Dim description As String = cp.Html.h4("Select the import source") & cp.Html.p("If you want to upload a new comma-delimited text file (csv), select the upload option. If you want to use a file you previously uploaded, select the second option.")
                Dim content As String = "" _
                    & "<div class=""ml-4"">" _
                    & "<div class=""pb-2"">" _
                    & "<label>" & cp.Html.RadioBox(RequestNameImportSource, ImportSourceUpload.ToString, importSource.ToString, "mr-2") & "Upload a comma delimited text file (up to 5 MBytes).</label>" _
                    & "</div>" _
                    & "<div class=""pb-2"">" _
                    & "<label>" & cp.Html.RadioBox(RequestNameImportSource, ImportSourceUploadFolder.ToString, importSource.ToString, "mr-2") & "Use a file you previously uploaded.</label>" _
                    & "</div>" _
                    & "</div>" _
                    & ""
                content &= cp.Html5.Hidden(rnSrcViewId, viewIdSelectSource)
                Return HtmlController.createLayout(cp, headerCaption, description, content, True, False, False, True)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
