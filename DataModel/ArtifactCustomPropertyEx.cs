using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>Extended properties for ArtifactCustomProperty entity</summary>
	public partial class ArtifactCustomProperty : Entity
	{
		/// <summary>Returns the artifact type enumeration value that the custom property belongs to</summary>
		public Artifact.ArtifactTypeEnum ArtifactTypeEnum
		{
			get
			{
				return (Artifact.ArtifactTypeEnum)ArtifactTypeId;
			}
		}

		/// <summary>Stores a list of custom property definitions associated with this specific artifact type</summary>
		public List<CustomProperty> CustomPropertyDefinitions
		{
			get;
			internal set;
		}

		/// <summary>Sets a legacy (pre-v4.0) text custom property with the provided legacy name ("TEXT_01")</summary>
		/// <param name="legacyName">The legacy name (e.g. TEXT_01)</param>
		/// <param name="value">The text value</param>
		public void SetLegacyCustomProperty(string legacyName, string value)
		{
			try
			{
				if (CustomPropertyDefinitions != null)
				{
					CustomProperty customProperty = CustomPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Text);
					if (customProperty != null)
					{
						if (string.IsNullOrEmpty(value))
						{
							this[customProperty.CustomPropertyFieldName] = null;
						}
						else
						{
							string serializedValue = value.ToDatabaseSerialization();
							this[customProperty.CustomPropertyFieldName] = serializedValue;
						}
					}
				}
				else
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
				}
			}
			catch (Exception)
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw;
			}
		}

		/// <summary>Sets a legacy (pre-v4.0) list custom property with the provided legacy name ("LIST_01")</summary>
		/// <param name="legacyName">The legacy name (e.g. LIST_01)</param>
		/// <param name="value">The list value</param>
		public void SetLegacyCustomProperty(string legacyName, int? value)
		{
			try
			{
				if (CustomPropertyDefinitions != null)
				{
					CustomProperty customProperty = CustomPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)DataModel.CustomProperty.CustomPropertyTypeEnum.List);
					if (customProperty != null)
					{
						if (value.HasValue)
						{
							string serializedValue = value.ToDatabaseSerialization();
							this[customProperty.CustomPropertyFieldName] = serializedValue;
						}
						else
						{
							this[customProperty.CustomPropertyFieldName] = null;
						}
					}
				}
				else
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
				}
			}
			catch (Exception)
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw;
			}
		}

		/// <summary>Returns the legacy (pre-v4.0) list custom property with the provided legacy name ("LIST_01")</summary>
		/// <param name="legacyName">The legacy name (e.g. LIST_01)</param>
		/// <returns>The integer list value</returns>
		public int LegacyCustomListProperty(string legacyName)
		{
			try
			{
				if (CustomPropertyDefinitions != null)
				{
					CustomProperty customProperty = CustomPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)DataModel.CustomProperty.CustomPropertyTypeEnum.List);
					if (customProperty != null)
					{
						string serializedValue = (string)this[customProperty.CustomPropertyFieldName];
						int? value = serializedValue.FromDatabaseSerialization_Int32();
						return (value.HasValue) ? value.Value : -1;
					}
					return -1;
				}
				else
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
				}
			}
			catch (Exception)
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw;
			}
		}

		/// <summary>Returns the legacy (pre-v4.0) string custom property with the provided legacy name ("TEXT_01")</summary>
		/// <param name="legacyName">The legacy name (e.g. TEXT_01)</param>
		/// <returns>The string value</returns>
		public string LegacyCustomTextProperty(string legacyName)
		{
			try
			{
				if (CustomPropertyDefinitions != null)
				{
					CustomProperty customProperty = CustomPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Text);
					if (customProperty != null)
					{
						string value = (string)this[customProperty.CustomPropertyFieldName];
						return value;
					}
					return "";
				}
				else
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
				}
			}
			catch (Exception)
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw;
			}
		}

		/// <summary>Returns the custom property with the given ID.</summary>
		/// <param name="custNumber">The position number to get the custom property of.</param>
		/// <param name="custProp">The custom proeprty object to pull the data from.</param>
		/// <returns>Object representing the value of the custom property. Null if not defined.</returns>
		public object CustomProperty(int propertyNumber)
		{
			//The return object.
			object retObj = null;

			try
			{
				if (CustomPropertyDefinitions != null)
				{
					//Get the custom property value.
					string custName = DataModel.CustomProperty.FIELD_PREPEND + propertyNumber.ToString("00");
					string custValue = (string)this[custName];
					if (!string.IsNullOrWhiteSpace(custValue))
					{
						try
						{
							CustomProperty prop = CustomPropertyDefinitions.Where(cp => cp.PropertyNumber == propertyNumber).Single();

							retObj = custValue.FromDatabaseSerialization(prop.Type.SystemType);
						}
						catch (Exception)
						{
							//Cannot log because we are in DataModel which does not Reference Common assembly
							List<int> newList = new List<int>();
						}
					}
				}
				else
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
				}
			}
			catch (Exception)
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw;
			}

			return retObj;
		}

		/// <summary>Sets the given custom property to the specified value.</summary>
		/// <param name="propertyNumber">The custom property field to save the value to.</param>
		/// <param name="newValue">The new value to save.</param>
		public void SetCustomProperty(int propertyNumber, object newValue)
		{
			if (CustomPropertyDefinitions != null)
			{
				//Get the custom property value.
				string custName = DataModel.CustomProperty.FIELD_PREPEND + propertyNumber.ToString("00");
				string setValue = null;

				try
				{
					setValue = newValue.ToDatabaseSerialization();
					this[custName] = setValue;
				}
				catch
				{
					//Cannot log because we are in DataModel which does not Reference Common assembly
					throw;
				}
			}
			else
			{
				//Cannot log because we are in DataModel which does not Reference Common assembly
				throw new InvalidOperationException("Unable to access CustomPropertyDefinitions");
			}
		}
	}

	/// <summary>Contains static extension properties and methods that can't be in the main class</summary>
	public static class CustomPropertyHelper
	{
		/// <summary>Generic cloning method for artifacts</summary>
		/// <typeparam name="T">The artifact type</typeparam>
		/// <param name="artifact">The artifact being cloned</param>
		/// <returns>The cloned copy</returns>
		public static ArtifactCustomProperty Clone(this ArtifactCustomProperty custProps)
		{
			//Create a new instance of the class
			ArtifactCustomProperty clonedArtifact = new ArtifactCustomProperty();

			//Since it's a subclass of artifact, can set all the values using the indexer
			//Make sure it can read/write values and also has a defined typecode (or nullable typecode)
			//That way we don't set any navigation properties
			foreach (KeyValuePair<string, PropertyInfo> kvp in custProps.Properties)
			{
				if (kvp.Value.CanRead && kvp.Value.CanWrite && (Type.GetTypeCode(kvp.Value.PropertyType) != TypeCode.Object || Type.GetTypeCode(Nullable.GetUnderlyingType(kvp.Value.PropertyType)) != TypeCode.Object))
				{
					clonedArtifact[kvp.Key] = custProps[kvp.Key];
				}
			}

			return clonedArtifact;
		}

	}
}
