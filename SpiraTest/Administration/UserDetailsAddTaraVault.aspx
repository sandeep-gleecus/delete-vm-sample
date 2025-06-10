<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true" CodeBehind="UserDetailsAddTaraVault.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.UserDetailsAddTaraVault" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <span class="UserNameAvatar avatar-mid" style="float:left">
                    <tstsc:ImageEx ID="imgUserAvatar" runat="server" />
                </span>
                <h2>
                    <tstsc:LabelEx ID="lblUserName" runat="server" />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_Title %>" />
                    </small>
                </h2>
            </div>
        </div>
        <div class="row" style="margin-top: 10px;">
            <div class="col-lg-9">
                <div class="btn-group">
				    <tstsc:LinkButtonEx ID="btnBackToUserDetails" runat="server">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_BackToUserDetails %>" />
				    </tstsc:LinkButtonEx>
                </div>
				<br />
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
					DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
				<br />
				<asp:Localize runat="server" Text="<%$ Resources:Main,Admin_UserDetailsAddTaraVaultProject_Legend1 %>" /><br />
				<asp:Localize runat="server" Text="<%$ Resources:Main,Admin_UserDetailsAddTaraVaultProject_Legend2 %>" />
				<div class="Spacer"></div>
				<tstsc:GridViewEx ID="grdProjectMembership" CssClass="DataGrid" runat="server" Width="100%"
					AutoGenerateColumns="False" DataKeyNames="ProjectId" HeaderStyle-CssClass="Header"
					ShowSubHeader="false" ShowHeaderWhenEmpty="true">
					<EmptyDataTemplate>
						<asp:Literal runat="server" Text="<%$ Resources:Messages,Admin_User_TaraVaultNoAvailProjects %>" />
					</EmptyDataTemplate>
					<Columns>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Fields,ID %>" >
							<ItemTemplate>
								<%# "[" + GlobalFunctions.ARTIFACT_PREFIX_PROJECT + ":" + (int)((Project)Container.DataItem).ProjectId + "]" %>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
							<ItemTemplate>
								<tstsc:LabelEx runat="server"
									ToolTip='<%# "<u>" + ((Project) Container.DataItem).Name + "</u><br />" + ((Project) Container.DataItem).Description %>'
									ID="Linkbutton1">
										<%# ((Project)Container.DataItem).Name %>
								</tstsc:LabelEx>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Fields,ProjectActive %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
							<ItemTemplate>
								<%# GlobalFunctions.DisplayYnFlag(((Project)Container.DataItem).IsActive) %>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Main,Admin_User_TaraVault_AddProject %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
							<ItemTemplate>
								<tstsc:CheckBoxYnEx runat="server" ID="chkAddProject" />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
				<div class="Spacer"></div>
				<tstsc:ButtonEx ID="btnSave" SkinID="ButtonPrimary" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Save %>" />
				<tstsc:ButtonEx ID="btnCancel" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Cancel %>" />
            </div>
        </div>
    </div>
</asp:Content>
