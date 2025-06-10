<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentTestCoverage.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentTestCoverage" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-IncidentTestCoverage"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="u-chart db" id="c3_incidentTestCoverage">
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-IncidentTestCoverage').popover({
        content: resx.InAppHelp_Chart_IncidentTestCoverage,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var incidentTestCoverageCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_incidentTestCoverageGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_incidentTestCoverageGraph(); });

    function load_incidentTestCoverageGraph()
    {
        //Load the test execution status for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveTestCoverage(SpiraContext.ProjectId, releaseId, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_incidentTestCoverage')
            };

            //Collections
            var categories = new Array();
            var columns = new Array();
            var colors = new Object();

            var column = new Array();
            column.push(resx.Global_Count);
            columns.push(column);
            var total = 0;

            //Convert the data
            for (var i = 0; i < data.length; i++)
            {
                var caption = data[i].caption;
                var count = data[i].count;
                var color = data[i].color;

                categories.push(caption);
                column.push(count);

                if (color)
                {
                    colors[i] = '#' + color;
                }
                total += count;
            }

            //C3js Bar Chart
            config.data = {
                columns: columns,
                type: 'bar',
                color: function (color, d)
                {
                    return colors[d.index];
                }
            };
            config.axis = {
                x: {
                    type: 'category',
                    categories: categories
                }
            };
            config.grid = {
                y: {
                    show: true
                }
            };
            config.bar = {
                width: {
                    ratio: 0.8
                }
            };
            config.legend = {
                show: false
            };

            config.tooltip = {
                format: {
                    value: function (value)
                    {
                        var percent = Math.round((value / total) * 100);
                        var format = d3.format(',');
                        return format(value) + ' (' + percent + '%)';
                    }
                }
            };

            incidentTestCoverageCharts.incidentTestCoverage = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>
