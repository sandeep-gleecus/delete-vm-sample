using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using System.Data;

namespace Inflectra.SpiraTest.Web.Classes
{
	/// <summary>
	/// Injects the custom property controls into an HTML table and links to the Ajax form manager control
	/// </summary>
	public static class UnityCustomPropertyInjector
	{

		/// <summary>Creates the custom controls in the specified div for the current project/artifact type</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="artifactType">The current artifact type</param>
		/// <param name="ulDefault">The HTML div to add the default controls to</param>
		/// <param name="divUsers">The optional HTML div to add any user controls to</param>
		/// <param name="divDates">The optional HTML div to add any date controls to</param>
		/// <param name="divRichText">The optional HTML div to add any rich text controls to</param>
		/// <param name="formManager">The AJAX form manager to link the controls with</param>
		/// <param name="numCols">The number of column pairs to use in the table. Default: 2 pairs (4 column)</param>
		/// <param name="forcePlainText">Do we want to force all text editing to plain (e.g. on mobile)</param>
		/// <param name="isReadOnly">Are we populating a read-only form</param>
		/// <returns>A list of any controls that need to be registered with the ScriptManager during the PreRender phase</returns>
		public static List<IScriptControl> CreateControls(
			int projectId,
			int projectTemplateId,
			Artifact.ArtifactTypeEnum artifactType,
			HtmlGenericControl ulDefault,
			AjaxFormManager formManager,
			HtmlGenericControl ulUsers = null,
			HtmlGenericControl ulDates = null,
			HtmlGenericControl ulRichText = null,
			int numCols = 3,
			bool forcePlainText = false,
			bool isReadOnly = false
			)
		{
			List<IScriptControl> preRenderScripts = new List<IScriptControl>();

			//We need to prefix all the IDs to make sure that they are unique
			string idPrefix = ulDefault.ID + "_";

			//Pull the custom property definitions..
			//****NOTE****: if we change the definition here make sure to ALSO change it here: RequirementsService.HierarchicalDocument_PopulateShape in the rich text custom properties section
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, true)
				.OrderBy(cp => cp.Position)
				.ThenBy(cp => cp.PropertyNumber)
				.ThenBy(cp => cp.CustomPropertyId)
				.ToList();

			//List to keep track of unique property numbers. [IN:6264]
			List<int> numsHit = new List<int>();

			//Loop through each custom property, adding them to the layout
			foreach (CustomProperty customProperty in customProperties)
			{
				//See if we've already processed this number. If so, skip.
				if (!numsHit.Contains(customProperty.PropertyNumber))
				{
					//Add it to our list.
					numsHit.Add(customProperty.PropertyNumber);

					//The base HtmlControl.
					HtmlGenericControl ulToAddFieldTo = ulDefault;

					//Create the control..
					WebControl propControl = null;
					AjaxFormControl propAjax = new AjaxFormControl();
					switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
					{
						#region TextBox
						case CustomProperty.CustomPropertyTypeEnum.Text:
							{
								//See if we have rich text or plain
								bool isRichText = false;
								if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText).Count() == 1)
								{
									isRichText = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText).Single().Value.FromDatabaseSerialization_Boolean().Value;
								}

								if (isRichText)
								{
									//First set it to be added to the correct part of the page
									if (ulRichText != null)
									{
										ulToAddFieldTo = ulRichText;
									}

									RichTextBoxJ richTextBox = new RichTextBoxJ();
									richTextBox.ID = idPrefix + customProperty.CustomPropertyFieldName;
									richTextBox.Page = ulDefault.Page;
									richTextBox.Height = Unit.Pixel(100);
									richTextBox.Screenshot_ProjectId = projectId;
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxLength).Count() == 1)
										richTextBox.MaxLength = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxLength).Single().Value.FromDatabaseSerialization_Int32().Value;

									//Save the control..
									propControl = richTextBox;
									//Set AJAX items..
									propAjax.ControlId = richTextBox.ID;
									propAjax.PropertyName = "textValue";

									preRenderScripts.Add(richTextBox);
								}
								else
								{
									UnityTextBoxEx textBox = new UnityTextBoxEx();
									textBox.ID = idPrefix + customProperty.CustomPropertyFieldName;
									textBox.CssClass = "u-input";
									textBox.Page = ulDefault.Page;
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxLength).Count() == 1)
										textBox.MaxLength = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxLength).Single().Value.FromDatabaseSerialization_Int32().Value;
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MinLength).Count() == 1)
										textBox.MetaData = (int)CustomProperty.CustomPropertyOptionEnum.MinLength + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MinLength).Single().Value.FromDatabaseSerialization_Int32().Value.ToString() + ";";
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
										textBox.MetaData += (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

									//Save the control..
									propControl = textBox;
									//Set AJAX items..
									propAjax.ControlId = textBox.ID;
									propAjax.PropertyName = "textValue";
								}
							}
							break;
						#endregion

						#region Integer, Decimal
						case CustomProperty.CustomPropertyTypeEnum.Decimal:
						case CustomProperty.CustomPropertyTypeEnum.Integer:
							{
								UnityTextBoxEx textBox = new UnityTextBoxEx();
								textBox.ID = idPrefix + customProperty.CustomPropertyFieldName;
								textBox.CssClass = "u-input";
								textBox.MetaData = "Format=" + ((customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Decimal) ? "dec" : "int") + ";";

								if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxValue).Count() == 1)
								{
									//Get the decimal or integer max value..
									CustomPropertyOptionValue value = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MaxValue).Single();
									string maxValue = "";
									if ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId == CustomProperty.CustomPropertyTypeEnum.Integer)
										maxValue = value.Value.FromDatabaseSerialization_Int32().ToSafeString();
									else if ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId == CustomProperty.CustomPropertyTypeEnum.Decimal)
										maxValue = value.Value.FromDatabaseSerialization_Decimal().ToSafeString();

									textBox.MetaData += (int)CustomProperty.CustomPropertyOptionEnum.MaxValue + "=" + maxValue + ";";
								}
								if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MinValue).Count() == 1)
								{
									//Get the decimal or integer max value..
									CustomPropertyOptionValue value = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.MinValue).Single();
									string minValue = "";
									if ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId == CustomProperty.CustomPropertyTypeEnum.Integer)
										minValue = value.Value.FromDatabaseSerialization_Decimal().ToSafeString();
									else if ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId == CustomProperty.CustomPropertyTypeEnum.Decimal)
										minValue = value.Value.FromDatabaseSerialization_Int32().ToSafeString();

									textBox.MetaData += (int)CustomProperty.CustomPropertyOptionEnum.MinValue + "=" + minValue + ";";
								}
								if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Decimal && customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Precision).Count() == 1)
									textBox.MetaData = (int)CustomProperty.CustomPropertyOptionEnum.Precision + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Precision).Single().Value.FromDatabaseSerialization_Int32().Value.ToString() + ";";
								if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
									textBox.MetaData += (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

								//Save the control..
								propControl = textBox;
								//Set AJAX items..
								propAjax.ControlId = textBox.ID;
								propAjax.PropertyName = (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Decimal) ? "textValue" : "intValue";
							}
							break;
						#endregion

						#region Boolean
						case CustomProperty.CustomPropertyTypeEnum.Boolean:
							{
								CheckBoxYnEx chkBox = new CheckBoxYnEx();
								chkBox.ID = idPrefix + customProperty.CustomPropertyFieldName;

								//Save the control..
								propControl = chkBox;
								//Set AJAX items..
								propAjax.ControlId = chkBox.ID;
								propAjax.PropertyName = "textValue";
							}
							break;
						#endregion

						#region Date
						case CustomProperty.CustomPropertyTypeEnum.Date:
							{
								//First set it to be added to the correct part of the page
								if (ulDates != null)
								{
									ulToAddFieldTo = ulDates;
								}

								UnityDateControl dateCtrl = new UnityDateControl();
								dateCtrl.ID = idPrefix + customProperty.CustomPropertyFieldName;

								dateCtrl.CssClass = "u-datepicker";
								dateCtrl.DisabledCssClass = "u-datepicker disabled";

								if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
									dateCtrl.MetaData += (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

								//Save the control..
								propControl = dateCtrl;
								//Set AJAX items..
								propAjax.ControlId = dateCtrl.ID;
								propAjax.PropertyName = "textValue";
							}
							break;
						#endregion

						#region User
						case CustomProperty.CustomPropertyTypeEnum.User:
							{
								//First set it to be added to the correct part of the page
								if (ulUsers != null)
								{
									ulToAddFieldTo = ulUsers;
								}

								UnityDropDownUserList list = new UnityDropDownUserList();
								list.ID = idPrefix + customProperty.CustomPropertyFieldName;
								list.NoValueItem = true;
								list.Enabled = !isReadOnly;
								list.NoValueItemText = (isReadOnly) ? Resources.Dialogs.Global_NoneDropDown : Resources.Dialogs.Global_PleaseSelectDropDown;

								//Set the list's data source..
								List<User> dataSource = new UserManager().RetrieveActiveByProjectId(projectId);

								if (dataSource != null)
								{
									list.DataTextField = "FullName";
									list.DataValueField = "UserId";
									list.ActiveItemField = "IsActive";
									list.DataSource = dataSource;
									list.CssClass = "u-dropdown u-dropdown_user";
									list.DisabledCssClass = "u-dropdown u-dropdown_user disabled";
									list.DataBind();

									//Load options..
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
										list.MetaData = (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

									//Save the control..
									propControl = list;
									//Set AJAX items..
									propAjax.ControlId = list.ID;
									propAjax.PropertyName = "intValue";
								}
								else
								{
									Logger.LogErrorEvent("CustomPropertyInjector.CreateControls()", "Null returned from UserManager.");
								}
							}
							break;

						#endregion

						#region List
						case CustomProperty.CustomPropertyTypeEnum.List:
							{
								UnityDropDownListEx list = new UnityDropDownListEx();
								list.ID = idPrefix + customProperty.CustomPropertyFieldName;
								list.NoValueItem = true;
								list.Enabled = !isReadOnly;
								list.NoValueItemText = (isReadOnly) ? Resources.Dialogs.Global_NoneDropDown : Resources.Dialogs.Global_PleaseSelectDropDown;

								//Set the list's data source..
								IOrderedEnumerable<CustomPropertyValue> dataSource = null;
								if (customProperty.List != null && customProperty.CustomPropertyListId.HasValue)
								{
									//We need to sort appropriately
									IOrderedEnumerable<CustomPropertyValue> sortedValues;
									if (customProperty.List.IsSortedOnValue)
										sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.Name);
									else
										sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.CustomPropertyValueId);
									dataSource = sortedValues;
								}

								if (dataSource != null)
								{
									list.DataTextField = "Name";
									list.DataValueField = "CustomPropertyValueId";
									list.DataSource = dataSource;
									list.CssClass = "u-dropdown";
									list.DisabledCssClass = "u-dropdown disabled";
									list.DataBind();

									//Load options..
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
										list.MetaData = (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

									//Save the control..
									propControl = list;
									//Set AJAX items..
									propAjax.ControlId = list.ID;
									propAjax.PropertyName = "intValue";
								}
								else
								{
									Logger.LogErrorEvent("CustomPropertyInjector.CreateControls()", "Null returned from CustomList.");
								}
							}
							break;
						#endregion

						#region MultiList
						case CustomProperty.CustomPropertyTypeEnum.MultiList:
							{
								UnityDropDownMultiList list = new UnityDropDownMultiList();
								list.ID = idPrefix + customProperty.CustomPropertyFieldName;
								list.NoValueItem = true;
								list.Enabled = !isReadOnly;
								list.NoValueItemText = (isReadOnly) ? Resources.Dialogs.Global_NoneDropDown : Resources.Dialogs.Global_PleaseSelectDropDown;

								//Set the list's data source..
								if (customProperty.List != null && customProperty.CustomPropertyListId.HasValue)
								{
									//We need to sort appropriately
									IOrderedEnumerable<CustomPropertyValue> sortedValues;
									if (customProperty.List.IsSortedOnValue)
										sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.Name);
									else
										sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.CustomPropertyValueId);
									list.DataTextField = "Name";
									list.DataValueField = "CustomPropertyValueId";
									list.DataSource = sortedValues;
									list.SelectionMode = ListSelectionMode.Multiple;
									list.CssClass = "u-dropdown u-dropdown_multi";
									list.DisabledCssClass = "u-dropdown u-dropdown_multi disabled";
									list.DataBind();

									//Load options..
									if (customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Count() == 1)
										list.MetaData = (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty + "=" + customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty).Single().Value.FromDatabaseSerialization_Boolean().Value.ToString();

									//Save the control..
									propControl = list;
									//Set AJAX items..
									propAjax.ControlId = list.ID;
									propAjax.PropertyName = "textValue";
								}
							}
							break;
							#endregion
					}

					//Set the tooltip.
					propControl.ToolTip = customProperty.Description;

					//Set the common Ajax stuff..
					propAjax.DataField = customProperty.CustomPropertyFieldName;
					propAjax.Direction = AjaxFormControl.FormControlDirection.InOut;
					formManager.ControlReferences.Add(propAjax);

					//Create Parent li item
					HtmlGenericControl liGroup = new HtmlGenericControl("li");
					liGroup.Attributes.Add("class", "ma0 mb2 pa0");
					ulToAddFieldTo.Controls.Add(liGroup);

					//Create the label and add it...
					LabelEx propLabel = new LabelEx
					{
						ID = propControl.ID + "_Label",
						Text = Microsoft.Security.Application.Encoder.HtmlEncode(customProperty.Name) + ":",
						AssociatedControlID = propControl.ID,
						ToolTip = customProperty.Description
					};

					//add the label AFTER the control if it is a rich text box - to enable easier ui styling when the RTE gets focus
					if (propControl is RichTextBoxJ)
					{
						//Add the control if it is not null
						if (propControl != null)
						{
							liGroup.Controls.Add(propControl);
						}
						liGroup.Controls.Add(propLabel);
					}
					else
					{
						liGroup.Controls.Add(propLabel);

						//add a space after the label to keep formatting consistent with control groups created on the aspx page
						liGroup.Controls.Add(new LiteralControl(" "));

						//Add the control if it is not null
						if (propControl != null)
						{
							liGroup.Controls.Add(propControl);
						}
					}
				}
				else
					Logger.LogErrorEvent(
						"Web.Classes.UnityCustomPropertyInjector::CreateControls",
							"Custom property for Artifact Type " +
							((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId).ToString() +
							" in Template #" +
							projectTemplateId +
							" had more than one active custom property in position #" +
							customProperty.PropertyNumber.ToString());
			}

			//Now we need to add any data mapping 'fake' custom properties
			//We don't need the actual artifact mapping value, just the list of which systems are configured
			//and if this artifact type even supports data mapping
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<ArtifactType> artifactTypes = dataMappingManager.RetrieveArtifactTypes();
			bool supportsDataMapping = artifactTypes.Any(a => a.ArtifactTypeId == (int)artifactType);
			int dataMappingCount = 0;
			if (supportsDataMapping)
			{
				//Get the list of active data-sync systems
				List<DataSyncSystem> dataSyncSystems = dataMappingManager.RetrieveDataSyncSystemsForProject(projectId);
				foreach (DataSyncSystem dataSyncSystem in dataSyncSystems)
				{
					//Create Parent Div Form Group
					HtmlGenericControl liGroup = new HtmlGenericControl("li");
					liGroup.Attributes.Add("class", "ma0 mb2 pa0");
					ulDefault.Controls.Add(liGroup);

					//Create the textbox
					UnityTextBoxEx textBox = new UnityTextBoxEx();
					textBox.ID = idPrefix + "_" + DataMappingManager.FIELD_PREPEND + dataSyncSystem.DataSyncSystemId;
					textBox.CssClass = "u-input";
					textBox.MaxLength = 255;
					textBox.Authorized_Permission = Project.PermissionEnum.ProjectAdmin;
					textBox.Page = ulDefault.Page;

					//Create the label..
					LabelEx propLabel = new LabelEx();
					propLabel.ID = idPrefix + "_" + textBox.ID + "_Label";
					propLabel.Text = dataSyncSystem.DisplayName + " ID:";
					propLabel.AssociatedControlID = textBox.ID;
					propLabel.Required = false;
					propLabel.Authorized_Permission = Project.PermissionEnum.ProjectAdmin;

					//Add the two items to our parent div.
					//add a space after the label to keep formatting consisten with control groups creating on the aspx page
					liGroup.Controls.Add(propLabel);
					liGroup.Controls.Add(new LiteralControl(" "));
					liGroup.Controls.Add(textBox);

					//Create any clearfix divs required and add them to the parent div.
					dataMappingCount += 1;
					if (dataMappingCount % 2 == 0 || dataMappingCount % 3 == 0)
					{
						HtmlGenericControl clearfixDiv = new HtmlGenericControl("DIV");

						// if (dataMappingCount % 2 == 0 && dataMappingCount % 3 == 0)
						//{
						//  clearfixDiv.Attributes.Add("class", "clearfix visible-lg-block clearfix visible-md-block clearfix visible-sm-block");
						//}
						if (dataMappingCount % 2 == 0)
						{
							clearfixDiv.Attributes.Add("class", "clearfix clearfix visible-md-block clearfix visible-sm-block");
						}
						if (dataMappingCount % 3 == 0)
						{
							clearfixDiv.Attributes.Add("class", "clearfix visible-lg-block");
						}

						ulDefault.Controls.Add(clearfixDiv);
					}

					//Register with the Ajax form manager
					AjaxFormControl propAjax = new AjaxFormControl();
					propAjax.Direction = AjaxFormControl.FormControlDirection.InOut;
					propAjax.ControlId = textBox.ID;
					propAjax.PropertyName = "textValue";
					propAjax.DataField = DataMappingManager.FIELD_PREPEND + dataSyncSystem.DataSyncSystemId;
					formManager.ControlReferences.Add(propAjax);
				}
			}

			return preRenderScripts;
		}
	}
}
