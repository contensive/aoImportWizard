﻿
Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard.Models
    Public Class ContentFieldModel
        Inherits Contensive.Models.Db.ContentFieldModel
        '
        Public Shared Function getFieldType(cp As CPBaseClass, dbFieldName As String, contentId As Integer) As Integer
            Try
                Using dt As DataTable = cp.Db.ExecuteQuery("select top 1 type from ccfields where (active>0)and(name=" & cp.Db.EncodeSQLText(dbFieldName) & ")And(contentid=" & contentId & ")")
                    If dt?.Rows IsNot Nothing AndAlso dt.Rows.Count > 0 Then Return cp.Utils.EncodeInteger(dt.Rows(0)(0))
                End Using
                Return 0
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get the database field list for this content
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ContentName"></param>
        ''' <param name="AllowID"></param>
        ''' <returns></returns>
        Public Shared Function getDbFieldList(cp As CPBaseClass, ContentName As String, AllowID As Boolean) As String
            Try
                Dim result As String = "," & cp.Content.GetProperty(ContentName, "SELECTFIELDLIST") & ","
                If Not AllowID Then
                    result = Replace(result, ",ID,", ",", , , vbTextCompare)
                End If
                result = Replace(result, ",CONTENTCONTROLID,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITSOURCEID,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITBLANK,", ",", , , vbTextCompare)
                result = Replace(result, ",EDITARCHIVE,", ",", , , vbTextCompare)
                result = Replace(result, ",DEVELOPER,", ",", , , vbTextCompare)
                result = Mid(result, 2, Len(result) - 2)
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace