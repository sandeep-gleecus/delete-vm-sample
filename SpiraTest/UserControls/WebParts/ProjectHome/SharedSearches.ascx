<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SharedSearches.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.SharedSearches" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Microsoft.Security.Application" %>
<tstsc:GridViewEx ID="grdSavedSearches" runat="server" SkinID="WidgetGrid">
    <HeaderStyle CssClass="SubHeader" />
    <Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ID="ingArtifactType" runat="server" ImageUrl='<%#"Images/" + GlobalFunctions.GetIconForArtifactType(((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).ArtifactTypeId) %>' AlternateText='<%#((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).ArtifactTypeName%>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1">
			<ItemTemplate>
				<tstsc:HyperLinkEx ID="lnkApplyFilter" Runat="server" Text='<%#: ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).Name %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,CreatorId %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<%# Microsoft.Security.Application.Encoder.HtmlEncode(((Inflectra.SpiraTest.DataModel.SavedFilter)Container.DataItem).UserName)%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="">
		    <ItemTemplate>
		        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="DeleteSearch" CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).SavedFilterId%>'
		            Text="<%$Resources:Buttons,Delete %>" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,SavedSearches_DeleteConfirmation %>" />
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="">
            <ItemTemplate>
                <tstsc:HyperLinkEx ID="lnkRss" runat="server">
                    <tstsc:ImageEx ID="imgRss" runat="server" ImageUrl="Images/action-rss.svg" CssClass="w4 h4" />
                </tstsc:HyperLinkEx>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
