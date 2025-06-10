<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="SecuritySettings.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.SecuritySettings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Main,SecuritySettings_Title %>" />
                </h2>
                <p class="my4">
                    <asp:Localize runat="server" Text="<%$Resources:Main,SecuritySettings_Legend1 %>" />
                    <%=this.productName%>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,SecuritySettings_Legend2 %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
                    DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
                <div class="Spacer"></div>
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="chkAllowUserRegistrationLabel" AssociatedControlID="chkAllowUserRegistration"
                            Text="<%$Resources:Main,SecuritySettings_AllowUserRegistration %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkAllowUserRegistration" NoValueItem="false" /><br style="clear:both" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,SecuritySettings_AllowUserRegistrationNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtMaxInvalidPasswordAttemptsLabel" AssociatedControlID="txtMaxInvalidPasswordAttempts"
                            Text="<%$Resources:Main,SecuritySettings_MaxInvalidPasswordAttempts %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtMaxInvalidPasswordAttempts" runat="server" SkinID="NarrowPlusFormControl"  CssClass="text-box" MaxLength="2" />
                        <asp:RequiredFieldValidator ControlToValidate="txtMaxInvalidPasswordAttempts" ErrorMessage="<%$Resources:Messages,SecuritySettings_MaxInvalidPasswordAttempts_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMaxInvalidPasswordAttempts"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_MaxInvalidPasswordAttempts_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,SecuritySettings_MaxInvalidPasswordAttemptsNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtMinRequiredPasswordLengthLabel" AssociatedControlID="txtMinRequiredPasswordLength"
                            Text="<%$Resources:Main,SecuritySettings_MinRequiredPasswordLength %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtMinRequiredPasswordLength" runat="server" SkinID="NarrowPlusFormControl"  CssClass="text-box" MaxLength="2" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtMinRequiredPasswordLength" ErrorMessage="<%$Resources:Messages,SecuritySettings_MinRequiredPasswordLength_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtMinRequiredPasswordLength"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_MinRequiredPasswordLength_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,SecuritySettings_MinRequiredPasswordLengthNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtMinRequiredNonalphanumericCharactersLabel" AssociatedControlID="txtMinRequiredNonalphanumericCharacters"
                            Text="<%$Resources:Main,SecuritySettings_MinRequiredNonalphanumericCharacters %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtMinRequiredNonalphanumericCharacters" runat="server" SkinID="NarrowPlusFormControl"  CssClass="text-box" MaxLength="2" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtMinRequiredNonalphanumericCharacters" ErrorMessage="<%$Resources:Messages,SecuritySettings_MinRequiredNonalphanumericCharacters_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="txtMinRequiredNonalphanumericCharacters"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_MinRequiredNonalphanumericCharacters_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,SecuritySettings_MinRequiredNonalphanumericCharactersNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtPasswordAttemptWindowLabel" AssociatedControlID="txtPasswordAttemptWindow"
                            Text="<%$Resources:Main,SecuritySettings_PasswordAttemptWindow %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtPasswordAttemptWindow" runat="server" Width="80px" CssClass="text-box" MaxLength="5" />
                        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Minutes %>" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" ControlToValidate="txtPasswordAttemptWindow" ErrorMessage="<%$Resources:Messages,SecuritySettings_PasswordAttemptWindow_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="txtPasswordAttemptWindow"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_PasswordAttemptWindow_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SecuritySettings_PasswordAttemptWindowNotes %>" />
                        </p>
                    </div>
                </div>

                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtPasswordChangeIntervalLabel" AssociatedControlID="txtPasswordChangeInterval"
                            Text="<%$Resources:Main,SecuritySettings_PasswordChangeInterval %>" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtPasswordChangeInterval" runat="server" Width="80px" CssClass="text-box" MaxLength="5" />
                        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Days %>" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="txtPasswordChangeInterval"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_PasswordChangeInterval_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize12" runat="server" Text="<%$Resources:Main,SecuritySettings_PasswordChangeInterval_Notes %>" />
                        </p>
                    </div>
                </div>

                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="chkChangePasswordOnFirstLoginLabel" AssociatedControlID="chkChangePasswordOnFirstLogin"
                            Text="<%$Resources:Main,SecuritySettings_ChangePasswordOnFirstLogin %>" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkChangePasswordOnFirstLogin" NoValueItem="false" /><br style="clear:both" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,SecuritySettings_ChangePasswordOnFirstLogin_Notes %>" />
                        </p>
                    </div>
                </div>

                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="chkPasswordContainNoNamesLabel" AssociatedControlID="chkPasswordContainNoNames"
                            Text="<%$Resources:Main,SecuritySettings_PasswordContainNoNames %>" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkPasswordContainNoNames" NoValueItem="false" /><br style="clear:both" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,SecuritySettings_PasswordContainNoNames_Notes %>" />
                        </p>
                    </div>
                </div>


                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="chkEnableMfaLabel" AssociatedControlID="chkEnableMfa"
                            Text="<%$Resources:Main,SecuritySettings_EnableMfa %>" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkEnableMfa" NoValueItem="false" /><br style="clear:both" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,SecuritySettings_EnableMfa_Notes %>" />
                        </p>
                    </div>
                </div>


                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtAuthenticationExpirationLabel" AssociatedControlID="txtAuthenticationExpiration"
                            Text="<%$Resources:Main,SecuritySettings_AuthenticationExpiration %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtAuthenticationExpiration" runat="server" Width="80px" CssClass="text-box" MaxLength="5" />
                        <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Global_Minutes %>" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" ControlToValidate="txtAuthenticationExpiration" ErrorMessage="<%$Resources:Messages,SecuritySettings_AuthenticationExpiration_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="txtAuthenticationExpiration"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_AuthenticationExpiration_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,SecuritySettings_AuthenticationExpirationNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtAuthenticationExpirationRememberMeLabel" AssociatedControlID="txtAuthenticationExpirationRememberMe"
                            Text="<%$Resources:Main,SecuritySettings_AuthenticationExpirationRememberMe %>" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8">
                        <tstsc:TextBoxEx ID="txtAuthenticationExpirationRememberMe" runat="server" Width="80px" CssClass="text-box" MaxLength="5" />
                        <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Main,Global_Minutes %>" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" ControlToValidate="txtAuthenticationExpirationRememberMe" ErrorMessage="<%$Resources:Messages,SecuritySettings_AuthenticationExpirationRememberMe_Required %>"
                            Text="*" runat="server" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator5" runat="server" ControlToValidate="txtAuthenticationExpirationRememberMe"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_AuthenticationExpirationRememberMe_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Main,SecuritySettings_AuthenticationExpirationRememberMeNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-4 col-lg-3">
                        <tstsc:LabelEx runat="server" ID="txtAllowedDomainsLabel" AssociatedControlID="txtAllowedDomains"
                            Text="<%$Resources:Main,SecuritySettings_AllowedDomains %>" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-8 data-very-wide">
                        <tstsc:TextBoxEx ID="txtAllowedDomains" runat="server" Width="100%" CssClass="text-box" MaxLength="255" />
                        <asp:RegularExpressionValidator ID="txtAllowedDomainsValidator" runat="server" ControlToValidate="txtAllowedDomains"
                            Text="*" ErrorMessage="<%$Resources:Messages,SecuritySettings_AllowedDomainsNotes_Invalid %>" Display="Dynamic"
                            ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_DOMAIN_LIST %>" />
                        <p class="Notes">
                            <asp:Localize ID="Localize11" runat="server" Text="<%$Resources:Main,SecuritySettings_AllowedDomainsNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-offset-4 col-lg-offset-3 btn-group pl3">
                        <tstsc:ButtonEx ID="btnUpdate" runat="server" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
