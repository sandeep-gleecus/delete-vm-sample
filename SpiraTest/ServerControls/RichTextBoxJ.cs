using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Server control that wraps the standard ckEditor
    /// </summary>
    [ToolboxData("<{0}:RichTextBoxJ runat=server></{0}:RichTextBoxJ>")]
    public class RichTextBoxJ : TextBox, IScriptControl, IAuthorizedControl
	{
		#region Private Strings
		private string Toolbar_Normal = @"toolbar:[{name:'styles',items:['Format','Font','FontSize']},{name:'basicstyles',items:['Bold','Italic','Underline','-','RemoveFormat']},{name:'colors',items:['TextColor','BGColor']},{name:'paragraph',items:['NumberedList','BulletedList','-','Outdent','Indent','-','Blockquote','-','JustifyLeft','JustifyCenter','JustifyRight']},{name:'insert',items:['Link','Unlink','-','Image','CodeSnippet','Table','HorizontalRule','-','PasteFromWord','-','CreateToken']},{name:'tools',items:['Maximize','-','Source','-','Templates','ShowBlocks']}]";
		private string Toolbar_AdminColor = @"toolbarGroups:[{name:'styles',groups:['styles']},{name:'basicstyles',groups:['basicstyles','cleanup']},{name:'paragraph',groups:['list','indent','blocks','align','bidi','paragraph']},{name:'insert',groups:['insert']},{name:'tools',groups:['tools']}],removeButtons:'Paste,Undo,Redo,Subscript,Superscript,Cut,Copy,Anchor,Strike,Format,lineheight,Outdent,Indent,Blockquote,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock,Link,Unlink,CreateToken,CodeSnippet,HorizontalRule,ShowBlocks,Source,Templates,lineheight'";
        private string Toolbar_Mini = @"toolbarGroups: [{ name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },{ name: 'paragraph', groups: [ 'list', 'indent', 'blocks', 'align', 'bidi','paragraph' ] },{ name: 'clipboard', groups: [ 'clipboard', 'undo' ] },{ name: 'editing', groups: [ 'find', 'selection', 'spellchecker', 'editing' ] },{ name: 'links', groups: [ 'links' ] },{ name: 'insert', groups: [ 'insert' ] },{ name: 'forms', groups: [ 'forms' ] },{ name: 'tools', groups: [ 'tools' ] },{ name: 'document', groups: [ 'mode', 'document', 'doctools' ] },{ name: 'others', groups: [ 'others' ] },{ name: 'styles', groups: [ 'styles' ] },{ name: 'colors', groups: [ 'colors' ] },{ name: 'about', groups:[ 'about' ]}],removeButtons:'Subscript,Superscript,Cut,Copy,Redo,Undo,Paste,PasteText,PasteFromWord,Scayt,Anchor,Image,HorizontalRule,SpecialChar,Source,Strike,Outdent,Indent,Styles,About,Format,TextColor,BGColor,FontSize,Font,CodeSnippet,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock'";
		#endregion

        protected AuthorizedControlBase authorizedControlBase;

		#region Constructor
		/// <summary>Instanitates a new rich text editor.</summary>
		public RichTextBoxJ()
		{
			ToolbarType = ToolbarModeEnum.NormalFull;
			ShowBottomBar = true;
            RenderOnlyScripts = false;

            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}
		#endregion

		#region Properties

        /// <summary>
        /// This is the type of artifact to attach any embedded screenshots against
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("This is the type of artifact to attach any embedded screenshots against"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum Screenshot_ArtifactType
        {
            get
            {
                if (ViewState["Screenshot_ArtifactType"] == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                else
                {
                    return ((DataModel.Artifact.ArtifactTypeEnum)ViewState["Screenshot_ArtifactType"]);
                }
            }
            set
            {
                ViewState["Screenshot_ArtifactType"] = value;
            }
        }


        /// <summary>
        /// This is the id of the artifact to attach any embedded screenshots against
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("This is the type of artifact to attach any embedded screenshots against"),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public int? Screenshot_ArtifactId
        {
            get
            {
                if (ViewState["Screenshot_ArtifactId"] == null)
                {
                    return null;
                }
                else
                {
                    return ((int?)ViewState["Screenshot_ArtifactId"]);
                }
            }
            set
            {
                ViewState["Screenshot_ArtifactId"] = value;
            }
        }

        /// <summary>
        /// This is the id of the project to store embedded screenshots in
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("This is the id of the project to store embedded screenshots in"),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public int? Screenshot_ProjectId
        {
            get
            {
                if (ViewState["Screenshot_ProjectId"] == null)
                {
                    return null;
                }
                else
                {
                    return ((int?)ViewState["Screenshot_ProjectId"]);
                }
            }
            set
            {
                ViewState["Screenshot_ProjectId"] = value;
            }
        }

        /// <summary>
        /// Should we allow all markup - needed for certain admin screens
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("Should we allow all markup - needed for certain admin screens"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public bool AllowAllMarkup
        {
            get
            {
                if (ViewState["AllowAllMarkup"] == null)
                {
                    return false;
                }
                else
                {
                    return ((bool)ViewState["AllowAllMarkup"]);
                }
            }
            set
            {
                ViewState["AllowAllMarkup"] = value;
            }
        }

        /// <summary>
        /// This is the type of artifact that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of artifact that the user's role needs to have permissions for"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum Authorized_ArtifactType
        {
            get
            {
                return authorizedControlBase.Authorized_ArtifactType;
            }
            set
            {
                authorizedControlBase.Authorized_ArtifactType = value;
            }
        }

        /// <summary>
        /// This is the type of action that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of action that the user's role needs to have permissions for"),
        DefaultValue(Project.PermissionEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public Project.PermissionEnum Authorized_Permission
        {
            get
            {
                return authorizedControlBase.Authorized_Permission;
            }
            set
            {
                authorizedControlBase.Authorized_Permission = value;
            }
        }

		/// <summary>The URL to upload any attachments to. The overrides the Screenshot project/artifact settings</summary>
		public string UploadImageUrl { get; set; }

		/// <summary>Whether or not tomake it inline (in a div) or form (in a text-area).</summary>
		public bool InlineEditor { get; set; }

		/// <summary>Overrides any other set value of TextMode.</summary>
		public override TextBoxMode TextMode
		{
			get
			{
				return TextBoxMode.MultiLine;
			}
			set { }
		}

		/// <summary>The type of toolbar to display & show.</summary>
		public ToolbarModeEnum ToolbarType { get; set; }

		/// <summary>Whether to show the current element path in the bottom panel.</summary>
		public bool ShowBottomBar { get; set; }

        public bool RenderOnlyScripts { get; set; }

		public bool RemoveSourceButton { get; set; }
		#endregion

        #region IScriptControl Members

        /// <summary>
        /// Gets any script descriptors
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            return null;
        }

        /// <summary>
        /// Returns any external script references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            //The main ckeditor script and the script that enables control of ckeditor using jquery (required by some plugins)
            yield return new ScriptReference("~/ckEditor/ckeditor-4.5.11.js?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER);
            yield return new ScriptReference("~/ckEditor/adapters/jquery.js?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER);
        }

        #endregion

        /// <summary>
        /// Renders unless special flag set
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!RenderOnlyScripts)
            {
                base.Render(writer);
            }
        }

		/// <summary>Called before we start writing out the object.</summary>
		/// <param name="e">EventArgs</param>
		protected override void OnPreRender(EventArgs e)
		{
			//Write out the needed javascript references
            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);

            //Add the relative script location
            string jsScriptLocation = "var CKEDITOR_BASEPATH = '" + UrlRewriterModule.ResolveUrl("~/ckEditor/") + "';\n";
            ScriptManager.RegisterClientScriptBlock(this, typeof(ServerControlCommon), "CKEDITOR_BASEPATH", jsScriptLocation, true);

            if (!RenderOnlyScripts)
            {
                //Now, add the javascript that creates the item.. (and end of page)
                string jsCreate = "";
                jsCreate += "$(document).ready(function(){ CKEDITOR.replace('" + ClientID + "', {";
                jsCreate += "customConfig:'',";
                jsCreate += generateOptions();
                jsCreate += "})";                
                jsCreate += "});";

                //Removed the code to make ckEditor look disabled because it was more confusing
                //so no way to set permissions on a RTE by user. The Authorized ArtifactType is ignored

                ScriptManager.RegisterStartupScript(this, GetType(), this.ClientID, jsCreate, true);
            }

			base.OnPreRender(e);
		}

		/// <summary>Generates an array of set options.</summary>
		/// <returns>A string to be used in a JS command to initialize the object.</returns>
		private string generateOptions()
		{
            List<string> pluginsToRemove = new List<string>();
			string retValue = "";

            //enable default browser spellchecking
            retValue += "disableNativeSpellChecker: false,";

			//The file upload URL..
            //First see if we have an override URL, then use the project/artifact default if specified
            if (string.IsNullOrWhiteSpace(UploadImageUrl))
            {
                if (Screenshot_ProjectId.HasValue)
                {
                    if (Screenshot_ArtifactType != Artifact.ArtifactTypeEnum.None && Screenshot_ArtifactId.HasValue)
                    {
                        //Get the rewriter URL for project/artifact
                        string url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Common.UrlRoots.NavigationLinkEnum.ScreenshotUpload, Screenshot_ProjectId.Value, Screenshot_ArtifactId.Value, (int)Screenshot_ArtifactType));
                        retValue += "uploadUrl: '" + GlobalFunctions.JSEncode(url) + "',";
                    }
                    else
                    {
                        //Get the rewriter URL for project only
                        string url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Common.UrlRoots.NavigationLinkEnum.ScreenshotUpload, Screenshot_ProjectId.Value));
                        retValue += "uploadUrl: '" + GlobalFunctions.JSEncode(url) + "',";
                    }
                }
                else
                {
                    //Remove the plugin
                    pluginsToRemove.Add("uploadimage");
                }
            }
            else
            {
                retValue += "uploadUrl:'" + GlobalFunctions.JSEncode(ResolveUrl(UploadImageUrl)) + "',";
            }

			switch (ToolbarType)
			{
				case ToolbarModeEnum.NormalFull:
					retValue += ((RemoveSourceButton) ? Toolbar_Normal.Replace("removeButtons:'", "removeButtons:'Source,") : Toolbar_Normal) + ",";
					break;
				case ToolbarModeEnum.Mini:
					retValue += ((RemoveSourceButton) ? Toolbar_Normal.Replace("removeButtons:'", "removeButtons:'Source,") : Toolbar_Mini) + ",";
					break;
				case ToolbarModeEnum.AdminColorPick:
					retValue += Toolbar_AdminColor + ",";
					break;
			}
            if (!ShowBottomBar)
            {
                pluginsToRemove.Add("elementspath");
                retValue += "resize_enabled:false,";
            }

            //See if we have a height specified
            if (!Height.IsEmpty)
            {
                retValue += "height:'" + GlobalFunctions.JSEncode(Height.ToString()) + "',";
            }
            else
            {
                retValue += "height:'" + "180px" + "',";
            }

            //See if we need to allow all markup
            if (AllowAllMarkup)
            {
                //We also allow MS-Word formatting in this case
                retValue += "allowedContent: true,pasteFromWordRemoveStyles: false,pasteFromWordRemoveFontStyles: false,";
            }

            //Remove any plugins
            if (pluginsToRemove.Count > 0)
            {
                retValue += "removePlugins:'";
                bool first = true;
                foreach (string plugin in pluginsToRemove)
                {
                    if (first)
                    {
                        retValue += plugin;
                    }
                    else
                    {
                        retValue += "," + plugin;
                    }
                    first = false;
                }
                retValue += "'";
            }

			return retValue.Trim(',');
		}

		/// <summary>The options avaialble for the toolbar display.</summary>
		public enum ToolbarModeEnum
		{
			NormalFull = 1,
			AdminColorPick = 2,
			Mini = 3
		}
	}
}