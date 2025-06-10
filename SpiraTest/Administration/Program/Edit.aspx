<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
    CodeBehind="Edit.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Program.Edit"
    Title="" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblProjectGroupName" runat="server" />
                    <small>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProgramEditDetails_Title %>" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProgramEditDetails_BackToList %>" />
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
            <div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Name %>" ID="txtNameLabel" AssociatedControlID="txtName" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx CssClass="text-box" ID="txtName" runat="server"
                            TextMode="SingleLine" MaxLength="50" />
                        <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                            ErrorMessage="<%$Resources:Messages,Admin_ProjectGroupDetails_NameRequired %>" ControlToValidate="txtName" />
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
                <div class="form-group row" id="portfolioFormGroup" runat="server" visible="false">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Portfolio %>" ID="ddlPortfolioLabel" AssociatedControlID="ddlPortfolio" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:DropDownListEx runat="server" NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                            ID="ddlPortfolio" DataTextField="Name" DataValueField="PortfolioId" NoValueItem="true" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ProjectTemplate %>" ID="ddlProjectTemplateLabel" AssociatedControlID="ddlProjectTemplate" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:DropDownListEx runat="server" NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                            ID="ddlProjectTemplate" DataTextField="Name" DataValueField="ProjectTemplateId" NoValueItem="true" />
                        <span>
                            <p><small><asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProgramDetails_ProjectTemplateNotes %>" /></small></p>
                        </span>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,WebSite %>" ID="txtWebSiteLabel" AssociatedControlID="txtWebSite" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx CssClass="text-box" ID="txtWebSite" runat="server" TextMode="SingleLine" MaxLength="255" />
                        <asp:RegularExpressionValidator ID="vldWebSite" runat="server" ControlToValidate="txtWebSite"
                            ErrorMessage="<%$Resources:Messages,Admin_ProjectDetails_WebSiteUrlInvalid %>" Text="*" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_URL%>" />
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
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Default %>" ID="chkDefaultYnLabel" AssociatedControlID="chkDefaultYn" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxYnEx runat="server" ID="chkDefaultYn" NoValueItem="false" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataEntry col-sm-9 col-sm-offset-3 col-lg-10 col-lg-offset-2 btn-group">
                        <tstsc:ButtonEx ID="btnSave" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div>
                <tstsc:TabControl ID="tclProjectGroupDetails" CssClass="TabControl2" TabWidth="120"
                    TabHeight="25" TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                    DisabledTabCssClass="TabDisabled" runat="server">
                    <TabPages>
                        <tstsc:TabPage ID="tabUsers" Caption="<%$Resources:ServerControls,TabControl_UserMembership%>" TabPageControlId="pnlUsers" />
                        <tstsc:TabPage ID="tabProjects" Caption="<%$Resources:ServerControls,TabControl_ProjectList%>" TabPageControlId="pnlProjects" />
                    </TabPages>
                </tstsc:TabControl>
                <asp:Panel ID="pnlUsers" runat="server" Width="100%">
                    <div class="TabControlHeader">
                        <div class="btn-group priority2">
                            <tstsc:DropMenu ID="btnMembershipSave" GlyphIconCssClass="far fa-save mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Save %>" />
                            <tstsc:DropMenu ID="btnMembershipAdd" GlyphIconCssClass="fas fa-plus mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add %>" />
                            <tstsc:DropMenu ID="btnMembershipDelete" GlyphIconCssClass="fas fa-trash-alt mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Remove %>" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ProjectGroupDetails_ConfirmUserDelete %>" />
                        </div>
                    </div>
                    <tstsc:GridViewEx ID="grdUserMembership" CssClass="DataGrid table" runat="server" Width="100%"
                        AutoGenerateColumns="False">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderStyle-CssClass="TickIcon" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="40px" HeaderStyle-Width="40px">
                                <ItemTemplate>
                                    <tstsc:CheckBoxEx runat="server" ID="chkDeleteMembership" MetaData="<%# ((ProjectGroupUser) Container.DataItem).UserId%>" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:BoundFieldEx DataField="FullName" HeaderText="<%$Resources:Fields,FullName %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
                            <tstsc:BoundFieldEx DataField="UserName" HeaderText="<%$Resources:Fields,UserName %>" HtmlEncode="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
                            <tstsc:BoundFieldEx DataField="User.Profile.Department" HeaderText="<%$Resources:Fields,Department %>" HtmlEncode="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
                            <tstsc:BoundFieldEx DataField="User.Profile.Organization" HeaderText="<%$Resources:Fields,Organization %>" HtmlEncode="true" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProgramRole %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" >
                                <ItemTemplate>
                                    <tstsc:DropDownListEx CssClass="DropDownList" runat="server" MetaData="<%# ((ProjectGroupUser) Container.DataItem).UserId%>"
                                        DataSource="<%# projectGroupRoles %>" DataTextField="Name" DataValueField="ProjectGroupRoleId"
                                        ID="ddlProjectGroupRole" Width="100%" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                </asp:Panel>
                <asp:Panel ID="pnlProjects" runat="server" Width="100%">
                    <tstsc:GridViewEx ID="grdProjectList" CssClass="DataGrid table" runat="server" ShowSubHeader="false"
                        Width="100%" AutoGenerateColumns="False">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:ImageEx runat="server" ImageUrl="Images/org-Project-outline.svg" AlternateText="<%$Resources:Fields,Project %>" Width="16px" />
                                    <tstsc:HyperlinkEx runat="server" SkinID="ButtonLink" ToolTip='<%# "<u>" + ((ProjectView)Container.DataItem).Name + "</u><br />" + ((ProjectView) Container.DataItem).Description%>'
                                        NavigateUrl='<%# UrlRoots.RetrieveProjectAdminUrl(((ProjectView) Container.DataItem).ProjectId, "Edit")%>'>
										<%#: ((ProjectView)Container.DataItem).Name%>
                                    </tstsc:HyperlinkEx>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,WebSite %>" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4">
                                <ItemTemplate>
                                    <tstsc:HyperLinkEx runat="server" Target="_blank" CssClass="external-link" NavigateUrl='<%# GlobalFunctions.FormNavigatableUrl(((ProjectView) Container.DataItem).Website)%>'
                                        ID="lnkWebSite" Visible='<%#!String.IsNullOrEmpty(((ProjectView) Container.DataItem).Website)%>'>
                                        <%#: GlobalFunctions.FormNavigatableUrl(((ProjectView)Container.DataItem).Website)%>
                                    </tstsc:HyperLinkEx>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,CreationDate %>" DataField="CreationDate" HtmlEncode="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3"/>
                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ProjectTemplate %>" DataField="ProjectTemplateName" HtmlEncode="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3"/>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <%# GlobalFunctions.DisplayYnFlag(((ProjectView)Container.DataItem).IsActive)%>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ID %>" DataField="ProjectId" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2"/>
                        </Columns>
                    </tstsc:GridViewEx>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
