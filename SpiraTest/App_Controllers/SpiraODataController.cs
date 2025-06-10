using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using System.Threading;
using Microsoft.OData.Edm;

namespace Inflectra.SpiraTest.Web.App_Controllers
{
	/// <summary>
	/// The controller for the ODATA query API
	/// </summary>
	/// <remarks>
	/// https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
	/// https://www.c-sharpcorner.com/article/using-odata-query-options-in-asp-net-web-api/
	/// https://stackoverflow.com/questions/39515218/odata-error-the-query-specified-in-the-uri-is-not-valid-the-property-cannot-be
	/// http://localhost/Spira/api/odata/Tasks?$filter=NAME%20eq%20%27Book%27
	/// https://weblogs.asp.net/jongalloway/does-asp-net-web-api-odata-filter-at-the-database-level-let-s-ask-intellitrace
	/// https://forums.asp.net/t/1807082.aspx?How+to+efficiently+dispose+the+ObjectContext+after+the+web+api+query+
	/// https://stackoverflow.com/questions/17111386/asp-net-webapi-generic-controller-for-odata-endpoint
	/// https://stackoverflow.com/questions/57806757/is-there-any-way-to-make-generic-action-method-in-controllerodatacontroller
	/// https://blog.scottlogic.com/2015/12/01/generalizing-odata.html
	/// https://stackoverflow.com/questions/53203378/odata-navigation-routing-for-a-generic-controller-with-odataqueryoptions-support
	/// https://github.com/EntityRepository/ODataServer
	/// http://localhost/Spira/api/odata/Tasks?username=administrator&api-key={XXXXX}
	/// </remarks>
	public partial class SpiraODataController : Microsoft.AspNet.OData.MetadataController
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.App_Controllers.SpiraODataController::";

		#region Authentication and Authorization

		/// <summary>
		/// Is the current user authenticated
		/// </summary>
		protected bool IsAuthenticated
		{
			get
			{
				if (Thread.CurrentPrincipal == null)
				{
					return false;
				}
				else if (Thread.CurrentPrincipal.Identity == null)
				{
					return false;
				}
				else
				{
					return Thread.CurrentPrincipal.Identity.IsAuthenticated;
				}
			}
		}

		/// <summary>
		/// The user id of the authenticated user (or null)
		/// </summary>
		protected int? AuthenticatedUserId
		{
			get
			{
				if (Thread.CurrentPrincipal == null)
				{
					return null;
				}
				else if (Thread.CurrentPrincipal.Identity == null)
				{
					return null;
				}
				else
				{
					int userId;
					if (Int32.TryParse(Thread.CurrentPrincipal.Identity.Name, out userId))
					{
						return userId;
					}
					return null;
				}
			}
		}

		/// <summary>
		/// Verifies that the current user is authenticated and authorized to run ODATA queries
		/// </summary>
		/// <remarks>
		/// Currently requires System Administrator or Report Admin permissions
		/// </remarks>
		protected void AuthenticateAndAuthorizeUser()
		{
			//Make sure ODATA is enabled at all
			if (!Common.Global.Feature_OData)
			{
				throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) { Content = new StringContent(Resources.Messages.Services_OData_NotSupported) });
			}
			
			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { Content = new StringContent(Resources.Messages.Services_AuthorizationFailure) });
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { Content = new StringContent(Resources.Messages.Services_AuthorizationFailure) });
			}

			//Make sure we're a system administrator or report admin
			try
			{
				User user = new UserManager().GetUserById(userId.Value);
				if (user == null || (!user.Profile.IsAdmin && !user.Profile.IsReportAdmin) || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { Content = new StringContent(Resources.Messages.Services_NotAuthorizedViewSystemWideData) });
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { Content = new StringContent(Resources.Messages.Services_AuthorizationFailure) });
			}
		}

		#endregion
	}
}
