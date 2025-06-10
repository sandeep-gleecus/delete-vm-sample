<%@ Page Language="c#" ValidateRequest="false" CodeBehind="DataTools.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.Project.DataTools" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<asp:Panel ID="pnlProjectDataTools" runat="server">
        <h2>
            <asp:Label runat="server" Text="<%$ Resources:Main,Admin_DataTools_Title %>" />
            <small>
                <tstsc:HyperLinkEx 
                    ID="lnkAdminHome" 
                    runat="server" 
                    Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                    >
                    <asp:Label id="lblDataCachingProjectName" Runat="server" />
				</tstsc:HyperLinkEx>
            </small>
        </h2>

		<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />


        <p class="mw720 mt5">
		    <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_Intro1 %>" />
        </p>

		<div class="btn-group-vertical priority1">
			<tstsc:HyperLinkEx  
                Authorized_Permission="SystemAdmin" 
                CssClass="btn btn-default tl" 
                ID="lnkRefreshIndexes" 
                NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_RefreshDatabaseIndexes\")" %>' 
                runat="server" 
                ToolTip="<%$ Resources:Messages,Admin_DataTools_DatabaseIndexes_Tooltip %>"
                >
                <span class="fas fa-database fa-fw mr2"></span>
                <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_RefreshThe %>" />&nbsp;
                <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_DataTools_DatabaseIndexes %>" />
			</tstsc:HyperLinkEx>
						
			<tstsc:HyperLinkEx runat="server" CssClass="btn btn-default tl" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_RefreshFolderHieararchy\")" %>'>
                <span class="far fa-folder-open fa-fw mr2"></span>
                <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_RefreshThe %>" />&nbsp;
                <asp:Localize runat="server" Text="<%$ Resources:Messages,Admin_DataTools_FolderHieararchyCaches %>" />
			</tstsc:HyperLinkEx>

            <tstsc:HyperLinkEx  runat="server" CssClass="btn btn-default tl" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_RefreshTestStatusTaskProgress\")" %>' >
                <span class="fas fa-check fa-fw mr2"></span>
                <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_RefreshThe %>" />&nbsp;
                <asp:Localize runat="server" Text="<%$ Resources:Messages,Admin_DataTools_TestStatus_TaskProgress_Cache %>" />
			</tstsc:HyperLinkEx>

			<tstsc:HyperLinkEx runat="server" CssClass="btn btn-default tl" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_RefreshTestCaseParameters\")" %>'>
                <span class="fas fa-brackets-curly fa-fw mr2"></span>
                <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_RefreshThe %>" />&nbsp;
                <asp:Localize runat="server" Text="<%$ Resources:Messages,Admin_DataTools_TestCaseParameterHierarchyCache %>" />
			</tstsc:HyperLinkEx>

		</div>


        <p class="mw720 mt6">
		    <asp:Label runat="server" Text="<%$ Resources:Messages,Admin_DataTools_Intro2 %>" />
        </p>

        <div class="u-wrapper width_md">
            <div class="u-box_2">
                <ul class="u-box_list" >
                    <li class="ma0 pa0">
                        <tstsc:LabelEx CssClass="u-box_list_label" runat="server" Text="<%$ Resources:Main,SiteMap_Requirements %>" AppendColon="true"/>
                        <div class="btn-group">
                            <asp:Label runat="server" ID="lblCheckReq" CssClass="label-addon" />
                            <tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" Text="<%$ Resources:Buttons,Check %>" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_CheckIndentationRequirement\")" %>' />
                            <tstsc:HyperLinkEx ID="btnCorrectRequirements" runat="server" SkinID="ButtonDefault" Text="<%$ Resources:Messages,Admin_DataTools_Correct %>" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_IndentationRequirement\")" %>' />
                        </div>
                    </li>
                    <li class="ma0 pa0">
                        <tstsc:LabelEx CssClass="u-box_list_label" ID="Label1" runat="server" Text="<%$ Resources:Main,SiteMap_Releases %>" AppendColon="true"/>
                        <div class="btn-group">
                            <asp:Label runat="server" ID="lblCheckRel" CssClass="label-addon" />
                            <tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" Text="<%$ Resources:Buttons,Check %>" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_CheckIndentationRelease\")" %>' />
					        <tstsc:HyperLinkEx ID="btnCorrectReleases" runat="server" SkinID="ButtonDefault" Text="<%$ Resources:Messages,Admin_DataTools_Correct %>" NavigateUrl='<%# "javascript:grdDataTools_Execute(" + this.ProjectId + ", \"DataTool_IndentationRelease\")" %>' />
                        </div>
                    </li>
                </ul>
            </div>
        </div>
	</asp:Panel>


	<tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;
		function grdDataTools_Execute(projectId, procActivity) {
			var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

			//Actually start the background copy
			ajxBackgroundProcessManager.display(projectId, procActivity, resx.Admin_DataTools_Title, resx.Admin_DataTools_Processing, projectId);
		}
	    function ajxBackgroundProcessManager_success(msg, returnCode) {
	        //See what event was fired and handle appropriately
	        if (msg == 'datatool_checkindentationrequirement')
	        {
	            if (returnCode == 1)
	            {
	                //OK
	                $get('<%=lblCheckReq.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusGood)%>';
                    $('#<%=btnCorrectRequirements.ClientID%>').hide();
	            }
	            if (returnCode == 0)
	            {
                    //Error
	                $get('<%=lblCheckReq.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusBad)%>';
                    $('#<%=btnCorrectRequirements.ClientID%>').show();
	            }
	        }
	        else if (msg == 'datatool_indentationrequirement') {
	            if (returnCode == 1)
	            {
	                //OK
	                $get('<%=lblCheckReq.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusGood)%>';
                    $('#<%=btnCorrectRequirements.ClientID%>').hide();
	            }
	            if (returnCode == 0)
	            {
                    //Error
	                $get('<%=lblCheckReq.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusBad)%>';
	            }
	        }

	        else if (msg == 'datatool_checkindentationrelease')
	        {
	            if (returnCode == 1)
	            {
	                //OK
	                $get('<%=lblCheckRel.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusGood)%>';
                    $('#<%=btnCorrectReleases.ClientID%>').hide();
	            }
	            if (returnCode == 0)
	            {
                    //Error
	                $get('<%=lblCheckRel.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusBad)%>';
                    $('#<%=btnCorrectReleases.ClientID%>').show();
	            }
	        }
	        else if (msg == 'datatool_indentationrelease') {
	            if (returnCode == 1)
	            {
	                //OK
	                $get('<%=lblCheckRel.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusGood)%>';
                    $('#<%=btnCorrectReleases.ClientID%>').hide();
	            }
	            if (returnCode == 0)
	            {
                    //Error
	                $get('<%=lblCheckRel.ClientID%>').innerHTML = '<%=GlobalFunctions.JSEncode(Resources.Messages.Admin_DataTools_CurrentStatusBad)%>';
                    $('#<%=btnCorrectReleases.ClientID%>').show();
	            }
	        }
	        else
	        {
	            //Just display a completed message
	            globalFunctions.display_info_message($get('<%=lblMessage.ClientID%>'), resx.Admin_DataTools_Success);
	        }
        }

		$(document).ready(function () {
		    //Hide the correct buttons on first load
		    $('#<%=btnCorrectRequirements.ClientID%>').hide();
		    $('#<%=btnCorrectReleases.ClientID%>').hide();
		});
	</script>
</asp:Content>
