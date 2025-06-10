<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="AutomationEngineDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.AutomationEngineDetails" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Label id="lblEngineName" Runat="server" />
                    <small>
				        <asp:Localize ID="Localize1" Text="<%$Resources:Main,Admin_AutomationEngineDetails_Title%>" runat="server" />
                    </small>
                </h2>
				<tstsc:HyperLinkEx ID="lnkAutomationEngineHome" runat="server" NavigateUrl="AutomationEngines.aspx" SkinID="ButtonDefault">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_AutomationEngineDetails_LinkHome %>" />
				</tstsc:HyperLinkEx>
                <p class="my4">
                    <asp:Localize Text="<%$Resources:Main,Admin_AutomationEngineDetails_Intro%>" runat="server" />
                </p>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList"
					Runat="server" id="ValidationSummary1" />
				<div class="Spacer"></div>
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Name %>" ID="txtNameLabel" AssociatedControlID="txtName" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50"  />
		                <asp:RequiredFieldValidator ID="vldName" runat="server" ControlToValidate="txtName" ErrorMessage="<%$Resources:Messages,Admin_AutomationEngineDetails_NameRequired %>" Text="*" />
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" ID="txtDescriptionLabel" AssociatedControlID="txtDescription" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
			            <tstsc:TextBoxEx ID="txtDescription" runat="server" TextMode="MultiLine" Height="80px" />
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Token %>" ID="txtTokenLabel" AssociatedControlID="txtToken" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:TextBoxEx ID="txtToken" runat="server" MaxLength="50"/>
		                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtToken" ErrorMessage="<%$Resources:Messages,Admin_AutomationEngineDetails_TokenRequired %>" Text="*" />
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ActiveYn %>" ID="chkActiveLabel" AssociatedControlID="chkActive" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:CheckBoxYnEx ID="chkActive" runat="server"/>
			        </div>
                </div>
                <div class="row">
                    <div class="col-sm-offset-3 col-lg-offset-2 btn-group pl3 pt4">
						<tstsc:ButtonEx id="btnUpdateAndClose" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True"
							Authorized_Permission="SystemAdmin" />
						<tstsc:ButtonEx id="btnInsertAndClose" Runat="server" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Insert %>" CausesValidation="True"
							Authorized_Permission="SystemAdmin" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
