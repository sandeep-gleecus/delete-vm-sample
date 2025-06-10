<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="TaskGraphs.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TaskGraphs" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-TaskGraphs"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>
<div class="df">
    <span class="mr2 self-center">
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Reports_GraphName %>" />:
    </span>
    <tstsc:DropDownListEx Runat="server" DataMember="Graph" DataTextField="Name" DataValueField="GraphId" ID="ddlGraphFilter"
        CssClass="DropDownList" Width="250px" />
</div>
<br style="clear: both" />
<tstsc:JqPlot runat="server" ID="jqSnapshotGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="SnapshotGraphs" />
<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-TaskGraphs').popover({
        content: resx.InAppHelp_Chart_TaskGraphs,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    })

    function <%=ddlGraphFilter.ClientID%>_selectedItemChanged(item)
    {
        var graphId = item.get_value();
        var graphName = item.get_text();
        var jqSnapshotGraph = $find('<%=jqSnapshotGraph.ClientID%>');
        jqSnapshotGraph.set_graphId(graphId, true);
        jqSnapshotGraph.update_settings();
    }
</script>
