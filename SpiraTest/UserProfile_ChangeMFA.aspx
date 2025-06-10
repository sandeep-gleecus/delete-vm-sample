<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="UserProfile_ChangeMFA.aspx.cs" Inherits="Inflectra.SpiraTest.Web.UserProfile_ChangeMFA" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
	<div class="w960 mvw-100 px4 pb5 pt5 pt4-sm mx-auto">

           <h1>
                <asp:PlaceHolder ID="plcAddMFA" runat="server" Visible="false">
                    <asp:Localize runat="server" Text="<%$Resources:Main,ChangeMfa_TitleAdd %>" />
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="plcChangeMFA" runat="server" Visible="false">
                    <asp:Localize runat="server" Text="<%$Resources:Main,ChangeMfa_TitleChange %>" />
                </asp:PlaceHolder>
            </h1>
            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox"/>
	        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="validationSummary" />
            
            <asp:PlaceHolder runat="server" ID="plcAddMFAText" Visible="false">
                <p class="lead">
                    <tstsc:ProfileView ID="profileView" runat="server" Font-Bold="true" />,
                    <asp:Localize runat="server" Text="<%$Resources:Main,ChangeMfa_IntroAdd %>" />
                </p>
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="plcChangeMFAText" Visible="false">
                <p class="lead">
                    <tstsc:ProfileView ID="profileView1" runat="server" Font-Bold="true" />,
                    <asp:Localize runat="server" Text="<%$Resources:Main,ChangeMfa_IntroChange %>" />                    
                </p>
            </asp:PlaceHolder>
            
            <div>
                <br />
                <div id="qrcode" style="width: 240px; border: 20px solid white;"></div>
            </div>

            <div class="mt5 ml4" style="width: 200px;">
                <tstsc:TextBoxEx ID="txtOTP" runat="server" MaxLength="6" placeholder="123456" SkinID="OneTimePassword" inputmode="numeric" pattern="[0-9]*" autocomplete="one-time-code" />
				<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOTP" ErrorMessage="<%$Resources:Messages,ChangeMfa_Otp_Required %>" ID="txtOTP_required" CssClass="validationSymbol">!</asp:RequiredFieldValidator>
				<asp:RegularExpressionValidator runat="server" ControlToValidate="txtOTP" ErrorMessage="<%$Resources:Messages,ChangeMfa_Otp_Invalid %>" ID="txtOTP_regex" CssClass="validationSymbol" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_MFA_OTP %>"></asp:RegularExpressionValidator>
            </div>
            
            <div class="mt5 ml4 btn-group">
                <tstsc:ButtonEx runat="server" Text="<%$Resources:Buttons,Submit %>" ID="btnSubmit" SkinId="ButtonPrimary" CausesValidation="true" />
                <a class="btn btn-default" runat="server" href="~/UserProfile.aspx">
                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                </a>
                <tstsc:ButtonEx runat="server" Text="<%$Resources:Main,ChangeMfa_Remove %>" ID="btnRemove" CssClass="btn btn-default"
                    CausesValidation="false" Visible="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,ChangeMfa_RemoveConfirm %>" />
            </div>

	</div>

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Scripts>
            <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.qrcode.js" />  
        </Scripts>
    </tstsc:ScriptManagerProxyEx>  

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">

        var barcodeUrl = '<%=Inflectra.SpiraTest.Web.GlobalFunctions.JSEncode(BarcodeUrl)%>';

        $(function () {
            var qrcode = new QRCode("qrcode", {
                text: barcodeUrl,
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.H
            });
            $("#Code").focus();
        });

    </script>
</asp:Content>
