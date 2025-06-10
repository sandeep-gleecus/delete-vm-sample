<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SummaryGraphs.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SummaryGraphs" %>
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
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Reports_XAxis %>" />
    </div>
    <tstsc:DropDownListEx ID="ddlXAxis" SkinID="NarrowPlusControl"
        runat="server" DataValueField="Name" DataTextField="Caption" DataMember="ArtifactFields"
        NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" />
    <div class="graph-label padding-next-to-dropdown pr1">
        <tstsc:HyperLinkEx runat="server" ID="btnSwitchValues" CssClass="fas fa-exchange-alt btn btn-default"/>
    </div>
    <div class="graph-label padding-next-to-dropdown pr1">
        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Reports_GroupedBy %>" />
    </div>
    <tstsc:DropDownListEx ID="ddlGroupedBy" SkinID="NarrowPlusControl"
        runat="server" DataValueField="Name" DataTextField="Caption" DataMember="ArtifactFields"
        NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" />
</div>
<br style="clear: both" />
<tstsc:JqPlot runat="server" ID="jqSummaryGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="SummaryGraphs" />
<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#' + '<%=helpSummaryGraphs.ClientID%>').popover({
        content: resx['InAppHelp_Chart_Summary' + '<%=ArtifactTypeId%>'],
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    function <%=ddlXAxis.ClientID%>_selectedItemChanged(item)
    {
        var field = item.get_value();
        var jqSummaryGraph = $find('<%=jqSummaryGraph.ClientID%>');
        jqSummaryGraph.set_xAxisField(field, true);
        jqSummaryGraph.update_settings();
    }
    function <%=ddlGroupedBy.ClientID%>_selectedItemChanged(item)
    {
        var field = item.get_value();
        var jqSummaryGraph = $find('<%=jqSummaryGraph.ClientID%>');
        jqSummaryGraph.set_groupByField(field, true);
        jqSummaryGraph.update_settings();
    }
    function <%=btnSwitchValues.ClientID%>_click(evt)
    {
        //Switch the values of the dropdowns
        var ddlXAxis = $find('<%=ddlXAxis.ClientID%>');
        var ddlGroupedBy = $find('<%=ddlGroupedBy.ClientID%>');
        var value1 = ddlXAxis.get_selectedItem().get_value();
        var value2 = ddlGroupedBy.get_selectedItem().get_value();
        ddlXAxis.set_selectedItem(value2);
        ddlGroupedBy.set_selectedItem(value1);
    }
</script>