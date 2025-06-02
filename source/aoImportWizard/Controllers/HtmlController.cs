using System;
using System.Text;
using Contensive.BaseClasses;
using Contensive.ImportWizard.Models;

namespace Contensive.ImportWizard.Controllers {
    public class HtmlController {

        // 
        // ====================================================================================================
        /// <summary>
        /// get an html select with all the fields from the uploaded source data
        /// </summary>
        /// <param name="app"></param>
        /// <param name="filename"></param>
        /// <param name="noneCaption"></param>
        /// <returns></returns>
        public static string getSourceFieldSelect(ApplicationModel app, string filename, string noneCaption, int dstContentId, bool allowControlFields, int selectedValue, string inputName, string htmlId) {
            try {
                var cp = app.cp;
                if (string.IsNullOrEmpty(filename))
                    return string.Empty;
                app.loadUploadFields(filename);
                if (app.sourceFieldCnt.Equals(0))
                    return string.Empty;
                // 
                // Build FileColumns
                // 
                string result = "";
                result = "<select name=\"" + cp.Utils.EncodeHTML(inputName) + "\" class=\"form-control js-import-select\" id=\"" + htmlId + "\">";
                result += "<option value=\"-1\">" + noneCaption + "</option>";
                for (int ptr = 0, loopTo = app.sourceFieldCnt - 1; ptr <= loopTo; ptr++)
                    result += "<option value=\"" + ptr + "\"" + (ptr == selectedValue ? " selected" : "") + ">column " + (ptr + 1) + " (" + (string.IsNullOrEmpty(app.uploadFields[ptr]) ? "[blank]" : app.uploadFields[ptr]) + ")</option>";
                if (allowControlFields) {
                    result += "<option value=\"-2\"" + (-2 == selectedValue ? " selected" : "") + ">Set value manually</option>";
                    if (dstContentId == app.peopleContentid)
                        result += "<option value=\"-3\"" + (-3 == selectedValue ? " selected" : "") + ">Set to firstname + lastname</option>";
                    if (dstContentId == app.peopleContentid)
                        result += "<option value=\"-4\"" + (-4 == selectedValue ? " selected" : "") + ">Set to firstname from name field</option>";
                    if (dstContentId == app.peopleContentid)
                        result += "<option value=\"-5\"" + (-5 == selectedValue ? " selected" : "") + ">Set to lastname from name field</option>";
                }
                result += "</select>";
                return result;
            } catch (Exception ex) {
                app.cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        public static string getRadio(CPBaseClass cp, string radioName, int radioValue, int selectedValue, string radioLabel, string htmlId) {
            string uniqueId = "id" + cp.Utils.GetRandomInteger();
            string result = "<label class=\"form-check-label\" for=\"" + uniqueId + "\">" + radioLabel + "</label>";
            result = "<input class=\"form-check-input\" type=\"radio\" name=\"" + radioName + "\" id=\"" + uniqueId + "\" value=\"" + radioValue + "\" " + (selectedValue == radioValue ? " checked" : "") + ">" + result;
            result = "<div class=\"ml-4 form-check\" id=\"" + htmlId + "\">" + result + "</div>";
            return result;
        }
        /// <summary>
        /// Create a layout for the import wizard
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="body"></param>
        /// <param name="allowCancel"></param>
        /// <param name="allowRestart"></param>
        /// <param name="allowBack"></param>
        /// <param name="allowContinue"></param>
        /// <returns></returns>
        public static string createLayout(CPBaseClass cp, string header, string description, string body, bool allowCancel, bool allowRestart, bool allowBack, bool allowContinue) {
            try {
                var layout = cp.AdminUI.CreateLayoutBuilder();
                layout.title = header;
                layout.description = description;
                layout.body = body;
                layout.includeBodyPadding = true;
                layout.includeBodyColor = false;
                layout.isOuterContainer = true;

                if (allowCancel)
                    layout.addFormButton(constants.ButtonCancel);
                if (allowRestart)
                    layout.addFormButton(constants.ButtonRestart);
                if (allowBack)
                    layout.addFormButton(constants.ButtonBack);
                if (allowContinue)
                    layout.addFormButton(constants.ButtonContinue);
                return layout.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Get an html select with teh current content's fields
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ContentName"></param>
        /// <param name="NoneCaption"></param>
        /// <param name="AllowID"></param>
        /// <returns></returns>
        public static string getDbFieldSelect(CPBaseClass cp, string ContentName, string NoneCaption, bool AllowID, string currentValue) {
            try {
                var options = new StringBuilder("<option value=\"\"" + (string.IsNullOrEmpty(currentValue) ? " selected" : "") + ">" + cp.Utils.EncodeHTML(NoneCaption) + "</option>");
                foreach (var row in ContentFieldModel.getDbFieldList(cp, ContentName, AllowID, false))
                    options.AppendLine("<option value=\"" + cp.Utils.EncodeHTML(row.name) + "\"" + ((row.name ?? "") == (currentValue ?? "") ? " selected" : "") + ">" + cp.Utils.EncodeHTML(row.caption) + "</option>");
                return "<select class=\"form-control\" name=xxxx><option value=\"\" style=\"Background-color:#E0E0E0;\">" + options.ToString() + "</option>";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}