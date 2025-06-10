<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true" CodeBehind="LicenseDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.LicenseDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
		<div class="row">
			<div class="col-lg-9">
				<h2>
					<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,LicenseDetails_Title %>" />
				</h2>
				<tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />
				<asp:ValidationSummary ID="ValidationSummary" runat="server" />
			</div>
		</div>
		<div class="row DataEntryForm my4">
			<div class="col-lg-9 col-sm-11">
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<asp:Literal ID="ddlLicenseProductLabel" runat="server" Text="<%$Resources:Fields,ProductType %>" />:
					</div>
					<div class="DataEntry col-sm-9">
						<asp:Literal runat="server" ID="ddlLicenseProductText" />
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_LicenseVersionNumber %>" Font-Bold="true" AssociatedControlID="lblLicenseVersion" ID="lblLicenseVersionLabel" />:
					</div>
					<div class="DataEntry col-sm-9">
						<tstsc:LabelEx ID="lblLicenseVersion" runat="server" />
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_LicenseType %>" Font-Bold="true" AssociatedControlID="lblLicenseType" ID="lblLicenseTypeLabel" />:
					</div>
					<div class="DataEntry col-sm-9">
						<tstsc:LabelEx ID="lblLicenseType" runat="server" />
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_NumConcurrentUsers %>" Font-Bold="true" AssociatedControlID="lblConcurrentUsers" ID="lblConcurrentUsersLabel" />:
					</div>
					<div class="DataEntry col-sm-9">
						<tstsc:LabelEx ID="lblConcurrentUsers" runat="server" />
						<asp:Localize runat="server" Text="<%$ Resources:Fields,general_isactive %>" />
						&nbsp;
                        <small>(<tstsc:HyperLinkEx
							Authorized_ArtifactType="None" Authorized_Permission="SystemAdmin" ID="lnkActiveSessions"
							NavigateUrl='<%# "ActiveSessions.aspx" %>' runat="server" Text="<%$ Resources:Main,Admin_LicenseDetails_ViewActiveSessions %>" />)</small>
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_Expiration %>" Font-Bold="true" AssociatedControlID="lblLicenseExpiration" ID="lblLicenseExpirationLabel" />:
					</div>
					<div class="DataEntry col-sm-9">
						<tstsc:LabelEx ID="lblLicenseExpiration" runat="server" />
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx ID="txtLicenseOrganizationLabel" runat="server" AssociatedControlID="txtLicenseOrganization" Text="<%$Resources:Main,LicenseDetail_Organization %>" Required="true" />:
					</div>
					<div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtLicenseOrganization" runat="server" TextMode="SingleLine" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtLicenseOrganization"
							ErrorMessage="<%$Resources:Messages,LicenseDetails_OrganizationRequired %>" Text="*" runat="server" />
					</div>
				</div>
				<div class="form-group row">
					<div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx ID="txtLicenseKeyLabel" runat="server" AssociatedControlID="txtLicenseKey" Text="<%$Resources:Main,LicenseDetail_LicenseKey %>" Required="true" />:
					</div>
					<div class="DataEntry col-sm-9 ws-normal">
						<tstsc:UnityTextBoxEx ID="txtLicenseKey" runat="server" TextMode="SingleLine" CssClass="w-100 text-box" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtLicenseKey"
							ErrorMessage="<%$Resources:Messages,LicenseDetails_LicenseKeyRequired %>" Text="*" runat="server" />
						<br />
						<br />
						<strong>
							<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,LicenseDetail_Legend1 %>" /></strong>
						<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,LicenseDetail_Legend2 %>" />
						<tstsc:HyperLinkEx ID="lnkCustomerArea" runat="server" SkinID="CustomerArea" Text="<%$Resources:Main,LicenseDetail_CustomerHomePage %>" />.
                        <div class="Spacer">
						</div>
						<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,LicenseDetail_Legend3 %>" />
						<tstsc:HyperLinkEx ID="lnkProductPurchase" runat="server" SkinID="ProductPurchase" Text="<%$Resources:Main,LicenseDetail_PurchaseOne %>" />
						<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,LicenseDetail_Legend4 %>" />.
					</div>
				</div>
				<div class="row">
					<div class="col-sm-offset-3 col-lg-offset-2 pl3 mt4">
						<tstsc:ButtonEx ID="btnLicenseUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>"
							CausesValidation="True" />
					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>
