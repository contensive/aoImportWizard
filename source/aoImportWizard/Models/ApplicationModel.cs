using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.ImportWizard.Controllers;
using Microsoft.VisualBasic;

namespace Contensive.ImportWizard.Models {
    // 
    // ====================================================================================================
    /// <summary>
    /// Use for access to global non-shared methods and aside caching
    /// </summary>
    /// <remarks></remarks>
    public class ApplicationModel : IDisposable {
        // 
        // privates passed in, do not dispose
        // 
        public readonly CPBaseClass cp;
        // 
        public int peopleContentid {
            get {
                if (peopleContentid_Local is not null)
                    return (int)peopleContentid_Local;
                peopleContentid_Local = cp.Content.GetID("people");
                return (int)peopleContentid_Local;
            }
        }
        private int? peopleContentid_Local = default;
        // 
        // Public ReadOnly Property wizard As New WizardType
        public int sourceFieldCnt { get; set; }
        public string[] uploadFields { get; set; }
        // 
        // ====================================================================================================
        /// <summary>
        /// Errors accumulated during rendering.
        /// </summary>
        /// <returns></returns>
        public List<PackageErrorClass> packageErrorList { get; set; } = new List<PackageErrorClass>();
        // 
        // ====================================================================================================
        /// <summary>
        /// data accumulated during rendering
        /// </summary>
        /// <returns></returns>
        public List<PackageNodeClass> packageNodeList { get; set; } = new List<PackageNodeClass>();
        // 
        // ====================================================================================================
        /// <summary>
        /// list of name/time used to performance analysis
        /// </summary>
        /// <returns></returns>
        public List<PackageProfileClass> packageProfileList { get; set; } = new List<PackageProfileClass>();
        // 
        // ====================================================================================================
        /// <summary>
        /// get the serialized results
        /// </summary>
        /// <returns></returns>
        public string getSerializedPackage() {
            try {
                string result = serializeObject(cp, new PackageClass() {
                    success = packageErrorList.Count.Equals(0),
                    nodeList = packageNodeList,
                    errorList = packageErrorList,
                    profileList = packageProfileList
                });
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public ApplicationModel(CPBaseClass cp, bool requiresAuthentication = true) {
            this.cp = cp;
            if (requiresAuthentication & !cp.User.IsAuthenticated) {
                packageErrorList.Add(new PackageErrorClass() { number = (int)constants.ResultErrorEnum.errAuthentication, description = "Authorization is required." });
                cp.Response.SetStatus(((int)constants.HttpErrorEnum.forbidden).ToString() + " Forbidden");
            }
        }
        // 
        public static string serializeObject(CPBaseClass CP, object dataObject) {
            try {
                string result = "";
                var json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                result = json_serializer.Serialize(dataObject);
                return result;
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }


        // 
        // ====================================================================================================
        /// <summary>
        /// Load the sourceField and sourceFieldCnt from a wizard file
        /// </summary>
        /// <param name="Filename"></param>
        public void loadUploadFields(string Filename) {
            try {
                string FileData;
                var ignoreLong = default(int);
                var ignoreBoolean = default(bool);
                bool foundFirstName = false;
                bool foundLastName = false;
                bool foundName = false;
                // 
                if (!string.IsNullOrEmpty(Filename)) {
                    if (sourceFieldCnt == 0) {
                        FileData = cp.PrivateFiles.Read(Filename);
                        if (!string.IsNullOrEmpty(FileData)) {
                            // 
                            // Build FileColumns
                            // 
                            var argreturn_cells = uploadFields;
                            GenericController.parseLine(FileData, 1, ref argreturn_cells, ref ignoreLong, ref ignoreBoolean);
                            uploadFields = argreturn_cells;
                            // 
                            // todo - implement new fields to allow name/firstname/lastname population
                            // For Each field As String In sourceFields
                            // foundFirstName = foundFirstName Or field.ToLowerInvariant().Equals("firstname") Or field.ToLowerInvariant().Equals("first name")
                            // foundLastName = foundLastName Or field.ToLowerInvariant().Equals("lastname") Or field.ToLowerInvariant().Equals("last name")
                            // foundName = foundName Or field.ToLowerInvariant().Equals("name")
                            // Next
                            // If (foundName And Not foundFirstName) Then
                            // '
                            // ' -- add firstname and lastname from name
                            // sourceFields.Append("Name-first-half]")
                            // End If
                            // If (foundName And Not foundLastName) Then
                            // '
                            // ' -- add firstname and lastname from name
                            // sourceFields.Append("Name-last-half")
                            // End If
                            // If (Not foundName And foundFirstName And foundLastName) Then
                            // '
                            // ' -- add firstname and lastname from name
                            // sourceFields.Append("First-Name Last-Name")
                            // End If
                            sourceFieldCnt = Information.UBound(uploadFields) + 1;
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }




        #region  IDisposable Support 
        protected bool disposed = false;
        // 
        // ==========================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    // 
                    // ----- call .dispose for managed objects
                    // 
                }
                // 
                // Add code here to release the unmanaged resource.
                // 
            }
            disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ApplicationModel() {
            Dispose(false);
        }
        #endregion
    }
    // 
    // ====================================================================================================
    /// <summary>
    /// list of events and their stopwatch times
    /// </summary>
    public class PackageProfileClass {
        public string name { get; set; }
        public long time { get; set; }
    }
    // 
    // ====================================================================================================
    /// <summary>
    /// remote method top level data structure
    /// </summary>
    [Serializable()]
    public class PackageClass {
        public bool success { get; set; } = false;
        public List<PackageErrorClass> errorList { get; set; } = new List<PackageErrorClass>();
        public List<PackageNodeClass> nodeList { get; set; } = new List<PackageNodeClass>();
        public List<PackageProfileClass> profileList { get; set; }
    }
    // 
    // ====================================================================================================
    /// <summary>
    /// data store for jsonPackage
    /// </summary>
    [Serializable()]
    public class PackageNodeClass {
        public string dataFor { get; set; } = "";
        public object data { get; set; } // IEnumerable(Of Object)
    }
    // 
    // ====================================================================================================
    /// <summary>
    /// error list for jsonPackage
    /// </summary>
    [Serializable()]
    public class PackageErrorClass {
        public int number { get; set; } = 0;
        public string description { get; set; } = "";
    }
    // 
}