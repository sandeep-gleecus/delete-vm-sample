<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSyncCustomPropMapping.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.DataSyncCustomPropMapping" Title="Untitled Page" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">

    <h2>
		<asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncCustomPropertyMapping_Title %>" />
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
    <div class="btn-group priority2">
		<tstsc:HyperLinkEx ID="lnkProjectMappings" runat="server" SkinID="ButtonDefault" >
            <span class="fas fa-arrow-left"></span>
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncFieldMapping_BackToMappings %>" />
		</tstsc:HyperLinkEx>
    </div>

	<p class="my3">
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncCustomPropertyMapping_Intro1 %>" /> <tstsc:LabelEx ID="lblPlugInName" runat="server" Font-Bold="true" />
		<asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncCustomPropertyMapping_Intro2 %>" />
	</p>
	
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

    <ul class="u-box_list mw720 mt4">
        <li class="ma0 pa0 mb4">
            <tstsc:LabelEx ID="lblArtifactTypeNameLabel" AssociatedControlID="lblArtifactTypeName" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Fields,ArtifactTypeId %>" />
			<tstsc:LabelEx ID="lblArtifactTypeName" runat="server" />
        </li>
        <li class="ma0 pa0">
            <tstsc:LabelEx ID="lblCustomPropertyNameLabel" AssociatedControlID="lblCustomPropertyName" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Fields,Name %>" />
			<span>
                <tstsc:LabelEx ID="lblCustomPropertyName" runat="server" />
                <span class="badge">
                    <tstsc:LabelEx ID="lblCustomPropertyType" runat="server" />
                </span>
			</span>
        </li>
        <li class="ma0 pa0">
            <tstsc:LabelEx ID="txtExternalKeyLabel" AssociatedControlID="txtExternalKey" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Fields,ExternalKey %>" />
			<tstsc:UnityTextBoxEx CssClass="u-input is-active" ID="txtExternalKey" runat="server" MaxLength="255" />
        </li>
    </ul>
    <asp:PlaceHolder ID="plcCustomValueMapping" runat="server">
		<h3 class="mt5">
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DataSyncCustomPropertyMapping_CustomValueMapping %>" />
		</h3>
		<tstsc:GridViewEx ID="grdDataMappings" runat="server" ShowHeader="true" ShowSubHeader="false" AutoGenerateColumns="false" CssClass="DataGrid" Width="100%">
			<HeaderStyle CssClass="Header" />
            <Columns>
                <tstsc:BoundFieldEx HeaderText="<%$Resources:Main,Admin_System_CustomListValue_HeaderValueName %>" DataField="CustomPropertyValueName" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
                    <ItemTemplate>
				        <tstsc:LabelEx ID="lblActive" runat="server" Text='<%#GlobalFunctions.DisplayYnFlag(((DataSyncCustomPropertyValueMappingView) Container.DataItem).IsActive)%>'/>
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                    <HeaderTemplate>
                        <asp:Localize runat="server" Text="<%$Resources:Fields,ExternalKey %>" />
                        <span class="badge">
                            <asp:Literal runat="server" Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncDisplayName) %>" />
                        </span>
                    </HeaderTemplate>
                    <ItemTemplate>
				        <tstsc:TextBoxEx ID="txtExternalKey" runat="server" MaxLength="255" Width="98%" CssClass="text-box" Text='<%#((DataSyncCustomPropertyValueMappingView) Container.DataItem).ExternalKey%>' MetaData='<%#((DataSyncCustomPropertyValueMappingView) Container.DataItem).CustomPropertyValueId%>' />
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
            </Columns>
        </tstsc:GridViewEx>
    </asp:PlaceHolder>
    <div class="mt5">
		<tstsc:ButtonEx id="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Update %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" />
	</div>
</asp:Content>
