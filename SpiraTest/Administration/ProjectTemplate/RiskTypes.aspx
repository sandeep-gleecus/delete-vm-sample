<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="RiskTypes.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskTypes" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize Text="<%$Resources:Main,RiskTypes_Title %>" runat="server" />
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
                    <asp:Localize runat="server" Text="<%$Resources:Main,RiskTypes_Intro %>" />
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
                    <tstsc:GridViewEx ID="grdEditRiskTypes" runat="server" DataMember="RiskType"
                        AutoGenerateColumns="False" ShowFooter="False" ShowSubHeader="False" CssClass="DataGrid"
                        Width="100%">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                                <ItemTemplate>
                                    <tstsc:LabelEx runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((RiskType) Container.DataItem).RiskTypeId) %>'
                                        ID="Label5" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:TextBoxEx MetaData='<%# ((RiskType) Container.DataItem).RiskTypeId %>'
                                        SkinID="WideControl" runat="server" Text='<%# ((RiskType) Container.DataItem).Name %>'
                                        MaxLength="128" ID="txtRiskTypeName" />
                                    <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" ControlToValidate="txtRiskTypeName"
                                        ErrorMessage="<%$Resources:Messages,RiskTypes_NameRequired %>" Text="*" runat="server" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Workflow %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                                <ItemTemplate>
                                    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlWorkflow" DataTextField="Name"
                                        DataValueField="RiskWorkflowId" NoValueItem="false" SelectedValue='<%# ((RiskType) Container.DataItem).RiskWorkflowId %>'
                                        Width="150px" DataSource="<%#this.workflows%>" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx ID="radDefault" runat="server" MetaData='<%# ((RiskType) Container.DataItem).RiskTypeId %>'
                                        Checked='<%#((RiskType) Container.DataItem).IsDefault %>'
                                        GroupName="RiskTypeDefaultGroup" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                                <ItemTemplate>
                                    <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" NoValueItem="false" Checked='<%# (((RiskType) Container.DataItem).IsActive) ? true : false%>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                    <div class="btn-group mt4">
                        <tstsc:ButtonEx ID="btnRiskTypesUpdate" SKinID="ButtonPrimary" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Save %>" />
                        <tstsc:ButtonEx ID="btnRiskTypesAdd" runat="server" CausesValidation="True"
                            Text="<%$Resources:Buttons,Add %>" />
                    </div>
                </div>
                <div class="Spacer"></div>
            </div>
        </div>
    </div>
</asp:Content>
