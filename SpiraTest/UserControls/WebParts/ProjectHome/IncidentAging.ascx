<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentAging.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentAging" %>

<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-IncidentAging"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>
<div class="u-chart db chart-click-thru" id="c3_incidentAging">
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />
<asp:HiddenField ID="hdnMaxAging" runat="server" />
<asp:HiddenField ID="hdnTimeInterval" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-IncidentAging').popover({
        content: resx.InAppHelp_Chart_IncidentAging,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var incidentAgingCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_incidentAgingGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_incidentAgingGraph(); });

    function load_incidentAgingGraph()
    {
        //Load the requirements coverage for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        var maximumAge = parseInt($('#<%=hdnMaxAging.ClientID%>').val());
        var ageInterval = parseInt($('#<%=hdnTimeInterval.ClientID%>').val());
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveAging(SpiraContext.ProjectId, releaseId, maximumAge, ageInterval, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_incidentAging')
            };

            //Collections
            var categories = new Array();
            var columns = new Array();

            var column = new Array();
            column.push(resx.Global_Count);
            columns.push(column);
            var total = 0;

            //Convert the data
            for (var i = 0; i < data.length; i++)
            {
                var caption = data[i].caption;
                var count = data[i].count;

                categories.push(caption);
                column.push(count);

                total += count;
            }

            //C3js Bar Chart
            config.data = {
                columns: columns,
                type: 'bar',
                color: function () { return '#d0d0d0'; },
                onclick: function (args)
                {
                    var projectId = SpiraContext.ProjectId;
                    var category = data[args.index].name;
                    var baseUrl = '<%=RedirectBaseUrl%>';
                    var url = baseUrl + '?graph=incidentaging&projectId=' + projectId + '&category=' + encodeURIComponent(category);
                    if (releaseId)
                    {
                        url += '&releaseId=' + releaseId;
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

            incidentAgingCharts.incidentAging = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>

