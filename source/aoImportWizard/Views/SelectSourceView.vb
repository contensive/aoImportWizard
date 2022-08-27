﻿
Imports Contensive.BaseClasses
Imports Contensive.Addons.PortalFramework
Imports System.Text

Namespace Contensive.ImportWizard.Controllers
    Public Class SelectSourceView
        '
        ''' <summary>
        ''' return the next view. 0 goes to the first form (start over)
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function processView(app As ApplicationController) As Integer
            Try
                Call WizardController.saveWizardRequestInteger(app.cp, RequestNameImportSource)
                Call WizardController.loadWizardPath(app)

                Dim Button As String = app.cp.Doc.GetText(RequestNameButton)
                Select Case Button
                    Case ButtonCancel
                        '
                        ' Cancel
                        '
                        Call WizardController.clearWizardValues(app.cp)
                        Return viewIdReturnBlank
                    Case ButtonContinue2
                        '
                        Select Case app.cp.Doc.GetInteger("ImportWizardSource")
                            Case 1
                                '
                                ' -- upload a commad delimited file
                                Return viewIdUpload
                            Case 2
                                '
                                ' -- use a file uploaded previously
                                Return viewIdSelectFile
                            Case Else
                                '
                                ' -- resource library (not implemented)
                                Return viewIdResourceLibrary
                        End Select
                End Select
                Return viewIdSelectSource
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        ''' <summary>
        ''' return the html for this view
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function getView(app As ApplicationController) As String
            Try
                'Dim body As New StringBuilder()
                'Dim selectedValue As Integer = app.cp.Doc.GetInteger(RequestNameImportSource, ImportSourceUpload)
                'body.Append(HtmlController.getRadio(app.cp, RequestNameImportSource, ImportSourceUpload, selectedValue, "Upload a comma delimited text file (up to 5 MBytes)."))
                'body.Append(HtmlController.getRadio(app.cp, RequestNameImportSource, ImportSourceUploadFolder, selectedValue, "Use a file already uploaded into your Upload Folder."))
                ''
                'Dim layout As New FormSimpleClass With {
                '    .title = "Select Import Source",
                '    .description = "There are several sources you can use for your data",
                '    .body = body.ToString(),
                '    .isOuterContainer = True,
                '    .formId = viewIdSelectSource,
                '    .
                '}
                'layout.addFormButton(ButtonCancel)
                'layout.addFormButton(ButtonContinue2)
                'Return layout.getHtml(app.cp)

                Dim cp As CPBaseClass = app.cp
                Dim headerCaption As String = "Import Wizard"
                Dim importSource As Integer = cp.Utils.EncodeInteger(WizardController.getWizardValue(cp, RequestNameImportSource, cp.Utils.EncodeText(ImportSourceUpload)))
                Dim description As String = cp.Html.h4("Select the import source") & cp.Html.p("There are several sources you can use for your data")
                Dim content As String = "" _
                    & "<div>" _
                    & "<div class=""pb-2"">" _
                    & "<label>" & cp.Html.RadioBox(RequestNameImportSource, ImportSourceUpload.ToString, importSource.ToString, "mr-2") & "Upload a comma delimited text file (up to 5 MBytes).</label>" _
                    & "</div>" _
                    & "<div class=""pb-2"">" _
                    & "<label>" & cp.Html.RadioBox(RequestNameImportSource, ImportSourceUploadFolder.ToString, importSource.ToString, "mr-2") & "Use a file already uploaded into your Upload Folder.</label>" _
                    & "</div>" _
                    & "</div>" _
                    & ""
                Return WizardController.getWizardContent(cp, headerCaption, ButtonCancel, "", ButtonContinue2, description, content)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
