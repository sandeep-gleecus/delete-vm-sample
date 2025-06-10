<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSyncDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.DataSyncDetails" Title="Untitled Page" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1>
                    <asp:Label id="lblPlugInName" Runat="server" />
                    <small>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DataSyncDetails_Title %>" />
                    </small>
                </h1>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkDataSyncHome" runat="server" NavigateUrl="DataSynchronization.aspx">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncDetails_BackToHome %>" />
                    </tstsc:HyperLinkEx>
                </div>
                <p class="my4">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_DataSyncDetails_Intro %>" />
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
			            <tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50" Width="500px" CssClass="text-box" />
		                <asp:RequiredFieldValidator ID="vldName" runat="server" ControlToValidate="txtName" ErrorMessage="<%$Resources:Messages,Admin_DataSyncDetails_NameRequired %>" Text="*" />
                        <p class="help-block">
                            <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_Name %>" />
                        </p>
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Caption %>" ID="txtCaptionLabel" AssociatedControlID="txtCaption" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCaption" runat="server" MaxLength="100" Width="100%" CssClass="text-box" />
                        <p class="help-block">
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_Caption %>" />
                        </p>
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" ID="txtDescriptionLabel" AssociatedControlID="txtDescription" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
			            <tstsc:TextBoxEx ID="txtDescription" runat="server" Width="100%" CssClass="text-box" TextMode="MultiLine" Height="100px" />
                        <p class="help-block">
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_Description %>" />
                        </p>
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ConnectionInfo %>" ID="txtConnectionLabel" AssociatedControlID="txtConnection" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
			            <tstsc:TextBoxEx ID="txtConnection" runat="server" Width="100%" CssClass="text-box" MaxLength="255" />
		                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtConnection" ErrorMessage="<%$Resources:Messages,Admin_DataSyncDetails_ConnectionRequired %>" Text="*" />
                        <p class="help-block">
                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_Connection %>" />
                        </p>
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" ID="chkActiveLabel" AssociatedControlID="chkActive" Text="<%$Resources:Fields,ActiveYn %>" />
			        </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:CheckBoxYnEx ID="chkActive" runat="server"/> 
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Login %>" ID="txtExternalLoginLabel" AssociatedControlID="txtExternalLogin" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:TextBoxEx ID="txtExternalLogin" runat="server" Width="500px" MaxLength="50" CssClass="text-box" />
		                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtExternalLogin" ErrorMessage="<%$Resources:Messages,Admin_DataSyncDetails_LoginRequired %>" Text="*" />
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Password %>" ID="txtExternalPasswordLabel" AssociatedControlID="txtExternalPassword" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:TextBoxEx ID="txtExternalPassword" runat="server" Width="500px" MaxLength="255" CssClass="text-box" />
                        <p class="help-block">
                            <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_LoginPasswordTo %>" />
                            <asp:Literal runat="server" ID="ltrPluginName" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_DataSyncDetails_TimeOffset %>" ID="txtTimeOffsetLabel" AssociatedControlID="txtTimeOffset" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:TextBoxEx ID="txtTimeOffset" runat="server" SkinID="NarrowPlusFormControl" CssClass="text-box" MaxLength="5" />
                        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Hours %>" />
		                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtTimeOffset" ErrorMessage="<%$Resources:Messages,Admin_DataSyncDetails_TimeOffsetRequired %>" Text="*" />
		                <asp:RegularExpressionValidator ID="vldTimeOffsetFormat" runat="server" ControlToValidate="txtTimeOffset" ErrorMessage="<%$Resources:Messages,Admin_DataSyncDetails_TimeOffsetInvalid %>" Text="*" ValidationExpression='<%#GlobalFunctions.VALIDATION_REGEX_INTEGER_NEGATIVE %>' />
                        <p class="help-block">
                            <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Main,DataSyncDetails_Notes_TimeOffset %>" />
                        </p>
			        </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" ID="chkAutoMapUsersLabel" AssociatedControlID="chkAutoMapUsers" Text="<%$Resources:Main,Admin_DataSyncDetails_AutoMapUsers %>" />
			        </div>
                    <div class="DataEntry col-sm-9">
			            <tstsc:CheckBoxYnEx ID="chkAutoMapUsers" runat="server"/> 
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_01 %>" ID="lblCustom01" AssociatedControlID="txtCustom01" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCustom01" runat="server" MaxLength="255" Width="100%" CssClass="text-box" />
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_02 %>" ID="lblCustom02" AssociatedControlID="txtCustom02" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCustom02" runat="server" MaxLength="255" Width="100%" CssClass="text-box" />
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_03 %>" ID="lblCustom03" AssociatedControlID="txtCustom03" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCustom03" runat="server" MaxLength="255" Width="100%" CssClass="text-box" />
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_04 %>" ID="lblCustom04" AssociatedControlID="txtCustom04" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCustom04" runat="server" MaxLength="255" Width="100%" CssClass="text-box" />
		            </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Custom_05 %>" ID="lblCustom05" AssociatedControlID="txtCustom05" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 data-very-wide">
			            <tstsc:TextBoxEx ID="txtCustom05" runat="server" MaxLength="255" Width="100%" CssClass="text-box" />
		            </div>
                </div>
                <div class="row">
                    <div class="col-sm-offset-3 col-lg-offset-2 btn-group pl3 mt4">
						<tstsc:ButtonEx id="btnUpdate" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" 
							    Authorized_Permission="ProjectAdmin" />
						<tstsc:ButtonEx id="btnAdd" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Add %>" CausesValidation="True"
							    Authorized_Permission="ProjectAdmin" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
