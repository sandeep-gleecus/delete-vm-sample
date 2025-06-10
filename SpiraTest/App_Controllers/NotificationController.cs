using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Validation.Reports.Windward.Core.Model;

namespace Inflectra.SpiraTest.Web.App_Controllers
{
	[AllowAnonymous]
	[RoutePrefix("api/notification")]
	public class NotificationController : ApiController
	{
		private string EMAIL_BODY_TEMPLATE = "The {0} is scheduled for periodic review on {1}.  Please review the validation documentation in preparation for this upcoming date.  If you have any questions, please contact the validation program administrator/ manager";


		[Route("periodic-review")]
		[HttpPost]
		public HttpResponseMessage ProcessPeriodicReviewNotifications()
		{
			ReleaseManager releaseManager = new ReleaseManager();
			NotificationManager notificationManager = new NotificationManager();

			var list = releaseManager.RetrievePeriodicReviewNotifications(DateTime.Today);

			foreach (var item in list)
			{
				var currentRelease = releaseManager.RetrieveById3(null, (int)item.ReleaseId, false);
				if (currentRelease.PeriodicReviewDate.HasValue)
				{
					var emailBody = String.Format(EMAIL_BODY_TEMPLATE, currentRelease.FullName, currentRelease.PeriodicReviewDate.Value);
					NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();

					msgToSend.subjectList.Add(0, "PENDING PERIODIC REVIEW");
					msgToSend.projectToken = "XX-xx";
					msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
					{
						Address = "pandyaram@hotmail.com",
						SubjectId = 0,
						UserId =1,
						ArtifactTokenId = -1,
						Name = "Sekhar Pandyaram",
						Source = "PeriodicReviewNotification"
					});

					notificationManager.SendEmail(msgToSend, emailBody);
				}
				

			}

			return Request.CreateResponse(HttpStatusCode.OK);
		}


	}
}
