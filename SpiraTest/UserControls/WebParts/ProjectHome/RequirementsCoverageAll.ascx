<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RequirementsCoverageAll.ascx.cs"
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementsCoverageAll" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-RequirementsCoverageRegression"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="u-chart db" id="c3_requirementCoverageAll">
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-RequirementsCoverageRegression').popover({
        content: resx.InAppHelp_Chart_RequirementsCoverageRegression,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var requirementsCoverageAllCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_requirementsCoverageAllGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_requirementsCoverageAllGraph(); });

    function load_requirementsCoverageAllGraph()
    {
        //Load the requirements regression coverage for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.Requirement_RetrieveTestCoverage(SpiraContext.ProjectId, releaseId, true, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_requirementCoverageAll')
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
                },
                selection: {
                    enabled: true
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

            requirementsCoverageAllCharts.requirementsCoverageAll = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>
