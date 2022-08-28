
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class _blankView
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
                        ' -- upload the file and continue
                        Dim Filename As String = app.cp.Doc.GetText(RequestNameImportUpload)
                        If String.IsNullOrEmpty(Filename) Then Return viewIdSelectSource
                        '
                        Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                        app.cp.TempFiles.SaveUpload(RequestNameImportUpload, "upload", importConfig.privateUploadPathFilename)
                        importConfig.save(app)
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
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)








            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
