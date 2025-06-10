<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentTags.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.DocumentTags" %>
<tstsc:TagCloud ID="tagCloud" runat="server" SkinID="TagCloud"
    DataCountField="Frequency" DataKeywordField="Name"
    DataMember="ProjectAttachmentTags"
    Debug="true"
    KeywordURLFormat="javascript:tagCloud_click('%k')" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>
        <asp:ServiceReference Path="~/Services/Ajax/DocumentsService.svc" />
    </Services>
</tstsc:ScriptManagerProxyEx>

<script language="javascript" type="text/javascript">
    function tagCloud_click (tagName)
    {
        //We need to set the tag name in the project settings and then navigate to the document list
        var projectId = <%=ProjectId%>;
        Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.CustomOperation(projectId, 'FilterOnTags', tagName, tagCloud_click_success);
    }
    function tagCloud_click_success (error)
    {
        if (!error || error == '')
        {
            window.location = '<%=DocumentsListUrl%>';
        }
        else
        {
            alert (error);
        }
    }
</script>
