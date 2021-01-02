Imports Contensive.BaseClasses
Public Module constants
    '
    Public Const RequestNameSubForm = "SubForm"
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
    Public Const adminCommonAddonGuid = "{76E7F79E-489F-4B0F-8EE5-0BAC3E4CD782}"
    Public Const DashboardAddonGuid = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}"
    Public Const PersonalizationGuid = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}"
    Public Const TextBoxGuid = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}"
    Public Const ContentBoxGuid = "{E341695F-C444-4E10-9295-9BEEC41874D8}"
    Public Const DynamicMenuGuid = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}"
    Public Const ChildListGuid = "{D291F133-AB50-4640-9A9A-18DB68FF363B}"
    Public Const DynamicFormGuid = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}"
    Public Const AddonManagerGuid = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}"
    Public Const FormWizardGuid = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}"
    Public Const ImportWizardGuid = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}"
    Public Const JQueryGuid = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}"
    Public Const JQueryUIGuid = "{840B9AEF-9470-4599-BD47-7EC0C9298614}"
    Public Const ImportProcessAddonGuid = "{5254FAC6-A7A6-4199-8599-0777CC014A13}"
    Public Const StructuredDataProcessorGuid = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}"
    Public Const jQueryFancyBoxGuid = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}"
    '
    ' forms
    '
    Public Const SubFormSource As Integer = 1
    Public Const SubFormSourceUpload As Integer = 2
    Public Const SubFormSourceUploadFolder As Integer = 3
    Public Const SubFormSourceResourceLibrary As Integer = 4
    Public Const SubFormNewMapping As Integer = 5
    Public Const SubFormGroup As Integer = 6
    Public Const SubFormKey As Integer = 7
    Public Const SubFormFinish As Integer = 8
    Public Const SubFormDestination As Integer = 9
    Public Const SubFormDone As Integer = 10
    '
    Public Const SubFormMax As Integer = 10
    '
    Public Const ImportSourceUpload As Integer = 1
    Public Const ImportSourceUploadFolder As Integer = 2
    Public Const ImportSourceResourceLibrary As Integer = 3
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
    'Load Import Map
    '
    Public Function loadImportMap(cp As CPBaseClass, ImportMapData As String) As ImportMapType
        Try
            Dim result As ImportMapType = New ImportMapType
            Dim Rows() As String
            Dim Pair() As String
            Dim Ptr As Long
            Dim SourceSplit() As String
            Dim MapPtr As Integer
            '
            If String.IsNullOrEmpty(ImportMapData) Then
                '
                ' Defaults
                '
                result.ContentName = "People"
                result.GroupID = 0
                result.MapPairCnt = 0
                result.SkipRowCnt = 1
            Else
                '
                ' read in what must be saved
                '
                Rows = Split(ImportMapData, vbCrLf)
                If UBound(Rows) <= 7 Then
                    '
                    ' Map file is bad
                    '
                    'Call HandleLocalError(KmaErrorInternal, App.EXEName, "ImportWizard.LoadImportMap failed because there was a problem with the format of the data", "LoadImportMap", False, True)
                Else
                    result.KeyMethodID = cp.Utils.EncodeInteger(Rows(0))
                    result.SourceKeyField = Rows(1)
                    result.DbKeyField = Rows(2)
                    result.ContentName = Rows(3)
                    result.GroupOptionID = cp.Utils.EncodeInteger(Rows(4))
                    result.GroupID = cp.Utils.EncodeInteger(Rows(5))
                    result.SkipRowCnt = cp.Utils.EncodeInteger(Rows(6))
                    result.DbKeyFieldType = cp.Utils.EncodeInteger(Rows(7))
                    result.importToNewContent = cp.Utils.EncodeBoolean(Rows(8))
                    result.MapPairCnt = 0
                    '
                    If UBound(Rows) > 8 Then
                        If String.IsNullOrEmpty(Trim(Rows(9))) Then
                            For Ptr = 10 To UBound(Rows)
                                Pair = Split(Rows(CInt(Ptr)), "=")
                                If UBound(Pair) > 0 Then
                                    MapPtr = result.MapPairCnt
                                    result.MapPairCnt = CInt(MapPtr + 1)
                                    ReDim Preserve result.MapPairs(MapPtr)
                                    result.MapPairs(MapPtr) = New MapPairType()
                                    result.MapPairs(CInt(MapPtr)).DbField = Pair(0)
                                    SourceSplit = Split(Pair(1), ",")
                                    If UBound(SourceSplit) > 0 Then
                                        result.MapPairs(CInt(MapPtr)).SourceFieldPtr = cp.Utils.EncodeInteger(SourceSplit(0))
                                        result.MapPairs(CInt(MapPtr)).DbFieldType = cp.Utils.EncodeInteger(SourceSplit(1))
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If

            End If
            Return result
        Catch ex As Exception
            cp.Site.ErrorReport(ex)
            Throw
        End Try
    End Function
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
Public Class MapPairType
    Public SourceFieldPtr As Integer
    Public SourceFieldName As String
    Public DbField As String
    Public DbFieldType As Integer
End Class
'
Public Class ImportMapType
    Public importToNewContent As Boolean
    Public ContentName As String
    Public KeyMethodID As Integer
    Public SourceKeyField As String
    Public DbKeyField As String
    Public DbKeyFieldType As Integer
    Public GroupOptionID As Integer
    Public GroupID As Integer
    Public SkipRowCnt As Integer
    Public MapPairCnt As Integer
    Public MapPairs() As MapPairType

    'Public Shared Widening Operator CType(v As String) As ImportMapType
    '    Throw New NotImplementedException()
    'End Operator
End Class


