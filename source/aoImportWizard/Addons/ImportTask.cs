using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

using Contensive.ImportWizard.Controllers;
using Contensive.ImportWizard.Models;
using Contensive.Models.Db;

namespace Contensive.ImportWizard.Addons {
    /// <summary>
    /// Process the import
    /// </summary>
    public class ImportTask : AddonBaseClass {
        /// <summary>
        /// Process the import
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            try {
                using (var app = new ApplicationModel(CP)) {
                    var taskList = DbBaseModel.createList<ImportWizardTaskModel>(CP, "DateCompleted is null");
                    foreach (ImportWizardTaskModel task in taskList) {
                        task.dateCompleted = DateTime.Now;
                        bool previousProcessAborted = task.dateStarted != DateTime.MinValue;
                        if (previousProcessAborted) {
                            task.resultMessage = "This task failed to complete.";
                        } else {
                            string cvsFilename = task.uploadFilename.Replace("/", @"\");
                            string importMapFilename = task.importMapFilename.Replace("/", @"\");
                            string resultMessage = processCSV(app, cvsFilename, importMapFilename);
                            string notifyBody;
                            if (!string.IsNullOrEmpty(resultMessage)) {
                                notifyBody = $"This email is to notify you that your data import is complete for [{CP.Site.Name}]\r\nThe following errors occurred during import\r\n{resultMessage}";
                            } else {
                                notifyBody = $"This email is to notify you that your data import is complete for [{CP.Site.Name}]";
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
                if (CSVFilename.Length > 0 && CSVFilename[0] == '\\') {
                    CSVFilename = CSVFilename.Substring(1);
                }
                string hint = "010";
                string importData = cp.PrivateFiles.Read(CSVFilename);
                if (string.IsNullOrEmpty(importData)) {
                    return string.Empty;
                }

                hint = "020";
                var importMap = ImportMapModel.create(cp, ImportMapPathFilename);
                hint = "040";
                string[,] importDataCells = GenericController.parseFile(importData);
                int importDataColumnCnt = importDataCells.GetUpperBound(0) + 1;
                string ImportTableName = "";
                string dBFieldName;
                if (!importMap.importToNewContent) {
                    if (string.IsNullOrEmpty(importMap.contentName)) {
                        importMap.contentName = "People";
                    }
                    ImportTableName = cp.Content.GetTable(importMap.contentName);
                } else {
                    importMap.skipRowCnt = 1;
                    importMap.mapPairCnt = importDataColumnCnt;
                    importMap.mapPairs = new ImportMapModel_MapPair[importDataColumnCnt];
                    importMap.mapPairs[importDataColumnCnt - 1] = new ImportMapModel_MapPair();
                    ImportTableName = importMap.contentName;
                    ImportTableName = ImportTableName.Replace(" ", "_");
                    ImportTableName = ImportTableName.Replace("-", "_");
                    ImportTableName = ImportTableName.Replace(",", "_");
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
                            dBFieldName = $"field{colPtr}";
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
                    string KeyCriteria = "(1=0)";
                    int rowPtr;
                    int rowCnt = importDataCells.GetUpperBound(1) + 1;
                    var matchFound = default(bool);
                    var loopTo1 = rowCnt - 1;
                    int LoopCnt = 0;
                    for (rowPtr = importMap.skipRowCnt; rowPtr <= loopTo1; rowPtr++) {
                        hint = "300";
                        bool updateRecord = false;
                        bool insertRecord = false;
                        int rowWidth = 0;
                        if (true) {
                            hint = "310";
                            if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodInsertAll) {
                                hint = "320";
                                insertRecord = true;
                            } else {
                                hint = "330";
                                string sourceKeyData = importDataCells[SourceKeyPtr, rowPtr];
                                if (sourceKeyData.Length > 2 & sourceKeyData.StartsWith("\"") & sourceKeyData.EndsWith("\"")) {
                                    sourceKeyData = sourceKeyData.Substring(1, sourceKeyData.Length - 2).Trim();
                                }
                                if (string.IsNullOrEmpty(sourceKeyData?.Trim())) {
                                    if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                        insertRecord = true;
                                    }
                                } else {
                                    hint = "340";
                                    switch (importMap.dbKeyFieldType) {
                                        case constants.FieldTypeAutoIncrement:
                                        case constants.FieldTypeCurrency:
                                        case constants.FieldTypeFloat:
                                        case constants.FieldTypeInteger:
                                        case constants.FieldTypeLookup:
                                        case constants.FieldTypeManyToMany:
                                        case constants.FieldTypeMemberSelect: {
                                                updateRecord = true;
                                                KeyCriteria = $"({importMap.dbKeyField}={cp.Db.EncodeSQLNumber(cp.Utils.EncodeNumber(sourceKeyData))})";
                                                break;
                                            }
                                        case constants.FieldTypeBoolean: {
                                                updateRecord = true;
                                                KeyCriteria = $"({importMap.dbKeyField}={cp.Db.EncodeSQLBoolean(cp.Utils.EncodeBoolean(sourceKeyData))})";
                                                break;
                                            }
                                        case constants.FieldTypeDate: {
                                                updateRecord = true;
                                                KeyCriteria = $"({importMap.dbKeyField}={cp.Db.EncodeSQLDate(cp.Utils.EncodeDate(sourceKeyData))})";
                                                break;
                                            }
                                        case constants.FieldTypeText:
                                        case constants.FieldTypeResourceLink:
                                        case constants.FieldTypeLink: {
                                                updateRecord = true;
                                                KeyCriteria = $"({importMap.dbKeyField}={cp.Db.EncodeSQLText(sourceKeyData.Length > 255 ? sourceKeyData.Substring(0, 255) : sourceKeyData)})";
                                                break;
                                            }
                                        case constants.FieldTypeLongText:
                                        case constants.FieldTypeHTML: {
                                                updateRecord = true;
                                                KeyCriteria = $"({importMap.dbKeyField}={cp.Db.EncodeSQLText(sourceKeyData)})";
                                                break;
                                            }

                                        default: {
                                                updateRecord = true;
                                                if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                                    insertRecord = true;
                                                }
                                                break;
                                            }
                                    }
                                }
                            }
                            var textFileManualUpdate = new List<textFileModel>();
                            string updateSQLFieldSet = "";
                            if (insertRecord | updateRecord) {
                                hint = "400";
                                int fieldPtr;
                                var loopTo2 = importMap.mapPairCnt - 1;
                                for (fieldPtr = 0; fieldPtr <= loopTo2; fieldPtr++) {
                                    hint = "500";
                                    int uploadFieldPtr = importMap.mapPairs[fieldPtr].uploadFieldPtr;
                                    if (uploadFieldPtr == -1 || uploadFieldPtr < -2 || uploadFieldPtr >= importDataColumnCnt) {
                                        // ignore -1 = ignore, -3 = firstname + lastname, -4 = firstname from name, -5 = lastname from name
                                    } else {
                                        hint = "600";
                                        dBFieldName = importMap.mapPairs[fieldPtr].dbFieldName;
                                        string importDataCellValue = "";
                                        if (uploadFieldPtr == -2) {
                                            importDataCellValue = importMap.mapPairs[fieldPtr].setValue;
                                        } else {
                                            importDataCellValue = importDataCells[uploadFieldPtr, rowPtr];
                                        }
                                        rowWidth += (importDataCellValue?.Trim() ?? "").Length;

                                        switch (importMap.mapPairs[fieldPtr].dbFieldType) {
                                            case constants.FieldTypeAutoIncrement:
                                            case constants.FieldTypeCurrency:
                                            case constants.FieldTypeFloat:
                                            case constants.FieldTypeInteger:
                                            case constants.FieldTypeLookup:
                                            case constants.FieldTypeManyToMany:
                                            case constants.FieldTypeMemberSelect: {
                                                    if (string.IsNullOrEmpty(importDataCellValue)) {
                                                        updateSQLFieldSet += $",{dBFieldName}=null";
                                                    } else {
                                                        double sourceConverted = cp.Utils.EncodeNumber(importDataCellValue);
                                                        updateSQLFieldSet += $",{dBFieldName}={cp.Db.EncodeSQLNumber(sourceConverted)}";
                                                    }
                                                    break;
                                                }
                                            case constants.FieldTypeBoolean: {
                                                    bool sourceConverted = cp.Utils.EncodeBoolean(importDataCellValue);
                                                    updateSQLFieldSet += $",{dBFieldName}={cp.Db.EncodeSQLBoolean(sourceConverted)}";
                                                    break;
                                                }
                                            case constants.FieldTypeDate: {
                                                    if (string.IsNullOrEmpty(importDataCellValue)) {
                                                        updateSQLFieldSet += $",{dBFieldName}=null";
                                                    } else {
                                                        var sourceConverted = cp.Utils.EncodeDate(importDataCellValue);
                                                        updateSQLFieldSet += $",{dBFieldName}={cp.Db.EncodeSQLDate(sourceConverted)}";
                                                    }
                                                    break;
                                                }
                                            case constants.FieldTypeText:
                                            case constants.FieldTypeLink:
                                            case constants.FieldTypeResourceLink: {
                                                    string sourceConverted = string.IsNullOrEmpty(importDataCellValue) ? "" : importDataCellValue.Length < 256 ? importDataCellValue : importDataCellValue.Substring(0, 255);
                                                    updateSQLFieldSet += $",{dBFieldName}={cp.Db.EncodeSQLText(sourceConverted)}";
                                                    break;
                                                }
                                            case constants.FieldTypeLongText:
                                            case constants.FieldTypeHTML: {
                                                    updateSQLFieldSet += $",{dBFieldName}={cp.Db.EncodeSQLText(importDataCellValue)}";
                                                    break;
                                                }
                                            case constants.FieldTypeTextFile:
                                            case constants.FieldTypeCSSFile:
                                            case constants.FieldTypeHTMLFile:
                                            case constants.FieldTypeJavascriptFile:
                                            case constants.FieldTypeXMLFile: {
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
                            if (rowWidth == 0) {
                                result += $"\r\nRow {rowPtr + 1} was not imported because it was empty.";
                            } else if (!string.IsNullOrEmpty(updateSQLFieldSet)) {
                                var cs = cp.CSNew();
                                int recordId = 0;
                                if (updateRecord) {
                                    matchFound = false;
                                    insertRecord = false;
                                    matchFound = cs.Open(importMap.contentName, KeyCriteria, "ID", false);
                                    cs.Close();
                                    if (matchFound) {
                                        updateRecord = true;
                                    } else if (importMap.keyMethodID == (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers) {
                                        insertRecord = true;
                                    }
                                }
                                if (insertRecord) {
                                    updateRecord = false;
                                    if (cs.Insert(importMap.contentName)) {
                                        if (!cs.OK()) {
                                            result += $"\r\nRow {rowPtr + 1} could not be imported because a record count not be inserted.";
                                        } else {
                                            recordId = cs.GetInteger("ID");
                                            KeyCriteria = $"(ID={cp.Utils.EncodeNumber(recordId)})";
                                            updateRecord = true;
                                        }
                                    }
                                    cs.Close();
                                }
                                if (updateRecord) {
                                    hint = "900";
                                    string UpdateSQL = $"update {ImportTableName} set {updateSQLFieldSet.Substring(1)} where {KeyCriteria}";
                                    cs.OpenSQL(UpdateSQL, cp.Content.GetDataSource(importMap.contentName));
                                }
                                if (textFileManualUpdate.Count > 0) {
                                    using (var manualUpdateCs = cp.CSNew()) {
                                        if (manualUpdateCs.Open(importMap.contentName, KeyCriteria)) {
                                            foreach (textFileModel textfile in textFileManualUpdate) {
                                                manualUpdateCs.SetField(textfile.fieldName, textfile.fieldValue);
                                            }
                                        }
                                    }
                                }
                                foreach (var mapPair in importMap.mapPairs) {
                                    if (mapPair.uploadFieldPtr == -3) {
                                        cp.Db.ExecuteNonQuery($"update {ImportTableName} set {mapPair.dbFieldName}=[firstname] + ' ' + [lastname] where {KeyCriteria}");
                                    } else if (mapPair.uploadFieldPtr == -4) {
                                        cp.Db.ExecuteNonQuery($"update {ImportTableName} set {mapPair.dbFieldName}=SUBSTRING(name, 1, CHARINDEX(' ', name) - 1) where {KeyCriteria}");
                                    } else if (mapPair.uploadFieldPtr == -5) {
                                        cp.Db.ExecuteNonQuery($"update {ImportTableName} set {mapPair.dbFieldName}=SUBSTRING(name, CHARINDEX(' ', name) + 1, LEN(name) - CHARINDEX(' ', name)) where {KeyCriteria}");
                                    }
                                }
                                if (importMap.groupOptionID != constants.GroupOptionNone) {
                                    var @group = DbBaseModel.create<GroupModel>(cp, importMap.groupID);
                                    if (group is not null) {
                                        foreach (PersonModel user in DbBaseModel.createList<PersonModel>(cp, KeyCriteria)) {
                                            if (user is not null) {
                                                switch (importMap.groupOptionID) {
                                                    case constants.GroupOptionAll: {
                                                            cp.Group.AddUser(group.id, user.id);
                                                            break;
                                                        }
                                                    case constants.GroupOptionOnMatch: {
                                                            if (matchFound) {
                                                                cp.Group.AddUser(group.id, user.id);
                                                            }
                                                            break;
                                                        }
                                                    case constants.GroupOptionOnNoMatch: {
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
                string allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
                string returnString = "";
                int cnt = Source.Length;
                if (cnt > 254) {
                    cnt = 254;
                }
                for (int Ptr = 0; Ptr < cnt; Ptr++) {
                    char chr = Source[Ptr];
                    if (allowed.IndexOf(chr) >= 0) {
                        returnString += chr;
                    } else {
                        returnString += "_";
                    }
                }
                return returnString;
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
