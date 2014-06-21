VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ProcessClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
'========================================================================
'
' Sample file to create a Contensive Aggregate Object
'
' An Aggregate Object is an ActiveX DLL (in process dll) called by Contensive
' in response to finding a 'token' in the content of a page. When the token is
' found, it is looked up in the Aggregate Function Objects table and the Program ID
' for this code is retrieved, and used to create an object from this class.
'
' Contensive v3.4+
'
' Contensive calls the execute method with four arguments:
'
'   CsvObject - an object pointer to the ContentServerClass object loaded for this application
'
'   MainObject - an object pointer to the MainClass (web client interface) for this hit
'
'   OptionString - an encoded string that carries the arguments for this add-on
'       Always use the Csv.GetAddonOption() method to retrieve a value from the Optionstring
'       Arguments are a name=value argument list
'       The arguments are created first, from the argument list in the Add-on record, and
'           second from changes made by the site administrators during edit.
'
'   FilterInput - when the addon is acting as a filter, this is the input. For instance, If this
'       Addon is a filter for a page, the Filter input will be the page content.
'
' The output depends on the context of the Add-on:
'
'   Page Add-ons create content the replaces their token on the page. The return string
'       is the content to be placed on the page.
'
'   Filter Add-ons take the FilterInput argument, modify as needed, and return it
'       as the return string.
'
'   Process Add-ons perform their function and return error messages through their return
'
'========================================================================
'
'Private Main As ccWeb3.MainClass
Private csv As Object
Private Main As Object
'
'=================================================================================
'   Execute Method, v3.4 Interface
'=================================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    On Error GoTo ErrorTrap
    '
    Dim Argument1 As String
    Dim Argument2 As String
    Dim DoSomethingPtr As Long
    '
    Dim ResultMessage As String
    Dim SQLDateNow As String
    Dim CS As Long
    Dim SQL As String
    Dim Filename As String
    Dim DataSource As String
    Dim CSVFilename As String
    Dim ImportMapFilename As String
    Dim NotifyEMail As String
    Dim NotifyBody As String
    Dim NotifySubject As String
    Dim SQLFieldName As String
    Dim PreviousProcessAborted As Boolean
    Dim notifyFromAddress As String
    '
    Set csv = CsvObject
    Set Main = MainObject
    '
    Argument1 = CsvObject.GetAddonOption("Argument1", OptionString)
    Argument2 = CsvObject.GetAddonOption("Argument2", OptionString)
    '
    '   Check this server for anything in the tasks queue
    '
    If csv.IsContentFieldSupported("Import Wizard Tasks", "ID") Then
        CS = csv.OpenCSContent("Import Wizard Tasks", "DateCompleted is null")
        Do While csv.IsCSOK(CS)
            Call csv.SetCS(CS, "DateCompleted", Now)
            Call csv.savecs(CS)
            PreviousProcessAborted = False
            If (csv.GetDataBuildVersion > "3.4.070") Then
                '
                ' Since only one ccTask can run at a time, if a task is found to be started, it must have aborted.
                ' Mark it aborted so it will not lock up all task processes
                '
                PreviousProcessAborted = (csv.GetCSDate(CS, "DateStarted") <> CDate(0))
            End If
            If PreviousProcessAborted Then
                'Call csv.SetCS(CS, "DateCompleted", Now)
                Call csv.SetCS(CS, "ResultMessage", "This task failed to complete.")
            Else
                If csv.IsContentFieldSupported("Import Wizard Tasks", "NotifyEmail") Then
                    NotifyEMail = csv.GetCS(CS, "NotifyEmail")
                End If
                NotifyBody = ""
                If csv.GetSiteProperty("BuildVersion", "", SystemMemberID) >= "3.3.583" Then
                    SQLFieldName = "SQLQuery"
                Else
                    SQLFieldName = "SQL"
                End If
                '
                ' Import a CSV file
                '
                CSVFilename = Replace(csv.GetCSText(CS, "uploadFilename"), "/", "\")
                ImportMapFilename = Replace(csv.GetCSText(CS, "ImportMapFilename"), "/", "\")
                '
                ResultMessage = ProcessCSV(csv, CSVFilename, ImportMapFilename)
                If ResultMessage <> "" Then
                    NotifyBody = "This email is to notify you that your data import is complete for [" & csv.ApplicationNameLocal & "]" & vbCrLf & "The following errors occurred during import" & vbCrLf & ResultMessage
                Else
                    NotifyBody = "This email is to notify you that your data import is complete for [" & csv.ApplicationNameLocal & "]"
                End If
                NotifySubject = "Import Completed"
                'Call csv.SetCS(CS, "DateCompleted", Now)
                If csv.GetDataBuildVersion > "3.4.070" Then
                    If ResultMessage = "" Then
                        ResultMessage = "OK"
                    End If
                    Call csv.SetCS(CS, "ResultMessage", ResultMessage)
                End If
                If NotifyEMail <> "" And NotifyBody <> "" Then
                    notifyFromAddress = csv.GetSiteProperty("EmailFromAddress", "", 0)
                    Call csv.SendEmail(NotifyEMail, notifyFromAddress, "Task Completion Notification", NotifyBody)
                End If
            End If
            csv.NextCSRecord (CS)
        Loop
        Call csv.closecs(CS)
    End If
    '
    Exit Function
ErrorTrap:
    'HandleError
End Function
'
'=================================================================================
'
'=================================================================================
'
Private Function ProcessCSV(ContentServer As Object, CSVFilename As String, ImportMapFilename As String) As String
    On Error GoTo ErrorTrap
    '
    Set csv = ContentServer
    '
    Dim matchFound As Boolean
    Dim colCnt As Long
    Dim rowCnt As Long
    Dim ImportMap As ImportMapType
    Dim sourcePtr As Long
    Dim Source As String
    Dim Rows() As String
    Dim rowPtr As Long
    Dim colPtr As Long
    Dim row As String
    Dim cells() As String
    Dim CS As Long
    Dim DBFieldName As String
    Dim SourceData As String
    Dim fieldPtr As Long
    Dim ContentName As String
    Dim DbKeyField As String
    Dim SourceKeyPtr As Long
    Dim SourceKeyData As String
    Dim KeyMethodID As Long
    Dim KeyCriteria As String
    Dim UpdateRecord As Boolean
    Dim InsertRecord As Boolean
    Dim recordId As Long
    Dim UpdateSQLFieldSet As String
    Dim UpdateSQL As String
    Dim LoopCnt As Long
    Dim RowWidth As Long
    Dim hint As String
    Dim ImportTableName As String
    '
    hint = "000"
    '
    If Left(CSVFilename, 1) = "\" Then
        CSVFilename = Mid(CSVFilename, 2)
    End If
    hint = "010"
    Source = csv.ReadVirtualFile(CSVFilename)
    If Source <> "" Then
        hint = "020"
        ImportMap = LoadImportMap(csv.ReadVirtualFile(ImportMapFilename))
        hint = "030"
        ContentName = ImportMap.ContentName
        hint = "040"
        cells = parseFile(Source)
        colCnt = UBound(cells, 1) + 1
        rowCnt = UBound(cells, 2) + 1
        If ImportMap.importToNewContent Then
            hint = "050"
            '
            ' create the destination table and import map
            '
            ImportMap.MapPairCnt = colCnt
            ReDim ImportMap.MapPairs(colCnt - 1)
            ImportTableName = ContentName
            ImportTableName = Replace(ImportTableName, " ", "_")
            ImportTableName = Replace(ImportTableName, "-", "_")
            ImportTableName = Replace(ImportTableName, ",", "_")
            hint = "060"
            Call csv.createContent(True, "default", ImportTableName, ContentName)
            Call csv.loadContentDefinitions
            hint = "070"
            For colPtr = 0 To colCnt - 1
                hint = "080"
                DBFieldName = cells(colPtr, 0)
                DBFieldName = encodeFieldName(DBFieldName)
                'DBFieldName = Replace(DBFieldName, " ", "_")
                'DBFieldName = Replace(DBFieldName, "-", "_")
                'DBFieldName = Replace(DBFieldName, ",", "_")
                If (DBFieldName = "") Then
                    DBFieldName = "field" & colPtr
                End If
                ImportMap.MapPairs(colPtr).DbField = DBFieldName
                ImportMap.MapPairs(colPtr).DbFieldType = 2
                ImportMap.MapPairs(colPtr).SourceFieldName = DBFieldName
                ImportMap.MapPairs(colPtr).SourceFieldPtr = colPtr
                hint = "090"
                Call csv.createContentField(True, ContentName, DBFieldName, 2)
            Next
            Call csv.loadContentDefinitions
        Else
            If ContentName = "" Then
                ContentName = "People"
            End If
            ImportTableName = csv.GetContentTablename(ImportMap.ContentName)
        End If
        
        If ImportMap.MapPairCnt > 0 Then
            hint = "200"
'            ContentName = ImportMap.ContentName
'            cells = parseFile(Source)
'            colCnt = UBound(cells, 1) + 1
'            rowCnt = UBound(cells, 2) + 1
'            If ImportMap.importToNewContent Then
'                ImportTableName = ContentName
'                ImportTableName = Replace(ImportTableName, " ", "_")
'                ImportTableName = Replace(ImportTableName, "-", "_")
'                ImportTableName = Replace(ImportTableName, ",", "_")
'                Call csv.createContent4(True, "default", ImportTableName, ContentName)
'                For colPtr = 0 To colCnt - 1
'                    DBFieldName = cells(colPtr, 0)
'                    DBFieldName = Replace(DBFieldName, " ", "_")
'                    DBFieldName = Replace(DBFieldName, "-", "_")
'                    DBFieldName = Replace(DBFieldName, ",", "_")
'                    If DBFieldName <> "" Then
'                        Call csv.createContentField2(True, ContentName, DBFieldName, 2)
'                    End If
'                Next
'            Else
'            End If
'
'            'Rows = kmaSplit(source, vbCrLf)
            DbKeyField = ImportMap.DbKeyField
            SourceKeyPtr = kmaEncodeInteger(ImportMap.SourceKeyField)
            KeyMethodID = ImportMap.KeyMethodID
            If (DbKeyField = "") Or (SourceKeyPtr < 0) Then
                KeyMethodID = KeyMethodInsertAll
            End If
            '
            '
            '
            KeyCriteria = "(1=0)"
            For rowPtr = ImportMap.SkipRowCnt To rowCnt - 1
            'For rowPtr = 0 To rowCnt - 1
            'For rowPtr = ImportMap.SkipRowCnt To UBound(Rows)
                hint = "300"
                UpdateRecord = False
                InsertRecord = False
                'row = Rows(rowPtr)
                RowWidth = 0
                If (rowPtr = 76) Or (rowPtr = 105) Then
                    rowPtr = rowPtr
                End If
                If True Then
                'If row <> "" Then
                    hint = "310"
                    'cells = kmaSplit(row, ",")
                    If KeyMethodID = KeyMethodInsertAll Then
                        hint = "320"
                        '
                        ' Insert the record
                        '
'                        CS = CSv.InsertCSRecord(ContentName, 0)
'                        If CSv.IsCSOK(CS) Then
'                            KeyCriteria = "(ID=" & KmaEncodeSQLNumber(CSv.GetCSInteger(CS, "ID")) & ")"
'                        End If
                        InsertRecord = True
                    Else
                        hint = "330"
                        '
                        ' Update or Update-And-Insert, Build Key Criteria
                        '
                        SourceKeyData = cells(SourceKeyPtr, rowPtr)
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
                            hint = "340"
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
                                Case FieldTypeText, FieldTypeResourceLink, FieldTypeLink
                                    '
                                    ' text
                                    '
                                    UpdateRecord = True
                                    KeyCriteria = "(" & DbKeyField & "=" & KmaEncodeSQLText(Left(SourceKeyData, 255)) & ")"
                                Case FieldTypeLongText, FieldTypeHTML
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
                        hint = "400"
                        '
                        ' Build Update SQL
                        '
                        UpdateSQLFieldSet = ""
                        'For fieldPtr = 0 To colCnt - 1
                        For fieldPtr = 0 To ImportMap.MapPairCnt - 1
                            hint = "500"
                            sourcePtr = ImportMap.MapPairs(fieldPtr).SourceFieldPtr
                            If sourcePtr < 0 Then
                                '
                                ' Bad pointer
                                '
                                sourcePtr = sourcePtr
                            ElseIf sourcePtr >= colCnt Then
                            'ElseIf sourcePtr > UBound(cells) Then
                                '
                                ' This data row was not as long as the header row - skip it
                                '
                                sourcePtr = sourcePtr
                            Else
                                hint = "600"
                                DBFieldName = ImportMap.MapPairs(fieldPtr).DbField
                                SourceData = cells(sourcePtr, rowPtr)
                                'SourceData = cells(sourcePtr)
                                'If Len(SourceData) > 2 And Left(SourceData, 1) = """" And Right(SourceData, 1) = """" Then
                                '    SourceData = Trim(Mid(SourceData, 2, Len(SourceData) - 2))
                                'End If
                                'If LCase(SourceData) = "mcgee" Then
                                '    SourceData = SourceData
                                'End If
                                RowWidth = RowWidth + Len(Trim(SourceData))
                                ' there are no fieldtypes defined as 0, and I do not want the CS open now, so we can avoid the insert if rowwidth=0
                                'If ImportMap.MapPairs(FieldPtr).DbFieldType = 0 Then
                                '    Call CSv.SetCS(CS, DBFieldName, SourceData)
                                'Else
                                Select Case ImportMap.MapPairs(fieldPtr).DbFieldType
                                    Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                        '
                                        ' number
                                        '
                                        UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeNumber(SourceData)
                                    Case FieldTypeBoolean
                                        '
                                        ' Boolean
                                        '
                                        UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeSQLBoolean(SourceData)
                                    Case FieldTypeDate
                                        '
                                        ' date
                                        '
                                        UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeSQLDate(SourceData)
                                    Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                    'Case FieldTypeText
                                        '
                                        ' text
                                        '
                                        UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeSQLText(Left(SourceData, 255))
                                    Case FieldTypeLongText, FieldTypeHTML
                                    'Case FieldTypeLongText
                                        '
                                        ' long text
                                        '
                                        UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeSQLLongText(SourceData)
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
                                    '    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & KmaEncodeSQLLongText(SourceData)
                                    Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                    'Case FieldTypeFile, FieldTypeRedirect
                                        '
                                        ' can not import these
                                        '
                                End Select
                            End If
                        Next
                    End If
                    hint = "700"
                    On Error Resume Next
                    'Call CSv.CloseCS(CS)
                    If Err.Number <> 0 Then
                        ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                    ElseIf RowWidth = 0 Then
                        ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " was not imported because it was empty."
                    ElseIf (UpdateSQLFieldSet <> "") Then
                        '
                        ' insert or update the table
                        '
                        If UpdateRecord Then
                            '
                            ' Update requested
                            '
                            On Error Resume Next
                            matchFound = False
                            InsertRecord = False
                            CS = csv.OpenCSContent(ContentName, KeyCriteria, "ID", False)
                            If Err.Number <> 0 Then
                                Err.Clear
                                ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                            Else
                                matchFound = csv.IsCSOK(CS)
                            End If
                            On Error GoTo ErrorTrap
                            Call csv.closecs(CS)
                            '
                            If matchFound Then
                                '
                                ' match was found, update all records found
                                '
                                UpdateRecord = True
                            ElseIf (KeyMethodID = KeyMethodUpdateOnMatchInsertOthers) Then
                                '
                                ' no match, convert to insert if that was requested
                                '
                                InsertRecord = True
                            End If
                        End If
                        If InsertRecord Then
                            '
                            ' Insert a new record and convert to an update
                            '
                            On Error Resume Next
                            UpdateRecord = False
                            CS = csv.InsertCSRecord(ContentName, 0)
                            If Err.Number <> 0 Then
                                Err.Clear
                                ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                            Else
                                If Not csv.IsCSOK(CS) Then
                                    ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported because a record count not be inserted."
                                Else
                                    recordId = csv.GetCSInteger(CS, "ID")
                                    KeyCriteria = "(ID=" & KmaEncodeSQLNumber(recordId) & ")"
                                    UpdateRecord = True
                                End If
                            End If
                            On Error GoTo ErrorTrap
                            Call csv.closecs(CS)
                        End If
                        '
                        ' Update the record if needed
                        '
                        If UpdateRecord Then
                            hint = "900"
                            '
                            ' Update all records in the current recordset
                            '
                            On Error Resume Next
                            UpdateSQL = "update " & ImportTableName & " set " & Mid(UpdateSQLFieldSet, 2) & " where " & KeyCriteria
                            Call csv.ExecuteSQL(csv.GetContentDataSource(ImportMap.ContentName), UpdateSQL)
                            If Err.Number <> 0 Then
                                Err.Clear
                                ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                            End If
                            On Error GoTo ErrorTrap
                        End If
                        If ImportMap.GroupOptionID <> GroupOptionNone Then
                            '
                            ' update/insert OK and records are People
                            '
                            CS = csv.OpenCSContent(ContentName, KeyCriteria, "ID", False)
                            Do While csv.IsCSOK(CS)
                                '
                                ' Add Groups
                                '
                                recordId = csv.GetCSInteger(CS, "ID")
                                Select Case ImportMap.GroupOptionID
                                    Case GroupOptionAll
                                        '
                                        '
                                        '
                                        Call AddGroupMember(recordId, ImportMap.GroupID)
                                    Case GroupOptionOnMatch
                                        '
                                        '
                                        '
                                        If matchFound Then
                                            Call AddGroupMember(recordId, ImportMap.GroupID)
                                        End If
                                    Case GroupOptionOnNoMatch
                                        '
                                        '
                                        '
                                        If Not matchFound Then
                                            Call AddGroupMember(recordId, ImportMap.GroupID)
                                        End If
                                End Select
                                Call csv.NextCSRecord(CS)
                            Loop
                            Call csv.closecs(CS)
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
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "ProcessCSV - Hint=" & hint, True, False)
End Function
'
'===========================================================================
'   Error handler
'===========================================================================
'
Private Function HandleLocalError(ErrNumber As Long, ErrSource As String, ErrDescription As String, MethodName As String, ErrorTrap As Boolean, Optional ResumeNext As Boolean) As String
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
    CS = csv.OpenCSContent("Member Rules", "(MemberID=" & MemberID & ")and(GroupID=" & GroupID & ")")
    If Not csv.IsCSOK(CS) Then
        Call csv.closecs(CS)
        CS = csv.InsertCSRecord("Member Rules", 0)
        If csv.IsCSOK(CS) Then
            Call csv.SetCS(CS, "MemberID", MemberID)
            Call csv.SetCS(CS, "GroupID", GroupID)
        End If
    End If
    Call csv.closecs(CS)
    '
    Exit Sub
    '
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "AddGroupMember", True, False)
End Sub

'
'========================================================================
' encodeFieldName
'
'========================================================================
'
Public Function encodeFieldName(Source As String) As String
    Dim allowed As String
    Dim chr As String
    Dim Ptr As Long
    Dim cnt As Long
    Dim returnString As String
    '
    returnString = ""
    cnt = Len(Source)
    If cnt > 254 Then
        cnt = 254
    End If
    allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_"
    For Ptr = 1 To cnt
        chr = Mid(Source, Ptr, 1)
        If InStr(1, allowed, chr, vbBinaryCompare) Then
            returnString = returnString & chr
        Else
            returnString = returnString & "_"
        End If
    Next
    encodeFieldName = returnString
End Function

