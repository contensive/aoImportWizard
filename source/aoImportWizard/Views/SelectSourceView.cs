using System;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class SelectSourceView {
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
                importConfig.importSource = (ImportDataModel_ImportTypeEnum)app.cp.Doc.GetInteger(constants.RequestNameImportSource);
                importConfig.save(app);
                // 
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            return srcViewId;
                        }
                    case constants.ButtonContinue: {
                            // 
                            switch (importConfig.importSource) {
                                case ImportDataModel_ImportTypeEnum.UploadFile: {
                                        // 
                                        // -- upload a commad delimited file
                                        return constants.viewIdUpload;
                                    }

                                default: {
                                        // 
                                        // -- use a file uploaded previously
                                        return constants.viewIdSelectFile;
                                    }
                            }

                            break;
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
                var importConfig = ImportConfigModel.create(app);
                string headerCaption = "Import Wizard";
                int importSource = Convert.ToInt32((int)importConfig.importSource);
                string description = cp.Html.h4("Select the import source") + cp.Html.p("If you want to upload a new comma-delimited text file (csv), select the upload option. If you want to use a file you previously uploaded, select the second option.");
                string content = "" + "<div class=\"ml-4\">" 
                    + "<div class=\"pb-2\">" 
                    + "<label>" + cp.Html.RadioBox(constants.RequestNameImportSource, constants.ImportSourceUpload.ToString(), importSource.ToString(), "mr-2") + "&nbsp;Upload a comma delimited text file (up to 5 MBytes).</label>" 
                    + "</div>" 
                    + "<div class=\"pb-2\">" + "<label>" + cp.Html.RadioBox(constants.RequestNameImportSource, constants.ImportSourceUploadFolder.ToString(), importSource.ToString(), "mr-2") + "&nbsp;Use a file you previously uploaded.</label>" + "</div>" + "</div>" + "";








                content += cp.Html5.Hidden(constants.rnSrcViewId, constants.viewIdSelectSource);
                return HtmlController.createLayout(cp, headerCaption, description, content, true, false, false, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}