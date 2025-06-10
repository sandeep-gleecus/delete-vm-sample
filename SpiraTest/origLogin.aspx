<%@ Page
	Language="c#"
	ValidateRequest="false"
	CodeBehind="Login.aspx.cs"
	AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.Login"
	MasterPageFile="~/MasterPages/Login.Master"
	Async="true" %>

<%@ Register
	TagPrefix="tstsc"
	Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" %>
<asp:Content
	ContentPlaceHolderID="cplHead"
	runat="server"
	ID="Content1">
</asp:Content>

<asp:Content
	ContentPlaceHolderID="cplMainContent"
	runat="server"
	ID="Content2">



	<%-- inflectra anniversary image --%>
	<p
		id="is-cake-time"
		class="tc dn w-100">
		<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" viewBox="0 0 5.8208332 5.8208335">
			<g transform="matrix(.00917 0 0 .00917.44.441)" fill="#4d4d4d">
				<path d="m507.96 311.64c-4.549 1.235-9.33 1.945-14.279 1.945-11.824 0-22.996-3.742-32.06-10.443-9.06 6.701-20.234 10.443-32.06 10.443-11.824 0-22.996-3.742-32.06-10.443-9.06 6.701-20.232 10.443-32.06 10.443-11.825 0-22.996-3.742-32.06-10.443-9.06 6.701-20.232 10.443-32.06 10.443-11.825 0-22.996-3.742-32.06-10.443-9.06 6.701-20.232 10.443-32.06 10.443-11.824 0-22.993-3.742-32.06-10.443-9.06 6.701-20.233 10.443-32.06 10.443-11.824 0-22.993-3.742-32.06-10.443-9.07 6.701-20.236 10.443-32.06 10.443-11.824 0-22.993-3.742-32.06-10.443-9.06 6.701-20.233 10.443-32.06 10.443-4.954 0-9.731-.71-14.281-1.945v48.36h477.36v-48.36" fill="#fdcb26" />
				<g fill="#de6f96">
					<path d="m30.6 471.18c0 .872.117 1.716.257 2.549h476.85c.141-.833.256-1.677.256-2.549v-95.88h-477.36v95.88" />
					<path d="m30.6 295.59c4.407 1.726 9.223 2.692 14.281 2.692 13.568 0 25.425-6.873 32.06-17.14 6.634 10.271 18.491 17.14 32.06 17.14 13.568 0 25.425-6.873 32.06-17.14 6.631 10.272 18.488 17.14 32.06 17.14 13.568 0 25.426-6.873 32.06-17.14 6.631 10.271 18.489 17.14 32.06 17.14 13.568 0 25.426-6.873 32.06-17.14 6.63 10.272 18.488 17.14 32.06 17.14 13.568 0 25.426-6.873 32.06-17.14 6.631 10.272 18.488 17.14 32.06 17.14 13.569 0 25.426-6.873 32.06-17.14 6.631 10.272 18.488 17.14 32.06 17.14 13.569 0 25.426-6.873 32.06-17.14 6.631 10.272 18.488 17.14 32.06 17.14 5.059 0 9.875-.967 14.281-2.692 13.715-5.374 23.385-18.231 23.385-33.26 0-1.506-.129-2.98-.318-4.44.199-.759.318-1.539.318-2.341v-46.11c0-6.702-7.289-12.136-16.279-12.136h-226.41v-7.65-7.65-30.921c0-8.449-6.852-15.3-15.301-15.3h-8.161c-8.449 0-15.3 6.851-15.3 15.3v30.921 7.65 7.65h-226.4c-8.99 0-16.279 5.432-16.279 12.136v46.11c0 .802.119 1.582.318 2.341-.19 1.457-.318 2.935-.318 4.44 0 15.02 9.67 27.885 23.385 33.26" />
					<path d="m523.26 489.02h-507.96c-8.449 0-15.3 8.617-15.3 19.247v1.282c0 10.631 6.851 19.247 15.3 19.247h507.96c8.449 0 15.301-8.616 15.301-19.247v-1.282c0-10.627-6.852-19.247-15.301-19.247" />
				</g>
				<path d="m272.34 126.1c.576.021 1.154.092 1.719-.024 17.826-3.685 35.09-27.452 31.455-48.52-3.369-19.51-17.531-27.666-25.727-41.895-3.396-5.89-5.17-13.779-3.91-21.515.816-5-.064-5.79-3.473-2.038-8.752 9.624-18.667 22.274-23.513 32.375-5.548 11.27-6.071 19.324-2.843 31.377.823 3.01 1.671 5.343 2.781 7.656 1.953 4.061-.177 7.252-4.905 5.417-2.35-.912-4.511-2.154-6.328-3.623-3.944-3.186-5.447-3.204-4.367 1.747 3.951 18.12 20.512 38.32 39.11 39.04" fill="#fdcb26" />
			</g></svg>
	</p>

    <p
		class="dn br4 my3 px5 py4 white tc fw-b fs-125"
		id="alert-IE-support"
        role="alert"
		style="background-color: #e63d3d;"
        >
        <asp:Localize
			ID="ltrAlertIE"
			Text="<%$ Resources:Messages,Login_IE11_SupportEnd%>"
			runat="server" />
	</p>




	<tstsc:LoginEx
		ID="LoginUser"
		runat="server"
		EnableViewState="false"
		SkinID="LoginForm">
		<LayoutTemplate>
			<tstsc:MessageBox
				ID="FailureText"
				runat="server"
				SkinID="MessageBox" />
			<asp:ValidationSummary
				ID="LoginUserValidationSummary"
				runat="server"
				ValidationGroup="LoginUserValidationGroup" />
			<asp:RequiredFieldValidator
				ID="UserNameRequired"
				runat="server"
				ControlToValidate="UserName"
				ErrorMessage="<%$Resources:Messages,Login_NeedToEnterUserName %>"
				ToolTip="<%$Resources:Messages,Login_NeedToEnterUserName %>"
				ValidationGroup="LoginUserValidationGroup" />
			<asp:RequiredFieldValidator
				ID="PasswordRequired"
				runat="server"
				ControlToValidate="Password"
				CssClass="failureNotification"
				ErrorMessage="<%$Resources:Messages,Login_NeedToEnterPassword %>"
				ToolTip="<%$Resources:Messages,Login_NeedToEnterPassword %>"
				ValidationGroup="LoginUserValidationGroup" />

			<div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
				<%-- admin set temp message to users --%>
				<asp:PlaceHolder
					ID="plcAdminMessage"
					runat="server"
					Visible="false">
					<div
						class="bg-peach br3 my3 px5 py4 near-black"
						id="alertAdminMessage"
                        role="alert"
                        >
						<asp:Literal
							ID="ltrAdminMessage"
							runat="server" />
					</div>
				</asp:PlaceHolder>

				<asp:PlaceHolder
					ID="plcOauthMsg"
					runat="server"
					Visible="false">
					<div
						class="bg-peach br4 my3 px5 py4 near-black"
                        id="alertOauthMessage"
						role="alert">
						<asp:Literal
							ID="ltrOauthMsg"
							Text=""
							runat="server" />
					</div>
				</asp:PlaceHolder>

				<h1 class="ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong">
					<asp:Literal
						ID="ltrLogin"
						runat="server"
						Text="<%$ Resources:Main,Login_PleaseLogin%>" />
                    <asp:Literal
						ID="ltrOAuthNoAccount"
						runat="server"
                        Visible="false"
						Text="<%$ Resources:Main,Login_OAuthProvider_NoAccount%>" />
				</h1>

                <h2
                    runat="server"
                    id="headingConnectAccount"
                    class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong"
                    Visible="false"
                    >
                    - 
                    <tstsc:HyperLinkEx 
                        ID="lnkConnectAccount"
                        ClientScriptMethod="showLoginForm()"
                        NavigateUrl="javascript:void(0)"
                        runat="server"
						Text="<%$ Resources:Main,Login_Oauth_ConnectAccount %>" 
                        />
                </h2>

                <div 
                    runat="server" 
                    id="pnlLoginForm"
                    ClientIDMode="Static"
                    >
				    <div class="mb5 mt4 mx3 relative">
					    <tstsc:UnityTextBoxEx
						    CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
						    ID="UserName"
						    MaxLength="50"
						    runat="server"
						    TextMode="SingleLine"
						    ClientIDMode="static" />
					    <asp:Label
						    AssociatedControlID="UserName"
						    CssClass="label-slideup"
						    runat="server"
						    Text="<%$ Resources:Main,Login_UserName %>" />
				    </div>
				    <div class="my4 mx3 relative">
					    <tstsc:UnityTextBoxEx
						    CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
						    ID="Password"
						    MaxLength="128"
						    runat="server"
						    TextMode="Password"
						    ClientIDMode="static" />
					    <asp:Label
						    AssociatedControlID="Password"
						    CssClass="label-slideup"
						    runat="server"
						    Text="<%$ Resources:Main,Login_Password %>" />
				    </div>
				    <div class="mx3 mb4 relative df">
					    <asp:CheckBox
						    runat="server"
						    ID="RememberMe" />
					    <asp:Label
						    AssociatedControlID="RememberMe"
						    CssClass="pr3"
						    ID="lblRememberMe"
						    runat="server"
						    Text="<%$Resources:Main,Login_KeepLoggedIn %>" />
				    </div>

				    <div class="btn-wrapper-wide px3 px0-xs">
					    <tstsc:ButtonEx
						    SkinID="ButtonPrimary"
						    ID="btnLogin"
						    type="submit"
						    CommandName="Login"
						    runat="server"
						    Text="<%$ Resources:Buttons,LogIn %>"
						    ValidationGroup="LoginUserValidationGroup"
						    ClientIDMode="static" />
				    </div>
                </div>




				<p class="fs-90 mb4 mt3 px3">
					<tstsc:HyperLinkEx ID="lnkForgotUserPassword" NavigateUrl="EmailPassword.aspx" runat="server"
						Text="<%$ Resources:Main,Login_ForgotPassword %>" />
					<asp:PlaceHolder ID="plcNeedAccount" runat="server">
                        &nbsp&nbsp&nbsp|&nbsp&nbsp&nbsp
                        <tstsc:HyperLinkEx ID="lnkNeedAccount" NavigateUrl="NeedAccount.aspx" runat="server"
							Text="<%$ Resources:Main,Login_RegisterForAccount %>" />
					</asp:PlaceHolder>
				</p>



                <h2
                    class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong"
                    id="headingOAuthCreateAccount"
                    runat="server"
                    Visible="false"
                    >
                    - 
                    <tstsc:HyperLinkEx 
                        ID="lnkOAuthCreateAccount" 
                        NavigateUrl="NeedAccount.aspx" 
                        runat="server"
						Text="<%$ Resources:Main,Login_Oauth_CreateAccount %>" 
                        />
				</h2>

                <h2
                    class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong"
                    id="headingOAuthRefresh"
                    runat="server"
                    Visible="false"
                    >
                    - 
                    <tstsc:HyperLinkEx 
                        ID="ltrOAuthRefresh" 
                        NavigateUrl="Login.aspx" 
                        runat="server"
						Text="<%$ Resources:Buttons,Cancel %>" 
                        />
				</h2>
			</div>



            <asp:PlaceHolder
                runat="server"
                ID="wrapperOAuthOptions"
                Visible="false"
                >
                <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
                    <div class="mx3 pa4 ba br3 b-subtle bg-page-accent justify-between df flex-wrap">
                        <h2 class="fs-h4 mt0 mb4 blue-strong w-100">
                            <asp:Literal
						        ID="Literal1"
						        runat="server"
						        Text="<%$ Resources:Main,Login_Oauth_ProviderTitle%>" 
                                />
                        </h2>
			            <%-- OAuth login options --%>
			            <asp:PlaceHolder
				            ID="plcOAuth"
				            runat="server" />
                    </div>
                </div>
            </asp:PlaceHolder>

			<%-- admin set permmanent notice to users --%>
			<asp:PlaceHolder
				ID="plcNotice"
				runat="server">
				<aside class="fs-90 o-60 pa4 my3">
					<asp:Literal
						ID="ltrNotice"
						runat="server" />
				</aside>
			</asp:PlaceHolder>
		</LayoutTemplate>
	</tstsc:LoginEx>

	<%-- MFA One Time Password Section --%>
	<asp:PlaceHolder ID="OneTimePasswordSection" runat="server" Visible="false">
		<div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
			<h1 class="ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong">
				<asp:Localize
					runat="server"
					Text="<%$ Resources:Main,Login_PleaseEnterOneTimePassword%>" />
			</h1>
			<div>
				<asp:Label ID="OneTimePasswordLabel" AssociatedControlID="OneTimePassword" runat="server" CssClass="label-slideup" Text="<%$Resources:Main,Login_OneTimePassword %>" />
				<tstsc:TextBoxEx ID="OneTimePassword" runat="server" MaxLength="6" placeholder="" SkinID="OneTimePassword" inputmode="numeric" pattern="[0-9]*" autocomplete="one-time-code" />
			</div>
			<div class="mt4">
				<tstsc:ButtonEx ID="btnLogInAfterOTP" runat="server" Text="<%$ Resources:Buttons,LogIn %>" SkinID="ButtonPrimary" CausesValidation="false" UseSubmitBehavior="true"/>
			</div>
		</div>
    </asp:PlaceHolder>



	<script type="text/javascript">
		(function () {
			var username = document.getElementById("UserName");
			var password = document.getElementById("Password");
			var btnLogin = document.getElementById("btnLogin");

			var oAuthLoginAttempt = <%= GetOAuthLogin() %>;

			// listeners for input fields
			username && username.addEventListener("input", function () {
				this.setAttribute('value', this.value)
				checkButton();
			}, false);
			password && password.addEventListener("input", function () {
				this.setAttribute('value', this.value)
				checkButton()
			}, false);

			function checkButton() {
				var adminOAuthAttempt = oAuthLoginAttempt && (username.value == "administrator");
				btnLogin.disabled = !username.value || !password.value || adminOAuthAttempt;

				// give useful message in btnlogin if user id 1 is attempting to connect to oauth
				//if (adminOAuthAttempt) {
				//    btnLogin.value = Inflectra.SpiraTest.Web.GlobalResources.LoginOAuth_NotAdmin;
				//} else if (btnLogin.value != Inflectra.SpiraTest.Web.GlobalResources.AjaxFormManager_Login) {
				//    btnLogin.value = Inflectra.SpiraTest.Web.GlobalResources.AjaxFormManager_Login;
				//}
			}
			window.onload = function () {
				checkButton();
				setTimeout(function () {
					username.setAttribute('value', username.value);
					password.setAttribute('value', password.value);
				}, 500);
			}



			// anniversary of inflectra corporation founding
			function celebrate() {
				var a = new Date(), b = a.getUTCDate(), c = a.getMonth();
				if (b === 25 && c == 6) {
					var cake = document.getElementById('is-cake-time'),
						years = a.getYear() - 106;
					cake.classList.remove('dn');
					cake.setAttribute("title", "Today is Inflectra's birthday (" + years + " years old today)");
				}
			}
            window.onload = function () {
                celebrate();

                //IE 11 handling
                if (!Array.prototype.find) {
                    document.getElementById("alert-IE-support").classList.remove("dn");
                }
            };
		})();

		function showLoginForm() {
			var pnlLoginForm = document.getElementById("pnlLoginForm");
			pnlLoginForm.classList.remove("dn");
		}
    </script>
</asp:Content>

