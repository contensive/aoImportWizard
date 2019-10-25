
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports System.Reflection

Namespace Models.Complex
    Public MustInherit Class baseComplexModel
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="cs"></param>
        Private Shared Function loadRecord(Of T As baseComplexModel)(cp As CPBaseClass, cs As CPCSBaseClass) As T
            Dim instance As T = Nothing
            Try
                If cs.OK() Then
                    Dim instanceType As Type = GetType(T)
                    instance = DirectCast(Activator.CreateInstance(instanceType), T)
                    For Each resultProperty As PropertyInfo In instance.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                        Select Case resultProperty.Name.ToLower()
                            Case "specialcasefield"
                            Case "sortorder"
                                '
                                ' -- customization for pc, could have been in default property, db default, etc.
                                Dim sortOrder As String = cs.GetText(resultProperty.Name)
                                If (String.IsNullOrEmpty(sortOrder)) Then
                                    sortOrder = "9999"
                                End If
                                resultProperty.SetValue(instance, sortOrder, Nothing)
                            Case Else
                                Select Case resultProperty.PropertyType.Name
                                    Case "Int32"
                                        resultProperty.SetValue(instance, cs.GetInteger(resultProperty.Name), Nothing)
                                    Case "Boolean"
                                        resultProperty.SetValue(instance, cs.GetBoolean(resultProperty.Name), Nothing)
                                    Case "DateTime"
                                        resultProperty.SetValue(instance, cs.GetDate(resultProperty.Name), Nothing)
                                    Case "Double"
                                        resultProperty.SetValue(instance, cs.GetNumber(resultProperty.Name), Nothing)
                                    Case Else
                                        resultProperty.SetValue(instance, cs.GetText(resultProperty.Name), Nothing)
                                End Select
                        End Select
                    Next
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return instance
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        Protected Shared Function createListFromSql(Of T As baseComplexModel)(cp As CPBaseClass, sql As String, pageSize  as Integer, pageNumber  as Integer) As List(Of T)
            Dim result As New List(Of T)
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim ignoreCacheNames As New List(Of String)
                Dim instanceType As Type = GetType(T)
                If (cs.OpenSQL(sql, "", pageSize, pageNumber)) Then
                    Dim instance As T
                    Do
                        instance = loadRecord(Of T)(cp, cs)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.GoNext()
                    Loop While cs.OK()
                End If
                cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
