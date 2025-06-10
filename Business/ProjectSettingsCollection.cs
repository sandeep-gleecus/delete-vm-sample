using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>Extends the hashtable class to encapsulate storage and retrieval of per-user project data in the database</summary>
    public class ProjectSettingsCollection : Hashtable
    {
        protected int projectId;
        protected int userId;
        protected string collectionName;

        /// <summary>
        /// Constructor method
        /// </summary>
        public ProjectSettingsCollection(int projectId, int userId, string collectionName)
            : base()
        {
            //Store the project and user this collection is stored for
            this.projectId = projectId;
            this.userId = userId;
            this.collectionName = collectionName;
        }

        /// <summary>
        /// Returns the current user id
        /// </summary>
        public int UserId
        {
            get
            {
                return this.userId;
            }
        }

        /// <summary>
        /// Returns the current project id
        /// </summary>
        public int ProjectId
        {
            get
            {
                return this.projectId;
            }
        }

        /// <summary>
        /// Persists the hashtable entries back to the database
        /// </summary>
        public void Save()
        {
            ProjectManager projectManager = new ProjectManager();

            //First get a list of the existing hashtable entries from the database
            ProjectCollection projectCollection = projectManager.RetrieveSettings(this.projectId, this.userId, this.collectionName);

            //Now iterate through the hashtable and compare with dataset
            foreach (DictionaryEntry entry in this)
            {
                //Get the entry key and values (convert to string and typecode)
                string entryKey = (string)entry.Key;
                object entryValue = entry.Value;
                int typeCode;
                string entryValueString = SerializeValue(entryValue, out typeCode);

                //See if the key exists in the entity collection
                bool keyFound = false;
                foreach (ProjectCollectionEntry projectCollectionEntry in projectCollection.Entries)
                {
                    if (projectCollectionEntry.EntryKey.ToLowerInvariant() == entryKey.ToLowerInvariant())
                    {
                        keyFound = true;
                        //Update the row if the values different
                        if (entryValueString != projectCollectionEntry.EntryValue)
                        {
                            projectManager.UpdateSetting(this.projectId, this.userId, this.collectionName, entryKey, entryValueString, typeCode);
                        }
                    }
                }

                //if the key was not found, then we need to do an insert
                if (!keyFound)
                {
                    projectManager.InsertSetting(this.projectId, this.userId, this.collectionName, entryKey, entryValueString, typeCode);
                }
            }

            //Finally if there is a dataset record with no matching key, then we need to delete the row
            foreach (ProjectCollectionEntry projectCollectionEntry in projectCollection.Entries)
            {
                //Get the key name from the database record
                string entryKey = projectCollectionEntry.EntryKey;
                if (this[entryKey] == null)
                {
                    projectManager.DeleteSetting(this.projectId, this.userId, this.collectionName, entryKey);
                }
            }
        }

        /// <summary>
        /// Converts the native object into a string and associated type-code
        /// </summary>
        /// <param name="entryValue">The native object value</param>
        /// <param name="typeCode">The type code of the native object [out]</param>
        /// <returns>The string representation of the object</returns>
        protected string SerializeValue(object entryValue, out int typeCode)
        {
            //See if we have one of our custom types that we need to handle
            if (entryValue.GetType() == typeof(DateRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.DateRange;
                return ((DateRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(DecimalRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.DecimalRange;
                return ((DecimalRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(EffortRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.EffortRange;
                return ((EffortRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(IntRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.IntRange;
                return ((IntRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(LongRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.LongRange;
                return ((LongRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(MultiValueFilter))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.MultiValueFilter;
                return ((MultiValueFilter)entryValue).ToString();
            }

            //Now handle the built-in typecodes

            //First we need to get the typecode of the object
            typeCode = (int)System.Type.GetTypeCode(entryValue.GetType());

            //Next we need to convert the value to string. Need to handle date-time differently
            if (typeCode == (int)TypeCode.DateTime)
            {
                return ((DateTime)entryValue).ToString("yyyyMMddTHHmmss");
            }
            else
            {
                return entryValue.ToString();
            }
        }

        /// <summary>
        /// Converts a string representation into the native object
        /// </summary>
        /// <param name="entryValue">The string version of the data</param>
        /// <param name="typeCode">The type of the object we want</param>
        /// <returns>The data in its native form</returns>
        protected object DeSerializeValue(string entryValue, int typeCode)
        {
            //First test the custom typecodes
            if (typeCode == (int)Common.Global.CustomTypeCodes.DateRange)
            {
                DateRange dateRange;
                DateRange.TryParse(entryValue, out dateRange);
                return dateRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.DecimalRange)
            {
                DecimalRange decimalRange;
                DecimalRange.TryParse(entryValue, out decimalRange);
                return decimalRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.EffortRange)
            {
                EffortRange effortRange;
                EffortRange.TryParse(entryValue, out effortRange);
                return effortRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.IntRange)
            {
                IntRange intRange;
                IntRange.TryParse(entryValue, out intRange);
                return intRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.LongRange)
            {
                LongRange longRange;
                LongRange.TryParse(entryValue, out longRange);
                return longRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.MultiValueFilter)
            {
                MultiValueFilter multiValueFilter;
                MultiValueFilter.TryParse(entryValue, out multiValueFilter);
                return multiValueFilter;
            }

            //Now the built-in typecodes
            if (typeCode == (int)TypeCode.Boolean)
            {
                return Boolean.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int32)
            {
                return Int32.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int16)
            {
                return Int16.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int64)
            {
                return Int64.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.DateTime)
            {
                return DateTime.ParseExact(entryValue, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                //Keep as a string value
                return entryValue;
            }
        }

        /// <summary>
        /// Loads the hashtable entries from the database
        /// </summary>
        public void Restore()
        {
            ProjectManager projectManager = new ProjectManager();
            ProjectCollection projectCollection = projectManager.RetrieveSettings(this.projectId, this.userId, this.collectionName);

            //Now populate the hashtable
            this.Clear();
			if (projectCollection != null)
			{
				foreach (ProjectCollectionEntry projectCollectionEntry in projectCollection.Entries)
				{
					string entryKey = projectCollectionEntry.EntryKey;
					string entryValue = projectCollectionEntry.EntryValue;
					int typeCode = projectCollectionEntry.EntryTypeCode;
					this.Add(entryKey, DeSerializeValue(entryValue, typeCode));
				}
			}
        }

		public void ClearSort(int projectId, int userId, string collectionName)
		{
			ProjectManager projectManager = new ProjectManager();

			//First get a list of the existing hashtable entries from the database
			ProjectCollection projectCollection = projectManager.RetrieveSettings(projectId, userId, collectionName);

			//Finally if there is a dataset record with no matching key, then we need to delete the row
			foreach (ProjectCollectionEntry projectCollectionEntry in projectCollection.Entries)
			{
				//Get the key name from the database record
				string entryKey = projectCollectionEntry.EntryKey;
				projectManager.DeleteSetting(projectId, userId, collectionName, entryKey);
			}
		}
    }
}
