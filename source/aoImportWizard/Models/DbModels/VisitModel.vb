
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class VisitModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Visits"      '<------ set content name
        Public Const contentTableName As String = "ccVisits"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property Bot As Boolean
        Public Property Browser As String
        Public Property CookieSupport As Boolean
        Public Property ExcludeFromAnalytics As Boolean
        Public Property HTTP_FROM As String
        Public Property HTTP_REFERER As String
        Public Property HTTP_VIA As String
        Public Property LastVisitTime As Date
        Public Property LoginAttempts  as Integer
        Public Property MemberID  as Integer
        Public Property MemberNew As Boolean
        Public Property Mobile As Boolean
        Public Property PageVisits  as Integer
        Public Property RefererPathPage As String
        Public Property REMOTE_ADDR As String
        Public Property RemoteName As String
        Public Property StartDateValue  as Integer
        Public Property StartTime As Date
        Public Property StopTime As Date
        Public Property TimeToLastHit  as Integer
        Public Property VerboseReporting As Boolean
        Public Property VisitAuthenticated As Boolean
        Public Property VisitorID  as Integer
        Public Property VisitorNew As Boolean


        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As VisitModel
            Return add(Of VisitModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As VisitModel
            Return create(Of VisitModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As VisitModel
            Return create(Of VisitModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As VisitModel
            Return createByName(Of VisitModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of VisitModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of VisitModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of VisitModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of VisitModel)
            Return createList(Of VisitModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of VisitModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of VisitModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of VisitModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of VisitModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of VisitModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As VisitModel
            Dim result As VisitModel = DirectCast(Me.Clone(), VisitModel)
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
