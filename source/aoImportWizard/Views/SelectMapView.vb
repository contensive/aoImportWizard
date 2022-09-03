
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectMapView
        '
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
                If String.IsNullOrEmpty(cp.Doc.GetText("selectMapRow")) Then
                    '
                    ' -- create new map for this content
                    ImportMapModel.buildNewImportMapForContent(app, importConfig)
                Else
                    '
                    ' -- use a previous mapping for this content
                    importConfig.importMapPathFilename = ImportMapModel.getMapPath(app) & cp.Doc.GetText("selectMapRow")
                End If
                importConfig.save(app)
                '
                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back to select content
                        Return viewIdSelectContent
                    Case ButtonContinue
                        '
                        ' -- 
                        '
                        Return viewIdMapping
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
                Dim description As String = cp.Html.h4("Select how the import maps to the table.") & "<p>To import data into this table, you have to map the input fields to the database fields. You can either create a new map or select one you have previously used.</p>"
                Dim mapPth As String = ImportMapModel.getMapPath(app)
                Dim fileList As New List(Of String)
                For Each file In ImportMapModel.getMapFileList(app)
                    fileList.Add(file.Name)
                Next
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim content As String = cp.Html5.SelectList("selectMapRow", importConfig.importMapPathFilename, String.Join(",", fileList), "Create New Field Mapping", "form-control")
                content &= cp.Html5.Hidden(rnSrcViewId, viewIdSelectMap)
                Return HtmlController.createLayout(cp, headerCaption, description, content, True, True, True, True)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
