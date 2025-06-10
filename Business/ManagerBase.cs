using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data.Objects;
using System.Data.Metadata.Edm;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using LinqToEdmx;
using LinqToEdmx.Map;
using LinqToEdmx.Model.Conceptual;
using System.Data.Objects.SqlClient;
using System.Data.EntityClient;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// The base class for all business objects (manager classes)
    /// </summary>
    public class ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ManagerBase::";

        //The following cannot be static since they are localized
        private SortedList<int, int> paginationOptions;
        protected SortedList<string, string> activeFlags;
        protected Dictionary<string, string> activeFlagsDic;

        protected static Edmx _edmx;

        //Constants
        private const string s_legalChars = "_@#$";
        public const int NoneFilterValue = -999;    //Represents a (None) filter in the Hierarchical Lookups

        //Database timeout for a long-running update of status
        protected const int SQL_COMMAND_TIMEOUT_CACHE_UPDATE = 180;

        #region Delegates

        /// <summary>
        /// Used for business managers to handle artifact-specific filters
        /// </summary>
        /// <param name="expressionList">The existing list of expressions</param>
        /// <param name="filter">The current filter</param>
        /// <param name="projectId">The current project (if applicable)</param>
        /// <param name="projectTemplateId">The current project template (if applicable)</param>
        /// <param name="p">The LINQ parameter</param>
        /// <param name="utcOffset">The current offset from UTC</param>
        /// <returns>True if handled, return False for the standard filter handling</returns>
        public delegate bool HandleSpecialFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset);

        /// <summary>
        /// Delegate used for tasks to report the status/progress of long-running tasks
        /// </summary>
        /// <param name="progress">The progress (0-100)</param>
        /// <param name="message">The message</param>
        public delegate void UpdateBackgroundProcessStatus(int progress, string message);

        #endregion

        protected static void EnsureValidEntityName(string name)
        {
            for (int i = 0; i < name.Length; ++i)
            {
                if (!Char.IsLetterOrDigit(name[i]) && s_legalChars.IndexOf(name[i]) == -1)
                {
                    throw new Exception("Entity names and property names cannot contain: " + name[i]);
                }
            }
        }

        #region Properties

        /// <summary>
        /// Returns the localized suffix for copied artifacts
        /// </summary>
        protected string CopiedArtifactNameSuffix
        {
            get
            {
                return " - " + GlobalResources.General.Global_Copy;
            }
        }

        #endregion

        #region Public functions

        /// <summary>
        /// Returns a sorted list of values to populate a pagination option lookup
        /// </summary>
        /// <returns>Sorted List containing pagination options</returns>
        public SortedList<int, int> GetPaginationOptions()
        {
            const string METHOD_NAME = "GetPaginationOptions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //If we don't have the pagination list populated, then create, otherwise just return
                if (paginationOptions == null)
                {
                    paginationOptions = new SortedList<int, int>();
					paginationOptions.Add(5, 5);
					paginationOptions.Add(15, 15);
                    paginationOptions.Add(30, 30);
                    paginationOptions.Add(50, 50);
                    paginationOptions.Add(100, 100);
                    paginationOptions.Add(250, 250);
                    paginationOptions.Add(500, 500);
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            return paginationOptions;
        }

        /// <summary>Returns the connection information for reporting to Admin page.</summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetConnectionString()
        {
            Dictionary<string, string> retList = new Dictionary<string, string>();

            try
            {
                //Get connection string items.
                string conStr = ((EntityConnection)new SpiraTestEntitiesEx().Connection).StoreConnection.ConnectionString;
                string[] bits = conStr.Split(';');
                foreach (string str in bits)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        string[] tags = str.Split('=');
                        if (tags.Length == 2)
                            retList.Add(tags[0].Replace('_', ' ').ToLowerInvariant().Trim(), tags[1].Trim());
                        else
                            retList.Add(retList.Count.ToString(), tags[0]);
                    }
                }

                //Get DB Server properties..
                using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
                {
                    SysInfoResult config = ct.ManagerBase_SysInfo().FirstOrDefault();
                    foreach (KeyValuePair<string, PropertyInfo> prop in config.Properties)
                    {
                        if (!prop.Key.ToLowerInvariant().Equals("properties"))
                        {
                            string propVal = config[prop.Key].ToSafeString();
                            if (!string.IsNullOrWhiteSpace(propVal))
                                retList.Add(prop.Key.ToLowerInvariant().Trim(), propVal);
                        }
                    }
                }
            }
            catch
            { }


            return retList;
        }

        #endregion

        #region Internal functions

        /// <summary>
        /// Gets the data type of the sort property
        /// </summary>
        /// <param name="sortProperty"></param>
        /// <returns></returns>
        protected static Type GetSortDataType<TElement>(string sortProperty)
        {
            //Make sure that the sort property is a member of the entity
            try
            {
                PropertyInfo propInfo = typeof(TElement).GetProperty(sortProperty);
                if (propInfo != null)
                {
                    return propInfo.PropertyType;
                }
                return null;
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// This function takes input strings and renders them safe to use in a SQL query
        /// </summary>
        /// <param name="input">Input string that needs encoding</param>
        /// <returns>SQL-safe string</returns>
        /// <remarks>The current implementation only handles quotes</remarks>
        public static string SqlEncode(string input)
        {
            return (input.Replace("'", "''"));
        }

        /// <summary>
        /// Returns a culture-invariant date-string for use in native SQL expressions
        /// </summary>
        /// <param name="dateTime">The date/time value</param>
        /// <returns>The formatted date-string for the database in question</returns>
        /// <remarks>Currently only supports SQL Server</remarks>
        public static string CultureInvariantDateTime(DateTime dateTime)
        {
            //We use the ODBC escaped timestamp format { ts 'yyyy-mm-dd hh:mm:ss[.fff] '}
            string dateTimeString = dateTime.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            return "{ts'" + dateTimeString + "'}";
        }

        /// <summary>
		/// Returns a sorted list of values to populate an Flag (Y/N) Lookup
		/// </summary>
		/// <returns>Sorted List containing flag values</returns>
		public SortedList<string, string> RetrieveFlagLookup()
		{
			const string METHOD_NAME = "RetrieveFlagLookup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the flag list populated, then create, otherwise just return
				if (activeFlags == null)
				{
					activeFlags = new SortedList<string, string>();
					activeFlags.Add("Y", GlobalResources.General.Global_Yes);
					activeFlags.Add("N", GlobalResources.General.Global_No);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return activeFlags;
		}

		/// <summary>
		/// Returns a dictionary of values to populate an Flag (Y/N) Lookup
		/// </summary>
		/// <returns>Sorted List containing flag values</returns>
		public Dictionary<string, string> RetrieveFlagLookupDictionary()
		{
			const string METHOD_NAME = "RetrieveFlagLookupDictionary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the flag list populated, then create, otherwise just return
				if (activeFlagsDic == null)
				{
					activeFlagsDic = new Dictionary<string, string>();
					activeFlagsDic.Add("Y", GlobalResources.General.Global_Yes);
					activeFlagsDic.Add("N", GlobalResources.General.Global_No);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return activeFlagsDic;
		}

        /// <summary>
        /// Converts a sort property and direction into the corresponding LINQ expression
        /// </summary>
        /// <typeparam name="TElement">The entity type</typeparam>
        /// <typeparam name="DataType">The type of the property being sorted</typeparam>
        /// <param name="sortProperty">The property of the entity</param>
        /// <param name="sortAscending">Should we sort ascending or descending</param>
        /// <param name="lamdaParamName">The lamda parameter name being used (e.g. if the entity is using a => a.x then the parameter name is "a")</param>
        /// <returns>the sort expression</returns>
        protected internal static SortEntry<TElement, DataType> ConvertSort<TElement,DataType>(string sortProperty, bool sortAscending, string lamdaParamName)
        {
            const string METHOD_NAME = "ConvertSort<TElement>";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create the (e => e.SortProperty) expression and set on the sort object
                ParameterExpression param = Expression.Parameter(typeof(TElement), lamdaParamName);
                MemberExpression memberExpression = LambdaExpression.PropertyOrField(param, sortProperty);

                SortEntry<TElement, DataType> sortEntry = new SortEntry<TElement, DataType>();
                sortEntry.Direction = (sortAscending) ? SortDirection.Ascending : SortDirection.Descending;
                sortEntry.Expression = BinaryExpression.Lambda<Func<TElement, DataType>>(memberExpression, new List<ParameterExpression> { param });

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return sortEntry;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

                throw;
            }
        }

		/// <summary>
		/// Returns an entity set by its name
		/// </summary>
		/// <param name="entitySetName">The name of the entity set</param>
		/// <returns>The entity set meta data</returns>
		public static EntitySet GetEntitySet(string entitySetName)
		{
			EntitySet entitySet = null;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var conceptualModelContainer = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
				entitySet = conceptualModelContainer.GetEntitySetByName(entitySetName, false);
			}
			return entitySet;
		}

		/// <summary>
		/// Returns the type of an entity that is in an entity set
		/// </summary>
		/// <param name="entitySetName">The name of the entity set</param>
		/// <returns>The type of entities in the set</returns>
		public static Type GetEntitySetItemType(string entitySetName)
		{
			const string METHOD_NAME = "GetEntitySetItemType";

			Type type = null;
			EntitySet entitySet = GetEntitySet(entitySetName);
			if (entitySet == null)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Unable to find entity set '" + entitySetName + "'");
			}
			else
			{
				string typeName = entitySet.ElementType.FullName;
				type = DataModel.ReportableEntities.ReportableEntitiesManager.GetEntityType(typeName);
			}

			return type;
		}

		/// <summary>
		/// Returns the meta-data for a specific entity property
		/// </summary>
		/// <param name="entityType">The type of entity</param>
		/// <param name="propertyName">The property name</param>
		/// <returns>The meta-data of the property</returns>
		public static EntityProperty GetPropertyInfo(Type entityType, string propertyName)
        {
            EntityProperty entityPropertyInfo = null;

            if (_edmx == null)
            {
                _edmx = Edmx.Load(@"res://DataModel/SpiraDataModel.csdl|res://DataModel/SpiraDataModel.ssdl|res://DataModel/SpiraDataModel.msl");
            }

            try
            {
                //Find the entry for this entity type
                LinqToEdmx.Model.Conceptual.EntityType entityTypeItem = _edmx.GetItems<LinqToEdmx.Model.Conceptual.EntityType>().FirstOrDefault(e => e.Name == entityType.Name);
                if (entityTypeItem != null)
                {
                    //Now we need to find the property
                    EntityProperty entityProperty = entityTypeItem.Properties.FirstOrDefault(p => p.Name == propertyName);
                    if (entityProperty != null)
                    {
                        entityPropertyInfo = entityProperty;
                    }
                }
            }
            catch (KeyNotFoundException)
            { 
                //Ignore, seems to sporadically get thrown
            }

            return entityPropertyInfo;
        }

        /// <summary>
        /// Returns the physical database table/view name for a specific entity
        /// </summary>
        /// <param name="entityType">The type of entity</param>
        /// <returns>The database table/view name</returns>
        protected internal string GetEntityMapping(Type entityType)
        {
            string tableName = "";

            if (_edmx == null)
            {
                _edmx = Edmx.Load(@"res://DataModel/SpiraDataModel.csdl|res://DataModel/SpiraDataModel.ssdl|res://DataModel/SpiraDataModel.msl");
            }

            //Find the mapping entry for this entity type
            EntityTypeMapping entityTypeMapping = _edmx.GetItems<EntityTypeMapping>().FirstOrDefault(m => m.TypeName == entityType.FullName);
            if (entityTypeMapping != null)
            {
                tableName = entityTypeMapping.MappingFragments[0].StoreEntitySet;
            }

            return tableName;
        }

        /// <summary>
        /// Returns the physical database table/view name for a specific entity
        /// </summary>
        /// <param name="entityName">The name of entity (not the full namespace)</param>
        /// <returns>The database table/view name</returns>
        protected internal string GetEntityMapping(string entityName)
        {
            string tableName = "";

            if (_edmx == null)
            {
                _edmx = Edmx.Load(@"res://DataModel/SpiraDataModel.csdl|res://DataModel/SpiraDataModel.ssdl|res://DataModel/SpiraDataModel.msl");
            }

            //The entity type is in the DataModel namespace, so need to prefix the type
            string entityTypeFullName = typeof(Entity).Namespace + "." + entityName;

            //Find the mapping entry for this entity type
            EntityTypeMapping entityTypeMapping = _edmx.GetItems<EntityTypeMapping>().FirstOrDefault(m => m.TypeName == entityTypeFullName);
            if (entityTypeMapping != null)
            {
                tableName = entityTypeMapping.MappingFragments[0].StoreEntitySet;
            }

            return tableName;
        }

        /// <summary>
        /// Returns the physical database column name for a specific entity property
        /// </summary>
        /// <param name="entityType">The type of entity</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>The database column name</returns>
        protected internal string GetPropertyMapping(Type entityType, string propertyName)
        {
            string columnName = "";

            if (_edmx == null)
            {
                _edmx = Edmx.Load(@"res://DataModel/SpiraDataModel.csdl|res://DataModel/SpiraDataModel.ssdl|res://DataModel/SpiraDataModel.msl");
            }

            //Find the mapping entry for this entity type
            EntityTypeMapping entityTypeMapping = _edmx.GetItems<EntityTypeMapping>().FirstOrDefault(m => m.TypeName == entityType.FullName);
            if (entityTypeMapping != null)
            {
                //Now we need to find the property
                foreach (MappingFragment mappingFragment in entityTypeMapping.MappingFragments)
                {
                    ScalarProperty scalarProperty = mappingFragment.ScalarProperties.FirstOrDefault(s => s.Name == propertyName);
                    if (scalarProperty != null)
                    {
                        columnName = scalarProperty.ColumnName;
                    }
                }
            }

            return columnName;
        }

        /// <summary>
        /// Creates the generic portion of each business object's filter clause when we're using native SQL and stored procedures
        /// for retrieval (typically used for hierarchical artifacts such as requirements)
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="projectTemplateId">The id of the current project template</param>
        /// <param name="filterProperty">The entity property being filtered on</param>
        /// <param name="filterValue">The value we're filtering on</param>
        /// <param name="filterColumn">The database column name that maps to the filter property</param>
        /// <param name="tablePrefix">The table alias prefix used in the base SQL query</param>
        /// <returns>The filter clause corresponding to the passed in filters</returns>
        /// <param name="utcOffset">The offset from UTC time for the current user's timezone</param>
        /// <param name="propertyInfo">The entity property being filtered on's meta-data</param>
        /// <param name="artifactType">The type of artifact the filter is for (pass None) if it's a compound and doesn't use custom properties</param>
        protected string CreateFilterClauseGeneric(int projectId, int projectTemplateId, string filterProperty, object filterValue, string filterColumn, string tablePrefix, EntityProperty propertyInfo, Artifact.ArtifactTypeEnum artifactType, double utcOffset)
        {
            //Make sure we actually have a filter column
            if (String.IsNullOrEmpty(filterColumn))
            {
                return "";
            }

            //See if we have a standard or custom filter because they work differently
            string filtersClause = "";
            int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(filterProperty);
            if (customPropertyNumber.HasValue)
            {
                //Handle the custom property types
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, artifactType, customPropertyNumber.Value, false);
                if (customProperty != null)
                {
                    switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
                    {
                        case CustomProperty.CustomPropertyTypeEnum.Text:
                            {
                                filtersClause += "AND " + tablePrefix + "." + filterColumn + " LIKE N'%" + SqlEncode((string)filterValue) + "%' ";
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Boolean:
                            {
                                //Check to see if we have the (None) case
                                if (filterValue is String && ((string)filterValue) == MultiValueFilter.NoneFilterValue.ToString())
                                {
                                    filtersClause += "AND " + tablePrefix + "." + filterColumn + " IS NULL ";
                                }
                                else
                                {
                                    //See if we are storing the filter value as a boolean or Y/N
                                    if (filterValue is bool)
                                    {
                                        bool boolValue = (bool)filterValue;
                                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " = '" + SqlEncode(boolValue.ToDatabaseSerialization()) + "' ";
                                    }
                                    else if (filterValue is String)
                                    {
                                        string filterValueString = (string)filterValue;
                                        bool boolValue = (filterValueString == "Y");
                                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " = '" + SqlEncode(boolValue.ToDatabaseSerialization()) + "' ";
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Date:
                            {
                                //Get the whole number of hours and minutes that the UTC offset is
                                int utcOffsetHours = (int)Math.Truncate(utcOffset);
                                double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
                                int utcOffsetMins = (int)(utcOffsetFraction * (double)60);

                                //Handle the special date-range type. 
                                DateRange dateRange = (DateRange)filterValue;
                                if (dateRange.StartDate.HasValue)
                                {
                                    //See if we need to consider the time component
                                    if (dateRange.ConsiderTimes)
                                    {
                                        filtersClause += String.Format("AND dbo.FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME({0},{1},{2},{3},{4}) = 1  ", tablePrefix + "." + filterColumn, CultureInvariantDateTime(dateRange.StartDate.Value), utcOffsetHours, utcOffsetMins, (dateRange.ConsiderTimes) ? 1 : 0);
                                    }
                                    else
                                    {
                                        DateTime startDateCutoff = dateRange.StartDate.Value.Date;
                                        filtersClause += String.Format("AND dbo.FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME({0},{1},{2},{3},{4}) = 1 ", tablePrefix + "." + filterColumn, CultureInvariantDateTime(startDateCutoff), utcOffsetHours, utcOffsetMins, (dateRange.ConsiderTimes) ? 1 : 0);
                                    }
                                }
                                if (dateRange.EndDate.HasValue)
                                {
                                    //See if we need to consider the time component
                                    if (dateRange.ConsiderTimes)
                                    {
                                        filtersClause += String.Format("AND dbo.FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME({0},{1},{2},{3},{4}) = 1", tablePrefix + "." + filterColumn, CultureInvariantDateTime(dateRange.EndDate.Value), utcOffsetHours, utcOffsetMins, (dateRange.ConsiderTimes) ? 1 : 0);
                                    }
                                    else
                                    {
                                        DateTime endDateCutoff = dateRange.EndDate.Value.Date;
                                        filtersClause += String.Format("AND dbo.FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME({0},{1},{2},{3},{4}) = 1 ", tablePrefix + "." + filterColumn, CultureInvariantDateTime(endDateCutoff), utcOffsetHours, utcOffsetMins, (dateRange.ConsiderTimes) ? 1 : 0);
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Decimal:
                            {
                                decimal filterValueDecimal;
                                if (filterValue is DecimalRange)
                                {
                                    //Handle the special decimal-range type
                                    DecimalRange decimalRange = (DecimalRange)filterValue;
                                    if (decimalRange.MinValue.HasValue)
                                    {
                                        filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_GREATER_THAN_DECIMAL(" + tablePrefix + "." + filterColumn + ", " + decimalRange.MinValue.Value + ") = 1 ";
                                    }
                                    if (decimalRange.MaxValue.HasValue)
                                    {
                                        filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_LESS_THAN_DECIMAL(" + tablePrefix + "." + filterColumn + ", " + decimalRange.MaxValue.Value + ") = 1 ";
                                    }
                                }
                                else if (filterValue is Decimal)
                                {
                                    filterValueDecimal = (decimal)filterValue;
                                    filtersClause += "AND  dbo.FN_CUSTOM_PROPERTY_EQUALS_DECIMAL(" + tablePrefix + "." + filterColumn + ", " + filterValueDecimal + ") = 1 ";
                                }
                                else if (filterValue is String && Decimal.TryParse((string)filterValue, out filterValueDecimal))
                                {
                                    filtersClause += "AND  dbo.FN_CUSTOM_PROPERTY_EQUALS_DECIMAL(" + tablePrefix + "." + filterColumn + ", " + filterValueDecimal + ") = 1 ";
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Integer:
                            {
                                int filterValueInt;
                                if (filterValue is IntRange)
                                {
                                    //Handle the special integer-range type
                                    IntRange intRange = (IntRange)filterValue;
                                    if (intRange.MinValue.HasValue)
                                    {
                                        filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_GREATER_THAN_INT(" + tablePrefix + "." + filterColumn + ", " + intRange.MinValue.Value + ") = 1 ";
                                    }
                                    if (intRange.MaxValue.HasValue)
                                    {
                                        filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_LESS_THAN_INT(" + tablePrefix + "." + filterColumn + ", " + intRange.MaxValue.Value + ") = 1 ";
                                    }
                                }
                                else if (filterValue is Int32)
                                {
                                    filterValueInt = (int)filterValue;
                                    filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(" + tablePrefix + "." + filterColumn + ", " + filterValueInt + ") = 1 ";
                                }
                                else if (filterValue is String && Int32.TryParse((string)filterValue, out filterValueInt))
                                {
                                    filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(" + tablePrefix + "." + filterColumn + ", " + filterValueInt + ") = 1 ";
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.List:
                        case CustomProperty.CustomPropertyTypeEnum.User:
                            {
                                int intValue;
                                if (filterValue is MultiValueFilter)
                                {
                                    //Handle the special multi-value lookup type
                                    MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;

                                    if (multiValueFilter.IsNone)
                                    {
                                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " IS NULL ";
                                    }
                                    else if (multiValueFilter.Values.Count > 0)
                                    {
                                        //Since custom properties are serialized as strings, need to explicitly convert each value in the
                                        //multivaluefilter to serialized custom property format rather than just using the built-in ToString().
                                        string inClause = "";
                                        foreach (int value in multiValueFilter.Values)
                                        {
                                            //We have to include it twice, once with default
                                            //serialization, and once with the new zero-padded serialization
                                            //Better performance than splitting the string and doing the CAST for each one
                                            if (inClause == "")
                                            {
                                                inClause = "'" + value.ToString() + "'";
                                            }
                                            else
                                            {
                                                inClause += ",'" + value.ToString() + "'";
                                            }
                                            if (inClause == "")
                                            {
                                                inClause = "'" + value.ToDatabaseSerialization() + "'";
                                            }
                                            else
                                            {
                                                inClause += ",'" + value.ToDatabaseSerialization() + "'";
                                            }
                                        }
                                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " IN (" + inClause + ") ";
                                    }

                                }
                                else if (filterValue is Int32)
                                {
                                    filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(" + tablePrefix + "." + filterColumn + ", " + (int)filterValue + ") = 1 ";
                                }
                                else if (Int32.TryParse((string)filterValue, out intValue))
                                {
                                    filtersClause += "AND dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(" + tablePrefix + "." + filterColumn + ", " + intValue + ") = 1 ";
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.MultiList:
                            {
                                int intValue;
                                if (filterValue is MultiValueFilter)
                                {
                                    //Handle the special multi-value lookup type
                                    MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;

                                    if (multiValueFilter.IsNone)
                                    {
                                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " IS NULL ";
                                    }
                                    else if (multiValueFilter.Values.Count > 0)
                                    {
                                        //Since custom properties are serialized as strings, need to explicitly convert each value in the
                                        //multivaluefilter to serialized custom property format rather than just using the built-in ToString().
                                        string orClause = "";
                                        foreach (int value in multiValueFilter.Values)
                                        {
                                            //We have to include it twice, once with default
                                            //serialization, and once with the new zero-padded serialization
                                            //Better performance than splitting the string and doing the CAST for each one
                                            string serializedValue = value.ToDatabaseSerialization();
                                            if (orClause == "")
                                            {
                                                orClause = "'" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' ";
                                            }
                                            else
                                            {
                                                orClause += "OR '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' ";
                                            }
                                            serializedValue = value.ToString();
                                            if (orClause == "")
                                            {
                                                orClause = "'" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' ";
                                            }
                                            else
                                            {
                                                orClause += "OR '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' ";
                                            }
                                        }
                                        filtersClause += "AND (" + orClause + ") ";
                                    }

                                }
                                else if (filterValue is Int32)
                                {
                                    //We have to include it twice, once with default
                                    //serialization, and once with the new zero-padded serialization
                                    string serializedValue1 = ((int)filterValue).ToDatabaseSerialization();
                                    string serializedValue2 = ((int)filterValue).ToString();
                                    filtersClause += "AND ('" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue1) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' OR " +
                                        "'" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue2) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%') ";
                                }
                                else if (Int32.TryParse((string)filterValue, out intValue))
                                {
                                    //We have to include it twice, once with default
                                    //serialization, and once with the new zero-padded serialization
                                    string serializedValue1 = intValue.ToDatabaseSerialization();
                                    string serializedValue2 = intValue.ToString();
                                    filtersClause += "AND ('" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue1) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%' OR " +
                                        "'" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' + " + tablePrefix + "." + filterColumn + " + '" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "' LIKE '%" + DatabaseExtensions.FORMAT_LIST_SEPARATOR + SqlEncode(serializedValue2) + DatabaseExtensions.FORMAT_LIST_SEPARATOR + "%') ";
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                //Handle standard field types
                if (filterValue is DateRange)
                {
                    //Get the whole number of hours and minutes that the UTC offset is
                    int utcOffsetHours = (int)Math.Truncate(utcOffset);
                    double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
                    int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

                    //Handle the special date-range type
                    DateRange dateRange = (DateRange)filterValue;
                    if (dateRange.StartDate.HasValue)
                    {
                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            filtersClause += "AND DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) >= " + CultureInvariantDateTime(dateRange.StartDate.Value) + " ";
                        }
                        else
                        {
                            filtersClause += "AND CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) AS FLOAT))AS DATETIME) >= " + CultureInvariantDateTime(dateRange.StartDate.Value.Date) + " ";
                        }
                    }
                    if (dateRange.EndDate.HasValue)
                    {
                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            filtersClause += "AND DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) <= " + CultureInvariantDateTime(dateRange.EndDate.Value) + " ";
                        }
                        else
                        {
                            filtersClause += "AND CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) AS FLOAT))AS DATETIME) <= " + CultureInvariantDateTime(dateRange.EndDate.Value.Date) + " ";
                        }
                    }
                }
                else if (filterValue is DecimalRange)
                {
                    //Handle the special decimal-range type
                    DecimalRange decimalRange = (DecimalRange)filterValue;
                    if (decimalRange.MinValue.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " >= " + decimalRange.MinValue.Value + " ";
                    }
                    if (decimalRange.MaxValue.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " <= " + decimalRange.MaxValue.Value + " ";
                    }
                }
                else if (filterValue is EffortRange)
                {
                    //Handle the special decimal-range type
                    EffortRange effortRange = (EffortRange)filterValue;
                    if (effortRange.MinValueInMinutes.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " >= " + effortRange.MinValueInMinutes + " ";
                    }
                    if (effortRange.MaxValueInMinutes.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " <= " + effortRange.MaxValueInMinutes + " ";
                    }
                }
                else if (filterValue is IntRange)
                {
                    //Handle the special integer-range type
                    IntRange intRange = (IntRange)filterValue;
                    if (intRange.MinValue.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " >= " + intRange.MinValue.Value + " ";
                    }
                    if (intRange.MaxValue.HasValue)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " <= " + intRange.MaxValue.Value + " ";
                    }
                }
                else if (filterValue is MultiValueFilter)
                {
                    //Handle the special multi-value lookup type
                    MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;

                    if (multiValueFilter.IsNone)
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " IS NULL ";
                    }
                    else if (!String.IsNullOrEmpty(multiValueFilter.ToString()))
                    {
                        filtersClause += "AND " + tablePrefix + "." + filterColumn + " IN (" + multiValueFilter.ToString() + ") ";
                    }
                }
                else
                {
                    //Handle string properties
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (filterColumn == "NAME")
                        {
							//If we have the name field, need to also search description (special case), can be disabled in a setting
							//for performance reasons when you have lots of items
							ProjectSettings settings = new ProjectSettings(projectId);
							if (settings.FilterNameAndDescription)
							{
								filtersClause += "AND (" + tablePrefix + "." + filterColumn + " LIKE N'%" + SqlEncode((string)filterValue) + "%' OR " + tablePrefix + ".DESCRIPTION LIKE N'%" + SqlEncode((string)filterValue) + "%') ";
							}
							else
							{
								filtersClause += "AND " + tablePrefix + "." + filterColumn + " LIKE N'%" + SqlEncode((string)filterValue) + "%' ";
							}
						}
                        else if (filterColumn == "FILENAME")
                        {
                            //If we have the filename field, need to also search tags and description (special case)
                            filtersClause += "AND (" + tablePrefix + ".FILENAME LIKE N'%" + SqlEncode((string)filterValue) + "%' OR " + tablePrefix + ".TAGS LIKE N'%" + SqlEncode((string)filterValue) + "%' OR " + tablePrefix + ".DESCRIPTION LIKE N'%" + SqlEncode((string)filterValue) + "%') ";
                        }
                        else
                        {
                            filtersClause += "AND " + tablePrefix + "." + filterColumn + " LIKE N'%" + SqlEncode((string)filterValue) + "%' ";
                        }
                    }
                    //Handle numeric values
                    if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                    {
                        //numeric values entered in text boxes are actually typed as strings
                        //so we need to convert if that case
                        int filterIntValue = -1;
                        if (filterValue.GetType() == typeof(int))
                        {
                            filterIntValue = (int)filterValue;
                        }
                        else if (filterValue.GetType() == typeof(int?))
                        {
                            int? nullableInt = (int?)filterValue;
                            if (nullableInt.HasValue)
                            {
                                filterIntValue = nullableInt.Value;
                            }
                        }
                        else
                        {
                            filterIntValue = Int32.Parse((string)filterValue);
                        }
                        //See if we have the special case of a (None) filter
                        if (filterIntValue == NoneFilterValue)
                        {
                            filtersClause += "AND " + tablePrefix + "." + filterColumn + " IS NULL ";
                        }
                        else
                        {
                            filtersClause += "AND " + tablePrefix + "." + filterColumn + " = " + filterIntValue.ToString() + " ";
                        }
                    }
                }
            }
            return filtersClause;
        }

        /// <summary>
        /// Creates the filter LINQ expression for a custom property
        /// </summary>
        /// <param name="expressionList">The list of filter LINQ expressions to add to</param>
        /// <param name="memberExpression">The property member expression (e.g. p.FieldName)</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="utcOffset">The current timezone offset from UTC</param>
        /// <param name="filterValue">The value we're filtering by</param>
        /// <param name="artifactType">The type of artifact being filtered on</param>
        /// <param name="customPropertyNumber">The custom property number</param>
        /// <param name="projectTemplateId">The id of the project template</param>
        protected static void CreateFilterEntryExpression(int projectId, int projectTemplateId, int customPropertyNumber, Artifact.ArtifactTypeEnum artifactType, object filterValue, double utcOffset, MemberExpression memberExpression, List<Expression> expressionList)
        {
            CustomPropertyManager customPropertyManager = new CustomPropertyManager();
            CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, artifactType, customPropertyNumber, false);
            if (customProperty != null)
            {
                switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
                {
                    case CustomProperty.CustomPropertyTypeEnum.Text:
                        {
                            if (filterValue is String)
                            {
                                string filterValueString = (string)filterValue;
                                //For text custom properties, use the standard keyword matcher, where we splt the filter value
                                //into separate keywords unless joined by quotes
                                MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                Expression expressionEntry = Expression.Call(memberExpression, containsMethod, Expression.Constant(filterValueString));
                                expressionList.Add(expressionEntry);
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.Boolean:
                        {
                            //Check to see if we have the (None) case
                            if (filterValue is String && ((string)filterValue) == MultiValueFilter.NoneFilterValue.ToString())
                            {
                                Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(null));
                                expressionList.Add(expressionEntry);
                            }
                            else
                            {
                                //See if we are storing the filter value as a boolean or Y/N
                                if (filterValue is bool)
                                {
                                    string filterValueString = ((bool)filterValue) ? "Y" : "N";
                                    Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueString));
                                    expressionList.Add(expressionEntry);
                                }
                                else if (filterValue is String)
                                {
                                    string filterValueString = (string)filterValue;
                                    Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueString));
                                    expressionList.Add(expressionEntry);
                                }
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.Date:
                        {
                            //Get the whole number of hours and minutes that the UTC offset is
                            int utcOffsetHours = (int)Math.Truncate(utcOffset);
                            double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
                            int utcOffsetMins = (int)(utcOffsetFraction * (double)60);

                            //Handle the special date-range type. 
                            DateRange dateRange = (DateRange)filterValue;
                            if (dateRange.StartDate.HasValue)
                            {
                                //See if we need to consider the time component
                                if (dateRange.ConsiderTimes)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyGreaterThanOrEqualDateTime", null, memberExpression, Expression.Constant(dateRange.StartDate.Value), Expression.Constant(utcOffsetHours), Expression.Constant(utcOffsetMins), Expression.Constant(dateRange.ConsiderTimes));
                                    expressionList.Add(expressionEntry);
                                }
                                else
                                {
                                    DateTime startDateCutoff = dateRange.StartDate.Value.Date;
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyGreaterThanOrEqualDateTime", null, memberExpression, Expression.Constant(startDateCutoff), Expression.Constant(utcOffsetHours), Expression.Constant(utcOffsetMins), Expression.Constant(dateRange.ConsiderTimes));
                                    expressionList.Add(expressionEntry);
                                }
                            }
                            if (dateRange.EndDate.HasValue)
                            {
                                //See if we need to consider the time component
                                if (dateRange.ConsiderTimes)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyLessThanOrEqualDateTime", null, memberExpression, Expression.Constant(dateRange.EndDate.Value), Expression.Constant(utcOffsetHours), Expression.Constant(utcOffsetMins), Expression.Constant(dateRange.ConsiderTimes));
                                    expressionList.Add(expressionEntry);
                                }
                                else
                                {
                                    DateTime endDateCutoff = dateRange.EndDate.Value.Date;
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyLessThanOrEqualDateTime", null, memberExpression, Expression.Constant(endDateCutoff), Expression.Constant(utcOffsetHours), Expression.Constant(utcOffsetMins), Expression.Constant(dateRange.ConsiderTimes));
                                    expressionList.Add(expressionEntry);
                                }
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.Decimal:
                        {
                            decimal filterValueDecimal;
                            if (filterValue is DecimalRange)
                            {
                                //Handle the special decimal-range type
                                DecimalRange decimalRange = (DecimalRange)filterValue;
                                if (decimalRange.MinValue.HasValue)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyGreaterThanOrEqualDecimal", null, memberExpression, Expression.Constant(decimalRange.MinValue.Value));
                                    expressionList.Add(expressionEntry);
                                }
                                if (decimalRange.MaxValue.HasValue)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyLessThanOrEqualDecimal", null, memberExpression, Expression.Constant(decimalRange.MaxValue.Value));
                                    expressionList.Add(expressionEntry);
                                }

                            }
                            else if (filterValue is Decimal)
                            {
                                filterValueDecimal = (decimal)filterValue;
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsDecimal", null, memberExpression, Expression.Constant(filterValueDecimal));
                                expressionList.Add(expressionEntry);
                            }
                            else if (filterValue is String && Decimal.TryParse((string)filterValue, out filterValueDecimal))
                            {
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsDecimal", null, memberExpression, Expression.Constant(filterValueDecimal));
                                expressionList.Add(expressionEntry);
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.Integer:
                        {
                            int filterValueInt;
                            if (filterValue is IntRange)
                            {
                                //Handle the special integer-range type
                                IntRange intRange = (IntRange)filterValue;
                                if (intRange.MinValue.HasValue)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyGreaterThanOrEqualInt", null, memberExpression, Expression.Constant(intRange.MinValue.Value));
                                    expressionList.Add(expressionEntry);
                                }
                                if (intRange.MaxValue.HasValue)
                                {
                                    Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyLessThanOrEqualInt", null, memberExpression, Expression.Constant(intRange.MaxValue.Value));
                                    expressionList.Add(expressionEntry);
                                }
                            }
                            else if (filterValue is Int32)
                            {
                                filterValueInt = (int)filterValue;
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsInt", null, memberExpression, Expression.Constant(filterValueInt));
                                expressionList.Add(expressionEntry);
                            }
                            else if (filterValue is String && Int32.TryParse((string)filterValue, out filterValueInt))
                            {
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsInt", null, memberExpression, Expression.Constant(filterValueInt));
                                expressionList.Add(expressionEntry);
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.List:
                    case CustomProperty.CustomPropertyTypeEnum.User:
                        {
                            int intValue;
                            if (filterValue is MultiValueFilter)
                            {
                                //Handle the special multi-value lookup type
                                MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;

                                //Check for the special case when we're filtering on 'None'
                                if (multiValueFilter.IsNone)
                                {
                                    Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(null));
                                    expressionList.Add(expressionEntry);
                                }
                                else if (multiValueFilter.Values.Count > 0)
                                {
                                    //Since custom properties are serialized as strings, need to explicitly convert each value in the
                                    //multivaluefilter to serialized custom property format rather than just using the built-in ToString().
                                    List<string> stringValueList = new List<string>();
                                    foreach (int value in multiValueFilter.Values)
                                    {
                                        //We have to include it twice, once with default
                                        //serialization, and once with the new zero-padded serialization
                                        //Better performance than splitting the string and doing the CAST for each one
                                        stringValueList.Add(value.ToString());
                                        stringValueList.Add(value.ToDatabaseSerialization());
                                    }
                                    MethodInfo containsMethod = typeof(List<string>).GetMethod("Contains");
                                    Expression expressionEntry = Expression.Call(Expression.Constant(stringValueList), containsMethod, memberExpression);
                                    expressionList.Add(expressionEntry);
                                 }

                            }
                            else if (filterValue is Int32)
                            {
                                intValue = (int)filterValue;
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsInt", null, memberExpression, Expression.Constant(intValue));
                                expressionList.Add(expressionEntry);
                            }
                            else if (Int32.TryParse((string)filterValue, out intValue))
                            {
                                Expression expressionEntry = Expression.Call(typeof(Functions), "CustomPropertyEqualsInt", null, memberExpression, Expression.Constant(intValue));
                                expressionList.Add(expressionEntry);
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.MultiList:
                        {
                            int intValue;
                            if (filterValue is MultiValueFilter)
                            {
                                //Handle the special multi-value lookup type
                                MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;

                                if (multiValueFilter.IsNone)
                                {
                                    Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(null));
                                    expressionList.Add(expressionEntry);
                                }
                                else if (multiValueFilter.Values.Count > 0)
                                {
                                    //Since custom properties are serialized as strings, need to explicitly convert each value in the
                                    //multivaluefilter to serialized custom property format rather than just using the built-in ToString().
                                    List<Expression> orList = new List<Expression>();
                                    foreach (int value in multiValueFilter.Values)
                                    {
                                        //We have to add the value twice, once in the new padded format, once in normal format
                                        //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                        //Padded
                                        {
                                            MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                            Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                            string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + value.ToDatabaseSerialization() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                            MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                            Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                            orList.Add(expressionEntry);
                                        }
                                        //Normal
                                        {
                                            MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                            Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                            string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + value.ToString() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                            MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                            Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                            orList.Add(expressionEntry);
                                        }
                                    }
                                    Expression orAggregate = orList.Aggregate(Expression.Or);
                                    expressionList.Add(orAggregate);
                                }

                            }
                            else if (filterValue is Int32)
                            {
                                //We have to add the value twice, once in the new padded format, once in normal format
                                List<Expression> orList = new List<Expression>();
                                //Padded
                                {
                                    //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                    MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                    Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                    string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + ((int)filterValue).ToDatabaseSerialization() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                    MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                    Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                    orList.Add(expressionEntry);
                                }
                                //Normal
                                {
                                    //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                    MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                    Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                    string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + ((int)filterValue).ToString() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                    MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                    Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                    orList.Add(expressionEntry);
                                }
                                Expression orAggregate = orList.Aggregate(Expression.Or);
                                expressionList.Add(orAggregate);
                            }
                            else if (Int32.TryParse((string)filterValue, out intValue))
                            {
                                //We have to add the value twice, once in the new padded format, once in normal format
                                List<Expression> orList = new List<Expression>();
                                //Padded
                                {
                                    //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                    MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                    Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                    string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + intValue.ToDatabaseSerialization() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                    MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                    Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                }
                                //Normal
                                {
                                    //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                    MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                    Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                    string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + intValue.ToDatabaseSerialization() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                    MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                    Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                }
                                Expression orAggregate = orList.Aggregate(Expression.Or);
                                expressionList.Add(orAggregate);
                            }
                        }
                        break;
                }
            }
        }

		/// <summary>
		/// Creates the filter LINQ expression for a standard field
		/// </summary>
		/// <param name="projectId">The id of the project (optional)</param>
		/// <param name="expressionList">The list of filter LINQ expressions to add to</param>
		/// <param name="memberExpression">The property member expression (e.g. p.FieldName)</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="propertyName">The name of the property being filtered on</param>
		/// <param name="utcOffset">The current timezone offset from UTC</param>
		/// <param name="usingEF">Are we using EF (as opposed to straight LINQ)</param>
		/// <param name="p">The parameter</param>
		/// <param name="filterValue">The value we're filtering by</param>
		protected static void CreateFilterEntryExpression(int? projectId, string propertyName, object filterValue, double utcOffset, MemberExpression memberExpression, List<Expression> expressionList, ParameterExpression p, bool usingEF = true)
        {
            //See if we have a special composite typed filter
            if (filterValue is DateRange)
            {
                //Get the whole number of hours and minutes that the UTC offset is
                int utcOffsetHours = (int)Math.Truncate(utcOffset);
                double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
                int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

                if (usingEF)
                {
                    //The following EF/Linq code does the equivalent of the following:
                    //if (dateRange.ConsiderTimes)
                    //{
                    //    filtersClause += "AND DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) >= " + CultureInvariantDateTime(dateRange.StartDate.Value) + " ";
                    //}
                    //else
                    //{
                    //    DATEADD(dd, 0, DATEDIFF(dd, 0, @your_date))
                    //    filtersClause += "AND CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + "," + tablePrefix + "." + filterColumn + ")) AS FLOAT))AS DATETIME) >= " + CultureInvariantDateTime(dateRange.StartDate.Value.Date) + " ";
                    //}

                    //Handle the special date-range type
                    MethodInfo addHoursMethod = typeof(EntityFunctions).GetMethod("AddHours", new Type[] { typeof(DateTime?), typeof(int?) });
                    DateRange dateRange = (DateRange)filterValue;
                    if (dateRange.StartDate.HasValue)
                    {
                        MethodCallExpression offsetHandlingHours = Expression.Call(addHoursMethod, Expression.Convert(memberExpression, typeof(DateTime?)), Expression.Constant(utcOffsetHours, typeof(int?)));
                        MethodCallExpression offsetHandlingMinutes = Expression.Call(typeof(EntityFunctions), "AddMinutes", null, offsetHandlingHours, Expression.Constant(utcOffsetMinutes, typeof(int?)));

                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            Expression expressionEntry = Expression.GreaterThanOrEqual(offsetHandlingMinutes, Expression.Constant(dateRange.StartDate.Value, typeof(DateTime?)));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            //Also need to strip off the time component
                            MethodCallExpression truncateTimeCall = Expression.Call(typeof(EntityFunctions), "TruncateTime", null, offsetHandlingMinutes);
                            Expression expressionEntry = Expression.GreaterThanOrEqual(truncateTimeCall, Expression.Constant(dateRange.StartDate.Value.Date, typeof(DateTime?)));
                            expressionList.Add(expressionEntry);
                        }
                    }
                    if (dateRange.EndDate.HasValue)
                    {
                        MethodCallExpression offsetHandlingHours = Expression.Call(addHoursMethod, Expression.Convert(memberExpression, typeof(DateTime?)), Expression.Constant(utcOffsetHours, typeof(int?)));
                        MethodCallExpression offsetHandlingMinutes = Expression.Call(typeof(EntityFunctions), "AddMinutes", null, offsetHandlingHours, Expression.Constant(utcOffsetMinutes, typeof(int?)));

                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            Expression expressionEntry = Expression.LessThanOrEqual(offsetHandlingMinutes, Expression.Constant(dateRange.EndDate.Value, typeof(DateTime?)));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            //Also need to strip off the time component
                            MethodCallExpression truncateTimeCall = Expression.Call(typeof(EntityFunctions), "TruncateTime", null, offsetHandlingMinutes);
                            Expression expressionEntry = Expression.LessThanOrEqual(truncateTimeCall, Expression.Constant(dateRange.EndDate.Value.Date, typeof(DateTime?)));
                            expressionList.Add(expressionEntry);
                        }
                    }
                }
                else
                {
                    //Using in-memory LINQ
                    //(DateTime?).GetValueOrDefault().AddHours(1).AddMinutes(1).Date

                    //Handle the special date-range type
                    DateRange dateRange = (DateRange)filterValue;
                    PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;
                    MethodInfo addHoursMethod = typeof(DateTime).GetMethod("AddHours");
                    MethodInfo addMinutesMethod = typeof(DateTime).GetMethod("AddMinutes");
                    Expression nonNullableExpression = memberExpression;
                    if (propInfo.IsNullable() && propInfo.PropertyType != typeof(DateTime))
                    {
                        //Get the 'Value' property safely using GetValueOrDefault() method
                        MethodInfo valMethod = typeof(DateTime?).GetMethod("GetValueOrDefault", new Type[] { typeof(DateTime) } );
                        nonNullableExpression = Expression.Call(memberExpression, valMethod, Expression.Constant(DateTime.MinValue.AddYears(1900)));
                    }

                    if (dateRange.StartDate.HasValue)
                    {
                        MethodCallExpression offsetHandlingHours = Expression.Call(nonNullableExpression, addHoursMethod, Expression.Constant((double)utcOffsetHours));
                        MethodCallExpression offsetHandlingMinutes = Expression.Call(offsetHandlingHours, addMinutesMethod, Expression.Constant((double)utcOffsetMinutes));

                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            Expression expressionEntry = Expression.GreaterThanOrEqual(offsetHandlingMinutes, Expression.Constant(dateRange.StartDate.Value, typeof(DateTime)));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            //Also need to strip off the time component
                            PropertyInfo dateProp = typeof(DateTime).GetProperty("Date");
                            Expression truncateTimeCall = Expression.MakeMemberAccess(offsetHandlingMinutes, dateProp);
                            Expression expressionEntry = Expression.GreaterThanOrEqual(truncateTimeCall, Expression.Constant(dateRange.StartDate.Value.Date, typeof(DateTime)));
                            expressionList.Add(expressionEntry);
                        }
                    }
                    if (dateRange.EndDate.HasValue)
                    {
                        MethodCallExpression offsetHandlingHours = Expression.Call(nonNullableExpression, addHoursMethod, Expression.Constant((double)utcOffsetHours));
                        MethodCallExpression offsetHandlingMinutes = Expression.Call(offsetHandlingHours, addMinutesMethod, Expression.Constant((double)utcOffsetMinutes));

                        //See if we need to consider the time component
                        if (dateRange.ConsiderTimes)
                        {
                            Expression expressionEntry = Expression.LessThanOrEqual(offsetHandlingMinutes, Expression.Constant(dateRange.EndDate.Value, typeof(DateTime)));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            //Also need to strip off the time component
                            PropertyInfo dateProp = typeof(DateTime).GetProperty("Date");
                            Expression truncateTimeCall = Expression.MakeMemberAccess(offsetHandlingMinutes, dateProp);
                            Expression expressionEntry = Expression.LessThanOrEqual(truncateTimeCall, Expression.Constant(dateRange.EndDate.Value.Date, typeof(DateTime)));
                            expressionList.Add(expressionEntry);
                        }
                    }
                }
            }
            else if (filterValue is DecimalRange)
            {
                //Handle the special decimal-range type
                DecimalRange decimalRange = (DecimalRange)filterValue;
                if (decimalRange.MinValue.HasValue)
                {
                    Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(decimalRange.MinValue.Value, typeof(Decimal)));
                    expressionList.Add(expressionEntry);
                }
                if (decimalRange.MaxValue.HasValue)
                {
                    Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(decimalRange.MaxValue.Value, typeof(Decimal)));
                    expressionList.Add(expressionEntry);
                }
            }
            else if (filterValue is EffortRange)
            {
                //Handle the special decimal-range type
                EffortRange effortRange = (EffortRange)filterValue;
                PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;
                if (effortRange.MinValueInMinutes.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(effortRange.MinValueInMinutes, typeof(Nullable<Int32>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(effortRange.MinValueInMinutes.Value, typeof(Int32)));
                        expressionList.Add(expressionEntry);
                    }
                }
                if (effortRange.MaxValueInMinutes.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Convert(Expression.Constant(effortRange.MaxValueInMinutes), typeof(Nullable<Int32>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(effortRange.MaxValueInMinutes.Value, typeof(Int32)));
                        expressionList.Add(expressionEntry);
                    }
                }
            }
            else if (filterValue is IntRange)
            {
                //Handle the special integer-range type
                PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;
                IntRange intRange = (IntRange)filterValue;
                if (intRange.MinValue.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(intRange.MinValue.Value, typeof(Nullable<Int32>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(intRange.MinValue.Value, typeof(Int32)));
                        expressionList.Add(expressionEntry);
                    }
                }
                if (intRange.MaxValue.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(intRange.MaxValue.Value, typeof(Nullable<Int32>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(intRange.MaxValue.Value, typeof(Int32)));
                        expressionList.Add(expressionEntry);
                    }
                }
            }

            else if (filterValue is LongRange)
            {
                //Handle the special integer-range type
                PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;
                LongRange longRange = (LongRange)filterValue;
                if (longRange.MinValue.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(longRange.MinValue.Value, typeof(Nullable<Int64>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(longRange.MinValue.Value, typeof(Int64)));
                        expressionList.Add(expressionEntry);
                    }
                }
                if (longRange.MaxValue.HasValue)
                {
                    //Need to see if the property is nullable
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(longRange.MaxValue.Value, typeof(Nullable<Int64>)));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.LessThanOrEqual(memberExpression, Expression.Constant(longRange.MaxValue.Value, typeof(Int64)));
                        expressionList.Add(expressionEntry);
                    }
                }
            }

            else if (filterValue is MultiValueFilter)
            {
                //Handle the special multi-value lookup type
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
                PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;

                if (multiValueFilter.IsNone)
                {
                    //See if the property is nullable, if not then it will always be false
                    //since non-nullable values can never be null
                    if (propInfo.IsNullable())
                    {
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(null));
                        expressionList.Add(expressionEntry);
                    }
                    else
                    {
                        Expression expressionEntry = Expression.Constant(false);
                        expressionList.Add(expressionEntry);
                    }
                }
                else if (multiValueFilter.Values.Count > 0)
                {
                    //Handle a multilist (string) separately from a list (int32) field
                    if (propInfo.PropertyType == typeof(string))
                    {
                        //Multi-List (string)
                        //Since multi-lists are serialized as strings, need to explicitly convert each value in the
                        //multivaluefilter to serialized list format rather than just using the built-in ToString().
                        List<Expression> orList = new List<Expression>();
                        foreach (int value in multiValueFilter.Values)
                        {
                            //We have to add the value twice, once in the new padded format, once in normal format
                            //Padded
                            {
                                //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + value.ToDatabaseSerialization() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                orList.Add(expressionEntry);
                            }
                            //Normal
                            {
                                //We have to prefix/suffix the value with the separator and use ,TBL.VALUE, LIKE '%,Value,%;'
                                MethodInfo concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                Expression memberExpressionWithPrefixSuffix = Expression.Call(concatMethod, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString()), Expression.Call(concatMethod, memberExpression, Expression.Constant(DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString())));
                                string serializedValue = DatabaseExtensions.FORMAT_LIST_SEPARATOR + value.ToString() + DatabaseExtensions.FORMAT_LIST_SEPARATOR;
                                MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                Expression expressionEntry = Expression.Call(memberExpressionWithPrefixSuffix, containsMethod, Expression.Constant(serializedValue));
                                orList.Add(expressionEntry);
                            }
                        }
                        Expression orAggregate = orList.Aggregate(Expression.Or);
                        expressionList.Add(orAggregate);
                    }
                    else
                    {
                        //Single-Select List (int?/int)
                        //Need to handle nullable list properties
                        if (propInfo.IsNullable())
                        {
                            //Get the 'Value' property
                            PropertyInfo valProp = typeof(Nullable<int>).GetProperty("Value");
                            memberExpression = Expression.MakeMemberAccess(memberExpression, valProp);
                            MethodInfo containsMethod = typeof(List<int>).GetMethod("Contains");
                            Expression expressionEntry = Expression.Call(Expression.Constant(multiValueFilter.Values), containsMethod, memberExpression);
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            MethodInfo containsMethod = typeof(List<int>).GetMethod("Contains");
                            Expression expressionEntry = Expression.Call(Expression.Constant(multiValueFilter.Values), containsMethod, memberExpression);
                            expressionList.Add(expressionEntry);
                        }   
                    }
                }
            }
            else
            {
                //Otherwise we need to look at the type of the property and the filter value
                PropertyInfo propInfo = (PropertyInfo)memberExpression.Member;
                if (propInfo.PropertyType == typeof(string) && filterValue is string)
                {
                    //If we have a Name field, need to treat as keywords and also search the Description field
					//Can disable the searching of descriptions in a project-level setting (for performance reasons)
                    string filterValueString = (string)filterValue;
                    if (propertyName == "Name")
                    {
						bool filterNameAndDescription = true;
						if (projectId.HasValue)
						{
							ProjectSettings settings = new ProjectSettings(projectId.Value);
							filterNameAndDescription = settings.FilterNameAndDescription;
						}
						
						if (memberExpression.Member.DeclaringType.GetProperty("Description") == null || !filterNameAndDescription)
                        {
                            //We don't have a description field, so just use the primary field
                            MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                            MatchCollection keywordMatches = Regex.Matches(filterValueString, Common.Global.REGEX_KEYWORD_MATCHER);
                            foreach (Match keywordMatch in keywordMatches)
                            {
                                string keyword = keywordMatch.Value.Replace("\"", "");
                                Expression expressionEntry = Expression.Call(memberExpression, containsMethod, Expression.Constant(keyword));
                                expressionList.Add(expressionEntry);
                            }
                        }
                        else
                        {
                            MemberExpression descriptionExpression = LambdaExpression.PropertyOrField(p, "Description");
                            if (descriptionExpression == null)
                            {
                                //We don't have a description field, so just use the primary field
                                MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                MatchCollection keywordMatches = Regex.Matches(filterValueString, Common.Global.REGEX_KEYWORD_MATCHER);
                                foreach (Match keywordMatch in keywordMatches)
                                {
                                    string keyword = keywordMatch.Value.Replace("\"", "");
                                    Expression expressionEntry = Expression.Call(memberExpression, containsMethod, Expression.Constant(keyword));
                                    expressionList.Add(expressionEntry);
                                }
                            }
                            else
                            {
                                MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                                MatchCollection keywordMatches = Regex.Matches(filterValueString, Common.Global.REGEX_KEYWORD_MATCHER);
                                foreach (Match keywordMatch in keywordMatches)
                                {
                                    string keyword = keywordMatch.Value.Replace("\"", "");
                                    Expression expressionEntryPart1 = Expression.Call(memberExpression, containsMethod, Expression.Constant(keyword));
                                    Expression expressionEntryPart2 = Expression.Call(descriptionExpression, containsMethod, Expression.Constant(keyword));
                                    Expression expressionEntry = Expression.Or(expressionEntryPart1, expressionEntryPart2);
                                    expressionList.Add(expressionEntry);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Handle general string properties - they become simple LIKE comparisons
                        //If not using EF (and just LINQ) need to make case insensitive
                        if (usingEF)
                        {
                            MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                            Expression expressionEntry = Expression.Call(memberExpression, containsMethod, Expression.Constant(filterValueString));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            //Need to also check for nulls
                            //p.ToLower().Contains()
                            MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes, null);
                            MethodInfo containsMethod = typeof(string).GetMethod("Contains");
                            Expression nullSafeValueExpression = Expression.Coalesce(memberExpression, Expression.Constant(""));
                            Expression expressionEntry1 = Expression.Call(nullSafeValueExpression, toLowerMethod);
                            Expression expressionEntry2 = Expression.Call(expressionEntry1, containsMethod, Expression.Constant(filterValueString.ToLower()));
                            expressionList.Add(expressionEntry2);
                        }
                    }
                }
                if (propInfo.PropertyType == typeof(int) || propInfo.PropertyType == typeof(int?))
                {
                    //Handle int properties
                    if (filterValue is long)
                    {
                        int filterValueInt = (int)((long)filterValue);
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueInt, propInfo.PropertyType));
                        expressionList.Add(expressionEntry);
                    }
                    if (filterValue is int)
                    {
                        //See if we have the special case of a (None) filter
                        int filterIntValue = (int)filterValue;
                        if (filterIntValue == NoneFilterValue)
                        {
                            Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(null, propInfo.PropertyType));
                            expressionList.Add(expressionEntry);
                        }
                        else
                        {
                            Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterIntValue, propInfo.PropertyType));
                            expressionList.Add(expressionEntry);
                        }
                    }
                    if (filterValue is String)
                    {
                        int intValue;
                        if (Int32.TryParse((string)filterValue, out intValue))
                        {
                            Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(intValue, propInfo.PropertyType));
                            expressionList.Add(expressionEntry);
                        }
                    }
                }
                if (propInfo.PropertyType == typeof(long) || propInfo.PropertyType == typeof(long?))
                {
                    //Handle long properties
                    if (filterValue is int)
                    {
                        long filterValueLong = (long)((int)filterValue);
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueLong, propInfo.PropertyType));
                        expressionList.Add(expressionEntry);
                    }
                    if (filterValue is long)
                    {
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant((long)filterValue, propInfo.PropertyType));
                        expressionList.Add(expressionEntry);
                    }
                    if (filterValue is String)
                    {
                        long longValue;
                        if (Int64.TryParse((string)filterValue, out longValue))
                        {
                            Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(longValue, propInfo.PropertyType));
                            expressionList.Add(expressionEntry);
                        }
                    }
                }
                if (propInfo.PropertyType == typeof(short) || propInfo.PropertyType == typeof(short?))
                {
                    //Handle short properties
                    if (filterValue is int)
                    {
                        short filterValueShort = (short)((int)filterValue);
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueShort, propInfo.PropertyType));
                        expressionList.Add(expressionEntry);
                    }
                    else if (filterValue is string)
                    {
                        short filterValueShort;
                        if (short.TryParse((string)filterValue, out filterValueShort))
                        {
                            Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueShort, propInfo.PropertyType));
                            expressionList.Add(expressionEntry);
                        }
                    }
                    else
                    {
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant((short)filterValue, propInfo.PropertyType));
                        expressionList.Add(expressionEntry);
                    }
                }
                if (propInfo.PropertyType == typeof(bool))
                {
                    //Handle bool properties
                    if (filterValue is bool)
                    {
                        bool filterValueBool = (bool)filterValue;
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueBool));
                        expressionList.Add(expressionEntry);
                    }
                    else if (filterValue is string)
                    {
                        bool filterValueBool = ((string)filterValue == "Y");
                        Expression expressionEntry = Expression.Equal(memberExpression, Expression.Constant(filterValueBool));
                        expressionList.Add(expressionEntry);
                    }
                }
                if (propInfo.PropertyType == typeof(DateTime))
                {
                    if (filterValue is string)
                    {
                        DateTime filterValueDateTime;
                        if (DateTime.TryParse((string)filterValue, out filterValueDateTime))
                        {
                            //We only want to consider the date component. However we also need to strip off the time part of the property itself
                            //However since LINQ to entities does not support the .Date property, we need to use DateTime >= Date && DateTime < (Date + 1 day)
                            Expression expressionEntry1 = Expression.GreaterThanOrEqual(memberExpression, Expression.Constant(filterValueDateTime.Date));
                            expressionList.Add(expressionEntry1);
                            Expression expressionEntry2 = Expression.LessThan(memberExpression, Expression.Constant(filterValueDateTime.Date.AddDays(1)));
                            expressionList.Add(expressionEntry2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a dictionary of filters into the corresponding LINQ expression
        /// </summary>
        /// <typeparam name="TElement">The entity type</typeparam>
        /// <param name="utcOffset">The offset from UTC for filtering date fields</param>
        /// <param name="filterList">The dictionary of filters</param>
        /// <param name="specialFilterHandler">Function to call that handles artifact-specific filters</param>
        /// <param name="artifactType">The current artifact type</param>
        /// <param name="ignoreList">Optional list of filters that should be ignored from the dictionary</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="projectTemplateId">The id of the current project template</param>
        /// <param name="usingEF">Are we using EF (as opposed to straight LINQ)</param>
        /// <returns>the filters expression</returns>
        public static Expression<Func<TElement, bool>> CreateFilterExpression<TElement>(int? projectId, int? projectTemplateId, Artifact.ArtifactTypeEnum artifactType, Dictionary<string, object> filterList, double utcOffset, List<string> ignoreList = null, HandleSpecialFilters specialFilterHandler = null, bool usingEF = true)
        {
            //Handle the case of no filters
            if (filterList == null || filterList.Count == 0)
            {
                return null;
            }

            //Build the base LINQ expression
            
            //The p => parameter that we'll use in the Where clause
            ParameterExpression p = Expression.Parameter(typeof(TElement), "p");

            //Loop through each of the filters and build the corresponding part of the clause
            List<Expression> expressionList = new List<Expression>();
            foreach (KeyValuePair<string, object> filter in filterList)
            {
                //See if we're in the ignore list
                if (ignoreList == null || !ignoreList.Contains(filter.Key))
                {
                    //See if we have a custom handler for this one
                    bool handled = false;
                    if (specialFilterHandler != null)
                    {
                        handled = specialFilterHandler(projectId, projectTemplateId, p, expressionList, filter, utcOffset);
                    }

                    //Continue to the next filter if handled
                    if (handled)
                    {
                        continue;
                    }

                    //Get the member and value (p.PropertyName)
                    //See if we have a multiple level property (e.g. p.NavigationProperty.PropertyName)
                    MemberExpression memberExpression = null;
                    if (filter.Key.Contains("."))
                    {
                        string[] properties = filter.Key.Split('.');
                        foreach (string property in properties)
                        {
                            if (memberExpression == null)
                            {
                                memberExpression = LambdaExpression.PropertyOrField(p, property);
                            }
                            else
                            {
                                memberExpression = LambdaExpression.PropertyOrField(memberExpression, property);
                            }
                        }
                    }
                    else
                    {
                        //Make sure the property actually exists
                        if (filter.Key != null && typeof(TElement).GetProperty(filter.Key) != null)
                        {
                            memberExpression = LambdaExpression.PropertyOrField(p, filter.Key);
                        }
                    }
                    if (memberExpression != null)
                    {
                        object filterValue = filter.Value;

                        //See if we have a standard or custom filter because they work differently
                        int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(filter.Key);
                        if (customPropertyNumber.HasValue)
                        {
                            //Handle the custom property types (need to also have a project id and project template id)
                            if (projectId.HasValue && projectTemplateId.HasValue)
                            {
                                CreateFilterEntryExpression(projectId.Value, projectTemplateId.Value, customPropertyNumber.Value, artifactType, filterValue, utcOffset, memberExpression, expressionList);
                            }
                        }
                        else
                        {
                            //Handle standard field types
                            CreateFilterEntryExpression(projectId, filter.Key, filterValue, utcOffset, memberExpression, expressionList, p, usingEF);
                        }
                    }
                }
            }

            //Return the complete expression with the different clauses ANDed
            if (expressionList.Count > 0)
            {
                Expression body = expressionList.Aggregate(Expression.And);
                return Expression.Lambda<Func<TElement, bool>>(body, p);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a hashtable of filters into the corresponding LINQ expression
        /// </summary>
        /// <typeparam name="TElement">The entity type</typeparam>
        /// <param name="utcOffset">The offset from UTC for filtering date/time fields</param>
        /// <param name="filterList">The hashtable of filters</param>
        /// <param name="projectId">The ID of the current project</param>
        /// <param name="projectTemplateId">The ID of the current project template</param>
        /// <param name="artifactType">The artifact type being filted on (if any)</param>
        /// <param name="ignoreList">Optional list of filters that should be ignored from the dictionary</param>
        /// <param name="usingEF">Are we using EF (as opposed to straight LINQ)</param>
        /// <returns>the filters expression</returns>
        public static Expression<Func<TElement, bool>> CreateFilterExpression<TElement>(int? projectId, int? projectTemplateId, Artifact.ArtifactTypeEnum artifactType, Hashtable filterList, double utcOffset, List<string> ignoreList = null, HandleSpecialFilters specialFilterHandler = null, bool usingEF = true)
        {
            //Handle the case of no filters
            if (filterList == null || filterList.Count == 0)
            {
                return null;
            }

            //Convert the hashtable into a dictionary and the use that overload
            Dictionary<string, object> filterList2 = new Dictionary<string, object>();
            foreach (DictionaryEntry dicEntry in filterList)
            {
                if (dicEntry.Key.GetType() == typeof(string))
                {
                    filterList2.Add((string)dicEntry.Key, dicEntry.Value);
                }
            }

            return CreateFilterExpression<TElement>(projectId, projectTemplateId, artifactType, filterList2, utcOffset, ignoreList, specialFilterHandler, usingEF);
        }
    }

    /// <summary>
    /// Contains the static extension functions that cannot be part of ManagerBase itself
    /// </summary>
    public static class Manager
    {
        #region General extension methods

        /// <summary>
        /// Provides strongly-typed Includes (i.e. Include(p => p.Property) instead of Include("Property")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static ObjectQuery<T> Include<T>(this ObjectQuery<T> query, Expression<Func<T, object>> exp)
        {
            Expression body = exp.Body;
            MemberExpression memberExpression = (MemberExpression)exp.Body;
            string path = GetIncludePath(memberExpression);
            return query.Include(path);
        }

        private static string GetIncludePath(MemberExpression memberExpression)
        {
            string path = "";
            if (memberExpression.Expression is MemberExpression)
            {
                path = GetIncludePath((MemberExpression)memberExpression.Expression) + ".";
            }
            PropertyInfo propertyInfo = (PropertyInfo)memberExpression.Member;
            return path + propertyInfo.Name;
        }

        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {

            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public static Expression<Func<T, bool>> BuildAnd<T>(params Expression<Func<T, bool>>[] conditions)
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null, (current, expression) => current == null ? expression : current.And(expression));
        }

        public static Expression<Func<T, bool>> BuildOr<T>(params Expression<Func<T, bool>>[] conditions)
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null, (current, expression) => current == null ? expression : current.Or(expression));
        }

        public static Expression<Func<T, bool>> BuildOrElse<T>(params Expression<Func<T, bool>>[] conditions)
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null, (current, expression) => current == null ? expression : current.OrElse(expression));
        }

        /// <summary>
        /// Gets a generic method from reflection
        /// </summary>
        /// <param name="staticType">The type that the method is a static method on</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="paramTypes">The parameters</param>
        /// <returns></returns>
        public static MethodInfo GetGenericMethod(this Type staticType, string methodName, params Type[] paramTypes)
        {
            var methods = from method in staticType.GetMethods()
                          where method.Name == methodName
                            && method.GetParameters()
                                .Select(parameter => parameter.ParameterType)
                                .Select(type => type.IsGenericType ? type.GetGenericTypeDefinition() : type)
                                .SequenceEqual(paramTypes)
                          select method;
            
            try
            {
                return methods.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousMatchException();
            }
        }

        /// <summary>
        /// Makes comparison between nullable and non-nullable types easier
        /// </summary>
        /// <param name="e1">Type 1</param>
        /// <param name="e2">Type 2</param>
        /// <returns>True if they are equal, accounting for NULLs</returns>
        public static Expression LinqEqual(Expression e1, Expression e2)
        {
            if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
                e2 = Expression.Convert(e2, e1.Type);
            else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
                e1 = Expression.Convert(e1, e2.Type);
            return Expression.Equal(e1, e2);
        }

        /// <summary>
        /// Returns TRUE if the type is nullable
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        } 

        #endregion

        #region Private expression tree helpers

        private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            var parameter = Expression.Parameter(typeof(TEntity), "Entity");
            //  create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields.
                String[] childProperties = propertyName.Split('.');
                property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                //Make sure the property exists
                if (property == null)
                {
                    resultType = typeof(object);
                    return null;
                }
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }
            resultType = property.PropertyType;
            // Create the order by expression.
            return Expression.Lambda(propertyAccess, parameter);
        }

        private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
        {
            Type type = typeof(TEntity);
            Type selectorResultType;
            LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
            if (selector == null)
            {
                //This happends if the provided field name does not exist
                return null;
            }
            else
            {
                MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
                                new Type[] { type, selectorResultType },
                                source.Expression, Expression.Quote(selector));
                return resultExp;
            }
        }
        #endregion

        #region Dynamic OrderBy sorting extensions

        /// <summary>
        /// Allows you to specify the ORDER BY clause as a string property name (useful for dynamic sorting)
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being sorted</typeparam>
        /// <param name="source">The query that we extend</param>
        /// <param name="fieldName">The property name</param>
        /// <returns>The LINQ query</returns>
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
            if (resultExp == null)
            {
                //Happens if the field doesn't exist
                return null;
            }
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// Allows you to specify the ORDER BY DESCENDING clause as a string property name (useful for dynamic sorting)
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being sorted</typeparam>
        /// <param name="source">The query that we extend</param>
        /// <param name="fieldName">The property name</param>
        /// <returns>The LINQ query</returns>
        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
            if (resultExp == null)
            {
                //Happens if the field doesn't exist
                return null;

            }
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// Allows you to specify the THEN BY clause as a string property name (useful for dynamic sorting)
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being sorted</typeparam>
        /// <param name="source">The query that we extend</param>
        /// <param name="fieldName">The property name</param>
        /// <returns>The LINQ query</returns>
        public static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenBy", fieldName);
            if (resultExp == null)
            {
                //Happens if the field doesn't exist
                return null;

            }
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// Allows you to specify the THEN BY DESCENDING clause as a string property name (useful for dynamic sorting)
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being sorted</typeparam>
        /// <param name="source">The query that we extend</param>
        /// <param name="fieldName">The property name</param>
        /// <returns>The LINQ query</returns>
        public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenByDescending", fieldName);
            if (resultExp == null)
            {
                //Happens if the field doesn't exist
                return null;
            }
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// Allows you to specify a complete [PropertyName] [ASC|DESC] sort expression
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being sorted</typeparam>
        /// <param name="source">The query that we extend</param>
        /// <param name="stableField">A stable field that is guaranteed to exist (it doesn't check for it)</param>
        /// <returns>The LINQ query</returns>
        /// <param name="sortExpression">The sort expression ([PropertyName] [ASC|DESC], [PropertyName] [ASC|DESC])</param>
        public static IOrderedQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression, string stableField) where TEntity : class
        {
            String[] orderFields = sortExpression.Split(',');
            IOrderedQueryable<TEntity> result = null;
            for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
            {
                String[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
                String sortField = expressionPart[0];

                Boolean sortDescending = (expressionPart.Length == 2) && (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
                if (sortDescending)
                {
                    result = (currentFieldIndex == 0 || result == null) ? source.OrderByDescending(sortField) : result.ThenByDescending(sortField);
                }
                else
                {
                    result = (currentFieldIndex == 0 || result == null) ? source.OrderBy(sortField) : result.ThenBy(sortField);
                }
            }

            //Finally add on the stable field
            if (result == null)
            {
                return source.OrderBy(stableField);
            }
            else
            {
                return result.ThenBy(stableField);
            }
        }

        #endregion
    }

    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;

            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }

    /* Various business exceptions */

    /// <summary>
    /// This exception is thrown when you try and restore an artifact that no longer exists
    /// </summary>
	[Serializable]
	public class ArtifactNotExistsException : ApplicationException
    {
        public ArtifactNotExistsException()
        {
        }
        public ArtifactNotExistsException(string message)
            : base(message)
        {
        }
        public ArtifactNotExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    /// <summary>
    /// This exception is thrown when a SELECT query results in an infinite recursive loop
    /// </summary>

    public class ArtifactAuthorizationException : Exception
    {
        public ArtifactAuthorizationException()
        {
        }
        public ArtifactAuthorizationException(string message)
            : base(message)
        {
        }
        public ArtifactAuthorizationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when data validation exception occurs (used by APIs mainly)
    /// </summary>
    /// <remarks>
    /// We catch provider native exceptions and throw this instead so that application code can be provider agnostic
    /// </remarks>
	[Serializable]
	public class DataValidationException : Exception
    {
        public DataValidationException()
        {
        }
        public DataValidationException(string message)
            : base(message)
        {
        }
        public DataValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
