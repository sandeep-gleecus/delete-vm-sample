using Inflectra.SpiraTest.DataModel;
using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Stores encrypted settings using configuration entities
    /// </summary>
    /// <remarks>
    /// There is a flag to mark if they are encrypted or not, that is used for migrated settings
    /// from older versions where they were not encrypted
    /// </remarks>
    public class SecureSettingsProvider : SettingsProvider
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Common.SecureSettingsProvider::";

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
                //Create the encryption algorithm
                SimpleAES simpleAES = new SimpleAES();

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
                        var query = from g in context.GlobalSecureSettings
                            where g.Name == propertyDefLocal.Name
                            select g;

                        GlobalSecureSetting globalSetting = query.FirstOrDefault();

                        //See if we have a match
                        if (globalSetting != null)
                        {
                            try
                            {
                                //First we need to decrypt the value (if necessary)
                                string clearValue;
                                if (!globalSetting.IsEncrypted || String.IsNullOrEmpty(globalSetting.Value))
                                {
                                    clearValue = globalSetting.Value;
                                }
                                else
                                {
                                    clearValue = simpleAES.DecryptString(globalSetting.Value);
                                }

                                //If a matching property is found, its value is used. 
                                //We only handle strings, integers, longs, dates and booleans
                                if (propertyDef.PropertyType == typeof(int))
                                {
                                    propertyValue.PropertyValue = Int32.Parse(clearValue);
                                }
                                if (propertyDef.PropertyType == typeof(long))
                                {
                                    propertyValue.PropertyValue = Int64.Parse(clearValue);
                                }
                                if (propertyDef.PropertyType == typeof(string))
                                {
                                    propertyValue.PropertyValue = clearValue;
                                }
                                if (propertyDef.PropertyType == typeof(bool))
                                {
                                    propertyValue.PropertyValue = Boolean.Parse(clearValue);
                                }
                                if (propertyDef.PropertyType == typeof(DateTime))
                                {
                                    propertyValue.PropertyValue = DateTime.ParseExact(clearValue, FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
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
                //Create the encryption algorithm
                SimpleAES simpleAES = new SimpleAES();

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
                            GlobalSecureSetting globalSetting = context.GlobalSecureSettings.SingleOrDefault(c => c.Name == propertyValueLocal.Name);
                            if (globalSetting == null)
                            {
                                //We need to add a new configuration entity first
                                globalSetting = new GlobalSecureSetting();
                                context.GlobalSecureSettings.AddObject(globalSetting);

                                //We need to add the name
                                globalSetting.Name = propertyValue.Name;
                            }

                            //All saved settings will be encrypted
                            globalSetting.IsEncrypted = true;

                            //We only handle strings, integers, longs, dates and booleans
                            string clearValue = null;
                            SettingsProperty propertyDef = propertyValue.Property;
                            if (propertyDef.PropertyType == typeof(int))
                            {
                                clearValue = ((int)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(long))
                            {
                                clearValue = ((long)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(string))
                            {
                                clearValue = (string)propertyValue.PropertyValue;
                            }
                            if (propertyDef.PropertyType == typeof(bool))
                            {
                                clearValue = ((bool)propertyValue.PropertyValue).ToString();
                            }
                            if (propertyDef.PropertyType == typeof(DateTime))
                            {
                                clearValue = ((DateTime)propertyValue.PropertyValue).ToString(FORMAT_DATE_TIME, DateTimeFormatInfo.InvariantInfo);
                            }

                            if (String.IsNullOrEmpty(clearValue))
                            {
                                globalSetting.Value = clearValue;
                            }
                            else
                            {
                                globalSetting.Value = simpleAES.EncryptToString(clearValue);
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
