<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<%@ Page Language="c#" CodeBehind="UserProfile.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.UserProfile"
	MasterPageFile="~/MasterPages/Main.Master" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	<div class="w960 mvw-100 px4 pb5 pt6 pt5-sm mx-auto">

		<h2 class="mx-auto w10 w-100-sm mb5 db fs-h1 yolk">
			<asp:Localize
				runat="server"
				Text="<%$Resources:ClientScript,GlobalNavigation_MyProfile %>" />
		</h2>

		<tstsc:MessageBox
			ID="lblMessage"
			runat="server"
			SkinID="MessageBox" />
		<asp:ValidationSummary
			CssClass="ValidationMessage"
			ShowMessageBox="False"
			ShowSummary="True"
			DisplayMode="BulletList"
			runat="server"
			ID="ValidationSummary1" />
		<tstsc:MessageBox
			ID="lblFileMessage"
			runat="server"
			SkinID="MessageBox" />


		<div class="mx-auto mb5 w10 w-100-sm">
			<tstsc:LabelEx
				ID="imgLogoLabel"
				CssClass="db fw-b mb3"
				runat="server"
				AssociatedControlID="imgAvatar"
				Required="false"
				Text="<%$Resources:Main,UserProfile_Logo %>" />
			<div style="width: 100px; height: 100px;" class="dib mb3 relative">
				<tstsc:UserNameAvatar ID="imgAvatar" runat="server" AvatarSize="100" ShowUserName="false" />
				<div class="avatar-overlay">
					<div class="avatar-instructions">
						<asp:Label runat="server" Text="<%$ Resources:Main,Global_Edit %>" />
					</div>
					<div class="avatar-buttons">
						<tstsc:ButtonEx ID="btnDeleteAvatar" SkinID="AvatarDelete" runat="server" Text="&#xf00d;" ToolTip="<%$ Resources:Main,UserProfile_Avatar_DeleteExisting %>" />
						<label>
							<span class="btnUploadAvatar">
								<asp:Label runat="server" ToolTip="<%$ Resources:Main,UserProfile_Avatar_UploadNew %>">
                                    <span class="fas fa-plus"></span>
								</asp:Label>
							</span>
							<asp:FileUpload ID="filAttachment" runat="server" CssClass="text-box" ToolTip="<%$ Resources:Main,UserProfile_Avatar_UploadNewTooltip %>" Style="display: inline-block; position: fixed; top: -100em;" />
						</label>
					</div>
				</div>
			</div>
			<p id="filNameVisible"></p>

			<p>
				<small>
					<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,UserProfile_Avatar_Notes %>" />
				</small>
			</p>
		</div>


		<div class="mx-auto mb6 mb5-xs w10 w-100-sm">
			<div class="mb5 relative">
				<label class="label-above">
					<asp:Literal
						ID="Literal8"
						runat="server"
						Text="<% $Resources:Main,UserProfile_NameId %>" />
				</label>
				<div>
					<asp:Label
						CssClass="pointer dib orange-hover transition-all"
						title="<%$Resources:Buttons,CopyToClipboard %>"
						data-copytoclipboard='true'
						ID="lblUserName"
						runat="server" />
					[US:<asp:Label
						CssClass="pointer dib orange-hover transition-all"
						ID="lblUserId"
						runat="server" />]
				</div>
			</div>
			<div class="mb5 relative">
				<tstsc:UnityTextBoxEx
					CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
					DisabledCssClass="u-input disabled"
					ID="txtFirstName"
					MaxLength="50"
					runat="server"
					TextMode="SingleLine" />
				<tstsc:LabelEx
					AssociatedControlID="txtFirstName"
					CssClass="label-slideup tl"
					ID="Literal1"
					Required="true"
					runat="server"
					Text="<% $Resources:Main,UserProfile_FirstName %>" />
				<asp:RequiredFieldValidator
					ControlToValidate="txtFirstName"
					ErrorMessage="<%$Resources:Messages,UserProfile_FirstNameRequired %>"
					ID="Requiredfieldvalidator1"
					runat="server"
					Text="*"></asp:RequiredFieldValidator>
			</div>
			<div class="mb5 relative">
				<tstsc:UnityTextBoxEx
					CssClass="w6 u-input u-input-minimal py3 px1 fs-110"
					DisabledCssClass="u-input disabled"
					ID="txtMiddleInitial"
					MaxLength="1"
					runat="server"
					TextMode="SingleLine" />
				<tstsc:LabelEx
					AssociatedControlID="txtMiddleInitial"
					CssClass="label-slideup tl"
					ID="Literal2"
					Required="false"
					runat="server"
					Text="<% $Resources:Main,UserProfile_MiddleInitial %>" />
			</div>
			<div class="mb5 relative">
				<tstsc:UnityTextBoxEx
					CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
					DisabledCssClass="u-input disabled"
					ID="txtLastName"
					MaxLength="50"
					runat="server"
					TextMode="SingleLine" />
				<tstsc:LabelEx
					AssociatedControlID="txtLastName"
					CssClass="label-slideup tl"
					ID="Literal3"
					Required="true"
					runat="server"
					Text="<% $Resources:Main,UserProfile_LastName %>" />
				<asp:RequiredFieldValidator
					ControlToValidate="txtLastName"
					ErrorMessage="<%$Resources:Messages,UserProfile_LastNameRequired %>"
					ID="Requiredfieldvalidator3"
					runat="server"
					Text="*"></asp:RequiredFieldValidator>
			</div>
			<div class="mb5 relative">
				<tstsc:UnityTextBoxEx
					CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
					DisabledCssClass="u-input disabled"
					ID="txtDepartment"
					MaxLength="50"
					runat="server"
					TextMode="SingleLine" />
				<tstsc:LabelEx
					AssociatedControlID="txtDepartment"
					CssClass="label-slideup tl"
					ID="txtDepartmentLabel"
					Required="false"
					runat="server"
					Text="<% $Resources:Main,UserProfile_Department %>" />
			</div>
			<div class="mb5 relative">
				<tstsc:UnityTextBoxEx
					CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
					DisabledCssClass="u-input disabled"
					ID="txtOrganization"
					MaxLength="50"
					runat="server"
					TextMode="SingleLine" />
				<tstsc:LabelEx
					AssociatedControlID="txtOrganization"
					CssClass="label-slideup tl"
					ID="txtOrganizationLabel"
					Required="false"
					runat="server"
					Text="<% $Resources:Main,UserProfile_Organization %>" />
			</div>
			<div class="mb5 relative">
				<tstsc:LabelEx
					AssociatedControlID="chkRssEnabled"
					CssClass="label-above"
					ID="RssEnabledLabel"
					runat="server"
					Text="<%$Resources:Main,UserProfile_EnableRSSFeeds %>" />
				<tstsc:CheckBoxYnEx
					ID="chkRssEnabled"
					runat="server" />
			</div>
			<div class="mb5 relative">
				<tstsc:LabelEx
					AssociatedControlID="txtRssToken"
					CssClass="label-above"
					ID="txtRssTokenLabel"
					runat="server"
					Text="<%$Resources:Main,UserProfile_RssTokenApiKey %>" 
                    />
                <p class="fw-b pa3 bg-peach-light br3">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,UserProfile_RssTokenApiKeyReminder %>" 
                        />
                </p>
				<span>
					<tstsc:LabelEx
						CssClass="mb2 dib pointer orange-hover transition-all text-obscured"
						title="<%$Resources:Buttons,CopyToClipboard %>"
						data-copytoclipboard='true'
						ID="txtRssToken"
						MaxLength="255"
						ReadOnly="true"
						runat="server" />
					<tstsc:ButtonEx
						CausesValidation="false"
						ID="btnGenerateNewToken"
						runat="server"
						Text="<%$Resources:Buttons,GenerateNew %>" />
				</span>
			</div>
			<div class="mb5 pb4 relative">
				<tstsc:LabelEx
					AssociatedControlID="ddlStartPage"
					CssClass="label-above"
					ID="ddlStartPageLabel"
					runat="server"
					Text="<% $Resources:Main,UserProfile_StartPage %>"
					Required="true" />
				<tstsc:UnityDropDownListEx
					CssClass="u-dropdown is-active w-100"
					DataTextField="Value"
					DataValueField="Key"
					DisabledCssClass="u-dropdown disabled"
					ID="ddlStartPage"
					runat="server" />
			</div>
			<div class="btn-group">
				<tstsc:ButtonEx
					CausesValidation="True"
					ID="btnUpdate"
					runat="server"
					SkinID="ButtonPrimary"
					Text="<%$Resources:Buttons,Save %>" />
				<tstsc:ButtonEx
					CausesValidation="False"
					ID="btnCancel"
					runat="server"
					Text="<%$Resources:Buttons,Cancel %>" />
			</div>
		</div>







		<tstsc:TabControl ID="tclUserDetails" TabWidth="110" runat="server">
			<TabPages>
				<tstsc:TabPage runat="server" ID="tabPassword" Caption="<%$ Resources:Buttons,AccountSecurity %>"
					TabPageControlId="pnlChangePassword" CheckPermissions="false" />
				<tstsc:TabPage runat="server" ID="tabEmail" Caption="<%$ Resources:Main,UserProfile_TabEmailPreferences %>"
					TabPageControlId="pnlNotification" CheckPermissions="false" />
				<tstsc:TabPage runat="server" ID="tabTaraV" Caption="<%$ Resources:Main,UserProfile_TabTaraV %>"
					TabPageControlId="pnlTaraV" CheckPermissions="false" />
				<tstsc:TabPage runat="server" ID="tabCulture" Caption="<%$ Resources:Main,UserProfile_RegionalSettings %>"
					TabPageControlId="pnlCulture" CheckPermissions="false" />
				<tstsc:TabPage runat="server" ID="tabActions" Caption="<%$ Resources:Fields,Actions %>"
					TabPageControlId="pnlActions" />
			</TabPages>
		</tstsc:TabControl>





		<asp:Panel runat="server" ID="pnlNotification" CssClass="TabControlPanel">
			<div class="mt5 mx-auto mb6 mb5-xs w10 w-100-sm">
				<div class="mb5 relative">
					<tstsc:UnityTextBoxEx
						CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
						DisabledCssClass="u-input disabled"
						ID="txtEmailAddress"
						MaxLength="50"
						runat="server"
						TextMode="SingleLine" />
					<tstsc:LabelEx
						AssociatedControlID="txtEmailAddress"
						CssClass="label-slideup tl"
						Required="true"
						runat="server"
						Text="<% $Resources:Main,UserProfile_EmailAddress %>" />
					<asp:RequiredFieldValidator ID="Requiredfieldvalidator2" runat="server" Text="*"
						ErrorMessage="<%$Resources:Messages,UserProfile_EmailRequired %>" ControlToValidate="txtEmailAddress" />
					<asp:RegularExpressionValidator runat="server" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_EMAIL_ADDRESS%>"
						ControlToValidate="txtEmailAddress" Text="*" ErrorMessage="<%$Resources:Messages,UserProfile_EmailNotValid %>" />
				</div>
				<div class="mb5 relative">
					<tstsc:LabelEx
						AssociatedControlID="ddlEmailActive"
						CssClass="label-above"
						Required="false"
						runat="server"
						Text="<% $Resources:Main,UserProfile_EnableNotifications %>" />
					<tstsc:UnityDropDownListEx
						CssClass="u-dropdown is-active"
						DataTextField="Value"
						DataValueField="Key"
						DisabledCssClass="u-dropdown disabled"
						ID="ddlEmailActive"
						NoValueItem="false"
						runat="server" />
					<small class="db mt3">
						<asp:Literal
							ID="Literal4"
							runat="server"
							Text="<% $Resources:Messages,UserDetails_detEnableNotifications %>" />
					</small>
				</div>
			</div>
		</asp:Panel>






		<!-- TaraVault Projects -->
		<asp:Panel runat="server" ID="pnlTaraV" CssClass="TabControlPanel">
			<div class="mt5 mx-auto mb6 mb5-xs w10 w-100-sm">
				<div class="mb5 relative">
					<tstsc:UnityTextBoxEx
						CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
						DisabledCssClass="u-input disabled"
						ID="lblTVaultLogin"
						runat="server"
						Enabled="false" />
					<tstsc:LabelEx
						CssClass="label-slideup tl"
						AssociatedControlID="lblTVaultLogin"
						ID="Label2"
						runat="server"
						Text="<%$ Resources:Main,UserProfile_TaravaultUser %>" />
				</div>
				<div class="mb5 relative">
					<tstsc:UnityTextBoxEx
						CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
						ID="txtTVaultPass"
						runat="server"
						MaxLength="255" />
					<tstsc:LabelEx
						CssClass="label-slideup tl"
						AssociatedControlID="txtTVaultPass"
						ID="Label4"
						runat="server"
						Text="<%$ Resources:Main,UserProfile_TaravaultPass %>" />
				</div>
			</div>
		</asp:Panel>

		<!-- Change Password / LDAP / OAuth Settings -->
		<asp:Panel runat="server" ID="pnlChangePassword" CssClass="TabControlPanel">
			<div class="mt5 mx-auto mb6 mb5-xs w10 w-100-sm">
				<asp:Panel runat="server" ID="pnlSubPass_Ldap">
					<h4>
						<asp:Literal runat="server" Text="<%$ Resources:Main,UserProfile_TabLDAPSettings %>" />
					</h4>
					<div class="mt5 mx-auto mb6 mb5-xs w10 w-100-sm">
						<b>
							<asp:Literal runat="server" Text="<% $Resources:Main,UserProfile_LDAPDN %>" />
						</b>
						<tstsc:LabelEx ID="lblLdapDn" runat="server" />
					</div>
				</asp:Panel>

				<%-- Add/Change MFA Settings if enabled --%>
				<asp:PlaceHolder ID="plcMfaSettings" runat="server">
					<fieldset class="fieldset-gray mt5 mx-auto mb4 mb3-xs w10 px4 w-100-sm">
						<legend>
							<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,UserProfile_MfaSettings %>" />
						</legend>
						<div class="ma0 mb4 pa0">
							<tstsc:LabelEx
								CssClass="fs-90"
								ID="LabelEx5"
								runat="server"
								Text="<% $Resources:Main,UserProfile_MfaSettingsIntro %>" />
						</div>
						<div class="ma0 mb5 pa0">
                        <asp:PlaceHolder ID="plcAddMFA" runat="server">
							<span class="badge">
								<asp:Localize runat="server" Text="<%$Resources:Fields,NotEnabled %>" />
							</span>
                            <asp:HyperLink ID="lnkAddMFA" runat="server" NavigateUrl="~/UserProfile_ChangeMFA.aspx" Text="<%$Resources:Main,UserProfile_MfaAdd %>" />
                        </asp:PlaceHolder>
                        <asp:PlaceHolder ID="plcChangeMFA" runat="server">
							<span class="badge">
								<asp:Localize runat="server" Text="<%$Resources:Fields,Enabled %>" />
							</span>
                            <asp:HyperLink ID="lnkChangeMFA" runat="server" NavigateUrl="~/UserProfile_ChangeMFA.aspx" Text="<%$Resources:Main,UserProfile_MfaChange %>" />
                        </asp:PlaceHolder>
						</div>
					</fieldset>
				</asp:PlaceHolder>

				<asp:Panel runat="server" ID="pnlSubPass_Oauth">
					<h4>
						<asp:Literal runat="server" Text="<%$ Resources:Main,UserProfile_OAuthLinked %>" />
					</h4>
					<p>
						<asp:Literal runat="server" ID="litOAuth" />
					</p>
					<button
						type="button"
						class="btn btn-default"
						onclick="dlgUnlinkOauthOpen()">
						<i class="fas fa-trash-alt mr2"></i>
						<asp:Literal runat="server" Text="<%$ Resources:Buttons,UnlinkAccount %>" />
					</button>
				</asp:Panel>

				<asp:Panel runat="server" ID="pnlSubPass_Pass">
					<%-- Change Password --%>
					<fieldset class="fieldset-gray mt5 mx-auto mb4 mb3-xs w10 px4 w-100-sm">
						<legend>
							<asp:Localize runat="server" Text="<%$Resources:Main,UserProfile_TabChangePassword %>" /></legend>
						<div class="ma0 mb4 pa0">
							<tstsc:LabelEx
								class="fs-90"
								runat="server"
								Text="<% $Resources:Messages,UserDetails_ToChangePassword %>" />
							<p>
								<small>
									<%= string.Format(Resources.Main.Register_PasswordMinLength, Membership.MinRequiredPasswordLength) %>
									<%= PasswordNoNamesLegend %>
								</small>
							</p>
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								AssociatedControlID="txtCurrentPassword"
								class="label-above"
								runat="server"
								Text="<% $Resources:Main,UserProfile_CurrentPassword %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtCurrentPassword"
								runat="server"
								MaxLength="128"
								TextMode="Password" />
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								AssociatedControlID="txtNewPassword"
								class="label-above"
								ID="Literal5"
								runat="server"
								Text="<% $Resources:Main,UserProfile_NewPassword %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtNewPassword"
								MaxLength="128"
								runat="server"
								TextMode="Password" />
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								AssociatedControlID="txtConfirmPassword"
								class="label-above"
								ID="Literal6"
								runat="server"
								Text="<% $Resources:Main,UserProfile_ConfirmNewPassword %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtConfirmPassword"
								MaxLength="128"
								runat="server"
								TextMode="Password" />
						</div>
					</fieldset>

					<%-- Change Password Reset Question & Answer --%>
					<fieldset class="fieldset-gray mt5 mx-auto mb6 mb5-xs w10 px4 w-100-sm">
						<legend>
							<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,UserProfile_ChangePasswordQuestionAnswer %>" />
						</legend>
						<div class="ma0 mb4 pa0">
							<tstsc:LabelEx
								CssClass="fs-90"
								ID="LabelEx1"
								runat="server"
								Text="<% $Resources:Messages,UserDetails_ToChangePasswordQuestionAnswer %>" />
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								AssociatedControlID="txtCurrentPasswordForQuestion"
								class="label-above"
								ID="LabelEx2"
								runat="server"
								Text="<% $Resources:Main,UserProfile_CurrentPassword %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtCurrentPasswordForQuestion"
								MaxLength="25"
								runat="server"
								TextMode="Password" />
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								class="label-above"
								AssociatedControlID="txtQuestion"
								ID="LabelEx3"
								runat="server"
								Text="<% $Resources:Fields,Question %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtQuestion"
								MaxLength="255"
								runat="server"
								TextMode="SingleLine" />
						</div>
						<div class="ma0 mb5 pa0">
							<tstsc:LabelEx
								class="label-above"
								AssociatedControlID="txtAnswer"
								ID="LabelEx4"
								runat="server"
								Text="<% $Resources:Fields,Answer %>" />
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								DisabledCssClass="u-input disabled"
								ID="txtAnswer"
								MaxLength="255"
								runat="server"
								TextMode="SingleLine" />
						</div>
					</fieldset>
				</asp:Panel>
			</div>
		</asp:Panel>



		<!-- Change Culture Settings -->
		<asp:Panel runat="server" ID="pnlCulture" CssClass="TabControlPanel">
			<div class="mt5 mx-auto mb6 mb5-xs w10 w-100-sm">
				<div class="mb4">
					<asp:Literal
						ID="Literal7"
						runat="server"
						Text="<% $Resources:Main,UserDetails_ToChangeCultureSettings %>" />
				</div>
				<div class="mb5">
					<tstsc:LabelEx
						AssociatedControlID="ddlUserCulture"
						CssClass="label-above"
						ID="lblUserCulture"
						runat="server"
						Text="<% $Resources:Main,UserProfile_UserCulture %>" />
					<tstsc:UnityDropDownListEx
						CssClass="u-dropdown is-active w-100"
						DataTextField="Value"
						DataValueField="Key"
						DisabledCssClass="u-dropdown disabled"
						ID="ddlUserCulture"
						NoValueItem="true"
						NoValueItemText="<%$Resources:Dialogs,UserProfile_UserCultureNoValue %>"
						runat="server" />
				</div>
				<div class="mb4">
					<tstsc:LabelEx
						CssClass="label-above"
						AssociatedControlID="ddlUserTimezone"
						ID="ddlUserTimezoneLabel"
						runat="server"
						Text="<% $Resources:Main,UserProfile_UserTimezone %>" />
					<tstsc:UnityDropDownListEx
						CssClass="u-dropdown is-active w-100"
						DataTextField="Value"
						DataValueField="Key"
						DisabledCssClass="u-dropdown disabled"
						ID="ddlUserTimezone"
						NoValueItem="true"
						NoValueItemText="<%$Resources:Dialogs,UserProfile_UserCultureNoValue %>"
						runat="server" />
				</div>
			</div>
		</asp:Panel>



		<!-- User's History Actions -->
		<asp:Panel ID="pnlActions" runat="server" CssClass="TabControlPanel">
			<div class="TabControlHeader">
				<asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
				<asp:Label ID="lblCount" runat="server" Font-Bold="True" />
				<asp:Localize runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
				<asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
				<asp:Localize runat="server" Text="<%$Resources:Main,Global_Items %>" />.
                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
				<div class="btn-group priority3">
					<tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" Text="<%$Resources:Buttons,Refresh %>"
						NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdUserActivity"
						ClientScriptMethod="load_data()">
                        <span class="fas fa-sync"></span>
					</tstsc:HyperLinkEx>
					<tstsc:DropMenu ID="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
						ClientScriptServerControlId="grdUserActivity" ClientScriptMethod="apply_filters()">
						<DropMenuItems>
							<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
							<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						</DropMenuItems>
					</tstsc:DropMenu>
				</div>
			</div>
			<tstsc:SortedGrid ID="grdUserActivity" runat="server" EnableViewState="false" CssClass="DataGrid"
				WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryActivityService"
				VisibleCountControlId="lblCount" TotalCountControlId="lblTotal" HeaderCssClass="Header"
				SubHeaderCssClass="SubHeader" FilterInfoControlId="lblFilterInfo"
				RowCssClass="Normal" DisplayAttachments="false" AllowEditing="false">
			</tstsc:SortedGrid>
		</asp:Panel>
	</div>


	<tstsc:DialogBoxPanel
		ID="dlgUnlinkOauth"
		Modal="true"
		runat="server"
		Width="600px">
		<p class="mx4 mt3">
			<asp:Literal runat="server" ID="litUnlinkSumm" />
		</p>

		<div class="pa4">
			<div class="u-box-3">
				<ul class="u-box_list">
					<li class="ma0 mb4 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="cleanUserName" Text="<%$ Resources:Fields,NewUsername %>" Required="true" AppendColon="true" />
						<tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="cleanUserName" runat="server" TextMode="SingleLine" MaxLength="64" ReadOnly="true" />
					</li>
					<li class="ma0 mb4 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="unlinkOauth_Password" Text="<%$ Resources:Fields,NewPassword %>" Required="true" AppendColon="true" data-password="true" />
						<tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkOauth_Password" runat="server" TextMode="Password" MaxLength="128" data-password="true" />
					</li>
					<li class="ma0 mb4 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="unlinkOauth_VerifyPassword" Text="<%$ Resources:Fields,ConfirmPassword %>" Required="true" AppendColon="true" data-password="true" />
						<tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkOauth_VerifyPassword" runat="server" TextMode="Password" MaxLength="128" data-password="true" />
                        <p id="unlinkOauth_PasswordsNotMatch" class="visibility-none my3 pa3 br3 bg-warning fs-90">
                            <asp:Literal runat="server" Text="<%$ Resources:Messages,UserDetails_NewPasswordError2 %>" />
                        </p>
					</li>
					<li class="ma0 mb4 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="unlinkOauth_NewQuestion" Text="<%$ Resources:Fields,PasswordQuestion %>" Required="true" AppendColon="true" data-password="true" />
						<tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkOauth_NewQuestion" runat="server" TextMode="SingleLine" MaxLength="255" data-password="true" />
					</li>
					<li class="ma0 mb4 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="unlinkOauth_NewAnswer" Text="<%$ Resources:Fields,PasswordAnswer %>" Required="true" AppendColon="true" data-password="true" />
						<tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkOauth_NewAnswer" runat="server" TextMode="SingleLine" MaxLength="255" data-password="true" />
					</li>
					<li class="ma0 mb4 pa0">
						<div class="btn-group mt4 ml_u-box-label">
							<tstsc:ButtonEx runat="server"
								ID="btnUnlinkOauth"
								CausesValidation="False"
								Text="<%$ Resources:Buttons,UnlinkAccount %>"
								UseSubmitBehavior="false"
								OnClientClick="" />
							<button
								class="btn btn-default"
								onclick="$find('<%= dlgUnlinkOauth.ClientID %>').close();return false;"
								type="button">
								<asp:Literal runat="server" Text="<%$Resources:Buttons,Cancel%>" />
							</button>
						</div>
					</li>
				</ul>
			</div>

		</div>
	</tstsc:DialogBoxPanel>



	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/HistoryActivityService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>

	<script type="text/javascript">
		// for creating rss token
		$('#<%=chkRssEnabled.ClientID%>').on('switchChange.bootstrapSwitch', function (event, state) {
			if (state) {
				var btnGenerateNewToken = $get('<%=btnGenerateNewToken.ClientID %>');
				btnGenerateNewToken.click();
			}
			else {
				var txtRssToken = $get('<%=txtRssToken.ClientID%>');
				txtRssToken.value = '';
			}
		});

		// functions for avatar logo editing
		$('#<%#filAttachment.ClientID%>').on("change", function () {
			var fileName = $('#<%#filAttachment.ClientID%>').val();
			$('#filNameVisible').text(fileName);
		});
		$('.avatar-instructions').on("click", function () {
			$('.avatar-buttons').addClass('active');
			$('.avatar-instructions').addClass('not-active');
        });

        // handling Unlinking dialog
        var unlinkOauth_Password = document.getElementById('<%=this.unlinkOauth_Password.ClientID%>');
        var unlinkOauth_VerifyPassword = document.getElementById('<%=this.unlinkOauth_VerifyPassword.ClientID%>');
        var unlinkOauth_NewQuestion = document.getElementById('<%=this.unlinkOauth_NewQuestion.ClientID%>');
        var unlinkOauth_NewAnswer = document.getElementById('<%=this.unlinkOauth_NewAnswer.ClientID%>');
        var btnUnlinkOauth = document.getElementById('<%=this.btnUnlinkOauth.ClientID%>');
        var msgPasswordMismatch = document.getElementById("unlinkOauth_PasswordsNotMatch");

        // listeners for input fields
		unlinkOauth_Password.addEventListener("input", function () {
			this.setAttribute('value', this.value)
			checkOauthUnlinkSubmit();
		}, false);
		unlinkOauth_VerifyPassword.addEventListener("input", function () {
			this.setAttribute('value', this.value)
			checkOauthUnlinkSubmit()
        }, false);
        unlinkOauth_NewQuestion.addEventListener("input", function () {
			this.setAttribute('value', this.value)
			checkOauthUnlinkSubmit()
        }, false);
        unlinkOauth_NewAnswer.addEventListener("input", function () {
			this.setAttribute('value', this.value)
			checkOauthUnlinkSubmit()
		}, false);

        function checkOauthUnlinkSubmit() {
            var anyInputNull = !unlinkOauth_Password.value || !unlinkOauth_VerifyPassword.value || !unlinkOauth_NewQuestion.value || !unlinkOauth_NewAnswer.value,
                passwordsDoNotMatch = unlinkOauth_Password.value && unlinkOauth_VerifyPassword.value && unlinkOauth_Password.value != unlinkOauth_VerifyPassword.value;
            // only enable the unlink button when all fields are filled in and the passwords match
            btnUnlinkOauth.disabled = anyInputNull || passwordsDoNotMatch;
            // if the passwords do not match show an info box explaining
            if (passwordsDoNotMatch) {
                msgPasswordMismatch.classList.remove("visibility-none");
            } else {
                msgPasswordMismatch.classList.add("visibility-none");
            }
      
        }

        function dlgUnlinkOauthOpen() {
            checkOauthUnlinkSubmit();
            $find('<%= dlgUnlinkOauth.ClientID %>').display();
            return false;
        }
    </script>
</asp:Content>
