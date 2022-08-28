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
            ' -- return default
            Dim rightNow As DateTime = Now
            result = New ImportConfigModel()
            result.importSource = ImportDataModel_ImportTypeEnum.UploadFile
            result.importMapPathFilename = constants.privateFilesMapFolder & "user" & app.cp.User.Id & "\map" & "-" & rightNow.Year & "-" & rightNow.Month & "-" & rightNow.Day & "-" & rightNow.Hour & "-" & rightNow.Minute & "-" & rightNow.Second & ".txt"
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
