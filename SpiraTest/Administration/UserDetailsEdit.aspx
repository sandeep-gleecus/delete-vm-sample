<%@ Page
	Language="c#"
	CodeBehind="UserDetailsEdit.aspx.cs"
	AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.Administration.UserDetailsEdit"
	MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="df mb5">
		<span class="UserNameAvatar avatar-mid mr4">
			<tstsc:ImageEx ID="imgUserAvatar" runat="server" />
		</span>
        <div>
		    <h2>
			    <tstsc:LabelEx ID="lblUserName" runat="server" />
			    <small>
				    <asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_EditUser %>" />
			    </small>
		    </h2>
		    <p class="my0">
			    <asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_AddEditUser_Legend %>" />
		    </p>
		    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
		    <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
			    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
	
            <div class="btn-group mt4">
		        <tstsc:ButtonEx ID="btnUpdateTop" SkinID="ButtonPrimary" runat="server" AlternateText="Update" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
		        <tstsc:ButtonEx ID="btnCancelTop" runat="server" AlternateText="Cancel" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
	        </div>
        </div>
	</div>


	<fieldset class="w-100 px4 mx0 my5 fieldset-gray">
		<legend>
			<asp:Localize runat="server" Text="<%$Resources:Fields,LastDateDetails %>" />
		</legend>
		<div class="df flex-wrap-sm px5 px0-sm">
			<p class="w-25 w-50-sm w-100-xs" runat="server" id="sectLastLock">
				<asp:Literal ID="ltrLastLockoutDateLabel" runat="server" Text="<%$Resources:Fields,LastLockoutDate %>" />:
                <span class="badge">
				    <asp:Literal ID="ltrLastLockoutDate" runat="server" />
                </span>
			</p>
			<p class="w-25 w-50-sm w-100-xs">
				<asp:Literal ID="ltrLastActivityDateLabel" runat="server" Text="<%$Resources:Fields,LastActivityDate %>" />:
                <span class="badge">
					<asp:Literal ID="ltrLastActivityDate" runat="server" />
                </span>
			</p>
			<p class="w-25 w-50-sm w-100-xs">
				<asp:Literal ID="ltrLastLoginDateLabel" runat="server" Text="<%$Resources:Fields,LastLoginDate %>" />:
                <span class="badge">
    				<asp:Literal ID="ltrLastLoginDate" runat="server" />
                </span>
			</p>
			<p class="w-25 w-50-sm w-100-xs" runat="server" id="sectLastChng">
				<asp:Literal ID="ltrLastPasswordChangedDateLabel" runat="server" Text="<%$Resources:Fields,LastPasswordChangeDate %>" />:
                <span class="badge">
					<asp:Literal ID="ltrLastPasswordChangedDate" runat="server" />
                </span>
			</p>
		</div>
	</fieldset>



    <fieldset class="w-100 px4 mx0 my5 fieldset-gray u-wrapper width_md">
		<legend>
			<asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_UserInformation %>" />
		</legend>
		<div class="u-box-50 px5 px4-md px0-sm">
			<ul class="u-box_list pl0" >
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtFirstName" 
                        Required="true" 
                        runat="server" 
                        Text="<%$Resources:Fields,FirstName %>" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtFirstName" 
                        MaxLength="50" 
                        runat="server" 
                        TextMode="SingleLine" 
                        />
					<asp:RequiredFieldValidator 
                        ControlToValidate="txtFirstName" 
                        ErrorMessage="<%$Resources:Messages,UserDetails_FirstNameRequired %>" 
                        ID="Requiredfieldvalidator1" 
                        runat="server" 
                        Text="*" 
                        />
				</li>
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        ID="LabelEx1" 
                        runat="server" 
                        Text="<%$Resources:Fields,MiddleInitial %>" 
                        Required="false" 
                        AssociatedControlID="txtMiddleInitial" 
                        AppendColon="true" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtMiddleInitial" 
                        runat="server" 
                        TextMode="SingleLine" 
                        MaxLength="1" 
                        />
                </li>
                <li class="ma0 mb5 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtLastName" 
                        ID="LabelEx2" 
                        Required="true" 
                        runat="server" 
                        Text="<%$Resources:Fields,LastName %>" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtLastName" 
                        MaxLength="50" 
                        runat="server" 
                        TextMode="SingleLine" 
                        />
					<asp:RequiredFieldValidator 
                        ID="Requiredfieldvalidator3" 
                        runat="server" Text="*" 
                        ErrorMessage="<%$Resources:Messages,UserDetails_LastNameRequired %>" 
                        ControlToValidate="txtLastName"
                        />
                </li>
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtFirstName" 
                        ID="LabelEx3" 
                        runat="server" 
                        Text="<%$Resources:Fields,UserName %>" 
                        Required="true" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtUserName" 
                        runat="server" 
                        TextMode="SingleLine" 
                        MaxLength="50" 
                        autocomplete="off" 
                        />
					<asp:RequiredFieldValidator 
                        ID="Requiredfieldvalidator4" 
                        runat="server" Text="*" 
                        ErrorMessage="<%$Resources:Messages,UserDetails_UserNameRequired %>" 
                        ControlToValidate="txtUserName"/>
					<asp:RegularExpressionValidator 
                        runat="server" 
                        Text="*" 
                        ErrorMessage="<%$Resources:Messages,UserDetails_UserNameInvalid %>" 
                        ControlToValidate="txtUserName" 
                        ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_USER_NAME%>" ID="Regularexpressionvalidator2" 
                        />
                </li>
			</ul>
        </div>
        <div class="u-box-50 px5 px4-md px0-sm">
			<ul class="u-box_list pl0">
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtDepartment" 
                        ID="LabelEx4" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,Department %>" 
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtDepartment" 
                        runat="server" 
                        TextMode="SingleLine" 
                        MaxLength="50" 
                        />
				</li>
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtOrganization" 
                        ID="txtOrganizationLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,Organization %>" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal" 
                        ID="txtOrganization" 
                        MaxLength="50" 
                        runat="server" 
                        TextMode="SingleLine" 
                        />

                </li>
                <li class="ma0 mb2 pa0">
					<tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtEmailAddress" 
                        ID="LabelEx5" 
                        Required="true" 
                        runat="server" 
                        Text="<%$Resources:Fields,EmailAddress %>" 
                        />
					<tstsc:UnityTextBoxEx 
                        CssClass="u-input u-input-minimal"
                        ID="txtEmailAddress" 
                        MaxLength="255" 
                        runat="server" 
                        TextMode="SingleLine" 
                        />
					<asp:RequiredFieldValidator 
                        ID="Requiredfieldvalidator2" 
                        runat="server" 
                        Text="*"
						ErrorMessage="<%$Resources:Messages,UserDetails_EmailAddressRequired %>" 
                        ControlToValidate="txtEmailAddress" 
                        />
					<asp:RegularExpressionValidator 
                        runat="server" 
                        ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EMAIL_ADDRESS%>"
						ControlToValidate="txtEmailAddress" 
                        Text="*" 
                        ErrorMessage="<%$Resources:Messages,UserDetails_EmailAddressInvalid %>"
						ID="Regularexpressionvalidator1" 
                        />
                </li>  
			</ul>
		</div>
	</fieldset>




	<fieldset class="w-100 px4 mx0 my5 fieldset-gray u-wrapper width_md">
		<legend>
			<asp:Literal runat="server" Text="<%#Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%>" />
			<asp:Localize runat="server" Text="<%$Resources:Main,Settings %>" />
		</legend>
		<div class="u-box-50 px5 px4-md px0-sm">
			<ul class="u-box_list pl0" >
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        ID="LabelEx6" 
                        runat="server" 
                        Text="<%$Resources:Fields,EmailEnabledYn %>" 
                        Required="true" 
                        AssociatedControlID="chkEmailEnabled" 
                        AppendColon="true" 
                        />
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkEmailEnabled" 
                        />
				</li>
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        ID="LabelEx7" 
                        runat="server" 
                        Text="<%$Resources:Fields,SystemAdministrator %>" 
                        Required="true" 
                        AssociatedControlID="chkAdminYn" 
                        AppendColon="true" 
                        />
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkAdminYn" 
                        />
                </li>
                <li class="ma0 mb4 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="chkReportAdmin" 
                        Required="true" 
                        runat="server" 
                        Text="<%$Resources:Fields,ReportAdmin %>" 
                        ToolTip="<%$Resources:Main,UserDetails_ReportAdminNotes %>"
                        />
                    <tstsc:CheckBoxYnEx runat="server" ID="chkReportAdmin" />
                </li>
                <li class="ma0 mb4 pa0" id="portfolioFormGroup" runat="server" visible="false">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="chkPortfolioAdmin" 
                        Required="true" 
                        runat="server" 
                        Text="<%$Resources:Fields,PortfolioViewer %>" 
                        ToolTip="<%$Resources:Main,UserDetails_PortfolioViewerNotes %>"
                        />
                    <tstsc:CheckBoxYnEx runat="server" ID="chkPortfolioAdmin" />
                </li>
                <li class="ma0 mb4 pa0">
					<tstsc:LabelEx 
                        ID="LabelEx8" 
                        runat="server" 
                        Text="<%$Resources:Fields,ActiveYn %>" 
                        Required="true" 
                        AssociatedControlID="chkActiveYn" 
                        AppendColon="true" 
                        />
					<tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" />
                </li>
                <li class="ma0 mb5 pa0" runat="server" id="sectLock">
					<tstsc:LabelEx 
                        ID="ddlLockedOutLabel" 
                        runat="server" 
                        Text="<%$Resources:Fields,LockedOut %>" 
                        Required="true" 
                        AssociatedControlID="chkLockedOut" 
                        AppendColon="true" 
                        />
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkLockedOut" 
                        />
                </li>
                <li class="ma0 mb3 pa0">
					<tstsc:LabelEx 
                        ID="RssTokenLabel" 
                        runat="server" 
                        AssociatedControlID="txtRssToken" 
                        Text="<%$Resources:Main,UserProfile_EnableRSSFeeds %>" 
                        />
					<tstsc:CheckBoxYnEx 
                        ID="chkRssEnabled" 
                        runat="server" 
                        />

                </li>
                <li class="ma0 mb2 pa0">
					<tstsc:LabelEx 
                        runat="server" 
                        AssociatedControlID="txtRssToken" 
                        Text="<%$Resources:Main,UserProfile_RssTokenApiKey %>" 
                        />
					<tstsc:LabelEx 
                        CssClass="pointer orange-hover transition-all text-obscured"
                        data-copytoclipboard='true'
                        ID="txtRssToken" 
                        runat="server" 
                        MaxLength="255" 
                        ReadOnly="true"
                        title="<%$Resources:Buttons,CopyToClipboard %>"
                        />
                </li>
                <li class="ma0 mb4 pa0">
                    <span class="ml_u-box-label">
					    <tstsc:ButtonEx 
                            ID="btnGenerateNewToken" 
                            runat="server" 
                            CausesValidation="false" 
                            Text="<%$Resources:Buttons,GenerateNew %>" 
                            />
                    </span>
                </li>
            </ul>
		</div>

        <div class="u-box-50 px5 px4-md px0-sm">
			<ul class="u-box_list pl0" >
			    <asp:PlaceHolder ID="plcLDAPInfo" runat="server">
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblLdapUserLabel" 
                            runat="server" 
                            Text="<%$Resources:Main,UserDetails_LdapManagedUser %>" 
                            AppendColon="true" 
                            CssClass="u-box_list_label"
                            />
						<tstsc:LabelEx 
                            ID="lblLdapUser" 
                            runat="server"                             
                            />
					</li>
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="txtLdapDnLabel" 
                            AssociatedControlID="txtLdapDn" 
                            runat="server" 
                            Text="LDAP DN:" 
                            data-ldap="true" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input u-input-minimal" 
                            ID="txtLdapDn" 
                            runat="server" 
                            TextMode="SingleLine" 
                            MaxLength="255" 
                            data-ldap="true" 
                            />
                    </li>
                    <li class="ma0 mb4 pa0">
                        <button
                            class="btn btn-default ml_u-box-label"
                            onclick="dlgUnlinkLdapOpen();"
                            type="button"
                            >
                            <i class="fas fa-trash-alt mr2"></i>
                            <asp:Localize  
                                runat="server" 
                                Text="<%$ Resources:Buttons,UnlinkAccount %>"
                                />
                        </button>
                    </li>
			    </asp:PlaceHolder>


			    <asp:PlaceHolder ID="plcOauthInfo" runat="server">
                    <li class="ma0 mb2 pa0">
						<tstsc:LabelEx 
                            AppendColon="true" 
                            AssociatedControlID="txtOauthProviderInfo"
                            runat="server" 
                            Text="<%$ Resources:Main,Admin_UserDetails_OAuthLogin %>" 
                            />
						<tstsc:LabelEx 
                            ID="txtOauthProviderInfo" 
                            runat="server" 
                            MaxLength="32"  
                            ReadOnly="true" 
                            />
					</li>
                    <li class="ma0 mb4 pa0">
                        <button
                            class="btn btn-default ml_u-box-label"
                            onclick="dlgUnlinkOauthOpen();"
                            type="button"
                            >
                            <i class="fas fa-trash-alt mr2"></i>
                            <asp:Localize  
                                runat="server" 
                                Text="<%$ Resources:Buttons,UnlinkAccount %>"
                                />
                        </button>
                    </li>
			    </asp:PlaceHolder>


			    <asp:PlaceHolder runat="server" ID="plcPassInfo">
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblNewPassword" 
                            runat="server" 
                            AssociatedControlID="txtNewPassword" 
                            Text="<%$Resources:Fields,NewPassword %>" 
                            Required="true" 
                            AppendColon="true" 
                            data-password="true" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input u-input-minimal"
                            ID="txtNewPassword" 
                            runat="server" 
                            TextMode="Password" 
                            MaxLength="128" 
                            data-password="true" 
                            />
                    </li>
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblConfirmPassword" 
                            runat="server" 
                            AssociatedControlID="txtConfirmPassword" 
                            Text="<%$Resources:Fields,ConfirmPassword %>" 
                            Required="true" AppendColon="true" data-password="true" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input u-input-minimal" 
                            ID="txtConfirmPassword" 
                            runat="server" 
                            TextMode="Password" 
                            MaxLength="128" 
                            data-password="true" 
                            />
                    </li>
                    <li class="ma0 mb5 pa0">
                        <div class="ml_u-box-label">
						    <p 
                                class="fs-80 gray mb2" 
                                data-password="true"
                                >
                                <%=String.Format(Resources.Main.Register_PasswordMinLength, Membership.MinRequiredPasswordLength)%>
						    </p>
						    <p 
                                class="fs-80 gray" 
                                data-password="true"
                                >
                                <%=String.Format(Resources.Main.Register_PasswordMinNonAlphaChars, Membership.MinRequiredNonAlphanumericCharacters)%>
						    </p>
                        </div>
                    </li>
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblPasswordQuestion" 
                            runat="server" 
                            AssociatedControlID="txtPasswordQuestion" 
                            Text="<%$Resources:Fields,PasswordQuestion %>" 
                            Required="true" AppendColon="true" data-password="true" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input u-input-minimal" 
                            ID="txtPasswordQuestion" 
                            runat="server" 
                            TextMode="SingleLine" 
                            MaxLength="255" 
                            data-password="true" 
                            />
                    </li>
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblPasswordAnswer" 
                            runat="server" 
                            AssociatedControlID="txtConfirmPassword" 
                            Text="<%$Resources:Fields,PasswordAnswer %>" 
                            Required="true" 
                            AppendColon="true" 
                            data-password="true" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input u-input-minimal" 
                            data-password="true" 
                            ID="txtPasswordAnswer" 
                            runat="server" 
                            TextMode="SingleLine" 
                            MaxLength="255" 
                            />
                    </li>
                    <asp:PlaceHolder runat="server" ID="plcLinkToLDAP" Visible="false">
                        <li class="ma0 mb4 pa0">
                            <button
                                class="btn btn-default ml_u-box-label"
                                onclick="displayLinkToLDAP();"
                                type="button"
                                >
                                <i class="fas fa-link mr2"></i>
                                <asp:Localize  
                                    runat="server" 
                                    Text="<%$ Resources:Buttons,LinkToLDAP %>"
                                    />
                            </button>
                        </li>
                    </asp:PlaceHolder>
    			</asp:PlaceHolder>

                <asp:PlaceHolder runat="server" ID="plcMfaSettings">
                    <li class="ma0 mb4 pa0">
						<tstsc:LabelEx 
                            ID="lblMfaEnabledLabel" 
                            runat="server" 
                            AssociatedControlID="lblMfaEnabled" 
                            Text="<%$Resources:Fields,MfaEnabledLong %>" 
                            Required="true" 
                            AppendColon="true" 
                            />
                        <span>
						    <tstsc:LabelEx 
                                ID="lblMfaEnabled" 
                                runat="server" 
                                CssClass="black"
                                />
                            <span class="Spacer"></span>
                            <tstsc:ButtonEx runat="server" ID="btnRemoveMfa" Text="<%$Resources:Buttons,Deactivate %>" Visible="false"
                                Confirmation="true" ConfirmationMessage="<%$Resources:Messages,ChangeMfa_RemoveConfirm %>" />
                        </span>
                    </li>
                </asp:PlaceHolder>
            </ul>
		</div>
	</fieldset>




	<fieldset class="w-100 px4 mx0 my5 fieldset-gray u-wrapper width_md">
		<legend>
			<asp:Localize runat="server" Text="<%$ Resources:Main,UserDetails_MembershipMapping %>" />
		</legend>
		

		<tstsc:TabControl ID="tclUserDetails" CssClass="TabControl2" TabWidth="120" TabHeight="25"
			TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
			DisabledTabCssClass="TabDisabled" runat="server">
			<TabPages>
				<tstsc:TabPage runat="server" ID="tabProjects" Caption="<%$ Resources:Main,ProjectMembership_Title %>" TabPageControlId="pnlProjects" />
				<tstsc:TabPage runat="server" ID="tabDataSync" Caption="<%$ Resources:Main,UserDetails_TabDataMapping %>" TabPageControlId="pnlDataSync" />
				<tstsc:TabPage runat="server" ID="tabTara" Caption="<%$ Resources:Main,Admin_UserDetails_TabTaraVault %>" TabPageControlId="pnlTaraVault" />
			</TabPages>
		</tstsc:TabControl>


		<asp:Panel ID="pnlProjects" runat="server" Width="100%">
			<div class="TabControlHeader">
				<div class="btn-group priority1">
					<tstsc:DropMenu ID="btnMembershipUpdate" GlyphIconCssClass="mr3 fas fa-save" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
					<tstsc:DropMenu ID="btnMembershipAdd" GlyphIconCssClass="mr3 fas fa-plus" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add%>" />
					<tstsc:DropMenu ID="btnMembershipDelete" GlyphIconCssClass="mr3 fas fa-trash-alt" runat="server" CausesValidation="False"
						Text="<%$Resources:Buttons,Delete%>" Confirmation="true" ConfirmationMessage="Are you sure you want to remove this user from the selected project(s)?" />
				</div>
			</div>
			<tstsc:GridViewEx ID="grdProjectMembership" CssClass="DataGrid" runat="server" Width="100%"
				AutoGenerateColumns="False" DataKeyNames="ProjectId">
				<HeaderStyle CssClass="Header" />
				<Columns>
					<tstsc:TemplateFieldEx HeaderStyle-CssClass="TickIcon priority1 text-center" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="priority1">
						<ItemTemplate>
							<tstsc:CheckBoxEx runat="server" ID="chkDeleteMembership" />
						</ItemTemplate>
					</tstsc:TemplateFieldEx>
					<tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Admin_Projects%>" DataField="ProjectName" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
					<tstsc:TemplateFieldEx HeaderText="Active" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
						<ItemTemplate>
							<%# GlobalFunctions.DisplayYnFlag((bool)((ProjectUser)Container.DataItem).Project.IsActive)%>
						</ItemTemplate>
					</tstsc:TemplateFieldEx>
					<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectId %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
						<ItemTemplate>
							<%#GlobalFunctions.ARTIFACT_PREFIX_PROJECT + String.Format(GlobalFunctions.FORMAT_ID, ((ProjectUser)Container.DataItem).ProjectId)%>
						</ItemTemplate>
					</tstsc:TemplateFieldEx>
					<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectRole %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
						<ItemTemplate>
							<tstsc:DropDownListEx CssClass="DropDownList" runat="server"
								DataSource="<%# projectRoles %>" DataTextField="Name" DataValueField="ProjectRoleId"
								ID="ddlProjectRole" Width="200px" SelectedValue='<%#((ProjectUser)Container.DataItem).ProjectRoleId.ToString() %>' />
						</ItemTemplate>
					</tstsc:TemplateFieldEx>
				</Columns>
			</tstsc:GridViewEx>
		</asp:Panel>
		<asp:Panel ID="pnlDataSync" runat="server" Width="100%" CssClass="row data-entry-wide DataEntryForm">
			<div class="col-sm-9 col-md-offset-1">
				<table class="DataGrid">
					<asp:Repeater ID="rptDataSyncMappings" runat="server">
						<ItemTemplate>
							<tr>
								<td class="priority1">
									<tstsc:LabelEx ID="lblPlugInName" AssociatedControlID="txtExternalKey" runat="server" Text='<%#:((DataSyncUserMappingView) Container.DataItem).DataSyncSystemDisplayName + " ID:"%>' />
								</td>
								<td class="priority1">
									<tstsc:TextBoxEx CssClass="text-box" ID="txtExternalKey" runat="server"
										TextMode="SingleLine" MaxLength="50" Text='<%#((DataSyncUserMappingView) Container.DataItem).ExternalKey%>'
										MetaData='<%#((DataSyncUserMappingView) Container.DataItem).DataSyncSystemId%>' />
								</td>
							</tr>
						</ItemTemplate>
					</asp:Repeater>
				</table>
			</div>
		</asp:Panel>
		<asp:Panel ID="pnlTaraVault" runat="server" Width="100%">
			<div class="row data-entry-wide DataEntryForm view-edit">
				<div class="col-md-6">
					<div class="form-group row">
						<div class="DataLabel col-sm-10">
							<tstsc:LabelEx ID="lblTVMaxReached" runat="server" />
						</div>
					</div>
					<div class="form-group row">
						<div class="DataLabel col-sm-4">
							<tstsc:LabelEx ID="LabelEx9" runat="server" Text="<%$ Resources:Main,Admin_User_TaraEnable %>" AssociatedControlID="chkTaraEnable" />
						</div>
						<div class="DataEntry col-sm-8">
							<tstsc:CheckBoxYnEx runat="server" ID="chkTaraEnable" />
						</div>
					</div>
					<div class="form-group row" id="trTaraAcct" style="display: none">
						<div class="DataLabel col-sm-4">
							<tstsc:LabelEx ID="LabelEx10" runat="server" Text="<%$ Resources:Main,Admin_User_TaraVaultLogin %>" AppendColon="true" />
						</div>
						<div class="DataLabel col-sm-8">
							<tstsc:LabelEx ID="lblTaraAccount" runat="server" />&nbsp(<asp:Label runat="server" ID="lblTaraAccountId" />)
						</div>
					</div>
					<div class="form-group row mb2" id="trTaraPass" style="display: none">
						<div class="DataLabel col-sm-4">
							<tstsc:LabelEx ID="LabelEx11" runat="server" Text="<%$ Resources:Main,Admin_User_TaraVaultPass %>" AssociatedControlID="txtTaraPass" Required="true" AppendColon="true" />
						</div>
						<div class="DataEntry col-sm-8">
							<tstsc:TextBoxEx CssClass="text-box" ID="txtTaraPass" runat="server" Width="200px"
								TextMode="SingleLine" MaxLength="40" />
						</div>
					</div>
				</div>
				<div class="col-md-6">
					<div class="row">
						<div class="TabControlHeader">
							<div class="btn-group">
								<asp:HyperLink ID="HyperLink2" CssClass="btn btn-default" runat="server" Text="<%$ Resources:Buttons,AddProjects %>" NavigateUrl='<%# "~/Administration/UserDetailsAddTaraVault.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + Request.QueryString[GlobalFunctions.PARAMETER_USER_ID] %>' />
							</div>
						</div>
					</div>
					<table>
						<asp:TableRow ID="trTaraProjects" runat="server" Visible="false">
							<asp:TableCell ColumnSpan="2">
								<asp:Label ID="Label1" runat="server" Text="<%$ Resources:Main,Admin_User_TaraVaultProjects %>" AppendColon="true" />
								<tstsc:GridViewEx ID="grdTaraProjects" CssClass="DataGrid" runat="server" PageSize="15"
									AllowSorting="true" AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False"
									Width="100%" AutoGenerateColumns="False" HeaderStyle-CssClass="Header"
									SubHeaderStyle-CssClass="SubHeader" ShowHeaderWhenEmpty="true"
									FooterStyle-CssClass="Footer" DataKeyNames="ProjectId">
									<EmptyDataTemplate>
										<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:Messages,Admin_User_TaraVaultNoProjects %>" />
										<asp:HyperLink ID="HyperLink1" runat="server" Text="<%$ Resources:Buttons,AddProjects %>" NavigateUrl='<%# "~/Administration/UserDetailsAddTaraVault.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + Request.QueryString[GlobalFunctions.PARAMETER_USER_ID] %>' />
									</EmptyDataTemplate>
									<Columns>
										<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Fields,ProjectId %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
											<ItemTemplate>
												<%# "[" + GlobalFunctions.ARTIFACT_PREFIX_PROJECT + ":" + (int)((Project)Container.DataItem).ProjectId + "]" %>
											</ItemTemplate>
										</tstsc:TemplateFieldEx>
										<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
											<ItemTemplate>
												<tstsc:LabelEx ID="LabelEx12" runat="server" ToolTip='<%# "<u>" + ((Project) Container.DataItem).Name + "</u><br />" + ((Project) Container.DataItem).Description %>'
													Text="<%# ((Project)Container.DataItem).Name %>" />
											</ItemTemplate>
										</tstsc:TemplateFieldEx>
										<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
											<ItemTemplate>
												<tstsc:LinkButtonEx ID="lnkRemoveTaraProj" runat="server" Text="<%$ Resources:Buttons,Remove %>" CommandArgument="<%# ((Project)Container.DataItem).ProjectId %>" CommandName="Remove" />
											</ItemTemplate>
										</tstsc:TemplateFieldEx>
									</Columns>
								</tstsc:GridViewEx>
							</asp:TableCell>
						</asp:TableRow>
					</table>
				</div>
			</div>
		</asp:Panel>
	</fieldset>



	<div class="btn-group mt5">
		<tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" AlternateText="Update" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
		<tstsc:ButtonEx ID="btnCancel" runat="server" AlternateText="Cancel" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
	</div>




	<tstsc:DialogBoxPanel 
        runat="server" 
        ID="dlgUnlinkOauth" 
        Modal="true"
        Width="600px"
        Title="<%$ Resources:Main,Admin_UserDetails_UnlinkOAuth_Title %>"
        >
		<p class="mx4 mt3"><asp:Literal runat="server" ID="litUnlinkOauth" Text="<%$ Resources:Main,Admin_UserDetails_UnlinkOAuthLdap_Summary %>" /></p>
        <div class="pa4">
            <div class="u-box-3">
			    <ul class="u-box_list" >
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
                                SkinID="ButtonPrimary"
					            ID="btnUnlinkOauth"
					            CausesValidation="False"
					            Text="<%$ Resources:Buttons,UnlinkAccount %>"
					            UseSubmitBehavior="false"
					            OnClientClick="" />
				            <button 
                                type="button"
                                class="btn btn-default"
					            onclick="$find('<%= dlgUnlinkOauth.ClientID %>').close();return false;">
					            <asp:Literal runat="server" Text="<%$Resources:Buttons,Cancel%>" />
				            </button>
			            </div>
                    </li>
                </ul>
            </div>
        </div>
	</tstsc:DialogBoxPanel>

	<tstsc:DialogBoxPanel 
        runat="server" 
        ID="dlgLinkToLDAP" 
        Modal="true"
        Width="600px"
        Title="<%$ Resources:Main,Admin_UserDetails_LinkToLdap_Title %>"
        >
		<p class="mx4 mt3"><asp:Literal runat="server" ID="Literal2" Text="<%$ Resources:Main,Admin_UserDetails_LinkToLdap_Summary %>" /></p>
        <div class="pa4">
            <div class="u-box-3">
			    <ul class="u-box_list" >
                    <li class="ma0 mb4 pa0">
    				    <tstsc:LabelEx runat="server" AssociatedControlID="linkLdap_NewLdapDn" Text="LDAP DN:" Required="true" AppendColon="true" />
	    			    <tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="linkLdap_NewLdapDn" runat="server" TextMode="SingleLine" MaxLength="255" />
		            </li>
                    <li class="ma0 mb4 pa0">
		                <div class="btn-group mt4 ml_u-box-label">
				            <tstsc:ButtonEx runat="server"
					            ID="btnLinkToLdapSubmit"
                                SkinID="ButtonPrimary"
					            CausesValidation="False"
					            Text="<%$ Resources:Buttons,Link %>"
					            UseSubmitBehavior="false"
					            OnClientClick="dlgLinkToLDAP_click(); return false;" />
				            <button 
                                type="button"
                                class="btn btn-default"
					            onclick="$find('<%= dlgLinkToLDAP.ClientID %>').close();return false;">
					            <asp:Literal runat="server" Text="<%$Resources:Buttons,Cancel%>" />
				            </button>
			            </div>
                    </li>
                </ul>
            </div>
        </div>
	</tstsc:DialogBoxPanel>

	<tstsc:DialogBoxPanel 
        runat="server" 
        ID="dlgUnlinkLDAP" 
        Modal="true"
        Width="600px"
        Title="<%$ Resources:Main,Admin_UserDetails_UnlinkLdap_Title %>"
        >
		<p class="mx4 mt3"><asp:Literal runat="server" ID="litUnlinkLdap" Text="<%$ Resources:Main,Admin_UserDetails_UnlinkOAuthLdap_Summary %>" /></p>
        <div class="pa4">
            <div class="u-box-3">
			    <ul class="u-box_list" >
                    <li class="ma0 mb4 pa0">
    				    <tstsc:LabelEx runat="server" AssociatedControlID="unlinkLdap_Password" Text="<%$ Resources:Fields,NewPassword %>" Required="true" AppendColon="true" data-password="true" />
	    			    <tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkLdap_Password" runat="server" TextMode="Password" MaxLength="128" data-password="true" />
		            </li>
                    <li class="ma0 mb4 pa0">
    				    <tstsc:LabelEx runat="server" AssociatedControlID="unlinkLdap_VerifyPassword" Text="<%$ Resources:Fields,ConfirmPassword %>" Required="true" AppendColon="true" data-password="true" />
	    			    <tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkLdap_VerifyPassword" runat="server" TextMode="Password" MaxLength="128" data-password="true" />
                        <p id="unlinkLdap_PasswordsNotMatch" class="visibility-none my3 pa3 br3 bg-warning fs-90">
                            <asp:Literal runat="server" Text="<%$ Resources:Messages,UserDetails_NewPasswordError2 %>" />
                        </p>
                    </li>
                    <li class="ma0 mb4 pa0">
			            <tstsc:LabelEx runat="server" AssociatedControlID="unlinkLdap_NewQuestion" Text="<%$ Resources:Fields,PasswordQuestion %>" Required="true" AppendColon="true" data-password="true" />
				        <tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkLdap_NewQuestion" runat="server" TextMode="SingleLine" MaxLength="255" data-password="true" />
			        </li>
                    <li class="ma0 mb4 pa0">
    				    <tstsc:LabelEx runat="server" AssociatedControlID="unlinkLdap_NewAnswer" Text="<%$ Resources:Fields,PasswordAnswer %>" Required="true" AppendColon="true" data-password="true" />
	    			    <tstsc:UnityTextBoxEx CssClass="u-input u-input-minimal" ID="unlinkLdap_NewAnswer" runat="server" TextMode="SingleLine" MaxLength="255" data-password="true" />
		            </li>
                    <li class="ma0 mb4 pa0">
		                <div class="btn-group mt4 ml_u-box-label">
				            <tstsc:ButtonEx runat="server"
					            ID="btnUnlinkLdap"
                                SkinID="ButtonPrimary"
					            CausesValidation="False"
					            Text="<%$ Resources:Buttons,UnlinkAccount %>"
					            UseSubmitBehavior="false"
					            OnClientClick="" />
				            <button 
                                type="button"
                                class="btn btn-default"
					            onclick="$find('<%= dlgUnlinkLDAP.ClientID %>').close();return false;">
					            <asp:Literal runat="server" Text="<%$Resources:Buttons,Cancel%>" />
				            </button>
			            </div>
                    </li>
                </ul>
            </div>
        </div>
	</tstsc:DialogBoxPanel>

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManagaer" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/UserService.svc" />
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

		//Handle the enabling and disabling of TaraVault
		$('#<%=chkTaraEnable.ClientID%>').on('switchChange.bootstrapSwitch', function (event, checked) {
			var value = ((checked) ? '' : 'none');
			$('#trTaraAcct').css('display', value);
			$('#trTaraPass').css('display', value);
		});

        //Update based on setting.
        $(document).ready(function () {
            var checked = $('#<%=chkTaraEnable.ClientID%>').prop("checked");
            var value = ((checked) ? '' : 'none');
            $('#trTaraAcct').css('display', value);
            $('#trTaraPass').css('display', value);

            //Make the RSS Token / API Key select on clicking
			$('#<%=txtRssToken.ClientID%>').on("click", function () {
                $(this).select();
            });
        });

	    //Display link to LDAP page
	    function displayLinkToLDAP() {
			$find('<%= dlgLinkToLDAP.ClientID%>').display();
	    }

	    //The user wants to link to LDAP
	    function dlgLinkToLDAP_click()
	    {
	        //Get the new LDAP DN
	        var linkLdap_NewLdapDn = $get('<%=linkLdap_NewLdapDn.ClientID%>');
	        var ldapDn = linkLdap_NewLdapDn.value;
	        if (globalFunctions.isNullOrUndefined(ldapDn) || ldapDn == '')
	        {
	            alert(resx.Admin_UserDetailsEdit_LdapDnRequired);
	            return;
	        }

	        //Attempt to link the user to ldap
	        var userId = <%=user.UserId%>;
	        Inflectra.SpiraTest.Web.Services.Ajax.UserService.User_LinkUserToLdapDn(userId, ldapDn, dlgLinkToLDAP_click_success, dlgLinkToLDAP_click_failure);
	    }
	    function dlgLinkToLDAP_click_success(message)
	    {
	        //If we get a message back, there was a validation issue
	        if (message)
	        {
	            //Display the message
	            globalFunctions.display_error_message(null, message);
	        }
	        else
	        {
                //Close the dialog
	            $find('<%= dlgLinkToLDAP.ClientID %>').close();

	            //Reload the page since it needs to reload from the server
	            window.location.reload(true);
	        }
	    }
	    function dlgLinkToLDAP_click_failure(ex)
	    {
	        //Display the message
	        globalFunctions.display_error(null, ex);
        }

        // handling Unlinking dialog for OAUTH
        var unlinkOauth_Password = document.getElementById('<%=this.unlinkOauth_Password.ClientID%>');
        var unlinkOauth_VerifyPassword = document.getElementById('<%=this.unlinkOauth_VerifyPassword.ClientID%>');
        var unlinkOauth_NewQuestion = document.getElementById('<%=this.unlinkOauth_NewQuestion.ClientID%>');
        var unlinkOauth_NewAnswer = document.getElementById('<%=this.unlinkOauth_NewAnswer.ClientID%>');
        var btnUnlinkOauth = document.getElementById('<%=this.btnUnlinkOauth.ClientID%>');
        var msgPasswordMismatchOauth = document.getElementById("unlinkOauth_PasswordsNotMatch");

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
                msgPasswordMismatchOauth.classList.remove("visibility-none");
            } else {
                msgPasswordMismatchOauth.classList.add("visibility-none");
            }
        }

        function dlgUnlinkOauthOpen() {
            checkOauthUnlinkSubmit();
            $find('<%= dlgUnlinkOauth.ClientID %>').display();
            return false;
        }

        // handling Unlinking dialog for LDAP
        var unlinkLdap_Password = document.getElementById('<%=this.unlinkLdap_Password.ClientID%>');
        var unlinkLdap_VerifyPassword = document.getElementById('<%=this.unlinkLdap_VerifyPassword.ClientID%>');
        var unlinkLdap_NewQuestion = document.getElementById('<%=this.unlinkLdap_NewQuestion.ClientID%>');
        var unlinkLdap_NewAnswer = document.getElementById('<%=this.unlinkLdap_NewAnswer.ClientID%>');
        var btnUnlinkLdap = document.getElementById('<%=this.btnUnlinkLdap.ClientID%>');
        var msgPasswordMismatchLdap = document.getElementById("unlinkLdap_PasswordsNotMatch");

        // listeners for input fields
		unlinkLdap_Password.addEventListener("input", function () {
			this.setAttribute('value', this.value)
			checkLdapUnlinkSubmit();
		}, false);
		unlinkLdap_VerifyPassword.addEventListener("input", function () {
			this.setAttribute('value', this.value)
            checkLdapUnlinkSubmit();
        }, false);
        unlinkLdap_NewQuestion.addEventListener("input", function () {
			this.setAttribute('value', this.value)
            checkLdapUnlinkSubmit();
        }, false);
        unlinkLdap_NewAnswer.addEventListener("input", function () {
			this.setAttribute('value', this.value)
            checkLdapUnlinkSubmit();
		}, false);

        function checkLdapUnlinkSubmit() {
            var anyInputNull = !unlinkLdap_Password.value || !unlinkLdap_VerifyPassword.value || !unlinkLdap_NewQuestion.value || !unlinkLdap_NewAnswer.value,
                passwordsDoNotMatch = unlinkLdap_Password.value && unlinkLdap_VerifyPassword.value && unlinkLdap_Password.value != unlinkLdap_VerifyPassword.value;
            // only enable the unlink button when all fields are filled in and the passwords match
            btnUnlinkLdap.disabled = anyInputNull || passwordsDoNotMatch;
            // if the passwords do not match show an info box explaining
            if (passwordsDoNotMatch) {
                msgPasswordMismatchLdap.classList.remove("visibility-none");
            } else {
                msgPasswordMismatchLdap.classList.add("visibility-none");
            }
        }

        function dlgUnlinkLdapOpen() {
            checkLdapUnlinkSubmit();
            $find('<%= dlgUnlinkLDAP.ClientID %>').display();
            return false;
        }
        
    </script>
</asp:Content>
