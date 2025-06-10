<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="MembershipAdd.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Program.MembershipAdd" Title="" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblProjectGroupName" runat="server" />
                    <small>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectGroupMembershipAdd_Title %>" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
			        <tstsc:HyperLinkEx ID="lnkBackToProjectGroupMembership" runat="server" SkinID="ButtonDefault">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectGroupMembershipAdd_BackToList %>" />
			        </tstsc:HyperLinkEx>
                </div>
                <div class="Spacer"></div>
			    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectGroupMembershipAdd_String1 %>" /><br />
			    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_ProjectGroupMembershipAdd_String2 %>" />
			    <div class="Spacer"></div>
                <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
			    <asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
				    ShowSummary="True" ShowMessageBox="False" />
			    <div class="Spacer"></div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-9 col-sm-11">
                <div class="TabControlHeader">
                    <div class="btn-group priority1">
						<tstsc:DropMenu id="btnAdd" GlyphIconCssClass="fas fa-save mr3" Runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add%>" SkinID="ButtonPrimary" />
						<tstsc:DropMenu id="btnCancel" Runat="server" GlyphIconCssClass="fas fa-times mr3" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
                    </div>
                    <div class="btn-group priority3 ml0">
                        <tstsc:DropMenu id="btnFilter" GlyphIconCssClass="fas fa-filter mr3" Runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter%>" />
						<tstsc:DropMenu id="btnClearFilters" Runat="server" GlyphIconCssClass="fas fa-times mr3" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter%>" />
                    </div>
                </div>
                <div class="table-responsive">
				    <tstsc:GridViewEx id="grdUserManagement" CssClass="DataGrid table" Runat="server" PageSize="15" AllowSorting="true" AllowCustomPaging="False" AllowPaging="True" ShowSubHeader="True" Width="100%" AutoGenerateColumns="False">
					    <HeaderStyle CssClass="Header" />
					    <SubHeaderStyle CssClass="SubHeader" />
					    <Columns>
                            <tstsc:FilterSortFieldEx DataField="Profile.FirstName" HeaderText="<%$Resources:Fields,FirstName %>" FilterField="Profile.FirstName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1"/> 
                            <tstsc:FilterSortFieldEx DataField="Profile.MiddleInitial" HeaderText="<%$Resources:Fields,MiddleInitialAbbreviation %>" FilterField="Profile.MiddleInitial" FilterType="TextBox" Sortable="true"/> 
                            <tstsc:FilterSortFieldEx DataField="Profile.LastName" HeaderText="<%$Resources:Fields,LastName %>" FilterField="Profile.LastName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1"/> 
                            <tstsc:FilterSortFieldEx DataField="UserName" HeaderText="<%$Resources:Fields,UserName %>" FilterField="UserName" FilterType="TextBox" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1"/> 
                            <tstsc:FilterSortFieldEx DataField="UserId" HeaderText="<%$Resources:Fields,ID %>" FilterField="UserId" FilterType="TextBox" FilterWidth="40px" Sortable="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1"/> 
						    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProgramRole %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" DataMember="ProjectGroupRole" DataSource="<%# projectGroupRoles %>" MetaData='<%# ((User) Container.DataItem).UserId%>'
                                        DataTextField="Name" DataValueField="ProjectGroupRoleId" ID="ddlProjectGroupRole" Width="200px" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_SelectRole %>" />
                                </ItemTemplate>
						    </tstsc:TemplateFieldEx>
					    </Columns>
				    </tstsc:GridViewEx>
                </div>
			</div>
		</div>
    </div>
</asp:Content>
