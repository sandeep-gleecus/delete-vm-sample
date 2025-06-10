<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
    CodeBehind="PortfolioCreate.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.PortfolioCreate"
    Title="" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_PortfolioCreate_Title %>" />
                    <small>
                        <tstsc:LabelEx ID="lblPortfolioName" runat="server" />
                    </small>
                </h1>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="PortfolioList.aspx">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PortfolioDetails_BackToList %>" />
                    </tstsc:HyperLinkEx>
                </div>
                <div class="Spacer"></div>
                <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectGroupDetails_String1 %>" /><br />
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectGroupDetails_String2 %>" />
                <div class="Spacer"></div>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
                <div class="Spacer"></div>
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-11 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Name %>" ID="txtNameLabel" AssociatedControlID="txtName" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx CssClass="text-box" ID="txtName" runat="server"
                            TextMode="SingleLine" MaxLength="50" />
                        <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                            ErrorMessage="<%$Resources:Messages,Admin_PortfolioDetails_NameRequired %>" ControlToValidate="txtName" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" ID="txtDescriptionLabel" AssociatedControlID="txtDescription" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:RichTextBoxJ ID="txtDescription" runat="server" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ActiveYn %>" ID="ddlActiveYnLabel" AssociatedControlID="chkActiveYn" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" NoValueItem="false"/>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataEntry btn-group col-sm-9 col-sm-offset-3 col-lg-10 col-lg-offset-2">
                        <tstsc:ButtonEx ID="btnInsert" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Insert %>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
