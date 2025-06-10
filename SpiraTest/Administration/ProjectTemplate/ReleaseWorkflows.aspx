<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="ReleaseWorkflows.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.ReleaseWorkflows" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,Admin_EditReleaseWorkflows_Title %>"
                        runat="server" />
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
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ReleaseTypes_Intro %>" />
                </p>
                <tstsc:GridViewEx ID="grdReleaseTypes" runat="server" ShowSubHeader="False" SkinID="DataGrid"
                    Width="100%" DataKeyNames="ReleaseTypeId">
                    <Columns>
                        <tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderColumnSpan="2" HeaderText="<%$Resources:Fields,Name %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1 tc">
                            <ItemTemplate>
                                <tstsc:ImageEx runat="server" AlternateText="<%$Resources:Fields,Release %>" ImageUrl="Images/artifact-Release.svg" CssClass="w4 h4" Visible="<%#((ReleaseType) Container.DataItem).ReleaseTypeId != (int)Release.ReleaseTypeEnum.Iteration && ((ReleaseType) Container.DataItem).ReleaseTypeId != (int)Release.ReleaseTypeEnum.Phase %>" />
                                <tstsc:ImageEx ID="ImageEx1" runat="server" AlternateText="<%$Resources:Fields,Iteration %>" ImageUrl="Images/artifact-Iteration.svg" CssClass="w4 h4" Visible="<%#((ReleaseType) Container.DataItem).ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || ((ReleaseType) Container.DataItem).ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase%>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <asp:Literal runat="server" Text='<%#: ((ReleaseType) Container.DataItem).Name %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Workflow %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlWorkflow" DataTextField="Name"
                                    DataValueField="ReleaseWorkflowId" NoValueItem="false" Width="400px" DataSource="<%#workflows %>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
                            <ItemTemplate>
                                <tstsc:LabelEx runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((ReleaseType) Container.DataItem).ReleaseTypeId) %>'
                                    ID="Label5" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <br />
                <p>
                    <asp:Localize ID="Localize3" runat="server" Text="<%$ Resources:Main,Admin_ReleaseWorkflows_Intro %>" />
                </p>
                <tstsc:GridViewEx ID="grdEditWorkflows" runat="server" ShowFooter="true" ShowSubHeader="False"
                    SkinID="DataGrid" Width="100%" DataKeyNames="ReleaseWorkflowId">
                    <HeaderStyle CssClass="Header" />
                    <FooterStyle CssClass="AddValueFooter" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_name %>" FooterColumnSpan="5" FooterStyle-HorizontalAlign="left" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx runat="server" Text='<%# ((ReleaseWorkflow) Container.DataItem).Name %>'
                                    Width="95%" MaxLength="50" ID="txtName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator6" ControlToValidate="txtName"
                                    ErrorMessage="<%$Resources:Messages,Admin_Workflows_Default_NameRequired %>"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                            <FooterTemplate>
                                <tstsc:LinkButtonEx ID="btnAddWorkflow" runat="server" CommandName="WorkflowAdd" Text="<%$Resources:Main,Admin_Workflows_AddWorkflow %>" SkinID="ButtonLinkAdd" />
                            </FooterTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_isdefault %>" FooterColumnSpan="-1" HeaderStyle-CssClass="Centered priority2" ItemStyle-CssClass="Centered priority2" >
                            <ItemStyle Width="50px" />
                            <ItemTemplate>
                                <tstsc:RadioButtonEx ID="radWorkflowDefault" runat="server" Checked='<%# ((ReleaseWorkflow) Container.DataItem).IsDefault %>'
                                    GroupName="WorkflowDefaultGroup" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_isactive %>" FooterColumnSpan="-1" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemStyle Width="80px" />
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActiveFlag" NoValueItem="false" Checked='<%# (((ReleaseWorkflow) Container.DataItem).IsActive) ? true: false %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_id %>" FooterColumnSpan="-1">
                            <ItemTemplate>
                                <asp:Literal runat="server" Text='<%# String.Format(Inflectra.SpiraTest.Web.GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW + Inflectra.SpiraTest.Web.GlobalFunctions.FORMAT_ID, ((ReleaseWorkflow) Container.DataItem).ReleaseWorkflowId) %>'
                                    ID="Label9" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,general_operations %>" FooterColumnSpan="-1" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <div class="btn-group">
                                    <tstsc:HyperLinkEx ID="btnWorkflowSteps" runat="server" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "ReleaseWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + ((ReleaseWorkflow) Container.DataItem).ReleaseWorkflowId %>'
                                        ConfirmationMessage="<%$Resources:Messages,Admin_Workflows_Default_EditActiveWarning %>" SkinID="ButtonPrimary"
                                        Confirmation='<%# ((ReleaseWorkflow) Container.DataItem).IsActive %>' >
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Steps %>" />
                                    </tstsc:HyperLinkEx>
                                    <tstsc:LinkButtonEx ID="btnWorkflowCopy" runat="server" CommandName="WorkflowCopy"
                                        CommandArgument='<%# ((ReleaseWorkflow) Container.DataItem).ReleaseWorkflowId%>'>
                                        <span class="far fa-clone"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>"/>
                                    </tstsc:LinkButtonEx>
                                    <tstsc:LinkButtonEx ID="btnWorkflowDelete" runat="server" CommandName="WorkflowDelete"
                                        CommandArgument='<%# ((ReleaseWorkflow) Container.DataItem).ReleaseWorkflowId%>'
                                        Confirmation="true" CausesValidation="false" ConfirmationMessage="<%$Resources:Messages,Admin_Workflows_Default_DeleteConfirm %>">
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
