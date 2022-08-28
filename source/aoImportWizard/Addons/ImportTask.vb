
Imports Contensive.ImportWizard.Controllers
Imports Contensive.BaseClasses
Imports Contensive.Models.Db
Imports Contensive.ImportWizard.Models
Imports ImportMapModel

Namespace Contensive.ImportWizard.Addons
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
    ''' <summary>
    ''' Process the import
    ''' </summary>
    Public Class ImportTask
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ''' <summary>
        ''' Process the import
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Try
                '
                '   Check this server for anything in the tasks queue
                Using app As New ApplicationModel(CP)
                    Dim taskList As List(Of ImportWizardTaskModel) = DbBaseModel.createList(Of ImportWizardTaskModel)(CP, "DateCompleted is null")
                    For Each task As ImportWizardTaskModel In taskList
                        task.dateCompleted = Now
                        Dim previousProcessAborted As Boolean = (task.dateStarted <> Date.MinValue)
                        If previousProcessAborted Then
                            task.resultMessage = "This task failed to complete."
                        Else
                            '
                            ' -- Import a CSV file
                            Dim cvsFilename As String = Replace(task.uploadFilename, "/", "\")
                            Dim importMapFilename As String = Replace(task.importMapFilename, "/", "\")
                            Dim resultMessage As String = processCSV(app, cvsFilename, importMapFilename)
                            Dim notifyBody As String
                            If Not String.IsNullOrEmpty(resultMessage) Then
                                notifyBody = "This email is to notify you that your data import is complete for [" & CP.Site.Name & "]" & vbCrLf & "The following errors occurred during import" & vbCrLf & resultMessage
                            Else
                                notifyBody = "This email is to notify you that your data import is complete for [" & CP.Site.Name & "]"
                            End If
                            Dim notifySubject As String = "Import Completed"
                            If String.IsNullOrEmpty(resultMessage) Then
                                resultMessage = "OK"
                            End If
                            task.resultMessage = If(resultMessage.Length > 255, resultMessage.Substring(0, 254), resultMessage)
                            If Not String.IsNullOrEmpty(task.notifyEmail) And Not String.IsNullOrEmpty(notifyBody) Then
                                Dim notifyFromAddress As String = CP.Site.GetText("EmailFromAddress", "")
                                Call CP.Email.send(task.notifyEmail, notifyFromAddress, "Task Completion Notification", notifyBody)
                            End If
                        End If
                        task.save(CP)
                    Next
                    Return Nothing
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '=====================================================================================
        ''' <summary>
        ''' process the input csv file
        ''' </summary>
        ''' <param name="app"></param>
        ''' <param name="CSVFilename"></param>
        ''' <param name="ImportMapFilename"></param>
        ''' <returns></returns>
        Private Function processCSV(app As ApplicationModel, CSVFilename As String, ImportMapFilename As String) As String
            Try
                Dim cp As CPBaseClass = app.cp
                Dim result As String = ""
                If Left(CSVFilename, 1) = "\" Then
                    CSVFilename = Mid(CSVFilename, 2)
                End If
                Dim hint As String = "010"
                Dim source As String = cp.CdnFiles.Read(CSVFilename)
                If String.IsNullOrEmpty(source) Then
                    '
                    ' -- if source is empty, return empty
                    Return String.Empty
                End If

                hint = "020"
                Dim importConfig As ImportConfigModel = ImportConfigModel.create(app)
                Dim importMap As ImportMapModel = ImportMapModel.create(cp, importConfig)
                hint = "040"
                Dim cells As String(,) = GenericController.parseFile(source)
                Dim colCnt As Integer = UBound(cells, 1) + 1
                Dim ImportTableName As String = ""
                Dim dBFieldName As String
                '
                ' -- setup destination content/database table, and setup importMap
                If Not importMap.importToNewContent Then
                    '
                    ' -- set default table to people
                    If (String.IsNullOrEmpty(importMap.contentName)) Then
                        importMap.contentName = "People"
                    End If
                    ImportTableName = cp.Content.GetTable(importMap.contentName)
                Else
                    '
                    ' create the destination table and import map
                    importMap.skipRowCnt = 1
                    importMap.mapPairCnt = colCnt
                    ReDim importMap.mapPairs(colCnt - 1)
                    importMap.mapPairs(colCnt - 1) = New ImportMapModel_MapPair
                    ImportTableName = importMap.contentName
                    ImportTableName = Replace(ImportTableName, " ", "_")
                    ImportTableName = Replace(ImportTableName, "-", "_")
                    ImportTableName = Replace(ImportTableName, ",", "_")
                    hint = "060"
                    cp.Content.AddContent(importMap.contentName, ImportTableName)
                    hint = "070"
                    Dim colPtr As Integer
                    For colPtr = 0 To colCnt - 1
                        importMap.mapPairs(colPtr) = New ImportMapModel_MapPair
                        hint = "080"
                        dBFieldName = cells(colPtr, 0)
                        dBFieldName = encodeFieldName(cp, dBFieldName)
                        If (String.IsNullOrEmpty(dBFieldName)) Then
                            dBFieldName = "field" & colPtr
                        End If
                        importMap.mapPairs(colPtr).dbFieldName = dBFieldName
                        importMap.mapPairs(colPtr).dbFieldType = 2
                        importMap.mapPairs(colPtr).uploadFieldName = dBFieldName
                        importMap.mapPairs(colPtr).uploadFieldPtr = colPtr
                        hint = "090"
                        cp.Content.AddContentField(importMap.contentName, dBFieldName, 2)
                    Next
                End If

                If importMap.mapPairCnt > 0 Then
                    hint = "200"
                    Dim SourceKeyPtr As Integer = cp.Utils.EncodeInteger(importMap.sourceKeyField)
                    If (String.IsNullOrEmpty(importMap.dbKeyField)) Or (SourceKeyPtr < 0) Then
                        importMap.keyMethodID = KeyMethodInsertAll
                    End If
                    '
                    Dim KeyCriteria As String = "(1=0)"
                    Dim rowPtr As Integer
                    Dim rowCnt As Integer = UBound(cells, 2) + 1
                    For rowPtr = importMap.skipRowCnt To rowCnt - 1
                        hint = "300"
                        Dim updateRecord As Boolean = False
                        Dim insertRecord As Boolean = False
                        Dim rowWidth As Integer = 0
                        If (rowPtr = 76) Or (rowPtr = 105) Then
                            rowPtr = rowPtr
                        End If
                        If True Then
                            hint = "310"
                            If importMap.keyMethodID = KeyMethodInsertAll Then
                                hint = "320"
                                insertRecord = True
                            Else
                                hint = "330"
                                '
                                ' Update or Update-And-Insert, Build Key Criteria
                                Dim sourceKeyData As String = cells(SourceKeyPtr, rowPtr)
                                If Len(sourceKeyData) > 2 And Left(sourceKeyData, 1) = """" And Right(sourceKeyData, 1) = """" Then
                                    sourceKeyData = Trim(Mid(sourceKeyData, 2, Len(sourceKeyData) - 2))
                                End If
                                If String.IsNullOrEmpty(Trim(sourceKeyData)) Then
                                    '
                                    ' Source had no data in key field, insert if allowed
                                    '
                                    If importMap.keyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                        insertRecord = True
                                    End If
                                Else
                                    hint = "340"
                                    '
                                    ' Source has good key field data
                                    '
                                    Select Case importMap.dbKeyFieldType
                                        Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                            '
                                            ' number
                                            '
                                            updateRecord = True
                                            KeyCriteria = "(" & importMap.dbKeyField & "=" & cp.Db.EncodeSQLNumber(cp.Utils.EncodeNumber(sourceKeyData)) & ")"
                                        Case FieldTypeBoolean
                                            '
                                            ' Boolean
                                            '
                                            updateRecord = True
                                            KeyCriteria = "(" & importMap.dbKeyField & "=" & cp.Db.EncodeSQLBoolean(cp.Utils.EncodeBoolean(sourceKeyData)) & ")"
                                        Case FieldTypeDate
                                            '
                                            ' date
                                            '
                                            updateRecord = True
                                            KeyCriteria = "(" & importMap.dbKeyField & "=" & cp.Db.EncodeSQLDate(cp.Utils.EncodeDate(sourceKeyData)) & ")"
                                        Case FieldTypeText, FieldTypeResourceLink, FieldTypeLink
                                            '
                                            ' text
                                            '
                                            updateRecord = True
                                            KeyCriteria = "(" & importMap.dbKeyField & "=" & cp.Db.EncodeSQLText(Left(sourceKeyData, 255)) & ")"
                                        Case FieldTypeLongText, FieldTypeHTML
                                            '
                                            ' long text
                                            '
                                            updateRecord = True
                                            KeyCriteria = "(" & importMap.dbKeyField & "=" & cp.Db.EncodeSQLText(sourceKeyData) & ")"
                                        Case Else
                                            '
                                            ' unknown field type
                                            '
                                            updateRecord = True
                                            If importMap.keyMethodID = KeyMethodUpdateOnMatchInsertOthers Then
                                                insertRecord = True
                                            End If
                                    End Select
                                    ' move to after the row work so we can skip the insert if the row width=0
                                End If
                            End If
                            '
                            ' -- store textfile fields to be updated manually after the upate
                            Dim textFileManualUpdate As New List(Of textFileModel)
                            '
                            ' If Insert, Build KeyCriteria and setup CS
                            Dim updateSQLFieldSet As String = ""
                            If (insertRecord Or updateRecord) Then
                                hint = "400"
                                Dim fieldPtr As Integer
                                '
                                ' Build Update SQL
                                For fieldPtr = 0 To importMap.mapPairCnt - 1
                                    hint = "500"
                                    Dim sourcePtr As Integer = importMap.mapPairs(fieldPtr).uploadFieldPtr
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
                                        dBFieldName = importMap.mapPairs(fieldPtr).dbFieldName
                                        Dim sourceData As String = cells(sourcePtr, rowPtr)
                                        rowWidth += Len(Trim(sourceData))
                                        ' there are no fieldtypes defined as 0, and I do not want the CS open now, so we can avoid the insert if rowwidth=0

                                        Select Case importMap.mapPairs(fieldPtr).dbFieldType
                                            Case FieldTypeAutoIncrement, FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger, FieldTypeLookup, FieldTypeManyToMany, FieldTypeMemberSelect
                                                '
                                                ' number, nullable
                                                If (String.IsNullOrEmpty(sourceData)) Then
                                                    updateSQLFieldSet &= "," & dBFieldName & "=null"
                                                Else
                                                    Dim sourceConverted As Double = cp.Utils.EncodeNumber(sourceData)
                                                    updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLNumber(sourceConverted)
                                                End If
                                            Case FieldTypeBoolean
                                                '
                                                ' Boolean, null is false
                                                Dim sourceConverted As Boolean = cp.Utils.EncodeBoolean(sourceData)
                                                updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLBoolean(sourceConverted)
                                            Case FieldTypeDate
                                                '
                                                ' date, nullable
                                                If (String.IsNullOrEmpty(sourceData)) Then
                                                    updateSQLFieldSet &= "," & dBFieldName & "=null"
                                                Else
                                                    Dim sourceConverted As Date = cp.Utils.EncodeDate(sourceData)
                                                    updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLDate(sourceConverted)
                                                End If
                                            Case FieldTypeText, FieldTypeLink, FieldTypeResourceLink
                                                '
                                                ' text, null is empty
                                                Dim sourceConverted As String = If(String.IsNullOrEmpty(sourceData), "", If(sourceData.Length < 256, sourceData, Left(sourceData, 255)))
                                                updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLText(sourceConverted)
                                            Case FieldTypeLongText, FieldTypeHTML
                                                '
                                                ' long text, null is empty
                                                updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLText(sourceData)
                                            'Case FieldTypeFile, FieldTypeImage, FieldTypeTextFile, FieldTypeCSSFile, FieldTypeXMLFile, FieldTypeJavascriptFile, FieldTypeHTMLFile
                                            '    '
                                            '    ' filenames, can not import these, but at least update the filename
                                            '    Dim sourceConverted As String = If(String.IsNullOrEmpty(sourceData), "", If(sourceData.Length < 256, sourceData, Left(sourceData, 255)))
                                            '    updateSQLFieldSet &= "," & dBFieldName & "=" & cp.Db.EncodeSQLText(sourceConverted)
                                            Case FieldTypeTextFile, FieldTypeCSSFile, FieldTypeHTMLFile, FieldTypeJavascriptFile, FieldTypeXMLFile
                                                '
                                                ' -- text files, like notes
                                                textFileManualUpdate.Add(New textFileModel With {
                                                    .fieldName = dBFieldName,
                                                    .fieldValue = sourceData
                                                })
                                        End Select
                                    End If
                                Next
                            End If
                            hint = "700"
                            If Err.Number <> 0 Then
                                result &= vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                            ElseIf rowWidth = 0 Then
                                result &= vbCrLf & "Row " & (rowPtr + 1) & " was not imported because it was empty."
                            ElseIf (Not String.IsNullOrEmpty(updateSQLFieldSet)) Then
                                '
                                ' insert or update the table
                                '
                                Dim cs As CPCSBaseClass = cp.CSNew()
                                Dim matchFound As Boolean
                                If updateRecord Then
                                    '
                                    ' Update requested
                                    matchFound = False
                                    insertRecord = False
                                    matchFound = cs.Open(importMap.contentName, KeyCriteria, "ID", False)
                                    Call cs.Close()
                                    If matchFound Then
                                        '
                                        ' match was found, update all records found
                                        updateRecord = True
                                    ElseIf (importMap.keyMethodID = KeyMethodUpdateOnMatchInsertOthers) Then
                                        '
                                        ' no match, convert to insert if that was requested
                                        insertRecord = True
                                    End If
                                End If
                                If insertRecord Then
                                    '
                                    ' Insert a new record and convert to an update
                                    updateRecord = False
                                    If cs.Insert(importMap.contentName) Then
                                        If Err.Number <> 0 Then
                                            Err.Clear()
                                            result &= vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                                        Else
                                            If Not cs.OK() Then
                                                result &= vbCrLf & "Row " & (rowPtr + 1) & " could not be imported because a record count not be inserted."
                                            Else
                                                Dim recordId As Integer = cs.GetInteger("ID")
                                                KeyCriteria = "(ID=" & cp.Utils.EncodeNumber(recordId) & ")"
                                                updateRecord = True
                                            End If
                                        End If
                                    End If
                                    Call cs.Close()
                                End If
                                '
                                ' Update the record if needed
                                If updateRecord Then
                                    hint = "900"
                                    '
                                    ' Update all records in the current recordset
                                    Dim UpdateSQL As String = "update " & ImportTableName & " set " & Mid(updateSQLFieldSet, 2) & " where " & KeyCriteria
                                    Call cs.OpenSQL(UpdateSQL, cp.Content.GetDataSource(importMap.contentName))
                                    If Err.Number <> 0 Then
                                        Err.Clear()
                                        result &= vbCrLf & "Row " & (rowPtr + 1) & " could not be imported. [" & Err.Description & "]"
                                    End If
                                End If
                                '
                                ' -- if there are manual update fields (textfile) then update them now
                                If textFileManualUpdate.Count > 0 Then
                                    Using manualUpdateCs As CPCSBaseClass = cp.CSNew()
                                        If manualUpdateCs.Open(importMap.contentName, KeyCriteria) Then
                                            For Each textfile As textFileModel In textFileManualUpdate
                                                manualUpdateCs.SetField(textfile.fieldName, textfile.fieldValue)
                                            Next
                                        End If
                                    End Using
                                End If
                                '
                                '
                                If importMap.groupOptionID <> GroupOptionNone Then
                                    '
                                    ' update/insert OK and records are People
                                    Dim group As GroupModel = DbBaseModel.create(Of GroupModel)(cp, importMap.groupID)
                                    If (group IsNot Nothing) Then
                                        For Each user As PersonModel In DbBaseModel.createList(Of PersonModel)(cp, KeyCriteria)
                                            If (user IsNot Nothing) Then
                                                '
                                                ' Add Groups
                                                Select Case importMap.groupOptionID
                                                    Case GroupOptionAll
                                                        '
                                                        cp.Group.AddUser(group.id, user.id)
                                                    Case GroupOptionOnMatch
                                                        '
                                                        If matchFound Then
                                                            cp.Group.AddUser(group.id, user.id)
                                                        End If
                                                    Case GroupOptionOnNoMatch
                                                        '
                                                        If Not matchFound Then
                                                            cp.Group.AddUser(group.id, user.id)
                                                        End If
                                                End Select
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                        End If
                        Dim LoopCnt As Integer = LoopCnt + 1
                        If LoopCnt > 10 Then
                            LoopCnt = 0
                        End If
                    Next
                End If
                Return result
            Catch ex As Exception
                app.cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        Public Shared Function encodeFieldName(cp As CPBaseClass, Source As String) As String

            Try
                Dim result As String = ""
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
                        returnString &= chr
                    Else
                        returnString &= "_"
                    End If
                Next
                result = returnString
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
    '
    Public Class textFileModel
        Public Property fieldName As String
        Public Property fieldValue As String
    End Class
End Namespace
