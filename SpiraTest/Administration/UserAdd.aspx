<%@ Page Language="c#" CodeBehind="UserAdd.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.UserAdd"
    MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="System.Data" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblUserName" runat="server" Text="<%$Resources:Main,UserDetails_NewUser %>" />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_AddUser %>" />
                    </small>
                </h2>
                <div class="Spacer"></div>
                <p><asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_AddEditUser_Legend %>" /></p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend><asp:Localize runat="server" Text="<%$Resources:Main,UserDetails_UserInformation %>" /></legend>
                <div class="col-md-6">
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,FirstName %>" Required="true" AssociatedControlID="txtFirstName" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtFirstName" runat="server" TextMode="SingleLine" MaxLength="50" />
                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*" ErrorMessage="<%$Resources:Messages,UserDetails_FirstNameRequired %>" ControlToValidate="txtFirstName" />
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx1" runat="server" Text="<%$Resources:Fields,MiddleInitial %>" Required="false" AssociatedControlID="txtMiddleInitial" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" SkinID="NarrowPlusFormControl" ID="txtMiddleInitial" runat="server" TextMode="SingleLine" MaxLength="1" />
                        </div>
                    </div>
                    <div class="form-group row mb5">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx2" runat="server" Text="<%$Resources:Fields,LastName %>" Required="true" AssociatedControlID="txtLastName" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtLastName" runat="server" TextMode="SingleLine" MaxLength="50" />
                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator3" runat="server" Text="*" ErrorMessage="<%$Resources:Messages,UserDetails_LastNameRequired %>" ControlToValidate="txtLastName"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx3" runat="server" Text="<%$Resources:Fields,UserName %>" Required="true" AssociatedControlID="txtFirstName" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtUserName" runat="server" TextMode="SingleLine" MaxLength="50" autocomplete="off" />
                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator4" runat="server" Text="*" ErrorMessage="<%$Resources:Messages,UserDetails_UserNameRequired %>" ControlToValidate="txtUserName"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator runat="server" Text="*" ErrorMessage="<%$Resources:Messages,UserDetails_UserNameInvalid %>" ControlToValidate="txtUserName" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_USER_NAME%>" ID="Regularexpressionvalidator2" />
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx4" runat="server" Text="<%$Resources:Fields,Department %>" Required="false" AssociatedControlID="txtDepartment" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtDepartment" runat="server" TextMode="SingleLine" MaxLength="50" />
                        </div>
                    </div>
                    <div class="form-group row mb5">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="txtOrganizationLabel" runat="server" Text="<%$Resources:Fields,Organization %>" Required="false" AssociatedControlID="txtOrganization" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtOrganization" runat="server" TextMode="SingleLine" MaxLength="50" />
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx5" runat="server" Text="<%$Resources:Fields,EmailAddress %>" Required="true" AssociatedControlID="txtEmailAddress" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtEmailAddress" runat="server" TextMode="SingleLine" MaxLength="255" />
                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator2" runat="server" Text="*"
                                ErrorMessage="<%$Resources:Messages,UserDetails_EmailAddressRequired %>" ControlToValidate="txtEmailAddress" />
                            <asp:RegularExpressionValidator runat="server" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EMAIL_ADDRESS%>"
                                ControlToValidate="txtEmailAddress" Text="*" ErrorMessage="<%$Resources:Messages,UserDetails_EmailAddressInvalid %>"
                                ID="Regularexpressionvalidator1" />
                        </div>
                    </div>
                </div>
            </fieldset>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend>
                    <asp:Literal runat="server" Text="<%#Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%>" />
                    <asp:Localize runat="server" Text="<%$Resources:Main,Settings %>" />
                </legend>
                <div class="col-md-6">
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx6" runat="server" Text="<%$Resources:Fields,EmailEnabledYn %>" Required="true" AssociatedControlID="chkEmailEnabled" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkEmailEnabled" />
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx7" runat="server" Text="<%$Resources:Fields,SystemAdministrator %>" Required="true" AssociatedControlID="chkAdminYn" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkAdminYn" />
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ReportAdmin %>" Required="true" AssociatedControlID="chkReportAdmin" AppendColon="true" ToolTip="<%$Resources:Main,UserDetails_ReportAdminNotes %>"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkReportAdmin" />
                        </div>
                    </div>
                    <div class="form-group row mb2" id="portfolioFormGroup" runat="server" visible="false">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,PortfolioViewer %>" Required="true" AssociatedControlID="chkPortfolioAdmin" AppendColon="true" ToolTip="<%$Resources:Main,UserDetails_PortfolioViewerNotes %>"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkPortfolioAdmin" />
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="LabelEx8" runat="server" AssociatedControlID="txtRssToken" Text="<%$Resources:Main,UserProfile_EnableRSSFeeds %>" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:CheckBoxYnEx ID="chkRssEnabled" runat="server" />
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="RssTokenLabel" runat="server" AssociatedControlID="txtRssToken" Text="<%$Resources:Main,UserProfile_RssTokenApiKey %>" AppendColon="true" />
                        </div>
                        <div class=" DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtRssToken" runat="server" CssClass="textEntry" MaxLength="255" ReadOnly="true" Style="margin-bottom: 5px"/>
                        </div>
                        <div class="col-sm-8 col-sm-offset-4 col-lg-offset-3">
                            <tstsc:ButtonEx ID="btnGenerateNewToken" runat="server" CausesValidation="false" Text="<%$Resources:Buttons,GenerateNew %>"/>
                        </div>
                    </div>

                </div>
                <div class="col-md-6">
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="lblNewPassword" runat="server" AssociatedControlID="txtNewPassword" Text="<%$Resources:Fields,NewPassword %>" Required="true" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtNewPassword" CssClass="ReadOnlyTextBox" runat="server" TextMode="Password" MaxLength="128" />
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" Text="<%$Resources:Fields,ConfirmPassword %>" Required="true" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtConfirmPassword" CssClass="ReadOnlyTextBox" runat="server" TextMode="Password" MaxLength="128" />
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataEntry col-sm-8 col-sm-offset-4 col-lg-9 col-lg-offset-3">
                            <p class="Notes mb2"><%=String.Format(Resources.Main.Register_PasswordMinLength, Membership.MinRequiredPasswordLength)%></p>
                            <p class="Notes"><%=String.Format(Resources.Main.Register_PasswordMinNonAlphaChars, Membership.MinRequiredNonAlphanumericCharacters)%></p>
                        </div>
                    </div>
                    <div class="form-group row mb2">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="lblPasswordQuestion" runat="server" AssociatedControlID="txtPasswordQuestion" Text="<%$Resources:Fields,PasswordQuestion %>" Required="true" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtPasswordQuestion" CssClass="ReadOnlyTextBox" runat="server" TextMode="SingleLine" MaxLength="255" />
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx ID="lblPasswordAnswer" runat="server" AssociatedControlID="txtConfirmPassword" Text="<%$Resources:Fields,PasswordAnswer %>" Required="true" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtPasswordAnswer" CssClass="ReadOnlyTextBox" runat="server" TextMode="SingleLine" MaxLength="255" />
                        </div>
                    </div>
                </div>
            </fieldset>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectMembership_Title%>" />
                </legend>
                <div class="col-md-8 form-inline">
                    <div class="form-group col-md-6 col-sm-10">
                        <div class="DataLabel">
                            <tstsc:LabelEx ID="lblProjectMultiSelect" runat="server" Text="<%$Resources:Main,Admin_Projects %>" AssociatedControlID="ddlProjectMultiSelect" AppendColon="true"/>
                        </div>
                        <div class="DataEntry">
                            <tstsc:DropDownMultiList SelectionMode="Multiple" runat="server" ID="ddlProjectMultiSelect" DataValueField="ProjectId" DataTextField="Name" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_ChooseProject %>"/>
                        </div>
                    </div>
                    <div class="form-group col-md-6 col-sm-10">
                        <div class="DataLabel">
                            <tstsc:LabelEx ID="lblProjectRole" runat="server" Text="<%$Resources:Fields,ProjectRole %>" AssociatedControlID="ddlProjectRole" AppendColon="true"/>
                        </div>
                        <div class="DataEntry">
                            <tstsc:DropDownListEx runat="server" ID="ddlProjectRole" DataValueField="ProjectRoleId" DataTextField="Name" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_SelectRole %>" />
                        </div>
                    </div>
                </div>
            </fieldset>
        </div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-sm-8 btn-group">
                <tstsc:ButtonEx ID="btnInsert" SkinID="ButtonPrimary" runat="server" AlternateText="Insert" CausesValidation="True" Text="<%$Resources:Buttons,Add%>" />
                <tstsc:ButtonEx ID="btnCancel" runat="server" AlternateText="Cancel" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
            </div>
        </div>
    </div>
    <script language="javascript" type="text/javascript">
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
    </script>
</asp:Content>
