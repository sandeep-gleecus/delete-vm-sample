<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReleaseCompletion.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseCompletion" %>

<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-WorkspaceCompletion"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div id="divWorkspaceReleaseCompletion" runat="server"></div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server"> 
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_reactC3Chart.js" />
        <asp:ScriptReference Path="~/TypeScript/rct_comp_workspaceProductCompletion.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>  

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-WorkspaceCompletion').popover({
        content: resx.InAppHelp_Chart_ProductWorkspaceCompletion,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData)
    {
        // set extra parameters to pass into props
        workspaceData.hideEmptyWorkspaces = true;
        workspaceData.showSprints = workspaceData.displaySprints;

        var divWorkspaceReleaseCompletion = $get('<%=divWorkspaceReleaseCompletion.ClientID%>');
        var releaseCompletionComponent = React.createElement(WorkspaceProductCompletion, workspaceData, null);
        ReactDOM.render(releaseCompletionComponent, divWorkspaceReleaseCompletion);
    };
    g_project_workspaceLoadCallbacks.push(loadData);
</script>
