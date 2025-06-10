<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="AssignedDocuments.ascx.cs" 
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.AssignedDocuments" 
    %>

<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>

<tstsc:GridViewEx 
    id="grdOwnedDocuments" 
    Runat="server" 
    EnableViewState="false" 
    SkinId="WidgetGrid" 
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
    >
	<Columns>
		<tstsc:TemplateFieldEx 
            HeaderColumnSpan="2"  
            HeaderStyle-CssClass="priority1" 
            ControlStyle-CssClass="priority1 w4 h4"
            >
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx 
                    CssClass="w4 h4" 
                    ImageUrl='<%# "Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(((ProjectAttachmentView)(Container.DataItem)).Filename)%>'
                    AlternateText="Document" 
                    ID="imgIcon" 
                    runat="server" 
                    />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx 
            ItemStyle-Wrap="True" 
            HeaderColumnSpan="-1" 
            DataField="Filename" 
            NameMaxLength="40" 
            CommandArgumentField="AttachmentId" 
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1" 
            />
		<tstsc:BoundFieldEx 
            DataField="ProjectName" 
            HeaderText="<%$Resources:Fields,Project %>" 
            MaxLength="20"  
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            />
		<tstsc:BoundFieldEx 
            DataField="DocumentStatusName" 
            HeaderText="<%$Resources:Fields,Status %>" 
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            />
		<tstsc:BoundFieldEx 
            ItemStyle-Wrap="False" 
            DataField="DocumentTypeName" 
            HeaderText="<%$Resources:Fields,Type %>" 
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            />
		<tstsc:TemplateFieldEx 
            ItemStyle-Wrap="False" 
            HeaderStyle-Wrap="False"  
            HeaderStyle-CssClass="priority4" 
            ItemStyle-CssClass="priority4"
            >
			<HeaderTemplate>
                <asp:Localize 
                    ID="Localize1" 
                    runat="server" 
                    Text="<%$Resources:Fields,DateOpened %>" 
                    />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx 
                    Runat="server" 
                    Text='<%# String.Format(GlobalFunctions.FORMAT_DATE,  GlobalFunctions.LocalizeDate(((ProjectAttachmentView) Container.DataItem).UploadDate)) %>' 
                    Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME,  GlobalFunctions.LocalizeDate(((ProjectAttachmentView) Container.DataItem).UploadDate)) %>' 
                    ID="lblCreationDate"
                    />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns> 
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx 
    ID="ajxScriptManager" 
    runat="server"
    >
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/DocumentsService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
