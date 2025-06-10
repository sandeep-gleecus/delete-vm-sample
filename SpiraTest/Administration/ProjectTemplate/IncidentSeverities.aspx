<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="IncidentSeverities.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentSeverities" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,IncidentSeverities_Title %>" runat="server" />
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
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,IncidentSeverities_Intro %>" />
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

                <tstsc:GridViewEx ID="grdEditIncidentSeverities" runat="server"
                    DataMember="IncidentSeverity" AutoGenerateColumns="False" ShowFooter="False"
                    ShowSubHeader="False" SkinID="DataGrid" Width="100%">
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# GlobalFunctions.ARTIFACT_PREFIX_INCIDENT_SEVERITY + String.Format (GlobalFunctions.FORMAT_ID, ((IncidentSeverity) Container.DataItem).SeverityId) %>'
                                    ID="Label8" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((IncidentSeverity) Container.DataItem).SeverityId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((IncidentSeverity) Container.DataItem).Name %>'
                                    Width="90%" MaxLength="20" ID="txtIncidentSeverityName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator5" ControlToValidate="txtIncidentSeverityName"
                                    ErrorMessage="<%$Resources:Messages,IncidentSeverities_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Color %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                            <ItemTemplate>
                                <table class="inner-table priority1"">
                                    <tr>
                                        <td>
                                            <tstsc:ColorPicker MaxLength="6" MetaData='<%# ((IncidentSeverity) Container.DataItem).SeverityId %>'
                                                CssClass="text-box" Width="80px" runat="server" Text='<%# ((IncidentSeverity) Container.DataItem).Color %>'
                                                ID="colIncidentSeverityColor" />
                                        </td>
                                        <td>
                                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator4" ControlToValidate="colIncidentSeverityColor"
                                                ErrorMessage="<%$Resources:Messages,IncidentSeverities_ColorRequired %>" Text="*"
                                                runat="server" />
                                            <asp:RegularExpressionValidator ID="Regularexpressionvalidator1" ValidationExpression="<%# GlobalFunctions.VALIDATION_REGEX_HTML_COLOR%>"
                                                ControlToValidate="colIncidentSeverityColor" ErrorMessage="<%$Resources:Messages,IncidentSeverities_ColorInvalid %>"
                                                Text="*" runat="server" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="Dropdownlistex5" NoValueItem="false"
                                    Checked='<%# (((IncidentSeverity) Container.DataItem).IsActive) ? true : false %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnIncidentSeverityUpdate" SkinID="ButtonPrimary" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
                    <tstsc:ButtonEx ID="btnIncidentSeverityAdd" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Add%>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
