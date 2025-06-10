using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using System.Reflection;

namespace Inflectra.SpiraTest.Web.App_Start
{
	/// <summary>
	/// Responsible for initializing the Web API
	/// </summary>
	public static class WebApiConfig
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.App_Start.WebApiConfig::";

		/// <summary>
		/// The maximum number of rows in a single query page
		/// </summary>
		private const int MAX_ROWS = 250;

		/// <summary>
		/// Registers all the Web API routes
		/// </summary>
		/// <param name="config">The HTTP configuration</param>
		/// <remarks>
		/// Currently we use WebAPI for the Spira ODATA Endpoint, all other APIs use WCF or ASMX endpoints
		/// </remarks>
		public static void Register(HttpConfiguration config)
		{
			const string METHOD_NAME = "Register";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of reportable entities
				Dictionary<string, string> reportableEntities = ReportManager.GetReportableEntities();

				//Create the OData model builder, only used for querying
				ODataModelBuilder builder = new ODataModelBuilder();

				//Add the reportable entities
				foreach (KeyValuePair<string, string> reportableEntity in reportableEntities)
				{
					//e.g. Key="SpiraTestEntities.R_RequirementIncidents", Value="Requirement Incidents"
					//Remove any spaces in the display name
					string entitySet = reportableEntity.Key.Replace("SpiraTestEntities.", ""); 
					string displayName = reportableEntity.Value.Replace(" ", "");
					ConfigureEntity(builder, entitySet, displayName);
				}

				//Configure the API routes and query options allowed
				config.Count().Filter().OrderBy().Expand().Select().MaxTop(MAX_ROWS);
				IList<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefault();
				routingConventions.Insert(0, new CustomControllerRoutingConvention());
				config.MapODataServiceRoute(
					routeName: "ODataRoute",
					routePrefix: "api/odata",
					model: builder.GetEdmModel(),
					routingConventions: routingConventions,
					pathHandler: new DefaultODataPathHandler()
					);

				//Add authentication handling
				config.MessageHandlers.Add(new ApiAuthenticationHandler());

				config.MapHttpAttributeRoutes();

				config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "api/{controller}/{id}",
					defaults: new { id = RouteParameter.Optional }
				);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Configures a specific reportable entity for use by the ODATA 4.0 Api
		/// </summary>
		/// <param name="builder">The ODATA builder</param>
		/// <param name="entitySet">The entity set (e.g. R_RequirementIncidents)</param>
		/// <param name="displayName">The display name (e.g. "RequirementIncidents)</param>
		private static void ConfigureEntity(ODataModelBuilder builder, string entitySet, string displayName)
		{
			const string METHOD_NAME = "ConfigureEntity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME + " (" + entitySet + ")");

			try
			{
				//Create the entity reference
				Type entityType = ReportManager.GetEntitySetItemType(entitySet);
				if (entityType != null)
				{
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, String.Format("Adding entity '{0}' to ODATA model", entityType.FullName));
					var typeConfig = builder.AddEntityType(entityType);
					var entity = builder.AddEntitySet(displayName, typeConfig);

					//Add the keys and properties using reflection
					PropertyInfo[] properties = entityType.GetProperties();
					foreach (PropertyInfo propInfo in properties)
					{
						Type type = propInfo.PropertyType;
						if (type.IsPrimitive || type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(String))
						{
							//Non-nullable integers/big-ints are keys
							if (!type.IsGenericType && (type == typeof(Int32) || type == typeof(Int64)))
							{
								//Key property
								typeConfig.HasKey(propInfo);
							}
							else
							{
								//Other property
								typeConfig.AddProperty(propInfo);
							}
						}
						else if (type.IsGenericType
									&& type.GetGenericTypeDefinition() == typeof(Nullable<>)
									&& type.GetGenericArguments().Any(t => t.IsValueType && t.IsPrimitive))
						{
							//Nullable primitive types
							typeConfig.AddProperty(propInfo);
						}						
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}
}
