<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.Edit" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h2>
		<tstsc:LabelEx ID="lblProjectName" runat="server" />
		<small>
			<asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_Title %>" />
		</small>
	</h2>
	<div class="btn-group priority1" role="group">
		<tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server">
            <span class="fas fa-arrow-left"></span>
            <asp:Localize ID="txtBackToList" runat="server" />
		</tstsc:HyperLinkEx>
	</div>

    <p>
	    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String1 %>" /><br />
	    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectDetails_String2 %>" />
    </p>

	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary 
        CssClass="ValidationMessage" 
        ShowMessageBox="False" 
        ShowSummary="True"
		DisplayMode="BulletList" 
        runat="server" 
        ID="ValidationSummary1" 
        />


    <section class="u-wrapper width_md">
        <div class="u-box_3">
            <%-- SUMMARY --%>
            <div 
                class="u-box_group u-cke_is-minimal"
                id="form-group_admin-product-edit-summary" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Fields,Description %>" />
                </div>
                <ul class="u-box_list" >
                    <li class="ma0 pa0 mt4">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ProjectName %>" Required="true" AppendColon="true"
							ID="txtNameLabel" AssociatedControlID="txtName" />
						<tstsc:UnityTextBoxEx CssClass="u-input" ID="txtName" runat="server" TextMode="SingleLine"
							MaxLength="50" Width="512" />
						<asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
							ErrorMessage="<%$Resources:Messages,Admin_ProjectDetails_ProjectNameRequired %>" ControlToValidate="txtName" />
                    </li>
                </ul>
                <ul class="u-box_list labels_absolute">
                    <li class="ma0 pa0 mt4">
						<tstsc:RichTextBoxJ ID="txtDescription" runat="server" />
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Description %>" Required="false" AppendColon="true"
							ID="txtDescriptionLabel" AssociatedControlID="txtDescription" />
                    </li>
                </ul>
            </div>
        </div>


        <div class="u-box_2 mt5">
            <%-- PROPERTIES --%>
            <div 
                class="u-box_group"
                id="form-group_admin-product-edit-properties" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Fields,Properties %>" />
                </div>
                <ul class="u-box_list" >
                    <li class="ma0 pa0 mb3">
						<tstsc:LabelEx 
                            AppendColon="true"
                            CssClass="u-box_list_label"
							ID="txtProjectTemplateLabel" 
                            Required="false" 
                            runat="server" 
                            Text="<%$Resources:Fields,Template %>" 
                            />
						<asp:Label runat="server" ID="txtProjectTemplate" CssClass="mr4" />
						<tstsc:ButtonEx
							ClientScriptMethod="btnChangeTemplate_click()"
							ID="btnChangeTemplate"
							runat="server"
							Text="<%$Resources:Buttons,Change %>"
							Visible="true" 
                            />
                    </li>
                    <li class="ma0 pa0 mb3">
						<tstsc:LabelEx 
                            runat="server" 
                            Text="<%$Resources:Fields,Program %>" 
                            Required="true" 
                            AppendColon="true"
							ID="ddlProjectGroupLabel" 
                            AssociatedControlID="ddlProjectGroup" 
                            />
						<tstsc:UnityDropDownListEx 
                            CssClass="u-dropdown" 
							DataMember="ProjectGroup" 
                            DataTextField="Name" 
                            DataValueField="ProjectGroupId" 
                            DisabledCssClass="u-dropdown disabled"
                            ID="ddlProjectGroup" 
                            runat="server"
                            />
                    </li>
                    <li class="ma0 pa0 mb3">
						<tstsc:LabelEx 
                            AppendColon="true"
                            AssociatedControlID="txtWebSite" 
							ID="txtWebSiteLabel" 
                            Required="false" 
                            runat="server" 
                            Text="<%$Resources:Fields,WebSite %>" 
                            />
						<tstsc:UnityTextBoxEx 
                            CssClass="u-input" 
                            ID="txtWebSite" 
                            MaxLength="255" 
                            runat="server"
							TextMode="SingleLine" 
                            Width="512" 
                            />
						<asp:RegularExpressionValidator ID="vldWebSite" runat="server" ControlToValidate="txtWebSite"
							ErrorMessage="<%$Resources:Messages,Admin_ProjectDetails_WebSiteUrlInvalid %>" Text="*" ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_URL%>" />
                    </li>
                    <li class="ma0 pa0 mb3" id="grpBaseline" runat="server">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_ProjectEdit_BaseliningEnabled %>" AppendColon="true" AssociatedControlID="chkBaseline" />
						<tstsc:CheckBoxYnEx runat="server" ID="chkBaseline" />
                    </li>
                    <li class="ma0 pa0 mb3" id="Li1" runat="server">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_ProjectEdit_SearchNameAndDescription %>" AppendColon="true" AssociatedControlID="chkSearchNameAndDescription" />
						<tstsc:CheckBoxYnEx runat="server" ID="chkSearchNameAndDescription" />
                    </li>
                    <li class="ma0 pa0 mb5">
						<tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ActiveYn %>" Required="true" AppendColon="true"
							ID="ddlActiveYnLabel" AssociatedControlID="chkActiveYn" />
						<tstsc:CheckBoxYnEx runat="server" ID="chkActiveYn" />
                    </li>
                    <li class="ma0 pa0">
		                <div class="btn-group ml_u-box-label">
			                <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
			                <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
		                </div>
                    </li>
                </ul>
            </div>
        </div>
    </section>

	<div class="form-group row">
	</div>


	<tstsc:DialogBoxPanel
		ID="dlgChangeTemplate"
		runat="server"
		Title="<%$Resources:Dialogs,Project_ChangeTemplate %>"
		Modal="true"
		Width="600px">
		<div id="dlgChangeTemplate-confirmNewTemplate" class="ma4">
			<p class="mb4">
				<asp:Localize runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Intro %>" />
			</p>
			<div class="alert alert-warning mb5">
				<asp:Literal ID="msgWarning1" runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Warning1 %>" />
			</div>
			<div class="flex items-center">
				<tstsc:LabelEx runat="server" Required="true" Text="<%$Resources:Fields,ProjectTemplate %>" AssociatedControlID="ddlTemplates" AppendColon="true" CssClass="mr4" />
				<tstsc:DropDownListEx
					runat="server"
					ID="ddlTemplates"
					DataValueField="ProjectTemplateId"
					DataTextField="Name"
					NoValueItem="true"
					NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>"
					SkinID="WideControl" />
			</div>
			<div class="btn-group mt5">
				<tstsc:ButtonEx runat="server" Text="<%$Resources:Buttons,Cancel%>" ID="btnCancelChange" ClientScriptMethod="btnCancelChange_click()" />
				<tstsc:ButtonEx runat="server" Text="<%$Resources:Buttons,Next%>" ID="btnConfirmNewTemplate" SkinID="ButtonPrimary" ClientScriptMethod="btnConfirmNewTemplate_click()" />
			</div>
		</div>

		<div id="dlgChangeTemplate-submit" class="dn ma4">
			<p class="fw-b mt0 mb4">
				<asp:Literal runat="server" Text="<%$Resources:Fields,ProjectName %>" />:
				<asp:Label runat="server" ID="dlgChangeTemplateProductName" ClientIDMode="static" />
			</p>
			<div id="grid-fieldMappings"></div>
			<div class="alert alert-danger mb5">
				<asp:Label CssClass="dn" ID="msgWarningStandardFieldsOk" ClientIDMode="static" runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Warning_StandardFieldsOk %>" />
				<asp:Label CssClass="dn" ID="msgWarningStandardFieldsNotOk" ClientIDMode="static" runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Warning_StandardFieldsNotOk %>" />
				<br />
				<br />
				<asp:Literal ID="msgWarrningCatchAll" runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Warning_CatchAll %>" />
				<br />
				<br />
				<asp:Literal ID="Literal1" runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_Warning_HowLong %>" />
			</div>
			<div class="form-group">
				<asp:Label runat="server" Text="<%$Resources:Dialogs,Project_ChangeTemplate_TypeConfirm %>" ID="lblChangeTemplateTypingCheck" />
				<input id="txtChangeTemplateTypingCheck" class="form-control" type="text" />
			</div>
			<div class="btn-group mt4">
				<tstsc:ButtonEx runat="server" Text="<%$Resources:Buttons,Cancel%>" ID="btnCancelChange3" ClientScriptMethod="btnCancelChange_click()" />
				<tstsc:ButtonEx runat="server" ClientIDMode="Static" Enabled="false" Text="<%$Resources:Buttons,Change%>" ID="btnSubmitChange" SkinID="ButtonPrimary" ClientScriptMethod="btnSubmitChange_click()" />
			</div>
		</div>
	</tstsc:DialogBoxPanel>

	<tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/ProjectTemplateService.svc" />
		</Services>
		<Scripts>
			<asp:ScriptReference Path="~/TypeScript/rct_comp_grid.js" />
		</Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
		var dlgChangeTemplateDecisions = {
			confirmTemplate: false,
			typingCheck: false
		};

		// trim spaces from before and after the name, and also convert multiple spaces between words to a single space
		var dlgChangeTemplateProductNameTrimmed = globalFunctions.trim(document.getElementById("dlgChangeTemplateProductName").innerText).replace(/\s+/g, " ");
		var txtChangeTemplateTypeCheck = document.getElementById("txtChangeTemplateTypingCheck");
		txtChangeTemplateTypeCheck.addEventListener("input", function () { btnSubmitEnableCheck() });
		function btnSubmitEnableCheck() {
			//have to check for a match with a trimmed and multiple spaces replaced with one because the raw html of the name is the true text, but the browser can render something else :(
			var isMatch = globalFunctions.trim(txtChangeTemplateTypeCheck.value).replace(/\s+/g, " ") == dlgChangeTemplateProductNameTrimmed;
			dlgChangeTemplateDecisions.typingCheck = isMatch;
			document.getElementById("btnSubmitChange").disabled = (dlgChangeTemplateDecisions.typingCheck && dlgChangeTemplateDecisions.confirmTemplate) ? "" : "true";
		}

		// when product grid is interacted with kick off the process of changing template
		function btnChangeTemplate_click() {
			//first reset any previous relevant state to make sure it is clear
			dlgChangeTemplateReset();

			//Display the dialog box
			var dlgChangeTemplate = $find('<%=dlgChangeTemplate.ClientID%>');
			dlgChangeTemplate.display();
		}

		// when click 'next' on the first page of the change template popup
		function btnConfirmNewTemplate_click() {
			//Move to the warnings page of the dialog
			var ddlTemplates = $find('<%=ddlTemplates.ClientID%>');
			var newTemplateId = ddlTemplates.get_selectedItem().get_value();
			var projectId = <%=ProjectId%>;
			// only progress if a template has been selected
			if (newTemplateId && newTemplateId != '') {
				Inflectra.SpiraTest.Web.Services.Ajax.ProjectTemplateService.RetrieveStandardFieldMappingInformation(projectId, newTemplateId, showFinalWarnings);

			} else {
				alert(resx.Administration_Project_NeedToSelectTemplate);
				return;
			}
		}

		function showFinalWarnings(data) {
			document.getElementById("dlgChangeTemplate-confirmNewTemplate").classList.add("dn");
			document.getElementById("dlgChangeTemplate-submit").classList.remove("dn");
			dlgChangeTemplateDecisions.confirmTemplate = true;

			if (data && data.length) {
				//these keys to this object match those sent from server
				var tableMeta = [
					{ ref: "artifactType", name: Inflectra.SpiraTest.Web.GlobalResources.Global_Artifact },
					{ ref: "artifactField", name: Inflectra.SpiraTest.Web.GlobalResources.Global_FieldName },
					{ ref: "affectedCount", name: Inflectra.SpiraTest.Web.GlobalResources.Admin_TemplateChange_FieldMappingLoss }
				];

				//render the table of data
				ReactDOM.render(React.createElement(ReactGrid,
					{
						data: data,
						meta: tableMeta
					}), document.getElementById('grid-fieldMappings')
				);
				//show the correct part of the warning message
				document.getElementById("msgWarningStandardFieldsNotOk").classList.remove("dn");
			} else {
				document.getElementById("msgWarningStandardFieldsOk").classList.remove("dn");
			}
		}

		function btnSubmitChange_click() {
			//Validate template is chosen
			var ddlTemplates = $find('<%=ddlTemplates.ClientID%>');
			var newTemplateId = ddlTemplates.get_selectedItem().get_value();
			if (!newTemplateId || newTemplateId == '') {
				alert(resx.Administration_Project_NeedToSelectTemplate);
				return;
			}

			if (dlgChangeTemplateDecisions.confirmTemplate && dlgChangeTemplateDecisions.typingCheck) {
				var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
				var backgroundProcess = 'Project_RemapTemplate';

				//start the background copy
				var projectId = <%=ProjectId%>;
				ajxBackgroundProcessManager.display(projectId, backgroundProcess, resx.Administration_Project_RemapTemplateTitle, resx.Administration_Project_RemapTemplateDialogText, newTemplateId);

				//close and reset the dialog
				btnCancelChange_click();
			}
		}

		function btnCancelChange_click() {
			var dlgChangeTemplate = $find('<%=dlgChangeTemplate.ClientID%>');
			dlgChangeTemplate.close();

			dlgChangeTemplateReset();
		}

		function dlgChangeTemplateReset() {
			//reset the dialog and state
			document.getElementById("dlgChangeTemplate-confirmNewTemplate").classList.remove("dn");
			document.getElementById("dlgChangeTemplate-submit").classList.add("dn");
			document.getElementById("msgWarningStandardFieldsOk").classList.add("dn");
			document.getElementById("msgWarningStandardFieldsNotOk").classList.add("dn");
			dlgChangeTemplateDecisions.confirmTemplate = false;
			dlgChangeTemplateDecisions.typingCheck = false;
			txtChangeTemplateTypeCheck.value = "";
			document.getElementById("btnSubmitChange").disabled = "true";

			//reset the dropdown
			var ddlTemplates = $find('<%=ddlTemplates.ClientID%>');
			ddlTemplates.set_selectedItem(false, false);

			//unmount react
			ReactDOM.unmountComponentAtNode(document.getElementById('grid-fieldMappings'));
		}

		function ajxBackgroundProcessManager_success(msg) {
			//Simply reload the page (since it will have the new project)
			window.location.reload();
		}

    </script>
</asp:Content>
