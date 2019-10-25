
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class MemberRuleModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Member Rules"      '<------ set content name
        Public Const contentTableName As String = "ccMemberRules"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property DateExpires As Date
        Public Property GroupID  as Integer
        Public Property MemberID  as Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As MemberRuleModel
            Return add(Of MemberRuleModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As MemberRuleModel
            Return create(Of MemberRuleModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As MemberRuleModel
            Return create(Of MemberRuleModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As MemberRuleModel
            Return createByName(Of MemberRuleModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of MemberRuleModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of MemberRuleModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of MemberRuleModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of MemberRuleModel)
            Return createList(Of MemberRuleModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of MemberRuleModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of MemberRuleModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of MemberRuleModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of MemberRuleModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of MemberRuleModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As MemberRuleModel
            Dim result As MemberRuleModel = DirectCast(Me.Clone(), MemberRuleModel)
            result.id = cp.Content.AddRecord(contentName)
            result.ccguid = cp.Utils.CreateGuid()
            result.save(cp)
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function
        '
        '===========================================================================
        ''' <summary>
        ''' Add the user to the group. If already in the group, leave user in. If not, add a new rule with no expiration date
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="MemberID"></param>
        ''' <param name="GroupID"></param>
        Public Shared Sub AddGroupMember(cp As CPBaseClass, MemberID As Integer, GroupID As Integer)
            Dim result As String = ""
            Try
                '
                '
                Dim sqlNow As String = cp.Db.EncodeSQLDate(Now)
                Dim memberRulelist As List(Of Models.MemberRuleModel) = Models.MemberRuleModel.createList(cp, "(MemberID=" & MemberID & ")and(GroupID=" & GroupID & ")and((dateexpires is null)or(dateexpires<" & sqlNow & "))")
                If (memberRulelist.Count = 0) Then
                    Dim memberRule = Models.MemberRuleModel.add(cp)
                    memberRule.MemberID = MemberID
                    memberRule.GroupID = GroupID
                    memberRule.save(cp)
                End If
            Catch ex As Exception
                Throw
            End Try

        End Sub

    End Class
End Namespace
