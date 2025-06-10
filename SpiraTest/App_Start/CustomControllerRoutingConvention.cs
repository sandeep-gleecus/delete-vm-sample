using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Inflectra.SpiraTest.Web.App_Start
{
	/// <summary>
	/// Routes all ODATA queries to the single RoutingController vs. having to have a unique controller per entity
	/// </summary>
	public class CustomControllerRoutingConvention : IODataRoutingConvention
	{
		public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
		{
			return null;
		}

		/// <summary>
		/// Always returns the generic ReportingController for ODATA queries
		/// </summary>
		public string SelectController(ODataPath odataPath, HttpRequestMessage request)
		{
			string controllerName = null;

			//See if we have an actual entity
			if (odataPath != null && odataPath.Segments.Count > 0)
			{
				//Use the generic SpiraODataController for all entities
				controllerName = "SpiraOData";
			}

			return controllerName;
		}
	}
}
