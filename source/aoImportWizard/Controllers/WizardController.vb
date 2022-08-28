
'Imports Contensive.ImportWizard.Models
'Imports Contensive.BaseClasses

'Namespace Contensive.ImportWizard.Controllers
'    Public Class WizardController
'        '
'        '=====================================================================================
'        ''' <summary>
'        ''' Get the html for the current wizard form
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="headerCaption"></param>
'        ''' <param name="buttonCancel"></param>
'        ''' <param name="buttonback2"></param>
'        ''' <param name="buttonContinue2"></param>
'        ''' <param name="description"></param>
'        ''' <param name="WizardContent"></param>
'        ''' <returns></returns>
'        Public Shared Function getWizardContent(cp As CPBaseClass, headerCaption As String, buttonCancel As String, buttonback2 As String, buttonContinue2 As String, description As String, WizardContent As String) As String
'            Try
'                Dim body As String = ""
'                If String.IsNullOrEmpty(buttonback2) Then
'                    body = "<div Class=""bg-white p-4"">" _
'                            & cp.Html.h2(headerCaption) _
'                            & cp.Html.div(description) _
'                            & cp.Html.div(WizardContent) _
'                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")

'                Else
'                    body = "<div Class=""bg-white p-4"">" _
'                            & cp.Html.h2(headerCaption) _
'                            & cp.Html.div(description) _
'                            & cp.Html.div(WizardContent) _
'                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonback2) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")
'                End If
'                Return body
'            Catch ex As Exception
'                cp.Site.ErrorReport(ex)
'                Throw
'            End Try
'        End Function
'        '
'        '=====================================================================================
'        ''' <summary>
'        ''' Clear the wizard
'        ''' </summary>
'        ''' <param name="cp"></param>
'        Public Shared Sub clearWizardValues(cp As CPBaseClass)
'            Try
'                Call cp.Db.ExecuteNonQuery("delete from ccProperties where name Like 'ImportWizard.%' and typeid=1 and keyid=" & cp.Visit.Id)
'            Catch ex As Exception
'                cp.Site.ErrorReport(ex)
'                Throw
'            End Try
'        End Sub

'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Load the wizard variables, and build the .Path used for next and previous calls. If the WizardId property is set, load it and use it. If no Wizard ID, use the Group wizard by default. If during form processing, the wizard changes, the process must save the new wizardid
'        ''' </summary>
'        ''' <param name="app"></param>
'        Public Shared Sub loadWizardPath(app As ApplicationModel)
'            Dim cp = app.cp
'            Try
'                Dim ImportWizardID As Integer
'                Dim EmailCID As Integer
'                '
'                ' Get the saved ImportWizardID
'                ImportWizardID = getWizardInteger(cp, RequestNameImportWizardID, 0)
'                '
'                If ImportWizardID = 0 Then
'                    '
'                    ' Default Wizard, for any type of email, nothing disabled
'                    EmailCID = cp.Content.GetRecordID("content", "Email Templates")
'                    app.wizard.GroupFormInstructions = "Select Group"
'                    app.wizard.KeyFormInstructions = "Select the key field"
'                    app.wizard.MappingFormInstructions = "Set Mapping"
'                    app.wizard.SourceFormInstructions = "Select the source"
'                    app.wizard.UploadFormInstructions = "Upload the file"
'                    '
'                End If
'                '
'                ' Build Wizard path from path properties
'                '
'                With app.wizard
'                    .PathCnt = 0
'                    ReDim .Path(viewIdMax)
'                    '
'                    .Path(.PathCnt) = viewIdSelectSource
'                    .PathCnt = .PathCnt + 1

'                    Select Case getWizardInteger(cp, RequestNameImportSource, ImportSourceUpload)
'                        Case ImportSourceUpload
'                            '
'                            '
'                            '
'                            .Path(.PathCnt) = viewIdUpload
'                            .PathCnt = .PathCnt + 1
'                        Case Else
'                            '
'                            '
'                            '
'                            .Path(.PathCnt) = viewIdSelectFile
'                            .PathCnt = .PathCnt + 1
'                    End Select
'                    '
'                    '
'                    '
'                    .Path(.PathCnt) = viewIdSelectTable
'                    .PathCnt = .PathCnt + 1
'                    '
'                    '
'                    '
'                    .Path(.PathCnt) = viewIdNewMapping
'                    .PathCnt = .PathCnt + 1
'                    '
'                    '
'                    '
'                    .Path(.PathCnt) = viewIdSelectKey
'                    .PathCnt = .PathCnt + 1
'                    '
'                    '
'                    '
'                    If getWizardInteger(cp, RequestNameImportContentID, 0) = cp.Content.GetID("people") Then
'                        '
'                        ' if importing into people, get them the option of adding to a group
'                        '
'                        .Path(.PathCnt) = viewIdSelectGroup
'                        .PathCnt = .PathCnt + 1
'                    End If
'                    '
'                    ' Finish
'                    '
'                    .Path(.PathCnt) = viewIdFinish
'                    .PathCnt = .PathCnt + 1
'                    '
'                    ' Done, thank you
'                    '
'                    .Path(.PathCnt) = viewIdDone
'                    .PathCnt = .PathCnt + 1
'                End With
'                '
'            Catch ex As Exception
'                cp.Site.ErrorReport(ex)
'                Throw
'            End Try
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' save a wizard visit property
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="Name"></param>
'        ''' <param name="Value"></param>
'        Public Shared Sub saveWizardValue(cp As CPBaseClass, Name As String, Value As String)
'            Call cp.Visit.SetProperty("ImportWizard." & Name, Value)
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Get a wizard visit property
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="Name"></param>
'        ''' <param name="DefaultValue"></param>
'        ''' <returns></returns>
'        Public Shared Function getWizardText(cp As CPBaseClass, Name As String, DefaultValue As String) As String
'            Return cp.Visit.GetText("ImportWizard." & Name, DefaultValue)
'        End Function
'        Public Shared Function getWizardInteger(cp As CPBaseClass, Name As String, DefaultValue As Integer) As Integer
'            Return cp.Visit.GetInteger("ImportWizard." & Name, DefaultValue)
'        End Function
'        Public Shared Function getWizardBoolean(cp As CPBaseClass, Name As String, DefaultValue As Boolean) As Boolean
'            Return cp.Visit.GetBoolean("ImportWizard." & Name, DefaultValue)
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' save a wizard value from the current request
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="RequestName"></param>
'        Public Shared Sub saveWizardRequestInteger(cp As CPBaseClass, RequestName As String)
'            Call saveWizardValue(cp, RequestName, cp.Doc.GetText(RequestName))
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' save a wizard value from the current request
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="RequestName"></param>
'        Public Shared Sub saveWizardRequestText(cp As CPBaseClass, RequestName As String)
'            Call saveWizardValue(cp, RequestName, cp.Doc.GetText(RequestName))
'        End Sub
'        '
'    End Class
'End Namespace