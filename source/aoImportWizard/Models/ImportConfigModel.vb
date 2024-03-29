﻿

Namespace Contensive.ImportWizard.Models
    Public Class ImportConfigModel
        Public Property importSource As ImportDataModel_ImportTypeEnum
        ''' <summary>
        ''' the uploaded data filename
        ''' </summary>
        ''' <returns></returns>
        Public Property privateUploadPathFilename As String
        '
        Public Property importMapPathFilename As String
        '
        Public Property notifyEmail As String
        ''' <summary>
        ''' contentid of the destination for the import
        ''' </summary>
        ''' <returns></returns>
        Public Property dstContentId As Integer
        '
        ''' <summary>
        ''' create new empty import map. ContentName requested because it must be valid before map is valid.
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="contentName"></param>
        Public Sub newEmptyImportMap(app As ApplicationModel, contentName As String)
            If String.IsNullOrEmpty(contentName) Then
                '
                ' -- problem, cannot create map until content is established.
            End If
            'Dim mapFilenameData As New MapFilenameDataModel With {
            '    .mapName = "Import " & contentName,
            '    .dateCreated = Now
            '}
            importMapPathFilename = ImportMapModel.createMapPathFilename(app, contentName, "Import " & contentName & ".txt")
            save(app)
        End Sub
        '
        ''' <summary>
        ''' create the current instance of the visits importdataobject
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function create(app As ApplicationModel) As ImportConfigModel
            Dim result As ImportConfigModel = app.cp.Visit.GetObject(Of ImportConfigModel)("ImportWizardData")
            '
            ' -- return data
            If result IsNot Nothing Then Return result
            '
            ' -- setup default values
            result = New ImportConfigModel With {
                .importSource = ImportDataModel_ImportTypeEnum.UploadFile,
                .notifyEmail = app.cp.User.Email,
                .dstContentId = 0
            }
            result.save(app)
            '
            ' -- setup a new import map
            'result.newEmptyImportMap(app)
            Return result
        End Function
        '
        ''' <summary>
        ''' save this instance of the data
        ''' </summary>
        ''' <param name="app"></param>
        Public Sub save(app As ApplicationModel)
            app.cp.Visit.SetProperty("ImportWizardData", app.cp.JSON.Serialize(Me))
        End Sub
        '
        ''' <summary>
        ''' clear the data
        ''' </summary>
        ''' <param name="app"></param>
        Public Shared Sub clear(app As ApplicationModel)
            app.cp.Visit.SetProperty("ImportWizardData", "")
        End Sub
        '
        '
        Public Shared Function convertImportTypeToInt(enumValue As ImportDataModel_ImportTypeEnum) As Integer
            Return Convert.ToInt32(enumValue)
        End Function
    End Class
    '
    Public Enum ImportDataModel_ImportTypeEnum
        UploadFile = 1
        SelectFile = 2

    End Enum
End Namespace

