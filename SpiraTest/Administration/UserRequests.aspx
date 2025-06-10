<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
    CodeBehind="UserRequests.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.UserRequests"
    Title="Untitled Page" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <asp:Localize runat="server" Text="<%$Resources:Main,UserRequests_TitleLong %>" />
    </h2>
    <div class="Spacer"></div>
    <div class="Spacer"></div>
    <p>
        <asp:Localize runat="server" Text="<%$Resources:Main,UserRequests_Legend %>" />
    </p>
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
        DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
    <div class="Spacer"></div>
    <div class="toolbar btn-toolbar-mid-page">
        <div class="btn-group priority3" role="group">
            <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-filter" ID="btnUserListFilter" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
            <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-times" ID="btnUserListClearFilters" runat="server" CausesValidation="False"
                Text="<%$Resources:Buttons,ClearFilter%>" />
        </div>
    </div>
    <tstsc:GridViewEx ID="grdUserManagement" CssClass="DataGrid" runat="server" PageSize="15"
        AllowSorting="true" AllowCustomPaging="True" AllowPaging="True" ShowSubHeader="True"
        Width="100%" AutoGenerateColumns="False" EnableViewState="false">
        <HeaderStyle CssClass="Header" />
        <SubHeaderStyle CssClass="SubHeader" />
        <Columns>
            <tstsc:FilterSortFieldEx DataField="Profile.FirstName" HeaderText="<%$Resources:Fields,FirstName %>" FilterField="Profile.FirstName"
                FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2" />
            <tstsc:FilterSortFieldEx DataField="Profile.MiddleInitial" HeaderText="<%$Resources:Fields,MiddleInitialAbbreviation %>" FilterField="Profile.MiddleInitial"
                FilterType="TextBox" Sortable="true" FilterWidth="25px" />
            <tstsc:FilterSortFieldEx DataField="Profile.LastName" HeaderText="<%$Resources:Fields,LastName %>" FilterField="Profile.LastName"
                FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2" />
            <tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$Resources:Fields,UserName %>" FilterField="UserName"
                FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" />
            <tstsc:FilterSortFieldEx DataField="Profile.IsEmailEnabled" HeaderText="<%$Resources:Fields,EmailEnabled %>" FilterField="Profile.IsEmailEnabled"
                FilterType="Flag" FilterLookupDataField="Key" FilterLookupTextField="Value" FilterWidth="70px"
                Sortable="true" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" SubHeaderStyle-CssClass="priority4" />
            <tstsc:FilterSortFieldEx DataField="UserId" HeaderText="<%$Resources:Fields,UserId %>" FilterField="UserId"
                FilterType="TextBox" FilterWidth="40px" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" />
            <tstsc:FilterSortFieldEx DataField="IsActive" HeaderText="Active" FilterField="IsActive"
                FilterType="Flag" FilterLookupDataField="Key" FilterLookupTextField="Value" FilterWidth="70px"
                Sortable="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority3" />
            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" >
                <ItemTemplate>
                    <div class="btn-group">
                        <tstsc:LinkButtonEx ID="btnApprove" runat="server" CommandName="ApproveUser" CommandArgument="<%#((User) Container.DataItem).UserId%>" >
                            <span class="fas fa-plus"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Approve %>" />
                        </tstsc:LinkButtonEx>
                        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="DeleteUser" CommandArgument="<%#((User) Container.DataItem).UserId%>" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,UserRequests_ConfirmDelete %>">
                            <span class="fas fa-trash-alt"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                        </tstsc:LinkButtonEx>
                    </div>
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
        </Columns>
    </tstsc:GridViewEx>
    <asp:ObjectDataSource ID="srcActiveFlag" runat="server" SelectMethod="RetrieveFlagLookup"
        TypeName="Inflectra.SpiraTest.Business.UserManager" />
</asp:Content>
