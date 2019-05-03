
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses

Namespace Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class applicationController
        Implements IDisposable
        '
        ' privates passed in, do not dispose
        '
        Private cp As CPBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Errors accumulated during rendering.
        ''' </summary>
        ''' <returns></returns>
        Public Property packageErrorList As New List(Of packageErrorClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' data accumulated during rendering
        ''' </summary>
        ''' <returns></returns>
        Public Property packageNodeList As New List(Of packageNodeClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' list of name/time used to performance analysis
        ''' </summary>
        ''' <returns></returns>
        Public Property packageProfileList As New List(Of packageProfileClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' get the serialized results
        ''' </summary>
        ''' <returns></returns>
        Public Function getSerializedPackage() As String
            Dim result As String = ""
            Try
                result = serializeObject(cp, New packageClass With {
                    .success = packageErrorList.Count.Equals(0),
                    .nodeList = packageNodeList,
                    .errorList = packageErrorList,
                    .profileList = packageProfileList
                })
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            '
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cp As CPBaseClass, Optional requiresAuthentication As Boolean = True)
            Me.cp = cp
            Dim sql As String = ""
            Dim cs As CPCSBaseClass = cp.CSNew()
            Dim localSystemStatus As String = ""
            If (requiresAuthentication And Not cp.User.IsAuthenticated) Then
                packageErrorList.Add(New packageErrorClass() With {.number = resultErrorEnum.errAuthentication, .description = "Authorization is required."})
                cp.Response.SetStatus(httpErrorEnum.forbidden & " Forbidden")
            End If
        End Sub
        '
        Public Shared Function serializeObject(ByVal CP As CPBaseClass, ByVal dataObject As Object) As String
            Dim result As String = ""
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                result = json_serializer.Serialize(dataObject)
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' list of events and their stopwatch times
        ''' </summary>
        Public Class packageProfileClass
            Public name As String
            Public time as Integer
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' remote method top level data structure
        ''' </summary>
        <Serializable()>
        Public Class packageClass
            Public success As Boolean = False
            Public errorList As New List(Of packageErrorClass)
            Public nodeList As New List(Of packageNodeClass)
            Public profileList As List(Of packageProfileClass)
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' data store for jsonPackage
        ''' </summary>
        <Serializable()>
        Public Class packageNodeClass
            Public dataFor As String = ""
            Public data As Object ' IEnumerable(Of Object)
        End Class
        '
        '====================================================================================================
        ''' <summary>
        ''' error list for jsonPackage
        ''' </summary>
        <Serializable()>
        Public Class packageErrorClass
            Public number  as Integer = 0
            Public description As String = ""
        End Class
        '
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
End Namespace
