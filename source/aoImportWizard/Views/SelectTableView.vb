
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports Contensive.Models.Db

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
                    ImportConfigModel.clear(app)
                    Return viewIdReturnBlank
                End If
                '
                ' Load the importmap with what we have so far
                '
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig)


                Dim useNewContentName As Boolean = cp.Doc.GetBoolean("useNewContentName")
                If useNewContentName Then
                    Dim newContentName As String = cp.Doc.GetText("newContentName")
                    ImportMap.contentName = newContentName
                    ImportMap.importToNewContent = True
                    ImportMap.skipRowCnt = 1
                    Call ImportMap.save(app, importConfig)
                    'Call WizardController.saveWizardRequestInteger(cp, RequestNameImportContentID)
                    Select Case Button
                        Case ButtonCancel
                            ImportConfigModel.clear(app)
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
                    If (importConfig.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)) Then
                        '
                        ' -- no change, use existing import map
                    Else
                        '
                        ' -- content changed, reset import map

                        importConfig.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)
                        importConfig.save(app)
                        ImportMap.contentName = cp.Content.GetName(importConfig.dstContentId)

                        Dim dbFieldNames() As String = Split(GenericController.getDbFieldList(cp, ImportMap.contentName, False), ",")
                        Dim dbFieldNameCnt As Integer = UBound(dbFieldNames) + 1
                        ImportMap.mapPairCnt = dbFieldNameCnt
                        ReDim ImportMap.mapPairs(dbFieldNameCnt - 1)
                        For rowPtr As Integer = 0 To dbFieldNameCnt - 1
                            '
                            ' -- set to manual value
                            Dim dbField As ContentFieldModel = Nothing
                            Dim dbFieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(dbFieldNames(rowPtr)) & ")And(contentid=" & importConfig.dstContentId & ")")
                            If dbFieldList.Count > 0 Then
                                dbField = dbFieldList.First()
                            End If
                            '
                            ' -- search uploadFields for matches to dbFields
                            '
                            '
                            ' -- created a 
                            ImportMap.mapPairs(rowPtr) = New ImportMapModel_MapPair With {
                            .uploadFieldPtr = -1,
                            .dbFieldName = dbFieldNames(rowPtr),
                            .dbFieldType = dbField.type,
                            .setValue = ""
                        }
                        Next
                        ImportMap.save(app, importConfig)
                    End If
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

                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig)
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
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectTable.ToString)
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
