<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="UserDetailsAddProjectMembership.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.UserDetailsAddProjectMembership" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblUserName" runat="server" />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_Title %>" />
                    </small>

                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:LinkButtonEx ID="btnBackToUserDetails" runat="server" >
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_BackToUserDetails %>" />
                    </tstsc:LinkButtonEx>
                </div>
                <div class="Spacer"></div>
                <p><asp:Localize runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_Legend1 %>" /></p>
                <p><asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_UserDetailsAddProjectMembership_Legend2 %>" /></p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
                    DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
            </div>
        </div>
        <div class="toolbar btn-toolbar-mid-page">
            <div class="btn-group priority1" role="group">
                <tstsc:DropMenu ID="btnAdd" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add %>" />
                <tstsc:DropMenu ID="btnCancel" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Cancel %>" />
            </div>
        </div>
        <div class="row table-responsive"></div>
            <tstsc:GridViewEx ID="grdProjectMembership" CssClass="DataGrid" runat="server" Width="100%"
                AutoGenerateColumns="False" DataKeyNames="ProjectId">
                <HeaderStyle CssClass="Header" />
                <Columns>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectName %>">
                        <ItemTemplate>
                            <tstsc:LabelEx runat="server" ToolTip='<%# "<u>" + ((ProjectView) Container.DataItem).Name + "</u><br />" + ((ProjectView) Container.DataItem).Description%>' ID="Linkbutton1">
									<%# ((ProjectView)Container.DataItem).Name%>
                            </tstsc:LabelEx>
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,WebSite %>">
                        <ItemTemplate>
                            <tstsc:HyperLinkEx 
                                runat="server" 
                                Target="_blank" 
                                NavigateUrl='<%# GlobalFunctions.FormNavigatableUrl(((ProjectView) Container.DataItem).Website)%>'
                                ID="lnkWebSite" 
                                Visible='<%#!String.IsNullOrEmpty(((ProjectView) Container.DataItem).Website)%>'
                                >
                                <%# GlobalFunctions.FormNavigatableUrl(((ProjectView)Container.DataItem).Website)%>
                                <i class="far fa-window-restore pl2"></i>
                            </tstsc:HyperLinkEx>
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,CreationDate %>" DataField="CreationDate" DataFormatString="<%$GlobalFunctions:FORMAT_DATE %>" />
                    <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,ActiveYn %>">
                        <ItemTemplate>
                            <%# GlobalFunctions.DisplayYnFlag(((ProjectView)Container.DataItem).IsActive)%>
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                        <ItemTemplate>
                            <%#GlobalFunctions.ARTIFACT_PREFIX_PROJECT + String.Format(GlobalFunctions.FORMAT_ID, (int)((ProjectView)Container.DataItem).ProjectId)%>
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectRole %>">
                        <ItemTemplate>
                            <tstsc:DropDownListEx CssClass="DropDownList" runat="server"
                                DataSource="<%# projectRoles %>" DataTextField="Name" DataValueField="ProjectRoleId"
                                ID="ddlProjectRole" Width="200px" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_SelectRole %>" />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                </Columns>
            </tstsc:GridViewEx>
        </div>
    </div>
</asp:Content>
