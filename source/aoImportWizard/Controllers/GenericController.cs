using System;

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
            foreach (var c in filenameNoExt.Trim().Replace(" ", "_")) {
                result += validCharacters.Contains(c.ToString()) ? c.ToString() : "-";
            }
            return string.IsNullOrEmpty(ext) ? result : $"{result}.{ext}";
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
        }
        //
        /// <summary>
        /// if date is invalid, set to minValue
        /// </summary>
        public static DateTime encodeMinDate(DateTime srcDate) {
            var returnDate = srcDate;
            if (srcDate < new DateTime(1900, 1, 1)) {
                returnDate = DateTime.MinValue;
            }
            return returnDate;
        }
        //
        /// <summary>
        /// if valid date, return the short date, else return blank string
        /// </summary>
        public static string getShortDateString(DateTime srcDate) {
            string returnString = "";
            var workingDate = encodeMinDate(srcDate);
            if (!isDateEmpty(srcDate)) {
                returnString = workingDate.ToShortDateString();
            }
            return returnString;
        }
        //
        public static bool isDateEmpty(DateTime srcDate) {
            return srcDate < new DateTime(1900, 1, 1);
        }
        //
        public static string getSortOrderFromInteger(int id) {
            return id.ToString().PadLeft(7, '0');
        }
        //
        public static string getDateForHtmlInput(DateTime source) {
            if (isDateEmpty(source)) {
                return "";
            } else {
                return $"{source.Year}-{source.Month.ToString().PadLeft(2, '0')}-{source.Day.ToString().PadLeft(2, '0')}";
            }
        }
        //
        public static string convertToDosPath(string sourcePath) {
            return sourcePath.Replace("/", @"\");
        }
        //
        public static string convertToUnixPath(string sourcePath) {
            return sourcePath.Replace(@"\", "/");
        }
        //
        // returns true if after removing this field, it is end of line
        // Returns a cell from a csv source and advances the ptr to the start of the next field
        // on entry, ptr points to the first character of the cell (1-based)
        // if at end of line, the parseFieldReturnEol is true
        // if end of file, return_eof is true
        //
        public static bool parseFieldReturnEol(string Source, int sourcePtr, ref string return_cell, ref int return_ptr, ref bool return_eof) {
            try {
                bool result;
                int crPtr;
                int endPtr;
                int Ptr;
                int workingPtr;
                bool IsQuoted;
                int commaptr;
                int lfPtr;
                int crlfPtr;
                //
                Ptr = sourcePtr;
                IsQuoted = false;
                result = false;
                //
                // find initial character
                //
                workingPtr = Ptr;
                while (Mid(Source, workingPtr, 1) == " ")
                    workingPtr += 1;
                if (Mid(Source, workingPtr, 1) == "\"") {
                    Ptr = workingPtr;
                    IsQuoted = true;
                }
                if (!IsQuoted) {
                    //
                    // non-Quoted field
                    //
                    commaptr = InStr(Ptr, Source, ",");
                    lfPtr = InStr(Ptr, Source, "\n");
                    crPtr = InStr(Ptr, Source, "\r");
                    crlfPtr = InStr(Ptr, Source, "\r\n");
                    //
                    // set workingPtr to the first one found
                    //
                    workingPtr = firstNonZero(commaptr, crlfPtr);
                    workingPtr = firstNonZero(workingPtr, lfPtr);
                    workingPtr = firstNonZero(workingPtr, crPtr);
                    workingPtr = firstNonZero(workingPtr, Source.Length);
                    if (workingPtr == crlfPtr) {
                        endPtr = workingPtr - 1;
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 2;
                        result = true;
                    } else if (workingPtr == lfPtr) {
                        endPtr = workingPtr - 1;
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = true;
                    } else if (workingPtr == crPtr) {
                        endPtr = workingPtr - 1;
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = true;
                    } else if (workingPtr == commaptr) {
                        endPtr = workingPtr - 1;
                        return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        return_ptr = workingPtr + 1;
                        result = false;
                    } else {
                        endPtr = Source.Length;
                        if (endPtr - sourcePtr + 1 > 0) {
                            return_cell = Mid(Source, sourcePtr, endPtr - sourcePtr + 1);
                        } else {
                            return_cell = "";
                        }
                        return_ptr = endPtr;
                        result = true;
                    }
                } else {
                    //
                    // Quoted field, pass the initial quote
                    //
                    Ptr += 1;
                    int startPtr;
                    startPtr = Ptr;
                    //
                    while (Ptr != 0 & InStr(Ptr, Source, "\"") == InStr(Ptr, Source, "\"\"")) {
                        Ptr = InStr(Ptr, Source, "\"\"");
                        if (Ptr == 0) {
                            endPtr = Source.Length;
                            return_cell = "";
                            return_ptr = endPtr;
                            result = true;
                        } else {
                            Ptr += 2;
                        }
                    }
                    if (Ptr != 0) {
                        Ptr = InStr(Ptr, Source, "\"");
                        endPtr = Ptr - 1;
                        // skip white space to next delimiter
                        while (Mid(Source, Ptr + 1, 1) == " " & Ptr < Source.Length)
                            Ptr += 1;
                        if (Ptr >= Source.Length) {
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 3;
                            result = true;
                        } else if ((Mid(Source, Ptr + 1, 2) ?? "") == "\r\n") {
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 3;
                            result = true;
                        } else if ((Mid(Source, Ptr + 1, 1) ?? "") == "\n") {
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 2;
                            result = true;
                        } else if ((Mid(Source, Ptr + 1, 1) ?? "") == "\r") {
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = Ptr + 2;
                            result = true;
                        } else {
                            return_cell = Mid(Source, startPtr, endPtr - startPtr + 1);
                            return_ptr = InStr(Ptr, Source, ",");
                            if (return_ptr <= 0) {
                                result = true;
                            } else {
                                return_ptr += 1;
                                result = false;
                            }
                        }
                        //
                        // convert double quotes back to single quotes
                        //
                        return_cell = return_cell.Replace("\"\"", "\"");
                    }
                }
                //
                // determine eof
                //
                if (return_ptr >= Source.Length) {
                    return_eof = true;
                }
                return result;
            } catch (Exception) {
                throw;
            }
        }
        //
        public static void parseLine(string Source, int source_ptr, ref string[] return_cells, ref int return_ptr, ref bool return_eof) {
            try {
                string Cell = "";
                var EOL = default(bool);
                int fieldPtr;
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
                        break;
                    }
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        public static string[,] parseFile(string Source) {
            try {
                string[,] result;
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
                colCnt = dummyCells.GetUpperBound(0) + 1;
                rowCnt = 0;
                //
                colPtr = 0;
                rowPtr = 0;
                srcPtr = 1;
                result = new string[colCnt, 1];

                while (!eof) {
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
                            // error - can not adjust columns after first row
                        } else {
                            colCnt = colPtr + 1;
                            var oldResult1 = result;
                            result = new string[colCnt, rowCnt];
                            if (oldResult1 is not null)
                                for (var i1 = 0; i1 <= oldResult1.Length / oldResult1.GetLength(1) - 1; ++i1)
                                    Array.Copy(oldResult1, i1 * oldResult1.GetLength(1), result, i1 * result.GetLength(1), Math.Min(oldResult1.GetLength(1), result.GetLength(1)));
                        }
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
                return result;
            } catch (Exception) {
                throw;
            }
        }
        //
        public static int firstNonZero(int a, int b) {
            int v = kmaGetFirstNonZeroLong(a, b);
            if (v == 1) {
                return a;
            } else if (v == 2) {
                return b;
            } else {
                return 0;
            }
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
        //
        // Helper methods replacing VB Strings.Mid (1-based) and Strings.InStr (1-based)
        //
        /// <summary>
        /// VB-compatible Mid function (1-based index)
        /// </summary>
        private static string Mid(string source, int start, int length) {
            if (string.IsNullOrEmpty(source)) return "";
            int zeroStart = start - 1;
            if (zeroStart < 0) zeroStart = 0;
            if (zeroStart >= source.Length) return "";
            if (zeroStart + length > source.Length) length = source.Length - zeroStart;
            return source.Substring(zeroStart, length);
        }
        //
        /// <summary>
        /// VB-compatible InStr function (1-based index). Returns 0 if not found.
        /// </summary>
        private static int InStr(int start, string source, string find) {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(find)) return 0;
            int zeroStart = start - 1;
            if (zeroStart < 0) zeroStart = 0;
            if (zeroStart >= source.Length) return 0;
            int idx = source.IndexOf(find, zeroStart, StringComparison.Ordinal);
            return idx < 0 ? 0 : idx + 1;
        }
    }
}
