using System;
using System.Collections.Generic;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class SelectMapView {
        // 
        public static int processView(ApplicationModel app, int srcViewId, List<string> returnUserError) {
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
                string contentName = cp.Content.GetName(importConfig.dstContentId);
                if (string.IsNullOrEmpty(contentName)) {
                    returnUserError.Add("Cannot create the import map without a valid table selection.");
                }
                if (string.IsNullOrEmpty(cp.Doc.GetText("selectMapRow"))) {
                    // 
                    // -- create new map for this content
                    ImportMapModel.buildNewImportMapForContent(app, importConfig, contentName);
                } else {
                    // 
                    // -- use a previous mapping for this content
                    importConfig.importMapPathFilename = ImportMapModel.getMapPath(app, contentName) + cp.Doc.GetText("selectMapRow");
                }
                importConfig.save(app);
                // 
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to select content
                            return constants.viewIdSelectContent;
                        }
                    case constants.ButtonContinue: {
                            // 
                            // -- 
                            // 
                            return constants.viewIdMapping;
                        }
                }
                return constants.viewIdUpload;
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
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
                string description = cp.Html.h4("Select how the import maps to the table.") + "<p>To import data into this table, you have to map the input fields to the database fields. You can either create a new map or select one you have previously used.</p>";
                var importConfig = ImportConfigModel.create(app);
                string contentName = cp.Content.GetName(importConfig.dstContentId);
                var optionList = new List<string>() { "<option value=\"\">Create New Field Mapping</option>" };
                string currentFilename = app.cp.PrivateFiles.GetFilename(importConfig.importMapPathFilename);
                foreach (var @file in ImportMapModel.getMapFileList(app, contentName)) {
                    string displayName = @file.mapName + ", " + @file.dateCreated.ToString();
                    optionList.Add("<option value=\"" + cp.Utils.EncodeHTML(@file.filename) + "\" " + ((@file.filename ?? "") == (currentFilename ?? "") ? " selected " : "") + ">" + displayName + "</option>");
                }
                string content = "<select name=\"selectMapRow\" class=\"form-control\" id=\"\">" + string.Join("", optionList) + "</select>";
                content += cp.Html5.Hidden(constants.rnSrcViewId, constants.viewIdSelectMap);
                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, true, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}