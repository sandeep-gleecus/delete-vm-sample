<%@ Page Language="c#" ValidateRequest="false" CodeBehind="EmailConfiguration.aspx.cs"
    AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.EmailConfiguration"
    MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
    Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
        <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Literal ID="Literal1" runat="server" Text="<% $Resources:Main,Admin_Notification_EMailServerSettings %>" />
                </h2>
                <br />
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:Panel ID="pnlEmailSettings" runat="server">
                    <asp:Label runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_String1 %>" />
                    <%= Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType %>
                    <asp:label runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_String2 %>" />
                    <div class="Spacer"></div>
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_String3 %>" />
                    <br />
                    <br />
                    <div class="row data-entry-wide DataEntryForm">
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_ActiveYn %>" ID="chkActiveYnLabel" AssociatedControlID="chkActiveYn" Required="true" AppendColon="false" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx ID="chkActiveYn" runat="server" NoValueItem="false"/>
                                <div style="clear: both">
                                    <small>
                                        <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_ActiveNotes1 %>" />
                                        <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%>
                                        <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_ActiveNotes2 %>" />
                                    </small>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_FromEmailAddress %>" ID="txtEmailFromAddressLabel" AssociatedControlID="txtEmailFromAddress" Required="true" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailFromAddress" runat="server" />
                                <asp:RequiredFieldValidator ControlToValidate="txtEmailFromAddress" ErrorMessage="<%$Resources:Messages,Admin_EmailConfiguration_FromAddressRequired %>"
                                    Text="*" runat="server" ID="valEmailReq" Display="Dynamic" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmailFromAddress"
                                    Text="*" ErrorMessage="<%$Resources:Messages,Admin_EmailConfiguration_AddressInvalid %>" ID="valEmailFormat"
                                    Display="Dynamic" />
                                <br />
                                <small>
                                    <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_FromAddressNotes %>" />                                
                                </small>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_ReplyToEmailAddress %>" ID="txtEmailReplyAddressLabel" AssociatedControlID="txtEmailReplyAddress" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailReplyAddress" runat="server" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmailReplyAddress"
                                    Text="*" ErrorMessage="<%$Resources:Messages,Admin_EmailConfiguration_AddressInvalid %>" ID="valEmailReplyFormat"
                                    Display="Dynamic" />
                                <br />
                                <small>
                                    <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_ReplyToAddressNotes %>" />                                
                                </small>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_SendHtml %>" ID="chkSendHTMLLabel" AssociatedControlID="chkSendHTML" Required="true" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkSendHTML" NoValueItem="false" />
                                <div style="clear: both">
                                    <small><asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_SendInHtmlNotes %>" /></small>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_AllowUsersControl %>" ID="chkAllowUserLabel" AssociatedControlID="chkAllowUser" Required="true" AppendColon="false" />                               
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkAllowUser" NoValueItem="false" />
                                <br />
                                <div style="clear: both">
                                    <small><asp:Localize ID="Localize11" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_AllowUserControlNotes %>" /></small></div>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_EmailConfiguration_SendEmailFromUsers %>" Required="true" AssociatedControlID="chkFromUser" AppendColon="false" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkFromUser" NoValueItem="false" />
                                <div style="clear: both">
                                    <small>
                                        <asp:Literal runat="server" Text="<%$ Resources:Main,Admin_EmailConfiguration_SendEmailFromUsersNotes %>" />
                                    </small>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$ Resources:Messages,Admin_EmailConfig_Seperator %>" Required="true" ID="chkSendSeperatorLabel" AssociatedControlID="chkSendSeperator" AppendColon="false" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkSendSeperator"
                                    DataTextField="Value" DataValueField="Key" NoValueItem="false" Width="70px" />
                                <div style="clear: both">
                                    <small>
                                        <asp:Literal runat="server" Text="<%$ Resources:Messages,Admin_EmailConfig_Seperator_Small %>" />
                                    </small>
                                </div>
                            </div>
                        </div>
                    </div>
                    <br />
                    <h3>
                        <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_MailServer %>" />
                    </h3>
                    <div class="Spacer"></div>
                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_MailServerIntro %>" />                                
                    <div class="Spacer"></div>
                    <div class="row data-entry-wide DataEntryForm">
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_HostName %>" ID="txtEmailHostNameLabel" AssociatedControlID="txtEmailHostName" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailHostName" runat="server" /><br />
                                <small><asp:Localize ID="Localize12" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_MailHostNotes %>" /></small>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_PortNumber %>" ID="txtEmailPortNumberLabel" AssociatedControlID="txtEmailPortNumber" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailPortNumber" runat="server" SkinID="NarrowPlusFormControl" MaxLength="5" />
                                <asp:RegularExpressionValidator runat="server" Text="*" ErrorMessage="<%$Resources:Messages,Admin_EmailConfiguration_PortNumberInvalid %>"
                                    ControlToValidate="txtEmailPortNumber" ID="valServerPort" Display="Dynamic" />
                                <br />
                                <small><asp:Localize ID="Localize13" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_PortNumberNotes %>" /></small>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_SslConnection %>" ID="chkUseSSLLabel" AssociatedControlID="chkUseSSL" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkUseSSL" NoValueItem="false" />
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_UserName %>" ID="txtEmailUserNameLabel" AssociatedControlID="txtEmailUserName" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailUserName" runat="server" /><br />
                                <small><asp:Localize ID="Localize14" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_UserNameNotes %>" /></small>
                            </div>
                        </div>
                        <div class="form-group row mx0 col-md-11">
                            <div class="DataLabel col-sm-3">
                                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_Password %>" ID="txtEmailPasswordLabel" AssociatedControlID="txtEmailPassword" Required="false" AppendColon="true" />
                            </div>
                            <div class="DataEntry col-sm-9ws-normal">
                                <tstsc:TextBoxEx ID="txtEmailPassword" runat="server" /><br />
                                <small><asp:Localize ID="Localize15" runat="server" Text="<%$Resources:Main,Admin_EmailConfiguration_PasswordNotes %>" /></small>
                            </div>
                        </div>
                    </div>
                    <tstsc:ButtonEx SkinID="ButtonPrimary" ID="btnEmailUpdate" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
