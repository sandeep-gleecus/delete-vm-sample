<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSyncProjects.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.DataSyncProjects" Title="Untitled Page" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">

    <h2>
		<tstsc:LabelEx ID="lblDataSyncName2" runat="server" />
        <span><asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncProjects_ProjectDataMapping %>" /></span>
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
    <div class="btn-group priority2 mb4">
		<tstsc:HyperLinkEx ID="lnkDataSyncHome" runat="server" SkinID="ButtonDefault" >
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncDetails_BackToHome %>" />
		</tstsc:HyperLinkEx>
    </div>

	
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />


	<asp:Panel ID="pnlProjectConfiguration" runat="server">
		<p class="mt3">
			<asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncProjects_Intro1%>" /> '<tstsc:LabelEx ID="lblDataSyncName" runat="server" Font-Bold="true" />'
			<asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncProjects_Intro2%>" />
		</p>


        <%-- PROPERTIES --%>
        <ul class="u-box_list w-50 w-100-sm my5" >
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ProjectName %>" ID="lblProjectName2Label" AssociatedControlID="lblProjectName2" Required="true" AppendColon="true" />                            
				<tstsc:LabelEx ID="lblProjectName2" runat="server" />
            </li>
            <li class="ma0 pa0 mb3">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ExternalKey %>" ID="txtProjectExtenalKeyLabel" AssociatedControlID="txtProjectExtenalKey" Required="true" AppendColon="true" />                            
				<tstsc:UnityTextBoxEx ID="txtProjectExtenalKey" runat="server" MaxLength="255" Width="300px" CssClass="u-input" />
            </li>
            <li class="ma0 pa0 mb4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ActiveYn %>" ID="chkActiveLabel" AssociatedControlID="chkActive" Required="true" AppendColon="true" />                            
				<tstsc:CheckBoxYnEx ID="chkActive" runat="server" />
            </li>
            <li class="ma0 pa0 mb2">
				<tstsc:ButtonEx id="btnUpdate" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" Authorized_Permission="ProjectAdmin" />
            </li>
        </ul>
   
	</asp:Panel>
            

	<asp:Panel ID="pnlDataMappings" runat="server">
		<h3>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncProjects_ArtifactFieldMapping%>" />
        </h3>
		<p class="my3">
            <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DataSyncProjects_ArtifactFieldMapping_Intro %>" />
		</p>
		<tstsc:GridViewEx ID="grdArtifacts" runat="server" ShowHeader="true" ShowSubHeader="false" AutoGenerateColumns="false" CssClass="DataGrid" Width="100%">
			<HeaderStyle CssClass="Header" />
            <Columns>
                <tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderText="<%$Resources:Fields,ArtifactTypeId %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                    <ItemTemplate>
                        <tstsc:ImageEx CssClass="w4 h4" runat="server" ImageUrl='<%# "Images/" + GlobalFunctions.GetIconForArtifactType(((Inflectra.SpiraTest.DataModel.ArtifactType) Container.DataItem).ArtifactTypeId) %>' AlternateText="<%#((Inflectra.SpiraTest.DataModel.ArtifactType) Container.DataItem).Name%>" />
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>             
                <tstsc:BoundFieldEx HeaderColumnSpan="-1" DataField="Name" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSyncProjects_StandardFields %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                    <ItemTemplate>
						<asp:Repeater ID="rptStandardFields" Runat="server">
							<ItemTemplate>
								<tstsc:HyperLinkEx ID="btnEditFieldMapping" Runat="server" NavigateUrl='<%# UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataSyncFieldMapping") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + this.dataSyncSystemId + "&" + GlobalFunctions.PARAMETER_ARTIFACT_FIELD_ID + "=" + ((Inflectra.SpiraTest.DataModel.ArtifactField) Container.DataItem).ArtifactFieldId%>'><%# ((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption%></tstsc:HyperLinkEx><br />
							</ItemTemplate>
                        </asp:Repeater>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSyncProjects_CustomProperties %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                    <ItemTemplate>
						<asp:Repeater ID="rptCustomProperties" Runat="server">
							<ItemTemplate>
								<tstsc:HyperLinkEx ID="btnEditCustomPropertyMapping" Runat="server" NavigateUrl='<%# UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataSyncCustomPropMapping") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + this.dataSyncSystemId + "&" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).ArtifactTypeId + "&" + GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_ID + "=" + ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).CustomPropertyId%>'><%#: ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).Name%></tstsc:HyperLinkEx><br />
							</ItemTemplate>
                        </asp:Repeater>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
            </Columns>							
		</tstsc:GridViewEx>
	</asp:Panel>
</asp:Content>
