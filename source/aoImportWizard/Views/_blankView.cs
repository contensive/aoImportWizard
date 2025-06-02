using System;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class _blankView {
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
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to select source
                            return constants.viewIdSelectSource;
                        }
                    case constants.ButtonContinue: {
                            // 
                            // -- upload the file and continue
                            string Filename = app.cp.Doc.GetText(constants.RequestNameImportUpload);
                            if (string.IsNullOrEmpty(Filename))
                                return constants.viewIdSelectSource;
                            // 
                            var importConfig = ImportConfigModel.create(app);
                            string argreturnFilename = importConfig.privateUploadPathFilename;
                            app.cp.TempFiles.SaveUpload(constants.RequestNameImportUpload, "upload", ref argreturnFilename);
                            importConfig.privateUploadPathFilename = argreturnFilename;
                            importConfig.save(app);
                            // 
                            return constants.viewIdSelectContent;
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
                string description = "";
                string content = "";
                var importConfig = ImportConfigModel.create(app);







                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, true, true);

            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}