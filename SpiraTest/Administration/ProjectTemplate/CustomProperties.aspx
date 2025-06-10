<%@ Page
	Title=""
	Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="CustomProperties.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.CustomProperties" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
		<div class="row">
			<div class="col-lg-9">
				<h2 class="mb5">
					<tstsc:LabelEx runat="server" ID="lblTitle" Text="<%$ Resources:Main,CustomProperties_Title %>" />
					<small>
						<tstsc:HyperLinkEx
							ID="lnkAdminHome"
							runat="server"
							title="<%$Resources:Main,Admin_Project_BackToHome %>">
							<asp:Literal ID="ltrProjectName" runat="server" />
						</tstsc:HyperLinkEx>
					</small>
				</h2>
			</div>
		</div>
		<div class="row">
			<div class="col-lg-6 col-md-9">
				<p>
					<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_Intro %>" />
				</p>
				<tstsc:MessageBox runat="server" SkinID="MessageBox" ID="msgBox" />
				<div class="Spacer"></div>
				<tstsc:GridViewEx ID="grdCustomProperties" runat="server" AutoGenerateColumns="False" CssClass="DataGrid" SubHeaderStyle-CssClass="Header" Width="100%" ShowHeader="true" ShowSubHeader="true">
					<Columns>
						<tstsc:TemplateFieldEx SubHeaderText="<%$Resources:Fields,CustomProperty_FieldNumber %>" HeaderColumnSpan="-1" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" SubHeaderStyle-CssClass="priority4">
							<ItemTemplate>
								<tstsc:LabelEx runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).PropertyNumber %>' ID="lblPropertyNumber" />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx SubHeaderText="<%$Resources:Fields,CustomProperty_PropertyName %>" HeaderColumnSpan="5" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
							<ItemTemplate>
								<tstsc:LabelEx ID="lblName" MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).CustomPropertyId %>' runat="server" Text='<%#: ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).Name %>' />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx SubHeaderText="<%$Resources:Fields,Type %>" HeaderColumnSpan="-1" ItemStyle-CssClass="priority2" SubHeaderStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
							<ItemTemplate>
								<tstsc:LabelEx ID="lblCustomPropertyType" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).CustomPropertyTypeName %>' />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx SubHeaderText="<%$Resources:Fields,CustomProperty_LegacyName %>" ItemStyle-CssClass="Disabled" HeaderColumnSpan="-1">
							<ItemTemplate>
								<tstsc:LabelEx ID="lblLegacyName" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).LegacyName %>' />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx SubHeaderText="<%$ Resources:Fields,Position %>" HeaderColumnSpan="-1">
							<ItemTemplate>
								<tstsc:LabelEx ID="lblPosition" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).Position %>' />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx SubHeaderText="<%$ Resources:Main,Admin_System_CustomList_HeaderActions %>" HeaderColumnSpan="-1" ItemStyle-VerticalAlign="Middle" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
							<ItemTemplate>
								<asp:PlaceHolder ID="pcAdd" runat="server" Visible="<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).CustomPropertyId <= 0 %>">
									<tstsc:HyperLinkEx ID="HyperLinkEx2" SkinID="ButtonDefault" runat="server" ClientScriptMethod='<%# "onDisplayProp(" + ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).PropertyNumber + ")" %>' NavigateUrl="javascript:void(0)">
											<span class="fas fa-plus"></span>
											<asp:Localize runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_AddDefinition %>" />
									</tstsc:HyperLinkEx>
								</asp:PlaceHolder>
								<asp:PlaceHolder ID="pcEditRemove" runat="server" Visible="<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).CustomPropertyId > 0 %>">
									<div class="btn-group">
										<tstsc:HyperLinkEx ID="HyperLinkEx1" SkinID="ButtonDefault" runat="server" ClientScriptMethod='<%# "onDisplayProp(" + ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).PropertyNumber + ")" %>' NavigateUrl="javascript:void(0)">
												<span class="far fa-edit"></span>
												<asp:Localize runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_EditDefinition %>" />
										</tstsc:HyperLinkEx>
										<tstsc:LinkButtonEx ID="LinkButtonEx1" runat="server" CommandName="Remove" CommandArgument="<%# ((Inflectra.SpiraTest.DataModel.CustomProperty)Container.DataItem).PropertyNumber %>" Confirmation="True" ConfirmationMessage="<%$ Resources:Messages,Admin_CustomPropertyDefinition_RemoveProperty %>">
												<span class="fas fa-trash-alt"></span>
												<asp:Localize runat="server" Text="<%$ Resources:Buttons,Remove %>" />
										</tstsc:LinkButtonEx>
									</div>
								</asp:PlaceHolder>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
			</div>
		</div>
	</div>
	<tstsc:DialogBoxPanel ID="pnlCustomPropDef" runat="server" CssClass="PopupPanel" Width="500px" Height="300px" Persistent="true" Modal="true">
		<div class="widgetBody" style="min-height: 300px; margin-left: 10px">
			<tstsc:TabControl runat="server" ID="tabPropDef" TabWidth="100">
				<TabPages>
					<tstsc:TabPage runat="server" ID="tbpDef" Caption="<%$ Resources:Main,Admin_CustomPropertyDefinition %>" TabPageControlId="panelDefinition" />
					<tstsc:TabPage runat="server" ID="tbpOpt" Caption="<%$ Resources:Main,Admin_CustomPropertyOptions %>" TabPageControlId="panelOptions" />
				</TabPages>
			</tstsc:TabControl>
			<asp:Panel runat="server" ID="panelDefinition" Style="height: 300px">
				<table class="DataEntryForm" style="margin-left: 5px; width: 500px;">
					<tbody>
						<tr id="defAliasTR">
							<td>
								<tstsc:LabelEx runat="server" AssociatedControlID="def_AliasName" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_Alias %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx ID="def_AliasName" MaxLength="64" runat="server" Style="width: 300px" CssClass="text-box" />
							</td>
						</tr>
						<tr id="defTypeTR">
							<td>
								<tstsc:LabelEx runat="server" AssociatedControlID="def_Type" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_Type %>" />:
							</td>
							<td>
								<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="def_Type" DataTextField="Name" DataValueField="CustomPropertyTypeId" NoValueItem="false" Width="100px" ClientScriptMethod="onTypeChange" />
							</td>
						</tr>
						<tr id="defListIdTR">
							<td>
								<tstsc:LabelEx runat="server" AssociatedControlID="def_AliasName" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_CustomList %>" />:
							</td>
							<td>
								<tstsc:DropDownListEx runat="server" CssClass="DropDownList" ID="def_SelectedList" DataTextField="Name" DataValueField="CustomPropertyListId" NoValueItem="false" Width="200px" ClientScriptMethod="onListChange" />
							</td>
						</tr>
						<tr id="defPositionTR">
							<td>
								<tstsc:LabelEx 
									AssociatedControlID="def_Position" 
									runat="server" 
									Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_DispPosition %>" 
									ToolTip="<%$ Resources:Main,Admin_CustomPropertyDefinition_DispPositionTooltip %>"
									/>:
							</td>
							<td>
								<input runat="server" id="def_Position" type="number" max="99" min="1" class="text-box w7 html-validation"/>
							</td>
						</tr>
						<tr id="defHelpTR">
							<td>
								<tstsc:LabelEx runat="server" AssociatedControlID="def_Help" Text="<%$ Resources:Main,Admin_CustomPropertyDefinition_ToolTip %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx runat="server" ID="def_Help" MaxLength="512" TextMode="MultiLine" CssClass="text-box" Width="300px" Height="100px" />
							</td>
						</tr>
					</tbody>
				</table>
			</asp:Panel>
			<asp:Panel runat="server" ID="panelOptions" Style="height: 200px">
				<table class="DataEntryForm" style="margin-left: 5px; width: 500px">
					<tbody>
						<tr id="optDefaultTR">
							<td>
								<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_Default %>" />:
							</td>
							<td style="min-width: 300px">
								<!-- Several controls are needed here, change depending on type selected. -->
								<tstsc:TextBoxEx runat="server" Style="width: 300px" CssClass="text-box" ID="opt_DefaultText" Text="" />
								<tstsc:TextBoxEx runat="server" Style="width: 150px" CssClass="text-box" ID="opt_DefaultNumber" Text="" />
								<tstsc:TextBoxEx runat="server" Style="width: 150px" CssClass="text-box" ID="opt_DefaultDecimal" Text="" />
								<tstsc:CheckBoxEx runat="server" ID="opt_DefaultBoolean" Checked="false" />
								<tstsc:DateControl runat="server" ID="opt_DefaultDate" Width="300px" />
								<tstsc:DropDownListEx runat="server" CssClass="DropDownList" ID="opt_DefaultList" DataTextField="Value" DataValueField="Key" NoValueItem="true" Width="150px" />
								<tstsc:DropDownMultiList runat="server" CssClass="DropDownList" ID="opt_DefaultMultiList" DataTextField="Value" DataValueField="Key" NoValueItem="true" SelectionMode="Multiple" Width="150px" />
								<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="opt_DefaultUser" DataTextField="FullName" DataValueField="UserId" NoValueItem="true" Width="150px" />
							</td>
						</tr>
						<tr id="optAllowEmptyTR">
							<td>
								<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_AllowNull %>" />:
							</td>
							<td>
								<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="opt_AllowEmpty" DataTextField="Value" DataValueField="Key" NoValueItem="false" Width="70px" ClientScriptMethod="onAllowEmptyChange" />
							</td>
						</tr>
						<tr id="optMinLengthTR">
							<td>
								<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_MinLength %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx runat="server" Style="width: 50px" CssClass="text-box" ID="opt_MinLength" />
							</td>
						</tr>
						<tr id="optMaxLengthTR">
							<td>
								<tstsc:LabelEx runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_MaxLength %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx runat="server" Style="width: 50px" CssClass="text-box" ID="opt_MaxLength" />
							</td>
						</tr>
						<tr id="optPrecisionTR">
							<td>
								<tstsc:LabelEx ID="LabelEx8" runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_Precision %>" />:
							</td>
							<td>
								<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="opt_Precision" DataTextField="Value" DataValueField="Key" NoValueItem="false" Width="70px" />
							</td>
						</tr>
						<tr id="optRichTextTR">
							<td>
								<tstsc:LabelEx ID="LabelEx9" runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_RichText %>" />:
							</td>
							<td>
								<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="opt_RichText" DataTextField="Value" DataValueField="Key" NoValueItem="false" Width="70px" />
							</td>
						</tr>
						<tr id="optMinValueTR">
							<td>
								<tstsc:LabelEx ID="LabelEx10" runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_MinValue %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx runat="server" Style="width: 150px" CssClass="text-box" ID="opt_MinValue" />
							</td>
						</tr>
						<tr id="optMaxValueTR">
							<td>
								<tstsc:LabelEx ID="LabelEx11" runat="server" Text="<%$ Resources:Main,Admin_CustomPropertyOptions_MaxValue %>" />:
							</td>
							<td>
								<tstsc:TextBoxEx runat="server" Style="width: 150px" CssClass="text-box" ID="opt_MaxValue" />
							</td>
						</tr>
					</tbody>
				</table>
			</asp:Panel>
			<div>
				<div class="btn-group mt4">
					<tstsc:ButtonEx ID="btn_Save" SkinID="ButtonPrimary" runat="server" Text="Save" ClientScriptMethod="onSaveDefClicked();" />
					<tstsc:ButtonEx ID="btn_Cancel" runat="server" Text="Cancel" ClientScriptMethod="onCancelClicked();" />
				</div>
				<tstsc:LabelEx 
					CssClass="pa2"
					ID="lblDefError" 
					runat="server" 
					Text="" 
					/>
			</div>
		</div>
	</tstsc:DialogBoxPanel>
	<script type="text/javascript">
		String.prototype.trim = function () { return this.replace(/^\s\s*/, '').replace(/\s\s*$/, ''); };
		var defListIdTR,
			ctrlDefText,
			ctrlDefNum,
			ctrlDefDec,
			ctrlDefBool,
			ctrlDefDate,
			ctrlDefListItem,
			ctrlDefMultiItem,
			ctrlDefUser,
			ctrlDefList,
			ctrlDefAlias,
			ctrlDefType,
			trDefMinLength,
			trDefMaxLength,
			trDefAllowEmpty,
			trDefPrecision,
			trDefRichText,
			trDefMinValue,
			trDefMaxValue,
			ctrlMinLen,
			ctrlMaxLen,
			ctrlMinVal,
			ctrlMaxVal,
			ctrlPrecision,
			ctrlAllowEmpty,
			ctrlRichText,
			ctrlDefPos,
			ctrlDefHelp,
			listString,
			custProp = [],
			customPropPositions = [],
			resx = Inflectra.SpiraTest.Web.GlobalResources,
			controlsInitialized = false;

		function initControls() {
			//Load controls..
			// - Options:
			ctrlDefText = $get('<%= this.opt_DefaultText.ClientID %>');
			ctrlDefNum = $get('<%= this.opt_DefaultNumber.ClientID %>');
			ctrlDefDec = $get('<%= this.opt_DefaultDecimal.ClientID %>');
			ctrlDefBool = $get('<%= this.opt_DefaultBoolean.ClientID %>');
			ctrlDefDate = $find('<%= this.opt_DefaultDate.ClientID %>');
			ctrlDefListItem = $find('<%= this.opt_DefaultList.ClientID %>');
			ctrlDefMultiItem = $find('<%= this.opt_DefaultMultiList.ClientID %>');
			ctrlDefUser = $find('<%= this.opt_DefaultUser.ClientID %>');
			ctrlMinLen = $get('<%= this.opt_MinLength.ClientID %>');
			ctrlMaxLen = $get('<%= this.opt_MaxLength.ClientID %>');
			ctrlMinVal = $get('<%= this.opt_MinValue.ClientID %>');
			ctrlMaxVal = $get('<%= this.opt_MaxValue.ClientID %>');
			ctrlPrecision = $find('<%= this.opt_Precision.ClientID %>');
			ctrlAllowEmpty = $find('<%= this.opt_AllowEmpty.ClientID %>');
			ctrlRichText = $find('<%= this.opt_RichText.ClientID %>');
			btnSave = document.getElementById('<%= this.btn_Save.ClientID %>');
			msgSaveErrors = $get('<%= this.lblDefError.ClientID %>');

			// - Option Rows:
			trDefMinLength = $get('optMinLengthTR');
			trDefMaxLength = $get('optMaxLengthTR');
			trDefAllowEmpty = $get('optAllowEmptyTR');
			trDefPrecision = $get('optPrecisionTR');
			trDefRichText = $get('optRichTextTR');
			trDefMinValue = $get('optMinValueTR');
			trDefMaxValue = $get('optMaxValueTR');
			// - Definitions:
			ctrlDefAlias = $get('<%= this.def_AliasName.ClientID %>');
			ctrlDefType = $find('<%= this.def_Type.ClientID %>');
			ctrlDefList = $find('<%= this.def_SelectedList.ClientID %>');
			ctrlDefPos = $get('<%= this.def_Position.ClientID %>');
			ctrlDefHelp = $get('<%= this.def_Help.ClientID %>');

			// - Definition Rows:
			trListIdTR = $get('defListIdTR');

			//Load our lists..
			listString = <%= this.getListValues() %>;
			//Load custom propery data.. This can NOT BE IN A LOOP, because we're calling a serverside function
			//  when this page is generate in-memory, before sending out to client.
			custProp = <%= this.getPropValues() %>;
			customPropPositions = custProp.filter(function (prop) { return prop.position; }).map(function (prop) { return prop.position; });

			ctrlDefPos.addEventListener('input', function (event) {
				if (event.target.value) {
					var isDuplicate = customPropPositions.indexOf(event.target.value) >= 0;
					// only values between 1 and 99, or that are integers are valid
					var isInvalid = event.target.value < 1 || event.target.value > 99 || !Number.isInteger(parseFloat(event.target.value));

					//show warning on position input if number is out of range or a duplicate to one already in use
					if (isDuplicate || isInvalid) {
						ctrlDefPos.classList.add("validation-error");
					} else {
						ctrlDefPos.classList.remove("validation-error");
					}

					//disable the save button if an out of range position number has been entered
					if (isInvalid) {
						//don't let property be saved in this case
						btnSave.disabled = true;
					} else {
						btnSave.disabled = false;
					}
				//make sure the save button is enabled even if the position field is blank (this is ok because the field is not required)
				} else {
					btnSave.disabled = false;
				}
			});


			//Only initialize once
			controlsInitialized = true;
		}

		function onTypeChange(arg1) {
			/* Handles when the type is changed. Markes specific fields visible / enabled or not. */
			var typeNum = parseInt(arg1.get_value());
			var displayElement = function (element, val) {
				if (val) {
					$(element).show();
				} else {
					$(element).hide();
				}
			};
			displayElement(ctrlDefText, (typeNum == 1)); // Text
			displayElement(ctrlDefNum, (typeNum == 2)); // Numeric
			displayElement(ctrlDefDec, (typeNum == 3)); // Decimal
			displayElement(ctrlDefBool, (typeNum == 4)); // Boolean
			displayElement(ctrlDefDate.get_element(), (typeNum == 5)); // Date
			displayElement(ctrlDefListItem.get_element(), (typeNum == 6)); // List
			displayElement(ctrlDefMultiItem.get_element(), (typeNum == 7)); // MultiList
			displayElement(ctrlDefUser.get_element(), (typeNum == 8)); // User
			displayElement(trDefMinLength, (typeNum == 1)); // Text
			displayElement(trDefMaxLength, (typeNum == 1)); // Text
			displayElement(trDefAllowEmpty, (typeNum != 4)); // Not Boolean
			displayElement(trDefPrecision, (typeNum == 3)); // Decimal
			displayElement(trDefRichText, (typeNum == 1)); // Text
			displayElement(trDefMinValue, (typeNum == 2 || typeNum == 3)); // Numeric, Decimal
			displayElement(trDefMaxValue, (typeNum == 2 || typeNum == 3)); // Numeric, Decimal
			displayElement(trListIdTR, (typeNum == 6 | typeNum == 7)); // List, MultiList

			//Makes sure the list values are syncronized up.
			if (typeNum == 6 || typeNum == 7) ctrlDefList.raiseSelectedItemChanged();
		}

		function onListChange(arg1) {
			/* Handles when the List is changed. */

			//Clear the existing items..
			ctrlDefListItem.clearItems();
			ctrlDefMultiItem.clearItems();

			//If we're allowing an empty field, add that as the first item..
			if (ctrlAllowEmpty.get_selectedItem().get_value() == "Y") {
				ctrlDefListItem.addItem('', '');
				ctrlDefMultiItem.addItem('', '');
			}
			ctrlDefListItem.set_dataSource(listString[arg1.get_value()]);
			ctrlDefMultiItem.set_dataSource(listString[arg1.get_value()]);

			//Databind.
			ctrlDefListItem.dataBind();
			ctrlDefMultiItem.dataBind();

			//Set the item..
			ctrlDefListItem.set_selectedItem('');
			ctrlDefMultiItem.set_selectedItem('');
		}

		function onAllowEmptyChange(arg1) {
			var typeNum = parseInt(ctrlDefType.get_selectedItem().get_value());

			//Call the list change, to handle in case the list item is selected..
			if (typeNum == 6) ctrlDefList.raiseSelectedItemChanged();

			//Other checks will go here.
		}

		function onSaveDefClicked(arg1) {
			//Clear existing errors..
			clearErrorFields();
			//Need to error check, first.
			var hasErrors = 0;
			var tab1Errors = 0;
			var hasErrorMsg = '';
			// - Make sure alias (name) isn't already used, or blank:
			var optName = ctrlDefAlias.value;
			if (optName == "") {
				ctrlDefAlias.classList.add("validation-error");
				hasErrors++;
				tab1Errors++;
				msgSaveErrors.innerHTML = resx.Admin_CustomProp_FieldError_NameEmpty;
			}
			else {
				//check for custom properties with this name - this is not allowed
				var nameDuplicates = custProp.filter(function (prop, index) {
					return optName == prop.name && index != ctrlDefAlias.positionNumber;
				});
				if (nameDuplicates.length) {
					ctrlDefAlias.classList.add("validation-error");
					hasErrors++;
					tab1Errors++;
					msgSaveErrors.innerHTML = resx.Admin_CustomProp_FieldError_NameUsed;
				}
			}

			// - Check that a Type is selected.
			if (!ctrlDefType.get_selectedItem() || ctrlDefType.get_selectedItem().get_value() == 0) {
				ctrlDefType.get_element().classList.add("validation-error");
				hasErrors++;
				tab1Errors++;
				msgSaveErrors.innerHTML = resx.Admin_CustomProp_FieldError_TypeEmpty;
			}
			else {
				var selectedType = parseInt(ctrlDefType.get_selectedItem().get_value());

				//Type is selected, now to check other things!
				if (selectedType == 1) // Text
				{
					// - Min & Max Length.
					if (ctrlMinLen.value.trim() != "" && isNaN(ctrlMinLen.value)) {
						ctrlMinLen.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
					}

					if (ctrlMaxLen.value.trim() != "" && isNaN(ctrlMaxLen.value)) {
						//Make sure it's not 'Max', first.
						if (ctrlMaxLen.value.trim().toLowerCase() != "max") {
							ctrlMaxLen.classList.add("validation-error");
							hasErrors++;
							hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
						}
					}

					//Now check values..
					var numMin = parseInt(ctrlMinLen.value);
					var numMax = ((ctrlMaxLen.value.trim().toLowerCase() == "max") ? Number.MAX_VALUE : parseInt(ctrlMaxLen.value));
					if (numMin < 0) {
						ctrlMinLen.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_NumLessZero;
					}
					if (numMax < 0) {
						ctrlMaxLen.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_NumLessZero;
					}
					if (numMax < numMin) {
						ctrlMinLen.classList.add("validation-error");
						ctrlMaxLen.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_MinHigherMax;
					}
				}
				else if (selectedType == 2 || selectedType == 3) // Integer & Decimal
				{
					//Default value..
					if (selectedType == 2) {
						//Check for int.
						if (ctrlDefNum.value.trim() != "" && isNaN(ctrlDefNum.value)) {
							ctrlDefNum.classList.add("validation-error");
							hasErrors++;
							hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
						}
						else {
							//Convert it to integer..
							if (ctrlDefNum.value.trim() != "") ctrlDefNum.value = parseInt(ctrlDefNum.value);
						}
					}
					else if (selectedType == 3) {
						//Check for int.
						if (ctrlDefDec.value.trim() != "" && isNaN(ctrlDefDec.value)) {
							ctrlDefDec.classList.add("validation-error");
							hasErrors++;
							hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
						}
					}

					//Min & Max Values
					if (ctrlMinVal.value.trim() != "" && isNaN(ctrlMinVal.value)) {
						ctrlMinVal.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
					}

					if (ctrlMaxVal.value.trim() != "" && isNaN(ctrlMaxVal.value)) {
						//Make sure it's not 'Max', first.
						if (ctrlMaxVal.value.trim().toLowerCase() != "max") {
							ctrlMaxVal.classList.add("validation-error");
							hasErrors++;
							hasErrorMsg = resx.Admin_CustomProp_FieldError_NaN;
						}
					}

					//Now check values..
					var numMin = parseInt(ctrlMinVal.value);
					var numMax = ((ctrlMaxVal.value.trim().toLowerCase() == "max") ? Number.MAX_VALUE : parseInt(ctrlMaxVal.value));
					if (numMax < numMin) {
						ctrlMinVal.classList.add("validation-error");
						ctrlMaxVal.classList.add("validation-error");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_MinHigherMax;
					}
				}
				else if (selectedType == 4) //Boolean
				{
					//Nothing extra to check here.
				}
				else if (selectedType == 5) //Date
				{
					if (ctrlDefDate.get_value() != "" && !ctrlDefDate.checkDate()) {
						ctrlDefDate.get_element().classList.add("dpkInvalid");
						hasErrors++;
						hasErrorMsg = resx.Admin_CustomProp_FieldError_BadDate;
					}
				}
				else if (selectedType == 6 || selectedType == 7) // List
				{
					//Make sure they selected a list.
					if (ctrlDefList.get_selectedItem() == null || parseInt(ctrlDefList.get_selectedItem().get_value()) == "NaN" || parseInt(ctrlDefList.get_selectedItem().get_value()) < 1) {
						ctrlDefList.get_element().classList.add("DropDownListInvalid");
						hasErrors++;
						tab1Errors++;
						msgSaveErrors.innerHTML = resx.Admin_CustomProp_FieldError_ListEmpty;
					}
				}
				else if (selectedType == 8) {
					//Nothing extra to check here.
				}
			}

			if (tab1Errors > 0) {
				//Switch to tab 1. Message already shown.
				var tab = $find('<%= this.tabPropDef.ClientID %>');
				tab.set_selectedTabClientId('<%= this.panelDefinition.ClientID %>');
			}
			else if (hasErrors > 0) {
				//Switch to tab 2. Message already shown.
				var tab = $find('<%= this.tabPropDef.ClientID %>');
				tab.set_selectedTabClientId('<%= this.panelOptions.ClientID %>');
				if (hasErrors > 1)
					msgSaveErrors.innerHTML = resx.Admin_CustomProp_FieldError_MultipleErrors;
				else
					msgSaveErrors.innerHTML = hasErrorMsg;
			}
			else {
				var parameter = "savedef_" + ctrlDefAlias.positionNumber;
				__doPostBack('<%= this.btn_Save.ClientID %>', parameter);
			}
		}

		function onDisplayProp(arg1) {
			//Get handle to dialog box
			var pnlCustomPropDef = $find('<%= this.pnlCustomPropDef.ClientID %>');

			//Make sure we're initialized
			if (!controlsInitialized) {
				initControls();
			}

			//Clear our fields, first..
			clearOptionFields();

			//Set our fields..
			var custNum = parseInt(arg1);
			var typeNum = parseInt(custProp[custNum]["type"]);
			if (typeNum == 0) typeNum = 1;
			ctrlDefPos.value = custProp[custNum]["position"];
			ctrlDefHelp.value = custProp[custNum]["description"];
			ctrlDefAlias.value = custProp[custNum]["name"];
			ctrlDefAlias.positionNumber = custNum;
			ctrlDefType.set_selectedItem(typeNum.toString());
			ctrlDefType.raiseSelectedItemChanged();
			pnlCustomPropDef.set_title(resx.Admin_CustomPropertyOptions_Header.replace("{0}", custNum));

			//Set default fields:
			if (custProp[custNum]["opt_1"])
				ctrlAllowEmpty.set_selectedItem((custProp[custNum]["opt_1"] == "Y") ? "Y" : "N");
			switch (typeNum) {
				case 1:
					{
						if (custProp[custNum]["opt_5"])
							ctrlDefText.value = custProp[custNum]["opt_5"];
						if (custProp[custNum]["opt_3"])
							ctrlMinLen.value = custProp[custNum]["opt_3"];
						if (custProp[custNum]["opt_2"])
							ctrlMaxLen.value = custProp[custNum]["opt_2"];
						if (custProp[custNum]["opt_4"])
							ctrlRichText.set_selectedItem((custProp[custNum]["opt_4"] == "Y") ? "Y" : "N");

					}
					break;

				case 2:
				case 3:
					{
						if (typeNum == 2 && custProp[custNum]["opt_5"])
							ctrlDefNum.value = custProp[custNum]["opt_5"];
						if (typeNum == 3 && custProp[custNum]["opt_5"])
							ctrlDefDec.value = custProp[custNum]["opt_5"];
						if (custProp[custNum]["opt_7"])
							ctrlMinVal.value = custProp[custNum]["opt_7"];
						if (custProp[custNum]["opt_6"])
							ctrlMaxVal.value = custProp[custNum]["opt_6"];
						if (typeNum == 3 && custProp[custNum]["opt_8"])
							ctrlPrecision.set_selectedItem(custProp[custNum]["opt_8"]);
					}
					break;

				case 4:
					{
						if (custProp[custNum]["opt_5"])
							ctrlDefBool.checked = (custProp[custNum]["opt_5"].trim().toLowerCase() == "y");
					}
					break;

				case 5:
					{
						if (custProp[custNum]["opt_5"]) {
							var month = parseInt(custProp[custNum]["opt_5"].substr(5, 2)) - 1;
							var day = parseInt(custProp[custNum]["opt_5"].substr(8, 2));
							var year = parseInt(custProp[custNum]["opt_5"].substr(0, 4));
							ctrlDefDate.set_value(ctrlDefDate.dateFormat(year, month, day));
						}
					}
					break;

				case 6:
				case 7:
					{
						if (custProp[custNum]["opt_listid"]) {
							ctrlDefList.set_selectedItem(custProp[custNum]["opt_listid"]);
							ctrlDefList.raiseSelectedItemChanged();
						}
						if (custProp[custNum]["opt_5"]) {
							if (typeNum == 6) {
								// parse ints on the value because it is a string of the form "000001"
								ctrlDefListItem.set_selectedItem(parseInt(custProp[custNum]["opt_5"]));
							}

							if (typeNum == 7) {
								// parse ints on the value because it is a string of the form "000001"
								var arrayOfInts = custProp[custNum]["opt_5"].replace(/;/g, ",").split(",").map(function (x) {
									return parseInt(x);
								})
								ctrlDefMultiItem.set_selectedItem(arrayOfInts.join());
							}
						}
					}
					break;

				case 8:
					{
						if (custProp[custNum]["opt_5"]) {
							ctrlDefUser.set_selectedItem(parseInt(custProp[custNum]["opt_5"]));
						}
					}
					break;
			}

			//Select first tab..
			var tab = $find('<%= this.tabPropDef.ClientID %>');
			tab.set_selectedTabClientId('<%= this.panelDefinition.ClientID %>');

			//Refresh fields and display the popup.
			pnlCustomPropDef.display();
		}

		function onCancelClicked() {
			//hide the dialog.
			$find('<%= this.pnlCustomPropDef.ClientID %>').close();
			//reset any validation settings
			msgSaveErrors.innerHTML = "";
			var dialog = document.getElementById('<%= this.pnlCustomPropDef.ClientID %>');
			var inputs = dialog.querySelectorAll("input");
			inputs.forEach(input => input.classList.remove("validation-error"));
		}

		function clearOptionFields() {
			ctrlDefText.value = "";
			ctrlDefNum.value = "";
			ctrlDefDec.value = "";
			ctrlDefBool.checked = false;
			ctrlDefDate.set_value("");
			ctrlDefListItem.set_selectedItem('');
			ctrlDefUser.set_selectedItem('');
			ctrlMinLen.value = "";
			ctrlMaxLen.value = "";
			ctrlMinVal.value = "";
			ctrlMaxVal.value = "";
			ctrlPrecision.set_selectedItem('');
			ctrlAllowEmpty.set_selectedItem('');
			ctrlRichText.set_selectedItem('N');
			ctrlDefAlias.value = "";
			ctrlDefType.set_selectedItem('1');
			ctrlDefList.set_selectedItem('');
			ctrlDefMultiItem.set_selectedItem('');
			ctrlDefPos.value = "";
			ctrlDefHelp.value = "";

			clearErrorFields();
		}
		function clearErrorFields() {
			ctrlDefAlias.className = ctrlDefAlias.className.replace(/ TextBoxInvalid/g, "");
			ctrlDefType.get_element().className = ctrlDefType.get_element().className.replace(/ DropDownListInvalid/g, "");
			ctrlDefList.get_element().className = ctrlDefList.get_element().className.replace(/ DropDownListInvalid/g, "");
			ctrlMinVal.className = ctrlMinVal.className.replace(/ TextBoxInvalid/g, "");
			ctrlMaxVal.className = ctrlMaxVal.className.replace(/ TextBoxInvalid/g, "");
			ctrlMinLen.className = ctrlMinLen.className.replace(/ TextBoxInvalid/g, "");
			ctrlMaxLen.className = ctrlMaxLen.className.replace(/ TextBoxInvalid/g, "");
			ctrlDefDec.className = ctrlDefDec.className.replace(/ TextBoxInvalid/g, "");
			ctrlDefNum.className = ctrlDefNum.className.replace(/ TextBoxInvalid/g, "");
		}
		function ddlArtifactType_selectedIndexChange(item) {
			var artifactTypeId = item.get_value();
			window.location = 'CustomProperties.aspx?artifactTypeId=' + artifactTypeId;
		}

    </script>
</asp:Content>
