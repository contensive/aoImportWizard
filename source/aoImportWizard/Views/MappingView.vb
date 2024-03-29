﻿
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports C5BaseModel = Contensive.Models.Db.DbBaseModel

Namespace Contensive.ImportWizard.Controllers
    Public Class MappingView
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
                If String.IsNullOrEmpty(Button) Then Return viewIdSelectSource
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
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                If cp.Doc.GetBoolean(RequestNameImportSkipFirstRow) Then
                    ImportMap.skipRowCnt = 1
                Else
                    ImportMap.skipRowCnt = 0
                End If
                Dim FieldCnt As Integer = cp.Doc.GetInteger("ccnt")
                ImportMap.mapPairCnt = FieldCnt
                If FieldCnt > 0 Then
                    ReDim ImportMap.mapPairs(FieldCnt - 1)
                    For Ptr = 0 To FieldCnt - 1
                        Dim dbFieldName As String = cp.Doc.GetText("DBFIELD" & Ptr)
                        Dim fieldTypeId As Integer = 0
                        Dim fieldList As List(Of ContentFieldModel) = C5BaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(dbFieldName) & ")and(contentid=" & importConfig.dstContentId & ")")
                        If fieldList.Count > 0 Then
                            fieldTypeId = fieldList.First().type
                        End If
                        Dim sourceFieldPtr As Integer = cp.Doc.GetInteger("SOURCEFIELD" & Ptr)
                        Dim setValue As String = Nothing
                        If sourceFieldPtr = -2 Then setValue = cp.Doc.GetText("setValueField" & Ptr)
                        '
                        ' -- set to manual value
                        ImportMap.mapPairs(Ptr) = New ImportMapModel_MapPair With {
                            .uploadFieldPtr = sourceFieldPtr,
                            .dbFieldName = dbFieldName,
                            .dbFieldType = fieldTypeId,
                            .setValue = setValue
                        }
                    Next
                End If



                Dim mapfilenamedata As MapFilenameDataModel = ImportMapModel.decodeMapFileName(cp, cp.PrivateFiles.GetFilename(importConfig.importMapPathFilename))
                Dim ImportMapName As String = ""
                If mapfilenamedata IsNot Nothing Then
                    ImportMapName = mapfilenamedata.mapName
                End If
                Dim mapName As String = cp.Doc.GetText(requestNameImportMapName)
                If mapName <> ImportMapName Then
                    importConfig.importMapPathFilename = ImportMapModel.createMapPathFilename(app, ImportMap.contentName, mapName)
                    importConfig.save(app)
                End If





                ImportMap.save(app, importConfig)
                '
                Select Case Button
                    Case ButtonBack
                        Return viewIdSelectMap
                    Case ButtonContinue
                        Return viewIdSelectKey
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
                Dim Description As String = cp.Html.h4("Create a New Mapping") & cp.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.")
                If String.IsNullOrEmpty(importConfig.privateUploadPathFilename) Then
                    '
                    ' -- no data in upload
                    Return HtmlController.createLayout(cp, headerCaption, Description, "<P>The file you are importing is empty. Please go back and select a different file.</P>", True, True, True, False)
                End If
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                '
                ' -- Skip first Row checkbox
                Dim result As String = ""
                result &= cp.Html.CheckBox(RequestNameImportSkipFirstRow, (ImportMap.skipRowCnt <> 0)) & "&nbsp;First row contains field names"
                '
                ' -- name of mapping
                Dim mapfilenamedata As MapFilenameDataModel = ImportMapModel.decodeMapFileName(cp, cp.PrivateFiles.GetFilename(importConfig.importMapPathFilename))
                Dim ImportMapName As String = ""
                If mapfilenamedata IsNot Nothing Then
                    ImportMapName = mapfilenamedata.mapName
                End If
                result &= "<div class=""mt-4 form-group"" style=""max-width:600px;"">"
                result &= "<label for=""js-import-name"" style="""">Name for this field map</label>"
                result &= cp.Html5.InputText(requestNameImportMapName, 100, ImportMapName, "form-control", "js-import-name")
                result &= "</div>"
                '
                ' Output the table
                '
                result &= "<TABLE class=""mt-4"" border=0 cellpadding=2 cellspacing=0 width=100%>"
                result &= "" _
                    & "<TR>" _
                    & "<TD align=left>Data From</TD>" _
                    & "<TD align=left width=150>Set Value</TD>" _
                    & "<TD align=center width=10></TD>" _
                    & "<TD align=left width=300>Save Data To</TD>" _
                    & "<TD align=left width=150>Type</TD>" _
                    & "</TR>"
                Dim uploadFieldSelectTemplate As String = HtmlController.getSourceFieldSelect(app, importConfig.privateUploadPathFilename, "Ignore", importConfig.dstContentId, True, -99, "{{inputName}}", "js-import-select-{{fieldPtr}}")
                Dim rowPtr As Integer = 0
                Dim fieldList As List(Of ContentFieldModel) = C5BaseModel.createList(Of ContentFieldModel)(cp, "contentId=" & importConfig.dstContentId)
                For Each mapPair As ImportMapModel_MapPair In ImportMap.mapPairs
                    '
                    ' -- this was created as an array, not a list. So when populating when we skip fields (contentcontrolid) it leaves null entries
                    If mapPair Is Nothing Then Continue For
                    '
                    ' -- 
                    Dim mapField As ContentFieldModel = fieldList.Find(Function(x) x.name = mapPair.dbFieldName)
                    '
                    ' -- classes for each column
                    Dim cell0Style As String = "min-width:100px;"
                    Dim cell1Style As String = ""
                    Dim cell2Style As String = ""
                    Dim cell3Style As String = ""
                    Dim cell4Style As String = ""
                    '
                    ' -- get row data specific to field type
                    Dim dbFieldTypeCaption As String
                    Dim valueEditor As String = ""
                    Dim valueEditorHtmlName As String = "setValueField" & rowPtr
                    Dim setValueValue As String = mapPair.setValue
                    Select Case mapPair.dbFieldType
                        Case FieldTypeBoolean
                            dbFieldTypeCaption = "true/false"
                            valueEditor = "<input type=""checkbox"" name=""" & valueEditorHtmlName & """ value=""1"" class=""js-import-manual-data"" style=""{{styles}}""" & If(cp.Utils.EncodeBoolean(setValueValue), " checked", "") & ">"
                            cell1Style &= "vertical-align:middle;text-align:center;"
                        Case FieldTypeCurrency, FieldTypeFloat
                            dbFieldTypeCaption = "Number"
                            valueEditor = "<input type=""number"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case FieldTypeDate
                            dbFieldTypeCaption = "Date"
                            valueEditor = "<input type=""date"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                            dbFieldTypeCaption = "Filename"
                            valueEditor = "<input type=""file"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case FieldTypeInteger
                            dbFieldTypeCaption = "Integer"
                            valueEditor = "<input type=""number"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case FieldTypeLongText, FieldTypeHTML
                            dbFieldTypeCaption = "Text (8000 char)"
                            valueEditor = "<input type=""text"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case FieldTypeLookup
                            '
                            ' -- lookup
                            If mapField.lookupContentId > 0 Then
                                '
                                ' -- lookup from a table
                                Dim mapContentName As String = cp.Content.GetName(mapField.lookupContentId)
                                valueEditor = cp.Html5.SelectContent(valueEditorHtmlName, setValueValue, mapContentName, "", "", "form-control js-import-manual-data").Replace("<select ", "<select style=""{{styles}}""")
                            ElseIf Not String.IsNullOrEmpty(mapField.lookupList) Then
                                '
                                ' -- lookup from a list
                                valueEditor = cp.Html5.SelectList(valueEditorHtmlName, setValueValue, mapField.lookupList, "", "form-control js-import-manual-data").Replace("<select ", "<select style=""{{styles}}""")
                            Else
                                '
                                ' -- invalid, just enter integer
                                valueEditor = "<input type=""number"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            End If


                            dbFieldTypeCaption = "Lookup"

                            'valueEditor = "<select name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}""><option name=""""></option></select>"
                        Case FieldTypeManyToMany
                            dbFieldTypeCaption = "Integer ID"
                            valueEditor = ""
                        Case FieldTypeMemberSelect
                            dbFieldTypeCaption = "Integer ID"
                            valueEditor = "<input type=""number"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            'valueEditor = "<select name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}""><option name=""""></option></select>"
                        Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                            dbFieldTypeCaption = "Text (255 char)"
                            valueEditor = "<input type=""text"" name=""" & valueEditorHtmlName & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                        Case Else
                            dbFieldTypeCaption = "Invalid [" & mapPair.dbFieldType & "]"
                            valueEditor = ""
                    End Select
                    Dim uploadFieldSelect As String = uploadFieldSelectTemplate.Replace("{{fieldPtr}}", rowPtr.ToString()).Replace("{{inputName}}", "SourceField" & rowPtr)
                    Select Case mapPair.uploadFieldPtr
                        Case -5
                            '
                            ' -- for people only. set to lastname of name
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""-5"">", "value=""-5"" selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                        Case -4
                            '
                            ' -- for people only. set to firstname of name
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""-4"">", "value=""-4"" selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                        Case -3
                            '
                            ' -- for people only. set to 'firstname lastname'
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""-3"">", "value=""-3"" selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                        Case -2
                            '
                            ' -- set value
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""-2"">", "value=""-2"" selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "")
                        Case -1
                            '
                            ' -- ignore
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""-1"">", "value=""-1"" selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                        Case Else
                            '
                            ' -- set to upload field value
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""" & mapPair.uploadFieldPtr & """>", "value=""" & mapPair.uploadFieldPtr & """ selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                    End Select
                    '
                    ' Now customize the caption for the DBField a little
                    '
                    Dim dbFieldCaption As String = mapPair.dbFieldName
                    If mapField IsNot Nothing Then
                        dbFieldCaption = mapField.caption
                    End If
                    Dim rowStyle As String
                    Dim cellClass As String = "text-align:center;vertical-align:middle;"
                    If rowPtr Mod 2 = 0 Then
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;vertical-align:middle;"
                    Else
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;vertical-align:middle;"
                    End If
                    result = result _
                        & vbCrLf _
                        & "<TR>" _
                        & "<TD style=""" & cell0Style & rowStyle & """ align=left>" & uploadFieldSelect & "</td>" _
                        & "<TD style=""" & cell1Style & rowStyle & """ align=left>" & valueEditor & "</td>" _
                        & "<TD style=""" & cell2Style & rowStyle & """ align=center>&gt;&gt;</TD>" _
                        & "<TD style=""" & cell3Style & rowStyle & """ align=left>&nbsp;" & dbFieldCaption & "<input type=hidden name=DbField" & rowPtr & " value=""" & mapPair.dbFieldName & """></td>" _
                        & "<TD style=""" & cell4Style & rowStyle & """ align=left>&nbsp;" & dbFieldTypeCaption & "</td>" _
                        & "</TR>"
                    rowPtr += 1
                Next
                '
                result &= "<input type=hidden name=Ccnt value=" & rowPtr & ">"
                result &= "</TABLE>"
                result &= cp.Html.Hidden(rnSrcViewId, viewIdMapping.ToString)
                Return HtmlController.createLayout(cp, headerCaption, Description, result, True, True, True, True)
                'Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, result)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
