<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OpenIssues.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.OpenIssues" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdIssueList" EnableViewState="false" Runat="server" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Description %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-Incident.svg" AlternateText="Issue" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderText="<%$Resources:Fields,Description%>" DataField="Name" NameMaxLength="60" CommandArgumentField="IncidentId" NavigateUrlFormat="#" HeaderColumnSpan="-1" />
		<tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,Project%>" DataField="ProjectName" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Width="75px">
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Priority%>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="lblPriority" Runat="server" Text='<%# ((IncidentView) Container.DataItem).PriorityName %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Width="75px">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Severity%>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="lblSeverity" Runat="server" Text='<%# ((IncidentView) Container.DataItem).SeverityName %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderStyle-Width="90px" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,DateOpened%>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx ID="lblDateOpened" Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).CreationDate)) %>'
                    Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).CreationDate)) %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>