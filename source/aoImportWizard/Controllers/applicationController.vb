
Imports Contensive.BaseClasses

Namespace Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' Use for access to global non-shared methods and aside caching
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ApplicationController
        Implements IDisposable
        '
        ' privates passed in, do not dispose
        '
        Private ReadOnly cp As CPBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Errors accumulated during rendering.
        ''' </summary>
        ''' <returns></returns>
        Public Property packageErrorList As New List(Of PackageErrorClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' data accumulated during rendering
        ''' </summary>
        ''' <returns></returns>
        Public Property packageNodeList As New List(Of PackageNodeClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' list of name/time used to performance analysis
        ''' </summary>
        ''' <returns></returns>
        Public Property packageProfileList As New List(Of PackageProfileClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' get the serialized results
        ''' </summary>
        ''' <returns></returns>
        Public Function getSerializedPackage() As String
            Try
                Dim result As String = serializeObject(cp, New PackageClass With {
                    .success = packageErrorList.Count.Equals(0),
                    .nodeList = packageNodeList,
                    .errorList = packageErrorList,
                    .profileList = packageProfileList
                })
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cp As CPBaseClass, Optional requiresAuthentication As Boolean = True)
            Me.cp = cp
            If requiresAuthentication And Not cp.User.IsAuthenticated Then
                packageErrorList.Add(New PackageErrorClass() With {.number = ResultErrorEnum.errAuthentication, .description = "Authorization is required."})
                cp.Response.SetStatus(HttpErrorEnum.forbidden & " Forbidden")
            End If
        End Sub
        '
        Public Shared Function serializeObject(ByVal CP As CPBaseClass, ByVal dataObject As Object) As String
            Try
                Dim result As String = ""
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                result = json_serializer.Serialize(dataObject)
                Return result
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' list of events and their stopwatch times
    ''' </summary>
    Public Class PackageProfileClass
        Public Property name As String
        Public Property time As Long
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' remote method top level data structure
    ''' </summary>
    <Serializable()>
    Public Class PackageClass
        Public Property success As Boolean = False
        Public Property errorList As New List(Of PackageErrorClass)
        Public Property nodeList As New List(Of PackageNodeClass)
        Public Property profileList As List(Of PackageProfileClass)
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' data store for jsonPackage
    ''' </summary>
    <Serializable()>
    Public Class PackageNodeClass
        Public Property dataFor As String = ""
        Public Property data As Object ' IEnumerable(Of Object)
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' error list for jsonPackage
    ''' </summary>
    <Serializable()>
    Public Class PackageErrorClass
        Public Property number As Integer = 0
        Public Property description As String = ""
    End Class
    '
End Namespace
