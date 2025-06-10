using Inflectra.SpiraTest.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("SkinID")]
    [ToolboxData("<{0}:ThemeStylePlaceHolder runat=\"server\" SkinID=\"ThemeThemeStyles\"></{0}:ThemeStylePlaceHolder>")]
    [ParseChildren(true, "ThemeStyles")]
    [Themeable(true)]
    [PersistChildren(false)]
    public class ThemeStylePlaceHolder : Control
    {
        private List<ThemeStyle> _styles;

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue("ThemeThemeStyles")]
        public override string SkinID { get; set; }

        [Browsable(false)]
        public List<ThemeStyle> ThemeStyles
        {
            get
            {
                if (_styles == null)
                    _styles = new List<ThemeStyle>();
                return _styles;
            }
        }

        /// <summary>
        /// The filename of the favicon links to add to the header (passing an empty string results in no links being rendered)
        /// </summary>
        /// <remarks>
        /// We prefix it with the product name, if we have one licensed
        /// So FavIcon.ico becomes --> SpiraPlan-FavIcon.ico
        /// </remarks>
        [
        Bindable(true),
        Category("Misc"),
        Description("The filename of the favicon links to add to the header (passing an empty string results in no links being rendered)"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string FavIconFilename
        {
            get
            {
                if (ViewState["FavIconFilename"] == null)
                {
                    return "";
                }
                else
                {
                    return ((string)ViewState["FavIconFilename"]);
                }
            }
            set
            {
                ViewState["FavIconFilename"] = value;
            }
        }

        protected override void CreateChildControls()
        {
            if (_styles == null)
                return;

            // add child controls
            ThemeStyles.ForEach(Controls.Add);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // get notified when page has finished its load stage
            Page.LoadComplete += Page_LoadComplete;
        }

        void Page_LoadComplete(object sender, EventArgs e)
        {
            // only remove if the page is actually using themes
            if (!string.IsNullOrEmpty(Page.StyleSheetTheme) || !string.IsNullOrEmpty(Page.Theme))
            {
                string themeName = !string.IsNullOrEmpty(Page.StyleSheetTheme)
                                                  ? Page.StyleSheetTheme
                                                  : Page.Theme;
                // Make sure only to remove style sheets from the added by
                // the runtime form the current theme.
                var themePath = string.Format("~/App_Themes/{0}", themeName);

                // find all existing stylesheets in header
                var removeCandidate = Page.Header.Controls.OfType<HtmlLink>()
                    .Where(link => link.Href.StartsWith(themePath)).ToList();

                // remove the automatically added style sheets
                removeCandidate.ForEach(Page.Header.Controls.Remove);

                //Add the FavIcon filenames if appropriate
                if (!String.IsNullOrWhiteSpace(FavIconFilename))
                {
                    string favIconFilename;
                    if (String.IsNullOrEmpty(ConfigurationSettings.Default.License_ProductType))
                    {
                        favIconFilename = FavIconFilename;
                    }
                    else
                    {
                        favIconFilename = ConfigurationSettings.Default.License_ProductType + "-" + FavIconFilename;
                    }

                    HtmlLink link1 = new HtmlLink();
                    link1.Attributes["rel"] = "icon";
                    link1.Attributes["type"] = "image/x-icon";
                    link1.Href = "~/App_Themes/" + themeName + "/" + favIconFilename;
                    HtmlLink link2 = new HtmlLink();
                    link2.Attributes["rel"] = "shortcut icon";
                    link2.Attributes["type"] = "image/x-icon";
                    link2.Href = "~/App_Themes/" + themeName + "/" + favIconFilename;
                    Page.Header.Controls.Add(link1);
                    Page.Header.Controls.Add(link2);
                }
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            // only add ThemeStyle controls
            if (obj is ThemeStyle)
                base.AddParsedSubObject(obj);
        }

    }
}
