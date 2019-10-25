
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models.Complex     '<------ set namespace
    Public Class xxxxxmodelNameGoesHerexxxxx        '<------ set set model Name and everywhere that matches this string
        Inherits baseComplexModel
        '
        '====================================================================================================
        '-- const
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property personName As String
        Public Property organizationName As String
        '
        '====================================================================================================
        ''' <summary>
        ''' get a list of objects matching the organizationId
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="organizationId"></param>
        ''' <param name="pageSize"></param>
        ''' <param name="pageNumber"></param>
        ''' <returns></returns>
        Public Overloads Shared Function createList(cp As CPBaseClass, organizationId  as Integer, Optional pageSize  as Integer = 999999, Optional pageNumber  as Integer = 1) As List(Of xxxxxmodelNameGoesHerexxxxx)
            Dim sql As String = My.Resources.sampleSql.Replace("{0}", organizationId.ToString())
            Return createListFromSql(Of xxxxxmodelNameGoesHerexxxxx)(cp, sql, pageSize, pageNumber)
        End Function

    End Class
End Namespace
