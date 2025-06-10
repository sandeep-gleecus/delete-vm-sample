<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSynchronization.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.DataSynchronization" Title="Untitled Page" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-10">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DataSynchronization_Title %>" />
                </h2>
                <p class="my3">
                    <%=this.productName%> <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSynchronization_Intro1 %>" />
                </p>
                <p class="my3">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_DataSynchronization_Intro2 %>" />
                </p>
				<tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
					ShowSummary="True" ShowMessageBox="False" />
                <div class="TabControlHeader">
                    <div class="btn-group priority3" role="group">
                        <tstsc:LinkButtonEx ID="btnRefresh" runat="server">
                            <span class="fas fa-sync"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>"/>
                        </tstsc:LinkButtonEx>
                        <tstsc:DropMenu id="btnAdd" Runat="server" Text="<%$Resources:Buttons,Add %>" CausesValidation="false" GlyphIconCssClass="mr3 fas fa-plus" Authorized_Permission="SystemAdmin" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
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
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSynchronization_DataMapping %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			                <ItemTemplate>
                                <tstsc:DropDownHierarchy runat="server" ID="ddlProjectMappings" NoValueItem="true"
                                    AutoPostBack="false" DataTextField="Name" DataValueField="ProjectId" ItemImage="Images/org-Project-outline.svg" SummaryItemImage="Images/org-Project.svg"
                                />

				                <tstsc:HyperLinkEx Visible="false" Runat="server" NavigateUrl='<%# UrlRoots.RetrieveProjectAdminUrl(ProjectId, "DataSyncProjects") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + ((DataSyncSystem) Container.DataItem).DataSyncSystemId%>' ID="lnkDataMapping" SkinID="ButtonDefault">
                                    <span class="fas fa-cogs"></span>
                                    <asp:Literal runat="server" Text="<%$Resources:Buttons,ViewProjectMappings %>" />
				                </tstsc:HyperLinkEx>
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
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations%>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			                <ItemTemplate>
                                <div class="btn-group">
	    			                <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="ForceReSync" CommandArgument='<%# ((DataSyncSystem) Container.DataItem).DataSyncSystemId%>' ID="lnkForceResync" Confirmation="false" Authorized_Permission="SystemAdmin" ToolTip="<%$Resources:Main,DataSync_ResetSyncTooltip %>" >
                                        <span class="fas fa-sync"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,ResetSync %>"/>
	    			                </tstsc:LinkButtonEx>
		    		                <tstsc:HyperLinkEx runat="server" NavigateUrl='<%# "DataSyncDetails.aspx?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + ((DataSyncSystem) Container.DataItem).DataSyncSystemId%>' ID="lnlEdit" Authorized_Permission="SystemAdmin" SkinID="ButtonDefault">
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>"/>
		    		                </tstsc:HyperLinkEx>
			    	                <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="DeleteDataSync" CommandArgument='<%# ((DataSyncSystem) Container.DataItem) ["DataSyncSystemId"]%>' ID="lnkDelete" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_AutomationEngineDetails_DeleteConfirm %>" Authorized_Permission="SystemAdmin" >
                                        <span class="fas fa-trash-alt"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>"/>
			    	                </tstsc:LinkButtonEx>
				                    <tstsc:HyperLinkEx Runat="server" SkinID="ButtonDefault" NavigateUrl='<%#"EventLog.aspx?" + GlobalFunctions.PARAMETER_EVENT_CATEGORY + "=" + Logger.EVENT_CATEGORY_DATA_SYNCHRONIZATION %>' ID="lnkViewEvents" Authorized_Permission="SystemAdmin" >
                                        <span class="far fa-eye"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Main,DataSynchronization_ViewErrors %>"/>
				                    </tstsc:HyperLinkEx>
                                </div>
			                </ItemTemplate>
		                </tstsc:TemplateFieldEx>
	                </Columns>													
                </tstsc:GridViewEx>
			</div>
		</div>
	</div>
</asp:Content>
