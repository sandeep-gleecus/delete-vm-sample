<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="VersionControlProviderDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.VersionControlProviderDetails" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
				    <asp:Label id="lblProviderName" Runat="server" />
                    <small>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_Title %>" />
                    </small>
                </h2>
				<tstsc:HyperLinkEx ID="lnkVersionControlHome" runat="server" NavigateUrl="VersionControl.aspx" SkinID="ButtonDefault" >
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_BackToHome %>" />
				</tstsc:HyperLinkEx>
				<p class="my3">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_Intro %>" />
				</p>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList"
					Runat="server" id="ValidationSummary1" />
			</div>
        </div>
        <div class="row DataEntryForm data-entry-wide my4" id="divSettings" runat="server">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Name %>" ID="txtNameLabel" AssociatedControlID="txtName" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
						<tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50" />
						<asp:RequiredFieldValidator ID="vldName" runat="server" ControlToValidate="txtName" ErrorMessage="<%$Resources:Messages,Admin_VersionControlProvider_NameRequired %>" Text="*" />
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" ID="txtDescriptionLabel" AssociatedControlID="txtDescription" Required="false" AppendColon="true" />                        
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
						<tstsc:TextBoxEx ID="txtDescription" runat="server" TextMode="MultiLine" Height="50px" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx ID="chkActiveLabel" runat="server" Text="<%$Resources:Fields,ActiveYn %>" AssociatedControlID="chkActive" />
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:CheckBoxYnEx ID="chkActive" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-lg-9 col-sm-11">
					    <h3>
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_DefaultSettings %>" />
					    </h3>
					    <p class="my3">
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_VersionControlProvider_Intro2 %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ConnectionInfo %>" ID="txtConnectionLabel" AssociatedControlID="txtConnection" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtConnection" runat="server" MaxLength="255" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtConnection" ErrorMessage="<%$Resources:Messages,Admin_VersionControlProvider_ConnectionInfoRequired %>" Text="*" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Login %>" ID="txtLoginLabel" AssociatedControlID="txtLogin" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtLogin" runat="server" MaxLength="255" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtLogin" ErrorMessage="<%$Resources:Messages,Admin_VersionControlProvider_LoginRequired %>" Text="*" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Password %>" ID="txtPasswordLabel" AssociatedControlID="txtPassword" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtPassword" runat="server" MaxLength="255" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtPassword" ErrorMessage="<%$Resources:Messages,Admin_VersionControlProvider_PasswordRequired %>" Text="*" />
						<div class="v-top display-inline-block">
                            <tstsc:ButtonEx ID="btnTest" runat="server" Text="<%$Resources:Buttons,Test %>" CausesValidation="true" />&nbsp;
						    <asp:PlaceHolder ID="plcViewEvents" runat="server" Visible="false">
							    <tstsc:HyperLinkEx ID="lnkViewEvents" runat="server" Text="<%$Resources:Main,VersionControlProviderDetails_ViewEvents %>" SkinID="ButtonPrimary" />
						    </asp:PlaceHolder>
                        </div>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Domain %>" ID="txtDomainLabel" AssociatedControlID="txtDomain" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtDomain" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_01%>" ID="lblCustom01" AssociatedControlID="txtCustom01" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtCustom01" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_02%>" ID="lblCustom02" AssociatedControlID="txtCustom02" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtCustom02" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_03%>" ID="lblCustom03" AssociatedControlID="txtCustom03" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtCustom03" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_04%>" ID="lblCustom04" AssociatedControlID="txtCustom04" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtCustom04" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_05%>" ID="lblCustom05" AssociatedControlID="txtCustom05" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtCustom05" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-offset-3 col-lg-offset-2 pl3 pt4 btn-group">
						<tstsc:ButtonEx id="btnUpdate" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" Authorized_Permission="SystemAdmin" />
						<tstsc:ButtonEx id="btnInsert" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Insert%>" CausesValidation="True" Authorized_Permission="SystemAdmin" />
						<tstsc:ButtonEx id="btnCancel" Runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" Authorized_Permission="SystemAdmin" />
					</div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
