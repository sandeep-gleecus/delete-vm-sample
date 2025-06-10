<%@ Page
	Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="LoginProviders.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.LoginProviders"
	Title="Untitled Page" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h2>
		<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_OAuthList_Title %>" />
	</h2>
	<div class="Spacer"></div>
	<p>
		<asp:Literal ID="litSummary" runat="server" Text="<%$ Resources:Main,Admin_OAuthList_Summary %>" />
	</p>
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

	<tstsc:GridViewEx runat="server"
		ID="grdproviders"
		CssClass="DataGrid"
		PageSize="50"
		Width="100%"
		AutoGenerateColumns="False"
		EnableViewState="false">
		<HeaderStyle CssClass="Header" />
		<SubHeaderStyle CssClass="SubHeader" />

		<Columns>
			<tstsc:FilterSortFieldEx DataField="Name"
				HeaderText="<%$ Resources:Fields,Name %>"
				ItemStyle-CssClass="priority3"
				HeaderStyle-CssClass="priority3 nowrap" />

			<tstsc:FilterSortFieldEx DataField="ClientId"
				HeaderText="<%$ Resources:Fields,ClientId %>"
				ItemStyle-CssClass="priority3"
				HeaderStyle-CssClass="priority3 nowrap" />

			<tstsc:TemplateFieldEx
				HeaderText="<%$ Resources:Fields,ActiveYn %>"
				ItemStyle-CssClass="priority3"
				HeaderStyle-CssClass="priority3 nowrap">
				<ItemTemplate>
					<asp:Literal runat="server" Text="<%# ((GlobalOAuthProvider) Container.DataItem).IsActive? Resources.Main.Global_Yes : Resources.Main.Global_No %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>

			<tstsc:TemplateFieldEx
				HeaderText="<%$ Resources:Fields,LoadedYn %>"
				ItemStyle-CssClass="priority2"
				HeaderStyle-CssClass="priority2 nowrap">
				<ItemTemplate>
					<asp:Literal runat="server" Text="<%# ((GlobalOAuthProvider) Container.DataItem).IsLoaded? Resources.Main.Global_Yes : Resources.Main.Global_No %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>

			<tstsc:FilterSortFieldEx DataField="Users.Count"
				HeaderText="<%$ Resources:Fields,NumUsers %>"
				ItemStyle-CssClass="priority1"
				HeaderStyle-CssClass="priority1 nowrap" />

			<tstsc:TemplateFieldEx
				HeaderText="<%$ Resources:Fields,Operations %>"
				ItemStyle-CssClass="priority1"
				HeaderStyle-CssClass="priority1 nowrap">
				<ItemTemplate>
					<tstsc:HyperLinkEx runat="server"
						SkinID="ButtonDefault"
						NavigateUrl='<%# "LoginProviders_Edit.aspx?id=" + ((GlobalOAuthProvider) Container.DataItem).OAuthProviderId.ToString() %>'
						Text="<%$ Resources:Buttons,Edit %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
</asp:Content>
