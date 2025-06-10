using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// The base class for all entities (artifact or otherwise)
    /// </summary>
    [DataContract(IsReference=true)]
    [Serializable]
    public abstract class Entity
    {
        public const string FORMAT_DATE_TIME_XML = "{0:yyyy-MM-ddTHH:mm:ss}";

        #region Property Accessor Functions

        /// <summary>
        /// Returns a handle to the Change tracker, if the entity has one
        /// </summary>
        public ObjectChangeTracker EntityChangeTracker
        {
            get
            {
                if (ContainsProperty("ChangeTracker") && this["ChangeTracker"] != null)
                {
                    return (ObjectChangeTracker)this["ChangeTracker"];
                }
                return null;
            }
        }

        /// <summary>
        /// Returns TRUE if the entity contains this property (different to having a null value)
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns></returns>
        public bool ContainsProperty(string propertyName)
        {
            //Get the current type
            Type type = this.GetType();

            //Get the property
            PropertyInfo propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (propInfo == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the current entity's properties as serialized XML
        /// </summary>
        /// <returns>The serialized XML</returns>
        /// <remarks>
        /// Does not include certain types such as ChangeTracker and TrackableCollections
        /// </remarks>
        public string InnerXml
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                //Loop through all the properties
                XmlElement xmlRootNode = xmlDoc.CreateElement("Entity");
                xmlDoc.AppendChild(xmlRootNode);
                foreach (KeyValuePair<string, PropertyInfo> kvp in this.Properties)
                {
                    string fieldName = kvp.Key;
                    object fieldValue = this[fieldName];
                    if (fieldValue != null && fieldValue.GetType() != typeof(ObjectChangeTracker) && (!fieldValue.GetType().IsGenericType || fieldValue.GetType().GetGenericTypeDefinition() != typeof(TrackableCollection<>)))
                    {
                        XmlNode xmlFieldNode = xmlDoc.CreateElement(fieldName);
                        xmlRootNode.AppendChild(xmlFieldNode);

                        //Serialize the data, for dates we need to make sure it's in the correct format
                        //that will be localized by the report writer
                        if (fieldValue.GetType() == typeof(DateTime))
                        {
                            DateTime dateTime = (DateTime)fieldValue;
                            xmlFieldNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, dateTime);
                        }
                        else if (fieldValue.GetType() == typeof(DateTime?))
                        {
                            DateTime? dateTime = (DateTime?)fieldValue;
                            if (dateTime.HasValue)
                            {
                                xmlFieldNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, dateTime.Value);
                            }
                        }
                        else if (fieldName.EndsWith("Effort") && fieldValue.GetType() == typeof(int))
                        {
							//Effort fields should be displayed in hours (decimal)
							                    int effort = (int)fieldValue;
							////// decimal hours = ((decimal)effort) / 60;
							////// xmlFieldNode.InnerText = hours.ToString("0.00");
							//////var quotient = (Math.Floor((decimal)effort / 60)).ToString();
							//////var remainder = effort % 60;
							//////xmlFieldNode.InnerText = quotient.ToString() + ":" + remainder.ToString();
							////// Calculate decimal hours
							////decimal hoursDecimal = (decimal)effort / 60;

							////// Create XML node for Actual Duration
							//////var xmlFieldNode = new XmlDocument().CreateElement("ActualDuration");

							////// Format as HH:MM
							////var quotient = (int)Math.Floor(hoursDecimal); // Whole hours
							////var remainder = effort % 60; // Remaining minutes

							////// Set the formatted duration
							////xmlFieldNode.InnerText = $"{quotient}:{remainder:D2}"; // Ensures two digits for minutes
							///// Calculate decimal hours
							decimal hoursDecimal = (decimal)effort / 60;

							// Determine whole hours and remaining minutes
							int wholeHours = (int)Math.Floor(hoursDecimal); // Whole hours
							int remainingMinutes = effort % 60; // Remaining minutes

							// Set the formatted duration
							xmlFieldNode.InnerText = effort.ToString(); // Output as decimal

						}
						else if (fieldName.EndsWith("Effort") && fieldValue.GetType() == typeof(int?))
                        {
                            //Effort fields should be displayed in hours (decimal)
                            int? effort = (int?)fieldValue;
                            if (effort.HasValue)
                            {
								// decimal hours = ((decimal)effort.Value) / 60;
								// xmlFieldNode.InnerText = hours.ToString("0.00");
								//var quotient = (Math.Floor((decimal)effort / 60)).ToString();
								//var remainder = effort % 60;
								//xmlFieldNode.InnerText = quotient.ToString() + ":" + remainder.ToString();
								decimal hoursDecimal = (decimal)effort / 60;

								// Determine whole hours and remaining minutes
								int wholeHours = (int)Math.Floor(hoursDecimal); // Whole hours
								int remainingMinutes = (int)(effort % 60); // Remaining minutes

								// Set the formatted duration
								xmlFieldNode.InnerText = effort.ToString(); // Output as decimal
							}
                        }
                        else if (fieldName.EndsWith("Duration") && fieldValue.GetType() == typeof(int))
                        {
                            //Duration fields should be displayed in hours (decimal)
                            int effort = (int)fieldValue;
							//decimal hours = ((decimal)effort) / 60;
							//xmlFieldNode.InnerText = hours.ToString("0.00");
							//var quotient = (Math.Floor((decimal)effort / 60)).ToString();
							//var remainder = effort % 60;
							//xmlFieldNode.InnerText = quotient.ToString() + ":" +remainder.ToString();	
							decimal hoursDecimal = (decimal)effort / 60;

							// Determine whole hours and remaining minutes
							int wholeHours = (int)Math.Floor(hoursDecimal); // Whole hours
							int remainingMinutes = effort % 60; // Remaining minutes

							// Set the formatted duration
							//xmlFieldNode.InnerText = $"{wholeHours}:{remainingMinutes:D2}"; // Format as HH:MM
							//decimal hoursDecimal = (decimal)effort / 60;
							xmlFieldNode.InnerText = effort.ToString(); // Output as decimal
						}
                        else if (fieldName.EndsWith("Duration") && fieldValue.GetType() == typeof(int?))
                        {
                            //Duration fields should be displayed in hours (decimal)
                            int? effort = (int?)fieldValue;
                            if (effort.HasValue)
                            {
								//decimal hours = ((decimal)effort.Value) / 60;
								//xmlFieldNode.InnerText = hours.ToString("0.00");
								//var quotient = (Math.Floor((decimal)effort / 60)).ToString();
								//var remainder = effort % 60;
								//xmlFieldNode.InnerText = quotient.ToString() + ":" + remainder.ToString();
								decimal hoursDecimal = (decimal)effort / 60;

								// Determine whole hours and remaining minutes
								int wholeHours = (int)Math.Floor(hoursDecimal); // Whole hours
								int remainingMinutes = (int)(effort % 60); // Remaining minutes

								// Set the formatted duration
								xmlFieldNode.InnerText = effort.ToString(); // Output as decimal
							}
                        }
                        else
                        {
                            xmlFieldNode.InnerText = fieldValue.ToString();
                        }
                    }
                }

                return xmlRootNode.InnerXml;
            }
        }

        /// <summary>
        /// Returns the current entity serialized XML
        /// </summary>
        /// <returns>The serialized XML including the entity itself</returns>
        public string OuterXml
        {
            get
            {
                return String.Format("<{0}>{1}</{0}>", this.GetType().Name, InnerXml);
            }
        }

        /// <summary>
        /// Returns the properties on the entity
        /// </summary>
        public Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                //Get the current type
                Type type = this.GetType();

                //Get the list of properties as a dictionary
                Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo propInfo in type.GetProperties())
                {
                    //Need to exclude the InnerXml and OuterXml properties because otherwise we'll get into a recursive loop
                    if (!properties.ContainsKey(propInfo.Name) && propInfo.Name != "InnerXml" && propInfo.Name != "OuterXml")
                    {
                        properties.Add(propInfo.Name, propInfo);
                    }
                }
                return properties;
            }
        }

        /// <summary>
        /// Allows the properties of the entity to be accessed like columns in a dataset using string values
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value (or Null if it doesn't exist)</returns>
        public object this[string propertyName]
        {
            get
            {
                //Get the current type
                Type type = this.GetType();

                //Get the property
                PropertyInfo propInfo = type.GetProperty(propertyName);
                if (propInfo == null)
                {
                    return null;
                }

                //now return its value, make sure it's readable and a non-indexed type
                if (!propInfo.CanRead || propInfo.GetIndexParameters().Length > 0)
                {
                    return null;
                }
                return propInfo.GetValue(this, null);
            }
            set
            {
                //Get the current type
                Type type = this.GetType();

                //Get the property
                PropertyInfo propInfo = type.GetProperty(propertyName);
                if (propInfo == null)
                {
                    throw new InvalidOperationException("The provided property name '" + propertyName + "' does not exist on the entity.");
                }

                //Make sure it's writable and doesn't use an indexer
                if (propInfo.CanWrite && propInfo.GetIndexParameters().Length == 0)
                {
                    propInfo.SetValue(this, value, null);
                }
            }
        }

        /// <summary>
        /// Gets the type for a specific property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The type of the property</returns>
        /// <remarks>Converts nullable types to their equivalent primitive ones</remarks>
        public Type GetPropertyType(string propertyName)
        {
            //Get the current type
            Type type = this.GetType();

            //Get the property
            PropertyInfo propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (propInfo == null)
            {
                return null;
            }

            //now return its value, convert generic types (int?) into a basic type (int)
            Type propertyType = propInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>))
            {
                return Nullable.GetUnderlyingType(propertyType);
            }
            else
            {
                return propertyType;
            }
        }

        #endregion
    }
}
