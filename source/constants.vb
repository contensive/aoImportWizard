
Option Explicit On
Option Strict On

Public Module constants
    '
    Public Const ButtonContinue2 = " Continue "
    Public Const ButtonBack2 = " Back "
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
    End Class

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
    '
    Public Const SubFormMax As Integer = 9
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
    Public Const RequestNameSubForm As String = "SubForm"
    Public Const RequestNameImportWizardID As String = "ImportWizardID"
    Public Const RequestNameImportSource As String = "ImportWizardSource"
    Public Const RequestNameImportContentID As String = "ImportWizardDestination"
    Public Const RequestNameImportUpload As String = "ImportWizardUpload"
    Public Const RequestNameImportKeyMethodID As String = "ImportWizardKeyMethodID"
    Public Const RequestNameImportSourceKeyFieldPtr As String = "ImportWizardSourceKeyFieldPtr"
    Public Const RequestNameImportDbKeyField As String = "ImportWizardDbkeyField"
    Public Const RequestNameImportGroupID As String = "ImportGroupID"
    Public Const RequestNameImportGroupNew As String = "inputNewGroupName"
    Public Const RequestNameImportGroupOptionID As String = "ImportGroupOptionID"
    Public Const RequestNameImportEmail As String = "ImportEmailNotify"
    Public Const RequestNameImportMapFile As String = "ImportMapFile"
    'Public Const RequestNameImportContentName  As String = "ImportContentName"
    Public Const RequestNameImportSkipFirstRow As String = "ImportSkipFirstRow"
    '
    ' -- errors for resultErrList
    Public Enum resultErrorEnum
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
    Public Enum httpErrorEnum
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

