using System;

namespace Contensive.ImportWizard.Models {
    public class ImportConfigModel {
        public ImportDataModel_ImportTypeEnum importSource { get; set; }
        /// <summary>
        /// the uploaded data filename
        /// </summary>
        /// <returns></returns>
        public string privateUploadPathFilename { get; set; }
        // 
        public string importMapPathFilename { get; set; }
        // 
        public string notifyEmail { get; set; }
        /// <summary>
        /// contentid of the destination for the import
        /// </summary>
        /// <returns></returns>
        public int dstContentId { get; set; }
        // 
        /// <summary>
        /// create new empty import map. ContentName requested because it must be valid before map is valid.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="contentName"></param>
        public void newEmptyImportMap(ApplicationModel app, string contentName) {
            if (string.IsNullOrEmpty(contentName)) {
                // 
                // -- problem, cannot create map until content is established.
            }
            // Dim mapFilenameData As New MapFilenameDataModel With {
            // .mapName = "Import " & contentName,
            // .dateCreated = Now
            // }
            importMapPathFilename = ImportMapModel.createMapPathFilename(app, contentName, "Import " + contentName + ".txt");
            save(app);
        }
        // 
        /// <summary>
        /// create the current instance of the visits importdataobject
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static ImportConfigModel create(ApplicationModel app) {
            var result = app.cp.Visit.GetObject<ImportConfigModel>("ImportWizardData");
            // 
            // -- return data
            if (result is not null)
                return result;
            // 
            // -- setup default values
            result = new ImportConfigModel() {
                importSource = ImportDataModel_ImportTypeEnum.UploadFile,
                notifyEmail = app.cp.User.Email,
                dstContentId = 0
            };
            result.save(app);
            // 
            // -- setup a new import map
            // result.newEmptyImportMap(app)
            return result;
        }
        // 
        /// <summary>
        /// save this instance of the data
        /// </summary>
        /// <param name="app"></param>
        public void save(ApplicationModel app) {
            app.cp.Visit.SetProperty("ImportWizardData", app.cp.JSON.Serialize(this));
        }
        // 
        /// <summary>
        /// clear the data
        /// </summary>
        /// <param name="app"></param>
        public static void clear(ApplicationModel app) {
            app.cp.Visit.SetProperty("ImportWizardData", "");
        }
        // 
        // 
        public static int convertImportTypeToInt(ImportDataModel_ImportTypeEnum enumValue) {
            return Convert.ToInt32((int)enumValue);
        }
    }
    // 
    public enum ImportDataModel_ImportTypeEnum {
        UploadFile = 1,
        SelectFile = 2

    }
}