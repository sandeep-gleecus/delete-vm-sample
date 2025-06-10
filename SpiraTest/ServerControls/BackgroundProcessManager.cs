using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// Displays an AJAX progress bar dialog that handles long-running background processes
	/// </summary>
	/// <remarks>
	/// Requires that the BackgroundProcessService.svc is available on the page
	/// </remarks>
	[ToolboxData("<{0}:BackgroundProcessManager runat=server></{0}:BackgroundProcessManager>")]
	public class BackgroundProcessManager : WebControl, IScriptControl
	{
		protected Dictionary<string, string> handlers;

		#region Properties

		[Category("Behavior")]
		[DefaultValue("")]
		[Description("The ID of the server control that we want to use to display error messages (div, span, etc.)")]
		public string ErrorMessageControlId
		{
			get
			{
				object obj = ViewState["errorMessageControlId"];

				return (obj == null) ? string.Empty : (string)obj;
			}
			set
			{
				ViewState["errorMessageControlId"] = value;
			}
		}

		/// <summary>Should we make the dialog box modal</summary>
		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Should we make the dialog box modal")]
		public bool Modal
		{
			get
			{
				object obj = ViewState["Modal"];

				return (obj == null) ? true : (bool)obj;
			}
			set
			{
				ViewState["Modal"] = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue("")]
		[Description("The ClientID of the HTML DOM element or ASP.NET server control that we want to use to display error messages (div, span, etc.)")]
		public string ErrorMessageControlClientId
		{
			get
			{
				object obj = ViewState["ErrorMessageControlClientId"];

				return (obj == null) ? string.Empty : (string)obj;
			}
			set
			{
				ViewState["ErrorMessageControlClientId"] = value;
			}
		}

		[Category("Data")]
		[DefaultValue("")]
		[Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")]
		public string WebServiceClass
		{
			get
			{
				object obj = ViewState["webServiceClass"];

				return (obj == null) ? string.Empty : (string)obj;
			}
			set
			{
				ViewState["webServiceClass"] = value;
			}
		}

		/// <summary>Should the page automatically redirect, or allow the user to press the 'Close' button.</summary>
		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Should the dialog automatically close and redirect to the success page?")]
		public bool AutoRedirect
		{
			get
			{
				object obj = ViewState["AutoRedirect"];

				return (obj == null) ? true : (bool)obj;
			}
			set
			{
				ViewState["AutoRedirect"] = value;
			}
		}

		/// <summary>How often - in seconds - the background process should check for an updated progress.</summary>
		[Category("Behavior")]
		[DefaultValue(1)]
		[Description("How often (in seconds) the dialog should check for progress updates.")]
		public decimal RefreshRateSecs
		{
			get
			{
				object obj = ViewState["RefreshRate"];
				return ((obj == null) ? 1 : (decimal)obj);
			}
			set
			{
				ViewState["RefreshRate"] = value;
			}
		}

		/// <summary>Whether or not the background process should call the 'success' method in event of an error.</summary>
		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Whether or not the background process should call the 'success' method in event of an error.")]
		public bool CallSuccessOnError
		{
			get
			{
				object obj = ViewState["SuccOnError"];

				return (obj == null) ? false : (bool)obj;
			}
			set
			{
				ViewState["SuccOnError"] = value;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Register the client script
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(System.EventArgs e)
		{
			base.OnPreRender(e);

			ScriptManager manager = ScriptManager.GetCurrent(this.Page);
			manager.RegisterScriptControl(this);

			//Now remove the height style attribute and replace with min-height so that it can stretch
			if (!this.Height.IsEmpty)
			{
				Unit height = this.Height;
				this.Height = Unit.Empty;
				this.Style.Add("min-height", height.ToString());
			}
		}

		/// <summary>
		/// Make the dialog box initially hidden
		/// </summary>
		/// <param name="writer"></param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
			//Need to specify this explicitly (i.e. not just relying on CSS)
			//because some of the child ajax controls will need to know this
			//when calculating offsets, etc.
			writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
		}

		/// <summary>
		/// Add the component descriptors to the render
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);

			ScriptManager manager = ScriptManager.GetCurrent(this.Page);
			manager.RegisterScriptDescriptors(this);
		}

		/// <summary>
		/// The base DOM element is a DIV
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

		#endregion

		#region IScriptControl Members

		/// <summary>Generate the properties, methods and events sent to the client component</summary>
		/// <returns></returns>
		public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
		{
			ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager", this.ClientID);

			//If theming is enabled, need to pass the theme folder so that images resolve correctly
			if (Page.EnableTheming && Page.Theme != "")
			{
				if (HttpContext.Current.Request.ApplicationPath == "/")
				{
					descriptor.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
				}
				else
				{
					descriptor.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
				}
			}
			descriptor.AddProperty("modal", Modal);
			descriptor.AddProperty("autoRedirect", this.AutoRedirect);
			descriptor.AddProperty("refreshRate", this.RefreshRateSecs);
			descriptor.AddProperty("callSuccessOnError", this.CallSuccessOnError);


			if (!string.IsNullOrEmpty(WebServiceClass))
			{
				//We pass it as a script property because this control expects it as a javascript object not a string
				//unlike some of the other (older) controls
				descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
			}

			if (!string.IsNullOrEmpty(ErrorMessageControlId))
			{
				//First we need to get the server control
				Control errorMessageControl = this.Parent.FindControl(this.ErrorMessageControlId);
				if (errorMessageControl != null)
				{
					string clientId = errorMessageControl.ClientID;
					descriptor.AddProperty("errorMessageControlId", clientId);
				}
			}
			else if (!String.IsNullOrEmpty(ErrorMessageControlClientId))
			{
				//Sometimes we've given the actual error control client id
				descriptor.AddProperty("errorMessageControlId", ErrorMessageControlClientId);
			}

			//Add any custom client-side handlers
			if (this.handlers != null)
			{
				foreach (KeyValuePair<string, string> handler in this.handlers)
				{
					descriptor.AddEvent(handler.Key, handler.Value);
				}
			}

			yield return descriptor;
		}

		/// <summary>
		/// Generate the script reference
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScriptReference> GetScriptReferences()
		{
			yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.BackgroundProcessManager.js"));
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Allows the passing in of a collection of client-side event handlers
		/// </summary>
		/// <param name="handlers">The collection of handlers</param>
		public void SetClientEventHandlers(Dictionary<string, string> handlers)
		{
			this.handlers = handlers;
		}

		#endregion
	}
}
