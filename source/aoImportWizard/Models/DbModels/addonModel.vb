
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class addonModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Add-ons"      '<------ set content name
        Public Const contentTableName As String = "ccAggregateFunctions"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property Admin As Boolean
        Public Property ArgumentList As String
        Public Property AsAjax As Boolean
        Public Property BlockDefaultStyles As Boolean
        Public Property BlockEditTools As Boolean
        Public Property CollectionID  as Integer
        Public Property Content As Boolean
        Public Property Copy As String
        Public Property CopyText As String
        Public Property CustomStylesFilename As String
        Public Property DotNetClass As String
        Public Property Email As Boolean
        Public Property Filter As Boolean
        Public Property FormXML As String
        Public Property Help As String
        Public Property HelpLink As String
        Public Property IconFilename As String
        Public Property IconHeight  as Integer
        Public Property IconSprites  as Integer
        Public Property IconWidth  as Integer
        Public Property InFrame As Boolean
        Public Property inlineScript As String
        Public Property IsInline As Boolean
        Public Property JavaScriptBodyEnd As String
        Public Property JavaScriptOnLoad As String
        Public Property JSFilename As String
        Public Property Link As String
        Public Property MetaDescription As String
        Public Property MetaKeywordList As String
        Public Property NavTypeID  as Integer
        Public Property ObjectProgramID As String
        Public Property OnBodyEnd As Boolean
        Public Property OnBodyStart As Boolean
        Public Property OnNewVisitEvent As Boolean
        Public Property OnPageEndEvent As Boolean
        Public Property OnPageStartEvent As Boolean
        Public Property OtherHeadTags As String
        Public Property PageTitle As String
        Public Property ProcessInterval  as Integer
        Public Property ProcessNextRun As Date
        Public Property ProcessRunOnce As Boolean
        Public Property ProcessServerKey As String
        Public Property RemoteAssetLink As String
        Public Property RemoteMethod As Boolean
        Public Property RobotsTxt As String
        Public Property ScriptingCode As String
        Public Property ScriptingEntryPoint As String
        Public Property ScriptingLanguageID  as Integer
        Public Property ScriptingTimeout As String
        Public Property StylesFilename As String
        Public Property Template As Boolean
        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As addonModel
            Return add(Of addonModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As addonModel
            Return create(Of addonModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As addonModel
            Return create(Of addonModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As addonModel
            Return createByName(Of addonModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of addonModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of addonModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of addonModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of addonModel)
            Return createList(Of addonModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of addonModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of addonModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of addonModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of addonModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of addonModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As addonModel
            Dim result As addonModel = DirectCast(Me.Clone(), addonModel)
            result.id = cp.Content.AddRecord(contentName)
            result.ccguid = cp.Utils.CreateGuid()
            result.save(cp)
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function

    End Class
End Namespace
