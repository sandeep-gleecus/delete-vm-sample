<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="IncidentStatuses.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentStatuses" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,IncidentStatuses_Title %>" runat="server" />
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
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,IncidentStatuses_Intro %>" />
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

                <tstsc:GridViewEx ID="grdEditIncidentStatuses" runat="server"
                    DataMember="IncidentStatus" AutoGenerateColumns="False" ShowFooter="False" ShowSubHeader="False"
                    CssClass="DataGrid" Width="100%">
                    <HeaderStyle CssClass="Header" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" >
                            <ItemStyle />
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# GlobalFunctions.ARTIFACT_PREFIX_INCIDENT_STATUS + String.Format (GlobalFunctions.FORMAT_ID, ((IncidentStatus) Container.DataItem).IncidentStatusId) %>'
                                    ID="Label6" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((IncidentStatus) Container.DataItem).IncidentStatusId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((IncidentStatus) Container.DataItem).Name %>'
                                    Width="95%" MaxLength="20" ID="txtIncidentStatusName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator2" ControlToValidate="txtIncidentStatusName"
                                    ErrorMessage="<%$Resources:Messages,IncidentStatuses_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Open %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemTemplate>
                                <tstsc:CheckBoxEx ID="chkOpenIncidentStatus" runat="server" MetaData='<%# ((IncidentStatus) Container.DataItem) ["IncidentStatusId"] %>'
                                    Checked='<%# ((IncidentStatus) Container.DataItem).IsOpenStatus %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemTemplate>
                                <tstsc:RadioButtonEx ID="radIncidentStatusDefault" runat="server" MetaData='<%# ((IncidentStatus) Container.DataItem) ["IncidentStatusId"] %>'
                                    Checked='<%# ((IncidentStatus) Container.DataItem).IsDefault %>'
                                    GroupName="IncidentTypeDefaultGroup" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActiveFlagYn" NoValueItem="false" Checked='<%# (((IncidentStatus) Container.DataItem).IsActive) ? true : false %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnIncidentStatusUpdate" SkinID="ButtonPrimary" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Save %>" />
                    <tstsc:ButtonEx ID="btnIncidentStatusAdd" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Add %>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
