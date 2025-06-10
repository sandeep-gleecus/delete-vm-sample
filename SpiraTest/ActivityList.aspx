<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ActivityList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.ActivityList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="container">
        <div class="main-panel">
            <div class="main-content row my4">
                <div class="col-sm-12">
                    <div class="btn-toolbar toolbar" role="toolbar">
                        <div class="btn-group priority1" role="group">
		                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkProjectHome" runat="server">
                                <span class="fas fa-arrow-left"></span>
                                <asp:Localize runat="server" id="lnkProjectHomeText" />
		                    </tstsc:HyperLinkEx>
                        </div>
                        <div class="btn-group priority2" role="group">
                            <tstsc:DropMenu id="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync" Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdHistory" ClientScriptMethod="load_data()" />
            				<tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
					            MenuWidth="140px" ClientScriptServerControlId="grdHistory" ClientScriptMethod="apply_filters()">
					            <DropMenuItems>
						            <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						            <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
					            </DropMenuItems>
				            </tstsc:DropMenu>   
                        </div>  
                    </div>                   
			        <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
		            <div class="alert alert-warning alert-narrow">
                        <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
				        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
				        <asp:Label ID="lblCount" runat="server" Font-Bold="True" />
				        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
				        <asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
				        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Items %>" />.
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                    </div>
				    <tstsc:SortedGrid ID="grdHistory" runat="server" EnableViewState="false" CssClass="DataGrid DataGrid-no-bands"
					    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryActivityService"
					    VisibleCountControlId="lblCount" TotalCountControlId="lblTotal" HeaderCssClass="Header"
                        SubHeaderCssClass="SubHeader" ErrorMessageControlId="lblMessage" FilterInfoControlId="lblFilterInfo"
					    RowCssClass="Normal" DisplayAttachments="false" AllowEditing="false">
				    </tstsc:SortedGrid>
                    <br />
                    <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                </div>
            </div>
        </div>
    </div>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Services>  
			<asp:ServiceReference Path="~/Services/Ajax/HistoryActivityService.svc" />
       </Services>  
    </tstsc:ScriptManagerProxyEx>
</asp:Content>
