<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestCasesPerStatusAndDay.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestCasesPerStatusAndDay" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-TestCasesPerStatusAndDay"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>
<div class="u-chart db" id="c3_testCasesPerStatusDay">
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />
<asp:HiddenField ID="hdnDateRange" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/GraphingService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-TestCasesPerStatusAndDay').popover({
        content: resx.InAppHelp_Chart_TestCasesPerStatusAndDay,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });


    /* Charts */
    var sidepanelCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_testCasesPerStatusDay(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_testCasesPerStatusDay(); });

    function load_testCasesPerStatusDay()
    {
        //Load the count of test runs by dates
        var dateFormatMonthFirst = <%=DateFormatMonthFirst.ToString().ToLowerInvariant()%>;
        var graphId = <%=(int)Inflectra.SpiraTest.DataModel.Graph.GraphEnum.TestCasesStatusRateCumulative%>;
        var dateRange = $('#<%=hdnDateRange.ClientID%>').val();
        var releaseId = $('#<%=hdnReleaseId.ClientID%>').val();
        var filters = {};
        if (releaseId && releaseId != '')
        {
            filters.ReleaseId = globalFunctions.serializeValueInt(releaseId);
        }
        Inflectra.SpiraTest.Web.Services.Ajax.GraphingService.RetrieveDateRange(SpiraContext.ProjectId, graphId, dateRange, filters, function (data)
        {
            //Success
            var cleanedData = prepDataForC3(data);

            sidepanelCharts.testCasesPerStatusDay = c3.generate({
                bindto: d3.select('#c3_testCasesPerStatusDay'),
                data: {
                    x: 'x',
                    columns: cleanedData.columns,
                    colors: cleanedData.colors,
                    type: "area-spline",
                    groups: cleanedData.groups
                },
                grid: {
                    y: {
                        show: true
                    }
                },
                axis: {
                    x: {
                        type: 'timeseries',
                        tick: {
                            format: function (x) {
                                if (dateFormatMonthFirst)
                                    return (x.getMonth()+1) + '/' + x.getDate();
                                else
                                    return x.getDate() + '/' + (x.getMonth()+1);
                            }
                        }
                    }
                }
            });
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });

        function prepDataForC3(data)
        {
            var res = new Object();
            if (data && data.Series)
            {
                res.columns = [['x']];
                res.colors = new Object;
                res.groups = [[]];
                for (var j = 0; j < data.XAxis.length; j++)
                {
                    //Convert the WCF date
                    var date = new Date(parseInt(data.XAxis[j].DateValue.substr(6)));
                    res.columns[0].push(date);
                }
                for (var i = 0; i < data.Series.length; i++)
                {
                    var column = [data.Series[i].Caption];
                    var values = data.Series[i].Values;
                    res.groups[0].push(data.Series[i].Caption);
                    for (var key in values)
                    {
                        if (key != '__type')
                        {
                            column.push(values[key]);
                        }
                    }
                    res.columns.push(column);
                    res.colors[data.Series[i].Caption] = "#" + (data.Series[i].Color == null ? "f1f1f1" : data.Series[i].Color);
                }
            }
            return res;
        }
    }
</script>