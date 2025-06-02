using System;
using System.Linq;
using Contensive.ImportWizard.Models;
using C5BaseModel = Contensive.Models.Db.DbBaseModel;
using Microsoft.VisualBasic;

namespace Contensive.ImportWizard.Controllers {
    public class SelectKeyView {
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
                    return srcViewId;
                if ((Button ?? "") == constants.ButtonCancel) {
                    // 
                    // Cancel
                    ImportConfigModel.clear(app);
                    return constants.viewIdReturnBlank;
                }
                if ((Button ?? "") == constants.ButtonRestart) {
                    // 
                    // Restart
                    ImportConfigModel.clear(app);
                    return constants.viewIdSelectSource;
                }
                // 
                var importConfig = ImportConfigModel.create(app);
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                ImportMap.keyMethodID = cp.Doc.GetInteger(constants.RequestNameImportKeyMethodID);
                ImportMap.sourceKeyField = cp.Doc.GetText(constants.RequestNameImportSourceKeyFieldPtr);
                ImportMap.dbKeyField = cp.Doc.GetText(constants.RequestNameImportDbKeyField);
                if (!string.IsNullOrEmpty(ImportMap.dbKeyField)) {
                    var fieldList = C5BaseModel.createList<ContentFieldModel>(cp, "(name=" + cp.Db.EncodeSQLText(ImportMap.dbKeyField) + ")and(contentid=" + cp.Content.GetID(ImportMap.contentName) + ")");
                    if (fieldList.Count > 0) {
                        ImportMap.dbKeyFieldType = fieldList.First().type;
                    }
                }
                ImportMap.save(app, importConfig);

                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to mapping
                            return constants.viewIdMapping;
                        }
                    case constants.ButtonContinue: {
                            // 
                            // -- continue to Select Group or finish
                            if (importConfig.dstContentId == app.peopleContentid) {
                                return constants.viewIdSelectGroup;
                            } else {
                                return constants.viewIdFinish;
                            }
                        }
                }
                // '
                // Select Case Button
                // Case ButtonBack2
                // '
                // ' -- back to select source
                // Return viewIdSelectSource
                // Case ButtonContinue2
                // '
                // ' -- upload the file and continue
                // Dim Filename As String = app.cp.Doc.GetText(RequestNameImportUpload)
                // If String.IsNullOrEmpty(Filename) Then Return viewIdSelectSource
                // '
                // Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                // app.cp.TempFiles.SaveUpload(RequestNameImportUpload, "upload", importConfig.privateUploadPathFilename)
                // importConfig.save(app)
                // '
                // Return viewIdSelectTable
                // End Select
                // Return viewIdUpload
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
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);

                int KeyMethodID = cp.Utils.EncodeInteger(ImportMap.keyMethodID);
                if (KeyMethodID == 0) {
                    KeyMethodID = (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers;
                }

                int SourceKeyFieldPtr;
                // 
                if (!string.IsNullOrEmpty(ImportMap.sourceKeyField)) {
                    SourceKeyFieldPtr = cp.Utils.EncodeInteger(ImportMap.sourceKeyField);
                } else {
                    SourceKeyFieldPtr = -1;
                }
                string Filename = importConfig.privateUploadPathFilename;
                string uploadFieldSelect = HtmlController.getSourceFieldSelect(app, Filename, "Select One", importConfig.dstContentId, false, SourceKeyFieldPtr, constants.RequestNameImportSourceKeyFieldPtr, "js-import-select");
                // 
                // -- Pick any field for key if developer or not the ccMembers table
                string LookupContentName;
                LookupContentName = cp.Content.GetRecordName("content", importConfig.dstContentId);
                string DBFieldSelect = HtmlController.getDbFieldSelect(cp, LookupContentName, "Select One", true, ImportMap.dbKeyField);
                DBFieldSelect = Strings.Replace(DBFieldSelect, "xxxx", constants.RequestNameImportDbKeyField);
                // 
                string Description = ""; // "cp.Html.h4("Update Control") & cp.Html.p("When your data is imported, it can either update your current database, or insert new records into your database. Use this form to control which records will be updated, and which will be inserted.")
                string Content = "" + "<div>" + "<h4>Update Options</h4>" + "<p>When the import file is added to the table, should records be insert, updated or both?</p>" + HtmlController.getRadio(cp, constants.RequestNameImportKeyMethodID, (int)MapKeyEnum.KeyMethodInsertAll, KeyMethodID, "Insert all imported data.", "js-radio-insert") + HtmlController.getRadio(cp, constants.RequestNameImportKeyMethodID, (int)MapKeyEnum.KeyMethodUpdateOnMatchInsertOthers, KeyMethodID, "Update database records from the import data when the key fields match. Insert all other imported data.", "js-radio-update-insert") + HtmlController.getRadio(cp, constants.RequestNameImportKeyMethodID, (int)MapKeyEnum.KeyMethodUpdateOnMatch, KeyMethodID, "Update database records from the import data when the key fields match. Ignore imported data that does not match.", "js-radio-update") + "<div id=\"js-key-fields\" style=\"display:none\">" + "<h4>Key Fields</h4>" + "<p>If records will be updated, select a field in the upload and a field in the table to match.</p>" + "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" + "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" + "<TABLE border=0 cellpadding=2 cellspacing=0 width=100%>" + "<tr><td>Imported&nbsp;Key&nbsp;</td><td>" + uploadFieldSelect + "</td></tr>" + "<tr><td>Database&nbsp;Key&nbsp;</td><td>" + DBFieldSelect + "</td></tr>" + "</table>" + "</td></tr>" + "</table>" + "</div>" + "</div>" + "";



















                Content += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdSelectKey.ToString());
                return HtmlController.createLayout(cp, headerCaption, Description, Content, true, true, true, true);
                // Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}