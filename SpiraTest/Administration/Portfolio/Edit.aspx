<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true"
    CodeBehind="Edit.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Portfolio.Edit"
    Title="" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblPortfolioName" runat="server" />
                    <small>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_PortfolioEditDetails_Title %>" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server">
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
            <div>
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
                    <div class="DataEntry col-sm-9 col-sm-offset-3 col-lg-10 col-lg-offset-2 btn-group">
                        <tstsc:ButtonEx ID="btnSave" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div>
                <tstsc:TabControl ID="tclPortfolioDetails" CssClass="TabControl2" TabWidth="120"
                    TabHeight="25" TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                    DisabledTabCssClass="TabDisabled" runat="server">
                    <TabPages>
                        <tstsc:TabPage ID="tabPrograms" Caption="<%$Resources:ServerControls,TabControl_ProgramList%>" TabPageControlId="pnlPrograms" />
                    </TabPages>
                </tstsc:TabControl>
                <asp:Panel ID="pnlPrograms" runat="server" Width="100%">
                    <tstsc:GridViewEx ID="grdPrograms" CssClass="DataGrid table" runat="server" ShowSubHeader="false"
                        Width="100%" AutoGenerateColumns="False">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:ImageEx runat="server" ImageUrl="Images/org-Program-outline.svg" AlternateText="<%$Resources:Fields,Project %>" Width="16px" />
                                    <tstsc:HyperlinkEx runat="server" ToolTip='<%# "<u>" + ((ProjectGroup)Container.DataItem).Name + "</u><br />" + ((ProjectGroup) Container.DataItem).Description%>'
                                        NavigateUrl='<%# UrlRoots.RetrieveGroupAdminUrl(((ProjectGroup) Container.DataItem).ProjectGroupId, "Edit")%>'>
										<%#: ((ProjectGroup)Container.DataItem).Name%>
                                    </tstsc:HyperlinkEx>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,WebSite %>" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4">
                                <ItemTemplate>
                                    <tstsc:HyperLinkEx runat="server" Target="_blank" CssClass="external-link" NavigateUrl='<%# GlobalFunctions.FormNavigatableUrl(((ProjectGroup) Container.DataItem).Website)%>'
                                        ID="lnkWebSite" Visible='<%#!String.IsNullOrEmpty(((ProjectGroup) Container.DataItem).Website)%>'>
                                        <%#: GlobalFunctions.FormNavigatableUrl(((ProjectGroup)Container.DataItem).Website)%>
                                    </tstsc:HyperLinkEx>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ProjectTemplate %>" DataField="ProjectTemplate.Name" HtmlEncode="true" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3"/>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <%# GlobalFunctions.DisplayYnFlag(((ProjectGroup)Container.DataItem).IsActive)%>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ID %>" DataField="ProjectGroupId" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2"/>
                        </Columns>
                    </tstsc:GridViewEx>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
