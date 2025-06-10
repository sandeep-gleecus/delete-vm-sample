<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="ProjectList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectList" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h1>
		<asp:Localize runat="server" Text="<%$Resources:Main,Admin_EditProjects_Title %>" />
	</h1>
	<p>
		<asp:Localize runat="server" Text="<%$Resources:Main,Admin_EditProjects_String1 %>" /><br />
		<asp:Localize runat="server" Text="<%$Resources:Main,Admin_EditProjects_String2 %>" />
	</p>
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage" DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />

	<div class="toolbar btn-toolbar-mid-page mt5">
		<div class="btn-group priority2" role="group">
			<tstsc:DropMenu ID="btnProjectListAdd" GlyphIconCssClass="fas fa-plus mr2" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Add %>" Authorized_ArtifactType="None" Authorized_Permission="SystemAdmin" />
		</div>
		<div class="btn-group priority3">
			<tstsc:DropMenu ID="btnProjectListFilter" runat="server" GlyphIconCssClass="fas fa-filter mr2" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
			<tstsc:DropMenu ID="btnProjectListClearFilters" runat="server" GlyphIconCssClass="fas fa-times mr2" CausesValidation="False" Text="<%$Resources:Buttons,ClearFilter %>" />
		</div>
	</div>

	<tstsc:GridViewEx ID="grdProjectManagement" CssClass="DataGrid table" runat="server" PageSize="15" AllowPaging="true" ShowSubHeader="True" Width="100%" AutoGenerateColumns="False">
		<SubHeaderStyle CssClass="SubHeader" />
		<HeaderStyle CssClass="Header" />
		<SelectedRowStyle CssClass="Selected" />
		<Columns>
			<tstsc:TemplateFieldEx ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1">
				<HeaderTemplate>
					<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,ProjectName %>" />
					<tstsc:ImageButtonEx ID="btnProjectNameSortUp" runat="server" CommandName="SortColumns" CausesValidation="False" CommandArgument="Name ASC" AlternateText="<%$Resources:ServerControls,GridViewEx_SortByAscending %>" ToolTip="<%$Resources:ServerControls,GridViewEx_SortByAscending %>" ImageUrl="Images/SortAscending.gif" HoverImageUrl="Images/SortAscendingHover.gif" SelectedImageUrl="Images/SortAscendingSelected.gif" Selected='<%# this.grdProjectManagement.SortExpression == "Name ASC"%>' />
					<tstsc:ImageButtonEx ID="btnProjectNameSortDown" runat="server" CommandName="SortColumns" CausesValidation="False" CommandArgument="Name DESC" AlternateText="<%$Resources:ServerControls,GridViewEx_SortByDescending %>" ToolTip="<%$Resources:ServerControls,GridViewEx_SortByDescending %>" ImageUrl="Images/SortDescending.gif" HoverImageUrl="Images/SortDescendingHover.gif" SelectedImageUrl="Images/SortDescendingSelected.gif" Selected='<%# this.grdProjectManagement.SortExpression == "Name DESC"%>' />
				</HeaderTemplate>
				<SubHeaderTemplate>
					<tstsc:TextBoxEx CssClass="text-box" ID="txtProjectNameFilter" runat="server" TextMode="SingleLine" MetaData="Name" />
				</SubHeaderTemplate>
				<ItemTemplate>
					<tstsc:ImageEx runat="server" ImageUrl="Images/org-Project-outline.svg" AlternateText="<%$Resources:Fields,Project %>" Width="16px" />
					<tstsc:HyperLinkEx runat="server" ToolTip='<%# "<u>" + ((ProjectView)Container.DataItem).Name + "</u><br />" + ((ProjectView) Container.DataItem).Description%>' NavigateUrl='<%# UrlRewriterModule.RetrieveProjectAdminUrl(((ProjectView) Container.DataItem).ProjectId, "Default")%>'>
							<%#: ((ProjectView)Container.DataItem).Name%>
					</tstsc:HyperLinkEx>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,Program %>" FilterField="ProjectGroupId" DataField="ProjectGroupName" Sortable="true" FilterType="DropDownList" FilterLookupDataField="ProjectGroupId" FilterLookupTextField="Name" FilterWidth="150px" ItemStyle-CssClass="priority1"
				HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" CommandArgumentField="ProjectGroupId" />
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,CreationDate %>" FilterField="CreationDate" DataField="CreationDate" Sortable="true" FilterType="DateControl" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" FooterStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority3" />
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,Template %>" FilterField="ProjectTemplateId" DataField="ProjectTemplateName" CommandArgumentField="ProjectTemplateId" Sortable="true" FilterType="DropDownList" FilterLookupDataField="ProjectTemplateId" FilterLookupTextField="Name" FilterWidth="150px" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" FooterStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2" />
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" FilterField="IsActive" DataField="IsActive" Sortable="true" FilterType="Flag" FilterLookupDataField="Key" ItemStyle-Width="80px" FilterLookupTextField="Value" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" FooterStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" />
			<tstsc:FilterSortFieldEx HeaderText="<%$Resources:Fields,ID %>" FilterField="ProjectId" DataField="ProjectId" Sortable="true" FilterType="TextBox" FilterWidth="40px" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" FooterStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2" />
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-Wrap="false" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" FooterStyle-CssClass="priority4" SubHeaderStyle-CssClass="priority4">
				<ItemTemplate>
					<div class="btn-group priority1">
						<tstsc:LinkButtonEx runat="server" CausesValidation="false" CommandName="Select" CommandArgument='<%# ((ProjectView) Container.DataItem).ProjectId%>'>
                            <span class="far fa-eye"></span>
                            <asp:Localize runat="server" Text="<%$ Resources:Buttons,View %>"/>
						</tstsc:LinkButtonEx>
						<tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" CausesValidation="false" NavigateUrl='<%# UrlRewriterModule.RetrieveProjectAdminUrl(((ProjectView) Container.DataItem).ProjectId, "Edit")%>'>
                            <span class="far fa-edit"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
						</tstsc:HyperLinkEx>
						<tstsc:HyperLinkEx
							runat="server"
							SkinID="ButtonDefault"
							Authorized_Permission="SystemAdmin"
							Confirmation="true"
							NavigateUrl='<%# "javascript:grdProjectManagement_cloneProjectConfirm(" + ((ProjectView) Container.DataItem).ProjectId + ")" %>'>
                            <span class="far fa-clone"></span>
                            <asp:Localize runat="server" Text="<%$ Resources:Buttons,Clone %>"/>
						</tstsc:HyperLinkEx>
						<tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" Authorized_Permission="SystemAdmin" Confirmation="true" NavigateUrl='<%# "javascript:grdProjectManagement_deleteProjectDialog(" + ((ProjectView) Container.DataItem).ProjectId + ", \"" + ((ProjectView) Container.DataItem).Name + "\")" %>'>
                            <span class="fas fa-trash-alt"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
						</tstsc:HyperLinkEx>
					</div>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>


	<tstsc:DialogBoxPanel
		ID="dlgCopyProductOperation"
		runat="server"
		SkinID="PopupOverflowVisible"
		Modal="true"
		Title="<%$ Resources:Buttons,Clone %>">
		<p class="mx4 mt4 fs-h5">
			<asp:Literal runat="server" Text="<%$ Resources:ClientScript,Administration_CopyProjectConfirm %>" />
		</p>
		<div class="mt3 mx4">
			<input
				type="radio"
				name="cloneProductOption"
				value="fullClone"
				checked
				id="dlgCopyProductOperation_fullClone" />
			<label for="dlgCopyProductOperation_fullClone">
				<asp:Literal runat="server" Text="<%$ Resources:Dialogs,Admin_ProductList_CloneProduct %>" />
			</label>
		</div>
		<div class="my0 mx4">
			<input
				type="radio"
				name="cloneProductOption"
				value="cloneAndReset"
				id="dlgCopyProductOperation_cloneAndReset" />
			<label for="dlgCopyProductOperation_cloneAndReset">
				<asp:Literal runat="server" Text="<%$ Resources:Dialogs,Admin_ProductList_CloneResetProduct %>" />
			</label>
		</div>
		<div class="my4 mx4">
			<input
				type="checkbox"
				id="dlgCopyProductOperation_templateToo" />
			<label for="dlgCopyProductOperation_templateToo">
				<asp:Literal runat="server" Text="<%$ Resources:Dialogs,Admin_ProductList_CloneTemplate %>" />
			</label>
		</div>

		<p class="alert alert-info w10 mx4 mb5">
			<asp:Literal runat="server" Text="<%$ Resources:Dialogs,Admin_ProductList_CloneInformation %>" />
		</p>

		<div class="btn-group ml4">
			<tstsc:ButtonEx
				ID="btnOperation"
				runat="server"
				SkinID="ButtonPrimary"
				ClientScriptMethod="grdProjectManagement_cloneProject()"
				Text="<%$Resources:Buttons,Clone %>" />
			<tstsc:ButtonEx
				ID="btnCancel"
				runat="server"
				ClientScriptMethod="dlgCopyProductOperation_close()"
				Text="<%$Resources:Buttons,Cancel %>" />
		</div>

	</tstsc:DialogBoxPanel>

	<tstsc:DialogBoxPanel
		ID="dlgDeleteOperation"
		runat="server"
		SkinID="PopupOverflowVisible"
		Modal="true"
		Title="<%$ Resources:Buttons,Delete %>">
		<p class="fw-b mt0 mb4">
			<asp:Literal runat="server" Text="<%$Resources:Fields,ProjectName %>" />:
			<asp:Label ClientIDMode="static" runat="server" ID="dlgDeleteProductName" />
		</p>
		<div class="alert alert-danger mb5">
			<asp:Literal
				ID="msgDeleteProject"
				runat="server"
				Text="<%$Resources:ClientScript,Administration_DeleteProjectConfirm %>" />
			<asp:Literal
				ID="ltlDeleteHasTV"
				runat="server"
				Visible="false"
				Text="<%$Resources:Dialogs,Administration_DeleteProjectConfirmTV %>" />
		</div>
		<div class="form-group">
			<asp:Label runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_TypeConfirm %>" ID="lblDeleteProjectTypingCheck" />
			<input id="txtDeleteProjectTypingCheck" class="form-control" type="text" />
		</div>
		<div class="btn-group mt4">
			<tstsc:ButtonEx
				runat="server"
				Text="<%$Resources:Buttons,Cancel%>"
				ID="btnCancelDelete"
				ClientScriptMethod="dlgDeleteOperation_close()" />
			<tstsc:ButtonEx
				runat="server"
				ClientIDMode="Static"
				Enabled="false"
				Text="<%$Resources:Buttons,Delete%>"
				ID="btnDeleteProductConfirm"
				SkinID="ButtonPrimary"
				ClientScriptMethod="grdProjectManagement_deleteProject()" />
		</div>

	</tstsc:DialogBoxPanel>


	<asp:ObjectDataSource ID="srcActiveFlag" runat="server" SelectMethod="RetrieveFlagLookup" TypeName="Inflectra.SpiraTest.Business.ProjectManager" />
	<asp:ObjectDataSource ID="srcProjectGroups" runat="server" SelectMethod="RetrieveActive" TypeName="Inflectra.SpiraTest.Business.ProjectGroupManager" />
	<asp:ObjectDataSource ID="srcProjectTemplates" runat="server" SelectMethod="RetrieveActive" TypeName="Inflectra.SpiraTest.Business.TemplateManager" />

	<tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;

		var grdProjectManagement_productIdToClone = 0;
		var grdProjectManagement_productIdToDelete = 0;
		var grdProjectManagement_productNameToDelete = "";
		var grdProjectManagement_productDeleteTypeCheck = false;
        var currentProductId = <%=ProjectId %>;

        var dlgDeleteProductNameTrimmed = "";

        var txtDeleteProjectTypingCheck = document.getElementById("txtDeleteProjectTypingCheck");
		txtDeleteProjectTypingCheck.addEventListener("input", function () { btnDeleteEnableCheck() });
		function btnDeleteEnableCheck() {
			//have to check for a match with a trimmed and multiple spaces replaced with one because the raw html of the name is the true text, but the browser can render something else :(
			var isMatch = globalFunctions.trim(txtDeleteProjectTypingCheck.value).replace(/\s+/g, " ") == dlgDeleteProductNameTrimmed;
			grdProjectManagement_productDeleteTypeCheck = isMatch;
			document.getElementById("btnDeleteProductConfirm").disabled = grdProjectManagement_productDeleteTypeCheck ? "" : "true";
		}

		function grdProjectManagement_cloneProjectConfirm(projectId) {
			grdProjectManagement_productIdToClone = projectId;

			var dlgCopyProductOperation = $find('<%=this.dlgCopyProductOperation.ClientID%>');
			dlgCopyProductOperation.display();
		}

		function grdProjectManagement_cloneProject() {
			//Make sure we have a product id set
			if (grdProjectManagement_productIdToClone) {
				var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
				var fullClone = document.getElementById("dlgCopyProductOperation_fullClone").checked;
				var cloneAndReset = document.getElementById("dlgCopyProductOperation_cloneAndReset").checked;
				var cloneTemplate = document.getElementById("dlgCopyProductOperation_templateToo").checked;


				var backgroundProcess = "";
				if (fullClone && !cloneTemplate) {
					backgroundProcess = "project_copy";
				} else if (fullClone && cloneTemplate) {
					backgroundProcess = "project_copy_template_copy";
				} else if (cloneAndReset && !cloneTemplate) {
					backgroundProcess = "project_copyreset";
				} else if (cloneAndReset && cloneTemplate) {
					backgroundProcess = "project_copyreset_template_copy";
				}
				console.log(backgroundProcess);

				//start the background copy
				ajxBackgroundProcessManager.display(null, backgroundProcess, resx.Administration_CopyProjectDialogTitle, resx.Administration_CopyProjectDialogText, grdProjectManagement_productIdToClone);

				//close and reset the dialog
				dlgCopyProductOperation_close();


			}
		}

		function dlgCopyProductOperation_close() {
			//Close the dialog box
			var dlgCopyProductOperation = $find('<%=this.dlgCopyProductOperation.ClientID%>');
			dlgCopyProductOperation.close();

			// clear out the saved project id and reset the radio buttons
			grdProjectManagement_productIdToClone = 0;
			document.getElementById("dlgCopyProductOperation_productOnly").checked = true;
		}

		function grdProjectManagement_deleteProjectDialog(projectId, projectName) {
			//first reset any state in the dialog - ie checks that it makes
			dlgDeleteOperation_reset();

			grdProjectManagement_productIdToDelete = projectId;
			grdProjectManagement_productNameToDelete = projectName;
            document.getElementById("dlgDeleteProductName").innerText = projectName;
            // we only know this value once the delete button has been clicked for that row - we can't set it on page load
			// trim spaces from before and after the name, and also convert multiple spaces between words to a single space
			dlgDeleteProductNameTrimmed = globalFunctions.trim(projectName).replace(/\s+/g, " ");

			var dlgDeleteOperation = $find('<%=this.dlgDeleteOperation.ClientID%>');
			dlgDeleteOperation.display();
		}

		function grdProjectManagement_deleteProject() {
			var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

			//Make sure that the user wants to delete the project
			if (grdProjectManagement_productDeleteTypeCheck) {
				//Actually start the background delete
				if (currentProductId == -1) {
					currentProductId = null;
				}
				ajxBackgroundProcessManager.display(currentProductId, 'Project_Delete', resx.Administration_DeleteProjectDialogTitle, resx.Administration_DeleteProjectDialogText, grdProjectManagement_productIdToDelete);

				//close and reset the dialog
				dlgDeleteOperation_close();
			}
		}

		function dlgDeleteOperation_close() {
			//Close the dialog box
			var dlgDeleteOperation = $find('<%=this.dlgDeleteOperation.ClientID%>');
			dlgDeleteOperation.close();
			dlgDeleteOperation_reset();

		}
		function dlgDeleteOperation_reset() {
			// clear out the saved project id and reset the radio buttons
            grdProjectManagement_productIdToDelete = 0;
            dlgDeleteProductNameTrimmed = "";
			document.getElementById("txtDeleteProjectTypingCheck").value = "";
			document.getElementById("btnDeleteProductConfirm").disabled = "true";
		}

		function ajxBackgroundProcessManager_success(msg) {
			//Simply reload the page (since it will have the new project)
			window.location.reload();
		}

		//Add the event handlers
		$(document).ready(function () {
			$('#<%=grdProjectManagement.ClientID%>').on("keydown", function (evt) {
				var keynum = evt.keyCode | evt.which;
				if (keynum == 13) {
					//Click on the button inside the DIV
					$('#<%=btnProjectListFilter.ClientID%> button').trigger('click');
				}
			});
		});
    </script>
</asp:Content>
