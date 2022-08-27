
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

    End Class
End Namespace