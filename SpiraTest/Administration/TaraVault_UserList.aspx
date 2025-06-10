<%@ Page Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="TaraVault_UserList.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.TaraVault_UserList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1>
                    <tstsc:LabelEx ID="LabelEx1" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultUsers %>" />
	                <small>
		                <asp:Literal ID="ltrProjectName" runat="server" />
	                </small>
                </h1>
                <div class="btn-group" role="group">
	                <tstsc:HyperLinkEx runat="server" ID="lnkBackToTaraVault" NavigateUrl="TaraVault.aspx" SkinID="ButtonDefault">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVault_BackToHome %>" />
	                </tstsc:HyperLinkEx>
                </div>
	            <p class="my3">
		            <%-- Introduction to what TaraVault is all about. --%>
		            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVaultUsers_Intro %>" />
	            </p>
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_TaraVault_UserList %>" />
                </h2>
				<tstsc:GridViewEx ID="grdUserList" CssClass="DataGrid" runat="server" AllowSorting="true"
					AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False" Width="100%"
					AutoGenerateColumns="False" EnableViewState="true" HeaderStyle-CssClass="Header"
					ShowHeaderWhenEmpty="true" DataKeyNames="UserId">
					<EmptyDataTemplate>
						<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NoUsersActivated %>" />
						<asp:HyperLink runat="server" ID="lnkActUsers" Text="<%$ Resources:Buttons,ActivateUsers %>" NavigateUrl="UserList.aspx" />
					</EmptyDataTemplate>
					<Columns>
						<tstsc:FilterSortFieldEx DataField="Profile.FullName" HeaderText="<%$ Resources:Fields,FullName %>" FilterField="Profile.FullName"
							FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
						<tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$ Resources:Fields,UserName %>" FilterField="UserName"
							FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Fields,TaraVault_UserLogin %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
							<ItemTemplate>
								<%# ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).TaraVault.VaultUserLogin %>&nbsp;(<%# ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).TaraVault.VaultUserId %>)
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
							<ItemTemplate>
                                <div class="btn-group" role="group">
								    <tstsc:HyperLinkEx ID="lnkEditUser" SkinID="ButtonDefault" runat="server" NavigateUrl='<%# "UserDetailsEdit.aspx?" + Inflectra.SpiraTest.Web.GlobalFunctions.PARAMETER_USER_ID + "=" + ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).UserId %>'>
                                        <span class="fas fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
								    </tstsc:HyperLinkEx>
                                    <tstsc:LinkButtonEx ID="btnAddUser" runat="server" CommandName="AddUserToProject" CommandArgument="<%#((Inflectra.SpiraTest.DataModel.User)Container.DataItem).UserId%>">
                                        <span class="fas fa-plus"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,AddToProject %>" />
                                    </tstsc:LinkButtonEx>
                                </div>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
                <br />
                <h2>
                    <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVault_ProjectMembers %>" />
                </h2>
				<tstsc:GridViewEx ID="grdProjectUsers" CssClass="DataGrid" runat="server" AllowSorting="true"
					AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False" Width="100%"
					AutoGenerateColumns="False" EnableViewState="true" HeaderStyle-CssClass="Header"
					ShowHeaderWhenEmpty="true" DataKeyNames="UserId">
					<EmptyDataTemplate>
						<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NoUsersActivated %>" />
						<asp:HyperLink runat="server" ID="lnkActUsers" Text="<%$ Resources:Buttons,ActivateUsers %>" NavigateUrl="UserList.aspx" />
					</EmptyDataTemplate>
					<Columns>
						<tstsc:FilterSortFieldEx DataField="Profile.FullName" HeaderText="<%$ Resources:Fields,FullName %>" FilterField="Profile.FullName"
							FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
						<tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$ Resources:Fields,UserName %>" FilterField="UserName"
							FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Fields,TaraVault_UserLogin %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
							<ItemTemplate>
								<%# ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).TaraVault.VaultUserLogin %>&nbsp;(<%# ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).TaraVault.VaultUserId %>)
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
							<ItemTemplate>
                                <div class="btn-group" role="group">
								    <tstsc:HyperLinkEx ID="lnkEditUser" SkinID="ButtonDefault" runat="server" NavigateUrl='<%# "UserDetailsEdit.aspx?" + Inflectra.SpiraTest.Web.GlobalFunctions.PARAMETER_USER_ID + "=" + ((Inflectra.SpiraTest.DataModel.User)Container.DataItem).UserId %>'>
                                        <span class="fas fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
								    </tstsc:HyperLinkEx>
                                    <tstsc:LinkButtonEx ID="btnRemoveUser" runat="server" CommandName="RemoveUserFromProject" CommandArgument="<%#((Inflectra.SpiraTest.DataModel.User)Container.DataItem).UserId%>" Visible="<%#((Inflectra.SpiraTest.DataModel.User)Container.DataItem).UserId != Inflectra.SpiraTest.Business.UserManager.UserSystemAdministrator %>">
                                        <span class="fas fa-trash-alt"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Remove %>" />
                                    </tstsc:LinkButtonEx>
                                </div>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
            </div>
        </div>
    </div>
</asp:Content>
