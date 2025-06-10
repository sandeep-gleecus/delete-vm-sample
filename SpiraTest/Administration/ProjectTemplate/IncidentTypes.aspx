<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="IncidentTypes.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentTypes" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize Text="<%$Resources:Main,IncidentTypes_Title %>" runat="server" />
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
                    <asp:Localize runat="server" Text="<%$Resources:Main,IncidentTypes_Intro %>" />
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
                    <tstsc:GridViewEx ID="grdEditIncidentTypes" runat="server" DataMember="IncidentType"
                        AutoGenerateColumns="False" ShowFooter="False" ShowSubHeader="False" CssClass="DataGrid"
                        Width="100%">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                                <ItemTemplate>
                                    <tstsc:LabelEx runat="server" Text='<%# GlobalFunctions.ARTIFACT_PREFIX_INCIDENT_TYPE + String.Format (GlobalFunctions.FORMAT_ID, ((IncidentType) Container.DataItem).IncidentTypeId) %>'
                                        ID="Label5" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:TextBoxEx MetaData='<%# ((IncidentType) Container.DataItem).IncidentTypeId %>'
                                        CssClass="text-box" runat="server" Text='<%# ((IncidentType) Container.DataItem).Name %>'
                                        MaxLength="20" ID="txtIncidentTypeName" />
                                    <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" ControlToValidate="txtIncidentTypeName"
                                        ErrorMessage="<%$Resources:Messages,IncidentTypes_NameRequired %>" Text="*" runat="server" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Workflow %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                                <ItemTemplate>
                                    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlWorkflow" DataTextField="Name"
                                        DataValueField="WorkflowId" NoValueItem="false" SelectedValue='<%# ((IncidentType) Container.DataItem).WorkflowId %>'
                                        Width="150px" DataSource="<%#this.workflows%>" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Issue %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                                <ItemTemplate>
                                    <tstsc:CheckBoxEx ID="chkIssue" runat="server" MetaData='<%# ((IncidentType) Container.DataItem).IncidentTypeId %>'
                                        Checked='<%# ((IncidentType) Container.DataItem).IsIssue %>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx ID="radDefault" runat="server" MetaData='<%# ((IncidentType) Container.DataItem).IncidentTypeId %>'
                                        Checked='<%#((IncidentType) Container.DataItem).IsDefault %>'
                                        GroupName="IncidentTypeDefaultGroup" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                                <ItemTemplate>
                                    <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" NoValueItem="false" Checked='<%# (((IncidentType) Container.DataItem).IsActive) ? true : false%>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                    <div class="btn-group mt4">
                        <tstsc:ButtonEx ID="btnIncidentTypesUpdate" SKinID="ButtonPrimary" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Save %>" />
                        <tstsc:ButtonEx ID="btnIncidentTypesAdd" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Add %>" />
                    </div>
                </div>
                <div class="Spacer"></div>
            </div>
        </div>
    </div>
</asp:Content>
