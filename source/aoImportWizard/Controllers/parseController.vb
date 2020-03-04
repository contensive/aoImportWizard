Imports Contensive.Addons.ImportWizard.Controllers.genericController
Namespace Controllers
    Public Class parseController
        '
        Public Function parseCsvData(Source As String) As String(,)
            Dim result As String(,)
            Try
                '
                Dim Cell As String = ""
                Dim EOL As Boolean
                Dim srcPtr As Integer
                Dim rowPtr As Integer
                Dim colPtr As Integer
                Dim eof As Boolean
                Dim colCnt As Integer
                Dim rowCnt As Integer
                Dim dummyCells As String() = {}
                '
                ' parse the first row to get colCnt
                '
                Call parseCsvData_Line(Source, 1, dummyCells, srcPtr, eof)
                colCnt = UBound(dummyCells) + 1
                rowCnt = 0
                '
                colPtr = 0
                rowPtr = 0
                srcPtr = 1
                ReDim result(colCnt - 1, 0)
                Do While Not eof
                    If rowPtr = 105 Then
                        rowPtr = rowPtr
                        If colPtr = 14 Then
                            colPtr = colPtr
                        End If
                    End If
                    If rowPtr >= rowCnt Then
                        rowCnt = rowPtr + 1
                        ReDim Preserve result(colCnt - 1, rowCnt - 1)
                    End If
                    If colPtr >= colCnt Then
                        If rowCnt <> 1 Then
                            '
                        Else
                            colCnt = colPtr + 1
                            ReDim Preserve result(colCnt - 1, rowCnt - 1)
                        End If
                    End If
                    If (rowPtr = 79) And (colPtr = 2) Then
                        rowPtr = rowPtr
                    End If
                    EOL = parseCsvData_FieldReturnEOL(Source, srcPtr, Cell, srcPtr, eof)
                    result(colPtr, rowPtr) = Cell
                    If EOL Then
                        colPtr = 0
                        rowPtr = rowPtr + 1
                    Else
                        colPtr = colPtr + 1
                    End If
                Loop
            Catch ex As Exception
                Throw
            End Try
            Return result
        End Function
        '
        '
        '
        Private Sub parseCsvData_Line(Source As String, source_ptr As Integer, return_cells() As String, return_ptr As Integer, return_eof As Boolean)
            On Error GoTo ErrorTrap
            '
            Dim Ptr As Integer
            Dim EOL As Boolean
            Dim fieldPtr As Integer
            '
            fieldPtr = 0
            return_ptr = source_ptr
            Do While Not EOL
                Dim last As Integer
                last = return_ptr
                Dim Cell As String = ""
                EOL = parseCsvData_FieldReturnEOL(Source, return_ptr, Cell, return_ptr, return_eof)
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
            'Call HandleLocalError(Err.Number, Err.Source, Err.Description, "parseLine", True, False)
        End Sub
        '
        '   returns true if after removing this field, it is end of line
        '   Returns a cell from a csv source and advances the ptr to the start of the next field
        '   on entry, ptr points to the first character of the cell
        '   if there are spaces, they are included in the cell, unless the first non-space is a quote.
        '   if at end of line, the parseFieldReturnEol is true
        '   if end of file, return_eof is true
        '
        Private Function parseCsvData_FieldReturnEOL(Source As String, sourcePtr As Integer, ByRef return_cell As String, ByRef return_ptr As Integer, ByRef return_eof As Boolean) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim crPtr As Integer
            Dim endPtr As Integer
            Dim quotePtr As Integer
            Dim dblQuotePtr As Integer
            Dim Ptr As Integer
            Dim workingPtr As Integer
            Dim IsQuoted As Boolean
            Dim commaptr As Integer
            Dim lfPtr As Integer
            Dim crlfPtr As Integer
            Dim cellLen As Integer
            Dim hint As String
            '
            Ptr = sourcePtr
            IsQuoted = False
            parseCsvData_FieldReturnEOL = False
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
                    parseCsvData_FieldReturnEOL = True
                ElseIf (workingPtr = lfPtr) Then
                    '
                    ' end of line for lf line
                    '
                    hint = hint & ",130"
                    endPtr = workingPtr - 1
                    return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                    return_ptr = workingPtr + 1
                    parseCsvData_FieldReturnEOL = True
                ElseIf (workingPtr = crPtr) Then
                    '
                    ' end of line for cr line
                    '
                    hint = hint & ",135"
                    endPtr = workingPtr - 1
                    return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                    return_ptr = workingPtr + 1
                    parseCsvData_FieldReturnEOL = True
                ElseIf (workingPtr = commaptr) Then
                    '
                    ' end of cell, skip comma
                    '
                    hint = hint & ",150"
                    endPtr = workingPtr - 1
                    return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                    return_ptr = workingPtr + 1
                    parseCsvData_FieldReturnEOL = False
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
                    parseCsvData_FieldReturnEOL = True
                    parseCsvData_FieldReturnEOL = True
                End If
            Else
                '
                ' Quoted field, pass the initial quote
                '
                hint = hint & ",170"
                Ptr = Ptr + 1
                Dim startPtr As Integer
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
                        parseCsvData_FieldReturnEOL = True
                        parseCsvData_FieldReturnEOL = True
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
                        parseCsvData_FieldReturnEOL = True
                    ElseIf (Mid(Source, Ptr + 1, 2) = vbCrLf) Then
                        ' ***** 20140131 - ptr is to the end quote
                        'If (Mid(Source, Ptr - 1, 2) = vbCrLf) Then
                        '
                        ' crlf end of line
                        '
                        hint = hint & ",250"
                        return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                        return_ptr = Ptr + 3
                        parseCsvData_FieldReturnEOL = True
                    ElseIf (Mid(Source, Ptr + 1, 1) = vbLf) Then
                        '
                        ' lf end of line
                        '
                        hint = hint & ",240"
                        return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                        return_ptr = Ptr + 2
                        parseCsvData_FieldReturnEOL = True
                    ElseIf (Mid(Source, Ptr + 1, 1) = vbCr) Then
                        '
                        ' cr end of line
                        '
                        hint = hint & ",240"
                        return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                        return_ptr = Ptr + 2
                        parseCsvData_FieldReturnEOL = True
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
                            parseCsvData_FieldReturnEOL = True
                        Else
                            'If return_ptr > 0 Then
                            return_ptr = return_ptr + 1
                            parseCsvData_FieldReturnEOL = False
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
            'Call HandleLocalError(Err.Number, Err.Source, Err.Description, "parseFieldReturnEol, hint = " & hint, True, False)
        End Function
        '
        '
        '
        Private Function firstNonZero(a As Integer, b As Integer) As Integer
            Dim v As Integer
            Dim result As String = ""
            Try


                v = kmaGetFirstNonZeroLong(a, b)
                If v = 1 Then
                    firstNonZero = a
                ElseIf v = 2 Then
                    firstNonZero = b
                Else
                    firstNonZero = 0
                End If
            Catch ex As Exception
                Throw
            End Try
        End Function
        '
        '




    End Class
End Namespace
