<%@ Page Language="c#" CodeBehind="UserLdapImport.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.UserLdapImport" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
    Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,UserLdapImport_Title %>" />
                </h2>
                <div class="Spacer"></div>
                <tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" NavigateUrl="UserList.aspx">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Main,UserLdapImport_BackToUserList %>"  />
                </tstsc:HyperLinkEx>
                <div class="Spacer"></div>
                <asp:Localize runat="server" Text="<%$Resources:Main,UserLdapImport_Legend1 %>" />
                <tstsc:HyperLinkEx ID="lnkLdapConfiguration" runat="server" NavigateUrl="LdapConfiguration.aspx" Text="<%$Resources:Main,UserLdapImport_LdapConfigLink %>" />,
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,UserLdapImport_Legend2 %>" />
                <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType%>.<br />
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,UserLdapImport_Legend3 %>" />
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-lg-9 TabControlHeader">
                <div class="btn-group priority3">
                    <tstsc:DropMenu ID="btnFilter" GlyphIconCssClass="mr3 fas fa-filter" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
                    <tstsc:DropMenu ID="btnClearFilters" GlyphIconCssClass="mr3 fas fa-times" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter%>" />
                </div>
                <div class="btn-group priority1">
                    <tstsc:DropMenu ID="btnImport" runat="server" Text="<%$Resources:Buttons,Import%>" CausesValidation="True"  SkinID="ButtonPrimary" />
                    <tstsc:DropMenu ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-9">
                <tstsc:GridViewEx ID="grdLdapUsers" Width="100%" runat="server" ShowSubHeader="True"
                    DataSource="<%# ldapUserCollection%>" CssClass="DataGrid" AutoGenerateColumns="false"
                    ShowHeader="true" PageSize="15" AllowPaging="True">
                    <HeaderStyle CssClass="Header" />
                    <SubHeaderStyle CssClass="SubHeader" />
                    <Columns>
                        <tstsc:CheckBoxFieldEx HeaderText="" HeaderStyle-CssClass="TickIcon" MetaDataField="Login"  ItemStyle-CssClass="priority1" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,CommonName %>" DataField="CommonName" FilterField="CommonName"
                            FilterType="TextBox" Sortable="true" FilterWidth="95%"  ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,Login%>" DataField="Login" FilterField="Login"
                            FilterType="TextBox" Sortable="true" FilterWidth="95%"  ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,FirstName%>" DataField="FirstName" FilterField="FirstName"
                            FilterType="TextBox" Sortable="true" FilterWidth="95%"  ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,MiddleInitial%>" DataField="MiddleInitial" FilterField="MiddleInitial"
                            FilterType="TextBox" Sortable="true" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,LastName%>" DataField="LastName" FilterField="LastName"
                            FilterType="TextBox" Sortable="true" FilterWidth="95%"  ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" />
                        <tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,EmailAddress%>" DataField="EmailAddress" FilterField="EmailAddress"
                            FilterType="TextBox" Sortable="true" FilterWidth="95%"  ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" />
                        <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,DistinguishedName%>" DataField="DistinguishedName"  ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" />
                    </Columns>
                </tstsc:GridViewEx>
            </div>
        </div>
    </div>
</asp:Content>
