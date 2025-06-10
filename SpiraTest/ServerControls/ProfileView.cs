using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security.AntiXss;

using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// Simple server control that displays the currently logged in user's full name
	/// </summary>
	[ToolboxData("<{0}:ProfileView runat=server></{0}:ProfileView>")]
	public class ProfileView : WebControl
	{
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Span;
			}
		}

		/// <summary>
		/// Renders out the user's profile
		/// </summary>
		/// <param name="output"></param>
		protected override void RenderContents(HtmlTextWriter output)
		{
			if (Page.User.Identity.IsAuthenticated)
			{
				ProfileEx profile = new ProfileEx();
				if (profile != null)
				{
					output.Write(AntiXssEncoder.HtmlEncode(profile.FullName, false));
				}
			}
		}
	}
}
