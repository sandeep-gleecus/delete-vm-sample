using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using System.Data.Objects;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for configuring,
	/// retrieving and setting custom properties of any of the artifacts in the system
	/// </summary>
	public class CustomPropertyManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.CustomPropertyManager::";

		public const string TEXT_LEGACY_FORMAT = "TEXT_{0:00}";
		public const string LIST_LEGACY_FORMAT = "LIST_{0:00}";

		/// <summary>Creates a new instance of the class.</summary>
		public CustomPropertyManager()
		{
			const string METHOD_NAME = CLASS_NAME + ".ctor()";
			Logger.LogEnteringEvent(METHOD_NAME);
			Logger.LogExitingEvent(METHOD_NAME);
		}

		#region Static Functions

		/// <summary>
		/// Is the provided artifact field actually a custom property
		/// </summary>
		/// <param name="fieldName">Is the field a custom property</param>
		/// <returns>The property number for the custom field, or NULL if not a custom field</returns>
		public static int? IsFieldCustomProperty(string fieldName)
		{
			int? propertyNumber = null;
			if (fieldName.Length > CustomProperty.FIELD_PREPEND.Length && fieldName.Substring(0, CustomProperty.FIELD_PREPEND.Length) == CustomProperty.FIELD_PREPEND)
			{
				int intValue;
				if (Int32.TryParse(fieldName.Substring(CustomProperty.FIELD_PREPEND.Length), out intValue))
				{
					propertyNumber = intValue;
				}
			}
			return propertyNumber;
		}

		/// <summary>
		/// Returns the sorted list of custom property values, also removes any inactive/deleted ones
		/// </summary>
		/// <param name="customList">The custom list</param>
		/// <returns>The sorted values list</returns>
		public static List<Entity> SortCustomListValuesForLookups(CustomPropertyList customList)
		{
			//We need to sort appropriately
			IOrderedEnumerable<CustomPropertyValue> sortedValues;
			if (customList.IsSortedOnValue)
				sortedValues = customList.Values.Where(cpv => cpv.IsActive && !cpv.IsDeleted).OrderBy(cpv => cpv.Name);
			else
				sortedValues = customList.Values.Where(cpv => cpv.IsActive && !cpv.IsDeleted).OrderBy(cpv => cpv.CustomPropertyValueId);

			return sortedValues.OfType<Entity>().ToList();
		}

		/// <summary>
		/// Returns the sorted list of custom property values
		/// </summary>
		/// <param name="customList">The custom list</param>
		/// <returns>The sorted values list</returns>
		public static List<CustomPropertyValue> SortCustomListValues(CustomPropertyList customList)
		{
			//We need to sort appropriately
			IOrderedEnumerable<CustomPropertyValue> sortedValues;
			if (customList.IsSortedOnValue)
				sortedValues = customList.Values.OrderBy(cpv => cpv.Name).ThenBy(cpv => cpv.CustomPropertyValueId);
			else
				sortedValues = customList.Values.OrderBy(cpv => cpv.CustomPropertyValueId);

			return sortedValues.ToList();
		}

		#endregion

		#region Definitions

		/// <summary>Retrieves a list of all defined custom properties for the specified artifact type and project template</summary>
		/// <param name="artType">The artifact type to get the definitions for.</param>
		/// <param name="includeDeleted">Should we include deleted</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="includeListValues">Should we include the custom lists and values</param>
		/// <param name="includeInactiveListValues">Should we include inactive custom list values</param>
		/// <returns>A list of custom properties.</returns>
		/// <remarks>Results include both active and inactive custom property values</remarks>
		public List<CustomProperty> CustomPropertyDefinition_RetrieveForArtifactType(int projectTemplateId, Artifact.ArtifactTypeEnum artType, bool includeListValues, bool includeDeleted = false, bool includeInactiveListValues = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinitions_RetrieveForArtifactType(int, artType)";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomProperty> retList = new List<CustomProperty>();

			retList = this.CustomPropertyDefinition_RetrieveForArtifactTypeId(projectTemplateId, (int)artType, includeListValues, includeDeleted, includeInactiveListValues);

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves a list of all defined custom properties for the specified artifact type for all project templates.</summary>
		/// <param name="artType">The artifact type to get the definitions for.</param>
		/// <returns>A list of custom properties.</returns>
		/// <remarks>Does not include the Options or Type properties</remarks>
		public List<CustomProperty> CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum artType)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinitions_RetrieveForArtifactType(artType)";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomProperty> retList = new List<CustomProperty>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from cp in context.CustomProperties
											.Include("Type")
											.Include("Options")
								where cp.ArtifactTypeId == (int)artType && !cp.IsDeleted
								orderby cp.ProjectTemplateId, cp.PropertyNumber, cp.CustomPropertyId
								select cp;

					retList = query.ToList();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom properties.");
					retList = new List<CustomProperty>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves a list of all defined custom properties for the specified artifact and project template</summary>
		/// <param name="artTypeId">The artifact type to get the definitions for.</param>
		/// <param name="includeDeleted">Should we include deleted</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="includeListValues">Should we include list values</param>
		/// <param name="includeInactiveListValues">Should we include inactive list values</param>
		/// <returns>A list of custom properties.</returns>
		/// <remarks>
		/// The list of values will include inactive ones so the calling code needs to check before using. Also the list values will
		/// not be sorted due to limitations in how entities work. Need to check the sort flag and sort after the fact.
		/// </remarks>
		public List<CustomProperty> CustomPropertyDefinition_RetrieveForArtifactTypeId(int projectTemplateId, int artTypeId, bool includeListValues, bool includeDeleted = false, bool includeInactiveListValues = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RetrieveForArtifactTypeId(int, int, bool, [bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomProperty> retList = new List<CustomProperty>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					ObjectQuery<CustomProperty> contextList = context.CustomProperties
																.Include("Type")
																.Include("Options");

					if (includeListValues)
					{
						contextList = contextList.Include("List");
					}

					var query = from cp in contextList
								where cp.ArtifactTypeId == artTypeId
									&& cp.ProjectTemplateId == projectTemplateId
									&& (!cp.IsDeleted || includeDeleted)
								orderby cp.PropertyNumber, cp.CustomPropertyId
								select cp;

					retList = query.ToList();

					//Get the values (joined through 'fix-up' implicitly')
					if (includeListValues)
					{
						var listQuery = from clv in context.CustomPropertyValues
										where clv.List.ProjectTemplateId == projectTemplateId
											&& (clv.IsActive || includeInactiveListValues) && !clv.IsDeleted
										orderby clv.Name, clv.CustomPropertyValueId
										select clv;
						listQuery.ToList();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom properties.");
					retList = new List<CustomProperty>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>
		/// retrieves the custom property definition for the given artifact at the specified property number in the specified project template
		/// Returns null if not defined.
		/// </summary>
		/// <param name="projectTemplateId">The id of the current project template</param>
		/// <param name="artType">The artifact type.</param>
		/// <param name="propertyNumber">The property number.</param>
		/// <param name="includeListValues">Should we include the list values</param>
		/// <returns>A CustomPropertyDefinition, or null if none defined.</returns>
		/// <remarks>
		/// Will not throw an exception. Will log error and return null.
		/// The list of values will include inactive ones so the calling code needs to check before using
		/// </remarks>
		public CustomProperty CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(int projectTemplateId, Artifact.ArtifactTypeEnum artType, int propertyNumber, bool includeListValues)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RetrieveForArtifactTypeAtPositionNumber(artType,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomProperty retProp = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					ObjectQuery<CustomProperty> contextList = context.CustomProperties
											.Include("Type")
											.Include("Options");

					if (includeListValues)
					{
						contextList = contextList.Include("List").Include("List.Values");
					}

					var query = from cp in contextList
								where cp.ArtifactTypeId == (int)artType &&
									cp.PropertyNumber == propertyNumber &&
									cp.ProjectTemplateId == projectTemplateId &&
									cp.IsDeleted == false
								select cp;

					retProp = query.FirstOrDefault();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom property for project PT" + projectTemplateId + " artifact " + artType.ToString() + " at property #" + propertyNumber.ToString());
					retProp = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retProp;
		}

		/// <summary>
		/// Copies the custom property definitions, options and values to a new blank project template
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="customPropertyIdMapping">Mapping of the custom property ids</param>
		/// <param name="customPropertyValueMapping">Mapping of the custom property value ids</param>
		/// <returns>A dictionary of the mapping between the old and new custom property value Ids</returns>
		protected internal void CustomPropertyDefinition_CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> customPropertyValueMapping)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_CopyToProjectTemplate(int, int, bool, [bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Store the mapping of custom property lists and values (for later)
			Dictionary<int, int> customPropertyListMapping = new Dictionary<int, int>();

			try
			{
				//Get the existing custom list and custom list values
				List<CustomPropertyList> existingCustomLists = CustomPropertyList_RetrieveForProjectTemplate(existingProjectTemplateId, true);

				//Add to the new project, storing the mappings
				foreach (CustomPropertyList existingCustomList in existingCustomLists)
				{
					int oldListId = existingCustomList.CustomPropertyListId;
					CustomPropertyList newCustomList = CustomPropertyList_Add(newProjectTemplateId, existingCustomList.Name, existingCustomList.IsActive, existingCustomList.IsSortedOnValue);
					int newListId = newCustomList.CustomPropertyListId;
					if (!customPropertyListMapping.ContainsKey(oldListId))
					{
						customPropertyListMapping.Add(oldListId, newListId);
					}

					//Now copy across the values
					//We force sorting by ID to ensure they get created in the same order [IN:3371]
					foreach (CustomPropertyValue existingCustomValue in existingCustomList.Values.OrderBy(c => c.CustomPropertyValueId))
					{
						int oldCustomValueId = existingCustomValue.CustomPropertyValueId;
						CustomPropertyValue newCustomValue = CustomPropertyList_AddValue(newListId, existingCustomValue.Name);
						int newCustomValueId = newCustomValue.CustomPropertyValueId;
						if (!customPropertyValueMapping.ContainsKey(oldCustomValueId))
						{
							customPropertyValueMapping.Add(oldCustomValueId, newCustomValueId);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom lists.");
				throw;
			}

			List<CustomProperty> existingCustomProperties;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the existing custom properties, including options
					var query = from cp in context.CustomProperties
									.Include("Options")
								where cp.ProjectTemplateId == existingProjectTemplateId && !cp.IsDeleted
								orderby cp.ArtifactTypeId, cp.PropertyNumber, cp.CustomPropertyId
								select cp;

					existingCustomProperties = query.ToList();

				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom properties from old.");
					throw;
				}
			}

			try
			{
				//Now loop through and add to destination project
				foreach (CustomProperty existingCustomProperty in existingCustomProperties)
				{
					int? oldListId = existingCustomProperty.CustomPropertyListId;
					int newCustomPropertyId;
					if (oldListId.HasValue && customPropertyListMapping.ContainsKey(oldListId.Value))
					{
						//We need to get the equivalent custom list id in the new project
						int newListId = customPropertyListMapping[oldListId.Value];
						CustomProperty newCustomProperty = CustomPropertyDefinition_AddToArtifact(
							newProjectTemplateId,
							(Artifact.ArtifactTypeEnum)existingCustomProperty.ArtifactTypeId,
							existingCustomProperty.CustomPropertyTypeId,
							existingCustomProperty.PropertyNumber,
							existingCustomProperty.Name,
							existingCustomProperty.Description,
							existingCustomProperty.Position,
							newListId);
						newCustomPropertyId = newCustomProperty.CustomPropertyId;
					}
					else
					{
						CustomProperty newCustomProperty = CustomPropertyDefinition_AddToArtifact(
							newProjectTemplateId,
							(Artifact.ArtifactTypeEnum)existingCustomProperty.ArtifactTypeId,
							existingCustomProperty.CustomPropertyTypeId,
							existingCustomProperty.PropertyNumber,
							existingCustomProperty.Name,
							existingCustomProperty.Description,
							existingCustomProperty.Position,
							null);
						newCustomPropertyId = newCustomProperty.CustomPropertyId;
					}

					//Add to the mapping
					if (!customPropertyIdMapping.ContainsKey(existingCustomProperty.CustomPropertyId))
					{
						customPropertyIdMapping.Add(existingCustomProperty.CustomPropertyId, newCustomPropertyId);
					}

					//Add the options
					foreach (CustomPropertyOptionValue existingOption in existingCustomProperty.Options)
					{
						//If we have a default list value, need to get the new value for the new project
						if (existingOption.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Default && existingCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
						{
							int? defaultValue = existingOption.Value.FromDatabaseSerialization_Int32();
							if (defaultValue.HasValue)
							{
								//Get the corresponding value in the new project, or set to null
								if (customPropertyValueMapping.ContainsKey(defaultValue.Value))
								{
									defaultValue = customPropertyValueMapping[defaultValue.Value];
								}
								else
								{
									defaultValue = null;
								}

								CustomPropertyDefinitionOptions_Add(newCustomPropertyId, existingOption.CustomPropertyOptionId, defaultValue.ToDatabaseSerialization());
							}
						}
						else if (existingOption.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Default && existingCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
						{
							List<int> existingDefaultValues = existingOption.Value.FromDatabaseSerialization_List_Int32();
							List<int> newDefaultValues = new List<int>();
							if (existingDefaultValues.Count > 0)
							{
								foreach (int existingDefaultValue in existingDefaultValues)
								{
									//Get the corresponding value in the new project, or set to null
									if (customPropertyValueMapping.ContainsKey(existingDefaultValue))
									{
										int newDefaultValue = customPropertyValueMapping[existingDefaultValue];
										newDefaultValues.Add(newDefaultValue);
									}
								}
							}
							CustomPropertyDefinitionOptions_Add(newCustomPropertyId, existingOption.CustomPropertyOptionId, newDefaultValues.ToDatabaseSerialization());
						}
						else
						{
							CustomPropertyDefinitionOptions_Add(newCustomPropertyId, existingOption.CustomPropertyOptionId, existingOption.Value);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Adding custom properties to new project.");
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Saves a new custom property definition.
		/// </summary>
		/// <param name="newCustomProp">The custom property definition to save.</param>
		/// <remarks>
		/// For text and list custom properties, it automatically assigns a TEXT_XX and LIST_XX legacy name
		/// for the first 10 text properties and the first 10 list properties. This makes it easier for users to use
		/// the older import/export tools and is needed for legacy API compatibility
		/// </remarks>
		/// <returns>The populated object</returns>
		public CustomProperty CustomPropertyDefinition_AddToArtifact(CustomProperty newCustomProp)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_AddToArtifact()";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//If we have a text or list property, try adding a 'legacy name' if available
					if (newCustomProp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text)
					{
						AddTextLegacyName(context, newCustomProp);
					}
					else if (newCustomProp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
					{
						AddListLegacyName(context, newCustomProp);
					}

					//Make sure we do not already have a property at this property number.
					var exists = context.CustomProperties
						.Any(cp =>
							cp.PropertyNumber == newCustomProp.PropertyNumber &&
							!cp.IsDeleted &&
							cp.ArtifactTypeId == newCustomProp.ArtifactTypeId &&
							cp.ProjectTemplateId == newCustomProp.ProjectTemplateId);
					if (exists)
						throw new ArgumentException("Custom property already exists at property " + newCustomProp.PropertyNumber +
								" for ArtifactType " + newCustomProp.ArtifactTypeId +
								" in ProjectTemplate " + newCustomProp.ProjectTemplateId,
							"PropertyNumber");

					//Check that the position is a useable number.
					if (newCustomProp.Position.HasValue && (newCustomProp.Position.Value > 99 || newCustomProp.Position.Value < 1))
					{
						Logger.LogWarningEvent(METHOD_NAME, "Updating custom property #" + newCustomProp.CustomPropertyId + ", invalid Position. Resetting.");
						newCustomProp.Position = null;
					}
					//Check that the Description is not too long.
					if (newCustomProp.Description != null && newCustomProp.Description.Length > 511)
					{
						Logger.LogWarningEvent(METHOD_NAME, "Updating custom property #" + newCustomProp.CustomPropertyId + ", Description too long. Trimming.");
						newCustomProp.Description = newCustomProp.Description.Substring(0, 511);
					}

					context.CustomProperties.AddObject(newCustomProp);
					context.SaveChanges();

					return newCustomProp;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Adding custom property.");
					throw;
				}
			}
		}

		/// <summary>
		/// Adds a legacy name to a text custom property
		/// </summary>
		/// <param name="context">The EF context</param>
		/// <param name="newCustomProp">The new custom property</param>
		protected void AddTextLegacyName(SpiraTestEntitiesEx context, CustomProperty newCustomProp)
		{
			//Loop through the existing text custom properties and see if we have a spare entry
			var query = from cp in context.CustomProperties
						where !cp.IsDeleted && cp.ArtifactTypeId == newCustomProp.ArtifactTypeId &&
							  cp.ProjectTemplateId == newCustomProp.ProjectTemplateId &&
							  !String.IsNullOrEmpty(cp.LegacyName) &&
							  cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text
						orderby cp.LegacyName
						select cp;
			List<CustomProperty> legacyProperties = query.ToList();
			string availableLegacyName = "";
			for (int i = 1; i <= 10; i++)
			{
				//See if we have any available legacy names
				string legacyName = String.Format(TEXT_LEGACY_FORMAT, i);
				if (legacyProperties.FirstOrDefault(c => c.LegacyName == legacyName) == null)
				{
					availableLegacyName = legacyName;
					break;
				}
			}
			if (!String.IsNullOrEmpty(availableLegacyName))
			{
				newCustomProp.LegacyName = availableLegacyName;
			}
		}

		/// <summary>
		/// Adds a legacy name to a list custom property
		/// </summary>
		/// <param name="context">The EF context</param>
		/// <param name="newCustomProp">The new custom property</param>
		protected void AddListLegacyName(SpiraTestEntitiesEx context, CustomProperty newCustomProp)
		{
			//Loop through the existing text custom properties and see if we have a spare entry
			var query = from cp in context.CustomProperties
						where !cp.IsDeleted && cp.ArtifactTypeId == newCustomProp.ArtifactTypeId &&
							  cp.ProjectTemplateId == newCustomProp.ProjectTemplateId &&
							  !String.IsNullOrEmpty(cp.LegacyName) &&
							  cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List
						orderby cp.LegacyName
						select cp;
			List<CustomProperty> legacyProperties = query.ToList();
			string availableLegacyName = "";
			for (int i = 1; i <= 10; i++)
			{
				//See if we have any available legacy names
				string legacyName = String.Format(LIST_LEGACY_FORMAT, i);
				if (legacyProperties.FirstOrDefault(c => c.LegacyName == legacyName) == null)
				{
					availableLegacyName = legacyName;
					break;
				}
			}
			if (!String.IsNullOrEmpty(availableLegacyName))
			{
				newCustomProp.LegacyName = availableLegacyName;
			}
		}

		/// <summary>Updates the specified custom property with the given one.</summary>
		/// <param name="customProperty">The updated Custom Property.</param>
		public void CustomPropertyDefinition_Update(CustomProperty customProperty)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_Update(CustomProperty)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//Check that the position is a useable number.
				if (customProperty.Position.HasValue && (customProperty.Position.Value > 99 || customProperty.Position.Value < 1))
				{
					Logger.LogWarningEvent(METHOD_NAME, "Updating custom property #" + customProperty.CustomPropertyId + ", invalid Position. Resetting.");
					customProperty.Position = null;
				}
				if (customProperty.Description != null && customProperty.Description.Length > 511)
				{
					Logger.LogWarningEvent(METHOD_NAME, "Updating custom property #" + customProperty.CustomPropertyId + ", Description too long. Trimming.");
					customProperty.Description = customProperty.Description.Substring(0, 511);
				}

				try
				{
					var query = from cp in context.CustomProperties
									.Include("Type")
									.Include("Options")
								where cp.ArtifactTypeId == customProperty.ArtifactTypeId &&
									cp.PropertyNumber == customProperty.PropertyNumber &&
									cp.ProjectTemplateId == customProperty.ProjectTemplateId &&
									!cp.IsDeleted
								select cp;

					CustomProperty retProp = query.FirstOrDefault();

					if (retProp != null)
					{
						//Update values.
						retProp.StartTracking();
						retProp.PropertyNumber = customProperty.PropertyNumber;
						retProp.Name = customProperty.Name;
						retProp.ArtifactTypeId = customProperty.ArtifactTypeId;
						retProp.IsDeleted = customProperty.IsDeleted;
						retProp.Position = customProperty.Position;
						retProp.Description = customProperty.Description;

						//If the type has changed to/from a Text or List property, need to either add or remove the legacy name
						if (retProp.CustomPropertyTypeId != (int)CustomProperty.CustomPropertyTypeEnum.Text && customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text)
						{
							AddTextLegacyName(context, retProp);
						}
						else if (retProp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text && customProperty.CustomPropertyTypeId != (int)CustomProperty.CustomPropertyTypeEnum.Text)
						{
							retProp.LegacyName = null;
						}
						if (retProp.CustomPropertyTypeId != (int)CustomProperty.CustomPropertyTypeEnum.List && customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
						{
							AddTextLegacyName(context, retProp);
						}
						else if (retProp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List && customProperty.CustomPropertyTypeId != (int)CustomProperty.CustomPropertyTypeEnum.List)
						{
							retProp.LegacyName = null;
						}

						//Update the type and list fields
						retProp.CustomPropertyTypeId = customProperty.CustomPropertyTypeId;
						retProp.CustomPropertyListId = customProperty.CustomPropertyListId;
						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Updating custom property.");
					throw;
				}

				//Now resave the options..
				CustomPropertyDefinitionOptions_RemoveAll(customProperty.CustomPropertyId);
				foreach (CustomPropertyOptionValue custOptValue in customProperty.Options)
				{
					CustomPropertyDefinitionOptions_Add(custOptValue.CustomPropertyId, custOptValue.CustomPropertyOptionId, custOptValue.Value);
				}
			}
		}

		/// <summary>Saves a new custom property definition.</summary>
		/// <param name="artType">The artifact type to save it to.</param>
		/// <param name="custTypeId">The type of custom property.</param>
		/// <param name="propertyNumber">The property number in the artifact.</param>
		/// <param name="name">The name of the custom property.</param>
		/// <param name="description">A short description of the custom property.</param>
		/// <param name="defaultValue">The default value of the custom property.</param>
		/// <param name="customPropertyListId">The id of the custom list to associate it to</param>
		/// <param name="projectTemplateId">The id of the current project template</param>
		/// <remarks>Does not specify the options</remarks>
		/// <returns>The populated object</returns>
		public CustomProperty CustomPropertyDefinition_AddToArtifact(
			int projectTemplateId,
			Artifact.ArtifactTypeEnum artType,
			int custTypeId,
			int propertyNumber,
			string name,
			string helpText,
			int? position,
			int? customPropertyListId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_AddToArtifact()";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomProperty retVal;
			try
			{
				retVal = CustomPropertyDefinition_AddToArtifact(new CustomProperty
				{
					ProjectTemplateId = projectTemplateId,
					ArtifactTypeId = (int)artType,
					CustomPropertyTypeId = custTypeId,
					PropertyNumber = propertyNumber,
					Name = name,
					Position = position,
					Description = helpText,
					CustomPropertyListId = customPropertyListId
				});
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "Adding custom property.");
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retVal;
		}

		/// <summary>Removes the custom property at the specified position from the given artifact type.</summary>
		/// <param name="artType">The artifact type to remove from.</param>
		/// <param name="propertyNumber">The property number of the custom property to remove.</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <remarks>Will fail silently. Errors will be logged.</remarks>
		public void CustomPropertyDefinition_RemoveFromArtifact(int projectTemplateId, Artifact.ArtifactTypeEnum artType, int propertyNumber)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RemoveFromArtifact(int, ArtType,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get all the specified props for the given artifact.
					var query = from cp in context.CustomProperties
								where cp.ArtifactTypeId == (int)artType &&
									  cp.ProjectTemplateId == projectTemplateId &&
									  cp.PropertyNumber == propertyNumber
									  && !cp.IsDeleted
								select cp;

					CustomProperty custProp = query.FirstOrDefault();
					if (custProp != null)
					{
						custProp.StartTracking();
						custProp.IsDeleted = true;
						context.SaveChanges();
					}

					Logger.LogExitingEvent(METHOD_NAME);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting custom property at property #" + propertyNumber + " for artifact " + artType.ToString());
					Logger.Flush();
					throw;
				}
			}
		}

		/// <summary>Removes the custom property using the CustProperty Key</summary>
		/// <param name="customPropertyId">The ID of the custom property definition.</param>
		/// <remarks>Used by the SampleDataManager and unit tests</remarks>
		public void CustomPropertyDefinition_RemoveById(int customPropertyId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RemoveById";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get all the specified props for the given artifact.
					var query = from cp in context.CustomProperties
								where cp.CustomPropertyId == customPropertyId
								select cp;

					CustomProperty custProp = query.Single();
					context.DeleteObject(custProp);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting custom property ID #" + customPropertyId.ToString());
					throw ex;
				}
			}
		}

		/// <summary>Removes the custom property definitions for the given artifact type.</summary>
		/// <param name="artifactType">The artifact type to remove from.</param>
		/// <param name="projectTemplateId">The id of the current project template</param>
		/// <remarks>Will fail silently. Errors will be logged.</remarks>
		public void CustomPropertyDefinition_RemoveAllFromArtifact(int projectTemplateId, Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RemoveAllFromArtifact";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get all the specified props for the given artifact.
					var query = from cp in context.CustomProperties
								where cp.ArtifactTypeId == (int)artifactType && cp.ProjectTemplateId == projectTemplateId
								select cp;

					foreach (CustomProperty prop in query.ToList())
					{
						prop.StartTracking();
						prop.IsDeleted = true;
					}
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleteing all custom property definitions for project template PT" + projectTemplateId + ", artifact " + artifactType.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves the Custom Property defition with the ID given. This is NOT the property number.</summary>
		/// <param name="customPropertyId">The ID of the custom property definition to return.</param>
		/// <param name="includeDeleted">Whether to return the deleted one.</param>
		/// <returns>The custom property definition, or null if it's not found or deleted.</returns>
		public CustomProperty CustomPropertyDefinition_RetrieveById(int customPropertyId, bool includeDeleted = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinition_RetrieveById";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomProperty retProp = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from cp in context.CustomProperties
									.Include("Type")
									.Include("Options")
								where cp.CustomPropertyId == customPropertyId &&
								(cp.IsDeleted == false || true == includeDeleted)
								select cp;

					if (query.Count() == 1)
						retProp = query.Single();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting custom property #" + customPropertyId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retProp;
		}

		#endregion

		#region Options

		/// <summary>Get all the option values for the speficied artifact and property property number.</summary>
		/// <param name="artType">The artifact type.</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="propertyNumber">The property number.</param>
		/// <param name="includeCustomProp">Whether to include the definition for the custom property.</param>
		/// <returns>A list of defined options.</returns>
		public List<CustomPropertyOptionValue> CustomPropertyOptions_RetrieveForCustomPropertyNumber(int projectTemplateId, Artifact.ArtifactTypeEnum artType, int propertyNumber, bool includeCustomProp = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_RetrieveForCustomPropertyNumber(int,ArtType,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomPropertyOptionValue> retList = new List<CustomPropertyOptionValue>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from co in context.CustomPropertyOptionValues
									.Include("CustomPropertyOption")
									.Include("CustomProperty")
								where co.CustomProperty.PropertyNumber == propertyNumber
										&& co.CustomProperty.ProjectTemplateId == projectTemplateId
										&& co.CustomProperty.ArtifactTypeId == (int)artType
								select co;
					retList = query.ToList();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving option values for project template PT" + projectTemplateId + " artifact type " + artType.ToString() + ", property #" + propertyNumber.ToString());
					retList = new List<CustomPropertyOptionValue>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Get the specified option values for the speficied artifact and property property number.</summary>
		/// <param name="artType">The artifact type.</param>
		/// <param name="option">The enum value of the option</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="propertyNumber">The custom property property number.</param>
		/// <returns>The defined optiion, or null if not specified.</returns>
		public CustomPropertyOptionValue CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(int projectTemplateId, Artifact.ArtifactTypeEnum artType, int propertyNumber, CustomProperty.CustomPropertyOptionEnum option)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_RetrieveForCustomPropertyNumber(int, ArtType,int,string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyOptionValue retOptValue = null;

			try
			{
				List<CustomPropertyOptionValue> optList = this.CustomPropertyOptions_RetrieveForCustomPropertyNumber(projectTemplateId, artType, propertyNumber);

				if (optList.Where(ol => ol.CustomPropertyOptionId == (int)option).Count() == 1)
					retOptValue = optList.Where(ol => ol.CustomPropertyOptionId == (int)option).FirstOrDefault();
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving option '" + option + "' for artifact " + artType.ToString() + ", property #" + propertyNumber.ToString());
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retOptValue;
		}

		/// <summary>Adds the option to the specified custom property.</summary>
		/// <param name="newOptionValue">The new option value</param>
		public void CustomPropertyOptions_Add(CustomPropertyOptionValue newOptionValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_AddForPropertyNumber(CustomPropertyOptionValue)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.CustomPropertyOptionValues.AddObject(newOptionValue);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Adding new custom property option value.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Adds a new custom property option value to the specified custom property ID.</summary>
		/// <param name="customPropertyId">The ID of the existing custom property to add the option to.</param>
		/// <param name="customPropertyOptionId">The id of the option.</param>
		/// <param name="value">The value of the option.</param>
		public void CustomPropertyOptions_AddForPropertyNumber(int customPropertyId, int customPropertyOptionId, object value)
		{
			//Create the new custom option..
			CustomPropertyOptionValue newOptValue = new CustomPropertyOptionValue();
			newOptValue.CustomPropertyId = customPropertyId;
			newOptValue.CustomPropertyOptionId = customPropertyOptionId;
			newOptValue.Value = value.ToDatabaseSerialization();

			this.CustomPropertyOptions_Add(newOptValue);
		}

		/// <summary>Removes the given custom property from the database.</summary>
		/// <param name="optionValueToRemove">The Custom Property Option Value to remove.</param>
		public void CustomPropertyOptions_Remove(CustomPropertyOptionValue optionValueToRemove)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_Remove(CustomPropertyOptionValue)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the item, first..
					var query = from cp in context.CustomPropertyOptionValues
								where cp.CustomPropertyId == optionValueToRemove.CustomPropertyId &&
									cp.CustomPropertyOptionId == optionValueToRemove.CustomPropertyOptionId
								select cp;

					CustomPropertyOptionValue del = query.FirstOrDefault();
					if (del != null)
					{
						context.CustomPropertyOptionValues.DeleteObject(del);
						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Removing custom property option # '" + optionValueToRemove.CustomPropertyOptionId + "' from Custom Property #" + optionValueToRemove.CustomPropertyId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes the specified option value from the database.</summary>
		/// <param name="customPropertyId">The custom property ID to remove it from.</param>
		/// <param name="customPropertyOptionId">The custom property option ID to remove.</param>
		public void CustomPropertyOptions_Remove(int customPropertyId, int customPropertyOptionId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_Remove(int,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make a new custom property option object..
			CustomPropertyOptionValue custOptionValue = new CustomPropertyOptionValue();
			custOptionValue.CustomPropertyId = customPropertyId;
			custOptionValue.CustomPropertyOptionId = customPropertyOptionId;

			this.CustomPropertyOptions_Remove(custOptionValue);

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Updates the option value for the specified custom property.</summary>
		/// <param name="option">The option to update.</param>
		public void CustomPropertyOptions_Update(CustomPropertyOptionValue optionValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_Update(CustomPropertyOptionValue)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Ge the option..
					var query = from cpo in context.CustomPropertyOptionValues
								where cpo.CustomPropertyId == optionValue.CustomPropertyId &&
									cpo.CustomPropertyOptionId == optionValue.CustomPropertyOptionId
								select cpo;

					//Verify we have one..
					CustomPropertyOptionValue custOptValue = query.FirstOrDefault();

					//Now, update the value..
					if (custOptValue != null)
					{
						custOptValue.StartTracking();
						custOptValue.Value = optionValue.Value;

						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Update existing property option '" + optionValue.CustomPropertyOptionId + "' on CustomId #" + optionValue.CustomPropertyId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Deletes all custom property option valuesfor the given Custom property ID.</summary>
		/// <param name="customPropId">The property to delete custom options from.</param>
		public void CustomPropertyDefinitionOptions_RemoveAll(int customPropertyId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinitionOptions_Delete(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from cpo in context.CustomPropertyOptionValues
								where cpo.CustomPropertyId == customPropertyId
								select cpo;

					List<CustomPropertyOptionValue> retList = query.ToList();

					for (int i = 0; i < retList.Count; i++)
					{
						context.CustomPropertyOptionValues.DeleteObject(retList[i]);
					}

					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting custom property options.");
					throw;
				}
			}
		}

		/// <summary>Adds the specified custom property option value to the given custom property.</summary>
		/// <param name="customPropId">The custom property ID to add to.</param>
		/// <param name="customPropOptionId">The id of the custom property option.</param>
		/// <param name="custOptValue">The value of the custom property option.</param>
		public void CustomPropertyDefinitionOptions_Add(int customPropId, int customPropOptionId, string custOptValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyDefinitionOptions_Add(int,string,string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (custOptValue != null)
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					try
					{
						CustomPropertyOptionValue newOptValue = new CustomPropertyOptionValue();
						newOptValue.CustomPropertyId = customPropId;
						newOptValue.CustomPropertyOptionId = customPropOptionId;
						newOptValue.Value = custOptValue;

						context.CustomPropertyOptionValues.AddObject(newOptValue);
						context.SaveChanges();
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Adding custom property options.");
						throw;
					}
				}
			}
		}

		#endregion

		#region ArtifactCustomProperty functions

		/// <summary>
		/// Saves the changes to a specific artifact custom property
		/// </summary>
		/// <param name="artifactCustomProperty">The artifact custom property entity</param>
		/// <param name="userId">The id the user making the change</param>
		/// <param name="recordHistory">Should we record history records for the change (and send notifications). Default: TRUE</param>
		/// <param name="rollbackId">Whether the save is a rollback or not.Default: NULL</param>
		public void ArtifactCustomProperty_Save(ArtifactCustomProperty artifactCustomProperty, int userId, long? rollbackId = null, bool recordHistory = true)
		{
			const string METHOD_NAME = "ArtifactCustomProperty_Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.ArtifactCustomProperties.ApplyChanges(artifactCustomProperty);
					context.SaveChanges(userId, recordHistory, recordHistory, rollbackId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Physically deletes all the custom properties associated with an artifact. Typically called by other business classes
		/// when the primary artifact is deleted
		/// </summary>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">The type of artifact</param>
		protected internal void ArtifactCustomProperty_DeleteByArtifactId(int artifactId, Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_RetrieveByArtifactId(int,int,ArtifactType,[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We always want the artifact custom property entity
					var query = from ac in context.ArtifactCustomProperties
								where ac.ArtifactId == artifactId &&
									  ac.ArtifactTypeId == (int)artifactType
								select ac;

					if (query.Count() > 0)
					{
						//Delete the artifact custom property record
						ArtifactCustomProperty artifactCustomProperty = query.FirstOrDefault();
						context.DeleteObject(artifactCustomProperty);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a blank new artifact custom property entity for the specific project, artifact type and artifact id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="customPropertyDefinitions">Links any custom property definitions that need to be connected</param>
		/// <returns>The new empty entity</returns>
		public ArtifactCustomProperty ArtifactCustomProperty_CreateNew(int projectId, Artifact.ArtifactTypeEnum artifactType, int artifactId, List<CustomProperty> customPropertyDefinitions = null)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_CreateNew";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Create the new artifact custom property entity
				ArtifactCustomProperty artifactCustomProperty = new ArtifactCustomProperty();
				artifactCustomProperty.ProjectId = projectId;
				artifactCustomProperty.ArtifactTypeId = (int)artifactType;
				artifactCustomProperty.ArtifactId = artifactId;

				if (customPropertyDefinitions != null)
				{
					artifactCustomProperty.CustomPropertyDefinitions = customPropertyDefinitions;
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return artifactCustomProperty;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns all the artifact custom properties for a particular artifact type and project
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactType">The artifact type</param>
		protected internal List<ArtifactCustomProperty> ArtifactCustomProperty_RetrieveForArtifactType(int projectId, int projectTemplateId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_RetrieveForArtifactType(int,ArtifactType)";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{

				List<ArtifactCustomProperty> artifactCustomProperties;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We always want the artifact custom property entity
					var query = from ac in context.ArtifactCustomProperties
								where ac.ArtifactTypeId == (int)artifactType &&
									  ac.ProjectId == projectId
								orderby ac.ArtifactId
								select ac;

					artifactCustomProperties = query.ToList();
				}

				//Get the list of custom prop definitions and add to each artifact custom property row
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, true);
				foreach (ArtifactCustomProperty artifactCustomProperty in artifactCustomProperties)
				{
					artifactCustomProperty.CustomPropertyDefinitions = customProperties;
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return artifactCustomProperties;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>Retrieves the custom property information associated with a particular artifact</summary>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>An artifact custom property entity</returns>
		/// <param name="customPropertyDefinitions">Links any custom property definitions if we already have them</param>
		/// <param name="includeCustPropDefinitions">Should we also retrieve the associated custom property definitions</param>
		/// <remarks>Does not check if the associated artifact has been marked as deleted, the calling function needs to do that</remarks>
		public ArtifactCustomProperty ArtifactCustomProperty_RetrieveByArtifactId(int projectId, int projectTemplateId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, bool includeCustPropDefinitions = false, List<CustomProperty> customPropertyDefinitions = null)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_RetrieveByArtifactId(int,int,ArtifactType,[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				ArtifactCustomProperty artifactCustomProperty = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We always want the artifact custom property entity
					var query = from ac in context.ArtifactCustomProperties
								where ac.ArtifactId == artifactId &&
									  ac.ArtifactTypeId == (int)artifactType &&
									  ac.ProjectId == projectId
								select ac;

					if (query.Count() > 0)
					{
						artifactCustomProperty = query.FirstOrDefault();

						//Get the list of custom prop definitions and add to the artifact custom property row
						if (includeCustPropDefinitions)
						{
							List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, true);
							artifactCustomProperty.CustomPropertyDefinitions = customProperties;
						}
						else if (customPropertyDefinitions != null)
						{
							//Link any provided definitions
							artifactCustomProperty.CustomPropertyDefinitions = customPropertyDefinitions;
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return artifactCustomProperty;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Copies over the list values from one artifact to another (e.g. incident to requirement)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="sourceArtifactType">The source artifact type</param>
		/// <param name="sourceArtifactId">The source artifact ID</param>
		/// <param name="destArtifactType">The destination artifact type</param>
		/// <param name="destArtifactId">The destination artifact ID</param>
		/// <remarks>
		/// Not currently uses for test set > test runs because that code works for multiple test runs at once
		/// </remarks>
		public void ArtifactCustomProperty_CopyListValues(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, int sourceArtifactId, Artifact.ArtifactTypeEnum destArtifactType, int destArtifactId)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_CopyListValues";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				ArtifactCustomProperty sourceCustomProperties = this.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, sourceArtifactId, sourceArtifactType, true);

				//Make sure we have some custom properties on the source
				if (sourceCustomProperties != null)
				{
					//Iterate through the list of destination custom properties and set the appropriate custom properties (if any)
					List<CustomProperty> destCustomPropertyDefinitions = this.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, destArtifactType, false);
					ArtifactCustomProperty destCustomProperties = this.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, destArtifactId, destArtifactType, false, destCustomPropertyDefinitions);
					//Add a new custom property row if necessary
					if (destCustomProperties == null)
					{
						destCustomProperties = this.ArtifactCustomProperty_CreateNew(projectId, destArtifactType, destArtifactId, destCustomPropertyDefinitions);
						destCustomProperties = this.CustomProperty_PopulateDefaults(projectTemplateId, destCustomProperties);
					}
					else
					{
						destCustomProperties.StartTracking();
					}

					foreach (CustomProperty sourceCustomPropertyDefinition in sourceCustomProperties.CustomPropertyDefinitions)
					{
						//See if we have a matching property in the destination custom properties (only works for lists)
						if (sourceCustomPropertyDefinition.CustomPropertyListId.HasValue)
						{
							foreach (CustomProperty destCustomPropertyDefinition in destCustomProperties.CustomPropertyDefinitions)
							{
								if (destCustomPropertyDefinition.CustomPropertyListId.HasValue &&
									destCustomPropertyDefinition.CustomPropertyListId.Value == sourceCustomPropertyDefinition.CustomPropertyListId.Value &&
									destCustomPropertyDefinition.Name.ToLowerInvariant() == sourceCustomPropertyDefinition.Name.ToLowerInvariant()
									)
								{
									//We have a matching custom list between the source and destination
									//So set the value on the matching destination property
									//We also check the name of the property (in case the list used multiple times) using a case invariant check
									object customPropertyValue = sourceCustomProperties.CustomProperty(sourceCustomPropertyDefinition.PropertyNumber);
									destCustomProperties.SetCustomProperty(destCustomPropertyDefinition.PropertyNumber, customPropertyValue);
								}
							}
						}
					}

					//Persist the changes
					this.ArtifactCustomProperty_Save(destCustomProperties, userId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Copies the custom properties from instance of an artifact to another
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template</param>
		/// <param name="projectId">The ID of the project containing the artifact</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying custom properties from</param>
		/// <param name="destArtifactId">The id of the artifact we're copying custom properties to</param>
		/// <param name="artifactType">The type of artifact being copied</param>
		/// <param name="changerId">The ID of the user copying the requirement</param>
		protected internal void ArtifactCustomProperty_Copy(int projectId, int projectTemplateId, int sourceArtifactId, int destArtifactId, Artifact.ArtifactTypeEnum artifactType, int changerId)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_Copy()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get the custom property definitions for this artifact type and project
				List<CustomProperty> customProperties = CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false);

				//Get the artifact custom property
				ArtifactCustomProperty sourceArtifactCustomProperty = ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, sourceArtifactId, artifactType, false, customProperties);
				if (sourceArtifactCustomProperty != null)
				{
					//See if we already have a destination artifact custom property
					ArtifactCustomProperty destArtifactCustomProperty = ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, destArtifactId, artifactType, false, customProperties);

					//Either create a new record or modify the existing one
					if (destArtifactCustomProperty == null)
					{
						destArtifactCustomProperty = ArtifactCustomProperty_CreateNew(projectId, artifactType, destArtifactId, customProperties);
					}
					else
					{
						destArtifactCustomProperty.StartTracking();
					}

					//Copy across the currently active properties
					foreach (CustomProperty customProperty in customProperties)
					{
						object propValue = sourceArtifactCustomProperty.CustomProperty(customProperty.PropertyNumber);
						if (propValue != null)
						{
							destArtifactCustomProperty.SetCustomProperty(customProperty.PropertyNumber, propValue);
						}
					}

					//Save the changes
					ArtifactCustomProperty_Save(destArtifactCustomProperty, changerId);
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Exports the custom properties from instance of an artifact to another in different projects, as long as the templates are the same
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template</param>
		/// <param name="sourceProjectId">The ID of the project containing the artifact</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying custom properties from</param>
		/// <param name="destProjectId">The ID of the project we're copying them to</param>
		/// <param name="destArtifactId">The id of the artifact we're copying custom properties to</param>
		/// <param name="artifactType">The type of artifact being copied</param>
		/// <param name="changerId">The ID of the user copying the requirement</param>
		protected internal void ArtifactCustomProperty_Export(int projectTemplateId, int sourceProjectId, int sourceArtifactId, int destProjectId, int destArtifactId, Artifact.ArtifactTypeEnum artifactType, int changerId)
		{
			const string METHOD_NAME = CLASS_NAME + "ArtifactCustomProperty_Export()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get the custom property definitions for this artifact type and project
				List<CustomProperty> customProperties = CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false);

				//Get the artifact custom property
				ArtifactCustomProperty sourceArtifactCustomProperty = ArtifactCustomProperty_RetrieveByArtifactId(sourceProjectId, projectTemplateId, sourceArtifactId, artifactType, false, customProperties);
				if (sourceArtifactCustomProperty != null)
				{
					//See if we already have a destination artifact custom property
					ArtifactCustomProperty destArtifactCustomProperty = ArtifactCustomProperty_RetrieveByArtifactId(sourceProjectId, projectTemplateId, destArtifactId, artifactType, false, customProperties);

					//Either create a new record or modify the existing one
					if (destArtifactCustomProperty == null)
					{
						destArtifactCustomProperty = ArtifactCustomProperty_CreateNew(destProjectId, artifactType, destArtifactId, customProperties);
					}
					else
					{
						destArtifactCustomProperty.StartTracking();
						destArtifactCustomProperty.ProjectId = destProjectId;
					}

					//Copy across the currently active properties
					foreach (CustomProperty customProperty in customProperties)
					{
						object propValue = sourceArtifactCustomProperty.CustomProperty(customProperty.PropertyNumber);
						if (propValue != null)
						{
							destArtifactCustomProperty.SetCustomProperty(customProperty.PropertyNumber, propValue);
						}
					}

					//Save the changes
					ArtifactCustomProperty_Save(destArtifactCustomProperty, changerId);
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>Gets the defained default value for the given custom property.</summary>
		/// <param name="artType">The artifact type.</param>
		/// <param name="propertyNumber">The property property number.</param>
		/// <returns>The default value, or null if not defined.</returns>
		/// <param name="projectTemplateId">The id of the current project template</param>
		public object CustomProperty_GetDefaultValueForField(int projectTemplateId, Artifact.ArtifactTypeEnum artType, int propertyNumber)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyOptions_RetrieveForCustomPropertyNumber(ArtType,int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			object retObj = null;

			//Need to get the definition..
			CustomPropertyOptionValue propOptionValue = this.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, artType, propertyNumber, CustomProperty.CustomPropertyOptionEnum.Default);
			if (propOptionValue != null)
			{
				retObj = propOptionValue.Value.FromDatabaseSerialization(propOptionValue.CustomProperty.Type.SystemType);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retObj;
		}

		/*
		#region RegEx Functions

		/// <summary>Retrieves a Regex object for the given ID.</summary>
		/// <param name="regexId">The ID to pull.</param>
		/// <returns>A Regex object, or null if error.</returns>
		public Regex RegularExpression_RetrieveRegexById(int regexId)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_RetrieveById(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			Regex retRegex = null;

			//Get the definition, first..
			try
			{
				CustomPropertyRegex regEx = this.RegularExpression_RetrieveById(regexId);

				retRegex = CustomPropertyManager.convertToRegex(regEx);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Generating Regular Expression object for ID #" + regexId.ToString());
				retRegex = null;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retRegex;
		}

		/// <summary>Returns the specified RegularExpression object.</summary>
		/// <param name="regexId">The ID number of the item to retrieve.</param>
		/// <returns>A CustomPropertyRegex object.</returns>
		public CustomPropertyRegex RegularExpression_RetrieveById(int regexId)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_RetrieveById(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyRegex retRegex = null;

			//Get the definition, first..
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from reg in context.CustomPropertyRegexes
								where reg.RegexId == regexId
								select reg;

					retRegex = query.Single();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Selecting CustomPropertyRegex object for ID #" + regexId.ToString());
					retRegex = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retRegex;
		}

		/// <summary>Returns a list of all defined regular expressions.</summary>
		/// <returns>A list of objects.</returns>
		public List<CustomPropertyRegex> RegularExpression_RetrieveAll()
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_RetrieveAll()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomPropertyRegex> retList = new List<CustomPropertyRegex>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from rx in context.CustomPropertyRegexes
								select rx;

					retList = query.ToList();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving list of Regular Expressions.");
					retList = new List<CustomPropertyRegex>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Returns a list of all system defined regular expressions.</summary>
		/// <returns>A list of objects.</returns>
		public List<CustomPropertyRegex> RegularExpression_RetrieveSystem()
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_RetrieveSystem()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomPropertyRegex> retList = new List<CustomPropertyRegex>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from rx in context.CustomPropertyRegexes
								where rx.IsSystem == true
								select rx;

					retList = query.ToList();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving list of System Regular Expressions.");
					retList = new List<CustomPropertyRegex>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;

		}

		public CustomPropertyRegex RegularExpression_RetrieveByName(string name)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_RetrieveByName(string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyRegex retRegex = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from rx in context.CustomPropertyRegexes
								where rx.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim()
								select rx;

					retRegex = query.Single();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving regular expression '" + name.ToLowerInvariant() + "':");
					retRegex = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retRegex;
		}

		/// <summary>Adds a new CustomPropertyRegex into the system.</summary>
		/// <param name="newRegex">The new RegularExpression.</param>
		/// <returns>The saved CustomPropertyRegex</returns>
		public CustomPropertyRegex RegularExpression_Add(CustomPropertyRegex newRegex)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_Add(CustPropReg)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					c.CustomPropertyRegexes.AddObject(newRegex);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Inserting new CustomPropertyRegex object.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newRegex;
		}

		/// <summary>Adds a new CustomPropertyRegex into the system.</summary>
		/// <param name="name">The name of the Regex.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="example">An example of the Regex</param>
		/// <param name="isCaseSensitive">Whether the Regex is case-sensitive or not.</param>
		/// <param name="isCompleteEntry">Whether the Regex is a complete entry.</param>
		/// <returns>The saved CustomPropertyRegex</returns>
		public CustomPropertyRegex RegularExpression_Add(string name, string pattern, string example, bool isCaseSensitive, bool isCompleteEntry)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_Add(string,string,string,bool,bool)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyRegex newRegex = new CustomPropertyRegex();
			newRegex.Example = example;
			newRegex.IsCaseSensitive = isCaseSensitive;
			newRegex.IsCompleteEntry = isCompleteEntry;
			newRegex.IsSystem = false;
			newRegex.Name = name;
			newRegex.Pattern = pattern.Trim();

			this.RegularExpression_Add(newRegex);

			Logger.LogExitingEvent(METHOD_NAME);
			return newRegex;
		}

		/// <summary>Updates an existing CustomPropertyRegex item.</summary>
		/// <param name="regex">The Regex item to update.</param>
		/// <remarks>Will throw exception if a system Regex is attempted to be updated.</remarks>
		public void RegularExpression_Update(CustomPropertyRegex regex)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_Update(CustomPropertyRegex)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//First get the existing item..
					var query = from reg in context.CustomPropertyRegexes
								where reg.RegexId == regex.RegexId
								select reg;

					CustomPropertyRegex pulledReg = query.Single();

					if (pulledReg.IsSystem)
						throw new RegularExpressionCannotChangeSystemException();
					else
					{
						pulledReg.StartTracking();
						pulledReg.Name = regex.Name;
						pulledReg.Pattern = regex.Pattern;
						pulledReg.Example = regex.Example;
						pulledReg.IsCaseSensitive = regex.IsCaseSensitive;
						pulledReg.IsCompleteEntry = regex.IsCompleteEntry;

						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Updating CustomPropertyRegex ID #" + regex.RegexId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes an existing CustomPropertyRegex item.</summary>
		/// <param name="regex">The Regex item to remove.</param>
		/// <remarks>Will throw exception if a system Regex is attempted to be removed.</remarks>
		public void RegularExpression_Delete(CustomPropertyRegex regex)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_Update(CustomPropertyRegex)";
			Logger.LogEnteringEvent(METHOD_NAME);

			this.RegularExpression_Delete(regex.RegexId);

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes an existing CustomPropertyRegex item.</summary>
		/// <param name="regexId">The Regex ID of the item to remove.</param>
		/// <remarks>Will throw exception if a system Regex is attempted to be removed.</remarks>
		public void RegularExpression_Delete(int regexId)
		{
			const string METHOD_NAME = CLASS_NAME + "RegularExpression_Update(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//First get the existing item..
					var query = from reg in context.CustomPropertyRegexes
								where reg.RegexId == regexId
								select reg;

					CustomPropertyRegex pulledReg = query.Single();

					if (pulledReg.IsSystem)
						throw new RegularExpressionCannotDeleteSystemException();
					else
					{
						c.CustomPropertyRegexes.DeleteObject(pulledReg);
						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Updating CustomPropertyRegex ID #" + regexId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion
		 */

		#region List Functions

		/// <summary>Returns all the defined lists in the current project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="includeValues">Whether or not to include the items in the list.</param>
		/// <param name="includeNullNames">Whether or not to include lists that haven't been configured yet.</param>
		/// <returns>A list of custom lists.</returns>
		/// <remarks>List values are not sorted.</remarks>
		public List<CustomPropertyList> CustomPropertyList_RetrieveForProjectTemplate(int projectTemplateId, bool includeValues = false, bool includeNullNames = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_RetrieveForProjectTemplate(int, [bool], [bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomPropertyList> retList = new List<CustomPropertyList>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the list of custom lists
					var query = from cl in context.CustomPropertyLists
								where cl.ProjectTemplateId == projectTemplateId
								select cl;


					retList = query.ToList();
					if (!includeNullNames)
						retList = retList.Where(cl => !String.IsNullOrWhiteSpace(cl.Name)).ToList();

					//Get the values (joined through 'fix-up' implicitly')
					if (includeValues)
					{
						var listQuery = from clv in context.CustomPropertyValues
										where clv.List.ProjectTemplateId == projectTemplateId
											&& clv.IsActive && !clv.IsDeleted
										orderby clv.Name, clv.CustomPropertyValueId
										select clv;
						listQuery.ToList();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving all custom lists in project.");
					retList = new List<CustomPropertyList>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Returns the specified list.</summary>
		/// <param name="listId">The list Id to return.</param>
		/// <param name="includeValues">Whether or not to include list values.</param>
		/// <param name="includeInactive">Should we include inactive values</param>
		/// <param name="includeDeleted">Should we include deleted values</param>
		/// <returns>A custom list.</returns>
		/// <remarks>List cvalues are sorted, if they are included.</remarks>
		public CustomPropertyList CustomPropertyList_RetrieveById(int listId, bool includeValues = false, bool includeInactive = false, bool includeDeleted = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_RetrieveById(int,[bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyList retList = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					ObjectQuery<CustomPropertyList> contextList = context.CustomPropertyLists;
					//if (includeValues) contextList = contextList.Include("Values");

					var query = from cl in contextList
								where cl.CustomPropertyListId == listId
								select cl;

					if (includeValues)
					{
						if (query.Count() > 0)
						{
							CustomPropertyList tempList = query.Single();

							var sortQuery = from clv in context.CustomPropertyValues
											where clv.CustomPropertyListId == listId
												&& (clv.IsActive || includeInactive)
												&& (!clv.IsDeleted || includeDeleted)
											select clv;
							if (tempList.IsSortedOnValue)
								sortQuery = sortQuery.OrderBy(clv => clv.Name);
							else
								sortQuery = sortQuery.OrderBy(clv => clv.CustomPropertyValueId);

							sortQuery.ToList();
						}
					}

					retList = query.Single();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Retrieving custom list ID#" + listId.ToString());
					retList = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Adds a new list to the system.</summary>
		/// <param name="newList">The custom list to add.</param>
		/// <returns>The complete custom list object.</returns>
		public CustomPropertyList CustomPropertyList_Add(CustomPropertyList newList)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Add(CustomPropertyList)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.CustomPropertyLists.AddObject(newList);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Adding list to database.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newList;
		}

		/// <summary>Removes the give custom list.</summary>
		/// <param name="listToRemove">The custoim list to remove.</param>
		public void CustomPropertyList_Remove(CustomPropertyList listToRemove)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Remove(CustomPropertyList)";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (listToRemove != null)
			{
				this.CustomPropertyList_Remove(listToRemove.CustomPropertyListId);
			}
			else
				throw new ArgumentNullException("listToRemove");

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes the custom list specified by the ID.</summary>
		/// <param name="customListId">The ID of the custom list.</param>
		public void CustomPropertyList_Remove(int customListId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Remove(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//First, purge any custom properties that are marked as deleted.
					bool needToSave = false;
					List<CustomProperty> delProps = context.CustomProperties.Where(cp => cp.IsDeleted == true && cp.CustomPropertyListId == customListId).ToList();
					foreach (CustomProperty delProp in delProps)
					{
						context.DeleteObject(delProp);
						needToSave = true;
					}
					if (needToSave)
						context.SaveChanges();

					//Now delete the list.
					//Pull the item..
					var query = from cl in context.CustomPropertyLists
								where cl.CustomPropertyListId == customListId
								select cl;

					context.CustomPropertyLists.DeleteObject(query.Single());
					context.SaveChanges();
				}
				catch (EntityForeignKeyException)
				{
					Logger.LogWarningEvent(METHOD_NAME, "Attempt to delete a custom list that is being used by at least one custom property.");
					throw;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting custom list.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Updates the specified Custom List</summary>
		/// <param name="updList">The custom list that needs updating.</param>
		/// <returns>The updated Custom List.</returns>
		public CustomPropertyList CustomPropertyList_Update(CustomPropertyList updList)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Update(CustomPropertyList)";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (updList == null)
			{
				Exception ex = new ArgumentNullException("updList", "The Custom List to update cannot be null.");
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw ex;
			}

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.CustomPropertyLists.ApplyChanges(updList);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Trying to update custom list #" + updList.CustomPropertyListId);
					throw;
				}
			}

			//Get the new list..
			updList = this.CustomPropertyList_RetrieveById(updList.CustomPropertyListId, true);

			Logger.LogExitingEvent(METHOD_NAME);
			return updList;
		}

		/// <summary>Updates the given custom list ID with the new values.</summary>
		/// <param name="custListId">The ID of the list to update.</param>
		/// <param name="newName">The new name. Blank or null will retain original value.</param>
		/// <returns>The updated custom list.</returns>
		public CustomPropertyList CustomPropertyList_Update(int custListId, string newName)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Update(int, string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Get the current one, and then set new values, and save it..
			CustomPropertyList origList = this.CustomPropertyList_RetrieveById(custListId);
			origList.StartTracking();

			if (!string.IsNullOrWhiteSpace(newName))
				origList.Name = newName;

			CustomPropertyList retList = this.CustomPropertyList_Update(origList);


			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Adds a new list to the current project template</summary>
		/// <param name="projectTemplateId">The id of the current project template</param>
		/// <param name="name">The name of the new custom list.</param>
		/// <param name="isActive">Is the list active or not</param>
		/// <param name="isSortedOnValue">Is the list sorted on value</param>
		/// <returns>The complete custom list object.</returns>
		public CustomPropertyList CustomPropertyList_Add(int projectTemplateId, string name, bool isActive = true, bool isSortedOnValue = true)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_Add(int, string,bool)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyList newList = new CustomPropertyList();
			newList.ProjectTemplateId = projectTemplateId;
			newList.Name = name;
			newList.IsActive = isActive;
			newList.IsSortedOnValue = isSortedOnValue;

			newList = this.CustomPropertyList_Add(newList);

			Logger.LogExitingEvent(METHOD_NAME);
			return newList;
		}

		/// <summary>Adds the given items into the list table for the given custom property list.</summary>
		/// <param name="custListId">The ID of the list to add the values to.</param>
		/// <param name="newValues">New values to add.</param>
		/// <param name="replaceAll">Whether or not the given list REPLACES all previously-defined values. Default: FALSE</param>
		/// <returns>A list object with the new values.</returns>
		public CustomPropertyList CustomPropertyList_AddValues(int custListId, List<CustomPropertyValue> newValues, bool replaceAll = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_UpdateValues(int,List<>,[bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyList retList = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the list..
					CustomPropertyList custList = this.CustomPropertyList_RetrieveById(custListId, true);

					if (custList != null)
					{
						custList.StartTracking();

						if (replaceAll)
							while (custList.Values.Count() > 0)
								custList.Values[0].MarkAsDeleted();

						foreach (CustomPropertyValue value in newValues) custList.Values.Add(value);

						context.CustomPropertyLists.ApplyChanges(custList);
						context.SaveChanges();
					}
					else
					{
						throw new ArtifactNotExistsException("Custom List #" + custListId.ToString() + " does not exist in database.");
					}

					//Pull the new custom list and return it..
					retList = this.CustomPropertyList_RetrieveById(custListId, true);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Adding list values to list.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Adds a new value to a custom list.</summary>
		/// <param name="custListId">The ListId</param>
		/// <param name="newValue">The new value to add.</param>
		/// <returns>The added value</returns>
		public CustomPropertyValue CustomPropertyList_AddValue(int custListId, CustomPropertyValue newValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_AddValue(int,CustomPropertyValue)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the list..
					CustomPropertyList custList = this.CustomPropertyList_RetrieveById(custListId, true);

					if (custList != null)
					{
						context.CustomPropertyValues.AddObject(newValue);
						context.SaveChanges();
					}
					else
					{
						throw new ArtifactNotExistsException("Custom List #" + custListId.ToString() + " does not exist in database.");
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Adding list values to list.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newValue;
		}

		/// <summary>Adds a new value to a custom list.</summary>
		/// <param name="custListId">The ListId</param>
		/// <param name="newValueName">The name of the new value to add.</param>
		/// <param name="isActive">Is the value active</param>
		/// <param name="isDeleted">Is the value deleted</param>
		/// <returns>The added value</returns>
		public CustomPropertyValue CustomPropertyList_AddValue(int custListId, string newValueName, bool isActive = true, bool isDeleted = false)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_AddValue(int,string.[bool])";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyValue retValue = null;

			try
			{
				//Make a new custom property value..
				CustomPropertyValue newValue = new CustomPropertyValue();
				newValue.CustomPropertyListId = custListId;
				newValue.Name = newValueName.Trim();
				newValue.IsActive = isActive;
				newValue.IsDeleted = isDeleted;

				//Call superfunction.
				retValue = this.CustomPropertyList_AddValue(custListId, newValue);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Adding new value '" + newValueName + "' to list #" + custListId.ToString());
				retValue = null;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Replaces the given value in the list with the new item.</summary>
		/// <param name="custListId">The custom list to update.</param>
		/// <param name="replValue">The new value. All fields must be set.</param>
		/// <returns>The updated custom property list.</returns>
		/// <remarks>Does not change the deleted flag</remarks>
		public CustomPropertyList CustomPropertyList_UpdateValue(CustomPropertyValue replValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_UpdateValue(List<>)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyList retList = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//Get the item they wish to update..
				try
				{
					var query = from clv in context.CustomPropertyValues
								where clv.CustomPropertyValueId == replValue.CustomPropertyValueId
								select clv;

					CustomPropertyValue realValue = query.Single();

					realValue.StartTracking();
					realValue.Name = replValue.Name;
					realValue.IsActive = replValue.IsActive;
					context.SaveChanges();

					//Get the new list..
					retList = this.CustomPropertyList_RetrieveById(realValue.CustomPropertyListId, true);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Updaing list value.");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Updates values in the list with the new given list. Will update or add new rows if necessary.</summary>
		/// <param name="replValue">A list of changed or added items.</param>
		/// <returns>An updated Custom List</returns>
		public CustomPropertyList CustomPropertyList_UpdateValues(int customListId, List<CustomPropertyValue> replValue)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_UpdateValues(List<>)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//First we get a list of all values for the list..
					var query = from clv in context.CustomPropertyValues
								where clv.CustomPropertyListId == customListId
								select clv;

					//Now loop through each given value, and update if necessary..
					foreach (CustomPropertyValue value in replValue)
					{
						if (value.CustomPropertyValueId > 0 && query.Where(clv => clv.CustomPropertyValueId == value.CustomPropertyValueId).Count() > 0)
						{
							//It exists. Update if necessary..
							CustomPropertyValue origValue = query.Where(clv => clv.CustomPropertyValueId == value.CustomPropertyValueId).Single();
							origValue.StartTracking();
							origValue.Name = value.Name.Trim();
							origValue.IsActive = value.IsActive;
						}
						else
						{
							//Doesn't exist. Add it. (make sure not marked as deleted)
							value.CustomPropertyListId = customListId;
							value.IsDeleted = false;
							context.CustomPropertyValues.AddObject(value);
						}
					}

					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Updaing custom list values.");
				}
			}

			//Pull the new list..
			CustomPropertyList retList = this.CustomPropertyList_RetrieveById(customListId, true);

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Deletes the value from the list (i.e. sets flag).</summary>
		/// <param name="custValueId">The ID of the value.</param>
		/// <returns>The updated Custom Property List.</returns>
		public CustomPropertyList CustomPropertyList_DeleteValue(int custValueId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_DeleteValue(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			CustomPropertyList retList = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from clv in context.CustomPropertyValues
								where clv.CustomPropertyValueId == custValueId
								select clv;

					CustomPropertyValue customPropertyValue = query.FirstOrDefault();
					if (customPropertyValue != null)
					{
						customPropertyValue.StartTracking();
						customPropertyValue.IsDeleted = true;
						context.SaveChanges();
					}

					retList = this.CustomPropertyList_RetrieveById(customPropertyValue.CustomPropertyListId);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting list value #" + custValueId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Undeletes the value from the list (i.e. unsets deleted flag).</summary>
		/// <param name="custValueId">The ID of the value.</param>
		public void CustomPropertyList_UndeleteValue(int custValueId)
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyList_UndeleteValue(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from clv in context.CustomPropertyValues
								where clv.CustomPropertyValueId == custValueId
								select clv;

					CustomPropertyValue customPropertyValue = query.FirstOrDefault();
					if (customPropertyValue != null)
					{
						customPropertyValue.StartTracking();
						customPropertyValue.IsDeleted = false;
						context.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Undeleting list value #" + custValueId.ToString());
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion

		#region Type Functions

		/// <summary>Returns the list of custom property types for attaching to a drop down.</summary>
		/// <returns>A dictionary of the custom property types.</returns>
		public List<CustomPropertyType> CustomPropertyTypes_ReturnDictionaryForDropDown()
		{
			const string METHOD_NAME = CLASS_NAME + "CustomPropertyTypes_ReturnDictionaryForDropDown()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<CustomPropertyType> retList = new List<CustomPropertyType>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					var query = from cpt in context.CustomPropertyTypes
								select cpt;

					retList = query.ToList();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Getting Custom Property Types from database.");
					retList = new List<CustomPropertyType>();
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		#endregion

		#region Static Functions

		/*
		/// <summary>Converts a CustomPropertyRegex to a compiled Regex.</summary>
		/// <param name="regex">The database-object Regex record.</param>
		/// <returns>A compiled Regex object.</returns>
		public static Regex convertToRegex(CustomPropertyRegex regex)
		{
			const string METHOD_NAME = CLASS_NAME + "convertToRegex(CustomPropertyRegex)";
			Logger.LogEnteringEvent(METHOD_NAME);

			Regex retRegex = null;

			if (regex != null)
			{
				//Generate the Regular Expression object..
				RegexOptions opt = RegexOptions.Compiled;
				if (!regex.IsCaseSensitive)
					opt = opt | RegexOptions.IgnoreCase;
				if (regex.IsCompleteEntry)
				{
					opt = opt | RegexOptions.Singleline;

					//Add $^'s if necessary..
					if (!regex.Pattern.EndsWith("$")) regex.Pattern += "$";
					if (!regex.Pattern.StartsWith("^")) regex.Pattern = "^" + regex.Pattern;
				}
				else
					opt = opt | RegexOptions.Multiline;

				retRegex = new Regex(regex.Pattern, opt);
			}
			else
			{
				throw new ArgumentNullException("regex", "Database object cannot be null.");
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retRegex;
		}
		 */

		#endregion

		#region Exceptions

		/*
		public class RegularExpressionCannotChangeSystemException : Exception
		{
			public RegularExpressionCannotChangeSystemException()
				: base(GlobalResources.Messages.CustomProperty_RegExException_CannotChange)
			{ }
		}

		public class RegularExpressionCannotDeleteSystemException : Exception
		{
			public RegularExpressionCannotDeleteSystemException()
				: base(GlobalResources.Messages.CustomProperty_RegExException_CannotDelete)
			{ }
		}*/

		#endregion

		/// <summary>
		/// Checks the artifact custom propert row for a specific artifact type and project and returns any validation messages
		/// if the row does not match the required options
		/// </summary>
		/// <param name="customProperties">The list of custom properies</param>
		/// <param name="artifactCustomProperty">The artifact custom property</param>
		/// <returns>A dictionary of validation messages</returns>
		public Dictionary<string, string> CustomProperty_Check(List<CustomProperty> customProperties, ArtifactCustomProperty artifactCustomProperty)
		{
			//Generate the list of exceptions to pass back..
			Dictionary<string, string> retList = new Dictionary<string, string>();

			//Now clear out unused fields, and verify existing fields..
			for (int i = 1; i <= CustomProperty.MAX_NUMBER_ARTIFACT_PROPERTIES; i++)
			{
				string propName = CustomProperty.FIELD_PREPEND + i.ToString("00");

				//For each one, first verify that one is defined..
				if (customProperties.Where(cp => cp.PropertyNumber == i).Count() == 1)
				{
					CustomProperty custPropDef = customProperties.Where(cp => cp.PropertyNumber == i).Single();

					//Now check the values entered.
					foreach (CustomPropertyOptionValue custOptValue in custPropDef.Options)
					{
						//Get the things we'll use more than once here..
						Type custPropType = Type.GetType(custPropDef.Type.SystemType.Trim());
						object custValue = artifactCustomProperty.CustomProperty(i);

						if (custValue != null)
						{

							switch ((CustomProperty.CustomPropertyOptionEnum)custOptValue.CustomPropertyOptionId)
							{
								#region Max Length
								case CustomProperty.CustomPropertyOptionEnum.MaxLength:
									{
										if (custPropType == typeof(String))
										{
											string value = (String)custValue;
											if (value.Length > int.Parse(custOptValue.Value))
											{
												//Too long. Trim it down.
												artifactCustomProperty[CustomProperty.FIELD_PREPEND + i.ToString("00")] = value.Substring(0, int.Parse(custOptValue.Value));
											}
										}
										else
										{
											//Remove the config option from the table, it should not be there.
											CustomPropertyOptions_Remove(custOptValue);
										}
									}
									break;
								#endregion

								#region Min Length
								case CustomProperty.CustomPropertyOptionEnum.MinLength:
									{
										if (custPropType == typeof(String))
										{
											string value = (String)custValue;
											if (value.Length < int.Parse(custOptValue.Value))
											{
												//Not long enough. Throw error.
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MinLengthError.Replace("%field%", custPropDef.Name).Replace("%min%", custOptValue.Value).Replace("%act%", value.Length.ToString()));
												break;
											}
										}
										else
										{
											//Remove the config option from the table, it should not be there.
											CustomPropertyOptions_Remove(custOptValue);
										}
									}
									break;
								#endregion

								/*
								#region Regular Expression
								case CustomProperty.CustomPropertyOptionEnum.RegEx:
									{
										if (custPropType == typeof(String))
										{
											string value = (String)custValue;
											//Get the RegEx object..
											Regex regEx = custMgr.RegularExpression_RetrieveRegexById(int.Parse(custOptValue.Value));
											if (!regEx.IsMatch(value))
											{
												//Throw new exception..
												retList.Add(propName, GlobalResources.Messages.CustomProperty_RegExError);
												break;
											}
										}
										else
										{
											//Remove the config option from the table, it should not be there.
											custMgr.CustomPropertyOptions_Remove(custOpt);
										}
									}
									break;
								#endregion
								*/

								#region Max Value
								case CustomProperty.CustomPropertyOptionEnum.MaxValue:
									{
										if (custPropType == typeof(DateTime))
										{
											DateTime value = (DateTime)custValue;
											if (value > custOptValue.Value.FromDatabaseSerialization_DateTime())
											{
												//TODO: Check UTC and how date is displayed here.
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MaxValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(UInt32))
										{
											UInt32 value = (UInt32)custValue;
											if (value > custOptValue.Value.FromDatabaseSerialization_UInt32())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MaxValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(Int32))
										{
											Int32 value = (Int32)custValue;
											if (value > custOptValue.Value.FromDatabaseSerialization_Int32())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MaxValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(Decimal))
										{
											Decimal value = (Decimal)custValue;
											if (value > custOptValue.Value.FromDatabaseSerialization_Decimal())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MaxValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else
										{
											//Remove the config option from the table, it should not be there.
											CustomPropertyOptions_Remove(custOptValue);
										}
									}
									break;
								#endregion

								#region Min Value
								case CustomProperty.CustomPropertyOptionEnum.MinValue:
									{
										if (custPropType == typeof(DateTime))
										{
											DateTime value = (DateTime)custValue;
											if (value < custOptValue.Value.FromDatabaseSerialization_DateTime())
											{
												//TODO: Check UTC and how date is displayed here.
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MinValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(UInt32))
										{
											UInt32 value = (UInt32)custValue;
											if (value < custOptValue.Value.FromDatabaseSerialization_UInt32())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MinValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(Int32))
										{
											Int32 value = (Int32)custValue;
											if (value < custOptValue.Value.FromDatabaseSerialization_Int32())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MinValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else if (custPropType == typeof(Decimal))
										{
											Decimal value = (Decimal)custValue;
											if (value < custOptValue.Value.FromDatabaseSerialization_Decimal())
											{
												retList.Add(propName, GlobalResources.Messages.CustomProperty_MinValueError.Replace("%field%", custPropDef.Name).Replace("%value%", custOptValue.Value));
												break;
											}
										}
										else
										{
											//Remove the config option from the table, it should not be there.
											CustomPropertyOptions_Remove(custOptValue);
										}
									}
									break;
									#endregion

									//Other options are for display..
							}
						}
						else if (custValue == null && custOptValue.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty)
						{
							bool? nullAllowed = custOptValue.Value.FromDatabaseSerialization_Boolean();

							if (!nullAllowed.HasValue || !nullAllowed.Value)
							{
								retList.Add(propName, GlobalResources.Messages.CustomProperty_ValueNullError.Replace("%field%", custPropDef.Name));
							}
						}
					}
				}
				else
				{
					//Nothing defined. Null out the value.
					artifactCustomProperty[CustomProperty.FIELD_PREPEND + i.ToString("00")] = null;
				}
			}
			return retList;
		}

		/// <summary>
		/// Checks the artifact custom propert row for a specific artifact type and project and returns any validation messages
		/// if the row does not match the required options
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template being used</param>
		/// <param name="artifactCustomProperty">The artifact custom property</param>
		/// <returns>A dictionary of validation messages</returns>
		public static Dictionary<string, string> CustomProperty_Check(int projectTemplateId, ArtifactCustomProperty artifactCustomProperty)
		{
			//Generate the manager to use for management..
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//Get the custom prop definitions.
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactCustomProperty.ArtifactTypeEnum, false);

			//Now call the instance method
			return customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
		}

		/// <summary>
		/// Updates the position record of a custom property
		/// </summary>
		/// <see cref="ArtifactManager.ArtifactField_ChangeListPosition"/>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The project template we're using</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="visible">Is the field current visible</param>
		/// <param name="customPropertyNumber">The custom property number that we want to update the position for</param>
		/// <param name="newPosition">The new position of the property</param>
		protected internal void CustomProperty_UpdateListPosition(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, int customPropertyNumber, int newPosition, bool visible)
		{
			const string METHOD_NAME = "CustomProperty_UpdateListPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the custom property record if there is one
					var query = from uc in context.UserCustomProperties.Include("CustomProperty")
								where uc.UserId == userId && uc.ProjectId == projectId &&
										uc.CustomProperty.ArtifactTypeId == (int)artifactType &&
										uc.CustomProperty.PropertyNumber == customPropertyNumber &&
										!uc.CustomProperty.IsDeleted
								select uc;

					UserCustomProperty userCustomProp = query.FirstOrDefault();
					if (userCustomProp == null)
					{
						//We need to create a new record entry. First we need to get the custom property id for this property number
						//if there isn't one we can just ignore the operation and fail quietly
						var query2 = from cp in context.CustomProperties
									 where cp.ArtifactTypeId == (int)artifactType && !cp.IsDeleted &&
										 cp.ProjectTemplateId == projectTemplateId && cp.PropertyNumber == customPropertyNumber
									 select cp;
						CustomProperty customProperty = query2.FirstOrDefault();
						if (customProperty != null)
						{
							userCustomProp = new UserCustomProperty();
							context.UserCustomProperties.AddObject(userCustomProp);
							userCustomProp.UserId = userId;
							userCustomProp.ProjectId = projectId;
							userCustomProp.CustomPropertyId = customProperty.CustomPropertyId;
							userCustomProp.IsVisible = visible;
							userCustomProp.ListPosition = newPosition;
							context.SaveChanges();
						}
					}
					else
					{
						//We just need to update the new position number
						userCustomProp.StartTracking();
						userCustomProp.IsVisible = visible;
						userCustomProp.ListPosition = newPosition;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the width of a custom property
		/// </summary>
		/// <see cref="ArtifactManager.ArtifactField_ChangeListPosition"/>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The project template</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="customPropertyNumber">The custom property number that we want to update the position for</param>
		/// <param name="newWidth">The new width of the property</param>
		protected internal void CustomProperty_UpdateColumnListWidth(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, int customPropertyNumber, int newWidth)
		{
			const string METHOD_NAME = "CustomProperty_UpdateColumnListWidth";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the custom property record if there is one
					var query = from uc in context.UserCustomProperties.Include("CustomProperty")
								where uc.UserId == userId && uc.ProjectId == projectId &&
										uc.CustomProperty.ArtifactTypeId == (int)artifactType &&
										uc.CustomProperty.PropertyNumber == customPropertyNumber &&
										!uc.CustomProperty.IsDeleted
								select uc;

					UserCustomProperty userCustomProp = query.FirstOrDefault();
					if (userCustomProp == null)
					{
						//We need to create a new record entry. First we need to get the custom property id for this property number
						//if there isn't one we can just ignore the operation and fail quietly
						var query2 = from cp in context.CustomProperties
									 where cp.ArtifactTypeId == (int)artifactType && !cp.IsDeleted &&
										 cp.ProjectTemplateId == projectTemplateId && cp.PropertyNumber == customPropertyNumber
									 select cp;
						CustomProperty customProperty = query2.FirstOrDefault();
						if (customProperty != null)
						{
							userCustomProp = new UserCustomProperty();
							context.UserCustomProperties.AddObject(userCustomProp);
							userCustomProp.UserId = userId;
							userCustomProp.ProjectId = projectId;
							userCustomProp.CustomPropertyId = customProperty.CustomPropertyId;
							userCustomProp.IsVisible = true;
							userCustomProp.Width = newWidth;
							context.SaveChanges();
						}
					}
					else
					{
						//We just need to update the new position number
						userCustomProp.StartTracking();
						userCustomProp.Width = newWidth;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Changes the visibility status of a particular custom property
		/// </summary>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The project template</param>
		/// <param name="userId">The user whose preferences we're using</param>
		/// <param name="customPropertyName">The field name of the custom property we want to toggle the visibility for (e.g. Custom_01)</param>
		/// <remarks>This only should be used for custom properties, there is an equivalent method for built-in fields</remarks>
		public void CustomProperty_ToggleListVisibility(int projectId, int projectTemplateId, int userId, DataModel.Artifact.ArtifactTypeEnum artifactType, string customPropertyName)
		{
			const string METHOD_NAME = "CustomProperty_ToggleListVisibility";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//We need to get the property number from the custom property field name
			//If it doesn't parse correctly simply do nothing
			int propertyNumber;
			if (Int32.TryParse(customPropertyName.Replace(CustomProperty.FIELD_PREPEND, ""), out propertyNumber))
			{
				try
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//First we need to retrieve the custom property record if there is one
						var query = from uc in context.UserCustomProperties.Include("CustomProperty")
									where uc.UserId == userId && uc.ProjectId == projectId &&
										  uc.CustomProperty.ArtifactTypeId == (int)artifactType &&
										  uc.CustomProperty.PropertyNumber == propertyNumber &&
										  !uc.CustomProperty.IsDeleted
									select uc;

						UserCustomProperty userCustomProp = query.FirstOrDefault();
						if (userCustomProp == null)
						{
							//We need to create a new record entry. First we need to get the custom property id for this property number
							//if there isn't one we can just ignore the operation and fail quietly
							var query2 = from cp in context.CustomProperties
										 where cp.ArtifactTypeId == (int)artifactType && !cp.IsDeleted &&
											   cp.ProjectTemplateId == projectTemplateId && cp.PropertyNumber == propertyNumber
										 select cp;
							CustomProperty customProperty = query2.FirstOrDefault();
							if (customProperty != null)
							{
								userCustomProp = new UserCustomProperty();
								context.UserCustomProperties.AddObject(userCustomProp);
								userCustomProp.UserId = userId;
								userCustomProp.ProjectId = projectId;
								userCustomProp.CustomPropertyId = customProperty.CustomPropertyId;
								userCustomProp.IsVisible = true;    //All custom properties are hidden by default
								context.SaveChanges();
							}
						}
						else
						{
							//We just need to flip the visible flag
							userCustomProp.StartTracking();
							userCustomProp.IsVisible = !userCustomProp.IsVisible;
							context.SaveChanges();
						}
					}
				}
				catch (System.Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Populates the default values for an artifact custom property row
		/// </summary>
		/// <param name="artCustProp">The artifact custom property row</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The populated row</returns>
		public ArtifactCustomProperty CustomProperty_PopulateDefaults(int projectTemplateId, ArtifactCustomProperty artCustProp)
		{
			//Generate the manager to use for management..
			CustomPropertyManager custMgr = new CustomPropertyManager();

			//Get the custom prop definitions if we don't have them
			List<CustomProperty> custProp;
			if (artCustProp.CustomPropertyDefinitions == null)
			{
				custProp = custMgr.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artCustProp.ArtifactTypeEnum, false);
			}
			else
			{
				custProp = artCustProp.CustomPropertyDefinitions;
			}

			//Now clear out unused fields, and verify existing fields..
			foreach (CustomProperty prop in custProp)
			{
				string propName = CustomProperty.FIELD_PREPEND + prop.PropertyNumber.ToString("00");

				//Get options set..
				if (prop.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Default).Count() == 1)
				{
					CustomPropertyOptionValue custOptValue = prop.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Default).Single();
					artCustProp[propName] = custOptValue.Value;
				}
			}

			return artCustProp;
		}
	}
}
