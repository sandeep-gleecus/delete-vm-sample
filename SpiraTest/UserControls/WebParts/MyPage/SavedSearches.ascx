<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SavedSearches.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.SavedSearches" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx ID="grdSavedSearches" runat="server" SkinID="WidgetGrid">
    <HeaderStyle CssClass="SubHeader" />
    <Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon"  HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ID="ingArtifactType" runat="server" ImageUrl='<%#"Images/" + GlobalFunctions.GetIconForArtifactType(((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).ArtifactTypeId) %>' AlternateText='<%#((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).ArtifactTypeName%>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<ItemTemplate>
				<tstsc:HyperLinkEx ID="lnkApplyFilter" Runat="server" Text='<%#: ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).Name %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Project %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<%#: ((Inflectra.SpiraTest.DataModel.SavedFilter)Container.DataItem).ProjectName%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText=""  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
		    <ItemTemplate>
		        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="DeleteSearch" CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).SavedFilterId%>'
		            Text="<%$Resources:Buttons,Delete %>" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,SavedSearches_DeleteConfirmation %>" />
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText=""  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
            <ItemTemplate>
                <tstsc:HyperLinkEx ID="lnkRss" runat="server">
                    <tstsc:ImageEx ID="imgRss" runat="server" ImageUrl="Images/action-rss.svg" CssClass="w4 h4"  />
                </tstsc:HyperLinkEx>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
