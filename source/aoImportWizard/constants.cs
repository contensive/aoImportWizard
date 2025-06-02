
namespace Contensive.ImportWizard {
    public static class constants {
        // 
        // Upload paths ('path' always ha a trailing slash and no leading slash)
        public const string privateFilesUploadPath = @"importwizard\uploads\";
        /// <summary>
        /// maps are stored in this path, in a folder with the word 'user' + user's id
        /// </summary>
        public const string privateFilesMapFolder = @"importwizard\maps\";
        // 
        // views
        // 
        public const int viewIdSelectSource = 1;
        public const int viewIdUpload = 2;
        public const int viewIdSelectFile = 3;
        public const int viewIdSelectContent = 9;
        public const int viewIdSelectMap = 4;
        public const int viewIdMapping = 5;
        public const int viewIdSelectGroup = 6;
        public const int viewIdSelectKey = 7;
        public const int viewIdFinish = 8;
        public const int viewIdDone = 10;
        /// <summary>
        /// return this view and the addon returns blank -- this is a signal to the calling program this this application is exiting and to handle the view
        /// </summary>
        public const int viewIdReturnBlank = 99;
        // 
        // 
        // 
        // 
        public const string rnRowCount = "rowCount";
        public const string rnSrcViewId = "SubForm";
        public const string RequestNameImportWizardID = "ImportWizardID";
        public const string RequestNameImportSource = "ImportWizardSource";
        public const string RequestNameImportContentID = "ImportWizardDestination";
        public const string RequestNameImportUpload = "ImportWizardUpload";
        public const string RequestNameImportKeyMethodID = "ImportWizardKeyMethodID";
        public const string RequestNameImportSourceKeyFieldPtr = "ImportWizardSourceKeyFieldPtr";
        public const string RequestNameImportDbKeyField = "ImportWizardDbkeyField";
        public const string RequestNameImportGroupID = "ImportGroupID";
        public const string RequestNameImportGroupNew = "inputNewGroupName";
        public const string RequestNameImportGroupOptionID = "ImportGroupOptionID";

        public const string RequestNameImportEmail = "ImportEmailNotify";
        public const string RequestNameImportMapFile = "ImportMapFile";
        // 
        public const string ButtonContinue = " Continue ";
        public const string ButtonBack = " Back ";
        public const string ButtonApply = "  Apply ";
        public const string ButtonLogin = "  Login  ";
        public const string ButtonLogout = "  Logout  ";
        public const string ButtonSendPassword = "  Send Password  ";
        public const string ButtonJoin = "   Join   ";
        public const string ButtonSave = "  Save  ";
        public const string ButtonOK = "     OK     ";
        public const string ButtonReset = "  Reset  ";
        public const string ButtonSaveAddNew = " Save + Add ";
        // Public Const ButtonSaveAddNew = " Save > Add "
        public const string ButtonCancel = " Cancel ";
        public const string ButtonRestartContensiveApplication = " Restart Contensive Application ";
        public const string ButtonCancelAll = "  Cancel  ";
        public const string ButtonFind = "   Find   ";
        public const string ButtonDelete = "  Delete  ";
        public const string ButtonDeletePerson = " Delete Person ";
        public const string ButtonDeleteRecord = " Delete Record ";
        public const string ButtonDeleteEmail = " Delete Email ";
        public const string ButtonDeletePage = " Delete Page ";
        public const string ButtonFileChange = "   Upload   ";
        public const string ButtonFileDelete = "    Delete    ";
        public const string ButtonClose = "  Close   ";
        public const string ButtonAdd = "   Add    ";
        public const string ButtonAddChildPage = " Add Child ";
        public const string ButtonAddSiblingPage = " Add Sibling ";
        public const string ButtonNext = "   Next   ";
        public const string ButtonPrevious = " Previous ";
        public const string ButtonFirst = "  First   ";
        public const string ButtonSend = "  Send   ";
        public const string ButtonSendTest = "Send Test";
        public const string ButtonCreateDuplicate = " Create Duplicate ";
        public const string ButtonActivate = "  Activate   ";
        public const string ButtonDeactivate = "  Deactivate   ";
        public const string ButtonOpenActiveEditor = "Active Edit";
        public const string ButtonPublish = " Publish Changes ";
        public const string ButtonAbortEdit = " Abort Edits ";
        public const string ButtonPublishSubmit = " Submit for Publishing ";
        public const string ButtonPublishApprove = " Approve for Publishing ";
        public const string ButtonPublishDeny = " Deny for Publishing ";
        public const string ButtonWorkflowPublishApproved = " Publish Approved Records ";
        public const string ButtonWorkflowPublishSelected = " Publish Selected Records ";
        public const string ButtonSetHTMLEdit = " Edit WYSIWYG ";
        public const string ButtonSetTextEdit = " Edit HTML ";
        public const string ButtonRefresh = " Refresh ";
        public const string ButtonOrder = " Order ";
        public const string ButtonSearch = " Search ";
        public const string ButtonSpellCheck = " Spell Check ";
        public const string ButtonLibraryUpload = " Upload ";
        public const string ButtonCreateReport = " Create Report ";
        public const string ButtonClearTrapLog = " Clear Trap Log ";
        public const string ButtonNewSearch = " New Search ";
        public const string ButtonReloadCDef = " Reload Content Definitions ";
        public const string ButtonImportTemplates = " Import Templates ";
        public const string ButtonRSSRefresh = " Update RSS Feeds Now ";
        public const string ButtonRequestDownload = " Request Download ";
        public const string ButtonFinish = " Finish ";
        public const string ButtonRegister = " Register ";
        public const string ButtonBegin = "Begin";
        public const string ButtonAbort = "Abort";
        public const string ButtonCreateGUID = " Create GUID ";
        public const string ButtonEnable = " Enable ";
        public const string ButtonDisable = " Disable ";
        public const string ButtonMarkReviewed = " Mark Reviewed ";
        public const string ButtonRestart = "Restart";
        // 
        public const string RequestNameRunAddon = "addonid";
        public const string RequestNameEditReferer = "EditReferer";
        public const string RequestNameRefreshBlock = "ccFormRefreshBlockSN";
        public const string RequestNameCatalogOrder = "CatalogOrderID";
        public const string RequestNameCatalogCategoryID = "CatalogCatID";
        public const string RequestNameCatalogForm = "CatalogFormID";
        public const string RequestNameCatalogItemID = "CatalogItemID";
        public const string RequestNameCatalogItemAge = "CatalogItemAge";
        public const string RequestNameCatalogRecordTop = "CatalogTop";
        public const string RequestNameCatalogFeatured = "CatalogFeatured";
        public const string RequestNameCatalogSpan = "CatalogSpan";
        public const string RequestNameCatalogKeywords = "CatalogKeywords";
        public const string RequestNameCatalogSource = "CatalogSource";
        // 
        public const string RequestNameLibraryFileID = "fileEID";
        public const string RequestNameDownloadID = "downloadid";
        public const string RequestNameLibraryUpload = "LibraryUpload";
        public const string RequestNameLibraryName = "LibraryName";
        public const string RequestNameLibraryDescription = "LibraryDescription";

        public const string RequestNameTestHook = "CC";       // input request that sets debugging hooks

        public const string RequestNameRootPage = "RootPageName";
        public const string RequestNameRootPageID = "RootPageID";
        public const string RequestNameContent = "ContentName";
        public const string RequestNameOrderByClause = "OrderByClause";
        public const string RequestNameAllowChildPageList = "AllowChildPageList";
        // 
        public const string RequestNameCRKey = "crkey";
        public const string RequestNameAdminForm = "af";
        public const string RequestNameAdminSubForm = "subform";
        public const string RequestNameButton = "button";
        public const string RequestNameAdminSourceForm = "asf";
        public const string RequestNameAdminFormSpelling = "SpellingRequest";
        public const string RequestNameInlineStyles = "InlineStyles";
        public const string RequestNameAllowCSSReset = "AllowCSSReset";
        // 
        public const string RequestNameReportForm = "rid";
        // 
        public const string RequestNameToolContentID = "ContentID";
        // 
        public const string RequestNameCut = "a904o2pa0cut";
        public const string RequestNamePaste = "dp29a7dsa6paste";
        public const string RequestNamePasteParentContentID = "dp29a7dsa6cid";
        public const string RequestNamePasteParentRecordID = "dp29a7dsa6rid";
        public const string RequestNamePasteFieldList = "dp29a7dsa6key";
        public const string RequestNameCutClear = "dp29a7dsa6clear";
        // 
        public const string RequestNameRequestBinary = "RequestBinary";
        // removed -- this was an old method of blocking form input for file uploads
        // Public Const RequestNameFormBlock = "RB"
        public const string RequestNameJSForm = "RequestJSForm";
        public const string RequestNameJSProcess = "ProcessJSForm";
        // 
        public const string RequestNameFolderID = "FolderID";
        // 
        public const string RequestNameEmailMemberID = "emi8s9Kj";
        public const string RequestNameEmailOpenFlag = "eof9as88";
        public const string RequestNameEmailOpenCssFlag = "8aa41pM3";
        public const string RequestNameEmailClickFlag = "ecf34Msi";
        public const string RequestNameEmailSpamFlag = "9dq8Nh61";
        public const string RequestNameEmailBlockRequestDropID = "BlockEmailRequest";
        public const string RequestNameVisitTracking = "s9lD1088";
        public const string RequestNameBlockContentTracking = "BlockContentTracking";
        public const string RequestNameCookieDetectVisitID = "f92vo2a8d";

        public const string RequestNamePageNumber = "PageNumber";
        public const string RequestNamePageSize = "PageSize";
        // 
        public const string RequestValueNull = "[NULL]";
        // 
        public const string SpellCheckUserDictionaryFilename = @"SpellCheck\UserDictionary.txt";
        // 
        public const string RequestNameStateString = "vstate";
        public const int FieldTypeInteger = 1;       // An long number
        public const int FieldTypeText = 2;          // A text field (up to 255 characters)
        public const int FieldTypeLongText = 3;      // A memo field (up to 8000 characters)
        public const int FieldTypeBoolean = 4;       // A yes/no field
        public const int FieldTypeDate = 5;          // A date field
        public const int FieldTypeFile = 6;          // A filename of a file in the files directory.
        public const int FieldTypeLookup = 7;        // A lookup is a FieldTypeInteger that indexes into another table
        public const int FieldTypeRedirect = 8;      // creates a link to another section
        public const int FieldTypeCurrency = 9;      // A Float that prints in dollars
        public const int FieldTypeTextFile = 10;     // Text saved in a file in the files area.
        public const int FieldTypeImage = 11;        // A filename of a file in the files directory.
        public const int FieldTypeFloat = 12;        // A float number
        public const int FieldTypeAutoIncrement = 13; // long that automatically increments with the new record
        public const int FieldTypeManyToMany = 14;    // no database field - sets up a relationship through a Rule table to another table
        public const int FieldTypeMemberSelect = 15; // This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        public const int FieldTypeCSSFile = 16;      // A filename of a CSS compatible file
        public const int FieldTypeXMLFile = 17;      // the filename of an XML compatible file
        public const int FieldTypeJavascriptFile = 18; // the filename of a javascript compatible file
        public const int FieldTypeLink = 19;           // Links used in href tags -- can go to pages or resources
        public const int FieldTypeResourceLink = 20;   // Links used in resources, link <img or <object. Should not be pages
        public const int FieldTypeHTML = 21;           // LongText field that expects HTML content
        public const int FieldTypeHTMLFile = 22;       // TextFile field that expects HTML content
        public const int FieldTypeMax = 22;
        // 
        public const string guidAddonImportTask = "{8EB631A1-C4D6-4538-A087-5033E5B6E7D9}";
        // 
        public const int viewIdMax = 10;
        // 
        public const int ImportSourceUpload = 1;
        public const int ImportSourceUploadFolder = 2;
        // 
        // 
        public const int GroupOptionNone = 1;
        public const int GroupOptionAll = 2;
        public const int GroupOptionOnMatch = 3;
        public const int GroupOptionOnNoMatch = 4;
        // 
        // ----- Buttons
        // 
        // 
        // ----- local scope variables
        // 
        // 
        // ----- Request Names
        // 

        // Public Const RequestNameImportContentName  As String = "ImportContentName"
        public const string RequestNameImportSkipFirstRow = "ImportSkipFirstRow";
        public const string requestNameImportMapName = "importMapName";
        // 
        // -- errors for resultErrList
        // 
        public enum ResultErrorEnum {
            errPermission = 50,
            errDuplicate = 100,
            errVerification = 110,
            errRestriction = 120,
            errInput = 200,
            errAuthentication = 300,
            errAdd = 400,
            errSave = 500,
            errDelete = 600,
            errLookup = 700,
            errLoad = 710,
            errContent = 800,
            errMiscellaneous = 900
        }
        // 
        // -- http errors
        public enum HttpErrorEnum {
            badRequest = 400,
            unauthorized = 401,
            paymentRequired = 402,
            forbidden = 403,
            notFound = 404,
            methodNotAllowed = 405,
            notAcceptable = 406,
            proxyAuthenticationRequired = 407,
            requestTimeout = 408,
            conflict = 409,
            gone = 410,
            lengthRequired = 411,
            preconditionFailed = 412,
            payloadTooLarge = 413,
            urlTooLong = 414,
            unsupportedMediaType = 415,
            rangeNotSatisfiable = 416,
            expectationFailed = 417,
            teapot = 418,
            methodFailure = 420,
            enhanceYourCalm = 420,
            misdirectedRequest = 421,
            unprocessableEntity = 422,
            locked = 423,
            failedDependency = 424,
            upgradeRequired = 426,
            preconditionRequired = 428,
            tooManyRequests = 429,
            requestHeaderFieldsTooLarge = 431,
            loginTimeout = 440,
            noResponse = 444,
            retryWith = 449,
            redirect = 451,
            unavailableForLegalReasons = 451,
            sslCertificateError = 495,
            sslCertificateRequired = 496,
            httpRequestSentToSecurePort = 497,
            invalidToken = 498,
            clientClosedRequest = 499,
            tokenRequired = 499,
            internalServerError = 500
        }
    }
    // 
    public class WizardType {
        // 
        // Attributes
        // 
        // SendMethodID as Integer
        // 
        // Form 'includes'
        // 
        // IncludeTemplateForm As Boolean
        // 
        // Value Defaults
        // 
        // DefaultTemplateID as Integer
        // 
        // Instructions
        // 
        public string SourceFormInstructions;
        public string UploadFormInstructions;
        public string MappingFormInstructions;
        public string GroupFormInstructions;
        public string KeyFormInstructions;
        // 

        // Current calculated Path
        // 
        public int[] Path;
        public int PathCnt;
    }
    // 
}