using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml.Serialization;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Classes
{
	public class UrlRewriterModule : IHttpModule
	{
		private const string CLASS_NAME = "Web.Classes.UrlRewriterModule::";

		/// <summary>
		/// Stores all the configured rewrites for the application.
		/// </summary>
		private static List<RewriterRule> REWRITES = null;

		#region IHttpModule Members
		/// <summary>Disposes the HTTPModule</summary>
		void IHttpModule.Dispose() { }

		/// <summary>Upon creating and loading a new instance of this HttpModule</summary>
		/// <param name="context">HttpApplication</param>
		void IHttpModule.Init(HttpApplication context)
		{
			// WARNING!  This does not work with Windows authentication!
			// If you are using Windows authentication, change to app.BeginRequest
			context.AuthorizeRequest += context_AuthorizeRequest;

			// On initilization of the DLL, we need to make sure that all rules have
			//  a generated Regex.
			if (REWRITES == null)
			{
				//We load all of the rewriter URLs here.
				REWRITES = new List<RewriterRule>();
				RewriterRuleCollection rules = RewriterConfigurationManager.Config.Rules;
				if (rules != null)
				{
					// iterate through each rule...
					foreach (RewriterRule rule in rules)
					{
						Debug.Write("Creating Regex for rule '" + rule.LookFor + "'...");
						string regExUrl = "^" + ResolveUrl(context.Context.Request.ApplicationPath, rule.LookFor) + "$";
						rule._compiled = new Regex(regExUrl, RegexOptions.IgnoreCase | RegexOptions.Compiled); //Set the stored field to the new compiled Regex.
						REWRITES.Add(rule); //Add the rule to our collection.
						Debug.WriteLine(" Done.");
					}
				}
			}
		}
		#endregion

		#region Event Handler
		/// <summary>Hit when the webserver goes to Authenticate a request.</summary>
		/// <param name="sender">HttpApplication</param>
		/// <param name="e">EventArgs</param>
		private void context_AuthorizeRequest(object sender, EventArgs e)
		{
			const string MODULE_NAME = CLASS_NAME + "context_AuthorizeRequest()";
			Logger.LogEnteringEvent(MODULE_NAME);

			HttpApplication app = (HttpApplication)sender;
			//Don't rewrite web service or resource URLs
			if (!app.Request.Path.EndsWith(".svc") && !app.Request.Path.EndsWith(".axd") && !app.Request.Path.EndsWith(".svc/js") && !app.Request.Path.EndsWith(".svc/jsdebug"))
			{
				Rewrite(app.Request.Path, app);
			}
			Logger.LogExitingEvent(MODULE_NAME);
		}
		#endregion

		/// <summary>This method is called during the module's BeginRequest event.</summary>
		/// <param name="requestedPath">The RawUrl being requested (includes path and querystring).</param>
		/// <param name="app">The HttpApplication instance.</param>
		private void Rewrite(string requestedPath, System.Web.HttpApplication app)
		{
			const string MODULE_NAME = CLASS_NAME + "Rewrite()";
			Logger.LogEnteringEvent(MODULE_NAME);
			Logger.LogTraceEvent(MODULE_NAME, " -- AppPath: " + app.Context.Request.ApplicationPath);

			// log information to the Trace object.
			app.Context.Trace.Write("ModuleRewriter", "Entering ModuleRewriter");

			//Loop through each of our Regexes.
			foreach (var rewrite in REWRITES)
			{
				// See if a match is found
				if (rewrite._compiled.IsMatch(requestedPath))
				{
					//Match found. If it's a redirect, stop here and redirect!
					string sendToUrl = ResolveUrl(
						app.Context.Request.ApplicationPath,
						rewrite._compiled.Replace(requestedPath, rewrite.SendTo)
					);
					if (rewrite.Redirect)
					{
						/* A redirect is a PERMANENT change. We send the HTTP 301 code. This is
						*    important to know, since browsers may (of very often do) cache the
						*    reply, so that the next time the user hits this URL, the browser
						*    automatically redirects it. (Meaning, the user's request MAY
						*    not his this code again for the same URL, as the browser remembers
						*    where that URL went to. */
						HttpContext.Current.Response.RedirectPermanent(sendToUrl, true);
						break;
					}
					else
					{
						// log rewriting information to the Trace object
						//Logger.LogTraceEvent(CLASS_NAME, "Rewriting URL: \"" + requestedPath + "\" »» \"" + sendToUrl + "\"");

						// Rewrite the URL
						RewriteUrl(app.Context, sendToUrl);
						break; // exit the for loop
					}
				}

			}

			// get the configuration rules
			RewriterConfiguration configuration = RewriterConfigurationManager.Config;
			if (configuration != null)
			{
				RewriterRuleCollection rules = configuration.Rules;
				if (rules != null)
				{
					// iterate through each rule...
					for (int i = 0; i < rules.Count; i++)
					{
						// get the pattern to look for, and Resolve the Url (convert ~ into the appropriate directory)
						string lookFor = "^" + ResolveUrl(app.Context.Request.ApplicationPath, rules[i].LookFor) + "$";

						// Create a regex (note that IgnoreCase is set...)
						Regex re = new Regex(lookFor, RegexOptions.IgnoreCase);

					}
				}
			}

			// Log information to the Trace object
			app.Context.Trace.Write("ModuleRewriter", "Exiting ModuleRewriter");

			Logger.LogExitingEvent(MODULE_NAME);
		}

		#region Static Utils
		/// <summary>Rewrite's a URL using HttpContext.RewriteUrl().</summary>
		/// <param name="context">The HttpContext object to rewrite the URL to.</param>
		/// <param name="sendToUrl">The URL to rewrite to.</param>
		internal static void RewriteUrl(HttpContext context, string sendToUrl)
		{
			const string MODULE_NAME = CLASS_NAME + "RewriteUrl()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string x, y;
			RewriteUrl(context, sendToUrl, out x, out y);

			Logger.LogExitingEvent(MODULE_NAME);
		}

		/// <summary>Rewrite's a URL using HttpContext.RewriteUrl().</summary>
		/// <param name="context">The HttpContext object to rewrite the URL to.</param>
		/// <param name="sendToUrl">The URL to rewrite to.</param>
		/// <param name="sendToUrlLessQString">Returns the value of sendToUrl stripped of the querystring.</param>
		/// <param name="filePath">Returns the physical file path to the requested page.</param>
		internal static void RewriteUrl(HttpContext context, string sendToUrl, out string sendToUrlLessQString, out string filePath)
		{
			const string MODULE_NAME = CLASS_NAME + "RewriteUrl()";
			Logger.LogEnteringEvent(MODULE_NAME);

			// see if we need to add any extra querystring information
			if (context.Request.QueryString.Count > 0)
			{
				if (sendToUrl.IndexOf('?') != -1)
					sendToUrl += "&" + context.Request.QueryString.ToString();
				else
					sendToUrl += "?" + context.Request.QueryString.ToString();
			}

			// first strip the querystring, if any
			string queryString = string.Empty;
			sendToUrlLessQString = sendToUrl;
			if (sendToUrl.IndexOf('?') > 0)
			{
				sendToUrlLessQString = sendToUrl.Substring(0, sendToUrl.IndexOf('?'));
				queryString = sendToUrl.Substring(sendToUrl.IndexOf('?') + 1);
			}

			// grab the file's physical path
			filePath = context.Server.MapPath(sendToUrlLessQString);

			// rewrite the path...
			//Logger.LogTraceEvent(MODULE_NAME, "HttpContext.RewritePath('" + sendToUrlLessQString + "', '', '" + queryString + "')");
			context.RewritePath(sendToUrlLessQString, string.Empty, queryString);

			Logger.LogExitingEvent(MODULE_NAME);
		}

		/// <summary>Converts a URL into one that is usable on the requesting client.</summary>
		/// <param name="url">The URL, which might contain ~.</param>
		/// <returns>A resolved URL.  If the input parameter url contains ~, it is replaced with the value of the appPath parameter.</returns>
		public static string ResolveUrl(string url)
		{
			const string MODULE_NAME = CLASS_NAME + "ResolveUrl()";
			Logger.LogEnteringEvent(MODULE_NAME + " - " + url);

			string retString = ResolveUrl(HttpContext.Current.Request.ApplicationPath, url);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Returns the URL for the given user's avatar image, or the default one if a null UserId is given.</summary>
		/// <param name="userId">The user ID to pull the image for.</param>
		/// <param name="themeName">The name of the current theme</param>
		/// <returns>The URL pointing to the user's avatar.</returns>
		public static string ResolveUserAvatarUrl(int? userId, string themeName = "")
		{
			string retUrl = HttpContext.Current.Request.ApplicationPath;
			if (retUrl != "/")
			{
				retUrl += "/";
			}
			retUrl += "UserAvatar.ashx";
			retUrl += "?" + GlobalFunctions.PARAMETER_USER_ID + "=";
			retUrl += ((userId.HasValue) ? userId.Value.ToSafeString() : "0");
			if (!string.IsNullOrEmpty(themeName))
			{
				retUrl += "&" + HttpUtility.UrlEncode(themeName);
			}

			return retUrl;
		}

		/// <summary>Requests a 'pretty' URL to use.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveRewriterURL(UrlRoots.NavigationLinkEnum navigationLink)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveRewriterURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveURL(navigationLink, 0, 0, null);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="projectId">The id of the project (-3 sets {0} for use in format strings)</param>
		/// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveProjectAdminUrl(int projectId, string administrationPage)
		{
			return UrlRoots.RetrieveProjectAdminUrl(projectId, administrationPage);
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveTemplateAdminUrl(int projectTemplateId, string administrationPage)
		{
			return UrlRoots.RetrieveTemplateAdminUrl(projectTemplateId, administrationPage);
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveGroupAdminUrl(int projectGroupId, string administrationPage)
		{
			return UrlRoots.RetrieveGroupAdminUrl(projectGroupId, administrationPage);
		}

		/// <summary>Requests a 'pretty' URL to use.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="projectId">The project ID of the artifact. Ignored if not needed.</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int projectId)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveRewriterURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveURL(navigationLink, projectId, 0, null);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Requests a 'pretty' URL to use.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="ProjectID">The project ID of the artifact. Ignored if not needed.</param>
		/// <param name="ArtifactID">The ID of the artifact. Ignored if not needed, specifying -2 will insert the token {art} for the ArtifactID, and specifying -3 will insert the token {0} for use in format strings, -4 will specify the Table view and -5 will specify the Board view</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int ProjectID, int ArtifactID)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveRewriterURL(NavigationlinkEnum,int,int)";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveURL(navigationLink, ProjectID, ArtifactID, null);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Requests a 'pretty' URL to use.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="ProjectID">The project ID of the artifact. Ignored if not needed. Specifying -1 will skip the project specifier part of the URL.</param>
		/// <param name="ArtifactID">The ID of the artifact. Ignored if not needed, specifying -2 will insert the token {art} for the ArtifactID, and specifying -3 will insert the token {0} for use in format strings, -4 will specify the Table view and -5 will specify the Board view</param>
		/// <param name="TabName">The name of the tab or extra item in the URL. Null if not specified.</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int ProjectID, int ArtifactID, string TabName)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveRewriterURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveURL(navigationLink, ProjectID, ArtifactID, TabName);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Requests a 'pretty' URL to use.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="projectId">The project ID of the artifact. Ignored if not needed. Specifying -1 will skip the project specifier part of the URL.</param>
		/// <param name="artifactID">The ID of the artifact. Ignored if not needed, specifying -2 will insert the token {art} for the ArtifactID, and specifying -3 will insert the token {0} for use in format strings, -4 will specify the Table view and -5 will specify the Board view</param>
		/// <param name="secondaryArtifactId">The id of any seconary ids needed (for some details pages).</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int projectId, int artifactId, int secondaryArtifactId)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveRewriterURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveURL(navigationLink, projectId, artifactId, secondaryArtifactId.ToString());

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Requests a 'pretty' URL to use for project group navigation items.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="tabName">The name of any page suffix after the main URL</param>
		/// <param name="projectGroupId">The project group ID of the artifact. Ignored if not needed.</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int projectGroupId, string tabName = "")
		{
			const string MODULE_NAME = CLASS_NAME + "RetrieveGroupRewriterURL";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrieveGroupURL(navigationLink, projectGroupId, false, tabName);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Requests a 'pretty' URL to use for portfolio navigation items.</summary>
		/// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
		/// <param name="portfolioId">The portfolio ID of the artifact. Ignored if not needed.</param>
		/// <returns>String of the new URL.</returns>
		public static string RetrievePortfolioRewriterURL(UrlRoots.NavigationLinkEnum navigationLink, int portfolioId)
		{
			const string MODULE_NAME = CLASS_NAME + "RetrievePortfolioRewriterURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			string retString = UrlRoots.RetrievePortfolioURL(navigationLink, portfolioId);

			Logger.LogExitingEvent(MODULE_NAME);
			return retString;
		}

		/// <summary>Goes through the rules
		/// </summary>
		/// <param name="rawUrl"></param>
		/// <returns></returns>
		public static string GetSiteMapURL(string rawUrl)
		{
			const string MODULE_NAME = CLASS_NAME + "GetSiteMapURL()";
			Logger.LogEnteringEvent(MODULE_NAME);

			//If we are installed on the root of the website (ApplicationPath = /) we need to handle the case separately
			string cleanURL;
			if (HttpContext.Current.Request.ApplicationPath == "/")
			{
				cleanURL = rawUrl;
			}
			else
			{
				cleanURL = rawUrl.Replace(HttpContext.Current.Request.ApplicationPath, "");
			}
			cleanURL = ((cleanURL.Contains("?")) ? cleanURL.Split('?')[0] : cleanURL);

			//Split it up into groups.
			string[] urlGroups = cleanURL.Split('/');

			bool foundProjId = false;
			bool foundArtId = false;
			bool foundTabId = false;
			bool foundProjectGroupToken = false;
			bool foundProjectGroupId = false;
			for (int i = 0; i < urlGroups.Length; i++)
			{
				//See if we have a project group 'pg' url
				if (foundProjectGroupToken)
				{
					if (GlobalFunctions.IsInteger(urlGroups[i]))
					{
						if (!foundProjectGroupId)
						{
							urlGroups[i] = "{projGroupId}";
							foundProjectGroupId = true;
						}
					}
					if (urlGroups[i].ToLowerInvariant().EndsWith(".aspx") || urlGroups[i].ToLowerInvariant().EndsWith(".aspx#"))
					{
						//Get the project group id (5.aspx for example)
						string[] pageSplit = urlGroups[i].Split('.');
						if (GlobalFunctions.IsInteger(pageSplit[0]))
						{
							urlGroups[i] = "{projGroupId}.aspx";
							foundProjectGroupId = true;
						}
					}
				}
				else
				{
					if (urlGroups[i].ToLowerInvariant() == UrlRoots.PROJECT_GROUP_PREFIX)
					{
						foundProjectGroupToken = true;
					}
					else if (GlobalFunctions.IsInteger(urlGroups[i]))
					{
						if (!foundProjId)
						{
							urlGroups[i] = "{projId}";
							foundProjId = true;
						}
						else if (!foundArtId)
						{
							urlGroups[i] = "{artId}";
							foundArtId = true;
						}
					}
					else
					{
						if (urlGroups[i].ToLowerInvariant().EndsWith(".aspx") || urlGroups[i].ToLowerInvariant().EndsWith(".aspx#"))
						{
							//Could be the Project ID (only if it's the first one we found)
							//  or the Item ID (if the ProjId has been found)
							//  or the Tab ID (only if we have already found the ProjId & ArtId and it's not a number).
							string[] pageSplit = urlGroups[i].Split('.');
							if (GlobalFunctions.IsInteger(pageSplit[0]))
							{
								//It's a number. Either the ProjId or ArtId
								if (!foundProjId)
								{
									urlGroups[i] = "{projId}.aspx";
									foundProjId = true;
								}
								else if (!foundArtId)
								{
									urlGroups[i] = "{artId}.aspx";
									foundArtId = true;
								}
							}
							else
							{
								//It was text. If we found the ProjId and ArtId, then it's the tab.
								if (foundProjId && foundArtId && !foundTabId)
								{
									urlGroups[i] = "{tabId}.aspx";
								}
							}
						}
					}
				}
			}

			Logger.LogExitingEvent(MODULE_NAME);
			//Recombine the split..
			if (HttpContext.Current.Request.ApplicationPath == "/")
			{
				return string.Join("/", urlGroups);
			}
			else
			{
				return HttpContext.Current.Request.ApplicationPath + string.Join("/", urlGroups);
			}
		}

		/// <summary>Will decode a passed URL into it's respective items. Available dictionary keys are: 'projId', 'artId', 'tabId', 'artType', values matching those in the web.rewrite file. </summary>
		/// <param name="RawUrl">A string that contains the URL.</param>
		/// <returns>Dictionary of key:string, value:object of keys in the URL.</returns>
		public static Dictionary<string, object> DecodeURL(string RawUrl)
		{
			const string MODULE_NAME = CLASS_NAME + "DecodeURL(string)";
			Logger.LogEnteringEvent(MODULE_NAME);

			Dictionary<string, object> retDict = new Dictionary<string, object>();

			//Decode the path, if necessary.
			string propUrl = ResolveUrl(HttpContext.Current.Request.ApplicationPath, RawUrl);
			//Get the tokens for the URL,
			string redirectUrl = GetSiteMapURL(propUrl);
			//Insert Regex tokens.
			redirectUrl = redirectUrl.Replace("{artId}", "([0-9]+?)");
			redirectUrl = redirectUrl.Replace("{projId}", "([0-9]+?)");
			redirectUrl = redirectUrl.Replace("{tabId}", "([a-zA-Z ]+?)");
			redirectUrl = redirectUrl.Replace(".", @"\.");
			//Run regEx.
			Match redMatch = new Regex(redirectUrl).Match(HttpContext.Current.Request.RawUrl);

			//Now get our items:
			if (redMatch.Groups[1].Success && GlobalFunctions.IsInteger(redMatch.Groups[1].Value))
				retDict.Add("projId", int.Parse(redMatch.Groups[1].Value));
			if (redMatch.Groups[2].Success && GlobalFunctions.IsInteger(redMatch.Groups[2].Value))
				retDict.Add("artId", int.Parse(redMatch.Groups[2].Value));
			if (redMatch.Groups[3].Success)
				retDict.Add("tabId", redMatch.Groups[3].Value);

			//Now search for our page type.
			bool foundPage = false;
			UrlRoots.NavigationLinkEnum pageType = UrlRoots.NavigationLinkEnum.None;
			while (!foundPage)
			{
				string[] levels = redMatch.Groups[0].Value.Split('/');
				for (int i = 0; i < levels.Length; i++)
				{
					//Lower it, and remove any extension.
					string tokenSearch = levels[i].Replace(".aspx", "").ToLowerInvariant();
					switch (tokenSearch)
					{
						case "grouphome":
							pageType = UrlRoots.NavigationLinkEnum.ProjectGroupHome;
							foundPage = true;
							break;
						case "document":
							pageType = UrlRoots.NavigationLinkEnum.Documents;
							foundPage = true;
							break;
						case "attachment":
							pageType = UrlRoots.NavigationLinkEnum.Attachment;
							foundPage = true;
							break;
						case "requirement":
							pageType = UrlRoots.NavigationLinkEnum.Requirements;
							foundPage = true;
							break;
						case "testcase":
							pageType = UrlRoots.NavigationLinkEnum.TestCases;
							foundPage = true;
							break;
						case "testrun":
							pageType = UrlRoots.NavigationLinkEnum.TestRuns;
							foundPage = true;
							break;
						case "teststeprun":
							//Technically this is broken, because the 'secondaryId' isn't defined.
							pageType = UrlRoots.NavigationLinkEnum.TestStepRuns;
							foundPage = true;
							break;
						case "teststep":
							pageType = UrlRoots.NavigationLinkEnum.TestSteps;
							foundPage = true;
							break;
						case "testset":
							pageType = UrlRoots.NavigationLinkEnum.TestSets;
							foundPage = true;
							break;
						case "incident":
							pageType = UrlRoots.NavigationLinkEnum.Incidents;
							foundPage = true;
							break;
						case "release":
							pageType = UrlRoots.NavigationLinkEnum.Releases;
							foundPage = true;
							break;
						case "task":
							pageType = UrlRoots.NavigationLinkEnum.Tasks;
							foundPage = true;
							break;
						case "report":
							pageType = UrlRoots.NavigationLinkEnum.Reports;
							foundPage = true;
							break;
						case "login":
							pageType = UrlRoots.NavigationLinkEnum.Login;
							foundPage = true;
							break;
						case "errorpage":
							pageType = UrlRoots.NavigationLinkEnum.ErrorPage;
							foundPage = true;
							break;
						case "mypage":
							pageType = UrlRoots.NavigationLinkEnum.MyPage;
							foundPage = true;
							break;
					}
				}
				//Check to see if it was the project home page.
				if (!foundPage && retDict.ContainsKey("projId"))
				{
					pageType = UrlRoots.NavigationLinkEnum.ProjectHome;
				}
				//Now force an exit. No need to loop forever.
				foundPage = true;
			}

			//Add the PageType only if it wasn't none.
			if (pageType != UrlRoots.NavigationLinkEnum.None)
				retDict.Add("artType", pageType);

			Logger.LogExitingEvent(MODULE_NAME);

			return retDict;
		}

		/// <summary>
		/// Resolves an Images/X.gif URL to the current App_Themes folder
		/// </summary>
		/// <param name="imagesUrl">The URL that is relative to the App_Themes folder</param>
		/// <returns>An absolute URL</returns>
		public static string ResolveImages(string imagesUrl, Page page)
		{
			const string MODULE_NAME = CLASS_NAME + "ResolveImages()";
			Logger.LogEnteringEvent(MODULE_NAME + " - " + imagesUrl);

			if (imagesUrl == null)
			{
				return null;
			}

			//If we have http in the image already, don't make any changes
			if (imagesUrl.Length > 4 && imagesUrl.ToLowerInvariant().Substring(0, 4) == "http")
			{
				Logger.LogExitingEvent(MODULE_NAME);
				return imagesUrl;
			}
			if (HttpContext.Current.Request.ApplicationPath == "/")
			{
				Logger.LogExitingEvent(MODULE_NAME);
				return imagesUrl.Replace("Images/", "/App_Themes/" + page.Theme + "/Images/");
			}
			else
			{
				Logger.LogExitingEvent(MODULE_NAME);
				return imagesUrl.Replace("Images/", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + page.Theme + "/Images/");
			}

		}

		/// <summary>Converts a URL into one that is usable on the requesting client, translating the ApplicationPath for '~'</summary>
		/// <param name="appPath">The application path.</param>
		/// <param name="url">The URL, which might contain ~.</param>
		/// <returns>A resolved URL.  If the input parameter url contains ~, it is replaced with the value of the appPath parameter.</returns>
		public static string ResolveUrl(string appPath, string url)
		{
			//Turned off logging because this function gets called too many times
			//Logger.LogEnteringEvent(CLASS_NAME + MODULE_NAME);

			string retUrl;
			if (url.Length == 0 || url[0] != '~')
				retUrl = url;       // there is no ~ in the first character position, just return the url
			else
			{
				if (url.Length == 1)
					retUrl = appPath;  // there is just the ~ in the URL, return the appPath
				if (url[1] == '/' || url[1] == '\\')
				{
					// url looks like ~/ or ~\
					if (appPath.Length > 1)
						retUrl = appPath + "/" + url.Substring(2);
					else
						retUrl = "/" + url.Substring(2);
				}
				else
				{
					// url looks like ~something
					if (appPath.Length > 1)
						retUrl = appPath + "/" + url.Substring(1);
					else
						retUrl = appPath + url.Substring(1);
				}
			}

			//Turned off logging because this function gets called too many times
			//Logger.LogExitingEvent(CLASS_NAME + MODULE_NAME);
			return retUrl.TrimEnd('#');
		}
		#endregion

		#region RewriterRule
		/// <summary>Represents a rewriter rule.  A rewriter rule is composed of a pattern to search for and a string to replace the pattern with (if matched).</summary>
		[Serializable()]
		public class RewriterRule
		{
			/// <summary>Gets or sets the pattern to look for.</summary>
			public string LookFor
			{
				get;
				set;
			}

			/// <summary>The string to replace the pattern with, if found.</summary>
			public string SendTo
			{
				get;
				set;
			}

			/// <summary>Whether it's a simple redirect or not.</summary>
			[XmlAttribute("Redirect")]
			public bool Redirect
			{
				get;
				set;
			}

			/// <summary>Generated on first app launch, to help with performance.</summary>
			[XmlIgnore()]
			public Regex _compiled { get; set; }
		}

		#endregion

		#region Helper Classes
		/// <summary>The RewriterRuleCollection models a set of RewriterRules in the Web.config file.</summary>
		[Serializable()]
		public class RewriterRuleCollection : CollectionBase
		{
			/// <summary>Adds a new RewriterRule to the collection.</summary>
			/// <param name="r">A RewriterRule instance.</param>
			public virtual void Add(RewriterRule r)
			{
				this.InnerList.Add(r);
			}

			/// <summary>Gets or sets a RewriterRule at a specified ordinal index.</summary>
			public RewriterRule this[int index]
			{
				get
				{
					return (RewriterRule)this.InnerList[index];
				}
				set
				{
					this.InnerList[index] = value;
				}
			}
		}

		/// <summary>Specifies the configuration settings in the Web.config for the RewriterRule.</summary>
		[Serializable()]
		[XmlRoot("RewriterConfig")]
		public class RewriterConfiguration
		{
			internal static RewriterRuleCollection _rules;

			/// <summary>A RewriterRuleCollection instance that provides access to a set of RewriterRules.</summary>
			public RewriterRuleCollection Rules
			{
				get
				{
					return _rules;
				}
				set
				{
					_rules = value;
				}
			}

		}

		static class RewriterConfigurationManager
		{
			public static RewriterConfiguration Config;

			static RewriterConfigurationManager()
			{
				PopulateRules();
			}

			private static void PopulateRules()
			{
				const string METHOD_NAME = "PopulateRules";

				// Get and open the file.
				try
				{
					string file = HttpContext.Current.Request.PhysicalApplicationPath + "web.rewrite";
					FileSecurity fileSecurity = new FileSecurity();
					FileStream fs = new FileStream(file, FileMode.Open, FileSystemRights.Read, FileShare.Read, 4096, FileOptions.None, fileSecurity);
					TextReader tr = new StreamReader(fs);
					// Deserialize it into our collection.
					XmlSerializer xml = new XmlSerializer(typeof(RewriterConfiguration));
					Config = (RewriterConfiguration)xml.Deserialize(tr); // Will set the static variable before the calling function returns it.
																		 //Close the file. 
					tr.Close();
					fs.Close();
				}
				catch (Exception exception)
				{
					//Log the error, then fail quietly
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}
			}
		}
		#endregion
	}
}
