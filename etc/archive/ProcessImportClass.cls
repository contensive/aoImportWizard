VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ProcessImportClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
Private ClassInitialized        ' if true, the module has been
Private CSv As ContentServerClass
'
'=================================================================================
'
'=================================================================================
'
Public Function ProcessCSV(ContentServer As Object, CSVFilename As String, ImportMapFilename As String) As String
    On Error GoTo ErrorTrap
    '
    Set CSv = ContentServer
    '
    Dim ImportMap As ImportMapType
    Dim SourcePtr As Long
    Dim Source As String
    Dim Rows() As String
    Dim RowPtr As Long
    Dim row As String
    Dim Cells() As String
    Dim CS As Long
    Dim DBFieldName As String
    Dim SourceData As String
    Dim FieldPtr As Long
    Dim ContentName As String
    Dim DbKeyField As String
    Dim SourceKeyPtr As Long
    Dim SourceKeyData As String
    Dim KeyMethodID As Long
    Dim KeyCriteria As String
    Dim UpdateRecord As Boolean
    Dim InsertRecord As Boolean
    Dim MemberID As Long
    Dim UpdateSQL As String
    Dim LoopCnt As Long
    Dim RowWidth As Long
    Dim Hint As String
    Dim ImportTableName As String
    '
    Hint = "000"
    ImportMap = LoadImportMap(CSv.ReadVirtualFile(ImportMapFilename))
    If ImportMap.MapPairCnt > 0 Then
        Hint = "100"
        Source = CSv.ReadVirtualFile(CSVFilename)
        If Source <> "" Then
            Hint = "200"
            ContentName = ImportMap.ContentName
            If ContentName = "" Then
                ContentName = "People"
            End If
            ImportTableName = CSv.GetContentTablename(ImportMap.ContentName)
            Rows = kmaSplit(Source, vbCrLf)
            DbKeyField = ImportMap.DbKeyField
            SourceKeyPtr = ImportMap.SourceKeyField
            KeyMethodID = ImportMap.KeyMethodID
            If (DbKeyField = "") Or (SourceKeyPtr < 0) Then
                KeyMethodID = KeyMethodInsertAll
            End If
            '
            '
            '
            KeyCriteria = "(1=0)"
            For RowPtr = ImportMap.SkipRowCnt To UBound(Rows)
                Hint = "300"
                UpdateRecord = False
                InsertRecord = False
                row = Rows(RowPtr)
                RowWidth = 0
                If row <> "" Then
                    Hint = "310"
                    Cells = kmaSplit(row, ",")
                    If KeyMethodID = KeyMethodInsertAll Then
                        Hint = "320"
                        '
                        ' Insert the record
                        '
'                        CS = CSv.InsertCSRecord(ContentName, 0)
'                        If CSv.IsCSOK(CS) Then
'                            KeyCriteria = "(ID=" & KmaEncodeSQLNumber(CSv.GetCSInteger(CS, "ID")) & ")"
'                        End If
                        InsertRecord = True
                    Else
                        Hint = "330"
                        '
                        ' Update or Update-And-Insert, Build Key Criteria
                        '
                        SourceKeyData = Cells(SourceKeyPtr)
                        If Len(SourceKeyData) > 2 And Left(SourceKeyData, 1) = """" And Right(SourceKeyData, 1) = """" Then
                            SourceKeyData = Trim(Mid(SourceKeyData, 2, Len(SourceKeyData) - 2))
                        End If
                        If Trim(SourceKeyData) = "" Then
                            '
                            ' Source had no data in key field, insert if allowed
                            '
                            If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                InsertRecord = True
                            End If
                        Else
                            Hint = "340"
                            '
                            ' Source has good key field data
                            '
                            Select Case ImportMap.DbKeyFieldType
                                Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                    '
                                    ' number
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeNumber(SourceKeyData) & ")"
                                Case FieldTypeBoolean
                                    '
                                    ' Boolean
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeSQLBoolean(SourceKeyData) & ")"
                                Case FieldTypeDate
                                    '
                                    ' date
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeSQLDate(SourceKeyData) & ")"
                                Case FieldTypeText
                                    '
                                    ' text
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeSQLText(Left(SourceKeyData, 255)) & ")"
                                Case FieldTypeLongText
                                    '
                                    ' long text
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeSQLLongText(SourceKeyData) & ")"
                                Case Else
                                    '
                                    ' unknown field type
                                    '
                                    UpdateRecord = True
                                    If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                        InsertRecord = True
                                    End If
                            End Select
                            ' move to after the row work so we can skip the insert if the row width=0
'                            CS = CSv.OpenCSContent(ContentName, KeyCriteria, "ID", False)
'                            If Not CSv.IsCSOK(CS) Then
'                                '
'                                ' No Match -- convert to insert if requested
'                                '
'                                Call CSv.CloseCS(CS)
'                                If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
'                                    'CS = CSv.InsertCSRecord(ContentName, 0)
'                                    InsertRecord = True
'                                End If
'                            Else
'                                '
'                                ' Match found, perform update
'                                '
'                                UpdateRecord = True
'                            End If
'                            'Call CSv.CloseCS(CS)
                        End If
                    End If
                    '
                    ' If Insert, Build KeyCriteria and setup CS
                    ' x If Update, CS is good -- else there is no open record
                    '
'                    If InsertRecord Then
'                        '
'                        ' Insert a new record
'                        '
'                        CS = CSv.InsertCSRecord(ContentName, 0)
'                        If CSv.IsCSOK(CS) Then
'                            KeyCriteria = "(ID=" & KmaEncodeSQLNumber(CSv.GetCSInteger(CS, "ID")) & ")"
'                        End If
'                    End If
                    If (InsertRecord Or UpdateRecord) Then
                        Hint = "400"
                        '
                        ' Build Update SQL
                        '
                        UpdateSQL = ""
                        For FieldPtr = 0 To ImportMap.MapPairCnt - 1
                            Hint = "500"
                            SourcePtr = ImportMap.MapPairs(FieldPtr).SourceFieldPtr
                            If SourcePtr < 0 Then
                                '
                                ' Bad pointer
                                '
                                SourcePtr = SourcePtr
                            ElseIf SourcePtr > UBound(Cells) Then
                                '
                                ' This data row was not as long as the header row - skip it
                                '
                            Else
                                Hint = "600"
                                DBFieldName = ImportMap.MapPairs(FieldPtr).DbField
                                SourceData = Cells(SourcePtr)
                                If Len(SourceData) > 2 And Left(SourceData, 1) = """" And Right(SourceData, 1) = """" Then
                                    SourceData = Trim(Mid(SourceData, 2, Len(SourceData) - 2))
                                End If
                                'If LCase(SourceData) = "mcgee" Then
                                '    SourceData = SourceData
                                'End If
                                RowWidth = RowWidth + Len(Trim(SourceData))
                                ' there are no fieldtypes defined as 0, and I do not want the CS open now, so we can avoid the insert if rowwidth=0
                                'If ImportMap.MapPairs(FieldPtr).DbFieldType = 0 Then
                                '    Call CSv.SetCS(CS, DBFieldName, SourceData)
                                'Else
                                Select Case ImportMap.MapPairs(FieldPtr).DbFieldType
                                    Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                        '
                                        ' number
                                        '
                                        UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeNumber(SourceData)
                                    Case FieldTypeBoolean
                                        '
                                        ' Boolean
                                        '
                                        UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeSQLBoolean(SourceData)
                                    Case FieldTypeDate
                                        '
                                        ' date
                                        '
                                        UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeSQLDate(SourceData)
                                    Case FieldTypeText
                                        '
                                        ' text
                                        '
                                        UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeSQLText(Left(SourceData, 255))
                                    Case FieldTypeLongText
                                        '
                                        ' long text
                                        '
                                        UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeSQLLongText(SourceData)
                                    'Case FieldTypeTextFile
                                    '
                                    ' can not map to any file type because these are sql updates and each record must have a unique filename
                                    ' the import needs to be rewritten to update one record at a time.
                                    '   1) pick up the key criteria and run it on live Db to select the record(s) to update
                                    '   2) update the records in the ContentSet and SaveCS
                                    '
                                    '    '
                                    '    ' text file
                                    '    '
                                    '    Filename = CSv.GetVirtualFilenameByTable(ImportTableName, DBFieldName, RecordID, "", FieldTypeTextFile)
                                    '    UpdateSQL = UpdateSQL & "," & DBFieldName & "=" & KmaEncodeSQLLongText(SourceData)
                                    Case FieldTypeFile, FieldTypeRedirect
                                        '
                                        ' can not import these
                                        '
                                End Select
                            End If
                        Next
                    End If
                    Hint = "700"
                    On Error Resume Next
                    'Call CSv.CloseCS(CS)
                    If Err.Number <> 0 Then
                        ProcessCSV = ProcessCSV & vbCrLf & "Row " & (RowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                    ElseIf RowWidth = 0 Then
                        ProcessCSV = ProcessCSV & vbCrLf & "Row " & (RowPtr + 1) & " was not imported because it was empty. [" & Err.Description & "]"
                    Else
                        '
                        ' If Update, Check the KeyCriteria by selecting the record
                        '
                        Hint = "800"
                        If UpdateRecord Then
                            CS = CSv.OpenCSContent(ContentName, KeyCriteria, "ID", False)
                            If CSv.IsCSOK(CS) Then
                                '
                                ' Good match, get the memberid for later
                                '
                                'MemberID = CSv.GetCSInteger(CS, "ID")
                            Else
                                '
                                ' No Match -- convert to insert if requested
                                '
                                UpdateRecord = False
                                Call CSv.CloseCS(CS)
                                If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                    InsertRecord = True
                                End If
                            End If
                        End If
                        '
                        ' Now save the record
                        '
                        If InsertRecord Then
                            '
                            ' Insert a new record and convert to an update
                            '
                            CS = CSv.InsertCSRecord(ContentName, 0)
                            If CSv.IsCSOK(CS) Then
                                MemberID = CSv.GetCSInteger(CS, "ID")
                                KeyCriteria = "(ID=" & KmaEncodeSQLNumber(MemberID) & ")"
                                UpdateRecord = True
                            End If
                        End If
                        If UpdateRecord And (UpdateSQL <> "") Then
                            Hint = "900"
                            '
                            ' Update all records in the current recordset
                            '
                            UpdateSQL = "update " & ImportTableName & " set " & Mid(UpdateSQL, 2) & " where " & KeyCriteria
                            Call CSv.ExecuteSQL(CSv.GetContentDataSource(ImportMap.ContentName), UpdateSQL)
                            If Err.Number <> 0 Then
                                ProcessCSV = ProcessCSV & vbCrLf & "Row " & (RowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                            ElseIf ImportMap.GroupOptionID <> GroupOptionNone Then
                                Do While CSv.IsCSOK(CS)
                                    If UpdateRecord Or InsertRecord Then
                                        Hint = "910"
                                        '
                                        ' Add Groups
                                        '
                                        MemberID = CSv.GetCSInteger(CS, "ID")
                                            Select Case ImportMap.GroupOptionID
                                                Case GroupOptionAll
                                                    '
                                                    '
                                                    '
                                                    Call AddGroupMember(MemberID, ImportMap.GroupID)
                                                Case GroupOptionOnMatch
                                                    '
                                                    '
                                                    '
                                                    If UpdateRecord Then
                                                        Call AddGroupMember(MemberID, ImportMap.GroupID)
                                                    End If
                                                Case GroupOptionOnNoMatch
                                                    '
                                                    '
                                                    '
                                                    If Not UpdateRecord Then
                                                        Call AddGroupMember(MemberID, ImportMap.GroupID)
                                                    End If
                                            End Select
                                        End If
                                    Call CSv.NextCSRecord(CS)
                                Loop
                            End If
                            Call CSv.CloseCS(CS)
                        End If
                    End If
                    On Error GoTo ErrorTrap
                End If
                LoopCnt = LoopCnt + 1
                If LoopCnt > 10 Then
                    LoopCnt = 0
                End If
            Next
        End If
    End If
    '
    Exit Function
    '
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "ProcessCSV - Hint=" & Hint, True, False)
End Function
'
'===========================================================================
'   Error handler
'===========================================================================
'
Public Function HandleLocalError(ErrNumber As Long, ErrSource As String, ErrDescription As String, MethodName As String, ErrorTrap As Boolean, Optional ResumeNext As Boolean) As String
    '
    Call HandleError("ProcessImportClass", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext)
    '
End Function
'
'
'
Private Sub AddGroupMember(MemberID As Long, GroupID As Long)
    On Error GoTo ErrorTrap
    '
    Dim CS As Long
    '
    CS = CSv.OpenCSContent("Member Rules", "(MemberID=" & MemberID & ")and(GroupID=" & GroupID & ")")
    If Not CSv.IsCSOK(CS) Then
        Call CSv.CloseCS(CS)
        CS = CSv.InsertCSRecord("Member Rules", 0)
        If CSv.IsCSOK(CS) Then
            Call CSv.SetCS(CS, "MemberID", MemberID)
            Call CSv.SetCS(CS, "GroupID", GroupID)
        End If
        Call CSv.CloseCS(CS)
    End If
    '
    Exit Sub
    '
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "AddGroupMember", True, False)
End Sub

