<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SnapshotGraphs.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SnapshotGraphs" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="helpSnapshotGraphs"  
    role="button" 
    runat="server"
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="graph-header">
    <div class="graph-label padding-next-to-dropdown pr1">
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Reports_GraphName %>" />:
    </div>
    <tstsc:DropDownListEx Runat="server" DataMember="Graph" DataTextField="Name" DataValueField="GraphId" ID="ddlGraphFilter"
        CssClass="DropDownList" />
    <asp:PlaceHolder runat="server" ID="plcIncidentTypeFilter">
        <div class="graph-label padding-next-to-dropdown pr1">
            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Dialogs,Global_Filter %>" />
        </div>
        <tstsc:DropDownListEx Runat="server" DataMember="IncidentType" DataTextField="Name" DataValueField="IncidentTypeId" ID="ddlIncidentType"
            NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll %>" SkinID="NarrowPlusControl" />
    </asp:PlaceHolder>
</div>
<div class="spacer" style="clear: both;"></div>
<tstsc:JqPlot runat="server" ID="jqSnapshotGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="SnapshotGraphs" />
<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#' + '<%=helpSnapshotGraphs.ClientID%>').popover({
        content: resx['InAppHelp_Chart_Snapshot' + '<%=ArtifactTypeId%>'],
        html: true,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });


    function <%=ddlGraphFilter.ClientID%>_selectedItemChanged(item)
    {
        var graphId = item.get_value();
        var graphName = item.get_text();
        var jqSnapshotGraph = $find('<%=jqSnapshotGraph.ClientID%>');
        jqSnapshotGraph.set_title(graphName);
        jqSnapshotGraph.set_graphId(graphId, true);
        jqSnapshotGraph.update_settings();
    }
    function <%=ddlIncidentType.ClientID%>_selectedItemChanged(item)
    {
        var incidentTypeId = item.get_value();
        var jqSnapshotGraph = $find('<%=jqSnapshotGraph.ClientID%>');
        var filters = {};
        if (incidentTypeId && incidentTypeId != '')
        {
            filters.IncidentTypeId = globalFunctions.serializeValueInt(incidentTypeId);
        }
        jqSnapshotGraph.set_filters(filters, true);
        jqSnapshotGraph.update_settings();
    }
</script>