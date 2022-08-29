
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

Namespace Contensive.ImportWizard.Controllers
    Public Class HtmlController

        '
        '====================================================================================================
        ''' <summary>
        ''' get an html select with all the fields from the uploaded source data
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="filename"></param>
        ''' <param name="noneCaption"></param>
        ''' <returns></returns>
        Public Shared Function getSourceFieldSelect(app As ApplicationModel, filename As String, noneCaption As String, dstContentId As Integer, allowControlFields As Boolean) As String
            Try
                Dim cp As CPBaseClass = app.cp
                If String.IsNullOrEmpty(filename) Then Return String.Empty
                Call app.loadUploadFields(filename)
                If app.sourceFieldCnt.Equals(0) Then Return String.Empty
                '
                ' Build FileColumns
                '
                Dim result As String = ""
                result = "<select name={{inputName}} class=""form-control js-import-select"" id=""js-import-select-{{fieldPtr}}"">"
                result &= "<option value=""-1"">" & noneCaption & "</option>"
                For Ptr As Integer = 0 To app.sourceFieldCnt - 1
                    result &= "<option value=""" & Ptr & """>column " & (Ptr + 1) & " (" & If(String.IsNullOrEmpty(app.uploadFields(Ptr)), "[blank]", app.uploadFields(Ptr)) & ")</option>"
                Next
                If allowControlFields Then
                    result &= "<option value=""-2"">Set value manually</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-3"">Set to firstname + lastname</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-4"">Set to firstname from name field</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-5"">Set to lastname from name field</option>"
                End If
                result &= "</select>"
                Return result
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        Public Shared Function getRadio(cp As CPBaseClass, radioName As String, radioValue As Integer, selectedValue As Integer, radioLabel As String) As String
            Dim uniqueId As String = "id" & cp.Utils.GetRandomInteger
            Dim result As String = "<label class=""form-check-label"" for=""" & uniqueId & """>" & radioLabel & "</label>"
            result = "<input class=""form-check-input"" type=""radio"" name=""" & radioName & """ id=""" & uniqueId & """ value=""" & radioValue & """ " & If(selectedValue = radioValue, " checked", "") & ">" & result
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
        ''
        ''
        ''
        'Public Shared Function createLayout(cp As CPBaseClass, headerCaption As String, buttonCancel As String, buttonback2 As String, buttonContinue2 As String, description As String, WizardContent As String) As String
        '    Try
        '        Dim body As String = ""
        '        If String.IsNullOrEmpty(buttonback2) Then
        '            body = "<div Class=""bg-white p-4"">" _
        '                    & cp.Html.h2(headerCaption) _
        '                    & cp.Html.div(description) _
        '                    & cp.Html.div(WizardContent) _
        '                    & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", ButtonReset) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")

        '        Else
        '            body = "<div Class=""bg-white p-4"">" _
        '                    & cp.Html.h2(headerCaption) _
        '                    & cp.Html.div(description) _
        '                    & cp.Html.div(WizardContent) _
        '                    & cp.Html.div(cp.Html.Button("button", buttonCancel) & cp.Html.Button("button", ButtonReset) & cp.Html.Button("button", buttonback2) & cp.Html.Button("button", buttonContinue2), "", "p-2 bg-secondary")
        '        End If
        '        Return body
        '    Catch ex As Exception
        '        cp.Site.ErrorReport(ex)
        '        Throw
        '    End Try
        'End Function
        '
        Public Shared Function createLayout(cp As CPBaseClass, header As String, description As String, body As String, allowCancel As Boolean, allowRestart As Boolean, allowBack As Boolean, allowContinue As Boolean) As String
            Try
                Return "" _
                    & cp.Html.div("" _
                        & cp.Html.h2(header) _
                        & cp.Html.div(description) _
                        & cp.Html.div(body, "", "mt-4") _
                        & cp.Html.div("" _
                            & If(allowCancel, cp.Html.Button("button", ButtonCancel, "mr-2"), "") _
                            & If(allowRestart, cp.Html.Button("button", ButtonRestart, "mr-2"), "") _
                            & If(allowBack, cp.Html.Button("button", ButtonBack, "mr-2"), "") _
                            & If(allowContinue, cp.Html.Button("button", ButtonContinue, "mr-2"), ""), "", "mt-4 p-2 bg-secondary") _
                        & "", "", "bg-white p-4")
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
                & "<option>" & Replace(ContentFieldModel.getDbFieldList(cp, ContentName, AllowID), ",", "</option><option>") & "</option>" _
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