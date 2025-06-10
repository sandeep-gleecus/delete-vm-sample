namespace Inflectra.OAuth2
{
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Security;


	public static class OAuth2Client
	{
		public static Uri OAuthCallbackOrigin { get; set; }
		public static string OAuthCallbackUrl { get; set; }
		public static bool AutoRegisterOAuthCallbackUrl { get; set; }
		public static string AuthorizationContextCookieName { get; set; }

		static OAuth2Client()
		{
			AutoRegisterOAuthCallbackUrl = false;
			OAuthCallbackUrl = "oauth";
			AuthorizationContextCookieName = "oauth2authctx";
		}


		//public static void RegisterCustomOAuthCallback(RouteCollection routes, string action, string controller, string area = null)
		//{
		//	routes.MapRoute(
		//		"OAuthCallback",
		//		OAuth2Client.OAuthCallbackUrl,
		//		new { controller, action, area });
		//}

	}
}
