<%@ Page Language="c#" CodeBehind="ProjectTemplateCreate.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplateCreate" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectTemplateCreate_Title %>" />
                    <small>
                        <tstsc:LabelEx ID="lblProjectTemplateName" runat="server" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="ProjectTemplateList.aspx">
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
                            MaxLength="50" />
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
                        <tstsc:LabelEx ID="lblTemplateBasedOn" Text="<%$Resources:Main,ProjectCreate_BasedOn %>" Required="True" runat="server" AssociatedControlID="ddlExistingTemplates" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <span class="btn-group priority1 radio-group" role="group">
                            <asp:Label ID="lblDefaultTemplate" data-checked="checked" runat="server" AssociatedControlID="radDefaultTemplate" CssClass="btn btn-default">
						        <tstsc:RadioButtonEx Checked="true" ID="radDefaultTemplate" runat="server" GroupName="BasedOn" AutoPostBack="true" />
						        <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_TemplateDefault %>" />
                            </asp:Label>
                            <asp:Label ID="lblExistingTemplate" runat="server" AssociatedControlID="radExistingTemplate" CssClass="btn btn-default">
						        <tstsc:RadioButtonEx ID="radExistingTemplate" runat="server" GroupName="BasedOn" AutoPostBack="true" />
						        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectTemplateCreate_ExistingTemplate %>" />
                            </asp:Label>
                        </span>
                        <asp:PlaceHolder ID="plcExistingTemplates" runat="server">
                            <div class="Spacer"></div>
                            <tstsc:DropDownListEx runat="server"
                                ID="ddlExistingTemplates" DataTextField="Name" DataValueField="ProjectTemplateId" NoValueItem="false" AutoPostBack="true" />
                        </asp:PlaceHolder>
                        <div class="alert alert-information mt4">
                            <p>
                                <span class="fa fa-info-circle"></span>
                                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectTemplateCreate_BasedOnNotes %>" />
                            </p>
                        </div>
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
                    <div class="DataEntry btn-group col-sm-9 col-sm-offset-3 col-lg-10 col-lg-offset-2">
                        <tstsc:ButtonEx ID="btnInsert" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Insert%>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
