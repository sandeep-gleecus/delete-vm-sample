<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentAging.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.IncidentAging" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-IncidentAging"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<asp:PlaceHolder ID="plcIncidentAging" runat="server">
    <div class="u-chart db chart-click-thru" id="c3_incidentAging">
    </div>
    <asp:HiddenField ID="hdnMaxAging" runat="server" />
    <asp:HiddenField ID="hdnTimeInterval" runat="server" />
</asp:PlaceHolder>
<div style="padding-left:30px; padding-right: 15px;">
    <tstsc:GridViewEx ID="grdProjectIncidentOpenCount" Runat="server" EnableViewState="false" SkinID="WidgetGrid">
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
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,NumberOpen %>" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,OpenByPriority %>">
			    <ItemTemplate />
		    </tstsc:TemplateFieldEx>
	    </Columns>
    </tstsc:GridViewEx>
</div>

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
        //Load the requirements coverage for the group
        var maximumAge = parseInt($('#<%=hdnMaxAging.ClientID%>').val());
        var ageInterval = parseInt($('#<%=hdnTimeInterval.ClientID%>').val());
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_RetrieveGroupAging(SpiraContext.ProjectGroupId, maximumAge, ageInterval, function (data)
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
