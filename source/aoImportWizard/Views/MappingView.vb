
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports Contensive.Models.Db

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
                '
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig)
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
                        Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(dbFieldName) & ")and(contentid=" & importConfig.dstContentId & ")")
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
                ImportMap.save(app, importConfig)
                '
                Select Case Button
                    Case ButtonBack2
                        Return viewIdSelectTable
                    Case ButtonContinue2
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
                '
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig)
                '
                ' Get Mapping fields
                '
                Dim Description As String = cp.Html.h4("Create a New Mapping") & cp.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.")

                If String.IsNullOrEmpty(importConfig.privateUploadPathFilename) Then
                    '
                    ' no data in upload
                    '
                    Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, "", Description, "<P>The file you are importing is empty. Please go back and select a different file.</P>")
                End If
                Dim uploadData As String = cp.PrivateFiles.Read(importConfig.privateUploadPathFilename)
                '
                ' Skip first Row checkbox
                '
                Dim result As String = ""
                result &= cp.Html.CheckBox(RequestNameImportSkipFirstRow, (ImportMap.skipRowCnt <> 0)) & "&nbsp;First row contains field names"
                result &= "<div>&nbsp;</div>"
                '
                ' Build FileColumns
                '
                Dim uploadFieldSelectTemplate As String = HtmlController.getSourceFieldSelect(app, importConfig.privateUploadPathFilename, "Ignore")
                '
                ' Build the Database field list
                '
                'Dim PeopleContentID As Integer = cp.Content.GetID("people")
                Dim ImportContentID As Integer = importConfig.dstContentId
                Dim ImportContentName As String = cp.Content.GetRecordName("content", ImportContentID)
                Dim dbFieldNames() As String = Split(GenericController.getDbFieldList(cp, ImportContentName, False), ",")
                '
                ' Output the table
                '
                result &= "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>"
                result &= "" _
                    & "<TR>" _
                    & "<TD align=left>Data From</TD>" _
                    & "<TD align=left width=200>Set Value</TD>" _
                    & "<TD align=center width=10></TD>" _
                    & "<TD align=left width=200>Save Data To</TD>" _
                    & "<TD align=left width=200>Type</TD>" _
                    & "</TR>"
                Dim rowMax As Integer = 0
                For rowPtr As Integer = 0 To UBound(dbFieldNames)
                    '
                    ' -- classes for each column
                    Dim cell0Style As String = ""
                    Dim cell1Style As String = ""
                    Dim cell2Style As String = ""
                    Dim cell3Style As String = ""
                    Dim cell4Style As String = ""
                    '
                    ' -- get field data
                    Dim dBFieldName As String = dbFieldNames(rowPtr)
                    Dim dbField As ContentFieldModel = Nothing
                    Dim dbFieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(dBFieldName) & ")And(contentid=" & cp.Content.GetID(ImportContentName) & ")")
                    If dbFieldList.Count > 0 Then
                        dbField = dbFieldList.First()
                    End If
                    '
                    ' Find match in current ImportMap
                    '
                    Dim dBFieldName_lcase As String = LCase(dBFieldName)
                    Dim importMap_MapPairPtr As Integer = 0
                    If ImportMap.mapPairCnt > 0 Then
                        For importMap_MapPairPtr = 0 To ImportMap.mapPairCnt - 1
                            If dBFieldName_lcase = LCase(ImportMap.mapPairs(importMap_MapPairPtr).dbFieldName) Then
                                Exit For
                            End If
                        Next
                    End If
                    If (importMap_MapPairPtr = ImportMap.mapPairCnt) Then
                        '
                        ' -- no field found -- error, skip field
                        Continue For
                    End If
                    If ImportMap.mapPairs(importMap_MapPairPtr).uploadFieldPtr < 0 Then
                        '
                        ' -- no DbField found matching the uploadField, find close matches
                        Dim uploadFieldName As String
                        For uploadFieldPtrx As Integer = 0 To app.sourceFieldCnt - 1
                            uploadFieldName = app.uploadFields(uploadFieldPtrx)
                            uploadFieldName = LCase(uploadFieldName)
                            uploadFieldName = Replace(uploadFieldName, " ", "")
                            '
                            ' check for exact match
                            '
                            If dBFieldName_lcase = uploadFieldName Then
                                Exit For
                            End If
                            '
                            ' check for pseudo match
                            '
                            Select Case dBFieldName_lcase
                                Case "zip"
                                    If (uploadFieldName = "postalcode") Or (uploadFieldName = "zip/postalcode") Then
                                        Exit For
                                    End If
                                Case "firstname"
                                    If uploadFieldName = "first" Then
                                        Exit For
                                    End If
                                Case "lastname"
                                    If uploadFieldName = "last" Then
                                        Exit For
                                    End If
                                Case "email"
                                    If (uploadFieldName = "e-mail") Or (uploadFieldName = "emailaddress") Or (uploadFieldName = "e-mailaddress") Then
                                        Exit For
                                    End If
                                Case "address"
                                    If (uploadFieldName = "address1") Or (uploadFieldName = "addressline1") Then
                                        Exit For
                                    End If
                                Case "address2"
                                    If (uploadFieldName = "addressline2") Then
                                        Exit For
                                    End If
                            End Select
                        Next
                    End If
                    '
                    ' -- get row data specific to field type
                    Dim dbFieldTypeCaption As String
                    Dim valueEditor As String = ""
                    If dbField Is Nothing Then
                        dbFieldTypeCaption = "Text (255 char)"
                        valueEditor = "<input name="""" type=""text"">"
                    Else
                        Dim setValueInput As String = "setValueField" & rowPtr
                        Dim setValueValue As String = ImportMap.mapPairs(importMap_MapPairPtr).setValue
                        Select Case dbField.type
                            Case FieldTypeBoolean
                                dbFieldTypeCaption = "true/false"
                                valueEditor = "<input type=""checkbox"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""js-import-manual-data"" style=""{{styles}}"">"
                                cell1Style &= "vertical-align:middle;text-align:center;"
                            Case FieldTypeCurrency, FieldTypeFloat
                                dbFieldTypeCaption = "Number"
                                valueEditor = "<input type=""number"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case FieldTypeDate
                                dbFieldTypeCaption = "Date"
                                valueEditor = "<input type=""date"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                dbFieldTypeCaption = "Filename"
                                valueEditor = "<input type=""file"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case FieldTypeInteger
                                dbFieldTypeCaption = "Integer"
                                valueEditor = "<input type=""number"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case FieldTypeLongText, FieldTypeHTML
                                dbFieldTypeCaption = "Text (8000 char)"
                                valueEditor = "<input type=""text"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case FieldTypeLookup
                                dbFieldTypeCaption = "Integer ID"
                                valueEditor = "<select name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}""><option name=""""></option></select>"
                            Case FieldTypeManyToMany
                                dbFieldTypeCaption = "Integer ID"
                                valueEditor = ""
                            Case FieldTypeMemberSelect
                                dbFieldTypeCaption = "Integer ID"
                                valueEditor = "<select name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}""><option name=""""></option></select>"
                            Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                dbFieldTypeCaption = "Text (255 char)"
                                valueEditor = "<input type=""text"" name=""" & setValueInput & """ value=""" & setValueValue & """ class=""form-control js-import-manual-data"" style=""{{styles}}"">"
                            Case Else
                                dbFieldTypeCaption = "Invalid [" & dbField.type & "]"
                                valueEditor = ""
                        End Select
                    End If
                    Dim uploadFieldSelect As String = uploadFieldSelectTemplate.Replace("{{fieldId}}", dbField.id.ToString()).Replace("{{inputName}}", "SourceField" & rowPtr)
                    Dim uploadFieldPtr As Integer = ImportMap.mapPairs(importMap_MapPairPtr).uploadFieldPtr
                    Select Case uploadFieldPtr
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
                            uploadFieldSelect = Replace(uploadFieldSelect, "value=""" & uploadFieldPtr & """>", "value=""" & uploadFieldPtr & """ selected>", , , vbTextCompare)
                            valueEditor = valueEditor.Replace("{{styles}}", "display:none;")
                    End Select
                    '
                    ' Now customize the caption for the DBField a little
                    '
                    Dim dbFieldCaption As String = dBFieldName
                    If Not cp.User.IsDeveloper Then
                        Select Case LCase(dbFieldCaption)
                            Case "id"
                                dbFieldCaption = "Contensive ID"
                        End Select
                    End If
                    Dim rowStyle As String
                    Dim cellClass As String = "text-align:center;vertical-align:middle;"
                    If rowPtr Mod 2 = 0 Then
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;"
                    Else
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;"
                    End If
                    result = result _
                        & vbCrLf _
                        & "<TR>" _
                        & "<TD style=""" & cell0Style & rowStyle & """ align=left>" & uploadFieldSelect & "</td>" _
                        & "<TD style=""" & cell1Style & rowStyle & """ align=left>" & valueEditor & "</td>" _
                        & "<TD style=""" & cell2Style & rowStyle & """ align=center>&gt;&gt;</TD>" _
                        & "<TD style=""" & cell3Style & rowStyle & """ align=left>&nbsp;" & dbFieldCaption & "<input type=hidden name=DbField" & rowPtr & " value=""" & dBFieldName & """></td>" _
                        & "<TD style=""" & cell4Style & rowStyle & """ align=left>&nbsp;" & dbFieldTypeCaption & "</td>" _
                        & "</TR>"
                    rowMax = rowPtr
                Next
                result &= "<input type=hidden name=Ccnt value=" & rowMax & ">"
                result &= "</TABLE>"
                result &= cp.Html.Hidden(rnSrcViewId, viewIdNewMapping.ToString)
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, result)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
