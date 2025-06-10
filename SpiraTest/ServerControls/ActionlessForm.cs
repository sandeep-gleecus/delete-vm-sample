using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web;
using System;
using System.Web.UI.WebControls;
using System.Globalization;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	[ToolboxData("<{0}:ActionlessForm runat=server></{0}:ActionlessForm>")]
	public class ActionlessForm : HtmlForm
	{
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Make sure it has an iD
            base.EnsureID();
        }

        /// <summary>
        /// Is this control located inside an UpdatePanelEx
        /// </summary>
        public bool InUpdatePanel
        {
            get
            {
                if (ViewState["InUpdatePanel"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["InUpdatePanel"];
                }
            }
            set
            {
                ViewState["InUpdatePanel"] = value;
            }
        }

		protected override void RenderAttributes(HtmlTextWriter writer)
		{
			writer.WriteAttribute("name", this.Name);
			base.Attributes.Remove("name");

			writer.WriteAttribute("method", this.Method);
			base.Attributes.Remove("method");

            //Remove the action to avoid postback issues with rewritten urls
            base.Attributes.Remove("action");

            //For the update panels on the dashboards to work, need to have an Action field with the various
            //querystring parameters removed (since they'll be duplicated with the values from the rewriter
            //For other URLs (with no update panels) need to leave out altogether so that the various
            //appending parameters are handled correctly
            if (InUpdatePanel)
            {
                //If the URL was not rewritten, simply use the full 'real' URL
                //If the URL was rewritten, need to remove the querystring and keep it in its 'pretty' form
                string prettyUrl = HttpContext.Current.Request.RawUrl;
                string realUrl = HttpContext.Current.Request.Url.AbsolutePath + HttpContext.Current.Request.Url.Query;
                string action;
                if (prettyUrl.Trim().ToLowerInvariant() == realUrl.Trim().ToLowerInvariant())
                {
                    //Not rewritten
                    action = realUrl;
                }
                else
                {
                    //Rewritten
                    action = prettyUrl;
                }
                writer.WriteAttribute("action", Microsoft.Security.Application.Encoder.HtmlAttributeEncode(action));
                Logger.LogTraceEvent("ActionlessForm::RenderAttributes", "action:" + action);
            }

            //Need to handle the default button property
            Control button = this.FindControl(this.DefaultButton);
            if ((button == null) && (this.Page != null))
            {
                char[] anyOf = new char[] { '$', ':' };
                if (this.DefaultButton.IndexOfAny(anyOf) != -1)
                {
                    button = this.Page.FindControl(this.DefaultButton);
                }
            }
            if (button is IButtonControl)
            {
                RegisterDefaultButtonScript(button, writer, false);
            }

            //Need to write out the client code that makes sure form submit ASP.NET standard events are fired
            //If the page has Submit statements, need to make sure the javascript code that checks validators is fired
            string clientOnSubmitEvent = "javascript: if (typeof WebForm_OnSubmit == 'function') { return WebForm_OnSubmit(); } else { return true; }";
            writer.WriteAttribute("onsubmit", clientOnSubmitEvent);

			this.Attributes.Render(writer);
            if (base.ID != null)
            {
                writer.WriteAttribute("id", base.ClientID);
            }
		}

        /// <summary>
        /// Renders the client-side script for handling the default button on pages
        /// </summary>
        /// <param name="button"></param>
        /// <param name="writer"></param>
        /// <param name="useAddAttribute"></param>
        protected internal void RegisterDefaultButtonScript(Control button, HtmlTextWriter writer, bool useAddAttribute)
        {
            string str = "javascript:return WebForm_FireDefaultButton(event, '" + button.ClientID + "')";
            if (useAddAttribute)
            {
                writer.AddAttribute("onkeypress", str);
            }
            else
            {
                writer.WriteAttribute("onkeypress", str);
            }
        }
	}
}
