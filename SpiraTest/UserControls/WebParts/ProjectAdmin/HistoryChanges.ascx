<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HistoryChanges.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.HistoryChanges" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<div class="ActivityFeed"">
    <asp:Repeater ID="rptActivity" runat="server">
        <ItemTemplate>
            <div class="ActivityFeedItem">
                <tstsc:UserNameAvatar ID="usrAvatar" UserId='<%#((HistoryChangeSetView)(Container.DataItem)).UserId %>' runat="server" AvatarSize="32" ShowFullName="false" ShowUserName="false" />
                <div class="ActivityFeedItemBody">
                    <tstsc:HyperLinkEx ID="lnkUser" runat="server" Text='<%#Microsoft.Security.Application.Encoder.HtmlEncode(((HistoryChangeSetView)(Container.DataItem)).UserName) %>' />
                    <asp:Literal ID="ltrUser" runat="server" Text='<%#Microsoft.Security.Application.Encoder.HtmlEncode(((HistoryChangeSetView)(Container.DataItem)).UserName) %>' Visible="false" />
                    <asp:Literal ID="ltrAction" runat="server" Text='<%#(((HistoryChangeSetView)(Container.DataItem)).ChangeTypeName).ToLower() %>' />
                    <asp:Literal ID="ltrArtifactType" runat="server" Text='<%#((HistoryChangeSetView)(Container.DataItem)).ArtifactTypeName %>' />
                    <tstsc:HyperLinkEx ID="lnkArtifact" runat="server" onmouseover='<%# "lnkArtifact_mouseover(" + ((HistoryChangeSetView)(Container.DataItem)).ChangeSetId + ")" %>' onmouseout="lnkArtifact_mouseout()">
                        <asp:Literal runat="server" Text='<%#GlobalFunctions.GetTokenForArtifact(GlobalFunctions.GetPrefixForArtifactType((int)((HistoryChangeSetView)(Container.DataItem))["ArtifactTypeId"]), (((HistoryChangeSetView)(Container.DataItem)).ArtifactId), true) %>' /> -
                        <asp:Literal runat="server" Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((HistoryChangeSetView)(Container.DataItem)).ArtifactDesc) %>' />
                    </tstsc:HyperLinkEx>
                    <br />
                    <span class="Date">
                        <asp:Literal ID="ltrDate" runat="server" Text='<%#(GlobalFunctions.LocalizeDate((((HistoryChangeSetView)(Container.DataItem)).ChangeDate))).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow)) %>' />                
                    </span>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>
        <asp:ServiceReference Path="~/Services/Ajax/HistoryChangeSetService.svc" />
    </Services>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    var activityFeed_isOverNameDesc = false;
    function lnkArtifact_mouseover(changeSetId)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        activityFeed_isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        var projectId = <%=ProjectId%>;
        Inflectra.SpiraTest.Web.Services.Ajax.HistoryChangeSetService.RetrieveNameDesc(projectId, changeSetId, null, lnkArtifact_mouseover_success);

    }
    function lnkArtifact_mouseover_success (tooltipData)
    {
        if (activityFeed_isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    }
    function lnkArtifact_mouseout()
    {
        hideddrivetip();
        activityFeed_isOverNameDesc = false;
    }

</script>