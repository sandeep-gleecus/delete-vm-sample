<%@ Page 
    Language="c#" 
    ValidateRequest="false" 
    CodeBehind="PasswordExpired.aspx.cs" 
    AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.PasswordExpired" 
    MasterPageFile="~/MasterPages/Login.Master" 
    %>

<asp:Content 
    ContentPlaceHolderID="cplHead" 
    runat="server" 
    ID="Content1"
    >
</asp:Content>

<asp:Content 
    ContentPlaceHolderID="cplMainContent" 
    runat="server" 
    ID="Content2"
    >
    <tstsc:ChangePasswordEx 
        ID="ChangeUserPassword" 
        runat="server" 
        EnableViewState="false"
        DisplayUserName="true"
        >
        <ChangePasswordTemplate>
            <tstsc:MessageBox 
                ID="FailureText" 
                SkinID="MessageBox" 
                runat="server" 
                />
            <asp:ValidationSummary 
                ID="ChangePasswordValidationSummary" 
                runat="server"
                    />

            <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm tc" runat="server">
                <h1 class="tc fw-b mt5 mt4-xs mb0 blue-strong">
                    <asp:Localize 
                        ID="Localize2" 
                        runat="server" 
                        Text="<%$Resources:Main,PasswordExpired_Title %>" 
                        />
                </h1>
                <p class="mt0 mb5 ml3 tl fs-125">
                    <asp:Localize 
                        ID="Localize3" 
                        runat="server" 
                        Text="<%$Resources:Main,PasswordExpired_Intro %>" 
                        />
                </p>

               <div class="mb5 mt4 mx3 relative">
                    <tstsc:UnityTextBoxEx 
                        CssClass="w-100 u-input u-input-minimal py3 px1 fs-110" 
                        ClientIDMode="static"  
                        ID="UserName" 
                        runat="server" 
                        TextMode="SingleLine" 
                        MaxLength="50" 
                        SkinId="FormControl" 
                        />
                    <tstsc:LabelEx 
                        ID="UserNameLabel" 
                        runat="server" 
                        CssClass="label-slideup tl" 
                        AssociatedControlID="UserName" 
                        Text="<%$Resources:Fields,UserName %>" 
                        Required="true" 
                        />
                    <asp:RequiredFieldValidator 
                        ID="UserNameRequired" 
                        runat="server" 
                        ControlToValidate="UserName"
                        CssClass="failureNotification" 
                        ErrorMessage="<%$Resources:Messages,ChangePassword_UserNameRequired %>" 
                        ToolTip="<%$Resources:Messages,ChangePassword_UserNameRequired %>"
                        />
                </div>

                <div class="mb5 mt4 mx3 relative">
                    <tstsc:UnityTextBoxEx 
                        CssClass="w-100 u-input u-input-minimal py3 px1 fs-110" 
                        ClientIDMode="static"  
                        ID="CurrentPassword" 
                        runat="server" 
                        TextMode="Password" 
                        MaxLength="50" 
                        SkinId="FormControl" 
                        />
                    <tstsc:LabelEx 
                        ID="CurrentPasswordLabel" 
                        runat="server" 
                        CssClass="label-slideup tl" 
                        AssociatedControlID="CurrentPassword" 
                        Text="<%$Resources:Fields,OldPassword %>" 
                        Required="true" 
                        />
                    <asp:RequiredFieldValidator 
                        ID="CurrentPasswordRequired" 
                        runat="server" 
                        ControlToValidate="CurrentPassword"
                        CssClass="failureNotification" 
                        ErrorMessage="<%$Resources:Messages,ChangePassword_CurrentPasswordRequired %>" 
                        ToolTip="<%$Resources:Messages,ChangePassword_CurrentPasswordRequired %>"
                        />
                </div>
                <div class="mb5 mt4 mx3 relative">
                    <tstsc:UnityTextBoxEx 
                        CssClass="w-100 u-input u-input-minimal py3 px1 fs-110" 
                        ClientIDMode="static"  
                        ID="NewPassword" 
                        runat="server" 
                        TextMode="Password" 
                        MaxLength="50" 
                        SkinId="FormControl" 
                        />
                    <tstsc:LabelEx 
                        ID="NewPasswordLabel" 
                        runat="server" 
                        AssociatedControlID="NewPassword" 
                        CssClass="label-slideup tl" 
                        Text="<%$Resources:Fields,NewPassword %>" 
                        Required="true" 
                        />
                    <asp:RequiredFieldValidator 
                        ID="NewPasswordRequired" 
                        runat="server" 
                        ControlToValidate="NewPassword"
                        CssClass="failureNotification" 
                        ErrorMessage="<%$Resources:Messages,ChangePassword_NewPasswordRequired %>" 
                        ToolTip="<%$Resources:Messages,ChangePassword_NewPasswordRequired %>"
                        />
                </div>
                <div class="mt4 mx3 relative">
                    <tstsc:UnityTextBoxEx 
                        CssClass="w-100 u-input u-input-minimal py3 px1 fs-110" 
                        ClientIDMode="static"  
                        ID="ConfirmNewPassword" 
                        runat="server" 
                        TextMode="Password" 
                        MaxLength="50" 
                        SkinId="FormControl" 
                        />
                    <tstsc:LabelEx 
                        ID="ConfirmNewPasswordLabel" 
                        runat="server" 
                        AssociatedControlID="ConfirmNewPassword" 
                        CssClass="label-slideup tl" 
                        Text="<%$Resources:Fields,ConfirmPassword %>" 
                        Required="true" 
                        />
                    <asp:RequiredFieldValidator 
                        ID="ConfirmNewPasswordRequired" 
                        runat="server" 
                        ControlToValidate="ConfirmNewPassword"
                        CssClass="failureNotification" 
                        ErrorMessage="<%$Resources:Messages,ChangePassword_ConfirmNewPasswordRequired %>" 
                        ToolTip="<%$Resources:Messages,ChangePassword_ConfirmNewPasswordRequired %>"
                        />
                </div>
                <asp:CompareValidator 
                    ID="NewPasswordCompare" 
                    runat="server" 
                    ControlToValidate="ConfirmNewPassword" 
                    ControlToCompare="NewPassword"
                    CssClass="failureNotification tl my3 fw4" 
                    ErrorMessage="<%$Resources:Messages,ChangePassword_ConfirmNewPasswordMustMatch %>" 
                    ToolTip="<%$Resources:Messages,ChangePassword_ConfirmNewPasswordMustMatch %>"
                    />
                <aside class="db mt2 mb5 tl px3">
                    <%=string.Format(Resources.Main.Register_PasswordMinLengthAndNonAlphaChars, Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters)%>
                    <%=PasswordNoNamesLegend%>
                </aside>
                
                <div class="btn-wrapper-wide px5 px0-xs">
                    <tstsc:ButtonEx 
                        ID="ChangePasswordPushButton" 
                        ClientIDMode="static"
                        runat="server" 
                        SkinID="ButtonPrimary" 
                        CommandName="ChangePassword"
                        Text="<%$Resources:Buttons,ChangePassword %>" 
                        UseSubmitBehavior="true" 
                        CausesValidation="true" 
                        />
                </div>
            </div>
        </ChangePasswordTemplate>
    </tstsc:ChangePasswordEx>

    <script type="text/javascript">
        (function () {
            function checkButton() {
                btn.disabled = !current.value || !newPassword.value || !confirm.value;
            }
            window.onload = function () {
                checkButton()
            }
            
            var current = document.getElementById("CurrentPassword");
            var newPassword = document.getElementById("NewPassword");
            var confirm = document.getElementById("ConfirmNewPassword");
            var btn = document.getElementById("ChangePasswordPushButton");

            current.addEventListener("input", function () {
                this.setAttribute('value', this.value);
                checkButton();
            }, false);
            newPassword.addEventListener("input", function () {
                this.setAttribute('value', this.value);
                checkButton();
            }, false);
            confirm.addEventListener("input", function () {
                this.setAttribute('value', this.value);
                checkButton();
            }, false);
        })();
    </script>
</asp:Content>
