<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Schedule.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.Schedule" %>

<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-WorkspaceSchedule"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="dn-md-up db-sm alert alert-info">
    <asp:Literal
        runat="server"
        Text="<%$Resources:Messages,Gantt_CannotDisplayOnThisScreen %>"
        />
</div>
<div id="divWorkspaceSchedule" class="mt4 dn-sm" runat="server"></div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server"> 
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_reactGantt.js" />
        <asp:ScriptReference Path="~/TypeScript/rct_comp_workspaceSchedule.js" />
        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-gantt.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>  

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-WorkspaceSchedule').popover({
        content: resx.InAppHelp_Chart_ProductSchedule,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    var loadData = function (workspaceData) {
        workspaceData.isCustomTaskClass = true;
        workspaceData.productId = SpiraContext.ProjectId;
        var divWorkspaceSchedule = $get('<%=divWorkspaceSchedule.ClientID%>');
        var scheduleComponent = React.createElement(WorkspaceSchedule, workspaceData, null);
        ReactDOM.render(scheduleComponent, divWorkspaceSchedule);
    };
    g_project_workspaceLoadCallbacks.push(loadData);
</script>
