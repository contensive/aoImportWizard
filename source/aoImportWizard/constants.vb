
Imports Contensive.BaseClasses

Namespace Contensive.ImportWizard
    Public Module constants
        '
        ' views
        '
        Public Const viewIdSelectSource As Integer = 1
        Public Const viewIdUpload As Integer = 2
        Public Const viewIdSelectFile As Integer = 3
        Public Const viewIdNewMapping As Integer = 5
        Public Const viewIdSelectGroup As Integer = 6
        Public Const viewIdSelectKey As Integer = 7
        Public Const viewIdFinish As Integer = 8
        Public Const viewIdSelectTable As Integer = 9
        Public Const viewIdDone As Integer = 10
        ''' <summary>
        ''' return this view and the addon returns blank -- this is a signal to the calling program this this application is exiting and to handle the view
        ''' </summary>
        Public Const viewIdReturnBlank As Integer = 99
        '
        '
        '
        '
        Public Const rnSrcViewId = "SubForm"
        Public Const RequestNameImportWizardID = "ImportWizardID"
        Public Const RequestNameImportSource = "ImportWizardSource"
        Public Const RequestNameImportContentID = "ImportWizardDestination"
        Public Const RequestNameImportUpload = "ImportWizardUpload"
        Public Const RequestNameImportKeyMethodID = "ImportWizardKeyMethodID"
        Public Const RequestNameImportSourceKeyFieldPtr = "ImportWizardSourceKeyFieldPtr"
        Public Const RequestNameImportDbKeyField = "ImportWizardDbkeyField"
        Public Const RequestNameImportGroupID = "ImportGroupID"
        Public Const RequestNameImportGroupNew = "inputNewGroupName"
        Public Const RequestNameImportGroupOptionID = "ImportGroupOptionID"

        Public Const RequestNameImportEmail = "ImportEmailNotify"
        Public Const RequestNameImportMapFile = "ImportMapFile"
        '
        Public Const ButtonContinue2 = " Continue "
        Public Const ButtonBack2 = " Back "
        Public Const ButtonApply = "  Apply "
        Public Const ButtonLogin = "  Login  "
        Public Const ButtonLogout = "  Logout  "
        Public Const ButtonSendPassword = "  Send Password  "
        Public Const ButtonJoin = "   Join   "
        Public Const ButtonSave = "  Save  "
        Public Const ButtonOK = "     OK     "
        Public Const ButtonReset = "  Reset  "
        Public Const ButtonSaveAddNew = " Save + Add "
        'Public Const ButtonSaveAddNew = " Save > Add "
        Public Const ButtonCancel = " Cancel "
        Public Const ButtonRestartContensiveApplication = " Restart Contensive Application "
        Public Const ButtonCancelAll = "  Cancel  "
        Public Const ButtonFind = "   Find   "
        Public Const ButtonDelete = "  Delete  "
        Public Const ButtonDeletePerson = " Delete Person "
        Public Const ButtonDeleteRecord = " Delete Record "
        Public Const ButtonDeleteEmail = " Delete Email "
        Public Const ButtonDeletePage = " Delete Page "
        Public Const ButtonFileChange = "   Upload   "
        Public Const ButtonFileDelete = "    Delete    "
        Public Const ButtonClose = "  Close   "
        Public Const ButtonAdd = "   Add    "
        Public Const ButtonAddChildPage = " Add Child "
        Public Const ButtonAddSiblingPage = " Add Sibling "
        Public Const ButtonContinue = " Continue >> "
        Public Const ButtonBack = "  << Back  "
        Public Const ButtonNext = "   Next   "
        Public Const ButtonPrevious = " Previous "
        Public Const ButtonFirst = "  First   "
        Public Const ButtonSend = "  Send   "
        Public Const ButtonSendTest = "Send Test"
        Public Const ButtonCreateDuplicate = " Create Duplicate "
        Public Const ButtonActivate = "  Activate   "
        Public Const ButtonDeactivate = "  Deactivate   "
        Public Const ButtonOpenActiveEditor = "Active Edit"
        Public Const ButtonPublish = " Publish Changes "
        Public Const ButtonAbortEdit = " Abort Edits "
        Public Const ButtonPublishSubmit = " Submit for Publishing "
        Public Const ButtonPublishApprove = " Approve for Publishing "
        Public Const ButtonPublishDeny = " Deny for Publishing "
        Public Const ButtonWorkflowPublishApproved = " Publish Approved Records "
        Public Const ButtonWorkflowPublishSelected = " Publish Selected Records "
        Public Const ButtonSetHTMLEdit = " Edit WYSIWYG "
        Public Const ButtonSetTextEdit = " Edit HTML "
        Public Const ButtonRefresh = " Refresh "
        Public Const ButtonOrder = " Order "
        Public Const ButtonSearch = " Search "
        Public Const ButtonSpellCheck = " Spell Check "
        Public Const ButtonLibraryUpload = " Upload "
        Public Const ButtonCreateReport = " Create Report "
        Public Const ButtonClearTrapLog = " Clear Trap Log "
        Public Const ButtonNewSearch = " New Search "
        Public Const ButtonReloadCDef = " Reload Content Definitions "
        Public Const ButtonImportTemplates = " Import Templates "
        Public Const ButtonRSSRefresh = " Update RSS Feeds Now "
        Public Const ButtonRequestDownload = " Request Download "
        Public Const ButtonFinish = " Finish "
        Public Const ButtonRegister = " Register "
        Public Const ButtonBegin = "Begin"
        Public Const ButtonAbort = "Abort"
        Public Const ButtonCreateGUID = " Create GUID "
        Public Const ButtonEnable = " Enable "
        Public Const ButtonDisable = " Disable "
        Public Const ButtonMarkReviewed = " Mark Reviewed "
        Public Const RequestNameRunAddon = "addonid"
        Public Const RequestNameEditReferer = "EditReferer"
        Public Const RequestNameRefreshBlock = "ccFormRefreshBlockSN"
        Public Const RequestNameCatalogOrder = "CatalogOrderID"
        Public Const RequestNameCatalogCategoryID = "CatalogCatID"
        Public Const RequestNameCatalogForm = "CatalogFormID"
        Public Const RequestNameCatalogItemID = "CatalogItemID"
        Public Const RequestNameCatalogItemAge = "CatalogItemAge"
        Public Const RequestNameCatalogRecordTop = "CatalogTop"
        Public Const RequestNameCatalogFeatured = "CatalogFeatured"
        Public Const RequestNameCatalogSpan = "CatalogSpan"
        Public Const RequestNameCatalogKeywords = "CatalogKeywords"
        Public Const RequestNameCatalogSource = "CatalogSource"
        '
        Public Const RequestNameLibraryFileID = "fileEID"
        Public Const RequestNameDownloadID = "downloadid"
        Public Const RequestNameLibraryUpload = "LibraryUpload"
        Public Const RequestNameLibraryName = "LibraryName"
        Public Const RequestNameLibraryDescription = "LibraryDescription"

        Public Const RequestNameTestHook = "CC"       ' input request that sets debugging hooks

        Public Const RequestNameRootPage = "RootPageName"
        Public Const RequestNameRootPageID = "RootPageID"
        Public Const RequestNameContent = "ContentName"
        Public Const RequestNameOrderByClause = "OrderByClause"
        Public Const RequestNameAllowChildPageList = "AllowChildPageList"
        '
        Public Const RequestNameCRKey = "crkey"
        Public Const RequestNameAdminForm = "af"
        Public Const RequestNameAdminSubForm = "subform"
        Public Const RequestNameButton = "button"
        Public Const RequestNameAdminSourceForm = "asf"
        Public Const RequestNameAdminFormSpelling = "SpellingRequest"
        Public Const RequestNameInlineStyles = "InlineStyles"
        Public Const RequestNameAllowCSSReset = "AllowCSSReset"
        '
        Public Const RequestNameReportForm = "rid"
        '
        Public Const RequestNameToolContentID = "ContentID"
        '
        Public Const RequestNameCut = "a904o2pa0cut"
        Public Const RequestNamePaste = "dp29a7dsa6paste"
        Public Const RequestNamePasteParentContentID = "dp29a7dsa6cid"
        Public Const RequestNamePasteParentRecordID = "dp29a7dsa6rid"
        Public Const RequestNamePasteFieldList = "dp29a7dsa6key"
        Public Const RequestNameCutClear = "dp29a7dsa6clear"
        '
        Public Const RequestNameRequestBinary = "RequestBinary"
        ' removed -- this was an old method of blocking form input for file uploads
        'Public Const RequestNameFormBlock = "RB"
        Public Const RequestNameJSForm = "RequestJSForm"
        Public Const RequestNameJSProcess = "ProcessJSForm"
        '
        Public Const RequestNameFolderID = "FolderID"
        '
        Public Const RequestNameEmailMemberID = "emi8s9Kj"
        Public Const RequestNameEmailOpenFlag = "eof9as88"
        Public Const RequestNameEmailOpenCssFlag = "8aa41pM3"
        Public Const RequestNameEmailClickFlag = "ecf34Msi"
        Public Const RequestNameEmailSpamFlag = "9dq8Nh61"
        Public Const RequestNameEmailBlockRequestDropID = "BlockEmailRequest"
        Public Const RequestNameVisitTracking = "s9lD1088"
        Public Const RequestNameBlockContentTracking = "BlockContentTracking"
        Public Const RequestNameCookieDetectVisitID = "f92vo2a8d"

        Public Const RequestNamePageNumber = "PageNumber"
        Public Const RequestNamePageSize = "PageSize"
        '
        Public Const RequestValueNull = "[NULL]"
        '
        Public Const SpellCheckUserDictionaryFilename = "SpellCheck\UserDictionary.txt"
        '
        Public Const RequestNameStateString = "vstate"
        Public Const FieldTypeInteger = 1       ' An long number
        Public Const FieldTypeText = 2          ' A text field (up to 255 characters)
        Public Const FieldTypeLongText = 3      ' A memo field (up to 8000 characters)
        Public Const FieldTypeBoolean = 4       ' A yes/no field
        Public Const FieldTypeDate = 5          ' A date field
        Public Const FieldTypeFile = 6          ' A filename of a file in the files directory.
        Public Const FieldTypeLookup = 7        ' A lookup is a FieldTypeInteger that indexes into another table
        Public Const FieldTypeRedirect = 8      ' creates a link to another section
        Public Const FieldTypeCurrency = 9      ' A Float that prints in dollars
        Public Const FieldTypeTextFile = 10     ' Text saved in a file in the files area.
        Public Const FieldTypeImage = 11        ' A filename of a file in the files directory.
        Public Const FieldTypeFloat = 12        ' A float number
        Public Const FieldTypeAutoIncrement = 13 'long that automatically increments with the new record
        Public Const FieldTypeManyToMany = 14    ' no database field - sets up a relationship through a Rule table to another table
        Public Const FieldTypeMemberSelect = 15 ' This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        Public Const FieldTypeCSSFile = 16      ' A filename of a CSS compatible file
        Public Const FieldTypeXMLFile = 17      ' the filename of an XML compatible file
        Public Const FieldTypeJavascriptFile = 18 ' the filename of a javascript compatible file
        Public Const FieldTypeLink = 19           ' Links used in href tags -- can go to pages or resources
        Public Const FieldTypeResourceLink = 20   ' Links used in resources, link <img or <object. Should not be pages
        Public Const FieldTypeHTML = 21           ' LongText field that expects HTML content
        Public Const FieldTypeHTMLFile = 22       ' TextFile field that expects HTML content
        Public Const FieldTypeMax = 22
        '
        Public Const guidAddonImportTask As String = "{8EB631A1-C4D6-4538-A087-5033E5B6E7D9}"
        '
        Public Const viewIdMax As Integer = 10
        '
        Public Const ImportSourceUpload As Integer = 1
        Public Const ImportSourceUploadFolder As Integer = 2
        '
        Public Const KeyMethodInsertAll As Integer = 1
        Public Const KeyMethodUpdateOnMatch As Integer = 2
        Public Const KeyMethodUpdateOnMatchInsertOthers As Integer = 3
        '
        Public Const GroupOptionNone As Integer = 1
        Public Const GroupOptionAll As Integer = 2
        Public Const GroupOptionOnMatch As Integer = 3
        Public Const GroupOptionOnNoMatch As Integer = 4
        '
        ' ----- Buttons
        '
        '
        ' ----- local scope variables
        '
        '
        ' ----- Request Names
        '

        'Public Const RequestNameImportContentName  As String = "ImportContentName"
        Public Const RequestNameImportSkipFirstRow As String = "ImportSkipFirstRow"
        '
        ' -- errors for resultErrList
        '
        Public Enum ResultErrorEnum
            errPermission = 50
            errDuplicate = 100
            errVerification = 110
            errRestriction = 120
            errInput = 200
            errAuthentication = 300
            errAdd = 400
            errSave = 500
            errDelete = 600
            errLookup = 700
            errLoad = 710
            errContent = 800
            errMiscellaneous = 900
        End Enum
        '
        ' -- http errors
        Public Enum HttpErrorEnum
            badRequest = 400
            unauthorized = 401
            paymentRequired = 402
            forbidden = 403
            notFound = 404
            methodNotAllowed = 405
            notAcceptable = 406
            proxyAuthenticationRequired = 407
            requestTimeout = 408
            conflict = 409
            gone = 410
            lengthRequired = 411
            preconditionFailed = 412
            payloadTooLarge = 413
            urlTooLong = 414
            unsupportedMediaType = 415
            rangeNotSatisfiable = 416
            expectationFailed = 417
            teapot = 418
            methodFailure = 420
            enhanceYourCalm = 420
            misdirectedRequest = 421
            unprocessableEntity = 422
            locked = 423
            failedDependency = 424
            upgradeRequired = 426
            preconditionRequired = 428
            tooManyRequests = 429
            requestHeaderFieldsTooLarge = 431
            loginTimeout = 440
            noResponse = 444
            retryWith = 449
            redirect = 451
            unavailableForLegalReasons = 451
            sslCertificateError = 495
            sslCertificateRequired = 496
            httpRequestSentToSecurePort = 497
            invalidToken = 498
            clientClosedRequest = 499
            tokenRequired = 499
            internalServerError = 500
        End Enum
    End Module
    '
    Public Class WizardType
        '
        ' Attributes
        '
        'SendMethodID as Integer
        '
        ' Form 'includes'
        '
        'IncludeTemplateForm As Boolean
        '
        ' Value Defaults
        '
        'DefaultTemplateID as Integer
        '
        ' Instructions
        '
        Public SourceFormInstructions As String
        Public UploadFormInstructions As String
        Public MappingFormInstructions As String
        Public GroupFormInstructions As String
        Public KeyFormInstructions As String
        '

        ' Current calculated Path
        '
        Public Path() As Integer
        Public PathCnt As Integer
    End Class
    '
End Namespace