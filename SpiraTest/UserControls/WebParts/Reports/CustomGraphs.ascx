<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomGraphs.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.CustomGraphs" %>
<%@ Import Namespace="Microsoft.Security.Application" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="helpSummaryGraphs"  
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
    <div class="btn-group radio-group" role="group">
        <tstsc:DropDownListEx Runat="server" DataMember="Graph" DataTextField="Name" DataValueField="GraphId" ID="ddlGraphSelection"
            NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" SkinID="WideControl" />
        <asp:Label runat="server" AssociatedControlID="radDonutChart" CssClass="btn btn-default" ID="lblDonutChart">
            <tstsc:RadioButtonEx ID="radDonutChart" runat="server" GroupName="GraphType" ToolTip="<%$Resources:Main,Graphs_DonutChart %>" />
            <span class="fas fa-chart-pie"></span>                               
        </asp:Label>
        <asp:Label runat="server" AssociatedControlID="radBarChart" CssClass="btn btn-default" ID="lblBarChart">
            <tstsc:RadioButtonEx ID="radBarChart" runat="server" GroupName="GraphType" ToolTip="<%$Resources:Main,Graphs_BarChart %>" />
            <span class="fas fa-chart-bar"></span>                               
        </asp:Label>
        <asp:Label runat="server" AssociatedControlID="radLineChart" CssClass="btn btn-default" ID="lblLineChart">
            <tstsc:RadioButtonEx ID="radLineChart" runat="server" GroupName="GraphType" ToolTip="<%$Resources:Main,Graphs_LineChart %>" />
            <span class="fas fa-chart-line"></span>                               
        </asp:Label>
    </div>
</div>
<br style="clear: both" />
<tstsc:MessageBox runat="server" ID="msgCustomGraph" />
<tstsc:JqPlot runat="server" ID="jqCustomGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="CustomGraphs" />

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#' + '<%=helpSummaryGraphs.ClientID%>').popover({
        content: <%=Microsoft.Security.Application.Encoder.JavaScriptEncode(this.GraphName, true)%>,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    function <%=ddlGraphSelection.ClientID%>_selectedItemChanged(item)
    {
        var graphId = item.get_value();
        var graphName = item.get_text();
        var jqCustomGraph = $find('<%=jqCustomGraph.ClientID%>');
        jqCustomGraph.set_title(graphName);
        jqCustomGraph.set_customGraphId(graphId, true);
        jqCustomGraph.update_settings();
    }

    function <%=radBarChart.ClientID%>_click(e)
    {
        var jqCustomGraph = $find('<%=jqCustomGraph.ClientID%>');
        jqCustomGraph.set_customGraphType(jqCustomGraph.customGraphTypeEnum.bar, true);
        jqCustomGraph.update_settings();
        $('#<%=this.lblBarChart.ClientID%>').addClass('active');
        $('#<%=this.lblDonutChart.ClientID%>').removeClass('active');
        $('#<%=this.lblLineChart.ClientID%>').removeClass('active');
    }
    function <%=radDonutChart.ClientID%>_click(e)
    {
        var jqCustomGraph = $find('<%=jqCustomGraph.ClientID%>');
        jqCustomGraph.set_customGraphType(jqCustomGraph.customGraphTypeEnum.donut, true);
        jqCustomGraph.update_settings();
        $('#<%=this.lblBarChart.ClientID%>').removeClass('active');
        $('#<%=this.lblDonutChart.ClientID%>').addClass('active');
        $('#<%=this.lblLineChart.ClientID%>').removeClass('active');
    }
    function <%=radLineChart.ClientID%>_click(e)
    {
        var jqCustomGraph = $find('<%=jqCustomGraph.ClientID%>');
        jqCustomGraph.set_customGraphType(jqCustomGraph.customGraphTypeEnum.line, true);
        jqCustomGraph.update_settings();
        $('#<%=this.lblBarChart.ClientID%>').removeClass('active');
        $('#<%=this.lblDonutChart.ClientID%>').removeClass('active');
        $('#<%=this.lblLineChart.ClientID%>').addClass('active');
    }

</script>