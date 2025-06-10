using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Edit Custom Properties Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "CustomProperties_Title", "Template-Custom-Properties/#edit-custom-properties", "CustomProperties_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class CustomProperties : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.ProjectTemplate.CustomProperties";
		private List<CustomProperty> _CustList = null;

		/// <summary>
		/// Stores the current artifact type
		/// </summary>
		protected Artifact.ArtifactTypeEnum ArtifactType { get; set; }

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Redirect if there's no project template selected.
			if (ProjectTemplateId < 1)
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

			try
			{
				//Display the project template name
				ltrProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
				lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");
				tabPropDef.SelectedTab = panelDefinition.ClientID;

				//Get the artifact type
				GetArtifactTypeFromQueryString();

				//Set the title to show the correct artifact
				List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(onlyThoseThatSupportCustomProperties: true);
				string artifactName = artifactTypes.Where(type => type.ArtifactTypeId == (int)ArtifactType).FirstOrDefault().Name;
				lblTitle.Text = string.Format(Resources.Main.CustomProperties_Title_WithArtifact, artifactName);

				if (Request.Form["__EVENTARGUMENT"] != null && Request.Form["__EVENTARGUMENT"].StartsWith("savedef"))
				{
					//Call the save-this-definition function..
					saveDefiniition();
				}

				//Register the event handlers
				grdCustomProperties.RowCommand += grdCustomProperties_RowCommand;

				LoadData();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the artifact type from the querystring
		/// </summary>
		protected void GetArtifactTypeFromQueryString()
		{
			ArtifactType = Artifact.ArtifactTypeEnum.Requirement;
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]))
			{
				int artifactTypeId;
				if (int.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID], out artifactTypeId))
				{
					ArtifactType = (Artifact.ArtifactTypeEnum)artifactTypeId;
				}
			}
		}


		/// <summary>Hit when the user selected a command on the datagrid.</summary>
		/// <param name="source">Datagrid</param>
		/// <param name="e">GridViewCommandEventArgs</param>
		protected void grdCustomProperties_RowCommand(object source, GridViewCommandEventArgs e)
		{
			//Get the artifact type
			GetArtifactTypeFromQueryString();

			switch (e.CommandName.ToLowerInvariant().Trim())
			{
				case "remove":
					{
						//They wanted to remove the selected item.
						int propertyNumber = int.Parse((string)e.CommandArgument);
						new CustomPropertyManager().CustomPropertyDefinition_RemoveFromArtifact(ProjectTemplateId, ArtifactType, propertyNumber);

						//Reload data..
						LoadData();

						msgBox.Text = string.Format(Resources.Messages.Admin_CustomPropertyDefinition_Removed, propertyNumber.ToString());
						msgBox.Type = ServerControls.MessageBox.MessageType.Information;
					}
					break;
			}
		}

		/// <summary>Loads the data to the controls.</summary>
		private void LoadData()
		{
			//Load up the fixed lists..
			CustomPropertyManager custMgr = new CustomPropertyManager();
			def_Type.DataSource = custMgr.CustomPropertyTypes_ReturnDictionaryForDropDown();
			opt_DefaultUser.DataSource = new UserManager().RetrieveActiveByProjectId(ProjectId);
			//this.opt_DefaultUser.DataSource = new UserManager().GetUsers(true);
			opt_AllowEmpty.DataSource = GlobalFunctions.YesNoList();
			opt_RichText.DataSource = GlobalFunctions.YesNoList();
			opt_Precision.DataSource = GlobalFunctions.NumberList(0, 10);
			def_SelectedList.DataSource = custMgr.CustomPropertyList_RetrieveForProjectTemplate(ProjectTemplateId);
			if (((List<CustomPropertyList>)def_SelectedList.DataSource).Count == 0)
				((List<CustomPropertyList>)def_SelectedList.DataSource).Add(new CustomPropertyList() { CustomPropertyListId = 0, Name = Resources.Main.Admin_System_CustomList_NoListsDefinedDrop });

			//Load our definitions..
			List<CustomProperty> baseList = custMgr.CustomPropertyDefinition_RetrieveForArtifactType(ProjectTemplateId, ArtifactType, false);
			_CustList = new List<CustomProperty>();
			// - Loop through to make a full table.
			for (int i = 1; i <= CustomProperty.MAX_NUMBER_ARTIFACT_PROPERTIES; i++)
			{
				//See if there's a position defined..
				if (baseList.Where(bl => bl.PropertyNumber == i).Count() == 1)
				{
					_CustList.Add(baseList.Single(bl => bl.PropertyNumber == i));
				}
				else
				{
					//Nothing in the position, add a blank entry..
					_CustList.Add(new CustomProperty { PropertyNumber = i });
				}
			}
			grdCustomProperties.DataSource = _CustList;

			//Link everything up.
			DataBind();
		}

		/// <summary>Returns a JSON formatted string with all custom list values.</summary>
		/// <returns>String suitable for inclusion in JS</returns>
		protected string getListValues()
		{
			string retString = null;

			//We need to get all our lists, and all the values for them..
			List<CustomPropertyList> lists = new CustomPropertyManager().CustomPropertyList_RetrieveForProjectTemplate(ProjectTemplateId, true);

			//Now create our two dictionaries..
			Dictionary<string, Dictionary<string, string>> retList = new Dictionary<string, Dictionary<string, string>>();
			foreach (CustomPropertyList list in lists)
			{
				Dictionary<string, string> values = new Dictionary<string, string>();
				foreach (CustomPropertyValue value in list.Values)
				{
					values.Add(JsonDictionaryOfStrings.KEY_PREFIX + value.CustomPropertyValueId.ToString(), value.Name);
				}

				retList.Add(list.CustomPropertyListId.ToString(), values);
			}

			JavaScriptSerializer serial = new JavaScriptSerializer();
			retString = serial.Serialize(retList);

			return retString;
		}

		/// <summary>Returns a JSON string object for loading into JS the custom property values.</summary>
		/// <param name="propNum">The number of the property - 1-30.</param>
		/// <returns>The string for the JS var.</returns>
		protected string getPropValues()
		{
			const string METHOD_NAME = CLASS_NAME + "getPropValues()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<Dictionary<string, string>> retProps = new List<Dictionary<string, string>>
			{
				//This one is for index '0', which isn't used.
				new Dictionary<string, string>()
			};
			string retString = null;


			try
			{
				//Loop through all our properties, in order.
				retString = "[";
				foreach (var prop in _CustList.OrderBy(p => p.PropertyNumber))
				{
					//Get the basic properties, first. 
					Dictionary<string, string> custPropFields = new Dictionary<string, string>
					{
						//Now create our two dictionaries..
						{ "name", prop.Name.ToSafeString() },
						{ "type", prop.CustomPropertyTypeId.ToString() },
						{ "position", prop.Position.ToString() },
						{ "description", prop.Description },
						{ "opt_listid", prop.CustomPropertyListId.ToString() }
					};

					//Now, add any selected options that they may want. (Required, default value, etc.)
					foreach (CustomPropertyOptionValue propOptValue in prop.Options)
					{
						if (propOptValue.CustomPropertyOptionId == 
							(int)CustomProperty.CustomPropertyOptionEnum.Default &&
							prop.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Date)
						{
							//Need to localize any dates
							if (!string.IsNullOrEmpty(propOptValue.Value))
							{
								DateTime? utcDate = propOptValue.Value.FromDatabaseSerialization_DateTime();
								if (utcDate.HasValue)
								{
									string localizedDate = GlobalFunctions.LocalizeDate(utcDate.Value).ToDatabaseSerialization();
									custPropFields.Add("opt_" + propOptValue.CustomPropertyOptionId, localizedDate);
								}
							}
						}
						else
						{
							//Otherwise, just write out the value.
							custPropFields.Add("opt_" + propOptValue.CustomPropertyOptionId, propOptValue.Value);
						}
					}

					//Add the property to our collection.
					retProps.Add(custPropFields);
				}

				JavaScriptSerializer serial = new JavaScriptSerializer();
				retString = serial.Serialize(retProps);
			}
			catch (Exception ex)
			{
				Logger.LogWarningEvent(METHOD_NAME, ex, "Error inserting custom properties into page.");
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retString;
		}

		/// <summary>Saves the specified custom property definition.</summary>
		private void saveDefiniition()
		{
			//Get our position into a number, if needed.
			int? posNum = null;
			if (!string.IsNullOrWhiteSpace(def_Position.Value) && int.TryParse(def_Position.Value, out int tryNum))
				posNum = tryNum;

			CustomProperty newProp = new CustomProperty
			{
				ProjectTemplateId = ProjectTemplateId,
				ArtifactTypeId = (int)ArtifactType,
				Name = def_AliasName.Text,
				Position = posNum,
				Description = def_Help.Text.Trim(),
				PropertyNumber = int.Parse(Request.Form["__EVENTARGUMENT"].Split('_')[1]),
				CustomPropertyTypeId = int.Parse(def_Type.SelectedValue)
			};

			//Load up the options..
			Dictionary<CustomProperty.CustomPropertyOptionEnum, string> dicOptions = new Dictionary<CustomProperty.CustomPropertyOptionEnum, string>();
			if (newProp.CustomPropertyTypeId != (int)CustomProperty.CustomPropertyTypeEnum.Boolean) //Everything except boolean.
				dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.AllowEmpty, (opt_AllowEmpty.SelectedValue == "Y").ToDatabaseSerialization());

			switch ((CustomProperty.CustomPropertyTypeEnum)newProp.CustomPropertyTypeId)
			{
				case CustomProperty.CustomPropertyTypeEnum.Text:
					{
						if (!string.IsNullOrWhiteSpace(opt_DefaultText.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, opt_DefaultText.Text);
						if (!string.IsNullOrWhiteSpace(opt_MinLength.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MinLength, int.Parse(opt_MinLength.Text).ToDatabaseSerialization());
						if (!string.IsNullOrWhiteSpace(opt_MaxLength.Text))
						{
							int maxValue = 0;
							if (opt_MaxLength.Text.Trim().ToLowerInvariant() == "max")
								maxValue = int.MaxValue;
							else
								maxValue = int.Parse(opt_MaxLength.Text);
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MaxLength, maxValue.ToDatabaseSerialization());
						}
						dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.RichText, (opt_RichText.SelectedValue == "Y").ToDatabaseSerialization());
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.Integer:
					{
						if (!string.IsNullOrWhiteSpace(opt_DefaultNumber.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, int.Parse(opt_DefaultNumber.Text).ToDatabaseSerialization());
						if (!string.IsNullOrWhiteSpace(opt_MinValue.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MinValue, int.Parse(opt_MinValue.Text).ToDatabaseSerialization());
						if (!string.IsNullOrWhiteSpace(opt_MaxValue.Text))
						{
							int maxValue = 0;
							if (opt_MaxValue.Text.Trim().ToLowerInvariant() == "max")
								maxValue = int.MaxValue;
							else
								maxValue = int.Parse(opt_MaxValue.Text);
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MaxValue, maxValue.ToDatabaseSerialization());
						}
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.Decimal: //(Float)
					{
						if (!string.IsNullOrWhiteSpace(opt_DefaultDecimal.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, decimal.Parse(opt_DefaultDecimal.Text).ToDatabaseSerialization());
						if (!string.IsNullOrWhiteSpace(opt_MinValue.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MinValue, decimal.Parse(opt_MinValue.Text).ToDatabaseSerialization());
						if (!string.IsNullOrWhiteSpace(opt_MaxValue.Text))
						{
							decimal maxValue = 0;
							if (opt_MaxValue.Text.Trim().ToLowerInvariant() == "max")
								maxValue = decimal.MaxValue;
							else
								maxValue = decimal.Parse(opt_MaxValue.Text);
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.MaxValue, maxValue.ToDatabaseSerialization());
						}
						if (opt_Precision.SelectedItem != null)
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Precision, int.Parse(opt_Precision.SelectedValue).ToDatabaseSerialization());
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.Boolean:
					{
						dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, opt_DefaultBoolean.Checked.ToDatabaseSerialization());
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.Date:
					{
						if (!string.IsNullOrWhiteSpace(opt_DefaultDate.Text))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, DateTime.Parse(opt_DefaultDate.Text).ToUniversalTime().ToDatabaseSerialization());
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.List:
					{
						newProp.CustomPropertyListId = int.Parse(def_SelectedList.SelectedValue);
						if (opt_DefaultList.RawSelectedValue != null && !string.IsNullOrWhiteSpace(opt_DefaultList.RawSelectedValue))
						{
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, int.Parse(opt_DefaultList.RawSelectedValue).ToDatabaseSerialization());
						}
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.MultiList:
					{
						newProp.CustomPropertyListId = int.Parse(def_SelectedList.SelectedValue);
						//Get the selected defaults, if any.
						List<uint> selectedItems = opt_DefaultMultiList.SelectedValues(true);

						if (selectedItems.Count > 0)
						{
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, selectedItems.ToDatabaseSerialization());
						}
					}
					break;
				case CustomProperty.CustomPropertyTypeEnum.User:
					{
						if (opt_DefaultUser.SelectedItem != null && !string.IsNullOrWhiteSpace(opt_DefaultUser.SelectedValue))
							dicOptions.Add(CustomProperty.CustomPropertyOptionEnum.Default, int.Parse(opt_DefaultUser.SelectedValue).ToDatabaseSerialization());
					}
					break;
			}

			//Now load up the option definitions.
			foreach (KeyValuePair<CustomProperty.CustomPropertyOptionEnum, string> optDef in dicOptions)
			{
				CustomPropertyOptionValue newOptValue = new CustomPropertyOptionValue();
				newOptValue.CustomPropertyOptionId = (int)optDef.Key;
				newOptValue.Value = optDef.Value;

				newProp.Options.Add(newOptValue);
			}

			//See if we need to update, or insert..
			CustomPropertyManager mgrProp = new CustomPropertyManager();
			CustomProperty existingProp = mgrProp.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(ProjectTemplateId, ArtifactType, newProp.PropertyNumber, false);
			if (existingProp != null)
			{
				//Update the existing one..
				newProp.CustomPropertyId = existingProp.CustomPropertyId;
				foreach (CustomPropertyOptionValue custOpt in newProp.Options)
				{
					custOpt.CustomPropertyId = existingProp.CustomPropertyId;
					custOpt.MarkAsModified();
				}
				//Now update it.
				mgrProp.CustomPropertyDefinition_Update(newProp);
			}
			else
			{
				//Create a new one..
				mgrProp.CustomPropertyDefinition_AddToArtifact(newProp);
			}

			msgBox.Text = string.Format(Resources.Messages.Admin_CustomPropertyDefinition_Saved, newProp.PropertyNumber);
			msgBox.Type = MessageBox.MessageType.Information;
		}
	}
}
