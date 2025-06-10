<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestSetStatus.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestSetStatus" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-TestSetStatus"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>
<table class="NoBorderTable inner-table" style="width: 100%">
	<tr>
		<td style="width: 100%">
            <div class="u-chart db chart-click-thru" id="c3_testSetExecutionStatus">
            </div>
            <asp:HiddenField ID="hdnReleaseId" runat="server" />
		</td>
		<td class="priority4" style="vertical-align: top">
            <tstsc:LabelEx ID="LabelEx1" runat="server" Font-Bold="true" Text="<%$Resources:Main,TestSetStatus_OverdueTestSets %>" />
            <br />
            <tstsc:GridViewEx EnableViewState="false" id="grdOverdueTestSets" Runat="server" ShowHeader="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService">
                <EmptyDataTemplate>
                    <span class="alert-success">
                        - <asp:Localize runat="server" Text="<%$Resources:Fields,None %>" /> -
                    </span>
                </EmptyDataTemplate>
                <Columns>
	                <tstsc:NameDescriptionFieldEx ItemStyle-Wrap="false" DataField="TestSetId" CommandName="ViewTestSet" CommandArgumentField="TestSetId"/>
	                <tstsc:TemplateFieldEx ItemStyle-Wrap="False" ItemStyle-HorizontalAlign="Center">
		                <ItemTemplate>
		                    <tstsc:LabelEx Runat="server" ID="lblPlannedDate"/>
		                </ItemTemplate>
	                </tstsc:TemplateFieldEx>
	            </Columns>
	        </tstsc:GridViewEx>
		</td>
	</tr>
</table>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-TestSetStatus').popover({
        content: resx.InAppHelp_Chart_TestSetStatus,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    /* Charts */
    var testSetExecutionCharts = new Object;

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_testSetGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_testSetGraph(); });

    function load_testSetGraph()
    {
        //Load the test execution status for all releases or specific release
        var releaseId = null;
        if ($('#<%=hdnReleaseId.ClientID%>').val() && $('#<%=hdnReleaseId.ClientID%>').val() != '')
        {
            releaseId = parseInt($('#<%=hdnReleaseId.ClientID%>').val());
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.TestSet_RetrieveExecutionSummary(SpiraContext.ProjectId, releaseId, function (data)
        {
            //Success
            //General options for charts
            config = {
                bindto: d3.select('#c3_testSetExecutionStatus')
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
                    var url = baseUrl + '?graph=testsetstatus&projectId=' + projectId + '&executionStatusId=' + executionStatusId;
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

            testSetExecutionCharts.testSetSummary = c3.generate(config);
        }, function (ex)
        {
            //Log error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }
</script>
