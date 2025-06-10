<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="ProjectMembershipAdd.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.ProjectMembershipAdd" Title="" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectMembershipAdd_Title %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <tstsc:LabelEx ID="lblProjectName" runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>
    <div class="btn-group priority1" role="group">
		<tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" ID="lnkBackToList">
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectMembershipAdd_BackToList %>" />
		</tstsc:HyperLinkEx>
    </div>

	<p class="mb5">
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectMembershipAdd_String1 %>" /><br />
		<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectMembershipAdd_String2 %>" />
	</p>

    <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
    <asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
	    ShowSummary="True" ShowMessageBox="False" />
    

    <div class="toolbar btn-toolbar-mid-page df items-center my4">
        <div class="btn-group priority1">
			<tstsc:DropMenu id="btnAdd" Runat="server" GlyphIconCssClass="fas fa-save mr3" CausesValidation="False" Text="<%$Resources:Buttons,Add%>" SkinID="ButtonPrimary"/>
			<tstsc:DropMenu id="btnCancel" Runat="server" GlyphIconCssClass="fas fa-times mr3" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
		</div>
        <div class="btn-group priority3">
            <tstsc:DropMenu id="btnFilter" GlyphIconCssClass="fas fa-filter mr3" Runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter%>" />
			<tstsc:DropMenu id="btnClearFilters" GlyphIconCssClass="fas fa-times mr3" Runat="server" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter%>" />
        </div>
    </div>


	<tstsc:GridViewEx id="grdUserManagement" CssClass="DataGrid" Runat="server" PageSize="15" AllowSorting="true" AllowCustomPaging="False" AllowPaging="True" ShowSubHeader="True" Width="100%" AutoGenerateColumns="False">
		<HeaderStyle CssClass="Header" />
		<SubHeaderStyle CssClass="SubHeader" />
		<Columns>
            <tstsc:FilterSortFieldEx DataField="Profile.FirstName" HeaderText="<%$Resources:Fields,FirstName %>" FilterField="Profile.FirstName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="Profile.MiddleInitial" HeaderText="<%$Resources:Fields,MiddleInitialAbbreviation %>" FilterField="Profile.MiddleInitial" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="Profile.LastName" HeaderText="<%$Resources:Fields,LastName %>" FilterField="Profile.LastName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$Resources:Fields,UserName %>" FilterField="UserName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="Profile.Department" HeaderText="<%$Resources:Fields,Department %>" FilterField="Profile.Department" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="Profile.Organization" HeaderText="<%$Resources:Fields,Organization %>" FilterField="Profile.Organization" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
            <tstsc:FilterSortFieldEx DataField="UserId" HeaderText="<%$Resources:Fields,ID %>" FilterField="UserId" FilterType="TextBox" FilterWidth="40px" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1" /> 
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectRole %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="nowrap priority1">
            <ItemTemplate>
                <tstsc:DropDownListEx CssClass="DropDownList" runat="server" DataSource="<%# projectRoles %>" MetaData='<%# ((User) Container.DataItem).UserId%>'
                    DataTextField="Name" DataValueField="ProjectRoleId" ID="ddlProjectRole" Width="200px" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_SelectRole %>" />
            </ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
</asp:Content>
