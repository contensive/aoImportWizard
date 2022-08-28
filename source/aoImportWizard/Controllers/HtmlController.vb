
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard.Controllers
    Public Class HtmlController
        '
        Public Shared Function getRadio(cp As CPBaseClass, radioName As String, radioValue As Integer, selectedValue As Integer, radioLabel As String) As String
            Dim uniqueId As String = "id" & cp.Utils.GetRandomInteger
            Dim result As String = "<label class=""form-check-label"" for=""" & uniqueId & """>" & radioLabel & "</label>"
            result = "<input class=""form-check-input"" type=""radio"" name=""" & radioName & """ id=""" & uniqueId & """ value=""" & radioValue & """ " & If(selectedValue = radioValue, " selected", "") & ">" & result
            result = "<div class=""ml-4 form-check"">" & result & "</div>"
            Return result
        End Function
        '
        '
        '
        '
        Private Shared Function wrapBody(cp As CPBaseClass, result As StringBuilder) As String
            '
            ' -- return
            Return cp.Html5.Div(cp.Html5.Form(result.ToString()), "import-wizard")
        End Function
        '
        '
        '
        Public Shared Function getWizardContent(cp As CPBaseClass, headerCaption As String, buttonCancel As String, buttonback2 As String, buttonContinue2 As String, description As String, WizardContent As String) As String
            Try
                Dim body As String = ""
                If String.IsNullOrEmpty(buttonback2) Then
                    body = "<div Class=""bg-white p-4"">" _
                            & cp.Html.h2(headerCaption) _
                            & cp.Html.div(description) _
                            & cp.Html.div(WizardContent) _
                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")

                Else
                    body = "<div Class=""bg-white p-4"">" _
                            & cp.Html.h2(headerCaption) _
                            & cp.Html.div(description) _
                            & cp.Html.div(WizardContent) _
                            & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", buttonback2) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")
                End If
                Return body
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get an html select with teh current content's fields
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ContentName"></param>
        ''' <param name="NoneCaption"></param>
        ''' <param name="AllowID"></param>
        ''' <returns></returns>
        Public Shared Function getDbFieldSelect(cp As CPBaseClass, ContentName As String, NoneCaption As String, AllowID As Boolean) As String
            Try
                '
                Dim result As String = "" _
                & "<select class=""form-control"" name=xxxx><option value="""" style=""Background-color:#E0E0E0;"">" & NoneCaption & "</option>" _
                & "<option>" & Replace(GenericController.getDbFieldList(cp, ContentName, AllowID), ",", "</option><option>") & "</option>" _
                & "</select>"
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace