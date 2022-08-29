
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
'Imports Contensive.Models.Db

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
                    '
                    ' -- reset import map
                    importConfig.newImportMap(app)
                    importConfig.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)
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
                        Case ButtonFinish
                            Return viewIdSelectSource
                        Case ButtonBack
                            Return viewIdUpload
                        Case ButtonContinue
                            Return viewIdFinish
                        Case Else
                            Return viewIdSelectTable
                    End Select
                End If
                '
                ' -- Match to existing table
                If (importConfig.dstContentId <> cp.Doc.GetInteger(RequestNameImportContentID)) Then
                    '
                    ' -- content changed, reset import map
                    importConfig.newImportMap(app)
                    importConfig.dstContentId = cp.Doc.GetInteger(RequestNameImportContentID)
                    importConfig.save(app)
                    '
                    ' -- build a new map
                    Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                    ImportMap.contentName = cp.Content.GetName(importConfig.dstContentId)
                    Dim dbFieldNames() As String = Split(ContentFieldModel.getDbFieldList(cp, ImportMap.contentName, False), ",")
                    Dim dbFieldNameCnt As Integer = UBound(dbFieldNames) + 1
                    ' todo di-hack
                    app.loadUploadFields(importConfig.privateUploadPathFilename)
                    Dim uploadFields() As String = app.uploadFields
                    ImportMap.mapPairCnt = dbFieldNameCnt
                    ReDim ImportMap.mapPairs(dbFieldNameCnt - 1)
                    Dim rowPtr As Integer = 0
                    For Each dbFieldName In dbFieldNames
                        '
                        ' -- setup mapPair
                        Dim mapRow = New ImportMapModel_MapPair()
                        ImportMap.mapPairs(rowPtr) = mapRow
                        mapRow.dbFieldName = dbFieldName
                        mapRow.setValue = Nothing
                        mapRow.dbFieldType = ContentFieldModel.getFieldType(cp, dbFieldName, importConfig.dstContentId)
                        mapRow.uploadFieldPtr = -1
                        mapRow.uploadFieldName = ""
                        '
                        ' -- search uploadFields for matches to dbFields
                        Dim dBFieldName_lower As String = LCase(dbFieldName)
                        Dim uploadFieldPtr As Integer = 0
                        For Each uploadField As String In uploadFields
                            Dim uploadField_lower As String = uploadField.ToLowerInvariant()
                            If uploadField_lower = dBFieldName_lower Then
                                mapRow.uploadFieldPtr = uploadFieldPtr
                                mapRow.uploadFieldName = uploadField
                                Exit For
                            End If
                            Select Case dBFieldName_lower
                                Case "company"
                                    If (uploadField_lower = "companyname") OrElse (uploadField_lower = "company name") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "zip"
                                    If (uploadField_lower = "zip code") OrElse (uploadField_lower = "zipcode") OrElse (uploadField_lower = "postal code") OrElse (uploadField_lower = "postalcode") OrElse (uploadField_lower = "zip/postalcode") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "firstname"
                                    If (uploadField_lower = "first") OrElse (uploadField_lower = "first name") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "lastname"
                                    If (uploadField_lower = "last") OrElse (uploadField_lower = "last name") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "email"
                                    If (uploadField_lower = "e-mail") OrElse (uploadField_lower = "emailaddress") OrElse (uploadField_lower = "e-mailaddress") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "address"
                                    If (uploadField_lower = "address1") OrElse (uploadField_lower = "addressline1") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "address2"
                                    If (uploadField_lower = "addressline2") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                                Case "phone"
                                    If (uploadField_lower = "phone number") OrElse (uploadField_lower = "phonenumber") Then
                                        mapRow.uploadFieldPtr = uploadFieldPtr
                                        mapRow.uploadFieldName = uploadField
                                        Exit For
                                    End If
                            End Select
                            uploadFieldPtr += 1
                        Next
                        rowPtr += 1
                    Next
                    ImportMap.save(app, importConfig)
                End If
                '
                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back
                        Return viewIdUpload
                    Case ButtonContinue
                        '
                        ' -- continue
                        Return viewIdNewMapping
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
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectTable.ToString)
                Return HtmlController.createLayout(cp, headerCaption, Description, Content, True, True, True, True)
                'Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
