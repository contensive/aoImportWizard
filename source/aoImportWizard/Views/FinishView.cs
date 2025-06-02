using System;
using Contensive.ImportWizard.Models;
using C5BaseModel = Contensive.Models.Db.DbBaseModel;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.ImportWizard.Controllers {
    public class FinishView {
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


                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to select key
                            return constants.viewIdSelectKey;
                        }
                    case constants.ButtonContinue: {
                            var importConfig = ImportConfigModel.create(app);
                            var ImportWizardTasks = C5BaseModel.addDefault<Contensive.Models.Db.ImportWizardTaskModel>(cp);
                            if (ImportWizardTasks is not null) {
                                ImportWizardTasks.name = Conversions.ToString(DateTime.Now) + " CSV Import";
                                ImportWizardTasks.uploadFilename = importConfig.privateUploadPathFilename;
                                ImportWizardTasks.notifyEmail = importConfig.notifyEmail;
                                ImportWizardTasks.importMapFilename = importConfig.importMapPathFilename;
                                ImportWizardTasks.save(cp);
                            }
                            // 
                            // -- queue the import tak
                            cp.Addon.ExecuteAsProcess(constants.guidAddonImportTask);
                            // 
                            // -- clear the import wizard to start fresh
                            ImportConfigModel.clear(app);
                            // 
                            // -- thank you page, with reset button
                            return constants.viewIdDone;
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
                string description = "";
                string content = "";
                var importConfig = ImportConfigModel.create(app);
                // 
                description = cp.Html.h4("Finish") + cp.Html.p("Your list will be submitted for import when you hit the finish button. Processing may take several minutes, depending on the size and complexity of your import. If you supply an email address, you will be notified with the import is complete.");
                content = "<div Class=\"p-2\"><label for=\"name381\">Email</label><div class=\"ml-5\">" + cp.Html5.InputText(constants.RequestNameImportEmail, 255, cp.User.Email) + "</div><div class=\"ml-5\"><small class=\"form-text text-muted\"></small></div></div>";
                content += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdFinish.ToString());
                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, true, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}