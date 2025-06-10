<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestExecutionStatus.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestExecutionStatus" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-TestExecutionStatus"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<table class="NoBorderTable inner-table" style="width:100%">
	<tr>
		<td>
            <div class="u-chart db chart-click-thru" id="c3_testExecutionStatus">
            </div>
            <asp:HiddenField ID="hdnReleaseId" runat="server" />
		</td>
		<td class="priority4">
            <tstsc:LabelEx runat="server" Font-Bold="true" Text="<%$Resources:Main,TestExecutionStatus_TotalNumRuns %>" />
			<tstsc:LabelEx id="lblTotalRunCount" Runat="server" /><br />
			<br />
            <tstsc:LabelEx ID="LabelEx2" runat="server" Font-Bold="true" Text="<%$Resources:Main,TestExecutionStatus_DailyRunCount %>" />
            <br />
			<asp:datalist id="lstDailyRunCount" Runat="server" CssClass="NoBorderTable">
				<ItemTemplate>
                    <tstsc:LinkButtonEx SkinID="ButtonLink" runat="server" CommandName="TestRunList" CommandArgument='<%#((TestRun_DailyCount)Container.DataItem).ExecutionDate%>'>
					<%# String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, ((TestRun_DailyCount)Container.DataItem).ExecutionDate)%>
                    </tstsc:LinkButtonEx>
					<span class="badge">
					    <%# ((TestRun_DailyCount) Container.DataItem).ExecutionCount%>
                    </span>
					<br />
				</ItemTemplate>
			</asp:datalist>
		</td>
	</tr>
</table>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-TestExecutionStatus').popover({
        content: resx.InAppHelp_Chart_TestExecutionStatus,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var testExecutionCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_testCaseGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_testCaseGraph(); });

    function load_testCaseGraph()
    {
        //Load the test execution status for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.TestCase_RetrieveExecutionSummary(SpiraContext.ProjectId, releaseId, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_testExecutionStatus')
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
                onclick: function (args)
                {
                    var projectId = SpiraContext.ProjectId;
                    var executionStatusId = data[args.index].name;
                    var baseUrl = '<%=RedirectBaseUrl%>';
                    var url = baseUrl + '?graph=testexecutionstatus&projectId=' + projectId + '&executionStatusId=' + executionStatusId;
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

            testExecutionCharts.testCaseSummary = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>
