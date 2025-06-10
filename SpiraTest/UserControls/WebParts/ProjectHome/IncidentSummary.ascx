<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentSummary.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentSummary" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<div class="df mb3">
    <span class="mr2 self-center">
        <asp:Localize runat="server" Text="<%$Resources:Main,IncidentSummary_DisplayingForIncidentType %>" />
    </span>
    <tstsc:DropDownListEx Runat="server" DataMember="IncidentType" DataTextField="Name" DataValueField="IncidentTypeId" ID="ddlIncidentTypeFilter" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll %>" MetaData="IncidentTypeId" CssClass="DropDownList" EnableViewState="true" AutoPostBack="true" Width="120px" />
</div>
<tstsc:GridViewEx id="grdIncidentSummary" Runat="server" DataMember="IncidentSummary" SkinID="WidgetGrid" ShowFooter="True">
	<HeaderStyle CssClass="SubHeader" />
	<FooterStyle CssClass="Footer" />
	<Columns>
		<tstsc:TemplateFieldEx>
			<HeaderStyle VerticalAlign="Middle"></HeaderStyle>
			<HeaderTemplate>
                 <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Status %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="Label2" Runat="server" Text='<%# ((DataRowView) Container.DataItem) ["IncidentStatusName"] %>' />
			</ItemTemplate>
			<FooterTemplate>
                 <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,TotalCaps %>" />
			</FooterTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
