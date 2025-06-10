<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
	CodeBehind="ProjectMembership.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.ProjectMembership" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h2>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,ProjectMembership_Title%>" />
		<small>
			<tstsc:HyperLinkEx
				ID="lnkAdminHome"
				runat="server"
				Title="<%$Resources:Main,Admin_Project_BackToHome %>">
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
		</small>
	</h2>
	
    <p class="mb5">
	    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectMembership_String1 %>" /><br />
	    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectMembership_String2 %>" />
    </p>

	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
		DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />


	<div class="toolbar btn-toolbar-mid-page flex items-center my4">
		<div class="btn-group priority2" role="group">
			<tstsc:DropMenu ID="btnProjectMembershipUpdate" SkinID="ButtonPrimary" GlyphIconCssClass="fas fa-save mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Save %>"></tstsc:DropMenu>
			<tstsc:DropMenu ID="btnProjectMembershipAdd" GlyphIconCssClass="fas fa-plus mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add%>"></tstsc:DropMenu>
			<tstsc:DropMenu ID="btnProjectMembershipDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt" CausesValidation="False" Text="<%$Resources:Buttons,Delete%>" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ProjectMembership_ConfirmRemove %>"></tstsc:DropMenu>
		</div>

		<div class="dib pl5">
			<div class="display-inline-block mr3">
				<strong>
					<asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>" />:</strong>
			</div>
			<tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
				<asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
				<asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
			</tstsc:DropDownListEx>
		</div>
	</div>

	<tstsc:GridViewEx ID="grdUserMembership" CssClass="DataGrid" runat="server"
		DataMember="ProjectUser" AutoGenerateColumns="False" Width="100%">
		<HeaderStyle CssClass="Header" />
		<Columns>
			<tstsc:TemplateFieldEx HeaderStyle-CssClass="TickIcon priority3" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="priority3">
				<ItemTemplate>
					<tstsc:CheckBoxEx runat="server" ID="chkDeleteMembership" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,FullName %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblFullName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).FullName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,UserName %>" 
                HeaderStyle-CssClass="priority3" 
                ItemStyle-CssClass="priority3" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblUserName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).UserName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,Department %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblDepartment" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).User.Profile.Department) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,Organization %>" 
                HeaderStyle-CssClass="priority4" 
                ItemStyle-CssClass="priority4" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblOrganization" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).User.Profile.Organization) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectRole %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
				<ItemTemplate>
					<tstsc:DropDownListEx runat="server" 
						ID="ddlProjectRole" 
						CssClass="DropDownList" 
						DataSource="<%# projectRoles %>"
						DataTextField="Name" 
						DataValueField="ProjectRoleId" 
						MetaData='<%# ((Entity)Container.DataItem)["UserId"] %>' />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
</asp:Content>
