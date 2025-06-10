<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin.ProjectList" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdProjectList" runat="server" SkinId="WidgetGrid">
	<Columns>
	    <tstsc:NameDescriptionFieldEx 
            HeaderText="<%$Resources:Fields,ProjectName %>" 
            HeaderStyle-Wrap="false" 
            ItemStyle-Wrap="true" 
            DataField="Name" 
            DescriptionField="Description" 
            CommandArgumentField="ProjectId"  
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1"
            />
		<tstsc:TemplateFieldEx>
			<HeaderStyle Wrap="False" />
			<ItemStyle Wrap="False" />
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Group %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:HyperlinkEx ID="lnkProjectGroup" runat="server"><%#:((Project)Container.DataItem).Group.Name%></tstsc:HyperlinkEx>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderStyle Wrap="False" />
			<ItemStyle Wrap="True" />
			<HeaderTemplate>
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,CreationDate %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx runat="server" ID="lblCreationDate"
                    Text='<%# String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((Project) Container.DataItem).CreationDate)) %>'
                    ToolTip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((Project) Container.DataItem).CreationDate)) %>' />					
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
