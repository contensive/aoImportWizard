
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models     '<------ set namespace
    Public Class PageContentModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Page Content"      '<------ set content name
        Public Const contentTableName As String = "ccPageContent"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AllowBrief As Boolean
        Public Property AllowChildListDisplay As Boolean
        Public Property AllowEmailPage As Boolean
        Public Property AllowFeedback As Boolean
        Public Property AllowHitNotification As Boolean
        Public Property AllowInChildLists As Boolean
        Public Property AllowInMenus As Boolean
        Public Property AllowLastModifiedFooter As Boolean
        Public Property AllowMessageFooter As Boolean
        Public Property AllowMetaContentNoFollow As Boolean
        Public Property AllowMoreInfo As Boolean
        Public Property AllowPrinterVersion As Boolean
        Public Property AllowReturnLinkDisplay As Boolean
        Public Property AllowReviewedFooter As Boolean
        Public Property AllowSeeAlso As Boolean
        Public Property AlternateContentID  as Integer
        Public Property AlternateContentLink As String
        Public Property ArchiveParentID  as Integer
        Public Property BlockContent As Boolean
        Public Property BlockPage As Boolean
        Public Property BlockSourceID  as Integer
        Public Property BriefFilename As String
        Public Property ChildListInstanceOptions As String
        Public Property ChildListSortMethodID  as Integer
        Public Property ChildPagesFound As Boolean
        Public Property Clicks  as Integer
        Public Property ContactMemberID  as Integer
        Public Property ContentPadding  as Integer
        Public Property Copyfilename As String
        Public Property CustomBlockMessage As String
        Public Property DateArchive As Date
        Public Property DateExpires As Date
        Public Property DateReviewed As Date
        Public Property DocFilename As String
        Public Property DocLabel As String
        Public Property Headline As String
        Public Property ImageFilename As String
        Public Property IsSecure As Boolean
        Public Property JSEndBody As String
        Public Property JSFilename As String
        Public Property JSHead As String
        Public Property JSOnLoad As String
        Public Property Link As String
        Public Property LinkAlias As String
        Public Property LinkLabel As String
        Public Property Marquee As String
        Public Property MenuHeadline As String
        Public Property MenuImageFileName As String
        Public Property OrganizationID  as Integer
        Public Property PageLink As String
        Public Property ParentID  as Integer
        Public Property ParentListName As String
        Public Property PodcastMediaLink As String
        Public Property PodcastSize  as Integer
        Public Property PubDate As Date
        Public Property RegistrationGroupID  as Integer
        Public Property RegistrationRequired As Boolean
        Public Property ReviewedBy  as Integer
        Public Property RSSDateExpire As Date
        Public Property RSSDatePublish As Date
        Public Property RSSDescription As String
        Public Property RSSLink As String
        Public Property RSSTitle As String
        Public Property TemplateID  as Integer
        Public Property TriggerAddGroupID  as Integer
        Public Property TriggerConditionGroupID  as Integer
        Public Property TriggerConditionID  as Integer
        Public Property TriggerRemoveGroupID  as Integer
        Public Property TriggerSendSystemEmailID  as Integer
        Public Property Viewings  as Integer        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As PageContentModel
            Return add(Of PageContentModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId  as Integer) As PageContentModel
            Return create(Of PageContentModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As PageContentModel
            Return create(Of PageContentModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As PageContentModel
            Return createByName(Of PageContentModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of PageContentModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId  as Integer)
            delete(Of PageContentModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of PageContentModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of PageContentModel)
            Return createList(Of PageContentModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId  as Integer) As String
            Return baseModel.getRecordName(Of PageContentModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of PageContentModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String)  as Integer
            Return baseModel.getRecordId(Of PageContentModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String)  as Integer
            Return baseModel.getCount(Of PageContentModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of PageContentModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As PageContentModel
            Dim result As PageContentModel = DirectCast(Me.Clone(), PageContentModel)
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
