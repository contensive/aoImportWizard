using System;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class DoneView {
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
                return constants.viewIdDone;
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



                headerCaption = "Import Wizard";
                description = cp.Html.h4("Import Requested") + cp.Html.p("Your import is underway and should only take a moment.");
                content = cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdDone.ToString());
                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, false, false);

            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}