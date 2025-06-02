using System;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.ImportWizard.Controllers {
    public sealed class GenericController {
        private GenericController() {
        }
        // 
        public static string normalizeFilename(string srcFilename) {
            string ext = System.IO.Path.GetExtension(srcFilename);
            string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(srcFilename);
            string result = "";
            string validCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (var c in filenameNoExt.Trim().Replace(" ", "_"))
                result += validCharacters.Contains(Conversions.ToString(c)) ? Conversions.ToString(c) : "-";
            return string.IsNullOrEmpty(ext) ? result : result + "." + ext;
        }
        // 
        public static string Base64Encode(string plainText) {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        // 
        public static string Base64Decode(string base64EncodedData) {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }        // 

        // 
        // ====================================================================================================
        /// <summary>
        /// if date is invalid, set to minValue
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static DateTime encodeMinDate(DateTime srcDate) {
            var returnDate = srcDate;
            if (srcDate < new DateTime(1900, 1, 1)) {
                returnDate = DateTime.MinValue;
            }
            return returnDate;
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// if valid date, return the short date, else return blank string 
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static string getShortDateString(DateTime srcDate) {
            string returnString = "";
            var workingDate = encodeMinDate(srcDate);
            if (!isDateEmpty(srcDate)) {
                returnString = workingDate.ToShortDateString();
            }
            return returnString;
        }
        // 
        // ====================================================================================================
        public static bool isDateEmpty(DateTime srcDate) {
            return srcDate < new DateTime(1900, 1, 1);
        }
        // 
        // ====================================================================================================
        public static string getSortOrderFromInteger(int id) {
            return id.ToString().PadLeft(7, '0');
        }
        // 
        // ====================================================================================================
        public static string getDateForHtmlInput(DateTime source) {
            if (isDateEmpty(source)) {
                return "";
            } else {
                return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, '0') + "-" + source.Day.ToString().PadLeft(2, '0');
            }
        }
        // 
        // ====================================================================================================
        public static string convertToDosPath(string sourcePath) {
            return sourcePath.Replace("/", @"\");
        }
        // 
        // ====================================================================================================
        public static string convertToUnixPath(string sourcePath) {
            return sourcePath.Replace(@"\", "/");
        }
        // 
        // returns true if after removing this field, it is end of line
        // Returns a cell from a csv source and advances the ptr to the start of the next field
        // on entry, ptr points to the first character of the cell
        // if there are spaces, they are included in the cell, unless the first non-space is a quote.
        // if at end of line, the parseFieldReturnEol is true
        // if end of file, return_eof is true
        // 
        public static bool parseFieldReturnEol(string Source, int sourcePtr, ref string return_cell, ref int return_ptr, ref bool return_eof) {
            try {
                bool result;
                // 
                int crPtr;
                int endPtr;
                int Ptr;
                int workingPtr;
                bool IsQuoted;
                int commaptr;
                int lfPtr;
                int crlfPtr;
                string hint;
                // 
                Ptr = sourcePtr;
                IsQuoted = false;
                result = false;
                // 
                // find initial character
                // 
                hint = "no used";
                workingPtr = Ptr;
                while (Strings.Mid(Source, workingPtr, 1) == " ")
                    workingPtr += 1;
                hint += ",110";
                if (Strings.Mid(Source, workingPtr, 1) == "\"") {
                    // 
                    // if first non-space is a quote, ignore the leading spaces
                    // 
                    Ptr = workingPtr;
                    IsQuoted = true;
                }
                hint += ",120";
                if (!IsQuoted) {
                    // 
                    // non-Quoted field
                    // 
                    hint += ",120";
                    commaptr = Strings.InStr(Ptr, Source, ",");
                    lfPtr = Strings.InStr(Ptr, Source, Constants.vbLf);
                    crPtr = Strings.InStr(Ptr, Source, Constants.vbCr);
                    crlfPtr = Strings.InStr(Ptr, Source, Constants.vbCrLf);
                    // 
                    // set workingPtr to the first one found
                    // 
                    workingPtr = firstNonZero(commaptr, crlfPtr);
                    workingPtr = firstNonZero(workingPtr, lfPtr);
                    workingPtr = firstNonZero(workingPtr, crPtr);
                    // workingPtr = firstNonZero(workingPtr, commaptr)
                    workingPtr = firstNonZero(workingPtr, Strings.Len(Source));
                    if (workingPtr == crlfPtr) {
                        // 
                        // end of line for crlf line
                        // 
                        hint += ",140";
                        endPtr = workingPtr - 1;
                        return_cell = Strings.Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 2;
                        result = true;
                    } else if (workingPtr == lfPtr) {
                        // 
                        // end of line for lf line
                        // 
                        hint += ",130";
                        endPtr = workingPtr - 1;
                        return_cell = Strings.Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = true;
                    } else if (workingPtr == crPtr) {
                        // 
                        // end of line for cr line
                        // 
                        hint += ",135";
                        endPtr = workingPtr - 1;
                        return_cell = Strings.Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = true;
                    } else if (workingPtr == commaptr) {
                        // 
                        // end of cell, skip comma
                        // 
                        hint += ",150";
                        endPtr = workingPtr - 1;
                        return_cell = Strings.Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = false;
                    } else {
                        // 
                        // non of the above (non found) might be end of file
                        // 
                        hint += ",160";
                        endPtr = Strings.Len(Source);
                        if (endPtr - sourcePtr + 1 > 0) {
                            return_cell = Strings.Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        } else {
                            return_cell = "";
                        }
                        return_ptr = endPtr;
                        result = true;
                        result = true;
                    }
                } else {
                    // 
                    // Quoted field, pass the initial quote
                    // 
                    hint += ",170";
                    Ptr += 1;
                    int startPtr;
                    startPtr = Ptr;
                    // 
                    while (Ptr != 0 & Strings.InStr(Ptr, Source, "\"") == Strings.InStr(Ptr, Source, "\"\"")) {
                        // pass the doublequote
                        hint += ",200";
                        Ptr = Strings.InStr(Ptr, Source, "\"\"");
                        if (Ptr == 0) {
                            // 
                            // neither quote or double quote were found - end of file error
                            // 
                            hint += ",210";
                            endPtr = Strings.Len(Source);
                            return_cell = "";
                            return_ptr = endPtr;
                            result = true;
                            result = true;
                        } else {
                            hint += ",220";
                            Ptr += 2;
                        }
                    }
                    if (Ptr != 0) {
                        // 
                        // ptr is on the closing quote
                        // 
                        hint += ",230";
                        Ptr = Strings.InStr(Ptr, Source, "\"");
                        endPtr = Ptr - 1;
                        // skip white space to next delimter
                        while (Strings.Mid(Source, Ptr + 1, 1) == " " & Ptr < Strings.Len(Source))
                            Ptr += 1;
                        if (Ptr >= Strings.Len(Source)) {
                            // 
                            // crlf end of line
                            // 
                            hint += ",240";
                            return_cell = Strings.Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 3;
                            result = true;
                        } else if ((Strings.Mid(Source, Ptr + 1, 2) ?? "") == Constants.vbCrLf) {
                            // ***** 20140131 - ptr is to the end quote
                            // If (Mid(Source, Ptr - 1, 2) = vbCrLf) Then
                            // 
                            // crlf end of line
                            // 
                            hint += ",250";
                            return_cell = Strings.Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 3;
                            result = true;
                        } else if ((Strings.Mid(Source, Ptr + 1, 1) ?? "") == Constants.vbLf) {
                            // 
                            // lf end of line
                            // 
                            hint += ",240";
                            return_cell = Strings.Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 2;
                            result = true;
                        } else if ((Strings.Mid(Source, Ptr + 1, 1) ?? "") == Constants.vbCr) {
                            // 
                            // cr end of line
                            // 
                            hint += ",240";
                            return_cell = Strings.Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 2;
                            result = true;
                        } else {
                            // 
                            // not end of line, skip over anything before the next comma
                            // 
                            hint += ",260";
                            return_cell = Strings.Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Strings.InStr(Ptr, Source, ",");
                            // ***** 20140131
                            if (return_ptr <= 0) {
                                // 
                                // end of line, end of file
                                // 
                                result = true;
                            } else {
                                return_ptr += 1;
                                result = false;
                            }
                        }
                        // 
                        // This is quoted text, so anything inside is converted to double quotes -- convert them back.
                        // 
                        return_cell = Strings.Replace(return_cell, "\"\"", "\"");
                        // 
                    }
                }
                // 
                // determine eof
                // 
                hint += ",300";
                if (return_ptr >= Strings.Len(Source)) {
                    return_eof = true;
                }
                return result;
            } catch (Exception ex) {
                throw;
            }
        }
        // 
        // 
        // 
        public static void parseLine(string Source, int source_ptr, ref string[] return_cells, ref int return_ptr, ref bool return_eof) {
            try {
                // 
                string Cell = "";
                var EOL = default(bool);
                int fieldPtr;
                // 
                fieldPtr = 0;
                return_ptr = source_ptr;
                while (!EOL) {
                    int last;
                    last = return_ptr;
                    EOL = parseFieldReturnEol(Source, return_ptr, ref Cell, ref return_ptr, ref return_eof);
                    Array.Resize(ref return_cells, fieldPtr + 1);
                    return_cells[fieldPtr] = Cell;
                    fieldPtr += 1;
                    if (return_ptr == 0) {
                        return_ptr = return_ptr;
                    }
                }
            } catch (Exception ex) {
                throw;
            }
        }
        // 
        // 
        // 
        public static string[,] parseFile(string Source) {
            string[,] parseFileRet = default;
            try {
                string[,] result;
                // 
                bool EOL;
                var srcPtr = default(int);
                int rowPtr;
                int colPtr;
                var eof = default(bool);
                int colCnt;
                int rowCnt;
                string[] dummyCells = Array.Empty<string>();
                // 
                // parse the first row to get colCnt
                // 
                parseLine(Source, 1, ref dummyCells, ref srcPtr, ref eof);
                colCnt = Information.UBound(dummyCells) + 1;
                rowCnt = 0;
                // 
                colPtr = 0;
                rowPtr = 0;
                srcPtr = 1;
                result = new string[colCnt, 1];

                while (!eof) {
                    if (rowPtr == 105) {
                        rowPtr = rowPtr;
                        if (colPtr == 14) {
                            colPtr = colPtr;
                        }
                    }
                    if (rowPtr >= rowCnt) {
                        rowCnt = rowPtr + 1;
                        var oldResult = result;
                        result = new string[colCnt, rowCnt];
                        if (oldResult is not null)
                            for (var i = 0; i <= oldResult.Length / oldResult.GetLength(1) - 1; ++i)
                                Array.Copy(oldResult, i * oldResult.GetLength(1), result, i * result.GetLength(1), Math.Min(oldResult.GetLength(1), result.GetLength(1)));
                    }
                    if (colPtr >= colCnt) {
                        if (rowCnt != 1) {
                            // 
                            // error - can not adjust columns after first row
                            // 
                        } else {
                            colCnt = colPtr + 1;
                            var oldResult1 = result;
                            result = new string[colCnt, rowCnt];
                            if (oldResult1 is not null)
                                for (var i1 = 0; i1 <= oldResult1.Length / oldResult1.GetLength(1) - 1; ++i1)
                                    Array.Copy(oldResult1, i1 * oldResult1.GetLength(1), result, i1 * result.GetLength(1), Math.Min(oldResult1.GetLength(1), result.GetLength(1)));
                        }
                    }
                    if (rowPtr == 79 & colPtr == 2) {
                        rowPtr = rowPtr;
                    }
                    string cell = "";
                    EOL = parseFieldReturnEol(Source, srcPtr, ref cell, ref srcPtr, ref eof);
                    result[colPtr, rowPtr] = cell;

                    if (EOL) {
                        colPtr = 0;
                        rowPtr += 1;
                    } else if (colPtr + 1 < colCnt) {
                        colPtr += 1;
                    } else {
                        colPtr = 0;
                        rowPtr += 1;
                    }
                }
                parseFileRet = result;
                return result;
            } catch (Exception ex) {
                throw;
            }
        }
        // 
        // 
        // 
        public static int firstNonZero(int a, int b) {
            int firstNonZeroRet = default;
            int v;
            v = kmaGetFirstNonZeroLong(a, b);
            if (v == 1) {
                firstNonZeroRet = a;
            } else if (v == 2) {
                firstNonZeroRet = b;
            } else {
                firstNonZeroRet = 0;
            }

            return firstNonZeroRet;
        }
        // 
        public static int kmaGetFirstNonZeroLong(int a, int b) {
            if (a == 0 & b == 0) {
                return 0;
            } else if (a == 0) {
                return 2;
            } else if (b == 0 | a < b) {
                return 1;
            } else {
                return 2;
            }
        }
    }
}