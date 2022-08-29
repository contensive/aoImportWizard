
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports C5BaseModel = Contensive.Models.Db.DbBaseModel

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectKeyView
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
                Dim importConfig = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                ImportMap.keyMethodID = cp.Doc.GetInteger(RequestNameImportKeyMethodID)
                ImportMap.sourceKeyField = cp.Doc.GetText(RequestNameImportSourceKeyFieldPtr)
                ImportMap.dbKeyField = cp.Doc.GetText(RequestNameImportDbKeyField)
                If Not String.IsNullOrEmpty(ImportMap.dbKeyField) Then
                    Dim fieldList As List(Of ContentFieldModel) = C5BaseModel.createList(Of ContentFieldModel)(cp, "(name=" & cp.Db.EncodeSQLText(ImportMap.dbKeyField) & ")and(contentid=" & cp.Content.GetID(ImportMap.contentName) & ")")
                    If (fieldList.Count > 0) Then
                        ImportMap.dbKeyFieldType = fieldList.First().type
                    End If
                End If
                ImportMap.save(app, importConfig)

                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back to mapping
                        Return viewIdNewMapping
                    Case ButtonContinue
                        '
                        ' -- continue to Select Group or finish
                        If importConfig.dstContentId = app.peopleContentid Then
                            Return viewIdSelectGroup
                        Else
                            Return viewIdFinish
                        End If
                End Select
                ''
                'Select Case Button
                '    Case ButtonBack2
                '        '
                '        ' -- back to select source
                '        Return viewIdSelectSource
                '    Case ButtonContinue2
                '        '
                '        ' -- upload the file and continue
                '        Dim Filename As String = app.cp.Doc.GetText(RequestNameImportUpload)
                '        If String.IsNullOrEmpty(Filename) Then Return viewIdSelectSource
                '        '
                '        Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                '        app.cp.TempFiles.SaveUpload(RequestNameImportUpload, "upload", importConfig.privateUploadPathFilename)
                '        importConfig.save(app)
                '        '
                '        Return viewIdSelectTable
                'End Select
                'Return viewIdUpload
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

                Dim KeyMethodID As Integer = cp.Utils.EncodeInteger(ImportMap.keyMethodID)
                If KeyMethodID = 0 Then
                    KeyMethodID = KeyMethodUpdateOnMatchInsertOthers
                End If






                Dim SourceKeyFieldPtr As Integer
                '
                If Not String.IsNullOrEmpty(ImportMap.sourceKeyField) Then
                    SourceKeyFieldPtr = cp.Utils.EncodeInteger(ImportMap.sourceKeyField)
                Else
                    SourceKeyFieldPtr = -1
                End If
                Dim Filename As String = importConfig.privateUploadPathFilename
                Dim SourceFieldSelect As String = Replace(HtmlController.getSourceFieldSelect(app, Filename, "Select One"), "xxxx", RequestNameImportSourceKeyFieldPtr)
                SourceFieldSelect = Replace(SourceFieldSelect, "value=" & SourceKeyFieldPtr, "value=" & SourceKeyFieldPtr & " selected", , , vbTextCompare)
                '
                'Dim PeopleContentID As Integer = CP.Content.GetID("people")
                Dim ImportContentID As Integer = importConfig.dstContentId
                'ImportContentName = cp.Content.GetRecordName("content", ImportContentID)
                Dim note As String

                Dim DBFieldSelect As String
                '
                ' Pick any field for key if developer or not the ccMembers table
                '
                Dim DbKeyField As String = ImportMap.dbKeyField
                Dim LookupContentName As String
                LookupContentName = cp.Content.GetRecordName("content", importConfig.dstContentId)
                DBFieldSelect = Replace(HtmlController.getDbFieldSelect(cp, LookupContentName, "Select One", True), "xxxx", RequestNameImportDbKeyField)
                DBFieldSelect = Replace(DBFieldSelect, ">" & DbKeyField & "<", " selected>" & DbKeyField & "<", , , vbTextCompare)
                note = ""
                '
                Dim Description As String = cp.Html.h4("Update Control") & cp.Html.p("When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted.")
                Dim Content As String = "" _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                    & "<TR><TD colspan=2>Key Fields</td></tr>" _
                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" _
                        & "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>" _
                        & "<tr><td>Imported&nbsp;Key&nbsp;</td><td>" & SourceFieldSelect & "</td></tr>" _
                        & "<tr><td>Database&nbsp;Key&nbsp;</td><td>" & DBFieldSelect & "</td></tr>" _
                        & "</table>" _
                        & "</td></tr>" _
                    & "<TR><TD colspan=2>Update Options</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodInsertAll.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Insert all imported data, regardless of key field.</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatchInsertOthers.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Update database records when the data in the key fields match. Insert all other imported data.</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportKeyMethodID, KeyMethodUpdateOnMatch.ToString, KeyMethodID.ToString) & "</td><td width=99% align=left>Update database records when the data in the key fields match. Ignore all other imported data.</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                Content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectKey.ToString)
                Return HtmlController.createLayout(cp, headerCaption, Description, Content, True, True, True, True)
                'Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
