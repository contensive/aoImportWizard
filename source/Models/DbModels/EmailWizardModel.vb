
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class EmailWizardModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Email Wizards"      '<------ set content name
        Public Const contentTableName As String = "ccEmailWizards"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AllowSpamFooterDefault As Boolean
        Public Property ContentFormInstructions As String
        Public Property DefaultConditionPeriod  as Integer
        Public Property DefaultContent As String
        Public Property DefaultTemplateID  as Integer
        Public Property EmailTypeID  as Integer
        Public Property GroupFormInstructions As String
        Public Property IncludeAllowSpamFooter As Boolean
        Public Property IncludeConditionPeriod As Boolean
        Public Property IncludeContentForm As Boolean
        Public Property IncludeGroupForm As Boolean
        Public Property IncludeLinkAuthentication As Boolean
        Public Property IncludeSchedule As Boolean
        Public Property IncludeTemplateForm As Boolean
        Public Property LinkAuthenticationDefault As Boolean
        Public Property TemplateFormInstructions As String



        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As EmailWizardModel
            Return add(Of EmailWizardModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As EmailWizardModel
            Return create(Of EmailWizardModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As EmailWizardModel
            Return create(Of EmailWizardModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As EmailWizardModel
            Return createByName(Of EmailWizardModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of EmailWizardModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of EmailWizardModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of EmailWizardModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of EmailWizardModel)
            Return createList(Of EmailWizardModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of EmailWizardModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of EmailWizardModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of EmailWizardModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of EmailWizardModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of EmailWizardModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As EmailWizardModel
            Dim result As EmailWizardModel = DirectCast(Me.Clone(), EmailWizardModel)
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

    End Class
End Namespace
