<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentOpenCount.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentOpenCount" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-IncidentOpenCount"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="col-md-4">
    <div class="u-chart db chart-click-thru" id="c3_incidentOpenClosedCount">
    </div>
</div>
<div class="col-md-8">
    <div class="u-chart db chart-click-thru" id="c3_incidentOpenCountByPrioritySeverity">
    </div>
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />
<asp:HiddenField ID="hdnUseSeverity" runat="server" />
<asp:HiddenField ID="hdnUseResolvedRelease" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-IncidentOpenCount').popover({
        content: resx.InAppHelp_Chart_IncidentOpenCount,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var incidentOpenCountCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_incidentOpenCountGraphs(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_incidentOpenCountGraphs(); });

    function load_incidentOpenCountGraphs()
    {
        //Load the requirements coverage for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        var useSeverity = ($('#<%=hdnUseSeverity.ClientID%>').val() == 'true');
        var useResolvedRelease = ($('#<%=hdnUseResolvedRelease.ClientID%>').val() == 'true');

        /* Open/Closed Count */
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveCountByOpenClosedStatus(SpiraContext.ProjectId, releaseId, useResolvedRelease, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_incidentOpenClosedCount')
            };

            //Collections
            var columns = new Array();
            var colors = new Object();
            var total = 0;

            //Convert the data
            for (var i = 0; i < data.Series.length; i++)
            {
                var caption = data.Series[i].Caption;
                var count = data.Series[i].Value;
                var color = data.Series[i].Color;

                var column = new Array();
                column.push(caption);
                column.push(count);
                columns.push(column);

                if (color)
                {
                    colors[caption] = '#' + color;
                }
                else
                {
                    colors[caption] = '#eeeeee';
                }

                total += count;
            }

            //C3js Donut Chart
            config.data = {
                columns: columns,
                colors: colors,
                type: 'donut',
                onclick: function (args)
                {
                    var projectId = SpiraContext.ProjectId;
                    var category = data.Series[args.index].Caption;
                    var baseUrl = '<%=RedirectBaseUrl%>';
                    var url = baseUrl + '?graph=incidentopenclosedcount&projectId=' + projectId + '&category=' + encodeURIComponent(category);
                    if (releaseId)
                    {
                        url += '&releaseId=' + releaseId;
                        if (useResolvedRelease)
                        {
                            url += "&useResolvedRelease=true";
                        }
                    }
                    window.location = url;
                },
                selection: {
                    enabled: true
                }
            };
            config.donut = {
                label: {
                    format: function (value, ratio, id)
                    {
                        return d3.format('d')(value);
                    }
                }
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

            incidentOpenCountCharts.incidentOpenClosedCount = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });

        /* Incident Open Count by Priority / Severity */
        var fn = Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveCountByPriority;
        if (useSeverity)
        {
            fn = Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveCountBySeverity;
        }
        fn(SpiraContext.ProjectId, releaseId, useResolvedRelease, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_incidentOpenCountByPrioritySeverity')
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
            for (var i = 0; i < data.Series.length; i++)
            {
                var caption = data.Series[i].Caption;
                var count = data.Series[i].Value;
                var color = data.Series[i].Color;

                categories.push(caption);
                column.push(count);

                if (color)
                {
                    colors[i] = '#' + color;
                }
                else
                {
                    colors[i] = '#eeeeee';
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
                onclick: function (args)
                {
                    var projectId = SpiraContext.ProjectId;
                    var category = data.Series[args.index].Caption;
                    var baseUrl = '<%=RedirectBaseUrl%>';
                    var url = baseUrl + '?graph=incidentopencountbypriority&projectId=' + projectId + '&category=' + encodeURIComponent(category);
                    if (useSeverity)
                    {
                        url = baseUrl + '?graph=incidentopencountbyseverity&projectId=' + projectId + '&category=' + encodeURIComponent(category);
                    }
                    if (releaseId)
                    {
                        url += '&releaseId=' + releaseId;
                        if (useResolvedRelease)
                        {
                            url += "&useResolvedRelease=true";
                        }
                    }
                    window.location = url;
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

            incidentOpenCountCharts.incidentOpenCountByPrioritySeverity = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>
