<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.ProjectList" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdProjectList" runat="server" SkinId="WidgetGrid">
	<SelectedRowStyle CssClass="Selected" />
	<Columns>
	    <tstsc:NameDescriptionFieldEx HeaderText="<%$Resources:Fields,Project %>" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" NavigateUrlFormat="~/{0}.aspx" DataField="ProjectName" DescriptionField="ProjectDescription" CommandArgumentField="ProjectId"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
		<tstsc:TemplateFieldEx>
			<HeaderStyle Wrap="False" />
			<ItemStyle Wrap="False" />
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Program %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:HyperlinkEx ID="lnkProjectGroup" runat="server"><%#:((UserRecentProject)Container.DataItem).Project.Group.Name%></tstsc:HyperlinkEx>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" Visible="false">
			<HeaderStyle Wrap="False" />
			<ItemStyle Wrap="True" />
			<HeaderTemplate>
                <asp:Localize runat="server" Text="<%$Resources:Fields,Portfolio %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:HyperlinkEx ID="lnkPortfolio" runat="server"><%#:((UserRecentProject)Container.DataItem).Project.Group.PortfolioName%></tstsc:HyperlinkEx>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
