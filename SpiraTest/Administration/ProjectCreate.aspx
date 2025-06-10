<%@ Page Language="c#" CodeBehind="ProjectCreate.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectCreate" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">

    <h2>
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectCreate_Title %>" />
        <small>
            <tstsc:LabelEx ID="lblProjectName" runat="server" />
        </small>
    </h2>


    <div class="btn-group priority1" role="group">
        <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="ProjectList.aspx">
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_BackToList %>" />
        </tstsc:HyperLinkEx>
    </div>

    <p class="my5">
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String1 %>" /><br />
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String2 %>" />
    </p>

    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
        DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />




    <div class="df mb4">
        <tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Fields,ProjectName %>" Required="true" AppendColon="true"
            ID="txtNameLabel" AssociatedControlID="txtName"/>
        <div>
            <tstsc:TextBoxEx ID="txtName" runat="server" TextMode="SingleLine"
                MaxLength="50" Width="512" />
            <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                ErrorMessage="<%$Resources:Messages,Admin_ProjectDetails_ProjectNameRequired %>" ControlToValidate="txtName" />
        </div>
    </div>
    <div class="df mb4">
        <tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Fields,Description %>" Required="false" AppendColon="true"
            ID="txtDescriptionLabel" AssociatedControlID="txtDescription" />
        <div>
            <tstsc:RichTextBoxJ ID="txtDescription" runat="server" />
         </div>
    </div>
    <div class="df mb4">
        <tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Fields,WebSite %>" Required="false" AppendColon="true"
            ID="txtWebSiteLabel" AssociatedControlID="txtWebSite" />
        <div>
            <tstsc:TextBoxEx CssClass="text-box" ID="txtWebSite" runat="server"
                TextMode="SingleLine" MaxLength="255"  Width="512" />
            <asp:RegularExpressionValidator ID="vldWebSite" runat="server" ControlToValidate="txtWebSite"
                ErrorMessage="<%$Resources:Messages,Admin_ProjectDetails_WebSiteUrlInvalid %>" Text="*" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_URL%>" />
        </div>
    </div>
    <div class="df mb4">
        <tstsc:LabelEx CssClass="w7" ID="lblProjectBasedOn" Text="<%$Resources:Main,ProjectCreate_BasedOn %>" Required="True" runat="server" AssociatedControlID="ddlExistingProjects" AppendColon="true" />
        <div>
            <span class="btn-group priority1 radio-group" role="group">
                <asp:Label ID="lblNewProject" data-checked="checked" runat="server" AssociatedControlID="radNewProject" CssClass="btn btn-default">
					<tstsc:RadioButtonEx Checked="true" ID="radNewProject" runat="server" GroupName="ProjectBasedOn" AutoPostBack="true" />
					<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_TemplateNewProject %>" />
                </asp:Label>
                <asp:Label ID="lblExistingProject" runat="server" AssociatedControlID="radExistingProject" CssClass="btn btn-default">
					<tstsc:RadioButtonEx ID="radExistingProject" runat="server" GroupName="ProjectBasedOn" AutoPostBack="true" />
					<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_TemplateExistingProject %>" />
                </asp:Label>
            </span>
            <asp:PlaceHolder ID="plcExistingProjects" runat="server">
                <div class="mt4"></div>
                <tstsc:DropDownListEx runat="server"
                    ID="ddlExistingProjects" DataTextField="Name" DataValueField="ProjectId" NoValueItem="false" AutoPostBack="true" />
                <div class="alert alert-info mw10 o-50 o-100-hover transition-all">
                    <p>
                        <i class="fas fa-info-circle"></i>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_BasedOnNotes %>" />
                    </p>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>
    <div class="df mb4">
        <tstsc:LabelEx CssClass="w7" ID="lblProjectTemplate" Text="<%$Resources:Fields,Template %>" Required="True" runat="server" AssociatedControlID="ddlProjectTemplates" AppendColon="true" />
        <div>
            <tstsc:DropDownListEx runat="server"
                ID="ddlProjectTemplates" DataTextField="Name" DataValueField="ProjectTemplateId" NoValueItem="false" AppendDataBoundItems="true" />
            <div class="alert alert-info mw10 o-50 o-100-hover transition-all">
                <p>
                    <i class="fas fa-info-circle"></i> 
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_TemplateNotes2 %>" />
                </p>
            </div>
        </div>
    </div>
    <asp:PlaceHolder runat="server" ID="plcProjectGroup">
        <div class="df mb4">
            <tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Fields,Program %>" Required="true" AppendColon="true"
                ID="ddlProjectGroupLabel" AssociatedControlID="ddlProjectGroup" />
            <div>
                <tstsc:DropDownListEx ID="ddlProjectGroup" runat="server"
                    DataMember="ProjectGroup" DataTextField="Name" DataValueField="ProjectGroupId" />
            </div>
        </div>
    </asp:PlaceHolder>
    <div class="df mb4" runat="server" id="grpBaseline">
		<tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Main,Admin_ProjectEdit_BaseliningEnabled %>" AppendColon="false" AssociatedControlID="chkBaseline" />
		<div>
			<tstsc:CheckBoxYnEx runat="server" ID="chkBaseline" />
		</div>
    </div>
    <div class="df mb5">
        <tstsc:LabelEx CssClass="w7" runat="server" Text="<%$Resources:Fields,ActiveYn %>" Required="true" AppendColon="true"
            ID="ddlActiveYnLabel" AssociatedControlID="chkActiveYn" />
        <div>
            <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" />
        </div>
    </div>

    <div class="btn-group ml7">
        <tstsc:ButtonEx ID="btnInsert" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Insert%>" CausesValidation="True" />
        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
    </div>
</asp:Content>
