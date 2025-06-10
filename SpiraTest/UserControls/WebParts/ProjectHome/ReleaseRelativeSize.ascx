<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReleaseRelativeSize.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseRelativeSize" %>

<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-RelativeSize"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div id="divWorkspaceRelativeSize" runat="server"></div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server"> 
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_reactC3Chart.js" />
        <asp:ScriptReference Path="~/TypeScript/rct_comp_workspaceRelativeSize.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>  

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-RelativeSize').popover({
        content: resx.InAppHelp_Chart_ReleaseRelativeSize,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData) {

        // set extra parameters to pass into props
        workspaceData.showSprints = workspaceData.displaySprints;

        //Get a handle to the div where React will render
        var divWorkspaceRelativeSize = $get('<%=divWorkspaceRelativeSize.ClientID%>');
        var relativeSizeComponent = React.createElement(WorkspaceRelativeSize, workspaceData, null);
        ReactDOM.render(relativeSizeComponent, divWorkspaceRelativeSize);
    };
    g_project_workspaceLoadCallbacks.push(loadData);
</script>
