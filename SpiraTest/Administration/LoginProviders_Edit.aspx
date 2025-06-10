<%@ Page
	Language="c#"
	CodeBehind="LoginProviders_Edit.aspx.cs"
	AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.Administration.LoginProviders_Edit"
	MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

	<fieldset class="w-100 px4 mx0 my5 fieldset-gray u-wrapper width_md">
		<legend>
			<asp:Literal runat="server" ID="ltlProviderName" />
		</legend>
		<p>
			<asp:Literal runat="server" ID="ltlDesc" />
		</p>
		<div class="btn-group priority1" role="group">
			<tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="~/Administration/LoginProviders.aspx">
				<span class="fas fa-arrow-left"></span>
				<asp:Localize runat="server" Text="<%$ Resources:Main,Admin_LoginProviderDetails_BackToList %>" />
			</tstsc:HyperLinkEx>
		</div>

		<div class="u-box_3 px5 px4-md px0-sm">
			<ul class="u-box_list pl0">
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx
						runat="server"
						Text="<%$ Resources:Fields,ClientIdKey %>"
						Required="true"
						AssociatedControlID="txtClientId"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ID="txtClientId"
						TextMode="SingleLine"
						MaxLength="255" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx
						runat="server"
						Text="<%$ Resources:Fields,ClientSecretKey %>"
						Required="true"
						AssociatedControlID="txtSecret"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ID="txtSecret"
						TextMode="SingleLine"
						MaxLength="255" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx runat="server"
						Text="<%$ Resources:Main,Global_Active %>"
						AssociatedControlID="chkActiveYn"
						AppendColon="true" />
					<tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx runat="server"
						Text="<%$ Resources:Fields,AuthorizationUrl %>"
						Required="true"
						AssociatedControlID="txtUrlAuth"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ID="txtUrlAuth"
						TextMode="SingleLine"
						MaxLength="256" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx
						runat="server"
						Text="<%$ Resources:Fields,TokenUrl %>"
						Required="true"
						AssociatedControlID="txtUrlTok"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ID="txtUrlTok"
						TextMode="SingleLine"
						MaxLength="256" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx
						runat="server"
						Text="<%$ Resources:Fields,ProfileUrl %>"
						Required="true"
						AssociatedControlID="txtUrlProf"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ID="txtUrlProf"
						TextMode="SingleLine"
						MaxLength="256" />
				</li>
				<li class="ma0 mb4 pa0">
					<tstsc:LabelEx
						runat="server"
						Text="<%$ Resources:Fields,ReturnUrl %>"
						AssociatedControlID="txtUrlProf"
						AppendColon="true" />
					<tstsc:UnityTextBoxEx
						CssClass="u-input u-input-minimal"
						runat="server"
						ReadOnly="true"
						ID="txtUrlReturn"
						TextMode="SingleLine"
						MaxLength="256" />
				</li>
				<li class="ma0 mb4 pa0">
					<div class="btn-group ml4 mt4 ml_u-box-label">
						<tstsc:ButtonEx runat="server"
							ID="btnUpdate"
							SkinID="ButtonPrimary"
							AlternateText="<%$ Resources:Buttons,Save %>"
							UseSubmitBehavior="true"
							Text="<%$ Resources:Buttons,Save %>" />
						<tstsc:ButtonEx runat="server"
							ID="btnCancel"
							AlternateText="<%$ Resources:Buttons,Cancel %>"
							Text="<%$ Resources:Buttons,Cancel %>" />
					</div>
				</li>
			</ul>
		</div>
	</fieldset>
	<asp:HiddenField runat="server" ID="numUsrs" />
</asp:Content>
