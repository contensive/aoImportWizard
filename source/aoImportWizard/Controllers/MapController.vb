
Imports Contensive.BaseClasses
Imports Contensive.ImportWizard.Controllers

Namespace Contensive.ImportWizard.Addons
    Public Class MapController
        '
        '
        ''' <summary>
        ''' Load Import Map
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ImportMapData"></param>
        ''' <returns></returns>
        Public Function loadImportMap(cp As CPBaseClass, ImportMapData As String) As ImportMapType
            Try
                Dim result As New ImportMapType
                Dim Rows() As String
                Dim Pair() As String
                Dim Ptr As Long
                Dim SourceSplit() As String
                Dim MapPtr As Integer
                '
                If String.IsNullOrEmpty(ImportMapData) Then
                    '
                    ' Defaults
                    '
                    result.ContentName = "People"
                    result.GroupID = 0
                    result.MapPairCnt = 0
                    result.SkipRowCnt = 1
                Else
                    '
                    ' read in what must be saved
                    '
                    Rows = Split(ImportMapData, vbCrLf)
                    If UBound(Rows) <= 7 Then
                        '
                        ' Map file is bad
                        '
                        'Call HandleLocalError(KmaErrorInternal, App.EXEName, "ImportWizard.LoadImportMap failed because there was a problem with the format of the data", "LoadImportMap", False, True)
                    Else
                        result.KeyMethodID = cp.Utils.EncodeInteger(Rows(0))
                        result.SourceKeyField = Rows(1)
                        result.DbKeyField = Rows(2)
                        result.ContentName = Rows(3)
                        result.GroupOptionID = cp.Utils.EncodeInteger(Rows(4))
                        result.GroupID = cp.Utils.EncodeInteger(Rows(5))
                        result.SkipRowCnt = cp.Utils.EncodeInteger(Rows(6))
                        result.DbKeyFieldType = cp.Utils.EncodeInteger(Rows(7))
                        result.importToNewContent = cp.Utils.EncodeBoolean(Rows(8))
                        result.MapPairCnt = 0
                        '
                        If UBound(Rows) > 8 Then
                            If String.IsNullOrEmpty(Trim(Rows(9))) Then
                                For Ptr = 10 To UBound(Rows)
                                    Pair = Split(Rows(CInt(Ptr)), "=")
                                    If UBound(Pair) > 0 Then
                                        MapPtr = result.MapPairCnt
                                        result.MapPairCnt = CInt(MapPtr + 1)
                                        ReDim Preserve result.MapPairs(MapPtr)
                                        result.MapPairs(MapPtr) = New MapPairType()
                                        result.MapPairs(CInt(MapPtr)).DbField = Pair(0)
                                        SourceSplit = Split(Pair(1), ",")
                                        If UBound(SourceSplit) > 0 Then
                                            result.MapPairs(CInt(MapPtr)).SourceFieldPtr = cp.Utils.EncodeInteger(SourceSplit(0))
                                            result.MapPairs(CInt(MapPtr)).DbFieldType = cp.Utils.EncodeInteger(SourceSplit(1))
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

        '
        '====================================================================================================
        ''' <summary>
        ''' Save the import map data
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="ImportMap"></param>
        Public Shared Sub saveImportMap(app As ApplicationController, ImportMap As ImportMapType)
            Try
                Dim cp As CPBaseClass = app.cp
                Dim ImportMapFile As String
                Dim ImportMapData As String
                Dim Ptr As Integer
                '
                ImportMapFile = WizardController.getWizardValue(cp, RequestNameImportMapFile, app.getDefaultImportMapFile(cp))
                ImportMapData = "" _
                & ImportMap.KeyMethodID _
                & vbCrLf & ImportMap.SourceKeyField _
                & vbCrLf & ImportMap.DbKeyField _
                & vbCrLf & ImportMap.ContentName _
                & vbCrLf & ImportMap.GroupOptionID _
                & vbCrLf & ImportMap.GroupID _
                & vbCrLf & ImportMap.SkipRowCnt _
                & vbCrLf & ImportMap.DbKeyFieldType _
                & vbCrLf & ImportMap.importToNewContent _
                & vbCrLf
                If ImportMap.MapPairCnt > 0 Then
                    For Ptr = 0 To ImportMap.MapPairCnt - 1
                        ImportMapData = ImportMapData & vbCrLf & ImportMap.MapPairs(Ptr).DbField & "=" & ImportMap.MapPairs(Ptr).SourceFieldPtr & "," & ImportMap.MapPairs(Ptr).DbFieldType
                    Next
                End If
                Call cp.CdnFiles.Save(ImportMapFile, ImportMapData)
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
    End Class
End Namespace