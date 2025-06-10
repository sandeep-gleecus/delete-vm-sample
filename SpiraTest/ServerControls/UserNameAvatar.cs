using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	[ToolboxData("<{0}:UserNameAvatar runat=\"server\" />")]
	public class UserNameAvatar : WebControl
	{
		private const string CLASS = "Inflectra.SpiraTest.Web.ServerControls.UserNameAvatar::";

		public UserNameAvatar()
		{
			//Set defaults.
			this.ShowAvatar = true;
			this.AvatarSize = 100;
			this.NavUrl = null;
			this.UserId = null;
			this.BoldUserName = true;
			this.ShowUserName = true;
			this.ShowFullName = false;
			this.Display = DisplayTypeEnum.Block;
		}

		#region Edit Properties
		/// <summary>Whether to display the avatar at all.</summary>
		[Description("Whether to show the avatar, or completely hide it.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool ShowAvatar
		{
			get;
			set;
		}

		/// <summary>Whether to display the avatar at all.</summary>
		[Description("Whether to show the user's login ID or not.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool ShowUserName
		{
			get;
			set;
		}

		/// <summary>The size of the avatar to display.</summary>
		[Description("The maximum size (width or height) of the avatar image.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(100)]
		public int? AvatarSize
		{
			get;
			set;
		}

		/// <summary>The size of the avatar to display.</summary>
		[Description("Whether to bold the username or not.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool? BoldUserName
		{
			get;
			set;
		}

		/// <summary>Link to the user's account page.</summary>
		[Description("The URL that the avatar & name will hot-link to. If empty/blank, will not be clickable.")]
		[Browsable(true)]
		[Category("Behavior")]
		[DefaultValue("")]
		public string NavUrl
		{
			get;
			set;
		}

		/// <summary>Display the user's full name or their login id.</summary>
		[Description("Whether to display the user's full name. True to see their full name, False (default) to see their login ID.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool ShowFullName
		{
			get;
			set;
		}

		/// <summary>The user id to display the avatar for. If not set, then the control will display the currently logged in user's avatar and name.</summary>
		[Description("The user we're displaying. If not set, will use the current logged-in user.")]
		[Browsable(true)]
		[Category("Behavior")]
		[DefaultValue(null)]
		public int? UserId
		{
			get;
			set;
		}

		/// <summary>How to display the Avatar and Username.</summary>
		[Description("How to display the Avatar Image and the User's Name.")]
		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(DisplayTypeEnum.Block)]
		public DisplayTypeEnum Display
		{
			get;
			set;
		}
		#endregion // Edit properties

		protected override void RenderContents(HtmlTextWriter output)
		{
			const string METHOD = CLASS + "RenderContents()";

			try
			{
				if (output == null)
					return;

				int? userId = null;
				User user = null;

				//Get our user ID. If we don't have one, then we need to display nothing. (We should never have one..)
				if (this.UserId.HasValue)
					userId = this.UserId;
				else
				{
                    if (this.Page != null && this.Page is PageLayout)
						userId = ((PageLayout)this.Page).UserId;
					else //Don't display the Avatar
						return;
				}

				//Get the user's account.
				if (userId.HasValue && userId.Value > 0)
					user = new UserManager().GetUserById(userId.Value, false);

				/* Now create our controls... */
				//Create our container div, first..
				HtmlGenericControl divCont = null;
				if (this.Display == DisplayTypeEnum.Block)
					divCont = new HtmlGenericControl("div");
				else
					divCont = new HtmlGenericControl("span");

				//Our controls.
				HtmlAnchor aLink = null;
				Image imgAvt = null;
				HtmlGenericControl spanName = null;

				//If we need to create the anchor, first..
				if (!string.IsNullOrWhiteSpace(this.NavUrl))
				{
					aLink = new HtmlAnchor();
					aLink.HRef = this.NavUrl;
				}

				//If we need to make our image..
				if (this.ShowAvatar)
				{
					imgAvt = new Image();
					string imgstyle = "";

					//Alternate text..
					if (user != null && user.Profile != null)
						imgAvt.AlternateText = user.Profile.FullName;
					else
						imgAvt.AlternateText = Resources.Main.UserProfile_Avatar;

					//Size & Alignment. (In 'style' tag..)
					if (this.AvatarSize.HasValue)
						imgstyle = "max-width:" + this.AvatarSize.Value + "px; max-height:" + this.AvatarSize.Value + "px;";
					else
						imgstyle = "max-width:100px; max-height:100px;";
					imgstyle += " vertical-align:middle;";
					imgAvt.Attributes.Add("style", imgstyle);

					//URL for the image..
					imgAvt.ImageUrl = UrlRewriterModule.ResolveUserAvatarUrl(userId, Page.Theme);

					//Add it to the container div.
					divCont.Controls.Add(imgAvt);
				}

				//If we need to create our username..
				if (this.ShowUserName)
				{
					spanName = new HtmlGenericControl("span");
					string styleText = "";
					if (this.BoldUserName.HasValue && this.BoldUserName.Value) styleText = "font-weight:bold;";
					if (ShowAvatar) styleText += " padding-left:5px;";
					if (!string.IsNullOrWhiteSpace(styleText)) spanName.Attributes.Add("style", styleText);

					if (this.ShowFullName && user.Profile != null)
						spanName.InnerText = user.Profile.FullName.Trim();
					else
						spanName.InnerText = user.UserName.Trim();
				}

				//Now add our controls to each other..
				if (aLink != null)
				{
					if (imgAvt != null) aLink.Controls.Add(imgAvt);
					if (spanName != null) aLink.Controls.Add(spanName);

					divCont.Controls.Add(aLink);
				}
				else
				{
					if (imgAvt != null) divCont.Controls.Add(imgAvt);
					if (spanName != null) divCont.Controls.Add(spanName);
				}

				//Now send it to the writer.
				divCont.RenderControl(output);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD, ex, "Trying to render UserAvatar control.");
			}
		}
	}

	/// <summary>Class to hold Display Type enum.</summary>
	public sealed class DisplayTypes
	{
		[NotifyParentProperty(true)]
		public DisplayTypeEnum DisplayTypeValue { get; set; }
	}

	/// <summary>Different Display Types for the control.</summary>
	public enum DisplayTypeEnum
	{
		/// <summary>Show as a blocked element. (div tag)</summary>
		Block = 0,
		/// <summary>Display as an inline element. (span tag)</summary>
		Inline = 1,
		/// <summary>To completely hide the control. Not used.</summary>
		Hidden = 2
	}

	/// <summary>Collection of DisplayTypes.</summary>
	public class ShieldPermissionsCollection : List<DisplayTypes>
	{ }
}
