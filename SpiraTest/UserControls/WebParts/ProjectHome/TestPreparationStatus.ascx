<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestPreparationStatus.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestPreparationStatus" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-testPreparationStatus"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>
<div class="df">
    <span class="mr2 self-center">
        <asp:Localize ID="Localize1" runat="server" Text="" />:
    </span>
</div>
 
 
<div class="col-md-8">
    <div class="u-chart db chart-click-thru" id="c3_testPreparationStatusSummary">
    </div>
</div>
<asp:HiddenField ID="hdnReleaseId" runat="server" />
<asp:HiddenField ID="hdnUseSeverity" runat="server" />
<asp:HiddenField ID="hdnUseResolvedRelease" runat="server" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>


<tstsc:JqPlot runat="server" ID="jqSnapshotGraph" GraphHeight="250px" GraphWidth="100%"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GraphingService" DataGridCssClass="DataGrid"
    GraphType="SnapshotGraphs" Visible="false" />

<script type="text/javascript">
	// Manage info popup
	var resx = Inflectra.SpiraTest.Web.GlobalResources;
	$('#help-testPreparationStatus').popover({
		content: resx.InAppHelp_Chart_RequirementGraphs,
		placement: "left",
		trigger: "hover focus",
		delay: { "show": 400, "hide": 100 }
	});


	    /* Charts */
    var testPreparationStatusSummaryChart = new Object;

        //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
	document.addEventListener("DOMContentLoaded", function () { load_testPrepStatusGraphs(); });
	Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_testPrepStatusGraphs(); });

	function load_testPrepStatusGraphs() {

		Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.TestCase_RetrieveTestPreparationStatusSummary(SpiraContext.ProjectId, function (data) {

			var categories = new Array();
			var columns = new Array();
            var colors = new Object();
			var finalTotal = 0;;

			config = {
				bindto: d3.select('#c3_testPreparationStatusSummary')
			};

			categories = categories.concat(data.Categories);
			for (var i = 0; i < data.Series.length; i++) {
				var colData = new Array();
				colData.push(data.Series[i].Name)
				 
				data.Series[i].IntegerValues.forEach(x => {
					colData.push(x);
					finalTotal += x;
				});

				columns.push(colData);
			}

			//C3js Bar Chart
			config.data = {
				columns: columns,
				type: 'bar',
				//color: function (color, d) {
				//	return colors[d.index];
				//},
				onclick: function (args) {
					 
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
				show: true
			};

			config.tooltip = {
				format: {
					value: function (value) {
						console.log('value is ' + value + ' total is ' + finalTotal);
						var percent = Math.round((value / finalTotal) * 100);
						var format = d3.format(',');
						return format(value) + ' (' + percent + '%)';
					}
				},
				grouped:false
			};

			 

			testPreparationStatusSummaryChart.testPreparationStatusSummary = c3.generate(config);

		}, function (ex) {
			//Log error
			var messageBox = $get('<%=this.MessageBoxClientID%>');
			globalFunctions.display_error(messageBox, ex);
		});

    }

</script>
