<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RequirementsSummary.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementsSummary" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdRequirementSummary" Runat="server" DataMember="RequirementSummary" SkinID="WidgetGrid" ShowFooter="True">
	<FooterStyle CssClass="Footer" />
	<Columns>
		<tstsc:TemplateFieldEx>
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Status %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label Runat="server" Text='<%# ((DataRowView) Container.DataItem) ["RequirementStatusName"] %>' ID="Label1"/>
			</ItemTemplate>
			<FooterTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,TotalCaps %>" />
			</FooterTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>