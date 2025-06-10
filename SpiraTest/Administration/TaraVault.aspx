<%@ Page Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="TaraVault.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.TaraVaultAdmin" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2 class="df items-center">
                    <tstsc:ImageEx runat="server" ID="imgTaraVaultLogo" class="logo-product logo-v-6 mr4"/>
                    <tstsc:LabelEx runat="server" CssClass="Title" Text="<%$ Resources:Main,Admin_TaraVaultConfig %>" />
                    <small>
		                <asp:Literal ID="ltrProjectName" runat="server" />
                    </small>
                </h2>
            </div>
        </div>
        <div class="row my3" runat="server" ID="divNotActivated">
            <div class="col-lg-9 col-sm-11">
                <div class="row mx0">
				    <div class="alert alert-warning text-center pa5">
					    <h4><asp:Label runat="server" ID="lblActivate" Text="<%$ Resources:Messages,Admin_TaraVault_Active %>" /></h4>
                        <br />
				        <tstsc:ButtonEx ID="btnActivate" SkinID="ButtonPrimary" runat="server" Text="<%$ Resources:Buttons,ActivateTara %>" />
				    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-9 my3">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	            <h4>
		            <strong><asp:Localize ID="txtIsActive" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultConfig_IsActive %>" /></strong>
	            </h4>
                <p>
		            <%-- Introduction to what TaraVault is all about. --%>
		            <asp:Localize ID="locIntro1" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultConfig_Intro1 %>" />
	            </p>
	            <p>
		            <%-- Describes projects in the account. --%>
		            <asp:Localize ID="locIntro2" runat="server" />
	            </p>
	            <p>
		            <%-- Describes users for the project. --%>
		            <asp:Localize ID="locIntro3" runat="server" />
	            </p>
            </div>
        </div>
        <div class="row" runat="server" ID="divProjectSettings">
            <!-- System Settings -->
                <div class="row DataEntryForm my4">
                    <div class="col-lg-9 col-sm-11">
                        <fieldset class="fieldset-gray">
			                <legend>
				                <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVault_SystemSummary %>" />
			                </legend>
                            <div class="form-group row mx0" ID="rowTaraAcctUsers" runat="server">
                                <div class="DataLabel col-sm-3 col-lg-4">
							        <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NumberUsers %>" AssociatedControlID="lblAccountUsers" />:
                                </div>
                                <div class="DataEntry col-sm-9 col-lg-6">
							        <asp:Label runat="server" ID="lblAccountUsers" />
                                </div>
                            </div>
                            <div class="form-group row mx0" ID="rowTaraActUsers" runat="server">
                                <div class="DataLabel col-sm-3 col-lg-4">
							        <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ActiveUsers %>" AssociatedControlID="lblAccountUsers" />:
                                </div>
                                <div class="DataEntry col-sm-9 col-lg-6">
							        <asp:Label runat="server" ID="lblActiveUsers" />&nbsp(<asp:HyperLink runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ActiveUsers_ViewList %>" NavigateUrl="TaraVault_UserList.aspx" />)
                                </div>
                            </div>
                            <div class="form-group row mx0" ID="rowTaraAcct" runat="server">
                                <div class="DataLabel col-sm-3 col-lg-4">
							        <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_AccountName %>" AssociatedControlID="lblAccountName" />:
                                </div>
                                <div class="DataEntry col-sm-9 col-lg-6">
							        <asp:Label runat="server" ID="lblAccountName" />&nbsp;(<asp:Label runat="server" ID="lblAccountId" />)
                                </div>
                            </div>
                            <div class="form-group row mx0" ID="Div1" runat="server">
                                <div class="DataLabel col-sm-3 col-lg-4">
							        <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ActiveProjects %>" AssociatedControlID="lblActiveProjects" />:
                                </div>
                                <div class="DataEntry col-sm-9 col-lg-6">
							        <asp:Label runat="server" ID="lblActiveProjects" />&nbsp(<asp:HyperLink runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ActiveUsers_ViewList %>" NavigateUrl="TaraVault_ProjectList.aspx" />)
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </div>
			<!-- Project Settings -->
            <div class="row DataEntryForm my4" runat="server" ID="divProjectProperties" Visible="false">
                <div class="col-lg-9 col-sm-11">
                    <fieldset class="fieldset-gray">
			            <legend>
				            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVault_ProjectSummary %>" />
			            </legend>
                        <div class="form-group row mx0">
                            <div class="DataLabel col-sm-3 col-lg-4">
							    <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjActive %>" />
                            </div>
                            <div class="DataEntry col-sm-9 col-lg-6">
							    <asp:Label runat="server" ID="lblProjectActive" Font-Bold="true" />
                            </div>
                        </div>
                        <div class="form-group row mx0" ID="rowProjectName" runat="server">
                            <div class="DataLabel col-sm-3 col-lg-4">
							    <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjName %>" AssociatedControlID="txtProjectName" />:
                            </div>
                            <div class="DataEntry col-sm-9 col-lg-6">
							    <tstsc:TextBoxEx ID="txtProjectName" runat="server" MaxLength="40" ValidationGroup="activateProject" />
                                &nbsp;(<asp:Label runat="server" ID="lblProjectId" />)
							    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtProjectName" SetFocusOnError="true" Text="*" ErrorMessage="This field is required." ValidationGroup="activateProject" />
                            </div>
                        </div>
                        <div class="form-group row mx0" ID="rowProjectType" runat="server">
                            <div class="DataLabel col-sm-3 col-lg-4">
							    <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjType %>" AssociatedControlID="ddlProjectType" />:
                            </div>
                            <div class="DataEntry col-sm-9 col-lg-6">
							    <asp:Label runat="server" ID="lblProjectType" />
							    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlProjectType" DataTextField="Value"
								    DataValueField="Key" NoValueItem="false" />
                            </div>
                        </div>
                        <div class="form-group row mx0"  runat="server">
                            <div class="DataLabel col-sm-3 col-lg-4">
							    <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjConnect %>" />:
                            </div>
                            <div class="DataEntry col-sm-9 col-lg-6">
							    <asp:Label runat="server" Font-Bold="true" ID="lblProjConnection" />
                            </div>
                        </div>
                        <div class="form-group row mx0" runat="server" ID="rowProjectUsers1">
                            <div class="DataLabel col-sm-12">
							    <asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_ProjUsers %>" />:
                            </div>
                            <div class="col-sm-12" runat="server" ID="rowProjectUsers2">
							    <tstsc:GridViewEx ID="grdUserList" CssClass="DataGrid" runat="server" PageSize="25"
								    AllowSorting="true" AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False"
								    Width="100%" AutoGenerateColumns="False" EnableViewState="true" HeaderStyle-CssClass="Header"
								    ShowHeaderWhenEmpty="true" DataKeyNames="UserId">
								    <EmptyDataTemplate>
									    <asp:Literal runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NoUsersEnabled %>" />&nbsp;
									    <asp:HyperLink runat="server" Text="<%$ Resources:Buttons,AddUsers %>" NavigateUrl="TaraVault_UserList.aspx" />
								    </EmptyDataTemplate>
								    <Columns>
									    <tstsc:FilterSortFieldEx DataField="Profile.FullName" HeaderText="<%$ Resources:Fields,FullName %>" FilterField="Profile.FullName"
										    FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
									    <tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$ Resources:Fields,UserName %>" FilterField="UserName"
										    FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
									    <tstsc:FilterSortFieldEx DataField="TaraVault.VaultUserLogin" HeaderText="<%$ Resources:Fields,TaraVault_UserLogin %>" FilterField="TaraVault.VaultUserLogin"
										    FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
									    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
										    <ItemTemplate>
											    <tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" Text="<%$ Resources:Buttons,EditUsers %>" NavigateUrl="TaraVault_UserList.aspx" />
										    </ItemTemplate>
									    </tstsc:TemplateFieldEx>
								    </Columns>
							    </tstsc:GridViewEx>
                            </div>
                        </div>
                        <div class="row mx0">
                            <div class="btn-group" role="group">
					            <tstsc:ButtonEx ID="btnProjActivate" SkinID="ButtonPrimary" runat="server" Confirmation="true" ConfirmationMessage="<%$ Resources:Main,Admin_TaraVault_Confirm_ProjActivate %>" Text="<%$ Resources:Buttons,Activate %>" Visible="false" ValidationGroup="activateProject" />
					            <tstsc:ButtonEx ID="btnProjDeactivate" runat="server" Confirmation="true" ConfirmationMessage="<%$ Resources:Main,Admin_TaraVault_Confirm_ProjDeactivate %>" Text="<%$ Resources:Buttons,Deactivate %>" Visible="false" />
					            <tstsc:ButtonEx ID="btnRefreshCache" runat="server" Text="<%$ Resources:Buttons,RefreshCache %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" Confirmation="false" SkinID="ButtonPrimary" />
					            <tstsc:ButtonEx ID="btnDeleteCache" runat="server" Text="<%$ Resources:Buttons,ClearCache %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_VersionControlProvider_ClearCacheWarning %>" />
                            </div>
                        </div>
                    </fieldset>
                </div>
            </div>
            <div class="row DataEntryForm my4" runat="server" ID="divNoProjectSelected" Visible="false">
                <div class="col-lg-9 col-sm-11">
					<asp:Label runat="server" Text="<%$ Resources:Main,Admin_TaraVault_SelectProject %>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
