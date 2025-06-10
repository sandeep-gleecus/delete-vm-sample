<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="PortfolioList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.PortfolioList" Title="" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h1>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_PortfolioList_Title %>" />
    </h1>
	<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_Portfolios_String1 %>" /><br />
	<asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Main,Admin_Portfolios_String2 %>" />
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage" DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />

    <div class="btn-toolbar-mid-page mt5">
        <div class="btn-group priority2" role="group">
			<tstsc:DropMenu ID="btnAdd" GlyphIconCssClass="fas fa-plus mr2" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add %>" Authorized_ArtifactType="None" Authorized_Permission="SystemAdmin" />
        </div>
        <div class="btn-group priority3">
            <tstsc:DropMenu ID="btnApplyFilters" GlyphIconCssClass="fas fa-filter mr2" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
			<tstsc:DropMenu ID="btnClearFilters" GlyphIconCssClass="fas fa-times mr2" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter %>" />
        </div>
    </div>


	<tstsc:GridViewEx ID="grdPortfolios" CssClass="DataGrid table" runat="server" PageSize="15" AllowSorting="true" AllowCustomPaging="False" AllowPaging="true" ShowSubHeader="True" Width="100%" AutoGenerateColumns="False">
		<SubHeaderStyle CssClass="SubHeader" />
		<HeaderStyle CssClass="Header" />
		<SelectedRowStyle CssClass="Selected" />
		<Columns>
			<tstsc:TemplateFieldEx ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1">
				<HeaderTemplate>
					<asp:Localize runat="server" Text="<%$Resources:Fields,Name %>" />
					<tstsc:ImageButtonEx ID="btnPortfolioNameSortUp" runat="server" CommandName="SortColumns" CausesValidation="False" CommandArgument="Name ASC" AlternateText="<%$Resources:ServerControls,GridViewEx_SortByAscending %>" ToolTip="<%$Resources:ServerControls,GridViewEx_SortByAscending %>" ImageUrl="Images/SortAscending.gif" HoverImageUrl="Images/SortAscendingHover.gif" SelectedImageUrl="Images/SortAscendingSelected.gif" Selected='<%# GetCurrentSortExpression() == "Name ASC"%>' />
					<tstsc:ImageButtonEx ID="btnPortfolioNameSortDown" runat="server" CommandName="SortColumns" CausesValidation="False" CommandArgument="Name DESC" AlternateText="<%$Resources:ServerControls,GridViewEx_SortByDescending %>" ToolTip="<%$Resources:ServerControls,GridViewEx_SortByDescending %>" ImageUrl="Images/SortDescending.gif" HoverImageUrl="Images/SortDescendingHover.gif" SelectedImageUrl="Images/SortDescendingSelected.gif" Selected='<%# GetCurrentSortExpression() == "Name DESC"%>' />
				</HeaderTemplate>
				<SubHeaderTemplate>
					<tstsc:TextBoxEx CssClass="text-box" ID="txtProjectNameFilter" runat="server" TextMode="SingleLine" Style="width: 200px" MetaData="Name" />
				</SubHeaderTemplate>
				<ItemTemplate>
                <tstsc:ImageEx runat="server" ImageUrl="Images/org-Program-outline.svg" AlternateText="<%$Resources:Fields,Program %>" Width="16px" />
					<tstsc:HyperLinkEx runat="server" ToolTip='<%# Inflectra.SpiraTest.Common.Strings.StripHTML(((Portfolio)Container.DataItem).Description,false,true) %>' NavigateUrl='<%# UrlRoots.RetrievePortfolioAdminUrl(((Portfolio) Container.DataItem).PortfolioId, "Edit")%>'>
								<%#: ((Portfolio)Container.DataItem).Name%>
					</tstsc:HyperLinkEx>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" FooterStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority3">
				<HeaderTemplate>
					<asp:Localize runat="server" Text="<%$Resources:Fields,Description %>" />
				</HeaderTemplate>
				<SubHeaderTemplate>
					<tstsc:TextBoxEx CssClass="text-box" ID="txtDescriptionFilter" runat="server" TextMode="SingleLine" Style="width: 200px" MetaData="Description" />
				</SubHeaderTemplate>
				<ItemTemplate>
					<%# GlobalFunctions.HtmlRenderAsPlainText(((Portfolio)Container.DataItem).Description)%>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" FilterField="IsActive" DataField="IsActive" Sortable="true" FilterType="Flag" FilterLookupDataField="Key" FilterLookupTextField="Value" FilterWidth="70px" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" FooterStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2"/>
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,ID %>" FilterField="PortfolioId" DataField="PortfolioId" Sortable="true" FilterType="TextBox" FilterWidth="40px" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" FooterStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority3"/>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-Wrap="false" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" FooterStyle-CssClass="priority4" SubHeaderStyle-CssClass="priority4">
				<ItemTemplate>
                    <div class="btn-group">
						<tstsc:HyperLinkEx ID="lnkEdit" runat="server" NavigateUrl='<%# UrlRoots.RetrievePortfolioAdminUrl(((Portfolio) Container.DataItem).PortfolioId, "Edit")%>' SkinID="ButtonDefault">
                            <span class="far fa-edit"></span>
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Buttons,Edit %>" />
                        </tstsc:HyperLinkEx>
						<tstsc:LinkButtonEx runat="server" CausesValidation="false" Confirmation="true"  ConfirmationMessage="<%$Resources:Messages,Admin_Portfolios_DeleteConfirm %>" CommandName="DeleteRow" CommandArgument='<%# ((Portfolio) Container.DataItem).PortfolioId%>' ID="lnkDelete">
                            <span class="fas fa-trash-alt"></span>
                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Buttons,Delete %>" />
						</tstsc:LinkButtonEx>
                    </div>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
	<asp:ObjectDataSource ID="srcActiveFlag" runat="server" SelectMethod="RetrieveFlagLookup" TypeName="Inflectra.SpiraTest.Business.PortfolioManager" />


    <script type="text/javascript">
        //Add the event handlers
        $(document).ready(function ()
        {
            $('#<%=grdPortfolios.ClientID%>').on("keydown", function (evt)
            {
                var keynum = evt.keyCode | evt.which;
                if (keynum == 13)
                {
                    //Click on the button inside the DIV
                    $('#<%=grdPortfolios.ClientID%> button').trigger('click');
                }
            });
        });
    </script>
</asp:Content>
