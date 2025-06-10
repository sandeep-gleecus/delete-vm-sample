<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentArtifacts.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.RecentArtifacts" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<tstsc:GridViewEx ID="grdRecentArtifacts" runat="server" SkinID="WidgetGrid" DataKeyNames="ArtifactTypeId, ArtifactId">
    <HeaderStyle CssClass="SubHeader" />
    <Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon"  HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ID="ingArtifactType" runat="server" Visible='<%#(((ArtifactInfoEx) Container.DataItem).ArtifactTypeId) != (int)Artifact.ArtifactTypeEnum.Document %>' ImageUrl='<%#"Images/" + GlobalFunctions.GetIconForArtifactType(((ArtifactInfoEx) Container.DataItem).ArtifactTypeId) %>' AlternateText='<%#((ArtifactInfoEx) Container.DataItem).ArtifactToken%>' />
			    <tstsc:ImageEx ID="ingFileType" runat="server" Visible='<%#(((ArtifactInfoEx) Container.DataItem).ArtifactTypeId) == (int)Artifact.ArtifactTypeEnum.Document %>' ImageUrl='<%#"Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(((ArtifactInfoEx) Container.DataItem).Name) %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<ItemTemplate>
				<tstsc:HyperLinkEx ID="lnkViewArtifact" Runat="server" Text='<%#: ((ArtifactInfoEx) Container.DataItem).Name %>' ToolTip='<%# "<u>[" + ((ArtifactInfoEx) Container.DataItem).ArtifactToken + "] " + Microsoft.Security.Application.Encoder.HtmlEncode(((ArtifactInfoEx) Container.DataItem).Name) + "</u><br />" + GlobalFunctions.HtmlRenderAsPlainText(((ArtifactInfoEx) Container.DataItem).Description)  %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:BoundFieldEx ItemStyle-Wrap="True" DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
        <tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,LastAccessed %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx ID="lblLastUpdateDate" Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE, ((ArtifactInfoEx) Container.DataItem).LastAccessed) %>' Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, ((ArtifactInfoEx) Container.DataItem).LastAccessed) %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
