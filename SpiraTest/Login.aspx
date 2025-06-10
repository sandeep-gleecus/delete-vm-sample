<%@ Page Language="c#" ValidateRequest="false" CodeBehind="Login.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Login" MasterPageFile="~/MasterPages/Login.Master" Async="true" %>
	<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
		<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
		</asp:Content>
		<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
			<tstsc:LoginEx ID="LoginUser" runat="server" EnableViewState="false" SkinID="LoginForm">
				<LayoutTemplate>
					<tstsc:MessageBox ID="FailureText" runat="server" SkinID="MessageBox"/>
					<asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" ValidationGroup="LoginUserValidationGroup"/>
					<asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="<%$Resources:Messages,Login_NeedToEnterUserName %>" ToolTip="<%$Resources:Messages,Login_NeedToEnterUserName %>" ValidationGroup="LoginUserValidationGroup"/>
					<asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" CssClass="failureNotification" ErrorMessage="<%$Resources:Messages,Login_NeedToEnterPassword %>" ToolTip="<%$Resources:Messages,Login_NeedToEnterPassword %>" ValidationGroup="LoginUserValidationGroup"/>
					<div  class="box-form" id="login-body" >
						<%-- admin set temp message to users --%>
						<asp:PlaceHolder ID="plcAdminMessage" runat="server" Visible="false">
							<div class="bg-peach br4 my3 px5 py4 near-black" id="alertAdminMessage" role="alert">
								<asp:Literal ID="ltrAdminMessage" runat="server"/>
							</div>
						</asp:PlaceHolder>
						<asp:PlaceHolder ID="plcOauthMsg" runat="server" Visible="false">
							<div class="bg-peach br4 my3 px5 py4 near-black" id="alertOauthMessage" role="alert">
								<asp:Literal ID="ltrOauthMsg" Text="" runat="server"/>
							</div>
						</asp:PlaceHolder>
						<h1>
							<asp:Literal ID="ltrLogin" runat="server" Text="<%$ Resources:Main,Login_PleaseLogin%>"/>
							<asp:Literal ID="ltrOAuthNoAccount" runat="server" Visible="false" Text="<%$ Resources:Main,Login_OAuthProvider_NoAccount%>"/>
						</h1>
						<h2 runat="server" id="headingConnectAccount" class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong" Visible="false">
							-
							<tstsc:HyperLinkEx ID="lnkConnectAccount" ClientScriptMethod="showLoginForm()" NavigateUrl="javascript:void(0)" runat="server" Text="<%$ Resources:Main,Login_Oauth_ConnectAccount %>"/>
						</h2>
						<div runat="server" id="pnlLoginForm" ClientIDMode="Static">
						<div class="form-group">
							<tstsc:TextBoxEx ID="UserName" TextMode="SingleLine" runat="server" MaxLength="50" SkinID="FormControlTop" placeholder="<%$ Resources:Main,Login_UserName %>"/>
							<tstsc:TextBoxEx ID="Password" TextMode="Password" runat="server" MaxLength="128" SkinID="FormControlBottom" placeholder="<%$ Resources:Main,Login_Password %>"/>
						</div>
							<div class="mx3 mb4 relative flex">
								<asp:CheckBox runat="server" ID="RememberMe"/>
								<asp:Label AssociatedControlID="RememberMe" CssClass="pr3" ID="lblRememberMe" runat="server" Text="<%$Resources:Main,Login_KeepLoggedIn %>"/>
							</div>
							<div class="btn-wrapper-wide px3 px0-xs">
								<tstsc:ButtonEx SkinID="ButtonPrimary" ID="btnLogin" type="submit" CommandName="Login" runat="server" Text="<%$ Resources:Buttons,LogIn %>" ValidationGroup="LoginUserValidationGroup" ClientIDMode="static"/>
							</div>
						</div>
						<p class="fs-90 mb4 mt3 px3">
							<tstsc:HyperLinkEx ID="lnkForgotUserPassword" NavigateUrl="EmailPassword.aspx" runat="server" Text="<%$ Resources:Main,Login_ForgotPassword %>"/>
							<asp:PlaceHolder ID="plcNeedAccount" runat="server">
								&nbsp&nbsp&nbsp
								<tstsc:HyperLinkEx ID="lnkNeedAccount" SkinID="ButtonPrimary" NavigateUrl="NeedAccount.aspx" runat="server" Text="<%$ Resources:Main,Login_RegisterForAccount %>"/>
							</asp:PlaceHolder>
						</p>
						<h2 class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong" id="headingOAuthCreateAccount" runat="server" Visible="false">
							-
							<tstsc:HyperLinkEx ID="lnkOAuthCreateAccount" NavigateUrl="NeedAccount.aspx" runat="server" Text="<%$ Resources:Main,Login_Oauth_CreateAccount %>"/>
						</h2>
						<h2 class="fs-h3 ma0 fw-b px3 px0-sm mt5 mt4-xs mb5 blue-strong" id="headingOAuthRefresh" runat="server" Visible="false">
							-
							<tstsc:HyperLinkEx ID="ltrOAuthRefresh" NavigateUrl="Login.aspx" runat="server" Text="<%$ Resources:Buttons,Cancel %>"/>
						</h2>
					</div>
					<asp:PlaceHolder runat="server" ID="wrapperOAuthOptions" Visible="false">
						<div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
							<div class="mx3 pa4 ba br3 b-subtle bg-page-accent justify-between flex flex-wrap">
								<h2 class="fs-h4 mt0 mb4 blue-strong w-100">
									<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:Main,Login_Oauth_ProviderTitle%>"/>
								</h2>
								<%-- OAuth login options --%>
								<asp:PlaceHolder ID="plcOAuth" runat="server"/>
							</div>
						</div>
					</asp:PlaceHolder>
					<%-- admin set permmanent notice to users --%>
					<asp:PlaceHolder ID="plcNotice" runat="server">
						<aside class="fs-90 o-60 pa4 my3">
							<asp:Literal ID="ltrNotice" runat="server"/>
						</aside>
					</asp:PlaceHolder>
				</LayoutTemplate>
			</tstsc:LoginEx>
	<script type="text/javascript">
		(function () {
			var username = document.getElementById("UserName");
			var password = document.getElementById("Password");
			var btnLogin = document.getElementById("btnLogin");

			var oAuthLoginAttempt = <%= GetOAuthLogin() %>;

			// listeners for input fields
			username.addEventListener("input", function () {
				this.setAttribute('value', this.value)
				checkButton();
			}, false);
			password.addEventListener("input", function () {
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
			<%-- function celebrate() {
				var a = new Date(), b = a.getUTCDate(), c = a.getMonth();
				if (b === 25 && c == 6) {
					var cake = document.getElementById('is-cake-time'),
						years = a.getYear() - 106;
					cake.classList.remove('dn');
					cake.setAttribute("title", "Today is Inflectra's birthday (" + years + " years old today)");
				}
			} --%>
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
