<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProgramCompletion.ascx.cs"
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome.ProgramCompletion" %>

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

<div id="divWorkspaceProgramCompletion" runat="server"></div>

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
        content: resx.InAppHelp_Chart_PortfolioWorkspaceCompletion,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData)
    {
        var divWorkspaceProgramCompletion = $get('<%=divWorkspaceProgramCompletion.ClientID%>');
        var programCompletionComponent = React.createElement(WorkspaceProductCompletion, workspaceData, null);
        ReactDOM.render(programCompletionComponent, divWorkspaceProgramCompletion);
    };
    g_portfolio_workspaceLoadCallbacks.push(loadData);
</script>
