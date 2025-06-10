<%@ Page Language="c#" CodeBehind="NotificationEventDetails.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationEventDetails" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Label ID="lblProjectName" runat="server" />
                    <small>
                        <asp:Literal ID="Literal8" runat="server" Text="<% $Resources:Main,Admin_Notification_ViewEditNotification %>" />
                    </small>
                </h2>
                <tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" ID="lnkBackToNotificationList">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<% $Resources:Main,Admin_Notification_BackToNotificationList %>" />
                </tstsc:HyperLinkEx>
                <div class="Spacer"></div>
                <br />
                <asp:Localize Text="<% $Resources:Messages,Admin_Notification_ViewEditNotificationEventText %>" runat="server" />
                <br />
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
            </div>
        </div>
        <br />
        <div class="row">
            <div class="col-lg-9">
                <div class="row data-entry-wide DataEntryForm">
                    <div class="form-group row no-side-margins col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<% $Resources:Main,Admin_Notification_EventName %>" AssociatedControlID="txtName" ID="txtNameLabel" Required="true" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtName" runat="server" TextMode="SingleLine" MaxLength="40" />
                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                                ErrorMessage="<%$Resources:Messages,Admin_EmailConfiguration_EventNameRequired %>" ControlToValidate="txtName" />
                        </div>
                    </div>
                    <div class="form-group row no-side-margins col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactType %>" AssociatedControlID="ddlArtifactType" ID="ddlArtifactTypeLabel" Required="true" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9ws-normal">
                            <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlArtifactType"
                                DataTextField="Name" DataValueField="ArtifactTypeId" NoValueItem="false" AutoPostBack="true" />
                            <div style="clear: both">
                                <small>
                                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_Notification_ArtifactTypeNotes %>" />                                
                                </small>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row no-side-margins col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<% $Resources:Main,Admin_Notification_OnCreation %>" AssociatedControlID="chkOnCreation" ID="chkOnCreationLabel" Required="true" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9ws-normal">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkOnCreation" NoValueItem="false" />
                            <div style="clear: both">
                                <small><asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_Notification_OnCreationNotes %>" /></small>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row no-side-margins mb2 col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_Notification_SubjectLine %>" ID="txtSubjectLabel" AssociatedControlID="txtSubject" Required="true" AppendColon="true" />
                        </div>
                        <div class="DataEntry data-very-wide col-sm-9ws-normal">
                            <tstsc:TextBoxEx CssClass="text-box" ID="txtSubject" runat="server" TextMode="SingleLine" MaxLength="200" />
                            <asp:RequiredFieldValidator ID="valSujReq" runat="server" Text="*" ErrorMessage="<%$Resources:Messages,Admin_Notification_SubjectLineRequired %>"
                                ControlToValidate="txtSubject" />
                            <p>
                                <small><asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_Notification_SubjectLineNotes %>" /></small>
                            </p>
                        </div>
                    </div>
                    <div class="form-group mb5 row no-side-margins">
                        <div class="DataLabel col-sm-8 col-sm-offset-4 col-lg-offset-3">
                            <tstsc:HyperLinkEx runat="server" NavigateUrl="javascript:void(0)" Text="<%$Resources:Main,Admin_Notification_DisplayTokens %>" ClientScriptServerControlId="dlgEmailTokens" ClientScriptMethod="display(event)" />
                        </div>
                    </div>
                    <br />
                    <div class="form-group row no-side-margins col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Configuration %>" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <div class="Spacer"></div>
                            <tstsc:TabControl ID="tclUserDetails" CssClass="TabControl2" TabWidth="120" TabHeight="25"
                                TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                                DisabledTabCssClass="TabDisabled" runat="server">
                                <TabPages>
                                    <tstsc:TabPage ID="tabArtifactFields" Caption="<%$Resources:Main,Admin_Notification_ArtifactFields %>" TabPageControlId="pnlArtifactFields" />
                                    <tstsc:TabPage ID="tabUsersRoles" Caption="<%$Resources:Main,Admin_Notification_ArtifactUsers %>" TabPageControlId="pnlUsersRoles" />
                                </TabPages>
                            </tstsc:TabControl>
                            <asp:Panel ID="pnlArtifactFields" runat="server" Width="100%">
                                <p>
                                    <asp:Literal ID="Literal2" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactFieldsIntro %>" />                    
                                </p>
                                <div style="max-width: 400px">
                                    <tstsc:GridViewEx runat="server" ID="grdAvailFields" AutoGenerateColumns="false"
                                        CssClass="DataGrid" Width="100%">
                                        <HeaderStyle CssClass="Header" />
                                        <Columns>
                                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,Field %>" DataField="Caption" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1"/>
                                            <tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Admin_Notification_NotifyOnChange %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <tstsc:CheckBoxEx runat="server" ID="chkSelected" Checked="false" MetaData='<%# ((ArtifactField) Container.DataItem).ArtifactFieldId %>' />
                                                </ItemTemplate>
                                            </tstsc:TemplateFieldEx>
                                        </Columns>
                                    </tstsc:GridViewEx>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlUsersRoles" runat="server" Width="100%">
                                <p>
                                    <asp:Literal ID="Literal3" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactUsersIntro %>" />                    
                                </p>
                                <div style="max-width: 400px">
                                    <table class="DataGrid priority1" style="width: 100%">
                                        <tr class="Header">
                                            <th class="col-xs-8">
                                                <asp:Literal ID="Literal1" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactUsersRoles %>" />
                                            </th>
                                            <th class="col-xs-4 text-center">
                                                <asp:Literal ID="Literal4" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactNotificationEnabled %>" />
                                            </th>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Literal ID="Literal5" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactOpener %>" />
                                            </td>
                                            <td class="text-center">
                                                <tstsc:CheckBoxEx runat="server" ID="chkOpener" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Literal ID="Literal6" runat="server" Text="<% $Resources:Main,Admin_Notification_ArtifactOwner %>" />
                                            </td>
                                            <td class="text-center">
                                                <tstsc:CheckBoxEx runat="server" ID="chkOwner" />
                                            </td>
                                        </tr>
                                        <tr style="background-color: transparent;"><td>&nbsp;</td><td></td></tr>
                                        <tr class="Header">
                                            <th colspan="2" class="priority1">
                                                <asp:Literal ID="Literal7" runat="server" Text="<% $Resources:Main,Admin_Notification_ProjectRoles %>" />
                                            </th>
                                        </tr>
                                        <tr style="background-color: transparent;">
                                            <td colspan="2">
                                                <tstsc:GridViewEx Width="100%" ID="grdAvailRoles" runat="server" ShowFooter="false"
                                                    ShowHeader="false" DataMember="ProjectRole" AutoGenerateColumns="False" BorderStyle="None"
                                                    GridLines="None" CssClass="inner-table priority1">
                                                    <Columns>
                                                        <tstsc:BoundFieldEx DataField="Name" DataFormatString="{0}:" ItemStyle-BorderStyle="None" ItemStyle-CssClass="col-xs-8"/>
                                                        <tstsc:TemplateFieldEx ItemStyle-BorderStyle="None" ItemStyle-CssClass="col-xs-4" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <tstsc:CheckBoxEx ID="chkNotifyRole" MetaData='<%# ((ProjectRole) Container.DataItem).ProjectRoleId %>'
                                                                    runat="server" />
                                                            </ItemTemplate>
                                                        </tstsc:TemplateFieldEx>
                                                    </Columns>
                                                </tstsc:GridViewEx>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="form-group row no-side-margins col-md-11">
                        <div class="DataLabel col-sm-3">
                            <tstsc:LabelEx runat="server" Text="<% $Resources:Main,Global_Active %>" ID="chkActiveYnLabel" AssociatedControlID="chkActiveYn" Required="true" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9ws-normal">
                            <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" NoValueItem="false" />
                            <div style="clear: both">
                                <small><asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_Notification_IsActiveNotes %>" /></small>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="btn-group">
                    <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
                    <tstsc:ButtonEx ID="btnInsert" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Add%>" CausesValidation="True" />
                    <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                </div>
            </div>
        </div>
    </div>
    <tstsc:DialogBoxPanel ID="dlgEmailTokens" runat="server" Title="<%$Resources:Main,Admin_Notification_EmailTokens %>" Modal="false" Persistent="true" Width="600px">
		<p><asp:Literal ID="litTokenHeader" runat="server" /></p>
		<tstsc:DataListEx ID="grdFields" runat="server" CssClass="DataGrid priority1" 
			ViewStateMode="Disabled" RepeatColumns="2" RepeatDirection="Horizontal"
			Width="100%" ItemStyle-BorderStyle="solid" ItemStyle-BorderColor="Transparent" ItemStyle-BorderWidth="1px">
			<ItemTemplate>
				<a runat="server" id="aTokenClick" style="cursor: pointer;">${<asp:Label runat="server"
					ID="lblToken" Text='<%# ((ArtifactField)Container.DataItem).Name %>' />}</a>
				</td>
				<td colspan="2">
					<tstsc:LabelEx runat="server" ID="lblTokenDesc" Text='<%# ((ArtifactField)Container.DataItem).Description %>' />
				&nbsp;
			</ItemTemplate>
		</tstsc:DataListEx>    
    </tstsc:DialogBoxPanel>
    <script type="text/javascript">
        function insert_token(insertToken)
        {
    	    var txtSubjectId = '<%= txtSubject.ClientID %>';
    	    globalFunctions.insertAtCaret(txtSubjectId, insertToken);
    	}
	</script>
</asp:Content>
