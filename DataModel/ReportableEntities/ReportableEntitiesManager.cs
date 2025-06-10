using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Inflectra.SpiraTest.DataModel.ReportableEntities
{
    /// <summary>
    /// Contains some static utility functions for handling the reportable entities
    /// </summary>
    public static class ReportableEntitiesManager
    {
		/// <summary>
		/// Returns the Type of an entity (needed by other assemblies)
		/// </summary>
		/// <param name="typeFullName">The type name</param>
		/// <returns>The corresponding type object</returns>
		public static Type GetEntityType(string typeFullName)
		{
			Type type = Type.GetType(typeFullName);
			return type;
		}

		/// <summary>
		/// Retrieves a list of the reportable entities in the system (key=EntityName, Value=DisplayName)
		/// </summary>
		/// <returns>These are the name of the reportable entity views that can be dynamically queried</returns>
		public static Dictionary<string,string> GetReportableEntities()
        {
            Dictionary<string,string> reportableEntities = new Dictionary<string,string>();

            //We use reflection to get all the entities in the appropriate DataModel subpackage namespace
            //Exclude abstract sealed classes since that excludes static classes such as the this utility class
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && !(t.IsAbstract || t.IsSealed) &&
                          t.Namespace == "Inflectra.SpiraTest.DataModel.ReportableEntities"
                    orderby t.Name
                    select t;
            List<Type> types = q.ToList();

            foreach (Type type in types)
            {
                //See if the type implements the ReportableEntity attribute
                ReportableEntityAttribute reportableEntityAttribute = (ReportableEntityAttribute)System.Attribute.GetCustomAttribute(type, typeof(ReportableEntityAttribute));

                if (reportableEntityAttribute != null)
                {
                    string displayName = reportableEntityAttribute.DisplayName;
                    string entityName = reportableEntityAttribute.EntitySet;
                    if (!reportableEntities.ContainsKey(entityName))
                    {
                        reportableEntities.Add(entityName, displayName);
                    }
                }
            }

            return reportableEntities;
        }
	}
}
