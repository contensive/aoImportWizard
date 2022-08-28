Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Models

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
    ''' <param name="ImportData"></param>
    ''' <returns></returns>
    Public Shared Function create(cp As CPBaseClass, importData As ImportDataModel) As ImportMapModel
        Try

            Dim ImportMapData As String = cp.PrivateFiles.Read(importData.importMapPathFilename)


            Dim result As New ImportMapModel
            Dim Rows As String()
            Dim Pair As String()
            Dim Ptr As Long
            Dim SourceSplit As String()
            Dim MapPtr As Integer
            '
            If String.IsNullOrEmpty(ImportMapData) Then
                '
                ' Defaults
                '
                result.contentName = "People"
                result.groupID = 0
                result.mapPairCnt = 0
                result.skipRowCnt = 1
                Return result
            End If
            '
            ' read in what must be saved
            '
            Rows = Split(ImportMapData, vbCrLf)
            If UBound(Rows) > 7 Then
                result.keyMethodID = cp.Utils.EncodeInteger(Rows(0))
                result.sourceKeyField = Rows(1)
                result.dbKeyField = Rows(2)
                result.contentName = Rows(3)
                result.groupOptionID = cp.Utils.EncodeInteger(Rows(4))
                result.groupID = cp.Utils.EncodeInteger(Rows(5))
                result.skipRowCnt = cp.Utils.EncodeInteger(Rows(6))
                result.dbKeyFieldType = cp.Utils.EncodeInteger(Rows(7))
                result.importToNewContent = cp.Utils.EncodeBoolean(Rows(8))
                result.mapPairCnt = 0
                '
                If String.IsNullOrEmpty(Trim(Rows(9))) Then
                    For Ptr = 10 To UBound(Rows)
                        Pair = Split(Rows(CInt(Ptr)), "=")
                        If UBound(Pair) > 0 Then
                            MapPtr = result.mapPairCnt
                            result.mapPairCnt = CInt(MapPtr + 1)
                            ReDim Preserve result.mapPairs(MapPtr)
                            result.mapPairs(MapPtr) = New ImportMapModel_MapPair()
                            result.mapPairs(CInt(MapPtr)).dbField = Pair(0)
                            SourceSplit = Split(Pair(1), ",")
                            If UBound(SourceSplit) > 0 Then
                                result.mapPairs(CInt(MapPtr)).sourceFieldPtr = cp.Utils.EncodeInteger(SourceSplit(0))
                                result.mapPairs(CInt(MapPtr)).dbFieldType = cp.Utils.EncodeInteger(SourceSplit(1))
                            End If
                        End If
                    Next
                End If
            End If
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
    Public Sub save(app As ApplicationModel)
        Try
            Dim ImportMapData As String = "" _
                & keyMethodID _
                & vbCrLf & sourceKeyField _
                & vbCrLf & dbKeyField _
                & vbCrLf & contentName _
                & vbCrLf & groupOptionID _
                & vbCrLf & groupID _
                & vbCrLf & skipRowCnt _
                & vbCrLf & dbKeyFieldType _
                & vbCrLf & importToNewContent _
                & vbCrLf
            If mapPairCnt > 0 Then
                For Ptr As Integer = 0 To mapPairCnt - 1
                    ImportMapData = ImportMapData & vbCrLf & mapPairs(Ptr).dbField & "=" & mapPairs(Ptr).sourceFieldPtr & "," & mapPairs(Ptr).dbFieldType
                Next
            End If
            Dim importData As ImportDataModel = ImportDataModel.create(app)
            Call app.cp.CdnFiles.Save(importData.importMapPathFilename, ImportMapData)
        Catch ex As Exception
            app.cp.Site.ErrorReport(ex)
            Throw
        End Try
    End Sub
    '
    '
    '====================================================================================================
    '
    Public Shared Function loadImportMap(cp As CPBaseClass, importMapData As String) As ImportMapModel
        Try
            Dim result As New ImportMapModel
            Dim Rows() As String
            Dim Pair() As String
            Dim Ptr As Integer
            Dim SourceSplit() As String
            Dim MapPtr As Integer
            '
            If String.IsNullOrEmpty(importMapData) Then
                '
                ' Defaults
                '
                result.contentName = "People"
                result.groupID = 0
                result.mapPairCnt = 0
                result.skipRowCnt = 1
            Else
                '
                ' read in what must be saved
                '
                Rows = Split(importMapData, vbCrLf)
                If UBound(Rows) <= 7 Then
                    '
                    ' Map file is bad
                    '
                    'Call HandleLocalError(KmaErrorInternal, App.EXEName, "ImportWizard.Result failed because there was a problem with the format of the data", "Result", False, True)
                Else
                    result.keyMethodID = cp.Utils.EncodeInteger(Rows(0))
                    result.sourceKeyField = Rows(1)
                    result.dbKeyField = Rows(2)
                    result.contentName = Rows(3)
                    result.groupOptionID = cp.Utils.EncodeInteger(Rows(4))
                    result.groupID = cp.Utils.EncodeInteger(Rows(5))
                    result.skipRowCnt = cp.Utils.EncodeInteger(Rows(6))
                    result.dbKeyFieldType = cp.Utils.EncodeInteger(Rows(7))
                    result.importToNewContent = cp.Utils.EncodeBoolean(Rows(8))
                    result.mapPairCnt = 0
                    '
                    If UBound(Rows) > 8 Then
                        If String.IsNullOrEmpty(Trim(Rows(9))) Then
                            For Ptr = 10 To UBound(Rows)
                                Pair = Split(Rows(Ptr), "=")
                                If UBound(Pair) > 0 Then
                                    MapPtr = result.mapPairCnt
                                    result.mapPairCnt = MapPtr + 1
                                    ReDim Preserve result.mapPairs(MapPtr)
                                    result.mapPairs(MapPtr) = New ImportMapModel_MapPair With {
                                            .dbField = Pair(0)
                                        }
                                    SourceSplit = Split(Pair(1), ",")
                                    If UBound(SourceSplit) > 0 Then
                                        result.mapPairs(MapPtr).sourceFieldPtr = cp.Utils.EncodeInteger(SourceSplit(0))
                                        result.mapPairs(MapPtr).dbFieldType = cp.Utils.EncodeInteger(SourceSplit(1))
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If
            End If
            Return result
        Catch ex As Exception
            cp.Site.ErrorReport(ex)
            Throw
        End Try
    End Function
    '
End Class
'
Public Class ImportMapModel_MapPair
    Public Property sourceFieldPtr As Integer
    Public Property sourceFieldName As String
    Public Property dbField As String
    Public Property dbFieldType As Integer
End Class
