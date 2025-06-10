<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectList" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:GridViewEx id="grdProjectList" runat="server" SkinId="WidgetGrid">
    <SelectedRowStyle CssClass="Selected" />
    <Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,ProjectName %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/org-Project-Outline.svg" AlternateText="Product" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:NameDescriptionFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" DataField="Name" DescriptionField="Description" CommandArgumentField="ProjectId" HeaderColumnSpan="-1" />
	    <tstsc:TemplateFieldEx>
		    <HeaderStyle Wrap="False" />
		    <ItemStyle Wrap="False" />
		    <HeaderTemplate>
			    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,WebSite %>" />
		    </HeaderTemplate>
		    <ItemTemplate>
				<tstsc:HyperLinkEx 
                    Runat="server" 
                    CssClass="url-to-shorten" 
                    Target="_blank" 
                    NavigateUrl='<%# GlobalFunctions.FormNavigatableUrl(((ProjectView) Container.DataItem).Website)%>' 
                    ID="lnkWebSite" 
                    Visible='<%#!String.IsNullOrEmpty(((ProjectView) Container.DataItem).Website)%>'
                    >
					<%#: GlobalFunctions.FormNavigatableUrl(((ProjectView)Container.DataItem).Website)%>
				</tstsc:HyperLinkEx>
                &nbsp;
                <i class="far fa-window-restore" title="opens in new windows"></i>
		    </ItemTemplate>
	    </tstsc:TemplateFieldEx>
	    <tstsc:TemplateFieldEx HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
		    <HeaderStyle Wrap="False" />
		    <ItemStyle Wrap="True" />
		    <HeaderTemplate>
			    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,CreationDate %>" />
		    </HeaderTemplate>
		    <ItemTemplate>
			    <tstsc:LabelEx runat="server" ID="lblCreationDate" ToolTip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((ProjectView)Container.DataItem).CreationDate))%>'>
				    <%# String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((ProjectView)Container.DataItem).CreationDate))%>
			    </tstsc:LabelEx>
		    </ItemTemplate>
	    </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
