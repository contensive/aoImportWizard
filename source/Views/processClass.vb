
Option Strict On
Option Explicit On

Imports Contensive.Addons.ImportWizard.Controllers
Imports Contensive.BaseClasses

Namespace Views
    '
    '========================================================================
    '
    ' Sample file to create a Contensive Aggregate Object
    '
    ' An Aggregate Object is an ActiveX DLL (in process dll) called by Contensive
    ' in response to finding a 'token' in the content of a page. When the token is
    ' found, it is looked up in the Aggregate Function Objects table and the Program ID
    ' for this code is retrieved, and used to create an object from this class.
    '
    ' Contensive v3.4+
    '
    ' Contensive calls the execute method with four arguments:
    '
    '   CsvObject - an object pointer to the ContentServerClass object loaded for this application
    '
    '   MainObject - an object pointer to the MainClass (web client interface) for this hit
    '
    '   OptionString - an encoded string that carries the arguments for this add-on
    '       Always use the Csv.GetAddonOption() method to retrieve a value from the Optionstring
    '       Arguments are a name=value argument list
    '       The arguments are created first, from the argument list in the Add-on record, and
    '           second from changes made by the site administrators during edit.
    '
    '   FilterInput - when the addon is acting as a filter, this is the input. For instance, If this
    '       Addon is a filter for a page, the Filter input will be the page content.
    '
    ' The output depends on the context of the Add-on:
    '
    '   Page Add-ons create content the replaces their token on the page. The return string
    '       is the content to be placed on the page.
    '
    '   Filter Add-ons take the FilterInput argument, modify as needed, and return it
    '       as the return string.
    '
    '   Process Add-ons perform their function and return error messages through their return
    '
    '========================================================================
    '
    Public Class processClass
        Inherits AddonBaseClass
        '
        ' - if nuget references are not there, open nuget command line and click the 'reload' message at the top, or run "Update-Package -reinstall" - close/open
        ' - Verify project root name space is empty
        ' - Change the namespace (AddonCollectionVb) to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        '
        '=====================================================================================
        ''' <summary>
        ''' AddonDescription
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ae As New applicationController(CP, False)
                    '
                    '
                    Dim ResultMessage As String
                    Dim CSVFilename As String
                    Dim ImportMapFilename As String
                    Dim NotifyBody As String
                    Dim NotifySubject As String
                    Dim PreviousProcessAborted As Boolean
                    Dim notifyFromAddress As String
                    '
                    '   Check this server for anything in the tasks queue
                    If CP.Content.IsField("Import Wizard Tasks", "ID") Then
                        Dim taskList As List(Of Models.ImportWizardTaskModel) = Models.ImportWizardTaskModel.createList(CP, "DateCompleted is null")
                        For Each task As Models.ImportWizardTaskModel In taskList
                            task.DateCompleted = Now
                            PreviousProcessAborted = (task.DateStarted <> Date.MinValue)
                            If PreviousProcessAborted Then
                                task.ResultMessage = "This task failed to complete."
                            Else
                                NotifyBody = ""
                                '
                                ' Import a CSV file
                                '
                                CSVFilename = Replace(task.uploadFilename, "/", "\")
                                ImportMapFilename = Replace(task.ImportMapFilename, "/", "\")
                                '
                                ResultMessage = ProcessCSV(CP, CSVFilename, ImportMapFilename)
                                If ResultMessage <> "" Then
                                    NotifyBody = "This email is to notify you that your data import is complete for [" & CP.Site.Name & "]" & vbCrLf & "The following errors occurred during import" & vbCrLf & ResultMessage
                                Else
                                    NotifyBody = "This email is to notify you that your data import is complete for [" & CP.Site.Name & "]"
                                End If
                                NotifySubject = "Import Completed"
                                If ResultMessage = "" Then
                                    ResultMessage = "OK"
                                End If
                                task.ResultMessage = ResultMessage
                                If task.NotifyEmail <> "" And NotifyBody <> "" Then
                                    notifyFromAddress = CP.Site.GetText("EmailFromAddress", "")
                                    Call CP.Email.send(task.NotifyEmail, notifyFromAddress, "Task Completion Notification", NotifyBody)
                                End If
                            End If
                            task.save(CP)
                        Next
                    End If
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function

        '
        '=================================================================================
        '
        Private Function ProcessCSV(cp As CPBaseClass, CSVFilename As String, ImportMapFilename As String) As String
            Dim result As String = ""
            Try
                Dim matchFound As Boolean
                Dim colCnt As Integer
                Dim rowCnt As Integer
                Dim ImportMap As ImportMapType
                Dim sourcePtr As Integer
                Dim Source As String
                Dim Rows() As String
                Dim rowPtr As Integer
                Dim colPtr As Integer
                Dim row As String
                Dim cells As String(,)
                ' Dim CS As Integer
                Dim DBFieldName As String
                Dim SourceData As String
                Dim fieldPtr As Integer
                Dim ContentName As String
                Dim DbKeyField As String
                Dim SourceKeyPtr As Integer
                Dim SourceKeyData As String
                Dim KeyMethodID As Integer
                Dim KeyCriteria As String
                Dim UpdateRecord As Boolean
                Dim InsertRecord As Boolean
                Dim recordId As Integer
                Dim UpdateSQLFieldSet As String
                Dim UpdateSQL As String
                Dim LoopCnt As Integer
                Dim RowWidth As Integer
                Dim hint As String
                Dim ImportTableName As String = ""
                '
                hint = "000"
                '
                If Left(CSVFilename, 1) = "\" Then
                    CSVFilename = Mid(CSVFilename, 2)
                End If
                hint = "010"
                Source = cp.File.ReadVirtual(CSVFilename)
                If Source <> "" Then
                    hint = "020"
                    ImportMap = genericController.LoadImportMap(cp, cp.File.ReadVirtual(ImportMapFilename))
                    hint = "030"
                    ContentName = ImportMap.ContentName
                    hint = "040"
                    cells = genericController.parseFile(Source)
                    colCnt = UBound(cells, 1) + 1
                    rowCnt = UBound(cells, 2) + 1
                    If ImportMap.importToNewContent Then
                        '
                        ' -- edge case, not supporting for now
                        'hint = "050"
                        '
                        ' create the destination table and import map
                        '
                        ImportMap.SkipRowCnt = 1
                        ImportMap.MapPairCnt = colCnt
                        ReDim ImportMap.MapPairs(colCnt - 1)
                        ImportMap.MapPairs(colCnt - 1) = New MapPairType
                        ImportTableName = ContentName
                        ImportTableName = Replace(ImportTableName, " ", "_")
                        ImportTableName = Replace(ImportTableName, "-", "_")
                        ImportTableName = Replace(ImportTableName, ",", "_")
                        hint = "060"

                        cp.Content.AddContent(ContentName, ImportTableName)

                        'Dim table As Models.TableModel = Models.TableModel.add(cp)
                        'table.name = ImportTableName
                        'table.save(cp)
                        '
                        'Dim content As Models.ContentModel = Models.ContentModel.add(cp)
                        'content.name = ContentName
                        'content.Active = True
                        'content.AuthoringTableID = table.id
                        'content.save(cp)

                        hint = "070"
                        For colPtr = 0 To colCnt - 1
                            ImportMap.MapPairs(colPtr) = New MapPairType
                            hint = "080"
                            DBFieldName = cells(colPtr, 0)
                            DBFieldName = encodeFieldName(cp, DBFieldName)
                            If (DBFieldName = "") Then
                                DBFieldName = "field" & colPtr
                            End If
                            ImportMap.MapPairs(colPtr).DbField = DBFieldName
                            ImportMap.MapPairs(colPtr).DbFieldType = 2
                            ImportMap.MapPairs(colPtr).SourceFieldName = DBFieldName
                            ImportMap.MapPairs(colPtr).SourceFieldPtr = colPtr
                            hint = "090"
                            cp.Content.AddContentField(ContentName, DBFieldName, 2)
                            'Dim field As Models.ContentFieldModel = Models.ContentFieldModel.add(cp)
                            'field.Active = True
                            'field.Type = 2
                            'field.ContentID = content.id
                            'field.save(cp)
                        Next
                    Else
                        If ContentName = "" Then
                            ContentName = "People"
                        End If
                        ImportTableName = cp.Content.GetTable(ImportMap.ContentName)
                    End If

                    If ImportMap.MapPairCnt > 0 Then
                        hint = "200"
                        DbKeyField = ImportMap.DbKeyField
                        SourceKeyPtr = cp.Utils.EncodeInteger(ImportMap.SourceKeyField)
                        KeyMethodID = ImportMap.KeyMethodID
                        If (DbKeyField = "") Or (SourceKeyPtr < 0) Then
                            KeyMethodID = KeyMethodInsertAll
                        End If
                        '
                        '
                        '
                        KeyCriteria = "(1=0)"
                        For rowPtr = ImportMap.SkipRowCnt To rowCnt - 1
                            hint = "300"
                            UpdateRecord = False
                            InsertRecord = False
                            'row = Rows(rowPtr)
                            RowWidth = 0
                            If (rowPtr = 76) Or (rowPtr = 105) Then
                                rowPtr = rowPtr
                            End If
                            If True Then
                                'If row <> "" Then
                                hint = "310"
                                'cells = kmaSplit(row, ",")
                                If KeyMethodID = KeyMethodInsertAll Then
                                    hint = "320"
                                    '
                                    InsertRecord = True
                                Else
                                    hint = "330"
                                    '
                                    ' Update or Update-And-Insert, Build Key Criteria
                                    '
                                    SourceKeyData = cells(SourceKeyPtr, rowPtr)
                                    If Len(SourceKeyData) > 2 And Left(SourceKeyData, 1) = """" And Right(SourceKeyData, 1) = """" Then
                                        SourceKeyData = Trim(Mid(SourceKeyData, 2, Len(SourceKeyData) - 2))
                                    End If
                                    If Trim(SourceKeyData) = "" Then
                                        '
                                        ' Source had no data in key field, insert if allowed
                                        '
                                        If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                            InsertRecord = True
                                        End If
                                    Else
                                        hint = "340"
                                        '
                                        ' Source has good key field data
                                        '
                                        Select Case ImportMap.DbKeyFieldType
                                            Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                                '
                                                ' number
                                                '
                                                UpdateRecord = True
                                                KeyCriteria = "(" & DbKeyField & "=" & cp.Db.EncodeSQLNumber(CDbl(SourceKeyData)) & ")"
                                            Case FieldTypeBoolean
                                                '
                                                ' Boolean
                                                '
                                                UpdateRecord = True
                                                KeyCriteria = "(" & DbKeyField & "=" & cp.Db.EncodeSQLBoolean(CBool(SourceKeyData)) & ")"
                                            Case FieldTypeDate
                                                '
                                                ' date
                                                '
                                                UpdateRecord = True
                                                KeyCriteria = "(" & DbKeyField & "=" & cp.Db.EncodeSQLDate(CDate(SourceKeyData)) & ")"
                                            Case FieldTypeText, FieldTypeResourceLink, FieldTypeLink
                                                '
                                                ' text
                                                '
                                                UpdateRecord = True
                                                KeyCriteria = "(" & DbKeyField & "=" & cp.Db.EncodeSQLText(Left(SourceKeyData, 255)) & ")"
                                            Case FieldTypeLongText, FieldTypeHTML
                                                '
                                                ' long text
                                                '
                                                UpdateRecord = True
                                                KeyCriteria = "(" & DbKeyField & "=" & cp.Db.EncodeSQLText(SourceKeyData) & ")"
                                            Case Else
                                                '
                                                ' unknown field type
                                                '
                                                UpdateRecord = True
                                                If KeyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                                    InsertRecord = True
                                                End If
                                        End Select
                                        ' move to after the row work so we can skip the insert if the row width=0
                                    End If
                                End If
                                '
                                ' If Insert, Build KeyCriteria and setup CS
                                ' x If Update, CS is good -- else there is no open record
                                '
                                '                  
                                If (InsertRecord Or UpdateRecord) Then
                                    hint = "400"
                                    '
                                    ' Build Update SQL
                                    '
                                    UpdateSQLFieldSet = ""
                                    'For fieldPtr = 0 To colCnt - 1
                                    For fieldPtr = 0 To ImportMap.MapPairCnt - 1
                                        hint = "500"
                                        sourcePtr = ImportMap.MapPairs(fieldPtr).SourceFieldPtr
                                        If sourcePtr < 0 Then
                                            '
                                            ' Bad pointer
                                            '
                                            sourcePtr = sourcePtr
                                        ElseIf sourcePtr >= colCnt Then
                                            '
                                            ' This data row was not as Integer as the header row - skip it
                                            '
                                            sourcePtr = sourcePtr
                                        Else
                                            hint = "600"
                                            DBFieldName = ImportMap.MapPairs(fieldPtr).DbField
                                            SourceData = cells(sourcePtr, rowPtr)
                                            RowWidth = RowWidth + Len(Trim(SourceData))
                                            ' there are no fieldtypes defined as 0, and I do not want the CS open now, so we can avoid the insert if rowwidth=0

                                            Select Case ImportMap.MapPairs(fieldPtr).DbFieldType
                                                Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                                    '
                                                    ' number
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLNumber(CDbl(SourceData))
                                                Case FieldTypeBoolean
                                                    '
                                                    ' Boolean
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLBoolean(CBool(SourceData))
                                                Case FieldTypeDate
                                                    '
                                                    ' date
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLDate(CDate(SourceData))
                                                Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                                    'Case FieldTypeText
                                                    '
                                                    ' text
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLText(Left(SourceData, 255))
                                                Case FieldTypeLongText, FieldTypeHTML
                                                    'Case FieldTypeLongText
                                                    '
                                                    ' long text
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLText(SourceData)
                                        'Case FieldTypeTextFile
                                        '
                                        ' can not map to any file type because these are sql updates and each record must have a unique filename
                                        ' the import needs to be rewritten to update one record at a time.
                                        '   1) pick up the key criteria and run it on live Db to select the record(s) to update
                                        '   2) update the records in the ContentSet and SaveCS
                                        '
                                        '    '
                                        '    ' text file
                                        '    '
                                                Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile

                                                    '
                                                    ' filenames, can not import these, but at least update the filename
                                                    '
                                                    UpdateSQLFieldSet = UpdateSQLFieldSet & "," & DBFieldName & "=" & cp.Db.EncodeSQLText(Left(SourceData, 255))
                                            End Select
                                        End If
                                    Next
                                End If
                                hint = "700"
                                ''on error Resume Next
                                'Call CSv.CloseCS(CS)
                                If Err.Number <> 0 Then
                                    ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                                ElseIf RowWidth = 0 Then
                                    ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " was not imported because it was empty."
                                ElseIf (UpdateSQLFieldSet <> "") Then
                                    '
                                    ' insert or update the table
                                    '
                                    Dim cs As CPCSBaseClass = cp.CSNew()
                                    If UpdateRecord Then
                                        '
                                        ' Update requested
                                        '
                                        'On Error Resume Next
                                        matchFound = False
                                        InsertRecord = False

                                        matchFound = cs.Open(ContentName, KeyCriteria, "ID", False)

                                        Call cs.Close()
                                        '
                                        If matchFound Then
                                            '
                                            ' match was found, update all records found
                                            '
                                            UpdateRecord = True
                                        ElseIf (KeyMethodID = KeyMethodUpdateOnMatchInsertOthers) Then
                                            '
                                            ' no match, convert to insert if that was requested
                                            '
                                            InsertRecord = True
                                        End If
                                    End If
                                    If InsertRecord Then
                                        '
                                        ' Insert a new record and convert to an update
                                        '
                                        'on error Resume Next
                                        UpdateRecord = False
                                        If cs.Insert(ContentName) Then
                                            If Err.Number <> 0 Then
                                                Err.Clear()
                                                ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                                            Else
                                                If Not cs.OK() Then
                                                    ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported because a record count not be inserted."
                                                Else
                                                    recordId = cs.GetInteger("ID")
                                                    KeyCriteria = "(ID=" & cp.Utils.EncodeNumber(recordId) & ")"
                                                    UpdateRecord = True
                                                End If
                                            End If
                                        End If

                                        'on error GoTo ErrorTrap
                                        Call cs.Close()
                                    End If
                                    '
                                    ' Update the record if needed
                                    '
                                    If UpdateRecord Then
                                        hint = "900"
                                        '
                                        ' Update all records in the current recordset
                                        '
                                        'on error Resume Next
                                        UpdateSQL = "update " & ImportTableName & " set " & Mid(UpdateSQLFieldSet, 2) & " where " & KeyCriteria
                                        Call cs.OpenSQL(UpdateSQL, cp.Content.GetDataSource(ImportMap.ContentName))
                                        If Err.Number <> 0 Then
                                            Err.Clear()
                                            ProcessCSV = ProcessCSV & vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                                        End If
                                        'on error GoTo ErrorTrap
                                    End If
                                    If ImportMap.GroupOptionID <> GroupOptionNone Then
                                        '
                                        ' update/insert OK and records are People
                                        '
                                        If cs.Open(ContentName, KeyCriteria, "ID", False) Then
                                            Do While cs.OK()
                                                '
                                                ' Add Groups
                                                '
                                                recordId = cs.GetInteger("ID")
                                                Select Case ImportMap.GroupOptionID
                                                    Case GroupOptionAll
                                                        '
                                                        '
                                                        '
                                                        Call Models.MemberRuleModel.AddGroupMember(cp, recordId, ImportMap.GroupID)
                                                    Case GroupOptionOnMatch
                                                        '
                                                        '
                                                        '
                                                        If matchFound Then
                                                            Call Models.MemberRuleModel.AddGroupMember(cp, recordId, ImportMap.GroupID)
                                                        End If
                                                    Case GroupOptionOnNoMatch
                                                        '
                                                        '
                                                        '
                                                        If Not matchFound Then
                                                            Call Models.MemberRuleModel.AddGroupMember(cp, recordId, ImportMap.GroupID)
                                                        End If
                                                End Select
                                                cs.GoNext()
                                            Loop
                                        End If

                                        Call cs.Close()
                                    End If
                                End If
                                'on error GoTo ErrorTrap
                            End If
                            LoopCnt = LoopCnt + 1
                            If LoopCnt > 10 Then
                                LoopCnt = 0
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                Throw
            End Try
            Return result

        End Function

        '
        '========================================================================
        ' encodeFieldName
        '
        '========================================================================
        '
        Public Function encodeFieldName(cp As CPBaseClass, Source As String) As String

            Dim result As String = ""
            Try
                Dim allowed As String
                Dim chr As String
                Dim Ptr As Integer
                Dim cnt As Integer
                Dim returnString As String
                '
                returnString = ""
                cnt = Len(Source)
                If cnt > 254 Then
                    cnt = 254
                End If
                allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_"
                For Ptr = 1 To cnt
                    chr = Mid(Source, Ptr, 1)
                    If CBool(InStr(1, allowed, chr, vbBinaryCompare)) Then
                        returnString = returnString & chr
                    Else
                        returnString = returnString & "_"
                    End If
                Next
                result = returnString
            Catch ex As Exception
                Throw
            End Try
            Return result
        End Function
    End Class
End Namespace
