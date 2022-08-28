
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectTableView
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
                ' Load the importmap with what we have so far
                '
                Dim importData As ImportDataModel = ImportDataModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importData)


                Dim useNewContentName As Boolean = cp.Doc.GetBoolean("useNewContentName")
                If useNewContentName Then
                    Dim newContentName As String = cp.Doc.GetText("newContentName")
                    ImportMap.contentName = newContentName
                    ImportMap.importToNewContent = True
                    ImportMap.skipRowCnt = 1
                    Call ImportMap.save(app)
                    'Call WizardController.saveWizardRequestInteger(cp, RequestNameImportContentID)
                    Select Case Button
                        Case ButtonCancel
                            ImportDataModel.clear(app)
                            Return viewIdReturnBlank
                        Case ButtonFinish
                            Return viewIdSelectSource
                        Case ButtonBack2
                            Return viewIdUpload
                        Case ButtonContinue2
                            Return viewIdFinish
                        Case Else
                            Return viewIdSelectTable
                    End Select
                Else
                    importData.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)
                    importData.save(app)
                    '
                    Select Case Button
                        Case ButtonBack2
                            '
                            ' -- back
                            Return viewIdUpload
                        Case ButtonContinue2
                            '
                            ' -- continue
                            Return viewIdNewMapping
                    End Select
                End If
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

                Dim importData As ImportDataModel = ImportDataModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importData)
                Dim ImportContentID As Integer = importData.dstContentId
                If ImportContentID = 0 Then
                    ImportContentID = cp.Content.GetID("People")
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
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectTable.ToString)
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
