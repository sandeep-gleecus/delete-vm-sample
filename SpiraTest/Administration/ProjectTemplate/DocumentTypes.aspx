<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="DocumentTypes.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.DocumentTypes" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,DocumentTypes_Title %>" runat="server" />
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
                <p class="my4">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,DocumentTypes_Intro %>" />
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
                <tstsc:GridViewEx ID="grdDocumentTypes" runat="server" DataMember="DocumentType"
                    AutoGenerateColumns="False" ShowFooter="False" ShowHeader="true" ShowSubHeader="false"
                    SkinID="DataGrid" Width="100%">
                    <HeaderStyle CssClass="Header" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
                            <ItemStyle />
                            <HeaderStyle VerticalAlign="Middle" HorizontalAlign="Left" />
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT_TYPE + String.Format (GlobalFunctions.FORMAT_ID, ((DocumentType) Container.DataItem).DocumentTypeId) %>'
                                    ID="lblDocumentTypeId" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1 col-sm-3">
                            <ItemStyle Wrap="false" />
                            <HeaderStyle />
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((DocumentType) Container.DataItem).DocumentTypeId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((DocumentType) Container.DataItem).Name %>'
                                    Width="95%" MaxLength="50" ID="txtName" />
                                <asp:RequiredFieldValidator ID="vldName" ControlToValidate="txtName" ErrorMessage="You need to enter a name for the document type"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Description %>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                            <ItemStyle Wrap="false"/>
                            <HeaderStyle />
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((DocumentType) Container.DataItem).DocumentTypeId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((DocumentType) Container.DataItem).Description %>'
                                    Width="95%" ID="txtDescription" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Workflow %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                            <ItemTemplate>
                                <tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="ddlWorkflow" DataTextField="Name"
                                    DataValueField="DocumentWorkflowId" NoValueItem="false" SelectedValue='<%# ((DocumentType) Container.DataItem).DocumentWorkflowId %>'
                                    Width="150px" DataSource="<%#this.workflows%>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemStyle HorizontalAlign="Center" />
                            <HeaderStyle />
                            <ItemTemplate>
                                <tstsc:RadioButtonEx runat="server" ID="radDefaultYn" Checked='<%# ((DocumentType) Container.DataItem).IsDefault %>'
                                    GroupName="DocumentTypes" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <HeaderStyle />
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" Checked='<%# ((DocumentType) Container.DataItem).IsActive ? true : false %>'/>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnUpdateDocumentTypes" runat="server" CausesValidation="True"
                        Text="<%$Resources:Buttons,Save%>" SkinID="ButtonPrimary" />
                    <tstsc:ButtonEx ID="btnAddDocumentType" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Add%>" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
