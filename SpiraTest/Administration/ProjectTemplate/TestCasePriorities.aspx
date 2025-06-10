<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="TestCasePriorities.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TestCasePriorities" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,TestCasePriorities_Title %>" runat="server" />
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
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,TestCasePriorities_Intro %>" />
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

                <tstsc:GridViewEx ID="grdEditTestCasePriorities" runat="server"
                    DataMember="TestCasePriority" AutoGenerateColumns="False" ShowFooter="False"
                    ShowSubHeader="False" CssClass="DataGrid" Width="100%">
                    <HeaderStyle CssClass="Header" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((TestCasePriority) Container.DataItem).TestCasePriorityId) %>'
                                    ID="Label7" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((TestCasePriority) Container.DataItem).TestCasePriorityId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((TestCasePriority) Container.DataItem).Name %>'
                                    Width="90%" MaxLength="20" ID="txtTestCasePriorityName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator3" ControlToValidate="txtTestCasePriorityName"
                                    ErrorMessage="<%$Resources:Messages,Admin_Priorities_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Color %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                            <ItemTemplate>
                                <table class="inner-table priority1">
                                    <tr>
                                        <td>
                                            <tstsc:ColorPicker MaxLength="6" MetaData='<%# ((TestCasePriority) Container.DataItem).TestCasePriorityId %>'
                                                CssClass="text-box" Width="80px" runat="server" Text='<%# ((TestCasePriority) Container.DataItem).Color %>'
                                                ID="colTestCasePriorityColor" />
                                        </td>
                                        <td>
                                            <asp:RequiredFieldValidator ID="Requiredfieldvalidator4" ControlToValidate="colTestCasePriorityColor"
                                                ErrorMessage="<%$Resources:Messages,Admin_Priorities_ColorRequired %>" Text="*"
                                                runat="server" />
                                            <asp:RegularExpressionValidator ID="Regularexpressionvalidator1" ValidationExpression="<%# GlobalFunctions.VALIDATION_REGEX_HTML_COLOR%>"
                                                ControlToValidate="colTestCasePriorityColor" ErrorMessage="<%$Resources:Messages,Admin_Priorities_ColorInvalid %>"
                                                Text="*" runat="server" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Score %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((TestCasePriority) Container.DataItem).TestCasePriorityId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((TestCasePriority) Container.DataItem).Score.ToString() %>'
                                    Width="90%" MaxLength="20" ID="txtScore" />
                                <asp:RequiredFieldValidator ControlToValidate="txtScore"
                                    ErrorMessage="<%$Resources:Messages,Admin_Priorities_ScoreRequired %>" Text="*" runat="server" />
                                <asp:RegularExpressionValidator ValidationExpression="<%$ GlobalFunctions:VALIDATION_REGEX_INTEGER%>"
                                    ControlToValidate="txtScore" ErrorMessage="<%$Resources:Messages,Admin_Priorities_ScoreMustBeNumeric %>"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="ddlActive" NoValueItem="false"
                                    Checked='<%# (((TestCasePriority) Container.DataItem).IsActive) ? true : false%>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Save%>" />
                    <tstsc:ButtonEx ID="btnAdd" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Add%>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
