using System;
using System.Linq;
using Contensive.ImportWizard.Models;
using Contensive.Models.Db;

namespace Contensive.ImportWizard.Controllers {
    public class SelectGroupView {
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

                var newGroupID = default(int);
                string newGroupName = cp.Doc.GetText(constants.RequestNameImportGroupNew);
                if (!string.IsNullOrEmpty(newGroupName)) {
                    var groupmodellist = DbBaseModel.createList<GroupModel>(cp, "(name=" + cp.Db.EncodeSQLText(newGroupName) + ")");
                    if (groupmodellist.Count != 0) {
                        var newGroup = groupmodellist.First();
                        newGroupID = newGroup.id;
                    }
                    if (newGroupID == 0) {
                        var newGroup = DbBaseModel.addDefault<GroupModel>(cp);
                        newGroup.name = newGroupName;
                        newGroup.caption = newGroupName;
                        newGroupID = newGroup.id;
                        newGroup.save(cp);
                    }

                }
                var importConfig = ImportConfigModel.create(app);
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                if (newGroupID != 0) {
                    ImportMap.groupID = newGroupID;
                } else {
                    ImportMap.groupID = cp.Doc.GetInteger(constants.RequestNameImportGroupID);
                }
                ImportMap.groupOptionID = cp.Doc.GetInteger(constants.RequestNameImportGroupOptionID);
                ImportMap.save(app, importConfig);
                // 
                switch (Button ?? "") {
                    case constants.ButtonBack: {
                            // 
                            // -- back to select key
                            return constants.viewIdSelectKey;
                        }

                    default: {
                            // 
                            // -- continue to finish
                            return constants.viewIdFinish;
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
                var ImportMap = ImportMapModel.create(cp, importConfig.importMapPathFilename);
                // 
                int GroupOptionID = ImportMap.groupOptionID;
                if (GroupOptionID == 0) {
                    GroupOptionID = constants.GroupOptionNone;
                }
                description = cp.Html.h4("Group Membership") + cp.Html.p("When your data is imported, people can be added to a group automatically. Select the option below, and a group.");
                content = "" + "<div>" + "<TABLE border=0 cellpadding=4 cellspacing=0 width=100%>" + "<TR><TD colspan=2>Group Options</td></tr>" + "<TR><TD width=10>" + cp.Html.RadioBox(constants.RequestNameImportGroupOptionID, constants.GroupOptionNone.ToString(), GroupOptionID.ToString()) + "</td><td width=99% align=left>Do not add to a group.</td></tr>" + "<TR><TD width=10>" + cp.Html.RadioBox(constants.RequestNameImportGroupOptionID, constants.GroupOptionAll.ToString(), GroupOptionID.ToString()) + "</td><td width=99% align=left>Add everyone to the the group.</td></tr>" + "<TR><TD width=10>" + cp.Html.RadioBox(constants.RequestNameImportGroupOptionID, constants.GroupOptionOnMatch.ToString(), GroupOptionID.ToString()) + "</td><td width=99% align=left>Add to the group if keys match.</td></tr>" + "<TR><TD width=10>" + cp.Html.RadioBox(constants.RequestNameImportGroupOptionID, constants.GroupOptionOnNoMatch.ToString(), GroupOptionID.ToString()) + "</td><td width=99% align=left>Add to the group if keys do NOT match.</td></tr>" + "<TR><TD colspan=2>Add to Existing Group</td></tr>" + "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" + cp.Html.SelectContent(constants.RequestNameImportGroupID, ImportMap.groupID.ToString(), "Groups", "", "", "form-control") + "</td></tr>" + "<TR><TD colspan=2>Create New Group</td></tr>" + "<TR><TD width=10>&nbsp;</td><td width=99% align=left>" + cp.Html.InputText(constants.RequestNameImportGroupNew, "", 100, "form-control") + "</td></tr>" + "</table>" + "</div>" + "";













                content += cp.Html.Hidden(constants.rnSrcViewId, constants.viewIdSelectGroup.ToString());
                return HtmlController.createLayout(cp, headerCaption, description, content, true, true, true, true);
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}