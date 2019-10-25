Attribute VB_Name = "ImportWizardModule"

Option Explicit
'
' forms
'
Public Const SubFormSource = 1
Public Const SubFormSourceUpload = 2
Public Const SubFormSourceUploadFolder = 3
Public Const SubFormSourceResourceLibrary = 4
Public Const SubFormNewMapping = 5
Public Const SubFormGroup = 6
Public Const SubFormKey = 7
Public Const SubFormFinish = 8
Public Const SubFormDestination = 9
'
Public Const SubFormMax = 9
'
Public Const ImportSourceUpload = 1
Public Const ImportSourceUploadFolder = 2
Public Const ImportSourceResourceLibrary = 3
'
Public Const KeyMethodInsertAll = 1
Public Const KeyMethodUpdateOnMatch = 2
Public Const KeyMethodUpdateOnMatchInsertOthers = 3
'
Public Const GroupOptionNone = 1
Public Const GroupOptionAll = 2
Public Const GroupOptionOnMatch = 3
Public Const GroupOptionOnNoMatch = 4
'
' ----- Buttons
'
'
' ----- local scope variables
'
'
' ----- Request Names
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
Public Const RequestNameImportGroupOptionID = "ImportGroupOptionID"
Public Const RequestNameImportEmail = "ImportEmailNotify"
Public Const RequestNameImportMapFile = "ImportMapFile"
'Public Const RequestNameImportContentName = "ImportContentName"
Public Const RequestNameImportSkipFirstRow = "ImportSkipFirstRow"
'
' ----- Types
'
Public Type WizardType
    '
    ' Attributes
    '
    'SendMethodID As Long
    '
    ' Form 'includes'
    '
    'IncludeTemplateForm As Boolean
    '
    ' Value Defaults
    '
    'DefaultTemplateID As Long
    '
    ' Instructions
    '
    SourceFormInstructions As String
    UploadFormInstructions As String
    MappingFormInstructions As String
    GroupFormInstructions As String
    KeyFormInstructions As String
    '
    ' Current calculated Path
    '
    Path() As Long
    PathCnt As Long
End Type
'
Public Type MapPairType
    SourceFieldPtr As Long
    SourceFieldName As String
    DbField As String
    DbFieldType As Long
End Type
'
Public Type ImportMapType
    ContentName As String
    KeyMethodID As Long
    SourceKeyField As String
    DbKeyField As String
    DbKeyFieldType As Long
    GroupOptionID As Long
    GroupID As Long
    SkipRowCnt As Long
    MapPairCnt As Long
    MapPairs() As MapPairType
End Type
'
'
'
Public Function LoadImportMap(ImportMapData As String) As ImportMapType
    On Error GoTo ErrorTrap
    '
    Dim Rows() As String
    Dim Pair() As String
    Dim Ptr As Long
    Dim SourceSplit() As String
    Dim MapPtr As Long
    '
    If ImportMapData = "" Then
        '
        ' Defaults
        '
        LoadImportMap.ContentName = "People"
        LoadImportMap.GroupID = 0
        LoadImportMap.MapPairCnt = 0
        LoadImportMap.SkipRowCnt = 1
    Else
        '
        ' read in what must be saved
        '
        Rows = Split(ImportMapData, vbCrLf)
        If UBound(Rows) <= 6 Then
            '
            ' Map file is bad
            '
            'Call HandleLocalError(KmaErrorInternal, App.EXEName, "ImportWizard.LoadImportMap failed because there was a problem with the format of the data", "LoadImportMap", False, True)
        Else
            LoadImportMap.KeyMethodID = kmaEncodeInteger(Rows(0))
            LoadImportMap.SourceKeyField = Rows(1)
            LoadImportMap.DbKeyField = Rows(2)
            LoadImportMap.ContentName = Rows(3)
            LoadImportMap.GroupOptionID = kmaEncodeInteger(Rows(4))
            LoadImportMap.GroupID = kmaEncodeInteger(Rows(5))
            LoadImportMap.SkipRowCnt = kmaEncodeInteger(Rows(6))
            LoadImportMap.DbKeyFieldType = kmaEncodeInteger(Rows(7))
            LoadImportMap.MapPairCnt = 0
            '
            If UBound(Rows) > 8 Then
                If Trim(Rows(8)) = "" Then
                    For Ptr = 9 To UBound(Rows)
                        Pair = Split(Rows(Ptr), "=")
                        If UBound(Pair) > 0 Then
                            MapPtr = LoadImportMap.MapPairCnt
                            LoadImportMap.MapPairCnt = MapPtr + 1
                            ReDim Preserve LoadImportMap.MapPairs(MapPtr)
                            LoadImportMap.MapPairs(MapPtr).DbField = Pair(0)
                            SourceSplit = Split(Pair(1), ",")
                            If UBound(SourceSplit) > 0 Then
                                LoadImportMap.MapPairs(MapPtr).SourceFieldPtr = kmaEncodeInteger(SourceSplit(0))
                                LoadImportMap.MapPairs(MapPtr).DbFieldType = kmaEncodeInteger(SourceSplit(1))
                            End If
                        End If
                    Next
                End If
            End If
        End If
    
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleLocalError(Err.Number, Err.Source, Err.Description, "LoadImportMap", True, False)
End Function
'
'===========================================================================
'   Error handler
'===========================================================================
'
Public Function HandleLocalError(ErrNumber As Long, ErrSource As String, ErrDescription As String, MethodName As String, ErrorTrap As Boolean, Optional ResumeNext As Boolean) As String
    '
    Call HandleError("ImportWizardModule", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext)
    '
End Function
