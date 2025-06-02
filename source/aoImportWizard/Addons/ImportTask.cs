using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

using Contensive.ImportWizard.Controllers;
using Contensive.ImportWizard.Models;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.ImportWizard.Addons {
    // 
    // ========================================================================
    // 
    // Sample file to create a Contensive Aggregate Object
    // 
    // An Aggregate Object is an ActiveX DLL (in process dll) called by Contensive
    // in response to finding a 'token' in the content of a page. When the token is
    // found, it is looked up in the Aggregate Function Objects table and the Program ID
    // for this code is retrieved, and used to create an object from this class.
    // 
    // Contensive v3.4+
    // 
    // Contensive calls the execute method with four arguments:
    // 
    // CsvObject - an object pointer to the ContentServerClass object loaded for this application
    // 
    // MainObject - an object pointer to the MainClass (web client interface) for this hit
    // 
    // OptionString - an encoded string that carries the arguments for this add-on
    // Always use the Csv.GetAddonOption() method to retrieve a value from the Optionstring
    // Arguments are a name=value argument list
    // The arguments are created first, from the argument list in the Add-on record, and
    // second from changes made by the site administrators during edit.
    // 
    // FilterInput - when the addon is acting as a filter, this is the input. For instance, If this
    // Addon is a filter for a page, the Filter input will be the page content.
    // 
    // The output depends on the context of the Add-on:
    // 
    // Page Add-ons create content the replaces their token on the page. The return string
    // is the content to be placed on the page.
    // 
    // Filter Add-ons take the FilterInput argument, modify as needed, and return it
    // as the return string.
    // 
    // Process Add-ons perform their function and return error messages through their return
    // 
    // ========================================================================
    /// <summary>
    /// Process the import
    /// </summary>
    public class ImportTask : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// Process the import
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            try {
                // 
                // Check this server for anything in the tasks queue
                using (var app = new ApplicationModel(CP)) {
                    var taskList = DbBaseModel.createList<ImportWizardTaskModel>(CP, "DateCompleted is null");
                    foreach (ImportWizardTaskModel task in taskList) {
                        task.dateCompleted = DateTime.Now;
                        bool previousProcessAborted = task.dateStarted != DateTime.MinValue;
                        if (previousProcessAborted) {
                            task.resultMessage = "This task failed to complete.";
                        } else {
                            // 
                            // -- Import a CSV file
                            string cvsFilename = Strings.Replace(task.uploadFilename, "/", @"\");
                            string importMapFilename = Strings.Replace(task.importMapFilename, "/", @"\");
                            string resultMessage = processCSV(app, cvsFilename, importMapFilename);
                            string notifyBody;
                            if (!string.IsNullOrEmpty(resultMessage)) {
                                notifyBody = "This email is to notify you that your data import is complete for [" + CP.Site.Name + "]" + Constants.vbCrLf + "The following errors occurred during import" + Constants.vbCrLf + resultMessage;
                            } else {
                                notifyBody = "This email is to notify you that your data import is complete for [" + CP.Site.Name + "]";
                            }
                            string notifySubject = "Import Completed";
                            if (string.IsNullOrEmpty(resultMessage)) {
                                resultMessage = "OK";
                            }
                            task.resultMessage = resultMessage.Length > 255 ? resultMessage.Substring(0, 254) : resultMessage;
                            if (!string.IsNullOrEmpty(task.notifyEmail) & !string.IsNullOrEmpty(notifyBody)) {
                                string notifyFromAddress = CP.Site.GetText("EmailFromAddress", "");
                                CP.Email.send(task.notifyEmail, notifyFromAddress, "Task Completion Notification", notifyBody);
                            }
                        }
                        task.save(CP);
                    }
                    return null;
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // =====================================================================================
        /// <summary>
        /// process the input csv file
        /// </summary>
        /// <param name="app"></param>
        /// <param name="CSVFilename"></param>
        /// <param name="ImportMapPathFilename"></param>
        /// <returns></returns>
        private string processCSV(ApplicationModel app, string CSVFilename, string ImportMapPathFilename) {
            try {
                var cp = app.cp;
                string result = "";
                if (Strings.Left(CSVFilename, 1) == @"\") {
                    CSVFilename = Strings.Mid(CSVFilename, 2);
                }
                string hint = "010";
                string importData = cp.PrivateFiles.Read(CSVFilename);
                if (string.IsNullOrEmpty(importData)) {
                    // 
                    // -- if source is empty, return empty
                    return string.Empty;
                }

                hint = "020";
                // Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                var importMap = ImportMapModel.create(cp, ImportMapPathFilename);
                hint = "040";
                string[,] importDataCells = GenericController.parseFile(importData);
                int importDataColumnCnt = Information.UBound(importDataCells, 1) + 1;
                string ImportTableName = "";
                string dBFieldName;
                // 
                // -- setup destination content/database table, and setup importMap
                if (!importMap.importToNewContent) {
                    // 
                    // -- set default table to people
                    if (string.IsNullOrEmpty(importMap.contentName)) {
                        importMap.contentName = "People";
                    }
                    ImportTableName = cp.Content.GetTable(importMap.contentName);
                } else {
                    // 
                    // create the destination table and import map
                    importMap.skipRowCnt = 1;
                    importMap.mapPairCnt = importDataColumnCnt;
                    importMap.mapPairs = new ImportMapModel_MapPair[importDataColumnCnt];
                    importMap.mapPairs[importDataColumnCnt - 1] = new ImportMapModel_MapPair();
                    ImportTableName = importMap.contentName;
                    ImportTableName = Strings.Replace(ImportTableName, " ", "_");
                    ImportTableName = Strings.Replace(ImportTableName, "-", "_");
                    ImportTableName = Strings.Replace(ImportTableName, ",", "_");
                    hint = "060";
                    cp.Content.AddContent(importMap.contentName, ImportTableName);
                    hint = "070";
                    int colPtr;
                    var loopTo = importDataColumnCnt - 1;
                    for (colPtr = 0; colPtr <= loopTo; colPtr++) {
                        importMap.mapPairs[colPtr] = new ImportMapModel_MapPair();
                        hint = "080";
                        dBFieldName = importDataCells[colPtr, 0];
                        dBFieldName = encodeFieldName(cp, dBFieldName);
                        if (string.IsNullOrEmpty(dBFieldName)) {
                            dBFieldName = "field" + colPtr;
                        }
                        importMap.mapPairs[colPtr].dbFieldName = dBFieldName;
                        importMap.mapPairs[colPtr].dbFieldType = 2;
                        importMap.mapPairs[colPtr].uploadFieldName = dBFieldName;
                        importMap.mapPairs[colPtr].uploadFieldPtr = colPtr;
                        hint = "090";
                        cp.Content.AddContentField(importMap.contentName, dBFieldName, 2);
                    }
                }

                if (importMap.mapPairCnt > 0) {
                    hint = "200";
                    int SourceKeyPtr = cp.Utils.EncodeInteger(importMap.sourceKeyField);
                    if (string.IsNullOrEmpty(importMap.dbKeyField) | SourceKeyPtr < 0) {
                        importMap.keyMethodID = (int)MapKeyEnum.KeyMethodInsertAll;
                    }
                    // 
                    string KeyCriteria = "(1=0)";
                    int rowPtr;
                    int rowCnt = Information.UBound(importDataCells, 2) + 1;
                    var matchFound = default(bool);
                    var loopTo1 = rowCnt - 1;
                    int LoopCnt = 0;
                    for (rowPtr = importMap.skipRowCnt; rowPtr <= loopTo1; rowPtr++) {
                        hint = "300";
                        bool updateRecord = false;
                        bool insertRecord = false;
                        int rowWidth = 0;
                        if (rowPtr == 76 | rowPtr == 105) {
                            rowPtr = rowPtr;
                        }
                        if (true) {
                            hint = "310";
                            if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodInsertAll) {
                                hint = "320";
                                insertRecord = true;
                            } else {
                                hint = "330";
                                // 
                                // Update or Update-And-Insert, Build Key Criteria
                                string sourceKeyData = importDataCells[SourceKeyPtr, rowPtr];
                                if (Strings.Len(sourceKeyData) > 2 & Strings.Left(sourceKeyData, 1) == "\"" & Strings.Right(sourceKeyData, 1) == "\"") {
                                    sourceKeyData = Strings.Trim(Strings.Mid(sourceKeyData, 2, Strings.Len(sourceKeyData) - 2));
                                }
                                if (string.IsNullOrEmpty(Strings.Trim(sourceKeyData))) {
                                    // 
                                    // Source had no data in key field, insert if allowed
                                    // 
                                    if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                        insertRecord = true;
                                    }
                                } else {
                                    hint = "340";
                                    // 
                                    // Source has good key field data
                                    // 
                                    switch (importMap.dbKeyFieldType) {
                                        case constants.FieldTypeAutoIncrement:
                                        case constants.FieldTypeCurrency:
                                        case constants.FieldTypeFloat:
                                        case constants.FieldTypeInteger:
                                        case constants.FieldTypeLookup:
                                        case constants.FieldTypeManyToMany:
                                        case constants.FieldTypeMemberSelect: {
                                                // 
                                                // number
                                                // 
                                                updateRecord = true;
                                                KeyCriteria = "(" + importMap.dbKeyField + "=" + cp.Db.EncodeSQLNumber(cp.Utils.EncodeNumber(sourceKeyData)) + ")";
                                                break;
                                            }
                                        case constants.FieldTypeBoolean: {
                                                // 
                                                // Boolean
                                                // 
                                                updateRecord = true;
                                                KeyCriteria = "(" + importMap.dbKeyField + "=" + cp.Db.EncodeSQLBoolean(cp.Utils.EncodeBoolean(sourceKeyData)) + ")";
                                                break;
                                            }
                                        case constants.FieldTypeDate: {
                                                // 
                                                // date
                                                // 
                                                updateRecord = true;
                                                KeyCriteria = "(" + importMap.dbKeyField + "=" + cp.Db.EncodeSQLDate(cp.Utils.EncodeDate(sourceKeyData)) + ")";
                                                break;
                                            }
                                        case constants.FieldTypeText:
                                        case constants.FieldTypeResourceLink:
                                        case constants.FieldTypeLink: {
                                                // 
                                                // text
                                                // 
                                                updateRecord = true;
                                                KeyCriteria = "(" + importMap.dbKeyField + "=" + cp.Db.EncodeSQLText(Strings.Left(sourceKeyData, 255)) + ")";
                                                break;
                                            }
                                        case constants.FieldTypeLongText:
                                        case constants.FieldTypeHTML: {
                                                // 
                                                // long text
                                                // 
                                                updateRecord = true;
                                                KeyCriteria = "(" + importMap.dbKeyField + "=" + cp.Db.EncodeSQLText(sourceKeyData) + ")";
                                                break;
                                            }

                                        default: {
                                                // 
                                                // unknown field type
                                                // 
                                                updateRecord = true;
                                                if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                                    insertRecord = true;
                                                }

                                                break;
                                            }
                                    }
                                    // move to after the row work so we can skip the insert if the row width=0
                                }
                            }
                            // 
                            // -- store textfile fields to be updated manually after the upate
                            var textFileManualUpdate = new List<textFileModel>();
                            // 
                            // If Insert, Build KeyCriteria and setup CS
                            string updateSQLFieldSet = "";
                            if (insertRecord | updateRecord) {
                                hint = "400";
                                int fieldPtr;
                                // 
                                // Build Update SQL
                                var loopTo2 = importMap.mapPairCnt - 1;
                                for (fieldPtr = 0; fieldPtr <= loopTo2; fieldPtr++) {
                                    hint = "500";
                                    int uploadFieldPtr = importMap.mapPairs[fieldPtr].uploadFieldPtr;
                                    if (uploadFieldPtr == -1 || uploadFieldPtr < -2 || uploadFieldPtr >= importDataColumnCnt) {
                                        // 
                                        // --  ignore -1 = ignore
                                        // --  ignore -3 = firstname + lastname
                                        // --  ignore -4 = firstname from name field
                                        // --  ignore -5 = lastname from name field
                                    } else {
                                        // 
                                        // -- handle>=0 as pointer to upload field
                                        // -- handle -2 use setValue
                                        // 
                                        hint = "600";
                                        dBFieldName = importMap.mapPairs[fieldPtr].dbFieldName;
                                        string importDataCellValue = "";
                                        if (uploadFieldPtr == -2) {
                                            importDataCellValue = importMap.mapPairs[fieldPtr].setValue;
                                        } else {
                                            importDataCellValue = importDataCells[uploadFieldPtr, rowPtr];
                                        }
                                        rowWidth += Strings.Len(Strings.Trim(importDataCellValue));
                                        // there are no fieldtypes defined as 0, and I do not want the CS open now, so we can avoid the insert if rowwidth=0

                                        switch (importMap.mapPairs[fieldPtr].dbFieldType) {
                                            case constants.FieldTypeAutoIncrement:
                                            case constants.FieldTypeCurrency:
                                            case constants.FieldTypeFloat:
                                            case constants.FieldTypeInteger:
                                            case constants.FieldTypeLookup:
                                            case constants.FieldTypeManyToMany:
                                            case constants.FieldTypeMemberSelect: {
                                                    // 
                                                    // number, nullable
                                                    if (string.IsNullOrEmpty(importDataCellValue)) {
                                                        updateSQLFieldSet += "," + dBFieldName + "=null";
                                                    } else {
                                                        double sourceConverted = cp.Utils.EncodeNumber(importDataCellValue);
                                                        updateSQLFieldSet += "," + dBFieldName + "=" + cp.Db.EncodeSQLNumber(sourceConverted);
                                                    }

                                                    break;
                                                }
                                            case constants.FieldTypeBoolean: {
                                                    // 
                                                    // Boolean, null is false
                                                    bool sourceConverted = cp.Utils.EncodeBoolean(importDataCellValue);
                                                    updateSQLFieldSet += "," + dBFieldName + "=" + cp.Db.EncodeSQLBoolean(sourceConverted);
                                                    break;
                                                }
                                            case constants.FieldTypeDate: {
                                                    // 
                                                    // date, nullable
                                                    if (string.IsNullOrEmpty(importDataCellValue)) {
                                                        updateSQLFieldSet += "," + dBFieldName + "=null";
                                                    } else {
                                                        var sourceConverted = cp.Utils.EncodeDate(importDataCellValue);
                                                        updateSQLFieldSet += "," + dBFieldName + "=" + cp.Db.EncodeSQLDate(sourceConverted);
                                                    }

                                                    break;
                                                }
                                            case constants.FieldTypeText:
                                            case constants.FieldTypeLink:
                                            case constants.FieldTypeResourceLink: {
                                                    // 
                                                    // text, null is empty
                                                    string sourceConverted = string.IsNullOrEmpty(importDataCellValue) ? "" : importDataCellValue.Length < 256 ? importDataCellValue : Strings.Left(importDataCellValue, 255);
                                                    updateSQLFieldSet += "," + dBFieldName + "=" + cp.Db.EncodeSQLText(sourceConverted);
                                                    break;
                                                }
                                            case constants.FieldTypeLongText:
                                            case constants.FieldTypeHTML: {
                                                    // 
                                                    // long text, null is empty
                                                    updateSQLFieldSet += "," + dBFieldName + "=" + cp.Db.EncodeSQLText(importDataCellValue);
                                                    break;
                                                }
                                            // Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                            // '
                                            // ' filenames, can not import these, but at least update the filename
                                            // Dim sourceConverted As String = If(String.IsNullOrEmpty(sourceData), "", If(sourceData.Length < 256, sourceData, Left(sourceData, 255)))
                                            // updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLText(sourceConverted)
                                            case constants.FieldTypeTextFile:
                                            case constants.FieldTypeCSSFile:
                                            case constants.FieldTypeHTMLFile:
                                            case constants.FieldTypeJavascriptFile:
                                            case constants.FieldTypeXMLFile: {
                                                    // 
                                                    // -- text files, like notes
                                                    textFileManualUpdate.Add(new textFileModel() {
                                                        fieldName = dBFieldName,
                                                        fieldValue = importDataCellValue
                                                    });
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                            hint = "700";
                            if (Information.Err().Number != 0) {
                                result += Constants.vbCrLf + "Row " + (rowPtr + 1) + " could not be imported. [" + Information.Err().Description + "]";
                            } else if (rowWidth == 0) {
                                result += Constants.vbCrLf + "Row " + (rowPtr + 1) + " was not imported because it was empty.";
                            } else if (!string.IsNullOrEmpty(updateSQLFieldSet)) {
                                // 
                                // insert or update the table
                                // 
                                var cs = cp.CSNew();
                                int recordId = 0;
                                if (updateRecord) {
                                    // 
                                    // Update requested
                                    matchFound = false;
                                    insertRecord = false;
                                    matchFound = cs.Open(importMap.contentName, KeyCriteria, "ID", false);
                                    cs.Close();
                                    if (matchFound) {
                                        // 
                                        // match was found, update all records found
                                        updateRecord = true;
                                    } else if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                        // 
                                        // no match, convert to insert if that was requested
                                        insertRecord = true;
                                    }
                                }
                                if (insertRecord) {
                                    // 
                                    // Insert a new record and convert to an update
                                    updateRecord = false;
                                    if (cs.Insert(importMap.contentName)) {
                                        if (Information.Err().Number != 0) {
                                            Information.Err().Clear();
                                            result += Constants.vbCrLf + "Row " + (rowPtr + 1) + " could not be imported. [" + Information.Err().Description + "]";
                                        } else if (!cs.OK()) {
                                            result += Constants.vbCrLf + "Row " + (rowPtr + 1) + " could not be imported because a record count not be inserted.";
                                        } else {
                                            recordId = cs.GetInteger("ID");
                                            KeyCriteria = "(ID=" + cp.Utils.EncodeNumber(recordId) + ")";
                                            updateRecord = true;
                                        }
                                    }
                                    cs.Close();
                                }
                                // 
                                // Update the record if needed
                                if (updateRecord) {
                                    hint = "900";
                                    // 
                                    // Update all records in the current recordset
                                    string UpdateSQL = "update " + ImportTableName + " set " + Strings.Mid(updateSQLFieldSet, 2) + " where " + KeyCriteria;
                                    cs.OpenSQL(UpdateSQL, cp.Content.GetDataSource(importMap.contentName));
                                    if (Information.Err().Number != 0) {
                                        Information.Err().Clear();
                                        result += Constants.vbCrLf + "Row " + (rowPtr + 1) + " could not be imported. [" + Information.Err().Description + "]";
                                    }
                                }
                                // 
                                // -- if there are manual update fields (textfile) then update them now
                                if (textFileManualUpdate.Count > 0) {
                                    using (var manualUpdateCs = cp.CSNew()) {
                                        if (manualUpdateCs.Open(importMap.contentName, KeyCriteria)) {
                                            foreach (textFileModel textfile in textFileManualUpdate)
                                                manualUpdateCs.SetField(textfile.fieldName, textfile.fieldValue);
                                        }
                                    }
                                }
                                // 
                                // -- now handle the post-save fields like firstname+lastname
                                foreach (var mapPair in importMap.mapPairs) {
                                    if (mapPair.uploadFieldPtr == -3) {
                                        // 
                                        // -- set field = firstname + lastname
                                        cp.Db.ExecuteNonQuery("update " + ImportTableName + " set " + mapPair.dbFieldName + "=[firstname] + ' ' + [lastname] where " + KeyCriteria);
                                    } else if (mapPair.uploadFieldPtr == -4) {
                                        // 
                                        // -- set field = first word from name field
                                        cp.Db.ExecuteNonQuery("update " + ImportTableName + " set " + mapPair.dbFieldName + "=SUBSTRING(name, 1, CHARINDEX(' ', name) - 1) where " + KeyCriteria);
                                    } else if (mapPair.uploadFieldPtr == -5) {
                                        // 
                                        // -- set field = last word from name field
                                        cp.Db.ExecuteNonQuery("update " + ImportTableName + " set " + mapPair.dbFieldName + "=SUBSTRING(name, CHARINDEX(' ', name) + 1, LEN(name) - CHARINDEX(' ', name)) where " + KeyCriteria);
                                    }
                                }
                                // 
                                // 
                                if (importMap.groupOptionID != constants.GroupOptionNone) {
                                    // 
                                    // update/insert OK and records are People
                                    var @group = DbBaseModel.create<GroupModel>(cp, importMap.groupID);
                                    if (group is not null) {
                                        foreach (PersonModel user in DbBaseModel.createList<PersonModel>(cp, KeyCriteria)) {
                                            if (user is not null) {
                                                // 
                                                // Add Groups
                                                switch (importMap.groupOptionID) {
                                                    case constants.GroupOptionAll: {
                                                            // 
                                                            cp.Group.AddUser(group.id, user.id);
                                                            break;
                                                        }
                                                    case constants.GroupOptionOnMatch: {
                                                            // 
                                                            if (matchFound) {
                                                                cp.Group.AddUser(group.id, user.id);
                                                            }

                                                            break;
                                                        }
                                                    case constants.GroupOptionOnNoMatch: {
                                                            // 
                                                            if (!matchFound) {
                                                                cp.Group.AddUser(group.id, user.id);
                                                            }

                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        LoopCnt = LoopCnt + 1;
                        if (LoopCnt > 10) {
                            LoopCnt = 0;
                        }
                    }
                }
                return result;
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        public static string encodeFieldName(CPBaseClass cp, string Source) {

            try {
                string result = "";
                string allowed;
                string chr;
                int Ptr;
                int cnt;
                string returnString;
                // 
                returnString = "";
                cnt = Strings.Len(Source);
                if (cnt > 254) {
                    cnt = 254;
                }
                allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
                var loopTo = cnt;
                for (Ptr = 1; Ptr <= loopTo; Ptr++) {
                    chr = Strings.Mid(Source, Ptr, 1);
                    if (Conversions.ToBoolean(Strings.InStr(1, allowed, chr, Constants.vbBinaryCompare))) {
                        returnString += chr;
                    } else {
                        returnString += "_";
                    }
                }
                result = returnString;
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
    // 
    public class textFileModel {
        public string fieldName { get; set; }
        public string fieldValue { get; set; }
    }
}