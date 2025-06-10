<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true" CodeBehind="GeneralSettings.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.GeneralSettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_Title %>" />
                    <br />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_Legend1 %>" />
		                <%=this.productName%>
			            <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,GeneralSettings_Legend2 %>" />
                    </small>
                </h2>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
		        <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
			        DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="ddlDefaultCultureLabel" AssociatedControlID="ddlDefaultCulture" Text="<%$Resources:Main,GeneralSettings_DefaultCulture %>" runat="server" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:DropDownListEx CssClass="DropDownList" runat="server"
						    ID="ddlDefaultCulture" DataTextField="Value" DataValueField="Key"
						    NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Administration_GeneralSettings_DefaultCultureNoValue %>" />
					    <p class="help-block" style="clear:both">
						    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,GeneralSettings_DefaultCultureNotes%>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="ddlDefaultTimezoneLabel" AssociatedControlID="ddlDefaultTimezone" Text="<%$Resources:Main,GeneralSettings_DefaultTimezone %>" runat="server" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:DropDownListEx runat="server"
						    ID="ddlDefaultTimezone" DataTextField="Value" DataValueField="Key"
						    NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Administration_GeneralSettings_DefaultCultureNoValue %>" />
					    <p class="help-block" style="clear:both">
						    <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_DefaultTimezoneNotes%>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="txtEmailWebServerUrlLabel" AssociatedControlID="txtEmailWebServerUrl" Text="<%$Resources:Main,GeneralSettings_WebServerUrl %>" runat="server" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:TextBoxEx SkinID="FormControl" ID="txtEmailWebServerUrl" runat="server" CssClass="text-box" />
					    <asp:RequiredFieldValidator ControlToValidate="txtEmailWebServerUrl" ErrorMessage="<%$Resources:Messages,GeneralSettings_WebServerUrlRequired %>" Text="*" runat="server" ID="valUrlReq" Display="Dynamic" />
					    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmailWebServerUrl" Text="*" ErrorMessage="<%$Resources:Messages,GeneralSettings_WebServerUrlInvalid %>" ID="valUrlFormat" Display="Dynamic" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_URL %>" />
					    <p class="help-block">
						    <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_WebServerUrl%>" /> <%= Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%> (e.g. http://myserver/<%= Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%>)
					    </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="txtGeneralAttachmentsFolderLabel" AssociatedControlID="txtGeneralAttachmentsFolder" Text="<%$Resources:Main,GeneralSettings_AttachmentsFolder %>" runat="server" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:TextBoxEx SkinID="FormControl" ID="txtGeneralAttachmentsFolder" runat="server" CssClass="text-box" />
					    <asp:RequiredFieldValidator ControlToValidate="txtGeneralAttachmentsFolder" ErrorMessage="<%$Resources:Messages,GeneralSettings_AttachmentsFolderRequired %>"
						    Text="*" runat="server" ID="Requiredfieldvalidator8" />
					    <p class="help-block">
						    <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_AttachmentFolderNotes %>" />
						    C:\Program Files\<%= this.productName %>\Attachments)                               
					    </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx AssociatedControlID="txtCacheFolder" Text="<%$ Resources:Main,Admin_GeneralSettings_CacheFolder %>" runat="server" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:TextBoxEx SkinID="FormControl" ID="txtCacheFolder" runat="server" CssClass="text-box" />
					    <p class="help-block">
						    <asp:Localize ID="ntsCacheFolder" runat="server" Text="<%$Resources:Main,Admin_GeneralSettings_CacheFolderNotes %>" />
					    </p>
                        <div class="Spacer"></div>
					    <tstsc:TextBoxEx ID="txtCacheRetention" runat="server" SkinID="NarrowPlusFormControl" MaxLength="8" />
					    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Minutes %>" />
					    <asp:RequiredFieldValidator ControlToValidate="txtCacheRetention" ErrorMessage="<%$Resources:Messages,GeneralSettings_VersionControlCacheRetentionRequired %>" Text="*" runat="server" ID="RequiredFieldValidator3" Display="Dynamic" />
					    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtCacheRetention" Text="*" ErrorMessage="<%$Resources:Messages,GeneralSettings_VersionControlCacheRetentionInvalid %>" ID="RegularExpressionValidator3" Display="Dynamic" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER_GREATER_THAN_ZERO %>" />
					    <p class="help-block">
						    <asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Main,GeneralSettings_VersionControlCacheRetentionNotesMinutes %>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx AssociatedControlID="txtLoginNotice" Text="<%$ Resources:Main,Admin_GeneralSettings_LoginNotice %>" runat="server" Required="false" AppendColon="true"/>
				    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
					    <tstsc:TextBoxEx TextMode="MultiLine" RichText="false" ID="txtLoginNotice" runat="server" MaxLength="255" Height="90"/>
					    <p class="help-block">
						    <asp:Localize ID="ntcLoginNotice" runat="server" Text="<%$Resources:Main,Admin_GeneralSettings_LoginNoticeNotes %>" />
					    </p>
				    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx AssociatedControlID="txtAdminMessage" Text="<%$ Resources:Main,Admin_GeneralSettings_AdminMessage %>" runat="server" Required="false" AppendColon="true"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
					    <tstsc:TextBoxEx ID="txtAdminMessage" runat="server" MaxLength="255"/>
					    <p class="help-block">
						    <asp:Localize ID="ntcAdminMessage" runat="server" Text="<%$Resources:Main,Admin_GeneralSettings_AdminMessageNotes %>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="chkInstantMessengerLabel" AssociatedControlID="chkInstantMessenger" Text="<%$Resources:Main,GeneralSettings_InstantMessenger %>" runat="server" Required="true" AppendColon="true"/>
				    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:CheckBoxYnEx runat="server" ID="chkInstantMessenger" NoValueItem="false"/>
					    <asp:Localize ID="chkInstantMessengerNotes" runat="server" Text="<%$Resources:Main,GeneralSettings_InstantMessengerNotes %>" />
                        <div class="Spacer"></div>
					    <tstsc:TextBoxEx ID="txtMessageRetention" runat="server" SkinID="NarrowPlusFormControl" MaxLength="8" />
					    <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Main,GeneralSettings_EventDaysToKeepUnits %>" />
					    <asp:RequiredFieldValidator ControlToValidate="txtMessageRetention" ErrorMessage="<%$Resources:Messages,GeneralSettings_InstantMessengerRetentionRequired %>" Text="*" runat="server" ID="RequiredFieldValidator2" Display="Dynamic" />
					    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMessageRetention" Text="*" ErrorMessage="<%$Resources:Messages,GeneralSettings_InstantMessengerRetentionInvalid %>" ID="RegularExpressionValidator2" Display="Dynamic" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
					    <p class="help-block">
						    <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Main,GeneralSettings_InstantMessengerRetention %>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="txtEventDaysToKeepLabel" AssociatedControlID="txtEventDaysToKeep" Text="<%$Resources:Main,GeneralSettings_EventDaysToKeep %>" runat="server" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:TextBoxEx SkinID="NarrowPlusFormControl" ID="txtEventDaysToKeep" runat="server" MaxLength="8" />
					    <asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_EventDaysToKeepUnits %>" />
					    <asp:RequiredFieldValidator ControlToValidate="txtEventDaysToKeep" ErrorMessage="<%$Resources:Messages,GeneralSettings_EventDaysToKeepRequired %>" Text="*" runat="server" ID="RequiredFieldValidator1" Display="Dynamic" />
					    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEventDaysToKeep" Text="*" ErrorMessage="<%$Resources:Messages,GeneralSettings_EventDaysToKeepInvalid %>" ID="RegularExpressionValidator1" Display="Dynamic" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_INTEGER %>" />
					    <p class="help-block">
						    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,GeneralSettings_EventDaysToKeepNotes %>" />
					    </p>
                    </div>
                </div>
                <div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="chkEnableFreeTextIndexingLabel" AssociatedControlID="chkEnableFreeTextIndexing" Text="<%$Resources:Main,GeneralSettings_FreeTextIndexingEnabled %>" runat="server" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:CheckBoxYnEx runat="server" ID="chkEnableFreeTextIndexing" NoValueItem="false" />
					    <p class="help-block">
					        <asp:Localize ID="chkEnableFreeTextIndexingNotes" runat="server" Text="<%$Resources:Main,GeneralSettings_FreeTextIndexingEnabledNotes %>" />
					    </p>
                    </div>
                </div>

                <asp:PlaceHolder runat="server" ID="plcCloudHosted">
                    <div class="form-group row mb5">
                        <div class="DataLabel col-sm-3 col-lg-2">
					        <tstsc:LabelEx ID="chkUseTaraVaultForSourceCodeLabel" AssociatedControlID="chkUseTaraVaultForSourceCode" Text="<%$Resources:Main,GeneralSettings_UseTaraVaultForSourceCode %>" runat="server" Required="true" AppendColon="true"/>
                        </div>
                        <div class="DataEntry col-sm-9">
					        <tstsc:CheckBoxYnEx runat="server" ID="chkUseTaraVaultForSourceCode" NoValueItem="false" />
					        <p class="help-block">
					            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,GeneralSettings_UseTaraVaultForSourceCodeNotes %>" />
					        </p>
                        </div>
                    </div>
                </asp:PlaceHolder>

				<div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
					    <tstsc:LabelEx ID="chkDisableRollupCalculationsLabel" AssociatedControlID="chkDisableRollupCalculations" Text="<%$Resources:Main,GeneralSettings_DisableRollupCalculations %>" runat="server" Required="true" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9">
					    <tstsc:CheckBoxYnEx runat="server" ID="chkDisableRollupCalculations" NoValueItem="false" />
					    <p class="help-block">
					        <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,GeneralSettings_DisableRollupCalculationsNotes %>" />
					    </p>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-offset-3 col-lg-offset-2 btn-group">
				        <tstsc:ButtonEx ID="btnGeneralUpdate" SkinId="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" />
				        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#<%=chkDisableRollupCalculations.ClientID%>').on('switchChange.bootstrapSwitch', function (evt, state) {
				var message = '<%=Inflectra.SpiraTest.Web.GlobalFunctions.JSEncode(Resources.Main.GeneralSettings_DisableRollupCalculationsNotes) %>';
				globalFunctions.globalAlert(message, "warning", true);
			});
		});
    </script>
</asp:Content>
