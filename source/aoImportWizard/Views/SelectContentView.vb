
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Model
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectContentView
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
                ' Load the importmap with what we have so far
                '
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                '
                If cp.Doc.GetBoolean("useNewContentName") Then
                    Dim contentName As String = cp.Doc.GetText("newContentName")
                    Dim mapName As String = "Import " & contentName
                    '
                    ' -- reset import map
                    importConfig.importMapPathFilename = ImportMapModel.createMapPathFilename(app, contentName, mapName)
                    importConfig.dstContentId = 0
                    importConfig.save(app)
                    '
                    ' -- new table. Save table and return
                    Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                    ImportMap.contentName = cp.Doc.GetText("newContentName")
                    ImportMap.importToNewContent = True
                    ImportMap.skipRowCnt = 1
                    Call ImportMap.save(app, importConfig)
                    '
                    Select Case Button
                        Case ButtonBack
                            '
                            ' -- back
                            Return viewIdSelectSource
                        Case Else
                            '
                            ' -- continue
                            Return viewIdSelectMap
                    End Select
                End If
                '
                ' -- Match to existing table
                If (importConfig.dstContentId <> cp.Doc.GetInteger(RequestNameImportContentID)) Then
                    '
                    ' -- content changed, reset import map
                    'importConfig.newImportMap(app)
                    importConfig.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)
                    importConfig.importMapPathFilename = ""
                    importConfig.save(app)
                End If
                '
                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back
                        Return viewIdSelectSource
                    Case Else
                        '
                        ' -- continue
                        Return viewIdSelectMap
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

                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                Dim ImportContentID As Integer = importConfig.dstContentId
                If ImportContentID = 0 Then
                    ImportContentID = app.peopleContentid
                End If
                Dim inputRadioNewContent As String
                Dim inputRadioExistingContent As String
                '
                If ImportMap.importToNewContent Then
                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""1"" checked>"
                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""0"">"
                Else
                    inputRadioNewContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""1"">"
                    inputRadioExistingContent = "<input type=""radio"" name=""useNewContentName"" class=""mr-2"" value=""0"" checked>"
                End If
                Dim Description As String = cp.Html.h4("Select the destination for your data") & cp.Html.p("For example, to import a list in to people, select People.")
                Dim contentSelect As String = cp.Html.SelectContent(RequestNameImportContentID, ImportContentID.ToString, "Content", "", "", "form-control")
                contentSelect = contentSelect.Replace("<select ", "<select style=""max-width:300px; display:inline;"" ")
                Dim Content As String = "" _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" _
                    & "<tr><td colspan=""2"">Import into an existing content table</td></tr>" _
                    & "<tr><td colspan=""2"">" & inputRadioExistingContent & contentSelect & "</td></tr>" _
                    & "<tr><td colspan=""2"">Create a new content table</td></tr>" _
                    & "<tr><td colspan=""2"">" & inputRadioNewContent & "<input style=""max-width:300px; display:inline;"" type=""text"" name=""newContentName"" value=""" & If(ImportMap.importToNewContent, ImportMap.contentName, "") & """ class=""form-control""></td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectContent.ToString)
                Return HtmlController.createLayout(cp, headerCaption, Description, Content, True, True, True, True)
                'Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
