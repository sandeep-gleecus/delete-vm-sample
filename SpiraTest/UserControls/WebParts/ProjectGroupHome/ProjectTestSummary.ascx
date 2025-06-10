<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectTestSummary.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectTestSummary" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-Program-ProjectTestSummary"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<div class="px4 px0-sm ov-x-auto">
    <tstsc:GridViewEx ID="grdProjectExecutionStatus" Runat="server" EnableViewState="false" SkinID="WidgetGrid">
	    <Columns>
    		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			    <HeaderTemplate>
				    <asp:Localize runat="server" Text="<%$Resources:Fields,ProjectName %>" />
			    </HeaderTemplate>
			    <ItemTemplate>
			        <tstsc:ImageEx ImageUrl="Images/org-Project-Outline.svg" AlternateText="Product" ID="imgIcon" runat="server" />
                    <asp:Literal runat="server" Text="<%# ((ProjectView)(Container.DataItem)).ArtifactToken %>" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:NameDescriptionFieldEx HeaderText="<%$Resources:Fields,Project%>" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" DataField="Name" DescriptionField="Description" CommandArgumentField="ProjectId" HeaderColumnSpan="-1" />
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,EndDate%>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			    <ItemTemplate>
                    <asp:Literal runat="server" Text="<%# String.Format(GlobalFunctions.FORMAT_DATE, ((ProjectView)(Container.DataItem)).EndDate)%>" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Global_NumReqs %>" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3"/>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Coverage %>">
			    <ItemTemplate>
			        <tstsc:Equalizer runat="server" ID="eqlReqCoverage" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Global_NumTests%>" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ExecutionStatus%>">
			    <ItemTemplate>
			        <tstsc:Equalizer runat="server" ID="eqlExecutionStatus" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,NumOpenIncidents %>" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,OpenByPriority %>">
			    <ItemTemplate />
		    </tstsc:TemplateFieldEx>
	    </Columns>
    </tstsc:GridViewEx>
</div>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-Program-ProjectTestSummary').popover({
        content: resx.InAppHelp_Chart_Program_ProjectTestSummary,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });
</script>
