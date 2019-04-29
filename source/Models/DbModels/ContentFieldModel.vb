
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class ContentFieldModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Content Fields"      '<------ set content name
        Public Const contentTableName As String = "ccFields"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AdminOnly As Boolean
        Public Property Authorable As Boolean
        Public Property Caption As String
        Public Property ContentID  as Integer
        Public Property createResourceFilesOnRoot As Boolean
        Public Property DefaultValue As String
        Public Property DeveloperOnly As Boolean
        Public Property editorAddonID  as Integer
        Public Property EditSortPriority  as Integer
        Public Property EditTab As String
        Public Property HTMLContent As Boolean
        Public Property IndexColumn  as Integer
        Public Property IndexSortDirection  as Integer
        Public Property IndexSortPriority  as Integer
        Public Property IndexWidth As String
        Public Property InstalledByCollectionID  as Integer
        Public Property IsBaseField As Boolean
        Public Property LookupContentID  as Integer
        Public Property LookupList As String
        Public Property ManyToManyContentID  as Integer
        Public Property ManyToManyRuleContentID  as Integer
        Public Property ManyToManyRulePrimaryField As String
        Public Property ManyToManyRuleSecondaryField As String
        Public Property MemberSelectGroupID  as Integer
        Public Property NotEditable As Boolean
        Public Property Password As Boolean
        Public Property prefixForRootResourceFiles As String
        'Public Property ReadOnly As Boolean
        Public Property RedirectContentID  as Integer
        Public Property RedirectID As String
        Public Property RedirectPath As String
        Public Property Required As Boolean
        Public Property RSSDescriptionField As Boolean
        Public Property RSSTitleField As Boolean
        Public Property Scramble As Boolean
        Public Property TextBuffered As Boolean
        Public Property Type  as Integer
        Public Property UniqueName As Boolean        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As ContentFieldModel
            Return add(Of ContentFieldModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As ContentFieldModel
            Return create(Of ContentFieldModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As ContentFieldModel
            Return create(Of ContentFieldModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As ContentFieldModel
            Return createByName(Of ContentFieldModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of ContentFieldModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of ContentFieldModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of ContentFieldModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of ContentFieldModel)
            Return createList(Of ContentFieldModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of ContentFieldModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of ContentFieldModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of ContentFieldModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of ContentFieldModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of ContentFieldModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As ContentFieldModel
            Dim result As ContentFieldModel = DirectCast(Me.Clone(), ContentFieldModel)
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
        '====================================================================================================
        ''' <summary>
        ''' return a field for the content name and field name
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="contentName"></param>
        ''' <param name="fieldName"></param>
        ''' <returns></returns>
        Public Shared Function getContentField(cp As CPBaseClass, contentName As String, fieldName As String) As Models.ContentFieldModel
            Dim emailContent As Models.ContentModel = ContentModel.create(cp, "email")
            If (emailContent IsNot Nothing) Then
                Dim fieldList As List(Of ContentFieldModel) = ContentFieldModel.createList(cp, "(contentid=" & emailContent.id & ")and(name=" & cp.Db.EncodeSQLText("testmemberid") & ")")
                If (fieldList.Count > 0) Then
                    Return fieldList(0)
                End If
            End If
            Return Nothing
        End Function
    End Class
End Namespace
