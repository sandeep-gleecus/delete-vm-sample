<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="IncidentWorkflows.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentWorkflows" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Literal runat="server" ID="lblTitle" Text="<%$Resources:Main,Admin_EditIncidentWorkflows_Title %>" />
                    <small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
                            <asp:Literal ID="ltrTemplateName" runat="server" />
				        </tstsc:HyperLinkEx>
                    </small>
                </h2>
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-lg-9">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary ID="vldSummary" runat="server" />
                <p>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$ Resources:Main,Admin_Workflows_Intro %>" />
                </p>
                <tstsc:GridViewEx ID="grdEditWorkflows" runat="server" AutoGenerateColumns="False"
                    ShowFooter="true" ShowSubHeader="false" CssClass="DataGrid" Width="100%">
                    <HeaderStyle CssClass="Header" />
                    <FooterStyle CssClass="AddValueFooter" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_name %>" FooterColumnSpan="6" FooterStyle-HorizontalAlign="left" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId %>' CssClass="text-box" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).Name %>' Width="90%" MaxLength="50" ID="txtName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator6" ControlToValidate="txtName" ErrorMessage="<%$Resources:Messages,Admin_Workflows_Default_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                            <FooterTemplate>
                                <tstsc:LinkButtonEx ID="btnAddWorkflow" runat="server" CommandName="WorkflowAdd" Text="<%$Resources:Main,Admin_Workflows_AddWorkflow %>" SkinID="ButtonLinkAdd" />
                            </FooterTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_isdefault %>" HeaderStyle-CssClass="Centered priority2" ItemStyle-CssClass="Centered priority2" FooterColumnSpan="-1" >
                            <ItemStyle Width="50px" />
                            <ItemTemplate>
                                <tstsc:RadioButtonEx ID="radWorkflowDefault" runat="server" MetaData='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId %>' Checked='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).IsDefault %>' GroupName="WorkflowDefaultGroup" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_notify %>" HeaderStyle-CssClass="Centered priority3" ItemStyle-CssClass="Centered priority3" FooterColumnSpan="-1">
                            <ItemStyle Width="50px" />
                            <ItemTemplate>
                                <tstsc:CheckBoxEx runat="server" ID="chkNotify" Checked="<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).IsNotify %>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_isactive %>" FooterColumnSpan="-1" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" >
                            <ItemStyle Width="80px" />
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActiveFlag" NoValueItem="false" Checked='<%# (((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).IsActive) ? true : false%>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_id %>" FooterColumnSpan="-1" >
                            <ItemTemplate>
                                <asp:Literal runat="server" Text='<%# String.Format(Inflectra.SpiraTest.Web.GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW + Inflectra.SpiraTest.Web.GlobalFunctions.FORMAT_ID, ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId) %>' ID="Label9" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_operations %>" FooterColumnSpan="-1" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" >
                            <ItemTemplate>
                                <div class="btn-group">
                                    <tstsc:LinkButtonEx 
                                        CausesValidation="false" 
                                        CommandName='WorkflowSteps' 
                                        CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId%>' 
                                        ID="btnWorkflowSteps" 
                                        runat="server" 
                                        SkinID="ButtonPrimary">
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Steps %>" />
                                    </tstsc:LinkButtonEx>
                                    <tstsc:LinkButtonEx ID="btnWorkflowCopy" runat="server" CommandName="WorkflowCopy" CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId%>' >
                                        <span class="far fa-clone"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>"/>
                                    </tstsc:LinkButtonEx>
                                    <tstsc:LinkButtonEx ID="btnWorkflowDelete" runat="server" CommandName="WorkflowDelete" CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.Workflow) Container.DataItem).WorkflowId%>' Confirmation="true" CausesValidation="false" ConfirmationMessage="<%$Resources:Messages,Admin_Workflows_Default_DeleteConfirm %>">
                                        <span class="fas fa-trash-alt"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>"/>
                                    </tstsc:LinkButtonEx>
                                </div>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="Spacer"></div>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnSave" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                    <tstsc:ButtonEx ID="btnCancel" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Cancel %>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
