<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuickLaunch.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.QuickLaunch" %>
<div class="flex-container flex-wrap">
    <div class="mr3 display-inline-block padding-next-to-dropdown">
        <asp:Localize runat="server" Text="<%$Resources:Main,QuickLaunch_CreateIncidentIn %>" />
    </div>
    <div class="launcher launcher-web-part display-inline-block">
        <tstsc:DropDownListEx ID="ddlChooseProject" runat="server" NoValueItem="true"
	        NoValueItemText="<%$Resources:Dialogs,Global_ChooseProject%>" DataTextField="Name" AutoPostBack="false"
	        DataValueField="ProjectId" DataMember="Project" ClientScriptMethod="ddlChooseProject_changed" />
        <div class="btn-launch">
            <tstsc:HyperLinkEx runat="server" ID="lnkCreateIncident" ToolTip="<%$Resources:Buttons,Create %>">
                <span class=" fas fa-plus" />
            </tstsc:HyperLinkEx>
        </div>
    </div>
</div>
<script language="javascript" type="text/javascript">
    //Called when the dropdown is loaded
    function ddlChooseProject_loaded()
    {
        var ddlChooseProject = $find('<%=ddlChooseProject.ClientID %>');
        var lnkCreateIncident = $get('<%=lnkCreateIncident.ClientID %>');
        if (ddlChooseProject.get_selectedItem() && ddlChooseProject.get_selectedItem().get_value())
        {
            var projectId = ddlChooseProject.get_selectedItem().get_value();
            var url = '<%=CreateIncidentBaseUrl %>';
            lnkCreateIncident.href = url.replace(globalFunctions.projectIdToken, projectId);
        }
        else
        {
            lnkCreateIncident.href = 'javascript:void(0)';
        }
    }

    //Called when the project is changed
    function ddlChooseProject_changed(item)
    {
        var lnkCreateIncident = $get('<%=lnkCreateIncident.ClientID %>');
        if (item.get_value())
        {
            var projectId = item.get_value();
            var url = '<%=CreateIncidentBaseUrl %>';
            lnkCreateIncident.href = url.replace(globalFunctions.projectIdToken, projectId);
        }
        else
        {
            lnkCreateIncident.href = 'javascript:void(0)';
        }
    }
</script>