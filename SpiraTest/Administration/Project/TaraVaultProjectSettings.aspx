<%@ Page Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="TaraVaultProjectSettings.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.Project.TaraVaultProjectSettings" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <tstsc:ImageEx runat="server" ID="imgTaraVaultLogo" class="w6 h6"/>
        <tstsc:LabelEx runat="server" CssClass="Title" Text="<%$ Resources:Main,Admin_TaraVaultConfig %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
		        <asp:Literal ID="ltrProjectName" runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>

    <div class="btn-group priority2 mb4">
        <tstsc:HyperLinkEx ID="lnkhome" runat="server" SkinID="ButtonDefault" >
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_BackToHome %>" />
		</tstsc:HyperLinkEx>
    </div>
            
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<h4 class="my4">
		<strong><asp:Localize ID="txtIsActive" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultConfig_IsActive %>" /></strong>
	</h4>
    <p class="mw720">
		<%-- Introduction to what TaraVault is all about. --%>
		<asp:Localize ID="locIntro1" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultConfig_Intro1 %>" />
	</p>
	<p class="mw720">
		<%-- Describes projects in the account. --%>
		<asp:Localize ID="locIntro2" runat="server" />
	</p>
	<p class="mw720">
		<%-- Describes users for the project. --%>
		<asp:Localize ID="locIntro3" runat="server" />
	</p>


    <div runat="server" ID="divProjectSettings" class="u-wrapper width_md">
	    <!-- Project Settings -->
        <div class="u-box_2" runat="server" ID="divProjectProperties">
            <div 
                class="u-box_group"
                id="form-group_admin-product-baseline-properties" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,Admin_TaraVault_ProjectSummary %>" />
                </div>
                <ul class="u-box_list" >
                    <li class="ma0 pa0 mb3">
						<tstsc:LabelEx runat="server" AssociatedControlID="lblProjectActive" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjActive %>" AppendColon="true" />
						<asp:Label runat="server" ID="lblProjectActive" Font-Bold="true" />
                    </li>
                    <li class="ma0 pa0 mb3" id="rowProjectName" runat="server">
						<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjName %>" AssociatedControlID="txtProjectName" AppendColon="true" />
                        <span>
						    <tstsc:TextBoxEx ID="txtProjectName" runat="server" MaxLength="40" ValidationGroup="activateProject" />
                            &nbsp;(<asp:Label runat="server" ID="lblProjectId" />)
						    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtProjectName" SetFocusOnError="true" Text="*" ErrorMessage="This field is required." ValidationGroup="activateProject" />
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3" id="rowProjectType" runat="server">
						<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjType %>" AssociatedControlID="ddlProjectType" AppendColon="true" />
                        <span>
						    <asp:Label runat="server" ID="lblProjectType" />
						    <tstsc:UnityDropDownListEx DisabledCssClass="u-dropdown disabled" CssClass="u-dropdown" runat="server" ID="ddlProjectType" DataTextField="Value"
							    DataValueField="Key" NoValueItem="false" />
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                    	<tstsc:LabelEx runat="server" AssociatedControlID="lblProjConnection" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjConnect %>" AppendColon="true" />
                        <asp:Label runat="server" Font-Bold="true" ID="lblProjConnection" />
                    </li>
                    <li class="ma0 pa0 my4" runat="server" ID="rowProjectUsers1">
						<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjUsers %>" AppendColon="true" />
                        <div runat="server" ID="rowProjectUsers2">
						    <tstsc:GridViewEx ID="grdUserList" CssClass="DataGrid" runat="server" PageSize="25"
							    AllowSorting="true" AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False"
							    Width="100%" AutoGenerateColumns="False" EnableViewState="true" HeaderStyle-CssClass="Header"
							    ShowHeaderWhenEmpty="true" DataKeyNames="UserId">
							    <EmptyDataTemplate>
								    <asp:Literal runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NoUsersEnabled %>" />&nbsp;
								    <asp:HyperLink runat="server" Text="<%$ Resources:Buttons,AddUsers %>" NavigateUrl='<%#UrlRoots.RetrieveProjectAdminUrl(ProjectId, "TaraVault_ProjectUsers")%>' />
							    </EmptyDataTemplate>
							    <Columns>
								    <tstsc:FilterSortFieldEx DataField="Profile.FullName" HeaderText="<%$ Resources:Fields,FullName %>" FilterField="Profile.FullName"
									    FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
								    <tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$ Resources:Fields,UserName %>" FilterField="UserName"
									    FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
								    <tstsc:FilterSortFieldEx DataField="TaraVault.VaultUserLogin" HeaderText="<%$ Resources:Fields,TaraVault_UserLogin %>" FilterField="UserLogin"
									    FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
								    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
									    <ItemTemplate>
										    <tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" Text="<%$ Resources:Buttons,EditUsers %>" NavigateUrl='<%#UrlRoots.RetrieveProjectAdminUrl(ProjectId, "TaraVault_ProjectUsers")%>' />
									    </ItemTemplate>
								    </tstsc:TemplateFieldEx>
							    </Columns>
						    </tstsc:GridViewEx>
                        </div>
                    </li>
                    <li class="ma0 pa0">
                        <div class="btn-group" role="group">
					        <tstsc:ButtonEx ID="btnProjActivate" SkinID="ButtonPrimary" runat="server" Confirmation="true" ConfirmationMessage="<%$ Resources:Main,Admin_TaraVault_Confirm_ProjActivate %>" Text="<%$ Resources:Buttons,Activate %>" Visible="false" ValidationGroup="activateProject" />
					        <tstsc:ButtonEx ID="btnProjDeactivate" runat="server" Confirmation="true" ConfirmationMessage="<%$ Resources:Main,Admin_TaraVault_Confirm_ProjDeactivate %>" Text="<%$ Resources:Buttons,Deactivate %>" Visible="false" />
					        <tstsc:ButtonEx ID="btnRefreshCache" runat="server" Text="<%$ Resources:Buttons,RefreshCache %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" Confirmation="false" SkinID="ButtonPrimary" />
					        <tstsc:ButtonEx ID="btnDeleteCache" runat="server" Text="<%$ Resources:Buttons,ClearCache %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_VersionControlProvider_ClearCacheWarning %>" />
                        </div>
                    </li>
                </ul>
            </div>
        </div>
    </div>
</asp:Content>
