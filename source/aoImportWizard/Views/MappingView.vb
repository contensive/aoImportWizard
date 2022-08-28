
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
                    ImportDataModel.clear(app)
                    Return viewIdReturnBlank
                End If
                '
                Dim importData As ImportDataModel = ImportDataModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importData)
                If cp.Doc.GetBoolean(RequestNameImportSkipFirstRow) Then
                    ImportMap.skipRowCnt = 1
                Else
                    ImportMap.skipRowCnt = 0
                End If
                Dim FieldCnt As Integer = cp.Doc.GetInteger("ccnt")
                ImportMap.mapPairCnt = FieldCnt
                If FieldCnt > 0 Then
                    ReDim ImportMap.mapPairs(FieldCnt - 1)
                    If FieldCnt > 0 Then
                        For Ptr = 0 To FieldCnt - 1
                            Dim SourceFieldPtr As Integer = cp.Doc.GetInteger("SOURCEFIELD" & Ptr)
                            Dim DbField As String = cp.Doc.GetText("DBFIELD" & Ptr)
                            ImportMap.mapPairs(Ptr) = New ImportMapModel_MapPair With {
                                .sourceFieldPtr = SourceFieldPtr,
                                .dbField = DbField
                            }
                            Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(DbField) & ")and(contentid=" & cp.Content.GetID(ImportMap.contentName) & ")")
                            If (fieldList.Count > 0) Then
                                ImportMap.mapPairs(Ptr).dbFieldType = fieldList.First().type
                            End If
                        Next
                    End If
                End If
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
                Dim importData As ImportDataModel = ImportDataModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importData)
                '
                ' Get Mapping fields
                '
                Dim FileData As String = ""
                Dim Description As String = cp.Html.h4("Create a New Mapping") & cp.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.")

                If String.IsNullOrEmpty(importData.privateCsvPathFilename) Then
                    '
                    ' no data in upload
                    '
                    Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, "", Description, "<P>The file you are importing is empty. Please go back and select a different file.</P>")
                End If

                Dim Filename As String = importData.privateCsvPathFilename
                If Not String.IsNullOrEmpty(Filename) Then
                    If Left(Filename, 1) = "\" Then
                        Filename = Mid(Filename, 2)
                    End If
                    FileData = cp.PrivateFiles.Read(Filename)
                End If
                '
                ' Skip first Row checkbox
                '
                Dim Content As String = cp.Html.CheckBox(RequestNameImportSkipFirstRow, (ImportMap.skipRowCnt <> 0)) & "&nbsp;First row contains field names"
                Content &= "<div>&nbsp;</div>"
                '
                ' Build FileColumns
                '
                Dim DefaultSourceFieldSelect As String = GenericController.getSourceFieldSelect(app, Filename, "Ignore")
                '
                ' Build the Database field list
                '
                Dim PeopleContentID As Integer = cp.Content.GetID("people")
                Dim ImportContentID As Integer = importData.dstContentId
                Dim ImportContentName As String = cp.Content.GetRecordName("content", ImportContentID)
                Dim DBFields() As String = Split(GenericController.getDbFieldList(cp, ImportContentName, False), ",")
                '
                ' Output the table
                '
                Content &= "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>"
                Content &= "" _
                    & "<TR>" _
                    & "<TD align=left>Data From</TD>" _
                    & "<TD align=left width=200>Set Value</TD>" _
                    & "<TD align=center width=10></TD>" _
                    & "<TD align=left width=200>Save Data To</TD>" _
                    & "<TD align=left width=200>Type</TD>" _
                    & "</TR>"
                Dim maxPtr As Integer = 0
                For Ptr As Integer = 0 To UBound(DBFields)
                    '
                    ' -- classes for each column
                    Dim cell0Style As String = ""
                    Dim cell1Style As String = ""
                    Dim cell2Style As String = ""
                    Dim cell3Style As String = ""
                    Dim cell4Style As String = ""
                    '
                    ' -- get field data
                    Dim DBFieldName As String = DBFields(Ptr)
                    Dim field As ContentFieldModel = Nothing
                    Dim fieldList As List(Of ContentFieldModel) = DbBaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(DBFieldName) & ")and(contentid=" & cp.Content.GetID(ImportContentName) & ")")
                    If fieldList.Count > 0 Then
                        field = fieldList.First()
                    End If
                    '
                    ' -- get row data specific to field type
                    Dim DbFieldType As String
                    Dim dataEditor As String = ""
                    If field Is Nothing Then
                        DbFieldType = "Text (255 char)"
                        dataEditor = "<input name="""" type=""text"">"
                    Else
                        Dim setValueInput As String = "setValueField" & field.id
                        Select Case field.type
                            Case FieldTypeBoolean
                                DbFieldType = "true/false"
                                dataEditor = "<input type=""checkbox"" name=""" & setValueInput & """ class=""js-import-manual-data"" style=""display:none;"">"
                                cell1Style &= "vertical-align:middle;text-align:center;"
                            Case FieldTypeCurrency, FieldTypeFloat
                                DbFieldType = "Number"
                                dataEditor = "<input type=""number"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case FieldTypeDate
                                DbFieldType = "Date"
                                dataEditor = "<input type=""date"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                DbFieldType = "Filename"
                                dataEditor = "<input type=""file"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case FieldTypeInteger
                                DbFieldType = "Integer"
                                dataEditor = "<input type=""number"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case FieldTypeLongText, FieldTypeHTML
                                DbFieldType = "Text (8000 char)"
                                dataEditor = "<input type=""text"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case FieldTypeLookup
                                DbFieldType = "Integer ID"
                                dataEditor = "<select name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;""><option name=""""></option></select>"
                            Case FieldTypeManyToMany
                                DbFieldType = "Integer ID"
                                dataEditor = ""
                            Case FieldTypeMemberSelect
                                DbFieldType = "Integer ID"
                                dataEditor = "<select name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;""><option name=""""></option></select>"
                            Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                DbFieldType = "Text (255 char)"
                                dataEditor = "<input type=""text"" name=""" & setValueInput & """ class=""form-control js-import-manual-data"" style=""display:none;"">"
                            Case Else
                                DbFieldType = "Invalid [" & field.type & "]"
                                dataEditor = ""
                        End Select
                    End If
                    Dim SourceFieldSelect As String = DefaultSourceFieldSelect.Replace("{{fieldId}}", field.id.ToString()).Replace("{{inputName}}", "SourceField" & Ptr)
                    Dim SourceFieldPtr As Integer = -1
                    If Not String.IsNullOrEmpty(DBFieldName) Then
                        '
                        ' Find match in current ImportMap
                        '
                        Dim lCaseDbFieldName As String = LCase(DBFieldName)
                        If ImportMap.mapPairCnt > 0 Then
                            Dim MapPtr As Integer
                            For MapPtr = 0 To ImportMap.mapPairCnt - 1
                                If lCaseDbFieldName = LCase(ImportMap.mapPairs(MapPtr).dbField) Then
                                    SourceFieldPtr = ImportMap.mapPairs(MapPtr).sourceFieldPtr
                                    Exit For
                                End If
                            Next
                        End If
                        If SourceFieldPtr = -1 Then
                            '
                            ' Find a default field - one that matches the DBFieldName if possible
                            '
                            Dim TestName As String
                            If app.sourceFieldCnt > 0 Then
                                For SourceFieldPtr = 0 To app.sourceFieldCnt - 1
                                    TestName = app.sourceFields(SourceFieldPtr)
                                    TestName = LCase(TestName)
                                    TestName = Replace(TestName, " ", "")
                                    '
                                    ' check for exact match
                                    '
                                    If lCaseDbFieldName = TestName Then
                                        Exit For
                                    End If
                                    '
                                    ' check for pseudo match
                                    '
                                    Select Case lCaseDbFieldName
                                        Case "zip"
                                            If (TestName = "postalcode") Or (TestName = "zip/postalcode") Then
                                                Exit For
                                            End If
                                        Case "firstname"
                                            If TestName = "first" Then
                                                Exit For
                                            End If
                                        Case "lastname"
                                            If TestName = "last" Then
                                                Exit For
                                            End If
                                        Case "email"
                                            If (TestName = "e-mail") Or (TestName = "emailaddress") Or (TestName = "e-mailaddress") Then
                                                Exit For
                                            End If
                                        Case "address"
                                            If (TestName = "address1") Or (TestName = "addressline1") Then
                                                Exit For
                                            End If
                                        Case "address2"
                                            If (TestName = "addressline2") Then
                                                Exit For
                                            End If
                                    End Select
                                Next
                            End If
                        End If
                        If SourceFieldPtr >= 0 Then
                            SourceFieldSelect = Replace(SourceFieldSelect, "value=""" & SourceFieldPtr & """>", "selected value=""" & SourceFieldPtr & """>", , , vbTextCompare)
                        Else
                            SourceFieldSelect = Replace(SourceFieldSelect, "value=""-1"">", "selected value=""-1"">", , , vbTextCompare)
                        End If
                    Else
                        DBFieldName = "[blank]"
                    End If
                    '
                    ' Now customize the caption for the DBField a little
                    '
                    Dim DBFieldCaption As String = DBFieldName
                    If Not cp.User.IsDeveloper Then
                        Select Case LCase(DBFieldCaption)
                            Case "id"
                                DBFieldCaption = "Contensive ID"
                        End Select
                    End If
                    Dim rowStyle As String
                    Dim cellClass As String = "text-align:center;vertical-align:middle;"
                    If Ptr Mod 2 = 0 Then
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;"
                    Else
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;"
                    End If
                    Content = Content _
                        & vbCrLf _
                        & "<TR>" _
                        & "<TD style=""" & cell0Style & rowStyle & """ align=left>" & SourceFieldSelect & "</td>" _
                        & "<TD style=""" & cell1Style & rowStyle & """ align=left>" & dataEditor & "</td>" _
                        & "<TD style=""" & cell2Style & rowStyle & """ align=center>&gt;&gt;</TD>" _
                        & "<TD style=""" & cell3Style & rowStyle & """ align=left>&nbsp;" & DBFieldCaption & "<input type=hidden name=DbField" & Ptr & " value=""" & DBFieldName & """></td>" _
                        & "<TD style=""" & cell4Style & rowStyle & """ align=left>&nbsp;" & DbFieldType & "</td>" _
                        & "</TR>"
                    maxPtr = Ptr
                Next
                Content &= "<input type=hidden name=Ccnt value=" & maxPtr & ">"
                Content &= "</TABLE>"
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdNewMapping.ToString)
                Return HtmlController.getWizardContent(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
