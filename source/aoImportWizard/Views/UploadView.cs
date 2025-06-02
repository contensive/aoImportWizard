using System;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class UploadView {
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
                if (string.IsNullOrEmpty(Button)) {
                    // 
                    // -- no button, stay here
                    return srcViewId;
                }
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
                            string privateUploadPathFilename = app.cp.Doc.GetText(constants.RequestNameImportUpload);
                            if (string.IsNullOrEmpty(privateUploadPathFilename)) {
                                // 
                                // -- no file uploaded, stay here
                                return srcViewId;
                            }
                            // 
                            var importConfig = ImportConfigModel.create(app);
                            if ((importConfig.privateUploadPathFilename ?? "") != (privateUploadPathFilename ?? "")) {
                                // 
                                // -- upload changed, reset map
                                importConfig.importMapPathFilename = "";
                                // importConfig.newEmptyImportMap(app)
                            }
                            bool localSaveUpload() { string argreturnFilename = importConfig.privateUploadPathFilename; var ret = app.cp.PrivateFiles.SaveUpload(constants.RequestNameImportUpload, constants.privateFilesUploadPath, ref argreturnFilename); importConfig.privateUploadPathFilename = argreturnFilename; return ret; }

                            if (!localSaveUpload()) {
                                // 
                                // -- upload failed, stay on this view
                                return srcViewId;
                            }
                            importConfig.privateUploadPathFilename = constants.privateFilesUploadPath + importConfig.privateUploadPathFilename;
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
                string description = cp.Html.h4("Upload your File") + cp.Html.p("Hit browse to locate the file you want to upload");
                string content = "" + "<div>" + "<TABLE border=0 cellpadding=10 cellspacing=0 width=100%>" + "<TR><TD width=1>&nbsp;</td><td width=99% align=left>" + cp.Html.InputFile(constants.RequestNameImportUpload) + "</td></tr>" + "</table>" + "</div>" + "";





                content += cp.Html5.Hidden(constants.rnSrcViewId, constants.viewIdUpload);
                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, true, true);
                // Return HtmlController.createLayout(cp, headerCaption, ButtonCancel, ButtonBack2, ButtonContinue2, description, content)
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}