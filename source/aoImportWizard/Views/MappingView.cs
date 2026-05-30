using System;
using System.Linq;
using Contensive.ImportWizard.Models;
using C5BaseModel = Contensive.Models.Db.DbBaseModel;

namespace Contensive.ImportWizard.Controllers {
    public class MappingView {
        //
        /// <summary>
        /// return the next view. 0 goes to the first form (start over)
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static int processView(ApplicationModel app, int srcViewId) {
            try {
                var cp = app.cp;
                string Button = app.cp.Doc.GetText(constants.RequestNameButton);
                if (string.IsNullOrEmpty(Button))
                    return constants.viewIdSelectSource;
                if ((Button ?? "") == constants.ButtonCancel) {
                    ImportConfigModel.clear(app);
                    return constants.viewIdReturnBlank;
                }
                if ((Button ?? "") == constants.ButtonRestart) {
                    ImportConfigModel.clear(app);
                    return constants.viewIdSelectSource;
                }
                //
                var importConfig = ImportConfigModel.create(app);
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                if (cp.Doc.GetBoolean(constants.RequestNameImportSkipFirstRow)) {
                    ImportMap.skipRowCnt = 1;
                } else {
                    ImportMap.skipRowCnt = 0;
                }
                int FieldCnt = cp.Doc.GetInteger("ccnt");
                ImportMap.mapPairCnt = FieldCnt;
                if (FieldCnt > 0) {
                    ImportMap.mapPairs = new ImportMapModel_MapPair[FieldCnt];
                    for (int Ptr = 0, loopTo = FieldCnt - 1; Ptr <= loopTo; Ptr++) {
                        string dbFieldName = cp.Doc.GetText("DBFIELD" + Ptr);
                        int fieldTypeId = 0;
                        var fieldList = C5BaseModel.createList<ContentFieldModel>(cp, "(name=" + cp.Db.EncodeSQLText(dbFieldName) + ")and(contentid=" + importConfig.dstContentId + ")");
                        if (fieldList.Count > 0) {
                            fieldTypeId = fieldList.First().type;
                        }
                        int sourceFieldPtr = cp.Doc.GetInteger("SOURCEFIELD" + Ptr);
                        string setValue = null;
                        if (sourceFieldPtr == -2)
                            setValue = cp.Doc.GetText("setValueField" + Ptr);
                        ImportMap.mapPairs[Ptr] = new ImportMapModel_MapPair() {
                            uploadFieldPtr = sourceFieldPtr,
                            dbFieldName = dbFieldName,
                            dbFieldType = fieldTypeId,
                            setValue = setValue
                        };
                    }
                }

                var mapfilenamedata = ImportMapModel.decodeMapFileName(cp, cp.PrivateFiles.GetFilename(importConfig.importMapPathFilename));
                string ImportMapName = "";
                if (mapfilenamedata is not null) {
                    ImportMapName = mapfilenamedata.mapName;
                }
                string mapName = cp.Doc.GetText(constants.requestNameImportMapName);
                if ((mapName ?? "") != (ImportMapName ?? "")) {
                    importConfig.importMapPathFilename = ImportMapModel.createMapPathFilename(app, ImportMap.contentName, mapName);
                    importConfig.save(app);
                }

                ImportMap.save(app, importConfig);
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            return constants.viewIdSelectMap;
                        }
                    case constants.ButtonContinue: {
                            return constants.viewIdSelectKey;
                        }
                }
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }

            return default;
        }
        //
        /// <summary>
        /// return the html for this view
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static string getView(ApplicationModel app) {
            try {
                var cp = app.cp;
                string headerCaption = "Import Wizard";
                var importConfig = ImportConfigModel.create(app);
                string Description = cp.Html.h4("Create a New Mapping") + cp.Html.p("This step lets you select which fields in your database you would like each field in your upload to be assigned.");
                if (string.IsNullOrEmpty(importConfig.privateUploadPathFilename)) {
                    return HtmlController.createLayout(cp, headerCaption, Description, "<P>The file you are importing is empty. Please go back and select a different file.</P>", true, true, true, false);
                }
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                string result = "";
                result += cp.Html.CheckBox(constants.RequestNameImportSkipFirstRow, ImportMap.skipRowCnt != 0) + "&nbsp;First row contains field names";
                //
                var mapfilenamedata = ImportMapModel.decodeMapFileName(cp, cp.PrivateFiles.GetFilename(importConfig.importMapPathFilename));
                string ImportMapName = "";
                if (mapfilenamedata is not null) {
                    ImportMapName = mapfilenamedata.mapName;
                }
                result += "<div class=\"mt-4 form-group\" style=\"max-width:600px;\">";
                result += "<label for=\"js-import-name\" style=\"\">Name for this field map</label>";
                result += cp.Html5.InputText(constants.requestNameImportMapName, 100, ImportMapName, "form-control", "js-import-name");
                result += "</div>";
                result += "<TABLE class=\"mt-4\" border=0 cellpadding=2 cellspacing=0 width=100%>";
                result += "<TR><TD align=left>Data From</TD><TD align=left width=150>Set Value</TD><TD align=center width=10></TD><TD align=left width=300>Save Data To</TD><TD align=left width=150>Type</TD></TR>";

                string uploadFieldSelectTemplate = HtmlController.getSourceFieldSelect(app, importConfig.privateUploadPathFilename, "Ignore", importConfig.dstContentId, true, -99, "{{inputName}}", "js-import-select-{{fieldPtr}}");
                int rowPtr = 0;
                var fieldList = C5BaseModel.createList<ContentFieldModel>(cp, "contentId=" + importConfig.dstContentId);
                foreach (ImportMapModel_MapPair mapPair in ImportMap.mapPairs) {
                    if (mapPair is null)
                        continue;
                    var mapField = fieldList.Find(x => (x.name ?? "") == (mapPair.dbFieldName ?? ""));
                    string cell0Style = "min-width:100px;";
                    string cell1Style = "";
                    string cell2Style = "";
                    string cell3Style = "";
                    string cell4Style = "";
                    string dbFieldTypeCaption;
                    string valueEditor = "";
                    string valueEditorHtmlName = "setValueField" + rowPtr;
                    string setValueValue = mapPair.setValue;
                    switch (mapPair.dbFieldType) {
                        case constants.FieldTypeBoolean: {
                                dbFieldTypeCaption = "true/false";
                                valueEditor = "<input type=\"checkbox\" name=\"" + valueEditorHtmlName + "\" value=\"1\" class=\"js-import-manual-data\" style=\"{{styles}}\"" + (cp.Utils.EncodeBoolean(setValueValue) ? " checked" : "") + ">";
                                cell1Style += "vertical-align:middle;text-align:center;";
                                break;
                            }
                        case constants.FieldTypeCurrency:
                        case constants.FieldTypeFloat: {
                                dbFieldTypeCaption = "Number";
                                valueEditor = "<input type=\"number\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeDate: {
                                dbFieldTypeCaption = "Date";
                                valueEditor = "<input type=\"date\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeFile:
                        case constants.FieldTypeImage:
                        case constants.FieldTypeTextFile:
                        case constants.FieldTypeCSSFile:
                        case constants.FieldTypeXMLFile:
                        case constants.FieldTypeJavascriptFile:
                        case constants.FieldTypeHTMLFile: {
                                dbFieldTypeCaption = "Filename";
                                valueEditor = "<input type=\"file\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeInteger: {
                                dbFieldTypeCaption = "Integer";
                                valueEditor = "<input type=\"number\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeLongText:
                        case constants.FieldTypeHTML: {
                                dbFieldTypeCaption = "Text (8000 char)";
                                valueEditor = "<input type=\"text\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeLookup: {
                                if (mapField.lookupContentId > 0) {
                                    string mapContentName = cp.Content.GetName(mapField.lookupContentId);
                                    valueEditor = cp.Html5.SelectContent(valueEditorHtmlName, setValueValue, mapContentName, "", "", "form-control js-import-manual-data").Replace("<select ", "<select style=\"{{styles}}\"");
                                } else if (!string.IsNullOrEmpty(mapField.lookupList)) {
                                    valueEditor = cp.Html5.SelectList(valueEditorHtmlName, setValueValue, mapField.lookupList, "", "form-control js-import-manual-data").Replace("<select ", "<select style=\"{{styles}}\"");
                                } else {
                                    valueEditor = "<input type=\"number\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                }
                                dbFieldTypeCaption = "Lookup";
                                break;
                            }
                        case constants.FieldTypeManyToMany: {
                                dbFieldTypeCaption = "Integer ID";
                                valueEditor = "";
                                break;
                            }
                        case constants.FieldTypeMemberSelect: {
                                dbFieldTypeCaption = "Integer ID";
                                valueEditor = "<input type=\"number\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }
                        case constants.FieldTypeText:
                        case constants.FieldTypeLink:
                        case constants.FieldTypeResourceLink: {
                                dbFieldTypeCaption = "Text (255 char)";
                                valueEditor = "<input type=\"text\" name=\"" + valueEditorHtmlName + "\" value=\"" + setValueValue + "\" class=\"form-control js-import-manual-data\" style=\"{{styles}}\">";
                                break;
                            }

                        default: {
                                dbFieldTypeCaption = $"Invalid [{mapPair.dbFieldType}]";
                                valueEditor = "";
                                break;
                            }
                    }
                    string uploadFieldSelect = uploadFieldSelectTemplate.Replace("{{fieldPtr}}", rowPtr.ToString()).Replace("{{inputName}}", "SourceField" + rowPtr);
                    switch (mapPair.uploadFieldPtr) {
                        case -5: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, "value=\"-5\">", "value=\"-5\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "display:none;");
                                break;
                            }
                        case -4: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, "value=\"-4\">", "value=\"-4\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "display:none;");
                                break;
                            }
                        case -3: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, "value=\"-3\">", "value=\"-3\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "display:none;");
                                break;
                            }
                        case -2: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, "value=\"-2\">", "value=\"-2\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "");
                                break;
                            }
                        case -1: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, "value=\"-1\">", "value=\"-1\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "display:none;");
                                break;
                            }

                        default: {
                                uploadFieldSelect = ReplaceIgnoreCase(uploadFieldSelect, $"value=\"{mapPair.uploadFieldPtr}\">", $"value=\"{mapPair.uploadFieldPtr}\" selected>");
                                valueEditor = valueEditor.Replace("{{styles}}", "display:none;");
                                break;
                            }
                    }
                    string dbFieldCaption = mapPair.dbFieldName;
                    if (mapField is not null) {
                        dbFieldCaption = mapField.caption;
                    }
                    string rowStyle;
                    if (rowPtr % 2 == 0) {
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:white;vertical-align:middle;";
                    } else {
                        rowStyle = "border-top:1px solid #e0e0e0;border-right:1px solid white;background-color:#f0f0f0;vertical-align:middle;";
                    }
                    result = result + "\r\n" + "<TR>" + $"<TD style=\"{cell0Style}{rowStyle}\" align=left>{uploadFieldSelect}</td>" + $"<TD style=\"{cell1Style}{rowStyle}\" align=left>{valueEditor}</td>" + $"<TD style=\"{cell2Style}{rowStyle}\" align=center>&gt;&gt;</TD>" + $"<TD style=\"{cell3Style}{rowStyle}\" align=left>&nbsp;{dbFieldCaption}<input type=hidden name=DbField{rowPtr} value=\"{mapPair.dbFieldName}\"></td>" + $"<TD style=\"{cell4Style}{rowStyle}\" align=left>&nbsp;{dbFieldTypeCaption}</td>" + "</TR>";

                    rowPtr += 1;
                }
                result += "<input type=hidden name=Ccnt value=" + rowPtr + ">";
                result += "</TABLE>";
                result += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdMapping.ToString());
                return HtmlController.createLayout(cp, headerCaption, Description, result, true, true, true, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        /// <summary>
        /// Case-insensitive string replace (replaces VB Strings.Replace with vbTextCompare)
        /// </summary>
        private static string ReplaceIgnoreCase(string source, string find, string replacement) {
            int idx = source.IndexOf(find, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return source;
            return source.Substring(0, idx) + replacement + source.Substring(idx + find.Length);
        }
    }
}
