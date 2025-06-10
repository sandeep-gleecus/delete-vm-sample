<%@ Page
	Language="c#"
	ValidateRequest="false"
	CodeBehind="NeedAccount.aspx.cs"
	AutoEventWireup="True"
	Async="true"
	Inherits="Inflectra.SpiraTest.Web.NeedAccount"
	MasterPageFile="~/MasterPages/Login.Master" %>

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
	<tstsc:CreateUserWizardEx
		ID="RegisterUser"
		runat="server"
		EnableViewState="false"
		DisableCreatedUser="true">
		<LayoutTemplate>
			<asp:PlaceHolder
				ID="wizardStepPlaceholder"
				runat="server" />
			<asp:PlaceHolder
				ID="navigationPlaceholder"
				runat="server" />
		</LayoutTemplate>



		<WizardSteps>
			<asp:CreateUserWizardStep
				ID="RegisterUserWizardStep"
				runat="server">
				<ContentTemplate>
					<tstsc:MessageBox
						ID="ErrorMessage"
						SkinID="MessageBox"
						runat="server" />
					<asp:ValidationSummary
						ID="RegisterUserValidationSummary"
						runat="server"
						ValidationGroup="RegisterUserValidationGroup" />

					<div class="px5 px4-sm px0-xs mx-auto mb7 mb6-xs w10 w-100-sm tc">
						<!-- Headers -->
						<h1 class="tc fw-b mt5 mt4-xs mb0 blue-strong">
							<asp:Localize
								ID="Localize2"
								runat="server"
								Text="<%$Resources:Main,Register_Heading %>" />
						</h1>
						<p class="mt0 mb5 ml3 tl fs-125">
							<asp:Localize
								ID="Localize3"
								runat="server"
								Text="<%$Resources:Main,Register_InstructionText %>" />
						</p>
						<!-- UserName -->
						<asp:Panel class="mb5 mt4 mx3 relative" runat="server" ID="userid">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ID="UserName"
								ClientIDMode="static"
								runat="server"
								MaxLength="50" />
							<tstsc:LabelEx
								ID="UserNameLabel"
								runat="server"
								AssociatedControlID="UserName"
								Required="true"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,UserName %>" />
							<asp:RequiredFieldValidator
								ID="UserNameRequired"
								runat="server"
								ControlToValidate="UserName"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_UserNameRequired %>"
								ToolTip="<%$Resources:Messages,Register_UserNameRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
							<asp:RegularExpressionValidator
								ID="UserNameRegEx"
								runat="server"
								ControlToValidate="UserName"
								Text="*"
								ValidationGroup="RegisterUserValidationGroup"
								ErrorMessage="<%$Resources:Messages,Register_UserNameNotValid %>"
								ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_USER_NAME%>" />
						</asp:Panel>
						<!-- Email -->
						<div class="mb5 mt4 mx3 relative">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="Email"
								runat="server"
								MaxLength="255" />
							<tstsc:LabelEx
								ID="EmailLabel"
								runat="server"
								AssociatedControlID="Email"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,EmailAddress %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ID="EmailRequired"
								runat="server"
								ControlToValidate="Email"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_EmailRequired %>"
								ToolTip="<%$Resources:Messages,Register_EmailRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
							<asp:RegularExpressionValidator
								ID="EmailRegEx"
								runat="server"
								ControlToValidate="Email"
								Text="*"
								ValidationGroup="RegisterUserValidationGroup"
								ErrorMessage="<%$Resources:Messages,Register_EmailNotValid %>"
								ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EMAIL_ADDRESS%>" />
						</div>
						<!-- First Name -->
						<div class="mb5 mt4 mx3 relative">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="FirstName"
								runat="server"
								MaxLength="50" />
							<tstsc:LabelEx
								ID="FirstNameLabel"
								runat="server"
								AssociatedControlID="FirstName"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,FirstName %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ID="RequiredFieldValidator3"
								runat="server"
								ControlToValidate="FirstName"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_FirstNameRequired %>"
								ToolTip="<%$Resources:Messages,Register_FirstNameRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
						</div>
						<!-- Last Name -->
						<div class="mb5 mt4 mx3 relative">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="LastName"
								runat="server"
								MaxLength="50" />
							<tstsc:LabelEx ID="LastNameLabel"
								runat="server"
								AssociatedControlID="LastName"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,LastName %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ID="RequiredFieldValidator4"
								runat="server"
								ControlToValidate="LastName"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_LastNameRequired %>"
								ToolTip="<%$Resources:Messages,Register_LastNameRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
						</div>
						<!-- Password -->
						<div class="mb5 mt4 mx3 relative" runat="server" id="pass1">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="Password"
								runat="server"
								TextMode="Password"
								MaxLength="50" />
							<tstsc:LabelEx
								ID="PasswordLabel"
								runat="server"
								AssociatedControlID="Password"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,Password %>" Required="true" />
							<asp:RequiredFieldValidator
								ID="PasswordRequired"
								runat="server"
								ControlToValidate="Password"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_PasswordRequired %>"
								ToolTip="<%$Resources:Messages,Register_PasswordRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
						</div>
						<!-- Confirm Password -->
						<div class="mt4 mx3 relative" runat="server" id="pass2">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="ConfirmPassword"
								runat="server"
								TextMode="Password"
								MaxLength="50"
								aria-describedby="passwordHelp" />
							<tstsc:LabelEx ID="ConfirmPasswordLabel"
								runat="server"
								AssociatedControlID="ConfirmPassword"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,ConfirmPassword %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ControlToValidate="ConfirmPassword"
								CssClass="failureNotification"
								Display="Dynamic"
								ErrorMessage="<%$Resources:Messages,Register_ConfirmPasswordRequired %>"
								ID="ConfirmPasswordRequired"
								runat="server"
								ToolTip="<%$Resources:Messages,Register_ConfirmPasswordRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
							<asp:CompareValidator
								ID="PasswordCompare"
								runat="server"
								ControlToCompare="Password"
								ControlToValidate="ConfirmPassword"
								CssClass="failureNotification"
								Display="Dynamic"
								ErrorMessage="<%$Resources:Messages,Register_PasswordsMustMatch %>"
								ValidationGroup="RegisterUserValidationGroup">*

							</asp:CompareValidator>
						</div>
						<aside
							id="passwordHelp"
							runat="server"
							class="db fs-90 mt2 mb5 tl px3">
							<%=String.Format(Resources.Main.Register_PasswordMinLength, Membership.MinRequiredPasswordLength)%>
							<%=PasswordNoNamesLegend%>
						</aside>
						<!-- Security Question -->
						<div class="mb5 mt4 mx3 relative" runat="server" id="pass3">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="Question"
								runat="server"
								MaxLength="255" />
							<tstsc:LabelEx
								ID="QuestionLabel"
								runat="server"
								AssociatedControlID="Question"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,PasswordQuestion %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ID="RequiredFieldValidator1"
								runat="server"
								ControlToValidate="Question"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_PasswordQuestionRequired %>"
								ToolTip="<%$Resources:Messages,Register_PasswordQuestionRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
						</div>
						<!-- Security Answer -->
						<div class="mb5 mt4 mx3 relative" runat="server" id="pass4">
							<tstsc:UnityTextBoxEx
								CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
								ClientIDMode="static"
								ID="Answer"
								runat="server"
								MaxLength="255" />
							<tstsc:LabelEx
								ID="AnswerLabel"
								runat="server"
								AssociatedControlID="Answer"
								CssClass="label-slideup tl"
								Text="<%$Resources:Fields,PasswordAnswer %>"
								Required="true" />
							<asp:RequiredFieldValidator
								ID="RequiredFieldValidator2"
								runat="server"
								ControlToValidate="Answer"
								CssClass="failureNotification"
								ErrorMessage="<%$Resources:Messages,Register_PasswordAnswerRequired %>"
								ToolTip="<%$Resources:Messages,Register_PasswordAnswerRequired %>"
								ValidationGroup="RegisterUserValidationGroup" />
						</div>
						<!-- Buttons! -->
						<div class="btn-group">
							<tstsc:ButtonEx
								ID="btnCreateUserButton"
								ClientIDMode="static"
								runat="server"
								SkinID="ButtonPrimary"
								CommandName="MoveNext"
								Text="<%$Resources:Buttons,Submit %>"
								ValidationGroup="RegisterUserValidationGroup" />
							<tstsc:ButtonEx
								ID="btnCancel"
								runat="server"
								CommandName="Cancel"
								Text="<%$Resources:Buttons,Cancel %>"
								CausesValidation="false" />
						</div>
					</div>
					<asp:HiddenField runat="server"
						ID="hdnOAuth1" />
					<asp:HiddenField runat="server"
						ID="hdnOAuth2" />
				</ContentTemplate>
				<CustomNavigationTemplate>
				</CustomNavigationTemplate>
			</asp:CreateUserWizardStep>



			<asp:CompleteWizardStep ID="CompleteWizardStep" runat="server">
				<ContentTemplate>
					<h1>
						<asp:Localize runat="server" Text="<%$Resources:Main,Register_CompleteTitle %>" />
					</h1>
					<p>
						<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Register_CompleteLegend %>" />
					</p>
					<p>
						<tstsc:ButtonEx ID="btnContinue" runat="server" CommandName="Continue" Text="<%$Resources:Buttons,Continue %>"
							ValidationGroup="RegisterUserValidationGroup" />
					</p>
				</ContentTemplate>
			</asp:CompleteWizardStep>
		</WizardSteps>
	</tstsc:CreateUserWizardEx>



	<script type="text/javascript">
		(function () {
            function checkButton() {
                var usernameNoValue = username && !username.value,
                    emailNoValue = email && !email.value,
                    firstNameNoValue = firstName && !firstName.value,
                    lastNameNoValue = lastName && !lastName.value,
                    passwordNoValue = password && !password.value,
                    confirmPasswordNoValue = confirmPassword && !confirmPassword.value,
                    questionNoValue = question && !question.value,
                    answerNoValue = answer && !answer.value;
				btnCreateUser.disabled = usernameNoValue || emailNoValue || firstNameNoValue || lastNameNoValue || passwordNoValue || confirmPasswordNoValue || questionNoValue || answerNoValue;
			}

			function updateField(field) {
				field.setAttribute('value', field.value);
				checkButton();
			}
			window.onload = function () {
				checkButton();
			}

			var username = document.getElementById("UserName");
			var email = document.getElementById("Email");
			var firstName = document.getElementById("FirstName");
			var lastName = document.getElementById("LastName");
			var password = document.getElementById("Password");
			var confirmPassword = document.getElementById("ConfirmPassword");
			var question = document.getElementById("Question");
			var answer = document.getElementById("Answer");

			var btnCreateUser = document.getElementById("btnCreateUserButton");

            // only add event listeners if the elements exist - they are not all rendered when registering a new oauth account
			username && username.addEventListener("input", function () { updateField(this); }, false);
			email && email.addEventListener("input", function () { updateField(this); }, false);
			firstName && firstName.addEventListener("input", function () { updateField(this); }, false);
			lastName && lastName.addEventListener("input", function () { updateField(this); }, false);
			password && password.addEventListener("input", function () { updateField(this); }, false);
			confirmPassword && confirmPassword.addEventListener("input", function () { updateField(this); }, false);
			question && question.addEventListener("input", function () { updateField(this); }, false);
			answer && answer.addEventListener("input", function () { updateField(this); }, false);
		})();
	</script>
</asp:Content>
