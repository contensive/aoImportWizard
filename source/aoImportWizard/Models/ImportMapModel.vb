Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard.Models
    Public Class ImportMapModel

        'Public Shared Widening Operator CType(v As String) As ImportMapType
        '    Throw New NotImplementedException()
        'End Operator
        '
        ' Import Map file layout
        '
        ' row 0 - KeyMethodID
        ' row 1 - SourceKey Field
        ' row 2 - DbKey Field
        ' row 3 - blank
        ' row4+ SourceField,DbField mapping pairs
        '
        ' - if nuget references are not there, open nuget command line and click the 'reload' message at the top, or run "Update-Package -reinstall" - close/open
        ' - Verify project root name space is empty
        ' - Change the namespace (AddonCollectionVb) to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        Public Property importToNewContent As Boolean
        Public Property contentName As String
        Public Property keyMethodID As Integer
        Public Property sourceKeyField As String
        Public Property dbKeyField As String
        Public Property dbKeyFieldType As Integer
        Public Property groupOptionID As Integer
        Public Property groupID As Integer
        Public Property skipRowCnt As Integer
        Public Property mapPairCnt As Integer
        Public Property mapPairs As ImportMapModel_MapPair()
        '
        ''' <summary>
        ''' Load Import Map
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="importConfig"></param>
        ''' <returns></returns>
        Public Shared Function create(cp As CPBaseClass, importConfig As ImportConfigModel) As ImportMapModel
            Try
                Dim result As ImportMapModel = cp.JSON.Deserialize(Of ImportMapModel)(cp.PrivateFiles.Read(importConfig.importMapPathFilename))
                If result IsNot Nothing Then Return result
                '
                result = New ImportMapModel() With {
                .contentName = "People",
                .groupID = 0,
                .mapPairCnt = 0,
                .skipRowCnt = 1
                }
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '

        '
        '====================================================================================================
        ''' <summary>
        ''' Save the import map data
        ''' </summary>
        ''' <param name="app"></param>
        Public Sub save(app As ApplicationModel, importConfig As ImportConfigModel)
            Try
                Call app.cp.PrivateFiles.Save(importConfig.importMapPathFilename, app.cp.JSON.Serialize(Me))
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
    End Class
    '
    Public Class ImportMapModel_MapPair
        ''' <summary>
        ''' 0-based index to the column of the uploaded data file
        ''' -1 = ignore this fields
        ''' -2 = set the value from the setValue field
        ''' </summary>
        ''' <returns></returns>
        Public Property uploadFieldPtr As Integer
        ''' <summary>
        ''' the name of the column from the uploaded data file
        ''' </summary>
        ''' <returns></returns>
        Public Property uploadFieldName As String
        Public Property dbFieldName As String
        Public Property dbFieldType As Integer
        ''' <summary>
        ''' if not null, ignore dbfield and set the 
        ''' </summary>
        ''' <returns></returns>
        Public Property setValue As String
    End Class

End Namespace