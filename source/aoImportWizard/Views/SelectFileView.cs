using System;
using System.Collections.Generic;
using System.Text;
using Contensive.ImportWizard.Models;
using Microsoft.VisualBasic;

namespace Contensive.ImportWizard.Controllers {
    public class SelectFileView {
        // 
        /// <summary>
        /// return the next view. 0 goes to the first form (start over)
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
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
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to select source
                            return constants.viewIdSelectSource;
                        }
                    case constants.ButtonContinue: {
                            // 
                            // -- continue to select source file
                            var importConfig = ImportConfigModel.create(app);
                            if (string.IsNullOrEmpty(app.cp.Doc.GetText("SelectFile"))) {
                                // 
                                // -- no file selected
                                returnUserError.Add("You must select a file to continue.");
                                return srcViewId;
                            }
                            if ((importConfig.privateUploadPathFilename ?? "") != (app.cp.Doc.GetText("SelectFile") ?? "")) {
                                // 
                                // -- there was a change in the file, reset the map
                                importConfig.importMapPathFilename = "";
                                importConfig.privateUploadPathFilename = app.cp.Doc.GetText("SelectFile");
                                importConfig.save(app);
                            }
                            if (Strings.Left(importConfig.privateUploadPathFilename, 1) == @"\")
                                importConfig.privateUploadPathFilename = Strings.Mid(importConfig.privateUploadPathFilename, 2);
                            importConfig.save(app);
                            // 
                            return constants.viewIdSelectContent;
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
                string Description = cp.Html.h4("Select a file to import.") + cp.Html.p("Select a file that will be imported. This is a list of files you have previously uploaded.");
                var fileList2 = new StringBuilder();
                int uploadPtr = 0;
                foreach (var @file in cp.PrivateFiles.FileList(constants.privateFilesUploadPath)) {
                    string uploadId = "upload" + uploadPtr;
                    fileList2.Append(@$"
                        <div class=""form-check"">
                            <input class=""form-check-input"" type=""radio"" name=""selectfile"" value=""{constants.privateFilesUploadPath + @file.Name}"" id=""{uploadId}"">
                            <label class=""form-check-label"" for=""{uploadId}"">{@file.Name}</label>
                        </div>");
                    //string input = @$"
                    //    <div class=""form-check"">
                    //        <input class=""form-check-input"" type=""radio"" name=""selectfile"" value=""{constants.privateFilesUploadPath + @file.Name}"" id=""{uploadId}"">
                    //        <label class=""form-check-label"" for=""{uploadId}"">@file.Name</label>
                    //    </div>";
                    //{ cp.Html.RadioBox("selectfile", constants.privateFilesUploadPath + @file.Name, "", "form-check-input", uploadId)}
                    //fileList2.Append(cp.Html.div(input, "", "pb-2"));
                    uploadPtr += 1;
                }
                string Content = "<div class=\"pl-4\">" + fileList2.ToString() + "</div>";
                Content += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdSelectFile.ToString());
                // 
                return HtmlController.createLayout(cp, headerCaption, Description, Content, true, true, true, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}