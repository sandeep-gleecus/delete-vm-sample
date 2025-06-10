<%@ Page Language="c#" CodeBehind="IncidentWorkflowTransition.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentWorkflowTransition" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
 	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1 class="mt0 mb4 fs-h3">
                    <tstsc:HyperLinkEx ID="lnkWorkflowDetails" runat="server" CssClass="btn btn-default"><span class="fas fa-angle-left"></span></tstsc:HyperLinkEx>
                    <asp:Literal ID="artifactName" runat="server" Text="<%$Resources:Main,Admin_EditIncidentWorkflows_Title %>"/>
                </h1>
                <h2>
                    <asp:Label ID="lblTransitionName" runat="server"/>
                    <small>
                        <asp:Literal runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_Title %>" />
                    </small>
                </h2>
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_Intro %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <div class="Spacer"></div>
                <div class="WorkFlowDiagram-wrapper">
                    <table class="WorkflowDiagram">
                        <tr>
                            <td>
                                <div class="Step">
                                    <asp:HyperLink ID="lnkIncomingStep" runat="server"/>
                                </div>
                            </td>
                            <td>
                                <span class="fas fa-arrow-right"></span>
                            </td>
                            <td>
                                <div class="Step">
                                    <asp:Label ID="lblTransitionName2" runat="server" Font-Italic="true"/>
                                </div>
                            </td>
                            <td>
                                <span class="fas fa-arrow-right"></span>
                            </td>
                            <td>
                                <div class="Step">
                                    <asp:HyperLink ID="lnkOutgoingStep" runat="server" />
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
                <br />
                <h3>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_DetailsSection %>" />
                </h3>
                <div class="row data-entry-wide DataEntryForm">
                    <div class="col-md-11">
                        <div class="form-group row no-side-margins">
                            <div class="DataLabel col-sm-4 col-lg-3">
                                <tstsc:LabelEx ID="txtTransitionNameLabel" AssociatedControlID="txtTransitionName" runat="server" Text="<%$Resources:Fields,Name %>" Required="true" AppendColon="true"/>
                            </div>
                            <div class="DataEntry col-sm-8">
                                <tstsc:TextBoxEx ID="txtTransitionName" runat="server" />
                            </div>
                        </div>
                        <div class="form-group mb2 row no-side-margins">
                            <div class="DataLabel col-sm-4 col-lg-3">
                                <tstsc:LabelEx ID="txtNotifySubjectLabel" AssociatedControlID="txtNotifySubject" runat="server" Text="<%$Resources:Fields,Subject %>" Required="false" AppendColon="true"/>
                            </div>
                            <div class="DataEntry data-very-wide col-sm-8">
                                <tstsc:TextBoxEx ID="txtNotifySubject" runat="server"/>
                            </div>
                        </div>
                        <div class="form-group mb5 row no-side-margins">
                            <div class="DataLabel col-sm-8 col-sm-offset-4 col-lg-offset-3">
                                <tstsc:HyperLinkEx runat="server" NavigateUrl="javascript:void(0)" Text="<%$Resources:Main,Admin_WorkflowTransition_DisplayTokens %>" ClientScriptServerControlId="dlgEmailTokens" ClientScriptMethod="display(event)" />
                            </div>
                        </div>
                        <div class="form-group row no-side-margins">
                            <div class="DataLabel col-sm-4 col-lg-3">
                                <tstsc:LabelEx ID="LabelEx1" AssociatedControlID="chkRequireSignature" runat="server" Text="<%$Resources:Main,WorkflowTransition_RequireSignature %>" Required="false" AppendColon="true"/>
                            </div>
                            <div class="DataEntry col-sm-8">
                                <tstsc:CheckBoxYnEx ID="chkRequireSignature" runat="server" />
                            </div>
                        </div>
                        <div class="btn-group mt4">
                            <tstsc:ButtonEx ID="btnUpdate" runat="server" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                            <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                        </div>
                    </div>
                </div>
                <h3>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_Conditions %>" />
                </h3>
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_ConditionsIntro %>" />
                </p>
                <div style="max-width: 650px;">
                    <table class="DataGrid" style="width: 100%;">
                        <tr class="Header">
                            <th class="priority1">
                                <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_ConditionType %>" />
                            </th>
                            <th class="priority1">
                                <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_ConditionData %>" />
                            </th>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <tstsc:LabelEx ID="chkExcecuteByDetectorYnLabel" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_DetectorExecuteLabel %>" AssociatedControlID="chkExecuteByDetectorYn" AppendColon="true"  />
                            </td>
                            <td class="priority1">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkExecuteByDetectorYn" NoValueItem="false"/>
                            </td>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <tstsc:LabelEx ID="chkExecuteByOwnerYnLabel" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_OwnerExecuteLabel %>" AssociatedControlID="chkExecuteByOwnerYn" AppendColon="true"  />
                            </td>
                            <td class="priority1">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkExecuteByOwnerYn" NoValueItem="false" />
                            </td>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_RolesExecuteLabel %>" />:
                            </td>
                            <td class="priority1">
                                <tstsc:GridViewEx Width="100%" ID="grdExecuteRoles" runat="server" ShowFooter="False"
                                    ShowHeader="False" DataMember="ProjectRole"
                                    AutoGenerateColumns="false" BorderStyle="None" GridLines="None">
                                    <Columns>
                                        <tstsc:BoundFieldEx DataField="Name" DataFormatString="{0}:" ItemStyle-BorderStyle="None" ItemStyle-CssClass="priority1" />
                                        <tstsc:TemplateFieldEx ItemStyle-BorderStyle="None" ItemStyle-CssClass="priority1" >
                                            <ItemTemplate>
                                                <tstsc:CheckBoxEx ID="chkExecuteRole" MetaData='<%# ((ProjectRole) Container.DataItem).ProjectRoleId %>'
                                                    runat="server" />
                                            </ItemTemplate>
                                        </tstsc:TemplateFieldEx>
                                    </Columns>
                                </tstsc:GridViewEx>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="mt4 btn-group">
                    <tstsc:ButtonEx ID="btnUpdate2" runat="server" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                    <tstsc:ButtonEx ID="btnCancel2" runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                </div>
                <h3>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_Notifications %>" />
                </h3>
                <p>
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_NotificationsIntro %>" />
                </p>
                <div style="max-width: 650px;">
                    <table class="DataGrid" style="width: 100%">
                        <tr class="Header">
                            <th class="priority1">
                                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_NotificationType %>" />
                            </th>
                            <th class="priority1">
                                <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_NotificationData %>" />
                            </th>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <tstsc:LabelEx ID="chkNotifyDetectorYnLabel" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_DetectorNotifyLabel %>" AssociatedControlID="chkNotifyDetectorYn" AppendColon="true"  />
                            </td>
                            <td class="priority1">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkNotifyDetectorYn" />
                            </td>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <tstsc:LabelEx ID="chkNotifyOwnerYnLabel" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_OwnerNotifyLabel %>" AssociatedControlID="chkNotifyOwnerYn" AppendColon="true"  />
                            </td>
                            <td class="priority1">
                                <tstsc:CheckBoxYnEx runat="server" ID="chkNotifyOwnerYn" />
                            </td>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,Admin_WorkflowTransition_RolesNotifyLabel %>" />:
                            </td>
                            <td class="priority1">
                                <tstsc:GridViewEx Width="100%" ID="grdNotifyRoles" runat="server" ShowFooter="False"
                                    ShowHeader="False" DataMember="ProjectRole"
                                    AutoGenerateColumns="false" BorderStyle="None" GridLines="None">
                                    <Columns>
                                        <tstsc:BoundFieldEx DataField="Name" DataFormatString="{0}:" ItemStyle-BorderStyle="None" ItemStyle-CssClass="priority1"  />
                                        <tstsc:TemplateFieldEx ItemStyle-BorderStyle="None" ItemStyle-CssClass="priority1" >
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
                <div class="mt4 btn-group">
                    <tstsc:ButtonEx ID="btnUpdate3" runat="server" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                    <tstsc:ButtonEx ID="btnCancel3" runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                </div>
            </div>
        </div>
    </div>
    <tstsc:DialogBoxPanel ID="dlgEmailTokens" runat="server" Title="<%$Resources:Main,Admin_WorkflowTransition_EmailTokens %>" Modal="false" Persistent="true" Width="600px">
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
            var txtNotifySubjectId = '<%= txtNotifySubject.ClientID %>';
    	        globalFunctions.insertAtCaret(txtNotifySubjectId, insertToken);
    	}
    </script>
</asp:Content>
