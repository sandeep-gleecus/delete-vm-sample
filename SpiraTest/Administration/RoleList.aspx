<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="RoleList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.RoleList" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <tstsc:LabelEx runat="server" CssClass="Title" Text="<%$Resources:Main,SiteMap_ViewEditRoles %>" />
    </h2>
    <div class="Spacer"></div>
    <div class="Spacer"></div>
    <p><asp:Localize runat="server" Text="<%$Resources:Main,Admin_RoleList_Intro %>" /></p>
    <div class="Spacer"></div>
    <div class="toolbar btn-toolbar-mid-page">
        <div class="btn-group priority3" role="group">
            <tstsc:DropMenu ID="btnRoleAdd" GlyphIconCssClass="mr3 fas fa-plus" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,AddRole %>" />
        </div>
    </div>
    <div class="table-responsive">
        <tstsc:GridViewEx ID="grdProjectRolesList" SkinID="DataGrid" runat="server" Width="100%"
            DataMember="ProjectRole" AutoGenerateColumns="False">
            <HeaderStyle CssClass="Header" />
            <Columns>
                <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,RoleName %>" DataField="Name" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
                <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,Description %>" HeaderStyle-Width="50%" DataField="Description" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" />
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" >
                    <ItemTemplate>
                        <%# GlobalFunctions.DisplayYnFlag(((ProjectRole)Container.DataItem).IsActive)%>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" >
                    <ItemTemplate>
                        <%# GlobalFunctions.ARTIFACT_PREFIX_PROJECT_ROLE + String.Format(GlobalFunctions.FORMAT_ID, ((ProjectRole)Container.DataItem).ProjectRoleId)%>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" >
                    <ItemTemplate>
                        <div class="btn-group">
                            <tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" NavigateUrl='<%# "RoleDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ROLE_ID + "=" + ((ProjectRole) Container.DataItem).ProjectRoleId%>'>
                                <span runat="server"  class='<%# ((((ProjectRole) Container.DataItem).ProjectRoleId) > 1)? "far fa-edit" : "far fa-eye" %>'></span>&nbsp;
                                <asp:Localize runat="server" Text='<%# ((((ProjectRole) Container.DataItem).ProjectRoleId) > 1)? Resources.Buttons.Edit : Resources.Buttons.View %>'/>
                            </tstsc:HyperLinkEx>
                            <tstsc:LinkButtonEx runat="server" CommandName="DeleteRole" Enabled='<%# ((int)((ProjectRole) Container.DataItem).ProjectRoleId) > 6 %>'
                                CommandArgument='<%# ((ProjectRole) Container.DataItem).ProjectRoleId%>'
                                CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_RoleList_DeleteConfirm %>">
                                <span class="fas fa-trash-alt"></span>
                                <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                            </tstsc:LinkButtonEx>
                        </div>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
            </Columns>
        </tstsc:GridViewEx>
    </div>
</asp:Content>
