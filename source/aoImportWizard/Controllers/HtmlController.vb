
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
        Public Shared Function getSourceFieldSelect(app As ApplicationModel, filename As String, noneCaption As String, dstContentId As Integer, allowControlFields As Boolean, selectedValue As Integer, inputName As String, htmlId As String) As String
            Try
                Dim cp As CPBaseClass = app.cp
                If String.IsNullOrEmpty(filename) Then Return String.Empty
                Call app.loadUploadFields(filename)
                If app.sourceFieldCnt.Equals(0) Then Return String.Empty
                '
                ' Build FileColumns
                '
                Dim result As String = ""
                result = "<select name=""" & cp.Utils.EncodeHTML(inputName) & """ class=""form-control js-import-select"" id=""" & htmlId & """>"
                result &= "<option value=""-1"">" & noneCaption & "</option>"
                For ptr As Integer = 0 To app.sourceFieldCnt - 1
                    result &= "<option value=""" & ptr & """" & If(ptr = selectedValue, " selected", "") & ">column " & (ptr + 1) & " (" & If(String.IsNullOrEmpty(app.uploadFields(ptr)), "[blank]", app.uploadFields(ptr)) & ")</option>"
                Next
                If allowControlFields Then
                    result &= "<option value=""-2""" & If(-2 = selectedValue, " selected", "") & ">Set value manually</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-3""" & If(-3 = selectedValue, " selected", "") & ">Set to firstname + lastname</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-4""" & If(-4 = selectedValue, " selected", "") & ">Set to firstname from name field</option>"
                    If dstContentId = app.peopleContentid Then result &= "<option value=""-5""" & If(-5 = selectedValue, " selected", "") & ">Set to lastname from name field</option>"
                End If
                result &= "</select>"
                Return result
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        Public Shared Function getRadio(cp As CPBaseClass, radioName As String, radioValue As Integer, selectedValue As Integer, radioLabel As String, htmlId As String) As String
            Dim uniqueId As String = "id" & cp.Utils.GetRandomInteger
            Dim result As String = "<label class=""form-check-label"" for=""" & uniqueId & """>" & radioLabel & "</label>"
            result = "<input class=""form-check-input"" type=""radio"" name=""" & radioName & """ id=""" & uniqueId & """ value=""" & radioValue & """ " & If(selectedValue = radioValue, " checked", "") & ">" & result
            result = "<div class=""ml-4 form-check"" id=""" & htmlId & """>" & result & "</div>"
            Return result
        End Function
        '
        Public Shared Function createLayout(cp As CPBaseClass, header As String, description As String, body As String, allowCancel As Boolean, allowRestart As Boolean, allowBack As Boolean, allowContinue As Boolean) As String
            Try

                Dim layout As New Contensive.Addons.PortalFramework.LayoutBuilderSimple With {
                    .title = header,
                    .description = description,
                    .body = body,
                    .includeBodyPadding = True,
                    .includeBodyColor = False,
                    .isOuterContainer = True
                    }
                If allowCancel Then layout.addFormButton(ButtonCancel)
                If allowRestart Then layout.addFormButton(ButtonRestart)
                If allowBack Then layout.addFormButton(ButtonBack)
                If allowContinue Then layout.addFormButton(ButtonContinue)
                Return layout.getHtml(cp)


                'Dim buttonBar As String = cp.Html.div("" _
                '    & If(allowCancel, cp.Html.Button("button", ButtonCancel, "mr-2"), "") _
                '    & If(allowRestart, cp.Html.Button("button", ButtonRestart, "mr-2"), "") _
                '    & If(allowBack, cp.Html.Button("button", ButtonBack, "mr-2"), "") _
                '    & If(allowContinue, cp.Html.Button("button", ButtonContinue, "mr-2"), ""), "", "p-2 bg-secondary")
                'Return "" _
                '    & "<div class=""bg-white"">" _
                '        & buttonBar _
                '        & "<div class=""p-4"">" _
                '            & cp.Html.h2(header) _
                '            & cp.Html.div(description) _
                '            & cp.Html.div(body, "", "mt-4") _
                '        & "</div>" _
                '        & buttonBar _
                '    & "</div>"
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
        Public Shared Function getDbFieldSelect(cp As CPBaseClass, ContentName As String, NoneCaption As String, AllowID As Boolean, currentValue As String) As String
            Try
                Dim options As New StringBuilder("<option value=""""" & If(String.IsNullOrEmpty(currentValue), " selected", "") & ">" & cp.Utils.EncodeHTML(NoneCaption) & "</option>")
                For Each row In ContentFieldModel.getDbFieldList(cp, ContentName, AllowID, False)
                    options.AppendLine("<option value=""" & cp.Utils.EncodeHTML(row.name) & """" & If(row.name = currentValue, " selected", "") & ">" & cp.Utils.EncodeHTML(row.caption) & "</option>")
                Next
                Return "<select class=""form-control"" name=xxxx><option value="""" style=""Background-color:#E0E0E0;"">" & options.ToString() & "</option>"
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace