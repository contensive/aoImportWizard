

Namespace Contensive.ImportWizard.Models
    Public Class ImportDataModel
        Public Property importSource As ImportDataModel_ImportTypeEnum
        ''' <summary>
        ''' the uploaded data filename
        ''' </summary>
        ''' <returns></returns>
        Public Property privateCsvPathFilename As String
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
        ''' create the current instance of the visits importdataobject
        ''' </summary>
        ''' <param name="app"></param>
        ''' <returns></returns>
        Public Shared Function create(app As ApplicationModel) As ImportDataModel
            Dim result As ImportDataModel = app.cp.Visit.GetObject(Of ImportDataModel)("ImportWizardData")
            '
            ' -- return data
            If result IsNot Nothing Then Return result
            '
            ' -- return default
            result = New ImportDataModel()
            result.importSource = ImportDataModel_ImportTypeEnum.UploadFile
            result.importMapPathFilename = "ImportWizard\ImportMap" & app.cp.Utils.GetRandomInteger & ".txt"
            result.save(app)
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

