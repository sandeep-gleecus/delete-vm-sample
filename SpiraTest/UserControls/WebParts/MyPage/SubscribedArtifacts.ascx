<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SubscribedArtifacts.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.SubscribedArtifacts" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<tstsc:GridViewEx ID="grdSubscribedArtifacts" runat="server" SkinID="WidgetGrid" DataKeyNames="ArtifactTypeId, ArtifactId">
    <HeaderStyle CssClass="SubHeader" />
    <Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon"  HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ID="ingArtifactType" runat="server" Visible='<%#(((NotificationUserSubscriptionView) Container.DataItem).ArtifactTypeId) != (int)Artifact.ArtifactTypeEnum.Document %>' ImageUrl='<%#"Images/" + GlobalFunctions.GetIconForArtifactType(((NotificationUserSubscriptionView) Container.DataItem).ArtifactTypeId) %>' AlternateText='<%#((NotificationUserSubscriptionView) Container.DataItem).ArtifactTypeName%>' />
			    <tstsc:ImageEx ID="ingFileType" runat="server" Visible='<%#(((NotificationUserSubscriptionView) Container.DataItem).ArtifactTypeId) == (int)Artifact.ArtifactTypeEnum.Document %>' ImageUrl='<%#"Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(((NotificationUserSubscriptionView) Container.DataItem).ArtifactName) %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<ItemTemplate>
				<tstsc:HyperLinkEx ID="lnkViewArtifact" Runat="server" Text='<%#: ((NotificationUserSubscriptionView) Container.DataItem).ArtifactName %>' ToolTip='<%# "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(((NotificationUserSubscriptionView) Container.DataItem).ArtifactName) + "</u><br />" + GlobalFunctions.HtmlRenderAsPlainText(((NotificationUserSubscriptionView) Container.DataItem).ArtifactDescription)  %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:BoundFieldEx ItemStyle-Wrap="True" DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
        <tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,LastUpdateDate %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx ID="lblLastUpdateDate" Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE, ((NotificationUserSubscriptionView) Container.DataItem).LastUpdateDate) %>' Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, ((NotificationUserSubscriptionView) Container.DataItem).LastUpdateDate) %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText=""  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
		    <ItemTemplate>
		        <tstsc:LinkButtonEx ID="btnUnsubscribe" runat="server" CommandName="Unsubscribe" CommandArgument='<%# ((NotificationUserSubscriptionView) Container.DataItem).ArtifactTypeId + ":" + ((NotificationUserSubscriptionView) Container.DataItem).ArtifactId%>'
		            CausesValidation="false" Confirmation="false">
                    <span class="fas fa-times"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Unsubscribe %>" />
		        </tstsc:LinkButtonEx>
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
