
Imports System.Linq
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models
Imports Contensive.Models.Db

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectGroupView
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

                Dim newGroupID As Integer
                Dim newGroupName As String = cp.Doc.GetText(RequestNameImportGroupNew)
                If Not String.IsNullOrEmpty(newGroupName) Then
                    Dim groupmodellist As List(Of GroupModel) = DbBaseModel.createList(Of GroupModel)(cp, "(name=" & cp.Db.EncodeSQLText(newGroupName) & ")")
                    If (groupmodellist.Count <> 0) Then
                        Dim newGroup As GroupModel = groupmodellist.First
                        newGroupID = newGroup.id
                    End If
                    If newGroupID = 0 Then
                        Dim newGroup = DbBaseModel.addDefault(Of GroupModel)(cp)
                        newGroup.name = newGroupName
                        newGroup.caption = newGroupName
                        newGroupID = newGroup.id
                        newGroup.save(cp)
                    End If

                End If
                Dim importConfig = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                If newGroupID <> 0 Then
                    ImportMap.groupID = newGroupID
                Else
                    ImportMap.groupID = cp.Doc.GetInteger(RequestNameImportGroupID)
                End If
                ImportMap.groupOptionID = cp.Doc.GetInteger(RequestNameImportGroupOptionID)
                ImportMap.save(app, importConfig)
                '
                Select Case Button
                    Case ButtonBack
                        '
                        ' -- back to select key
                        Return viewIdSelectKey
                    Case Else
                        '
                        ' -- continue to finish
                        Return viewIdFinish
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
                Dim description As String = ""
                Dim content As String = ""
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim ImportMap As ImportMapModel = ImportMapModel.create(cp, importConfig.importMapPathFilename)
                '
                Dim GroupOptionID = ImportMap.groupOptionID
                If GroupOptionID = 0 Then
                    GroupOptionID = GroupOptionNone
                End If
                description = cp.Html.h4("Group Membership") & cp.Html.p("When your data is imported, people can be added to a group automatically. Select the option below, and a group.")
                content = "" _
                    & "<div>" _
                    & "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" _
                    & "<TR><TD colspan=2>Group Options</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionNone.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Do not add to a group.</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionAll.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add everyone to the the group.</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys match.</td></tr>" _
                    & "<TR><TD width=10>" & cp.Html.RadioBox(RequestNameImportGroupOptionID, GroupOptionOnNoMatch.ToString, GroupOptionID.ToString) & "</td><td width=99% align=left>Add to the group if keys do NOT match.</td></tr>" _
                    & "<TR><TD colspan=2>Add to Existing Group</td></tr>" _
                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & cp.Html.SelectContent(RequestNameImportGroupID, ImportMap.groupID.ToString, "Groups", "", "", "form-control") & "</td></tr>" _
                    & "<TR><TD colspan=2>Create New Group</td></tr>" _
                    & "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" & cp.Html.InputText(RequestNameImportGroupNew, "", 100, "form-control") & "</td></tr>" _
                    & "</table>" _
                    & "</div>" _
                    & ""
                content &= cp.Html.Hidden(rnSrcViewId, viewIdSelectGroup.ToString)
                Return HtmlController.createLayout(cp, headerCaption, description, content, True, True, True, True)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
