<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RiskSummary.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RisksSummary" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx 
    id="grdRiskSummary" 
    Runat="server" 
    AutoGenerateColumns="false"
    GridLines="None"
    CssClass="WidgetGrid dt-fixed risk-summary-grid" 
    ShowHeader="true" 
    ShowSubHeader="true"
    >
	<Columns>
		<tstsc:TemplateFieldEx>
			<SubHeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,RiskImpactId %>" />
			</SubHeaderTemplate>
			<HeaderTemplate>
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label Runat="server" Text='<%#: ((RiskImpact) Container.DataItem).Name %>' ID="lblImpactName"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
