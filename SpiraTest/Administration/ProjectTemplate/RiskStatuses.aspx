<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="RiskStatuses.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskStatuses" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,RiskStatuses_Title %>" runat="server" />
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
                <tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" />
                <p>
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,RiskStatuses_Intro %>" />
                </p>
                
                <div class="TabControlHeader">
                    <div class="display-inline-block">
                        <strong><asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>"/>:</strong>
                    </div>
                    <tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
                    </tstsc:DropDownListEx>
                </div>

                <tstsc:GridViewEx ID="grdEditRiskStatuses" runat="server"
                    DataMember="RiskStatus" AutoGenerateColumns="False" ShowFooter="False" ShowSubHeader="False"
                    CssClass="DataGrid" Width="100%">
                    <HeaderStyle CssClass="Header" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" >
                            <ItemStyle />
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((RiskStatus) Container.DataItem).RiskStatusId) %>'
                                    ID="Label6" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((RiskStatus) Container.DataItem).RiskStatusId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((RiskStatus) Container.DataItem).Name %>'
                                    Width="95%" MaxLength="128" ID="txtRiskStatusName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator2" ControlToValidate="txtRiskStatusName"
                                    ErrorMessage="<%$Resources:Messages,RiskStatuses_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Open %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemTemplate>
                                <tstsc:CheckBoxEx ID="chkOpenRiskStatus" runat="server" MetaData='<%# ((RiskStatus) Container.DataItem) ["RiskStatusId"] %>'
                                    Checked='<%# ((RiskStatus) Container.DataItem).IsOpen %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemTemplate>
                                <tstsc:RadioButtonEx ID="radRiskStatusDefault" runat="server" MetaData='<%# ((RiskStatus) Container.DataItem) ["RiskStatusId"] %>'
                                    Checked='<%# ((RiskStatus) Container.DataItem).IsDefault %>'
                                    GroupName="RiskTypeDefaultGroup" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>

                       <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Position %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((RiskStatus) Container.DataItem).RiskStatusId %>'
                                    SkinID="NarrowControl" runat="server" Text='<%# ((RiskStatus) Container.DataItem).Position.ToString() %>'
                                    Width="90%" MaxLength="20" ID="txtPosition" />
                                <asp:RequiredFieldValidator ControlToValidate="txtPosition"
                                    ErrorMessage="<%$Resources:Messages,Admin_Statuses_PositionRequired %>" Text="*" runat="server" />
                                <asp:RegularExpressionValidator ValidationExpression="<%$ GlobalFunctions:VALIDATION_REGEX_INTEGER%>"
                                    ControlToValidate="txtPosition" ErrorMessage="<%$Resources:Messages,Admin_Statuses_PositionMustBeNumeric %>"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>

                        <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActiveFlagYn" NoValueItem="false" Checked='<%# (((RiskStatus) Container.DataItem).IsActive) ? true : false %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnRiskStatusUpdate" SkinID="ButtonPrimary" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Save %>" />
                    <tstsc:ButtonEx ID="btnRiskStatusAdd" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Add %>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
