<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
	CodeBehind="UserList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.UserList"
	Title="Untitled Page" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h2>
		<asp:Localize runat="server" Text="<%$Resources:Main,UserList_Title %>" />
		<small>
			<tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkUserLdapImport"
				runat="server" Text="<%$Resources:Main,UserList_ImportFromLDAP %>" NavigateUrl="UserLdapImport.aspx" />
		</small>
	</h2>

	<p>
		<asp:Localize runat="server" Text="<%$Resources:Main,UserList_Legend1 %>" />
	</p>
	
    <p>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,UserList_Legend2 %>" />
	</p>
	
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage" DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />

	<div class="TabControlHeader mt6">
		<div class="btn-group priority1">
			<tstsc:DropMenu ID="btnUserListAdd" GlyphIconCssClass="mr3 fas fa-plus" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add%>" />
		</div>
		<div class="btn-group priority3">
			<tstsc:DropMenu ID="btnUserListFilter" GlyphIconCssClass="mr3 fas fa-filter" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
			<tstsc:DropMenu ID="btnUserListClearFilters" GlyphIconCssClass="mr3 fas fa-times" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter%>" />
		</div>

		<div class="dib pl5">
			<div class="display-inline-block mr3">
				<strong>
					<asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>" />:</strong>
			</div>
			<tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
				<asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
				<asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
				<asp:ListItem Text="<%$Resources:Main,Global_Inactive %>" Value="allinactive" />
			</tstsc:DropDownListEx>
		</div>
	</div>


	<tstsc:GridViewEx ID="grdUserManagement" CssClass="DataGrid" runat="server" PageSize="15"
		AllowSorting="true" AllowCustomPaging="True" AllowPaging="True" ShowSubHeader="True"
		Width="100%" AutoGenerateColumns="False" EnableViewState="false">
		<HeaderStyle CssClass="Header" />
		<SubHeaderStyle CssClass="SubHeader" />
		<PagerStyle CssClass="Pagination" />
		<Columns>
			<tstsc:FilterSortFieldEx DataField="Profile.FirstName" HeaderText="<%$Resources:Fields,FirstName %>" FilterField="Profile.FirstName"
				FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3 nowrap" />
			<tstsc:FilterSortFieldEx DataField="Profile.MiddleInitial" HeaderText="<%$Resources:Fields,MiddleInitialAbbreviation %>" FilterField="Profile.MiddleInitial"
				FilterType="TextBox" Sortable="true" FilterWidth="25px" />
			<tstsc:FilterSortFieldEx DataField="Profile.LastName" HeaderText="<%$Resources:Fields,LastName %>" FilterField="Profile.LastName"
				FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3 nowrap" />
			<tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$Resources:Fields,UserName %>" FilterField="UserName"
				FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2 nowrap" />
			<tstsc:FilterSortFieldEx DataField="Profile.IsAdmin" HeaderText="<%$Resources:Fields,Admin %>" FilterField="Profile.IsAdmin"
				FilterType="Flag" FilterLookupDataField="Key" FilterLookupTextField="Value" FilterWidth="70px"
				Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1 nowrap" />
			<tstsc:FilterSortFieldEx DataField="Profile.IsEmailEnabled" HeaderText="<%$Resources:Fields,EmailEnabled %>" FilterField="Profile.IsEmailEnabled"
				FilterType="Flag" FilterLookupDataField="Key" FilterLookupTextField="Value" FilterWidth="70px"
				Sortable="true" />
			<tstsc:FilterSortFieldEx DataField="Profile.Department" HeaderText="<%$Resources:Fields,Department %>" FilterField="Profile.Department"
				FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2 nowrap" />
			<tstsc:FilterSortFieldEx DataField="Profile.Organization" HeaderText="<%$Resources:Fields,Organization %>" FilterField="Profile.Organization"
				FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4 nowrap" />
			<tstsc:FilterSortFieldEx DataField="UserId" HeaderText="<%$Resources:Fields,UserId %>" FilterField="UserId"
				FilterType="TextBox" FilterWidth="40px" Sortable="true" />
			<tstsc:TemplateFieldEx
				HeaderText="<%$Resources:Fields,ExternalLogin %>"
				ItemStyle-CssClass="priority4"
				HeaderStyle-CssClass="priority4 nowrap">
				<ItemTemplate>
					<asp:Literal runat="server" Text="<%# UserLoginType((User)Container.DataItem) %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx
				HeaderText="<%$Resources:Fields,MfaEnabled %>"
				ItemStyle-CssClass="priority4"
				HeaderStyle-CssClass="priority4 nowrap">
				<ItemTemplate>
					<asp:Literal runat="server" Text="<%#: GlobalFunctions.DisplayYnFlag(!String.IsNullOrEmpty(((User)Container.DataItem).MfaToken)) %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx
				HeaderText="<%$Resources:Main,Global_Active%>"
				ItemStyle-CssClass="priority4"
				HeaderStyle-CssClass="priority4 nowrap">
				<ItemTemplate>
					<asp:Literal runat="server" Text="<%# ((User) (Container.DataItem)).IsActive ? Resources.Main.Global_Yes : Resources.Main.Global_No %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1 nowrap">
				<ItemTemplate>
					<tstsc:HyperLinkEx ID="lnkEditUser" SkinID="ButtonDefault" runat="server" NavigateUrl='<%# "UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + ((User) Container.DataItem).UserId%>' Text="<%$Resources:Buttons,Edit %>" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>

	<asp:ObjectDataSource ID="srcActiveFlag" runat="server" SelectMethod="RetrieveFlagLookup"
		TypeName="Inflectra.SpiraTest.Business.UserManager" />

	<script type="text/javascript">
		//Add the event handlers
		$(document).ready(function () {
			$('#<%=grdUserManagement.ClientID%>').on("keydown", function (evt) {
				var keynum = evt.keyCode | evt.which;
				if (keynum == 13) {
					//Click on the button inside the DIV
					$('#<%=btnUserListFilter.ClientID%> button').trigger('click');
				}
			});
		});
    </script>
</asp:Content>
