using System;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class SelectContentView {
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
                // Load the importmap with what we have so far
                // 
                var importConfig = ImportConfigModel.create(app);
                // 
                if (cp.Doc.GetBoolean("useNewContentName")) {
                    string contentName = cp.Doc.GetText("newContentName");
                    string mapName = "Import " + contentName;
                    // 
                    // -- reset import map
                    importConfig.importMapPathFilename = ImportMapModel.createMapPathFilename(app, contentName, mapName);
                    importConfig.dstContentId = 0;
                    importConfig.save(app);
                    // 
                    // -- new table. Save table and return
                    var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                    ImportMap.contentName = cp.Doc.GetText("newContentName");
                    ImportMap.importToNewContent = true;
                    ImportMap.skipRowCnt = 1;
                    ImportMap.save(app, importConfig);
                    // 
                    switch (Button ?? "") {
                        case constants.ButtonBack: {
                                // 
                                // -- back
                                return constants.viewIdSelectSource;
                            }

                        default: {
                                // 
                                // -- continue
                                return constants.viewIdSelectMap;
                            }
                    }
                }
                // 
                // -- Match to existing table
                if (importConfig.dstContentId != cp.Doc.GetInteger(constants.RequestNameImportContentID)) {
                    // 
                    // -- content changed, reset import map
                    // importConfig.newImportMap(app)
                    importConfig.dstContentId = cp.Doc.GetInteger(constants.RequestNameImportContentID);
                    importConfig.importMapPathFilename = "";
                    importConfig.save(app);
                }
                // 
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back
                            return constants.viewIdSelectSource;
                        }

                    default: {
                            // 
                            // -- continue
                            return constants.viewIdSelectMap;
                        }
                }
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

                var importConfig = ImportConfigModel.create(app);
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                int ImportContentID = importConfig.dstContentId;
                if (ImportContentID == 0) {
                    ImportContentID = app.peopleContentid;
                }
                string inputRadioNewContent;
                string inputRadioExistingContent;
                // 
                if (ImportMap.importToNewContent) {
                    inputRadioNewContent = "<input type=\"radio\" name=\"useNewContentName\" class=\"mr-2\" value=\"1\" checked>";
                    inputRadioExistingContent = "<input type=\"radio\" name=\"useNewContentName\" class=\"mr-2\" value=\"0\">";
                } else {
                    inputRadioNewContent = "<input type=\"radio\" name=\"useNewContentName\" class=\"mr-2\" value=\"1\">";
                    inputRadioExistingContent = "<input type=\"radio\" name=\"useNewContentName\" class=\"mr-2\" value=\"0\" checked>";
                }
                string Description = cp.Html.h4("Select the destination for your data") + cp.Html.p("For example, to import a list in to people, select People.");
                string contentSelect = cp.Html.SelectContent(constants.RequestNameImportContentID, ImportContentID.ToString(), "Content", "", "", "form-control");
                contentSelect = contentSelect.Replace("<select ", "<select style=\"max-width:300px; display:inline;\" ");
                string Content = "" + "<div>" + "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" + "<tr><td colspan=\"2\">Import into an existing content table</td></tr>" + "<tr><td colspan=\"2\">" + inputRadioExistingContent + "&nbsp;" + contentSelect + "</td></tr>" + "<tr><td colspan=\"2\">Create a new content table</td></tr>" + "<tr><td colspan=\"2\">" + inputRadioNewContent + "&nbsp;<input style=\"max-width:300px; display:inline;\" type=\"text\" name=\"newContentName\" value=\"" + (ImportMap.importToNewContent ? ImportMap.contentName : "") + "\" class=\"form-control\"></td></tr>" + "</table>" + "</div>" + "";








                Content += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdSelectContent.ToString());
                return HtmlController.createLayout(cp, headerCaption, Description, Content, true, true, true, true);
                // Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, Description, Content)
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}