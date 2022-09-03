
Imports System.Linq
Imports System.Text
Imports Contensive.ImportWizard.Models
Imports Contensive.ImportWizard.Controllers
Imports Contensive.ImportWizard.Controllers.GenericController
Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Contensive.ImportWizard.Addons
    ''' <summary>
    ''' The addon that runs on the page -- setup the import files
    ''' </summary>
    Public Class ImportWizardAddon
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ''' <summary>
        ''' The addon that runs on the page -- setup the import files
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using app As New ApplicationModel(CP, False)
                    '
                    ' Process incoming form
                    '
                    Dim viewId As Integer = CP.Doc.GetInteger(rnSrcViewId)
                    Select Case viewId
                        Case viewIdSelectSource
                            '
                            ' Source and ContentName
                            '
                            viewId = SelectSourceView.processView(app, viewId)
                        Case viewIdUpload
                            '
                            ' Upload
                            '
                            viewId = UploadView.processView(app, viewId)
                        Case viewIdSelectFile
                            '
                            ' Select file
                            '
                            viewId = SelectFileView.processView(app, viewId)
                        Case viewIdSelectContent
                            '
                            ' Source and ContentName
                            '
                            viewId = SelectContentView.processView(app, viewId)
                        Case viewIdSelectMap
                            '
                            ' select Mapping
                            '
                            viewId = SelectMapView.processView(app, viewId)
                        Case viewIdMapping
                            '
                            ' Mapping - Save Values to the file pointed to by RequestNameImportMapFile
                            '
                            viewId = MappingView.processView(app, viewId)
                        Case viewIdSelectKey
                            '
                            ' Select Key Field
                            '
                            viewId = SelectKeyView.processView(app, viewId)
                        Case viewIdSelectGroup
                            '
                            ' Add to group
                            '
                            viewId = SelectGroupView.processView(app, viewId)
                        Case viewIdFinish
                            '
                            ' Determine next or previous form
                            '
                            viewId = FinishView.processView(app, viewId)
                        Case viewIdDone
                            '
                            ' nothing to do, keep same form
                            viewId = DoneView.processView(app, viewId)
                    End Select
                    '
                    ' Get Next Form
                    '
                    Dim body As String = ""
                    Select Case viewId
                        Case viewIdUpload
                            '
                            ' Upload file to Upload folder
                            '
                            body = UploadView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectFile
                            '
                            ' Select a file from the upload folder
                            '
                            body = SelectFileView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectContent
                            '
                            ' Destination
                            '
                            body = SelectContentView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectMap
                            '
                            '
                            '
                            body = SelectMapView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdMapping
                            '
                            '
                            '
                            body = MappingView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectKey
                            '
                            ' Select Key
                            '
                            body = SelectKeyView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdSelectGroup
                            '
                            ' Select a group to add
                            '
                            body = SelectGroupView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdFinish
                            '
                            ' Ask for an email address to notify when the list is complete
                            '
                            body = FinishView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdDone
                            '
                            ' Thank you
                            '
                            body = DoneView.getView(app)
                            Return CP.Html.Form(body)
                        Case viewIdReturnBlank
                            '
                            ' -- 
                            Return ""
                        Case Else
                            '
                            ' -- data source
                            body = SelectSourceView.getView(app)
                            Return CP.Html.Form(body)
                    End Select
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
