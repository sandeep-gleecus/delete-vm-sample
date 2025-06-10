<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestExecutionStatus.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.TestExecutionStatus" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-TestExecutionStatusProgram"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<asp:PlaceHolder ID="plcGroupSummaryGraph" runat="server">
    <div class="u-chart db" id="c3_testExecutionStatus">
    </div>
</asp:PlaceHolder>
<div style="padding-left:30px; padding-right: 15px;">
    <tstsc:GridViewEx ID="grdProjectExecutionStatus" Runat="server" EnableViewState="false" SkinID="WidgetGrid">
	    <Columns>
            <tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			    <HeaderTemplate>
				    <asp:Localize runat="server" Text="<%$Resources:Fields,ProjectName %>" />
			    </HeaderTemplate>
			    <ItemTemplate>
			        <tstsc:ImageEx ImageUrl="Images/org-Project-Outline.svg" AlternateText="Product" ID="imgIcon" runat="server" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:NameDescriptionFieldEx HeaderText="<%$Resources:Fields,Project%>" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" DataField="Name" DescriptionField="Description" CommandArgumentField="ProjectId" HeaderColumnSpan="-1" />
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Global_NumTests%>" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Global_NumRuns%>" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3"/>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ExecutionStatus%>">
			    <ItemTemplate>
			        <tstsc:Equalizer runat="server" ID="eqlExecutionStatus" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
	    </Columns>
    </tstsc:GridViewEx>
</div>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-TestExecutionStatusProgram').popover({
        content: resx.InAppHelp_Chart_TestExecutionStatusProgram,
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
        //Get the active/all releases setting
        var activeReleasesOnly = <%=this.activeReleasesOnly.ToString().ToLowerInvariant()%>;

        //Load the test execution status for the entire group
        Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.TestCase_RetrieveGroupExecutionSummary(SpiraContext.ProjectGroupId, activeReleasesOnly, function (data)
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
