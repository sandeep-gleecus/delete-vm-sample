<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="Edit.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.Edit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblProjectTemplateName" runat="server" />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectTemplate_Edit_Title %>" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectTemplate_Edit_BackToList %>" />
                    </tstsc:HyperLinkEx>
                </div>
                <div class="Spacer"></div>
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String1 %>" /><br />
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String2 %>" />
                <div class="Spacer"></div>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
                <div class="Spacer"></div>
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,TemplateName %>" Required="true" AppendColon="true"
                            ID="txtNameLabel" AssociatedControlID="txtName" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx CssClass="text-box" ID="txtName" runat="server" TextMode="SingleLine"
                            MaxLength="50"  Width="512"/>
                        <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                            ErrorMessage="<%$Resources:Messages,Admin_ProjectTemplateEdit_TemplateNameRequired %>" ControlToValidate="txtName" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" Required="false" AppendColon="true"
                                ID="txtDescriptionLabel" AssociatedControlID="txtDescription" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:RichTextBoxJ ID="txtDescription" runat="server" />
                     </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ActiveYn %>" Required="true" AppendColon="true"
                            ID="ddlActiveYnLabel" AssociatedControlID="chkActiveYn" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx 
                            AppendColon="true"
                            AssociatedControlID="chkBulkChangeStatus" 
                            ID="chkBulkChangeStatusLabel" 
                            Required="true" 
                            runat="server" 
                            Text="<%$Resources:Main,ProjectTemplate_CanBulkChangeStatus %>" 
                            Tooltip="<%$Resources:Messages,ProjectTemplate_CanBulkChangeStatus %>" 
                            />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkBulkChangeStatus" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataEntry btn-group col-sm-9 col-sm-offset-3 col-lg-10 col-lg-offset-2">
                        <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
</asp:Content>
