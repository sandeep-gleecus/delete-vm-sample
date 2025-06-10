using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Stores the project template -specific (not per-user) settings using configuration entities
    /// </summary>
    public class ProjectTemplateSettingsProvider : SettingsProvider
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ProjectTemplateSettingsProvider::";

        private const string FORMAT_DATE_TIME = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="name">The name of the provider</param>
        /// <param name="config">The name value pairs of configurations</param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(this.ApplicationName, config);
        }

        /// <summary>
        /// Returns the application name
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            }
            set
            {
                //Do Nothing
            }
        }

        /// <summary>
        /// Gets the property values
        /// </summary>
        /// <param name="settingsContext">The settings context</param>
        /// <param name="collection">The collection of properties</param>
        /// <returns>The collection of property values</returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext settingsContext, SettingsPropertyCollection collection)
        {
            const string METHOD_NAME = "GetPropertyValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the new settings value collection
            SettingsPropertyValueCollection propertyValues = new SettingsPropertyValueCollection();

            //Get the project template id from context
            int projectTemplateId = 0;
            if (settingsContext.Contains("ProjectTemplateId") && settingsContext["ProjectTemplateId"] is Int32)
            {
                projectTemplateId = (int)settingsContext["ProjectTemplateId"];
            }
            if (projectTemplateId <= 0)
            {
               throw new InvalidOperationException("You need to pass in a ProjectTemplateId to the settings provider");
            }

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Iterate through each property
                    foreach (SettingsProperty propertyDef in collection)
                    {
                        //Compiler will issue a warning if we attempt to use the iteration variable
                        //in a lambda expression, so we reassign propDef to a new variable
                        SettingsProperty propertyDefLocal = propertyDef;
                    
                        //Create a new instance of SettingsPropertyValue from the property definition
                        SettingsPropertyValue propertyValue = new SettingsPropertyValue(propertyDef);

                        //Query the database for the one setting that is enabled and 
                        //has the name matching the name of the property
                        var query = from p in context.ProjectTemplateSettingValues.Include(p => p.Setting)
                            where p.Setting.Name == propertyDefLocal.Name && p.ProjectTemplateId == projectTemplateId
									select p;

                        ProjectTemplateSettingValue projectSettingValue = query.FirstOrDefault();

                        //See if we have a match
                        if (projectSettingValue != null)
                        {
                            try
                            {
                                //If a matching property is found, its value is used. 
                                //We only handle strings, integers, longs, dates and booleans
                                if (propertyDef.PropertyType == typeof(int))
                                {
                                    propertyValue.PropertyValue = Int32.Parse(projectSettingValue.Value);
                                }
                                if (propertyDef.PropertyType == typeof(long))
                                {
                                    propertyValue.PropertyValue = Int64.Parse(projectSettingValue.Value);
                                }
                                if (propertyDef.PropertyType == typeof(string))
                                {
                                    propertyValue.PropertyValue = projectSettingValue.Value;
                                }
                                if (propertyDef.PropertyType == typeof(bool))
                                {
                                    propertyValue.PropertyValue = Boolean.Parse(projectSettingValue.Value);
                                }
                                if (propertyDef.PropertyType == typeof(decimal))
                                {
                                    propertyValue.PropertyValue = projectSettingValue.Value.FromDatabaseSerialization_Decimal();
                                }
                                if (propertyDef.PropertyType == typeof(DateTime))
                                {
                                    propertyValue.PropertyValue = DateTime.ParseExact(projectSettingValue.Value, FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
                                }
                            }
                            catch (FormatException)
                            {
                                //Do Nothing, leave the value unset
                            }
                        }
                        else
                        {
                            //Do nothing: the default property value is used instead.
                        }

                        //Indicate that the setting has not changed
                        propertyValue.IsDirty = false;
                        
                        //Add the setting to the collection to be returned to the settings class
                        propertyValues.Add(propertyValue);
                    }
                }
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            //Return the values
            return propertyValues;
        }

        /// <summary>
        /// Sets the property values
        /// </summary>
        /// <param name="settingsContext">The settings context</param>
        /// <param name="collection">The collection of property values</param>
        public override void SetPropertyValues(SettingsContext settingsContext, SettingsPropertyValueCollection collection)
        {
            const string METHOD_NAME = "SetPropertyValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Get the project template id from context
            int projectTemplateId = 0;
            if (settingsContext.Contains("ProjectTemplateId") && settingsContext["ProjectTemplateId"] is Int32)
            {
                projectTemplateId = (int)settingsContext["ProjectTemplateId"];
            }
            if (projectTemplateId <= 0)
            {
                throw new InvalidOperationException("You need to pass in a ProjectTemplateId to the settings provider");
            }

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    foreach (SettingsPropertyValue propertyValue in collection)
                    {
                        //If the setting value has changed, update the data source
                        if (propertyValue.IsDirty)
                        {
                            //Create a new variable for use in the lambda expression
                            SettingsPropertyValue propertyValueLocal = propertyValue;
                            
                            //Update the correct entity with the new value of the setting property
                            ProjectTemplateSetting projectSetting = context.ProjectTemplateSettings.SingleOrDefault(p => p.Name == propertyValueLocal.Name);
                            if (projectSetting == null)
                            {
                                //We need to add a new project setting entity first
                                projectSetting = new ProjectTemplateSetting();
                                context.ProjectTemplateSettings.AddObject(projectSetting);

                                //We need to add the name
                                projectSetting.Name = propertyValue.Name;

                            }
                            //We only handle strings, integers, longs, dates and booleans
                            ProjectTemplateSettingValue projectSettingValue = context.ProjectTemplateSettingValues.SingleOrDefault(p => p.Setting.Name == propertyValueLocal.Name && p.ProjectTemplateId == projectTemplateId);
                            if (projectSettingValue == null)
                            {
                                projectSettingValue = new ProjectTemplateSettingValue();
                                projectSettingValue.ProjectTemplateId = projectTemplateId;
                                projectSetting.Values.Add(projectSettingValue);
                            }
                            SettingsProperty propertyDef = propertyValue.Property;
                            if (propertyDef.PropertyType == typeof(int))
                            {
                                projectSettingValue.Value = ((int)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(decimal))
                            {
                                projectSettingValue.Value = ((decimal)propertyValue.PropertyValue).ToDatabaseSerialization();
                            }
                            if (propertyDef.PropertyType == typeof(long))
                            {
                                projectSettingValue.Value = ((long)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(string))
                            {
                                projectSettingValue.Value = (string)propertyValue.PropertyValue;
                            }
                            if (propertyDef.PropertyType == typeof(bool))
                            {
                                projectSettingValue.Value = ((bool)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(DateTime))
                            {
                                projectSettingValue.Value = ((DateTime)propertyValue.PropertyValue).ToString(FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
                            }
                        }
                    }

                    //Persist to database
                    context.SaveChanges();
                }
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

                throw;
            }
            
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }
    }
}
