using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using System.Diagnostics;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default text-box to include an additional
	/// property that can be used to store a meta-data field that is used in dynamic filters
	/// </summary>
	[ToolboxData("<{0}:DiagramEditor runat=server></{0}:DiagramEditor>")]
	public class DiagramEditor : WebControl, IScriptControl, IAuthorizedControl
	{
		protected AuthorizedControlBase authorizedControlBase;

		#region Constructor
		/// <summary>Instanitates a new rich text editor.</summary>
		public DiagramEditor()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}
		#endregion

		#region Properties
		/// <summary>
		/// The raw data to render as a diagram
		/// </summary>
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		public string Data
		{
			get
			{
				String s = (String)ViewState["Data"];
				return (s == null ? String.Empty : s);
			}

			set
			{
				ViewState["Data"] = value;
			}
		}

		/// <summary>
		/// This is the type of diagram being edited
		/// </summary>
		[
		Bindable(true),
		Category("Behavior"),
		Description("This is the type of diagram you are working with"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute),
		]
		public string DiagramType
		{
			get
			{
				if (ViewState["DiagramType"] == null)
				{
					return "";
				}
				else
				{
					return ((string)ViewState["DiagramType"]);
				}
			}
			set
			{
				ViewState["DiagramType"] = value;
			}
		}

		public Artifact.ArtifactTypeEnum Authorized_ArtifactType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Project.PermissionEnum Authorized_Permission { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		#endregion

		#region IScriptControl Members

		/// <summary>
		/// Gets any script descriptors
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
		{
			ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DiagramEditor", ClientID);
			desc.AddProperty("data", this.Data);
			desc.AddProperty("diagramType", this.DiagramType);
			yield return desc;
		}

		public IEnumerable<ScriptReference> GetScriptReferences()
		{
			yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DiagramEditor.js"));
		}
		#endregion

		/// <summary>
		/// Add the persistent tooltip javascript
		/// </summary>
		/// <param name="e">The Event Arguments</param>
		protected override void OnPreRender(EventArgs e)
		{
			//First execute the base class
			base.OnPreRender(e);

			//We need to register a client component to go with the server control
			ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

			if (scriptManager == null)
			{
				throw new InvalidOperationException("ScriptManager required on the page.");
			}

			scriptManager.RegisterScriptControl(this);
		}

		/// <summary>
		/// We need to render this control as a DIV
		/// </summary>
		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

		/// <summary>
		/// Renders out the script descriptors
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
			if (!DesignMode)
			{
				ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
			}
		}

	}
}
