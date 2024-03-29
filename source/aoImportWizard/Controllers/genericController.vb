﻿
Imports Contensive.ImportWizard.Models
Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard.Controllers
    Public NotInheritable Class GenericController
        Private Sub New()
        End Sub
        '
        Public Shared Function normalizeFilename(srcFilename As String) As String
            Dim ext As String = IO.Path.GetExtension(srcFilename)
            Dim filenameNoExt As String = IO.Path.GetFileNameWithoutExtension(srcFilename)
            Dim result As String = ""
            Dim validCharacters As String = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            For Each c In filenameNoExt.Trim().Replace(" "c, "_")
                result &= If(validCharacters.Contains(c), c, "-")
            Next
            Return If(String.IsNullOrEmpty(ext), result, result & "." & ext)
        End Function
        '
        Public Shared Function Base64Encode(ByVal plainText As String) As String
            Dim plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText)
            Return System.Convert.ToBase64String(plainTextBytes)
        End Function
        '
        Public Shared Function Base64Decode(ByVal base64EncodedData As String) As String
            Dim base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData)
            Return System.Text.Encoding.UTF8.GetString(base64EncodedBytes)
        End Function        '

        '
        '====================================================================================================
        ''' <summary>
        ''' if date is invalid, set to minValue
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function encodeMinDate(srcDate As DateTime) As DateTime
            Dim returnDate As DateTime = srcDate
            If srcDate < New DateTime(1900, 1, 1) Then
                returnDate = DateTime.MinValue
            End If
            Return returnDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' if valid date, return the short date, else return blank string 
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function getShortDateString(srcDate As DateTime) As String
            Dim returnString As String = ""
            Dim workingDate As DateTime = encodeMinDate(srcDate)
            If Not isDateEmpty(srcDate) Then
                returnString = workingDate.ToShortDateString()
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        Public Shared Function isDateEmpty(srcDate As DateTime) As Boolean
            Return (srcDate < New DateTime(1900, 1, 1))
        End Function
        '
        '====================================================================================================
        Public Shared Function getSortOrderFromInteger(id As Integer) As String
            Return id.ToString().PadLeft(7, "0"c)
        End Function
        '
        '====================================================================================================
        Public Shared Function getDateForHtmlInput(source As DateTime) As String
            If isDateEmpty(source) Then
                Return ""
            Else
                Return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, "0"c) + "-" + source.Day.ToString().PadLeft(2, "0"c)
            End If
        End Function
        '
        '====================================================================================================
        Public Shared Function convertToDosPath(sourcePath As String) As String
            Return sourcePath.Replace("/", "\")
        End Function
        '
        '====================================================================================================
        Public Shared Function convertToUnixPath(sourcePath As String) As String
            Return sourcePath.Replace("\", "/")
        End Function
        '
        '   returns true if after removing this field, it is end of line
        '   Returns a cell from a csv source and advances the ptr to the start of the next field
        '   on entry, ptr points to the first character of the cell
        '   if there are spaces, they are included in the cell, unless the first non-space is a quote.
        '   if at end of line, the parseFieldReturnEol is true
        '   if end of file, return_eof is true
        '
        Public Shared Function parseFieldReturnEol(Source As String, sourcePtr As Integer, ByRef return_cell As String, ByRef return_ptr As Integer, ByRef return_eof As Boolean) As Boolean
            Try
                Dim result As Boolean
                '
                Dim crPtr As Integer
                Dim endPtr As Integer
                Dim Ptr As Integer
                Dim workingPtr As Integer
                Dim IsQuoted As Boolean
                Dim commaptr As Integer
                Dim lfPtr As Integer
                Dim crlfPtr As Integer
                Dim hint As String
                '
                Ptr = sourcePtr
                IsQuoted = False
                result = False
                '
                ' find initial character
                '
                hint = "no used"
                workingPtr = Ptr
                Do While Mid(Source, workingPtr, 1) = " "
                    workingPtr += 1
                Loop
                hint &= ",110"
                If Mid(Source, workingPtr, 1) = """" Then
                    '
                    ' if first non-space is a quote, ignore the leading spaces
                    '
                    Ptr = workingPtr
                    IsQuoted = True
                End If
                hint &= ",120"
                If Not IsQuoted Then
                    '
                    ' non-Quoted field
                    '
                    hint &= ",120"
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
                        hint &= ",140"
                        endPtr = workingPtr - 1
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                        return_ptr = workingPtr + 2
                        result = True
                    ElseIf (workingPtr = lfPtr) Then
                        '
                        ' end of line for lf line
                        '
                        hint &= ",130"
                        endPtr = workingPtr - 1
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                        return_ptr = workingPtr + 1
                        result = True
                    ElseIf (workingPtr = crPtr) Then
                        '
                        ' end of line for cr line
                        '
                        hint &= ",135"
                        endPtr = workingPtr - 1
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                        return_ptr = workingPtr + 1
                        result = True
                    ElseIf (workingPtr = commaptr) Then
                        '
                        ' end of cell, skip comma
                        '
                        hint &= ",150"
                        endPtr = workingPtr - 1
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                        return_ptr = workingPtr + 1
                        result = False
                    Else
                        '
                        ' non of the above (non found) might be end of file
                        '
                        hint &= ",160"
                        endPtr = Len(Source)
                        If (endPtr - sourcePtr + 1 > 0) Then
                            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1)
                        Else
                            return_cell = ""
                        End If
                        return_ptr = endPtr
                        result = True
                        result = True
                    End If
                Else
                    '
                    ' Quoted field, pass the initial quote
                    '
                    hint &= ",170"
                    Ptr += 1
                    Dim startPtr As Integer
                    startPtr = Ptr
                    '
                    Do While (Ptr <> 0) And (InStr(Ptr, Source, """") = InStr(Ptr, Source, """"""))
                        ' pass the doublequote
                        hint &= ",200"
                        Ptr = InStr(Ptr, Source, """""")
                        If (Ptr = 0) Then
                            '
                            ' neither quote or double quote were found - end of file error
                            '
                            hint &= ",210"
                            endPtr = Len(Source)
                            return_cell = ""
                            return_ptr = endPtr
                            result = True
                            result = True
                        Else
                            hint &= ",220"
                            Ptr += 2
                        End If
                    Loop
                    If Ptr <> 0 Then
                        '
                        ' ptr is on the closing quote
                        '
                        hint &= ",230"
                        Ptr = InStr(Ptr, Source, """")
                        endPtr = Ptr - 1
                        ' skip white space to next delimter
                        Do While (Mid(Source, Ptr + 1, 1) = " ") And (Ptr < Len(Source))
                            Ptr += 1
                        Loop
                        If (Ptr >= Len(Source)) Then
                            '
                            ' crlf end of line
                            '
                            hint &= ",240"
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                            return_ptr = Ptr + 3
                            result = True
                        ElseIf (Mid(Source, Ptr + 1, 2) = vbCrLf) Then
                            ' ***** 20140131 - ptr is to the end quote
                            'If (Mid(Source, Ptr - 1, 2) = vbCrLf) Then
                            '
                            ' crlf end of line
                            '
                            hint &= ",250"
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                            return_ptr = Ptr + 3
                            result = True
                        ElseIf (Mid(Source, Ptr + 1, 1) = vbLf) Then
                            '
                            ' lf end of line
                            '
                            hint &= ",240"
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                            return_ptr = Ptr + 2
                            result = True
                        ElseIf (Mid(Source, Ptr + 1, 1) = vbCr) Then
                            '
                            ' cr end of line
                            '
                            hint &= ",240"
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                            return_ptr = Ptr + 2
                            result = True
                        Else
                            '
                            ' not end of line, skip over anything before the next comma
                            '
                            hint &= ",260"
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1)
                            return_ptr = InStr(Ptr, Source, ",")
                            ' ***** 20140131
                            If return_ptr <= 0 Then
                                '
                                ' end of line, end of file
                                '
                                result = True
                            Else
                                return_ptr += 1
                                result = False
                            End If
                        End If
                        '
                        ' This is quoted text, so anything inside is converted to double quotes -- convert them back.
                        '
                        return_cell = Replace(return_cell, """""", """")
                        '
                    End If
                End If
                '
                ' determine eof
                '
                hint &= ",300"
                If return_ptr >= Len(Source) Then
                    return_eof = True
                End If
                Return result
            Catch ex As Exception
                Throw
            End Try
        End Function
        '
        '
        '
        Public Shared Sub parseLine(Source As String, source_ptr As Integer, ByRef return_cells() As String, ByRef return_ptr As Integer, ByRef return_eof As Boolean)
            Try
                '
                Dim Cell As String = ""
                Dim EOL As Boolean
                Dim fieldPtr As Integer
                '
                fieldPtr = 0
                return_ptr = source_ptr
                Do While Not EOL
                    Dim last As Integer
                    last = return_ptr
                    EOL = parseFieldReturnEol(Source, return_ptr, Cell, return_ptr, return_eof)
                    ReDim Preserve return_cells(fieldPtr)
                    return_cells(fieldPtr) = Cell
                    fieldPtr += 1
                    If return_ptr = 0 Then
                        return_ptr = return_ptr
                    End If
                Loop
            Catch ex As Exception
                Throw
            End Try
        End Sub
        '
        '
        '
        Public Shared Function parseFile(Source As String) As String(,)
            Try
                Dim result As String(,)
                '
                Dim EOL As Boolean
                Dim srcPtr As Integer
                Dim rowPtr As Integer
                Dim colPtr As Integer
                Dim eof As Boolean
                Dim colCnt As Integer
                Dim rowCnt As Integer
                Dim dummyCells As String() = Array.Empty(Of String)
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
                            ' error - can not adjust columns after first row
                            '
                        Else
                            colCnt = colPtr + 1
                            ReDim Preserve result(colCnt - 1, rowCnt - 1)
                        End If
                    End If
                    If (rowPtr = 79) And (colPtr = 2) Then
                        rowPtr = rowPtr
                    End If
                    Dim cell As String = ""
                    EOL = parseFieldReturnEol(Source, srcPtr, cell, srcPtr, eof)
                    result(colPtr, rowPtr) = cell

                    If EOL Then
                        colPtr = 0
                        rowPtr += 1
                    Else
                        If colPtr + 1 < colCnt Then
                            colPtr += 1
                        Else
                            colPtr = 0
                            rowPtr += 1
                        End If
                    End If
                Loop
                parseFile = result
                Return result
            Catch ex As Exception
                Throw
            End Try
        End Function
        '
        '
        '
        Public Shared Function firstNonZero(a As Integer, b As Integer) As Integer
            Dim v As Integer
            v = kmaGetFirstNonZeroLong(a, b)
            If v = 1 Then
                firstNonZero = a
            ElseIf v = 2 Then
                firstNonZero = b
            Else
                firstNonZero = 0
            End If
        End Function
        '
        Public Shared Function kmaGetFirstNonZeroLong(a As Integer, b As Integer) As Integer
            If (a = 0) And (b = 0) Then
                Return 0
            ElseIf (a = 0) Then
                Return 2
            ElseIf (b = 0) Or (a < b) Then
                Return 1
            Else
                Return 2
            End If
        End Function
    End Class
End Namespace

