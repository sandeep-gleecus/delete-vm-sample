<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSynchronization.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.DataSynchronization" Title="Untitled Page" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">

    <h2 class="my2">
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DataSynchronization_Title %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>
    <p class="my3">
        <%=this.productName%> <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSynchronization_Intro1 %>" />
    </p>
	<tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
		ShowSummary="True" ShowMessageBox="False" />
    <div class="mb5">
        <tstsc:LinkButtonEx ID="btnRefresh" runat="server">
            <span class="fas fa-sync"></span>
            <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>"/>
        </tstsc:LinkButtonEx>
    </div>

        
    <tstsc:GridViewEx ID="grdDataSynchronization" Runat="server" AutoGenerateColumns="False" CssClass="DataGrid" ShowHeader="true" ShowFooter="false" ShowSubHeader="false" Width="100%">
	    <HeaderStyle CssClass="Header" />
	    <Columns>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSynchronization_PlugIn %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
		        <ItemTemplate>
                    <h4>
    		            <tstsc:LabelEx ID="lblDataSyncName" Text='<%#: ((DataSyncSystem) Container.DataItem).DisplayName%>' ToolTip='<%# ((DataSyncSystem) Container.DataItem).Description%>' runat="server" />
                        <small>
                            <asp:Literal runat="server" Text="<%#: ((DataSyncSystem) Container.DataItem).Name%>" />
                        </small>
                    </h4>
		        </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,LastSyncDate %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			    <ItemTemplate>
				    <%# (!((DataSyncSystem) Container.DataItem).LastSyncDate.HasValue) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate((DateTime)(((DataSyncSystem) Container.DataItem) ["LastSyncDate"])))%>
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
		        <ItemTemplate>
		            <tstsc:LabelEx ID="lblActive" runat="server" Text='<%#GlobalFunctions.DisplayYnFlag(((DataSyncSystem) Container.DataItem).IsActive) %>' />
		        </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:BoundFieldEx DataField="DataSyncStatusName" HeaderText="<%$Resources:Fields,Status %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
            <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ExternalKey %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3"/>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSynchronization_DataMapping %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			    <ItemTemplate>
				    <tstsc:HyperLinkEx Runat="server" NavigateUrl='<%# UrlRoots.RetrieveProjectAdminUrl(ProjectId, "DataSyncProjects") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + ((DataSyncSystem) Container.DataItem).DataSyncSystemId%>' ID="lnkDataMapping" SkinID="ButtonDefault">
                        <span class="fas fa-cogs"></span>
                        <asp:Literal runat="server" Text="<%$Resources:Buttons,ViewProjectMappings %>" />
				    </tstsc:HyperLinkEx>
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
	    </Columns>													
    </tstsc:GridViewEx>
</asp:Content>
