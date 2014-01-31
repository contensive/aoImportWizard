Attribute VB_Name = "ImportWizardModule"

Option Explicit
'
' forms
'
Public Const SubFormSource = 1
Public Const SubFormSourceUpload = 2
Public Const SubFormSourceUploadFolder = 3
Public Const SubFormSourceResourceLibrary = 4
Public Const SubFormNewMapping = 5
Public Const SubFormGroup = 6
Public Const SubFormKey = 7
Public Const SubFormFinish = 8
Public Const SubFormDestination = 9
'
Public Const SubFormMax = 9
'
Public Const ImportSourceUpload = 1
Public Const ImportSourceUploadFolder = 2
Public Const ImportSourceResourceLibrary = 3
'
Public Const KeyMethodInsertAll = 1
Public Const KeyMethodUpdateOnMatch = 2
Public Const KeyMethodUpdateOnMatchInsertOthers = 3
'
Public Const GroupOptionNone = 1
Public Const GroupOptionAll = 2
Public Const GroupOptionOnMatch = 3
Public Const GroupOptionOnNoMatch = 4
'
' ----- Buttons
'
'
' ----- local scope variables
'
'
' ----- Request Names
'
Public Const RequestNameSubForm = "SubForm"
Public Const RequestNameImportWizardID = "ImportWizardID"
Public Const RequestNameImportSource = "ImportWizardSource"
Public Const RequestNameImportContentID = "ImportWizardDestination"
Public Const RequestNameImportUpload = "ImportWizardUpload"
Public Const RequestNameImportKeyMethodID = "ImportWizardKeyMethodID"
Public Const RequestNameImportSourceKeyFieldPtr = "ImportWizardSourceKeyFieldPtr"
Public Const RequestNameImportDbKeyField = "ImportWizardDbkeyField"
Public Const RequestNameImportGroupID = "ImportGroupID"
Public Const RequestNameImportGroupNew = "inputNewGroupName"
Public Const RequestNameImportGroupOptionID = "ImportGroupOptionID"
Public Const RequestNameImportEmail = "ImportEmailNotify"
Public Const RequestNameImportMapFile = "ImportMapFile"
'Public Const RequestNameImportContentName = "ImportContentName"
Public Const RequestNameImportSkipFirstRow = "ImportSkipFirstRow"
'
' ----- Types
'
Public Type WizardType
    '
    ' Attributes
    '
    'SendMethodID As Long
    '
    ' Form 'includes'
    '
    'IncludeTemplateForm As Boolean
    '
    ' Value Defaults
    '
    'DefaultTemplateID As Long
    '
    ' Instructions
    '
    SourceFormInstructions As String
    UploadFormInstructions As String
    MappingFormInstructions As String
    GroupFormInstructions As String
    KeyFormInstructions As String
    '
    ' Current calculated Path
    '
    Path() As Long
    PathCnt As Long
End Type
'
Public Type MapPairType
    SourceFieldPtr As Long
    SourceFieldName As String
    DbField As String
    DbFieldType As Long
End Type
'
Public Type ImportMapType
    ContentName As String
    KeyMethodID As Long
    SourceKeyField As String
    DbKeyField As String
    DbKeyFieldType As Long
    GroupOptionID As Long
    GroupID As Long
    SkipRowCnt As Long
    MapPairCnt As Long
    MapPairs() As MapPairType
End Type
'
'
'
Public Function LoadImportMap(ImportMapData As String) As ImportMapType
    On Error GoTo ErrorTrap
    '
    Dim Rows() As String
    Dim Pair() As String
    Dim Ptr As Long
    Dim SourceSplit() As String
    Dim MapPtr As Long
    '
    If ImportMapData = "" Then
        '
        ' Defaults
        '
        LoadImportMap.ContentName = "People"
        LoadImportMap.GroupID = 0
        LoadImportMap.MapPairCnt = 0
        LoadImportMap.SkipRowCnt = 1
    Else
        '
        ' read in what must be saved
        '
        Rows = Split(ImportMapData, vbCrLf)
        If UBound(Rows) <= 6 Then
            '
            ' Map file is bad
            '
            'Call HandleLocalError(KmaErrorInternal, App.EXEName, "ImportWizard.LoadImportMap failed because there was a problem with the format of the data", "LoadImportMap", False, True)
        Else
            LoadImportMap.KeyMethodID = kmaEncodeInteger(Rows(0))
            LoadImportMap.SourceKeyField = Rows(1)
            LoadImportMap.DbKeyField = Rows(2)
            LoadImportMap.ContentName = Rows(3)
            LoadImportMap.GroupOptionID = kmaEncodeInteger(Rows(4))
            LoadImportMap.GroupID = kmaEncodeInteger(Rows(5))
            LoadImportMap.SkipRowCnt = kmaEncodeInteger(Rows(6))
            LoadImportMap.DbKeyFieldType = kmaEncodeInteger(Rows(7))
            LoadImportMap.MapPairCnt = 0
            '
            If UBound(Rows) > 8 Then
                If Trim(Rows(8)) = "" Then
                    For Ptr = 9 To UBound(Rows)
                        Pair = Split(Rows(Ptr), "=")
                        If UBound(Pair) > 0 Then
                            MapPtr = LoadImportMap.MapPairCnt
                            LoadImportMap.MapPairCnt = MapPtr + 1
                            ReDim Preserve LoadImportMap.MapPairs(MapPtr)
                            LoadImportMap.MapPairs(MapPtr).DbField = Pair(0)
                            SourceSplit = Split(Pair(1), ",")
                            If UBound(SourceSplit) > 0 Then
                                LoadImportMap.MapPairs(MapPtr).SourceFieldPtr = kmaEncodeInteger(SourceSplit(0))
                                LoadImportMap.MapPairs(MapPtr).DbFieldType = kmaEncodeInteger(SourceSplit(1))
                            End If
                        End If
                    Next
                End If
            End If
        End If
    
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "LoadImportMap", True, False)
End Function
'
'===========================================================================
'   Error handler
'===========================================================================
'
Public Function HandleLocalError(ErrNumber As Long, ErrSource As String, ErrDescription As String, MethodName As String, ErrorTrap As Boolean, Optional ResumeNext As Boolean) As String
    '
    Call HandleError("ImportWizardModule", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext)
    '
End Function
'
'   returns true if after removing this field, it is end of line
'   Returns a cell from a csv source and advances the ptr to the start of the next field
'   on entry, ptr points to the first character of the cell
'   if there are spaces, they are included in the cell, unless the first non-space is a quote.
'   if at end of line, the parseFieldReturnEol is true
'   if end of file, return_eof is true
'
Public Function parseFieldReturnEol(Source As String, sourcePtr As Long, return_cell As String, return_ptr As Long, return_eof As Boolean) As Boolean
    On Error GoTo ErrorTrap
    '
    Dim crPtr As Long
    Dim endPtr As Long
    Dim quotePtr As Long
    Dim dblQuotePtr As Long
    Dim Ptr As Long
    Dim workingPtr As Long
    Dim IsQuoted As Boolean
    Dim commaptr As Long
    Dim lfPtr As Long
    Dim crlfPtr As Long
    Dim cellLen As Long
    Dim hint As String
    '
    Ptr = sourcePtr
    IsQuoted = False
    parseFieldReturnEol = False
    '
    ' find initial character
    '
    hint = "no used"
    workingPtr = Ptr
    Do While Mid(Source, workingPtr, 1) = " "
        workingPtr = workingPtr + 1
    Loop
    hint = hint & ",110"
    If Mid(Source, workingPtr, 1) = """" Then
        '
        ' if first non-space is a quote, ignore the leading spaces
        '
        Ptr = workingPtr
        IsQuoted = True
    End If
    hint = hint & ",120"
    If Not IsQuoted Then
        '
        ' non-Quoted field
        '
        hint = hint & ",120"
        commaptr = InStr(Ptr, Source, ",")
        lfPtr = InStr(Ptr, Source, vbLf)
        crPtr = InStr(Ptr, Source, vbCr)
        crlfPtr = InStr(Ptr, Source, vbCrLf)
        '
        ' set workingPtr to the first one found
        '
        workingPtr = firstNonZero(commaptr, crlfPtr)
        workingPtr = firstNonZero(workingPtr, lfPtr)
        workingPtr = firstNonZero(workingPtr, crPtr)
        'workingPtr = firstNonZero(workingPtr, commaptr)
        workingPtr = firstNonZero(workingPtr, Len(Source))
        If (workingPtr = crlfPtr) Then
            '
            ' end of line for crlf line
            '
            hint = hint & ",140"
            endPtr = workingPtr - 1
            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
            return_ptr = workingPtr + 2
            parseFieldReturnEol = True
        ElseIf (workingPtr = lfPtr) Then
            '
            ' end of line for lf line
            '
            hint = hint & ",130"
            endPtr = workingPtr - 1
            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
            return_ptr = workingPtr + 1
            parseFieldReturnEol = True
        ElseIf (workingPtr = crPtr) Then
            '
            ' end of line for cr line
            '
            hint = hint & ",135"
            endPtr = workingPtr - 1
            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
            return_ptr = workingPtr + 1
            parseFieldReturnEol = True
        ElseIf (workingPtr = commaptr) Then
            '
            ' end of cell, skip comma
            '
            hint = hint & ",150"
            endPtr = workingPtr - 1
            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
            return_ptr = workingPtr + 1
            parseFieldReturnEol = False
        Else
            '
            ' non of the above (non found) might be end of file
            '
            hint = hint & ",160"
            endPtr = Len(Source)
            If (endPtr - sourcePtr + 1 > 0) Then
                return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
            Else
                return_cell = ""
            End If
            return_ptr = endPtr
            parseFieldReturnEol = True
            parseFieldReturnEol = True
        End If
    Else
        '
        ' Quoted field, pass the initial quote
        '
        hint = hint & ",170"
        Ptr = Ptr + 1
Dim startPtr As Long
        startPtr = Ptr
        '
        Do While (Ptr <> 0) And (InStr(Ptr, Source, """") = InStr(Ptr, Source, """"""))
            ' pass the doublequote
            hint = hint & ",200"
            Ptr = InStr(Ptr, Source, """""")
            If (Ptr = 0) Then
                '
                ' neither quote or double quote were found - end of file error
                '
                hint = hint & ",210"
                endPtr = Len(Source)
                return_cell = ""
                return_ptr = endPtr
                parseFieldReturnEol = True
                parseFieldReturnEol = True
            Else
                hint = hint & ",220"
                Ptr = Ptr + 2
            End If
        Loop
        If Ptr <> 0 Then
            '
            ' ptr is on the closing quote
            '
            hint = hint & ",230"
            Ptr = InStr(Ptr, Source, """")
            endPtr = Ptr - 1
            ' skip white space to next delimter
            Do While (Mid(Source, Ptr + 1, 1) = " ") And (Ptr < Len(Source))
                Ptr = Ptr + 1
            Loop
            If (Ptr >= Len(Source)) Then
                '
                ' crlf end of line
                '
                hint = hint & ",240"
                return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                return_ptr = Ptr + 3
                parseFieldReturnEol = True
            ElseIf (Mid(Source, Ptr + 1, 2) = vbCrLf) Then
            ' ***** 20140131 - ptr is to the end quote
            'If (Mid(Source, Ptr - 1, 2) = vbCrLf) Then
                '
                ' crlf end of line
                '
                hint = hint & ",250"
                return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                return_ptr = Ptr + 3
                parseFieldReturnEol = True
            ElseIf (Mid(Source, Ptr + 1, 1) = vbLf) Then
                '
                ' lf end of line
                '
                hint = hint & ",240"
                return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                return_ptr = Ptr + 2
                parseFieldReturnEol = True
            ElseIf (Mid(Source, Ptr + 1, 1) = vbCr) Then
                '
                ' cr end of line
                '
                hint = hint & ",240"
                return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                return_ptr = Ptr + 2
                parseFieldReturnEol = True
            Else
                '
                ' not end of line, skip over anything before the next comma
                '
                hint = hint & ",260"
                return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                return_ptr = InStr(Ptr, Source, ",")
                ' ***** 20140131
                If return_ptr <= 0 Then
                    '
                    ' end of line, end of file
                    '
                    parseFieldReturnEol = True
                Else
                    'If return_ptr > 0 Then
                    return_ptr = return_ptr + 1
                    parseFieldReturnEol = False
                End If
            End If
        End If
    End If
    '
    ' determine eof
    '
    hint = hint & ",300"
    If return_ptr >= Len(Source) Then
        return_eof = True
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "parseFieldReturnEol, hint = " & hint, True, False)
End Function
'
'
'
Public Sub parseLine(Source As String, source_ptr As Long, return_cells() As String, return_ptr As Long, return_eof As Boolean)
    On Error GoTo ErrorTrap
    '
    Dim Cell As String
    Dim Ptr As Long
    Dim EOL As Boolean
    Dim fieldPtr As Long
    '
    fieldPtr = 0
    return_ptr = source_ptr
    Do While Not EOL
Dim last As Long
last = return_ptr
        EOL = parseFieldReturnEol(Source, return_ptr, Cell, return_ptr, return_eof)
        ReDim Preserve return_cells(fieldPtr)
        return_cells(fieldPtr) = Cell
        fieldPtr = fieldPtr + 1
        If return_ptr = 0 Then
            return_ptr = return_ptr
        End If
    Loop
    '
    Exit Sub
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "parseLine", True, False)
End Sub
'
'
'
Public Function parseFile(Source As String) As Variant
    On Error GoTo ErrorTrap
    '
    Dim Cell As String
    Dim Ptr As Long
    Dim EOL As Boolean
    Dim srcPtr As Long
    Dim rowPtr As Long
    Dim colPtr As Long
    Dim eof As Boolean
    Dim cells() As String
    Dim colCnt As Long
    Dim rowCnt As Long
    Dim dummyCells() As String
    '
    ' parse the first row to get colCnt
    '
    Call parseLine(Source, 1, dummyCells, srcPtr, eof)
    colCnt = UBound(dummyCells) + 1
    rowCnt = 0
    '
    colPtr = 0
    rowPtr = 0
    srcPtr = 1
    ReDim cells(colCnt - 1, 0)
    Do While Not eof
If rowPtr = 105 Then
    rowPtr = rowPtr
    If colPtr = 14 Then
        colPtr = colPtr
    End If
End If
        If rowPtr >= rowCnt Then
            rowCnt = rowPtr + 1
            ReDim Preserve cells(colCnt - 1, rowCnt - 1)
        End If
        If colPtr >= colCnt Then
            If rowCnt <> 1 Then
                '
                ' error - can not adjust columns after first row
                '
                Call HandleError("ImportWizardModule", "parseFile", KmaErrorInternal, App.EXEName, "Error in file, row [" & rowCnt & "] has [" & colPtr & "] columns, but only [" & colCnt & "] columns were found in the first row. This is not allowed.", False, False)
            Else
                colCnt = colPtr + 1
                ReDim Preserve cells(colCnt - 1, rowCnt - 1)
            End If
        End If
        If (rowPtr = 79) And (colPtr = 2) Then
            rowPtr = rowPtr
        End If
        EOL = parseFieldReturnEol(Source, srcPtr, Cell, srcPtr, eof)
        cells(colPtr, rowPtr) = Cell
        If EOL Then
            colPtr = 0
            rowPtr = rowPtr + 1
        Else
            colPtr = colPtr + 1
        End If
    Loop
    parseFile = cells
    '
    Exit Function
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "parseFile", True, False)
End Function
'
'
'
Private Function firstNonZero(a As Long, b As Long) As Long
    Dim v As Long
    v = kmaGetFirstNonZeroLong(a, b)
    If v = 1 Then
        firstNonZero = a
    ElseIf v = 2 Then
        firstNonZero = b
    Else
        firstNonZero = 0
    End If
End Function
