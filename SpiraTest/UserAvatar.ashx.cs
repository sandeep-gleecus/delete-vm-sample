using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>Summary description for UserAvatar</summary>
	public class UserAvatar : IHttpHandler
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserAvatar::";

		public void ProcessRequest(HttpContext context)
		{
			const string METHOD = CLASS_NAME + "ProcessRequest()";

            Logger.LogEnteringEvent(METHOD);

            try
            {
                int? userId = null;
                string themeName = "InflectraTheme";    //Default unless specified
                User user = null;

                //Get the User's ID from the querystring..
                try
                {
                    if (!string.IsNullOrWhiteSpace(context.Request.Params[GlobalFunctions.PARAMETER_USER_ID]))
                    {
                        int tryId;
                        if (Int32.TryParse(context.Request.Params[GlobalFunctions.PARAMETER_USER_ID], out tryId))
                            userId = tryId;
                    }
                }
                catch
                {
                    userId = null;
                }

                //Get the theme name if specified
                if (!String.IsNullOrWhiteSpace(context.Request.Params[GlobalFunctions.PARAMETER_THEME_NAME]))
                {
                    themeName = context.Request.Params[GlobalFunctions.PARAMETER_THEME_NAME].Trim();
                }

                //Now try to pull the user..
                if (userId.HasValue && userId.Value > 0)
                {
                    //Now get the user's avatar..
                    user = new UserManager().GetUserById(userId.Value, false);
                }

                if (user != null && user.Profile != null && !String.IsNullOrEmpty(user.Profile.AvatarImage))
                {
                    context.Response.Buffer = false;
                    if (String.IsNullOrEmpty(user.Profile.AvatarMimeType))
                    {
                        //Default to PNG
                        context.Response.ContentType = "image/png";
                    }
                    else
                    {
                        context.Response.ContentType = user.Profile.AvatarMimeType;
                    }
                    context.Response.AddHeader("Cache-Control", "max-age=604800"); //Cache only for a week.

                    //Need to convert the base64 image back into raw form
                    //http://stackoverflow.com/questions/5083336/decoding-base64-image
                    byte[] bitmapData = Convert.FromBase64String(user.Profile.AvatarImage);
                    context.Response.BinaryWrite(bitmapData);
                    context.Response.Flush();
                }
                else
                {
                    //Send them to the default avatar.
                    string fileName = HttpContext.Current.Server.MapPath("~/App_Themes/" + themeName + "/Images/artifact-Resource.svg");
                    context.Response.Buffer = false;
                    context.Response.ContentType = "image/svg+xml";
                    context.Response.AddHeader("Cache-Control", "max-age=604800"); //Cache only for a week.
                    context.Response.WriteFile(fileName);
                    context.Response.Flush();
                }
            }
            catch (System.Web.HttpException)
            {
                //Ignore these are if the client closes the connection
            }
            catch (Exception exception)
            {
                //Log but don't throw (just return nothing)
                Logger.LogErrorEvent(CLASS_NAME + METHOD, exception);
            }

            Logger.LogExitingEvent(METHOD);
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}