using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.ImportWizard.Controllers;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Addons {
    /// <summary>
    /// The addon that runs on the page -- setup the import files
    /// </summary>
    public class ImportWizardAddon : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// The addon that runs on the page -- setup the import files
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            try {
                // 
                // -- initialize application. If authentication needed and not login page, pass true
                using (var app = new ApplicationModel(CP, false)) {
                    // 
                    // Process incoming form
                    var userErrors = new List<string>();
                    int viewId = CP.Doc.GetInteger(constants.rnSrcViewId);
                    switch (viewId) {
                        case constants.viewIdSelectSource: {
                                // 
                                // Source and ContentName
                                // 
                                viewId = SelectSourceView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdUpload: {
                                // 
                                // Upload
                                // 
                                viewId = UploadView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdSelectFile: {
                                // 
                                // Select file
                                // 
                                viewId = SelectFileView.processView(app, viewId, userErrors);
                                break;
                            }
                        case constants.viewIdSelectContent: {
                                // 
                                // Source and ContentName
                                // 
                                viewId = SelectContentView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdSelectMap: {
                                // 
                                // select Mapping
                                // 
                                viewId = SelectMapView.processView(app, viewId, userErrors);
                                break;
                            }
                        case constants.viewIdMapping: {
                                // 
                                // Mapping - Save Values to the file pointed to by RequestNameImportMapFile
                                // 
                                viewId = MappingView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdSelectKey: {
                                // 
                                // Select Key Field
                                // 
                                viewId = SelectKeyView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdSelectGroup: {
                                // 
                                // Add to group
                                // 
                                viewId = SelectGroupView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdFinish: {
                                // 
                                // Determine next or previous form
                                // 
                                viewId = FinishView.processView(app, viewId);
                                break;
                            }
                        case constants.viewIdDone: {
                                // 
                                // nothing to do, keep same form
                                viewId = DoneView.processView(app, viewId);
                                break;
                            }
                    }
                    // 
                    // Get Next Form
                    // 
                    string body = "";
                    switch (viewId) {
                        case constants.viewIdUpload: {
                                // 
                                // Upload file to Upload folder
                                // 
                                body = UploadView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdSelectFile: {
                                // 
                                // Select a file from the upload folder
                                // 
                                body = SelectFileView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdSelectContent: {
                                // 
                                // Destination
                                // 
                                body = SelectContentView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdSelectMap: {
                                // 
                                // 
                                // 
                                body = SelectMapView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdMapping: {
                                // 
                                // 
                                // 
                                body = MappingView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdSelectKey: {
                                // 
                                // Select Key
                                // 
                                body = SelectKeyView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdSelectGroup: {
                                // 
                                // Select a group to add
                                // 
                                body = SelectGroupView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdFinish: {
                                // 
                                // Ask for an email address to notify when the list is complete
                                // 
                                body = FinishView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdDone: {
                                // 
                                // Thank you
                                // 
                                body = DoneView.getView(app);
                                return CP.Html.Form(body);
                            }
                        case constants.viewIdReturnBlank: {
                                // 
                                // -- 
                                return "";
                            }

                        default: {
                                // 
                                // -- data source
                                body = SelectSourceView.getView(app);
                                return CP.Html.Form(body);
                            }
                    }
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}