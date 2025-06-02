using System;
using System.Collections.Generic;
using System.Data;
using Contensive.BaseClasses;

namespace Contensive.ImportWizard.Models {
    public class ContentFieldModel : Contensive.Models.Db.ContentFieldModel {
        // 
        public static int getFieldType(CPBaseClass cp, string dbFieldName, int contentId) {
            try {
                using (var dt = cp.Db.ExecuteQuery("select top 1 type from ccfields where (active>0)and(name=" + cp.Db.EncodeSQLText(dbFieldName) + ")And(contentid=" + contentId + ")")) {
                    if (dt?.Rows is not null && dt.Rows.Count > 0)
                        return cp.Utils.EncodeInteger(dt.Rows[0][0]);
                }
                return 0;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Get the database field list for this content
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="contentName"></param>
        /// <param name="allowID"></param>
        /// <returns></returns>
        public static List<ContentFieldData> getDbFieldList(CPBaseClass cp, string contentName, bool allowID, bool orderByNameCaption) {
            try {
                var result = new List<ContentFieldData>();
                var dt = cp.Db.ExecuteQuery("select name,caption from ccfields where (active>0)and(contentid=" + cp.Content.GetID(contentName) + ") order by " + (orderByNameCaption ? "name" : "caption"));
                if (dt?.Rows is null || dt.Rows.Count == 0)
                    return result;
                foreach (DataRow row in dt.Rows) {
                    string fieldName = cp.Utils.EncodeText(row["name"]);
                    if (!allowID & fieldName == "id")
                        continue;
                    result.Add(new ContentFieldData() {
                        caption = cp.Utils.EncodeText(row["caption"]),
                        name = fieldName
                    });
                }
                return result;
                // Dim result As String = "," & cp.Content.GetProperty(ContentName, "SELECTFIELDLIST") & ","
                // If Not AllowID Then
                // result = Replace(result, ",ID,", ",", , , vbTextCompare)
                // End If
                // result = Replace(result, ",CONTENTCONTROLID,", ",", , , vbTextCompare)
                // result = Replace(result, ",EDITSOURCEID,", ",", , , vbTextCompare)
                // result = Replace(result, ",EDITBLANK,", ",", , , vbTextCompare)
                // result = Replace(result, ",EDITARCHIVE,", ",", , , vbTextCompare)
                // result = Replace(result, ",DEVELOPER,", ",", , , vbTextCompare)
                // result = Mid(result, 2, Len(result) - 2)
                // '
                // Return result
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
    // 
    public class ContentFieldData {
        public string name { get; set; }
        public string caption { get; set; }
    }
}