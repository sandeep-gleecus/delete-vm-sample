using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Drawing;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in WebPartZone to handle the case where the web parts are enclosed in an AJAX update panel
    /// and to support non-IE browsers
    /// </summary>
    public class WebPartZoneEx : WebPartZone, IScriptControl
    {
        [
        Category("Appearance"),
        Description("Sets the color of the drag zone"),
        PersistenceMode(PersistenceMode.Attribute),
        TypeConverter(typeof(WebColorConverter)), DefaultValue(typeof(Color), "Blue")
        ]
        public Color DragZoneColor
        {
            get
            {
                object obj2 = this.ViewState["DragZoneColor"];
                if (obj2 != null)
                {
                    Color color = (Color)obj2;
                    if (!color.IsEmpty)
                    {
                        return color;
                    }
                }
                return Color.Blue;
            }
            set
            {
                this.ViewState["DragZoneColor"] = value;
            }
        }

        //Some versions of .NET throw a method not found exception from base class - so need to have own version of it
        [
        Description("Zone_PartChromePadding"),
        DefaultValue(typeof(Unit), "5px"),
        Category("WebPart")
        ]
        public new Unit PartChromePadding
        {
            get
            {
                object obj2 = this.ViewState["PartChromePadding"];
                if (obj2 != null)
                {
                    return (Unit)obj2;
                }
                return Unit.Pixel(5);
            }
            set
            {
                if (value.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["PartChromePadding"] = value;
            }
        }

        /// <summary>
        /// Renders the web part manager control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            ScriptManager current = ScriptManager.GetCurrent(this.Page);
            if (current != null)
            {
                current.RegisterScriptDescriptors(this);
            }
        }

        protected internal new bool RenderClientScript
        {
            get
            {
                return base.RenderClientScript;
            }
        } 
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            scriptManager.RegisterScriptControl(this);
        }

        /// <summary>
        /// Returns the customized SpiraTest web part chrome class
        /// </summary>
        /// <returns></returns>
        protected override WebPartChrome CreateWebPartChrome()
        {
            return new WebPartChromeEx(this, WebPartManager.GetCurrentWebPartManager(this.Page));
        }

        protected override void RenderDropCue(HtmlTextWriter writer)
        {
            base.RenderDropCue(new FixPaddingHtmlTextWriter(writer));
        }

        /// <summary>
        /// Extends the postback handler to handle the new 'DrillDown' verb and prevent maintaining scroll on editing
        /// </summary>
        /// <param name="eventArgument"></param>
        protected override void RaisePostBackEvent(string eventArgument)
        {
            base.RaisePostBackEvent(eventArgument);

            //First handle the built-in events of the base class
            if (!string.IsNullOrEmpty(eventArgument))
            {
                string[] eventArguments = eventArgument.Split(new char[] { ':' });
                //Make sure we are using the extended web manager class
                if (this.WebPartManager != null && this.WebPartManager is WebPartManagerEx)
                {
                    WebPartManagerEx webPartManagerEx = (WebPartManagerEx)this.WebPartManager;
                    WebPartCollection webParts = webPartManagerEx.WebParts;
                    string verbName = eventArguments[0];
                    string webPartName = eventArguments[1];
                    WebPart webPart = webParts[webPartName];
                    if ((webPart != null) && !webPart.IsClosed)
                    {
                        if (String.Equals(verbName, "drilldown", StringComparison.OrdinalIgnoreCase))
                        {
                            //Raise the event that the drilldown was fired
                            webPartManagerEx.DrillDownWebPart(webPart);
                        }

                        if (String.Equals(verbName, "edit", StringComparison.OrdinalIgnoreCase))
                        {
                            //Sets the maintain scroll position to false so that the edit section is visible
                            this.Page.MaintainScrollPositionOnPostBack = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the ajax control and sets any properties/events on it
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Get the highlight color
            string highlightColor = "black";
            string dragZoneColor = "gray";
            if (this.WebPartManager.DisplayMode.AllowPageDesign && this.AllowLayoutChange)
            {
                highlightColor = ColorTranslator.ToHtml(this.DragHighlightColor);
                dragZoneColor = ColorTranslator.ToHtml(this.DragZoneColor);
            }

            //WebPartZone
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone", ClientID);
            desc.AddProperty("uniqueId", this.UniqueID);
            desc.AddComponentProperty("webPartManager", this.WebPartManager.ClientID);
            desc.AddProperty("allowLayoutChange", this.AllowLayoutChange);
            desc.AddProperty("highlightColor", highlightColor);
            desc.AddProperty("dragZoneColor", dragZoneColor);
            desc.AddProperty("menuPopupCssClass", this.MenuPopupStyle.CssClass);
            desc.AddProperty("isVertical", (this.LayoutOrientation == Orientation.Vertical));
            yield return desc;

            AtlasWebPartChrome webPartChrome = new AtlasWebPartChrome(this, this.WebPartManager);

            //WebPart
            foreach (WebPart webPart in this.WebParts)
            {
                desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart", webPartChrome.GetWebPartChromeClientID(webPart));
                PartChromeType partChromeType = this.GetEffectiveChromeType(webPart);
                if (partChromeType == PartChromeType.TitleOnly || partChromeType == PartChromeType.TitleAndBorder)
                {
                    desc.AddElementProperty("titleElement", webPartChrome.GetWebPartTitleClientID(webPart));
                }
                desc.AddComponentProperty("zone", webPart.Zone.ClientID);
                desc.AddProperty("zoneIndex", webPart.ZoneIndex);
                desc.AddProperty("allowZoneChange", webPart.AllowZoneChange);
                yield return desc;
            }
        }

        /// <summary>
        /// Returns the list of script files that need to be included
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.WebParts.js"));
        }

        private class FixPaddingHtmlTextWriter : HtmlTextWriter
        {
            // Methods
            public FixPaddingHtmlTextWriter(TextWriter writer)
                : base(writer)
            {
            }

            public override void AddStyleAttribute(HtmlTextWriterStyle key, string value)
            {
                if ((((key == HtmlTextWriterStyle.PaddingBottom) || (key == HtmlTextWriterStyle.PaddingLeft)) || ((key == HtmlTextWriterStyle.PaddingRight) || (key == HtmlTextWriterStyle.PaddingTop))) && (value == "1"))
                {
                    base.AddStyleAttribute(key, "1px");
                }
                else
                {
                    base.AddStyleAttribute(key, value);
                }
            }
        }

        private class AtlasWebPartChrome : WebPartChrome
        {
            // Methods
            public AtlasWebPartChrome(WebPartZoneBase zone, WebPartManager manager)
                : base(zone, manager)
            {
            }

            public new string GetWebPartChromeClientID(WebPart webPart)
            {
                return base.GetWebPartChromeClientID(webPart);
            }

            public new string GetWebPartTitleClientID(WebPart webPart)
            {
                return base.GetWebPartTitleClientID(webPart);
            }
        }
    }
}
