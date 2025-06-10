using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Stores the settings using configuration entities
    /// </summary>
    public class ConfigurationSettingsProvider : SettingsProvider
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Common.ConfigurationSettingsProvider::";

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

            try
            {
                using (SpiraTestEntities context = new SpiraTestEntities())
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
                        var query = from g in context.GlobalSettings
                            where g.Name == propertyDefLocal.Name
                            select g;

                        DataModel.GlobalSetting globalSetting = query.FirstOrDefault();

                        //See if we have a match
                        if (globalSetting != null)
                        {
                            try
                            {
                                //If a matching property is found, its value is used. 
                                //We only handle strings, integers, longs, dates and booleans
                                if (propertyDef.PropertyType == typeof(int))
                                {
                                    propertyValue.PropertyValue = Int32.Parse(globalSetting.Value);
                                }
                                if (propertyDef.PropertyType == typeof(long))
                                {
                                    propertyValue.PropertyValue = Int64.Parse(globalSetting.Value);
                                }
                                if (propertyDef.PropertyType == typeof(string))
                                {
                                    propertyValue.PropertyValue = globalSetting.Value;
                                }
                                if (propertyDef.PropertyType == typeof(bool))
                                {
                                    propertyValue.PropertyValue = Boolean.Parse(globalSetting.Value);
                                }
                                if (propertyDef.PropertyType == typeof(DateTime))
                                {
                                    propertyValue.PropertyValue = DateTime.ParseExact(globalSetting.Value, FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
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

            try
            {
                using (SpiraTestEntities context = new SpiraTestEntities())
                {
                    foreach (SettingsPropertyValue propertyValue in collection)
                    {
                        //If the setting value has changed, update the data source
                        if (propertyValue.IsDirty)
                        {
                            //Create a new variable for use in the lambda expression
                            SettingsPropertyValue propertyValueLocal = propertyValue;
                            
                            //Update the correct entity with the new value of the setting property
                            DataModel.GlobalSetting globalSetting = context.GlobalSettings.SingleOrDefault(c => c.Name == propertyValueLocal.Name);
                            if (globalSetting == null)
                            {
                                //We need to add a new configuration entity first
                                globalSetting = new DataModel.GlobalSetting();
                                context.GlobalSettings.AddObject(globalSetting);

                                //We need to add the name
                                globalSetting.Name = propertyValue.Name;

                            }
                            //We only handle strings, integers, longs, dates and booleans
                            SettingsProperty propertyDef = propertyValue.Property;
                            if (propertyDef.PropertyType == typeof(int))
                            {
                                globalSetting.Value = ((int)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(long))
                            {
                                globalSetting.Value = ((long)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(string))
                            {
                                globalSetting.Value = (string)propertyValue.PropertyValue;
                            }
                            if (propertyDef.PropertyType == typeof(bool))
                            {
                                globalSetting.Value = ((bool)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(DateTime))
                            {
                                globalSetting.Value = ((DateTime)propertyValue.PropertyValue).ToString(FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
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
