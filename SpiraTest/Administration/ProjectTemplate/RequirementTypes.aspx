<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="RequirementTypes.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RequirementTypes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize Text="<%$Resources:Main,RequirementTypes_Title %>" runat="server" />
                    <small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
                            <tstsc:LabelEx ID="lblTemplateName" runat="server" />
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
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,RequirementTypes_Intro %>" />
                </p>
                <tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" />

                <div class="TabControlHeader">
                    <div class="display-inline-block">
                        <strong><asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>"/>:</strong>
                    </div>
                    <tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
                    </tstsc:DropDownListEx>
                </div>

                <div class="table-responsive">
                    <tstsc:GridViewEx ID="grdEditRequirementTypes" runat="server" DataMember="RequirementType"
                        AutoGenerateColumns="False" ShowFooter="False" ShowSubHeader="False" CssClass="DataGrid"
                        Width="100%">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                                <ItemTemplate>
                                    <tstsc:LabelEx runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((RequirementType) Container.DataItem).RequirementTypeId) %>'
                                        ID="Label5" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:TextBoxEx MetaData='<%# ((RequirementType) Container.DataItem).RequirementTypeId %>'
                                        CssClass="text-box" runat="server" Text='<%# ((RequirementType) Container.DataItem).Name %>'
                                        MaxLength="20" ID="txtRequirementTypeName" />
                                    <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" ControlToValidate="txtRequirementTypeName"
                                        ErrorMessage="<%$Resources:Messages,RequirementTypes_NameRequired %>" Text="*" runat="server" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Workflow %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                                <ItemTemplate>
                                    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlWorkflow" DataTextField="Name"
                                        DataValueField="RequirementWorkflowId" NoValueItem="false" SelectedValue='<%# ((RequirementType) Container.DataItem).RequirementWorkflowId %>'
                                        Width="150px" DataSource="<%#this.workflows%>" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,HasSteps %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                                <ItemTemplate>
                                    <tstsc:CheckBoxYnEx runat="server" ID="chkSteps" NoValueItem="false" Checked='<%# (((RequirementType) Container.DataItem).IsSteps)%>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx ID="radDefault" runat="server" MetaData='<%# ((RequirementType) Container.DataItem).RequirementTypeId %>'
                                        Checked='<%#((RequirementType) Container.DataItem).IsDefault %>'
                                        GroupName="RequirementTypeDefaultGroup" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                                <ItemTemplate>
                                    <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" NoValueItem="false" Checked='<%# ((RequirementType) Container.DataItem).IsActive%>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                    <div class="btn-group mt4">
                        <tstsc:ButtonEx ID="btnRequirementTypesUpdate" SKinID="ButtonPrimary" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Save %>" />
                        <tstsc:ButtonEx ID="btnRequirementTypesAdd" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Add %>" />
                    </div>
                </div>
                <div class="Spacer"></div>
            </div>
        </div>
    </div>
</asp:Content>
