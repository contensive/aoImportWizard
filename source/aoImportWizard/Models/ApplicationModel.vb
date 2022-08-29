
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Controllers

Namespace Contensive.ImportWizard.Models
    '
    '====================================================================================================
    ''' <summary>
    ''' Use for access to global non-shared methods and aside caching
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ApplicationModel
        Implements IDisposable
        '
        ' privates passed in, do not dispose
        '
        Public ReadOnly cp As CPBaseClass
        '
        Public ReadOnly Property peopleContentid As Integer
            Get
                If peopleContentid_Local IsNot Nothing Then Return CInt(peopleContentid_Local)
                peopleContentid_Local = cp.Content.GetID("people")
                Return CInt(peopleContentid_Local)
            End Get
        End Property
        Private peopleContentid_Local As Integer? = Nothing
        '
        'Public ReadOnly Property wizard As New WizardType
        Public Property sourceFieldCnt As Integer
        Public Property uploadFields As String()
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


        '
        '====================================================================================================
        ''' <summary>
        ''' Load the sourceField and sourceFieldCnt from a wizard file
        ''' </summary>
        ''' <param name="Filename"></param>
        Public Sub loadUploadFields(Filename As String)
            Try
                Dim FileData As String
                Dim ignoreLong As Integer
                Dim ignoreBoolean As Boolean
                Dim foundFirstName As Boolean = False
                Dim foundLastName As Boolean = False
                Dim foundName As Boolean = False
                '
                If Not String.IsNullOrEmpty(Filename) Then
                    If sourceFieldCnt = 0 Then
                        FileData = cp.PrivateFiles.Read(Filename)
                        If Not String.IsNullOrEmpty(FileData) Then
                            '
                            ' Build FileColumns
                            '
                            Call GenericController.parseLine(FileData, 1, uploadFields, ignoreLong, ignoreBoolean)
                            '
                            ' todo - implement new fields to allow name/firstname/lastname population
                            'For Each field As String In sourceFields
                            '    foundFirstName = foundFirstName Or field.ToLowerInvariant().Equals("firstname") Or field.ToLowerInvariant().Equals("first name")
                            '    foundLastName = foundLastName Or field.ToLowerInvariant().Equals("lastname") Or field.ToLowerInvariant().Equals("last name")
                            '    foundName = foundName Or field.ToLowerInvariant().Equals("name")
                            'Next
                            'If (foundName And Not foundFirstName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("Name-first-half]")
                            'End If
                            'If (foundName And Not foundLastName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("Name-last-half")
                            'End If
                            'If (Not foundName And foundFirstName And foundLastName) Then
                            '    '
                            '    ' -- add firstname and lastname from name
                            '    sourceFields.Append("First-Name Last-Name")
                            'End If
                            sourceFieldCnt = UBound(uploadFields) + 1
                        End If
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub




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
