<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductCompletion.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProductCompletion" %>

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

<div id="divWorkspaceProductCompletion" runat="server"></div>

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
        content: resx.InAppHelp_Chart_ProgramWorkspaceCompletion,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData)
    {
        var divWorkspaceProductCompletion = $get('<%=divWorkspaceProductCompletion.ClientID%>');
        var productCompletionComponent = React.createElement(WorkspaceProductCompletion, workspaceData, null);
        ReactDOM.render(productCompletionComponent, divWorkspaceProductCompletion);
    };
    g_projectGroup_workspaceLoadCallbacks.push(loadData);
</script>
