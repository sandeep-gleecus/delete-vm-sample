<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DateRangeGraphs.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.DateRangeGraphs" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="helpDateRangeGraphs"  
    role="button" 
    runat="server"
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="graph-header">
    <div class="graph-label padding-next-to-dropdown pr1">
        <asp:Localize runat="server" Text="<%$Resources:Main,Reports_GraphName %>" />:
    </div>
    <tstsc:DropDownListEx Runat="server" DataMember="Graph" DataTextField="Name" DataValueField="GraphId" ID="ddlGraphFilter"
        CssClass="DropDownList" />
    <asp:PlaceHolder runat="server" ID="plcIncidentTypeFilter">
        <div class="graph-label padding-next-to-dropdown pr1">
            <asp:Localize runat="server" Text="<%$Resources:Dialogs,Global_Filter %>" />
        </div>
        <tstsc:DropDownListEx Runat="server" DataTextField="Name" DataValueField="IncidentTypeId" ID="ddlIncidentType"
            NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll %>" />
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="plcTestCaseTypeFilter">
        <div class="graph-label padding-next-to-dropdown pr1">
            <asp:Localize runat="server" Text="<%$Resources:Dialogs,Global_Filter %>" />
        </div>
        <tstsc:DropDownListEx Runat="server" DataTextField="Name" DataValueField="TestCaseTypeId" ID="ddlTestCaseType"
            NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll %>" />
    </asp:PlaceHolder>
    <div class="pull-right">
        <div class="graph-label pull-left padding-next-to-dropdown">
            <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Reports_DateRange %>" />:
        </div>
        <tstsc:DateRangeFilter ID="datDateRange" runat="server" CssClass="DatePicker control-far-right" />
    </div>
</div>
<div class="spacer" style="clear: both;"></div>
<tstsc:JqPlot runat="server" ID="jqDateRangeGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="DateRangeGraphs" />
<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;

    $('#' + '<%=helpDateRangeGraphs.ClientID%>').popover({
        content: resx['InAppHelp_Chart_DateRange' + '<%=ArtifactTypeId%>'],
        html: true,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });
    
    function <%=ddlGraphFilter.ClientID%>_selectedItemChanged(item)
    {
        var graphId = item.get_value();
        var graphName = item.get_text();
        var jqDateRangeGraph = $find('<%=jqDateRangeGraph.ClientID%>');
        jqDateRangeGraph.set_title(graphName);
        jqDateRangeGraph.set_graphId(graphId, true);
        jqDateRangeGraph.update_settings();
    }
    function <%=ddlIncidentType.ClientID%>_selectedItemChanged(item)
    {
        var incidentTypeId = item.get_value();
        var jqDateRangeGraph = $find('<%=jqDateRangeGraph.ClientID%>');
        var filters = {};
        if (incidentTypeId && incidentTypeId != '')
        {
            filters.IncidentTypeId = globalFunctions.serializeValueInt(incidentTypeId);
        }
        jqDateRangeGraph.set_filters(filters, true);
        jqDateRangeGraph.update_settings();
    }
    function <%=ddlTestCaseType.ClientID%>_selectedItemChanged(item)
    {
        var testCaseTypeId = item.get_value();
        var jqDateRangeGraph = $find('<%=jqDateRangeGraph.ClientID%>');
        var filters = {};
        if (testCaseTypeId && testCaseTypeId != '')
        {
            filters.TestCaseTypeId = globalFunctions.serializeValueInt(testCaseTypeId);
        }
        jqDateRangeGraph.set_filters(filters, true);
        jqDateRangeGraph.update_settings();
    }
    function <%=datDateRange.ClientID%>_updated(dateRange)
    {
        var jqDateRangeGraph = $find('<%=jqDateRangeGraph.ClientID%>');
        jqDateRangeGraph.set_dateRange(dateRange, true);
        jqDateRangeGraph.update_settings();
    }
</script>