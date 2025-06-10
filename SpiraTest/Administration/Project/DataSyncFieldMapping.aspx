<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="DataSyncFieldMapping.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.DataSyncFieldMapping" Title="Untitled Page" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DataSyncFieldMapping_Title %>" />
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
        <asp:Literal ID="ltrIntroText" runat="server" />
	</p>
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

    
    <ul class="u-box_list w-100 mt4">
        <li class="ma0 pa0">
			<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ArtifactTypeId %>" AppendColon="true" AssociatedControlID="lblArtifactTypeName"/>
			<tstsc:LabelEx ID="lblArtifactTypeName" runat="server" />
        </li>
        <li class="ma0 pa0">
			<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,FieldName %>" AppendColon="true" AssociatedControlID="lblArtifactFieldName"/>
			<tstsc:LabelEx ID="lblArtifactFieldName" runat="server" />
        </li>
        <li class="ma0 pa0 mt4">
		    <tstsc:GridViewEx ID="grdDataMappings" runat="server" ShowHeader="true" ShowSubHeader="false" AutoGenerateColumns="false" CssClass="DataGrid" Width="100%">
			    <HeaderStyle CssClass="Header" />
                <Columns>
                    <tstsc:BoundFieldEx DataField="ArtifactFieldValueName" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn%>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
				            <tstsc:LabelEx ID="lblActive" runat="server" Text='<%#GlobalFunctions.DisplayYnFlag(((DataSyncFieldValueMappingView) Container.DataItem).IsActive.Value)%>' />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                        <HeaderTemplate>
                            <asp:Localize runat="server" Text="<%$Resources:Fields,ExternalKey%>" />
                            <span class="badge">
                                <asp:Literal runat="server" Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncDisplayName) %>" />
                            </span>
                        </HeaderTemplate>
                        <ItemTemplate>
				            <tstsc:TextBoxEx ID="txtExternalKey" runat="server" MaxLength="255" Text='<%#((DataSyncFieldValueMappingView) Container.DataItem).ExternalKey%>' MetaData='<%#((DataSyncFieldValueMappingView) Container.DataItem).ArtifactFieldValue%>' Width="100%" />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Primary%>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                        <ItemTemplate>
				            <tstsc:CheckBoxYnEx ID="chkPrimary" runat="server" Checked='<%#(((DataSyncFieldValueMappingView) Container.DataItem).PrimaryYn.Equals("Y")) ? true : false %>' />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                </Columns>
            </tstsc:GridViewEx>
        </li>
        <li class="ma0 pa0 mt4">
		    <tstsc:ButtonEx id="btnUpdate" Runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="false" Authorized_Permission="ProjectAdmin" SkinID="ButtonPrimary"/>
	    </li>
    </ul>

</asp:Content>
