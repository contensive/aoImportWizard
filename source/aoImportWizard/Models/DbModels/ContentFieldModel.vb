
Option Explicit On
Option Strict On

Imports Contensive.Models.Db
Imports Contensive.BaseClasses

Namespace Models
    Public Class ContentFieldModel
        Inherits DbBaseModel
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
            Dim content As ContentModel = DbBaseModel.createByUniqueName(Of ContentModel)(cp, contentName)
            If (content IsNot Nothing) Then
                Dim fieldList As List(Of ContentFieldModel) = ContentFieldModel.createList(Of ContentFieldModel)(cp, "(contentid=" & content.id & ")and(name=" & cp.Db.EncodeSQLText(fieldName) & ")")
                If (fieldList.Count > 0) Then
                    Return fieldList(0)
                End If
            End If
            Return Nothing
        End Function
    End Class
End Namespace
