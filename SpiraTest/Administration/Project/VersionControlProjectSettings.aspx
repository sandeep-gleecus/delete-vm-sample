<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="VersionControlProjectSettings.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.VersionControlProjectSettings" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
		<asp:Label ID="lblProviderName" runat="server" />
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_VersionControlProject_Title %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>
    <div class="btn-group priority2">
		<tstsc:HyperLinkEx ID="lnkVersionControlHome" runat="server" SkinID="ButtonDefault" >
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_BackToHome %>" />
		</tstsc:HyperLinkEx>
    </div>
	<p class="my3">
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_VersionControlProject_Intro1 %>" />
        '<asp:Label ID="lblProviderName2" runat="server" Font-Bold="true" />'
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_VersionControlProject_Intro2 %>" />
	</p>

    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <tstsc:MessageBox ID="msgStatus" runat="server" SkinID="MessageBox" />
    <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList"
        runat="server" ID="ValidationSummary1" />


    <ul class="u-box_list mt5 mw720" id="divSettings" runat="server">
        <li class="ma0 pa0 mb3">
            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ProjectName %>" ID="lblProjectName2Label" AssociatedControlID="lblProjectName2" Required="true" AppendColon="true" />
            <tstsc:LabelEx ID="lblProjectName2" runat="server" />
        </li>
        <li class="ma0 pa0 mb3">
            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_VersionControlProject_ActiveForProject %>" ID="chkActiveLabel" AssociatedControlID="chkActive" Required="true" AppendColon="true" />
            <tstsc:CheckBoxYnEx ID="chkActive" runat="server" />
        </li>
        <asp:Panel ID="pnlProjectSettings" runat="server">
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ConnectionInfo %>" ID="txtConnectionLabel" AssociatedControlID="txtConnection" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtConnection" runat="server" MaxLength="255" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Login %>" ID="txtLoginLabel" AssociatedControlID="txtLogin" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtLogin" runat="server" MaxLength="255" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Password %>" ID="txtPasswordLabel" AssociatedControlID="txtPassword" Required="false" AppendColon="true" />
                <span>
                    <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtPassword" runat="server" MaxLength="255" />
                    <div class="v-top display-inline-block">
                        <tstsc:ButtonEx ID="btnTest" runat="server" Text="<%$Resources:Buttons,Test %>" CausesValidation="true" />&nbsp;
                        <asp:PlaceHolder ID="plcViewEvents" runat="server" Visible="false">
                            <tstsc:HyperLinkEx ID="lnkViewEvents" runat="server" Text="<%$Resources:Main,VersionControlProviderDetails_ViewEvents %>" SkinID="ButtonPrimary" />
                        </asp:PlaceHolder>
                    </div>
                </span>
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Domain %>" ID="txtDomainLabel" AssociatedControlID="txtDomain" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtDomain" runat="server" MaxLength="50" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_01%>" ID="lblCustom01" AssociatedControlID="txtCustom01" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtCustom01" runat="server" MaxLength="50" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_02%>" ID="lblCustom02" AssociatedControlID="txtCustom02" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtCustom02" runat="server" MaxLength="50"  />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_03%>" ID="lblCustom03" AssociatedControlID="txtCustom03" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtCustom03" runat="server" MaxLength="50" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_04%>" ID="lblCustom04" AssociatedControlID="txtCustom04" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtCustom04" runat="server" MaxLength="50" />
            </li>
            <li class="ma0 pa0 mb4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_05%>" ID="lblCustom05" AssociatedControlID="txtCustom05" Required="false" AppendColon="true" />
                <tstsc:UnityTextBoxEx CssClass="u-input" DisabledCssClass="u-input disabled" ID="txtCustom05" runat="server" MaxLength="50" />
            </li>
        </asp:Panel>
        <li class="ma0 pa0">
            <div class="btn-group ml_u-box-label">
                <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$ Resources:Buttons,Save %>" CausesValidation="True" Authorized_Permission="ProjectAdmin" />
                <tstsc:ButtonEx ID="btnClearCache" runat="server" Text="<%$ Resources:Buttons,RefreshCache %>" CausesValidation="False" OnClick="btnClearCache_Click" />
				<tstsc:ButtonEx ID="btnDeleteCache" runat="server" Text="<%$ Resources:Buttons,ClearCache %>" CausesValidation="False" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_VersionControlProvider_ClearCacheWarning %>" OnClick="btnDeleteCache_Click" />
                <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$ Resources:Buttons,Cancel %>" CausesValidation="False" />
            </div>
        </li>
    </ul>
</asp:Content>
