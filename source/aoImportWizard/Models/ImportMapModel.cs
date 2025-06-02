using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.ImportWizard.Controllers;
using C5BaseModel = Contensive.Models.Db.DbBaseModel;
using Microsoft.VisualBasic;

namespace Contensive.ImportWizard.Models {
    public class ImportMapModel {

        // Public Shared Widening Operator CType(v As String) As ImportMapType
        // Throw New NotImplementedException()
        // End Operator
        // 
        // Import Map file layout
        // 
        // row 0 - KeyMethodID
        // row 1 - SourceKey Field
        // row 2 - DbKey Field
        // row 3 - blank
        // row4+ SourceField,DbField mapping pairs
        // 
        // - if nuget references are not there, open nuget command line and click the 'reload' message at the top, or run "Update-Package -reinstall" - close/open
        // - Verify project root name space is empty
        // - Change the namespace (AddonCollectionVb) to the collection name
        // - Change this class name to the addon name
        // - Create a Contensive Addon record with the namespace apCollectionName.ad
        public bool importToNewContent { get; set; }
        public string contentName { get; set; }
        /// <summary>
        /// see MapKeyEnum, 1=insert all, 2=update if match, 3=update if match, else insert
        /// </summary>
        /// <returns></returns>
        public int keyMethodID { get; set; }
        public string sourceKeyField { get; set; }
        public string dbKeyField { get; set; }
        public int dbKeyFieldType { get; set; }
        public int groupOptionID { get; set; }
        public int groupID { get; set; }
        public int skipRowCnt { get; set; }
        public int mapPairCnt { get; set; }
        public ImportMapModel_MapPair[] mapPairs { get; set; }
        // 
        public static string getMapPath(ApplicationModel app, string contentName) {
            return constants.privateFilesMapFolder + "user" + app.cp.User.Id + @"\" + contentName.Replace(" ", "-") + @"\";
        }
        // 
        /// <summary>
        /// create filename
        /// YYYY-MM-DD-HH-MM-SS-normalizedFilename
        /// </summary>
        /// <param name="app"></param>
        /// <param name="contentName"></param>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public static string createMapPathFilename(ApplicationModel app, string contentName, string mapName) {
            var dateCreated = DateTime.Now;
            return getMapPath(app, contentName) + dateCreated.Year + "-" + dateCreated.Month.ToString().PadLeft(2, '0') + "-" + dateCreated.Day.ToString().PadLeft(2, '0') + "_" + dateCreated.Hour.ToString().PadLeft(2, '0') + "-" + dateCreated.Minute.ToString().PadLeft(2, '0') + "-" + dateCreated.Second.ToString().PadLeft(2, '0') + "_" + GenericController.normalizeFilename(mapName) + ".txt";
        }
        // 
        public static MapFilenameDataModel decodeMapFileName(CPBaseClass cp, string mapFilename) {
            // 
            if (mapFilename.Length < 23)
                return null;
            int yearPart = cp.Utils.EncodeInteger(mapFilename.Substring(0, 4));
            int monthPart = cp.Utils.EncodeInteger(mapFilename.Substring(5, 2));
            int dayPart = cp.Utils.EncodeInteger(mapFilename.Substring(8, 2));
            int hourPart = cp.Utils.EncodeInteger(mapFilename.Substring(11, 2));
            int minutePart = cp.Utils.EncodeInteger(mapFilename.Substring(14, 2));
            int secondPart = cp.Utils.EncodeInteger(mapFilename.Substring(17, 2));
            if (yearPart == 0 | monthPart == 0 | dayPart == 0)
                return null;
            // 
            string filenamePart = mapFilename.Substring(20);
            if (string.IsNullOrEmpty(filenamePart))
                return null;
            // 
            return new MapFilenameDataModel() {
                filename = mapFilename,
                mapName = System.IO.Path.GetFileNameWithoutExtension(filenamePart),
                dateCreated = new DateTime(yearPart, monthPart, dayPart, hourPart, minutePart, secondPart)
            };
        }

        // 
        /// <summary>
        /// Return a list of filename plus dateadded
        /// </summary>
        /// <param name="app"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static List<MapFilenameDataModel> getMapFileList(ApplicationModel app, string contentName) {
            try {
                var result = new List<MapFilenameDataModel>();
                foreach (var fileDetail in app.cp.PrivateFiles.FileList(getMapPath(app, contentName))) {
                    var mapField = decodeMapFileName(app.cp, fileDetail.Name);
                    if (mapField is null)
                        continue;
                    result.Add(mapField);
                    // Dim mapFilename As String = fileDetail.Name
                    // '
                    // If mapFilename.Length < 23 Then Continue For
                    // Dim yearPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(0, 4))
                    // Dim monthPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(5, 2))
                    // Dim dayPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(8, 2))
                    // Dim hourPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(11, 2))
                    // Dim minutePart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(14, 2))
                    // Dim secondPart As Integer = app.cp.Utils.EncodeInteger(mapFilename.Substring(17, 2))
                    // If yearPart = 0 Or monthPart = 0 Or dayPart = 0 Then Continue For
                    // '
                    // Dim filenamePart As String = mapFilename.Substring(20)
                    // If String.IsNullOrEmpty(filenamePart) Then Continue For
                    // '
                    // result.Add(New MapFilenameDataModel With {
                    // .filename = mapFilename,
                    // .mapName = filenamePart,
                    // .dateCreated = New Date(yearPart, monthPart, dayPart, hourPart, minutePart, secondPart)
                    // })
                }
                return result;
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        /// <summary>
        /// Load Import Map
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="importMapPathFilename"></param>
        /// <returns></returns>
        public static ImportMapModel create(CPBaseClass cp, string importMapPathFilename) {
            try {
                var result = cp.JSON.Deserialize<ImportMapModel>(cp.PrivateFiles.Read(importMapPathFilename));
                if (result is not null)
                    return result;
                // 
                result = new ImportMapModel() {
                    contentName = "People",
                    groupID = 0,
                    mapPairCnt = 0,
                    mapPairs = Array.Empty<ImportMapModel_MapPair>(),
                    skipRowCnt = 1,
                    keyMethodID = 1
                };
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 

        // 
        // ====================================================================================================
        /// <summary>
        /// Save the import map data
        /// </summary>
        /// <param name="app"></param>
        public void save(ApplicationModel app, ImportConfigModel importConfig) {
            try {
                app.cp.PrivateFiles.Save(importConfig.importMapPathFilename, app.cp.JSON.Serialize(this));
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        /// <summary>
        /// Create new import map based on contentName. contentName is explicitly requested instead of using importconfig.contentid so this
        /// call must explicitly note the content is valid
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="importConfig"></param>
        /// <param name="contentName"></param>
        public static void buildNewImportMapForContent(ApplicationModel app, ImportConfigModel importConfig, string contentName) {
            int hint = 0;
            try {
                var cp = app.cp;
                // 
                // -- build a new map
                string mapName = "Import " + contentName;
                importConfig.importMapPathFilename = createMapPathFilename(app, contentName, mapName);
                importConfig.dstContentId = cp.Content.GetID(contentName);
                importConfig.save(app);
                hint = 10;
                // 
                var ImportMap = create(cp, importConfig.importMapPathFilename);
                ImportMap.contentName = contentName;

                var fieldList = C5BaseModel.createList<ContentFieldModel>(cp, "(contentId=" + importConfig.dstContentId + ")and(active>0)", "caption");

                var dbFieldList = ContentFieldModel.getDbFieldList(cp, ImportMap.contentName, false, false);
                // Dim dbFieldNames() As String = Split(ContentFieldModel.getDbFieldList(cp, ImportMap.contentName, False, False), ",")
                // Dim dbFieldNameCnt As Integer = UBound(dbFieldNames) + 1
                // todo di-hack
                app.loadUploadFields(importConfig.privateUploadPathFilename);
                string[] uploadFields = app.uploadFields;
                ImportMap.mapPairCnt = dbFieldList.Count;
                ImportMap.mapPairs = new ImportMapModel_MapPair[dbFieldList.Count];
                int rowPtr = 0;
                hint = 20;

                foreach (var dbField in dbFieldList) {
                    hint = 30;
                    // 
                    // -- content controlId should be set to table's contentid
                    if (dbField.name.ToLowerInvariant() == "contentcontrolid")
                        continue;
                    // 
                    string dbFieldName = dbField.name;
                    // 
                    // -- setup mapPair
                    var mapRow = new ImportMapModel_MapPair();
                    ImportMap.mapPairs[rowPtr] = mapRow;
                    mapRow.dbFieldName = dbFieldName;
                    mapRow.setValue = null;
                    mapRow.dbFieldType = ContentFieldModel.getFieldType(cp, dbFieldName, importConfig.dstContentId);
                    mapRow.uploadFieldPtr = -1;
                    mapRow.uploadFieldName = "";
                    hint = 40;
                    // 
                    // -- search uploadFields for matches to dbFields
                    string dBFieldName_lower = Strings.LCase(dbFieldName);
                    int uploadFieldPtr = 0;
                    foreach (string uploadField in uploadFields) {
                        hint = 41;
                        if (!string.IsNullOrEmpty(uploadField)) {
                            // 
                            string uploadField_lower = uploadField.ToLowerInvariant();
                            if ((uploadField_lower ?? "") == (dBFieldName_lower ?? "")) {
                                mapRow.uploadFieldPtr = uploadFieldPtr;
                                mapRow.uploadFieldName = uploadField;
                                break;
                            }
                            hint = 42;
                            bool exitFor = false;
                            switch (dBFieldName_lower ?? "") {
                                case "company": {
                                        hint = 43;
                                        if (uploadField_lower == "companyname" || uploadField_lower == "company name") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "zip": {
                                        hint = 44;
                                        if (uploadField_lower == "zip code" || uploadField_lower == "zipcode" || uploadField_lower == "postal code" || uploadField_lower == "postalcode" || uploadField_lower == "zip/postalcode") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "firstname": {
                                        hint = 45;
                                        if (uploadField_lower == "first" || uploadField_lower == "first name") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "lastname": {
                                        hint = 46;
                                        if (uploadField_lower == "last" || uploadField_lower == "last name") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "email": {
                                        hint = 47;
                                        if (uploadField_lower == "e-mail" || uploadField_lower == "emailaddress" || uploadField_lower == "e-mailaddress") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "address": {
                                        hint = 48;
                                        if (uploadField_lower == "address1" || uploadField_lower == "addressline1") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "address2": {
                                        hint = 49;
                                        if (uploadField_lower == "addressline2") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                                case "phone":
                                case "cellphone": {
                                        hint = 50;
                                        if (uploadField_lower == "phone number" || uploadField_lower == "phonenumber") {
                                            mapRow.uploadFieldPtr = uploadFieldPtr;
                                            mapRow.uploadFieldName = uploadField;
                                            exitFor = true;
                                            break;
                                        }

                                        break;
                                    }
                            }

                            if (exitFor) {
                                break;
                            }
                        }
                        uploadFieldPtr += 1;
                    }
                    hint = 60;
                    rowPtr += 1;
                }
                hint = 70;
                ImportMap.save(app, importConfig);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex, $"hint [{hint}]");
                throw;
            }
        }
    }
    // 
    public class ImportMapModel_MapPair {
        /// <summary>
        /// 0-based index to the column of the uploaded data file
        /// -1 = ignore this fields
        /// -2 = set the value from the setValue field
        /// -3 = combine 'firstname lastname' (valid for people content only. All other tables will ignore this)
        /// -4 = firstname from name field (valid for people content only. All other tables will ignore this)
        /// -5 = lastname from name field (valid for people content only. All other tables will ignore this)
        /// </summary>
        /// <returns></returns>
        public int uploadFieldPtr { get; set; }
        /// <summary>
        /// the name of the column from the uploaded data file
        /// </summary>
        /// <returns></returns>
        public string uploadFieldName { get; set; }
        public string dbFieldName { get; set; }
        public int dbFieldType { get; set; }
        /// <summary>
        /// if not null, ignore dbfield and set the 
        /// </summary>
        /// <returns></returns>
        public string setValue { get; set; }
    }
    // 
    public class MapFilenameDataModel {
        public string filename { get; set; }
        public string mapName { get; set; }
        public DateTime dateCreated { get; set; }
    }
    // 
    public enum MapKeyEnum {
        KeyMethodInsertAll = 1,
        KeyMethodUpdateOnMatch = 2,
        KeyMethodUpdateOnMatchInsertOthers = 3
    }

}