<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OverallCompletion.ascx.cs"
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome.OverallCompletion" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-OverallCompletion"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div id="divWorkspaceOverallCompletion" runat="server"></div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server"> 
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_reactC3Chart.js" />
        <asp:ScriptReference Path="~/TypeScript/rct_comp_workspaceOverallCompletion.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>  

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-OverallCompletion').popover({
        content: resx.InAppHelp_Chart_PortfolioOverallCompletion,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData) {
        //Get a handle to the div where React will render
        var divWorkspaceOverallCompletion = $get('<%=divWorkspaceOverallCompletion.ClientID%>');

        //render the dashboard widget
        var overallCompletionComponent = React.createElement(
            WorkspaceOverallCompletion,
            { workspace: workspaceData.workspace },
            null
        );
        ReactDOM.render(overallCompletionComponent, divWorkspaceOverallCompletion);
    };
    g_portfolio_workspaceLoadCallbacks.push(loadData);
</script>
