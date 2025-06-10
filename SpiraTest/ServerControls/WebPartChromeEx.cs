using System;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls.WebParts;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays the customized web-part title bar used in SpiraTest
    /// </summary>
    public class WebPartChromeEx : WebPartChrome
    {
        private Style _titleTextStyle = null;

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="zone">The web part zone reference</param>
        /// <param name="manager">Handle to the web part manager</param>
        public WebPartChromeEx(WebPartZoneBase zone, WebPartManager manager)
            : base(zone, manager)
        {
        }

        protected new WebPartZoneEx Zone
        {
            get
            {
                return (WebPartZoneEx)base.Zone;
            }
        }

        public override void PerformPreRender()
        {
            base.PerformPreRender();

            //Get a handle to the title style
            Style partTitleStyle = this.Zone.PartTitleStyle;
            this._titleTextStyle = this.CreateTitleTextStyle(partTitleStyle);
        }

        private void RenderTitleText(HtmlTextWriter writer, WebPart webPart)
        {
            if (this._titleTextStyle == null)
            {
                this._titleTextStyle = this.CreateTitleTextStyle(this.Zone.PartTitleStyle);
            }
            if (!this._titleTextStyle.IsEmpty)
            {
                this._titleTextStyle.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Title, this.GenerateDescriptionText(webPart), true);
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "float: left");
            string titleUrl = webPart.TitleUrl;
            string text = webPart.DisplayTitle;
            if (!string.IsNullOrEmpty(titleUrl) && !this.DragDropEnabled)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Zone.ResolveClientUrl(titleUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.H3); 
            }
            writer.WriteEncodedText(text);
            writer.RenderEndTag();
        }

        private string GenerateDescriptionText(WebPart webPart)
        {
            string displayTitle = webPart.DisplayTitle;
            string description = webPart.Description;
            if (!string.IsNullOrEmpty(description))
            {
                displayTitle = displayTitle + " - " + description;
            }
            return displayTitle;
        }
         
        private Style CreateTitleTextStyle(Style partTitleStyle)
        {
            Style style = new Style();
            if (partTitleStyle.ForeColor != Color.Empty)
            {
                style.ForeColor = partTitleStyle.ForeColor;
            }
            style.Font.CopyFrom(partTitleStyle.Font);
            return style;
        }
 
        private void RenderButton(System.Web.UI.HtmlTextWriter writer, string clientID, string buttonName, string buttonImage, string altText, WebPart webPart)
        {
            //Get the client web part manager component
            string webPartManagerClientId = this.WebPartManager.ClientID;

            writer.AddStyleAttribute("width", "14px");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, string.Format("{0}{1}", clientID, buttonName));
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            //Take account of the themes folder if theming enabled
            string imagesFolder = "Images";
            if (webPart.Page.EnableTheming && webPart.Page.Theme != "")
            {
                imagesFolder = UrlRewriterModule.ResolveUrl(UrlRewriterModule.ResolveImages("Images/", webPart.Page));
            }

            HtmlImage img = new HtmlImage();
            img.Src = string.Format(imagesFolder + "Widget{0}.gif", buttonImage);
            img.Alt = altText;
            img.Attributes.Add("onmouseover", string.Format("javascript:this.src='" + imagesFolder + "Widget{0}Selected.gif';", buttonImage));
            img.Attributes.Add("onmouseout", string.Format("javascript:this.src='" + imagesFolder + "Widget{0}.gif';", buttonImage));
            if (buttonName == "verbs")
            {
                img.Attributes.Add("id", string.Format("{0}VerbsPopup", clientID));
            }
            else
            {
                string postbackControlId = Zone.UniqueID.Replace('_', '$');
                img.Attributes.Add("onclick", string.Format("$find('" + webPartManagerClientId + "').SubmitPage('{0}', '{2}:{1}');", postbackControlId, webPart.ID, buttonName));
            }
            img.RenderControl(writer);
            writer.RenderEndTag();
            //Span 
            //TD 
            writer.RenderEndTag();
        }

        private void RenderTitleBar(System.Web.UI.HtmlTextWriter writer, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            TitleStyle partTitleStyle = this.Zone.PartTitleStyle;

            string clientID = this.GetWebPartChromeClientID(webPart);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, partTitleStyle.CssClass);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            //Title Icon 
            if (this.Zone.ShowTitleIcons)
            {
                if (!string.IsNullOrEmpty(webPart.TitleIconImageUrl))
                {
                    writer.AddStyleAttribute("width", "1px");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    HtmlImage imgIcon = new HtmlImage();
                    imgIcon.Src = webPart.TitleIconImageUrl;
                    imgIcon.RenderControl(writer);
                    //TD 
                    writer.RenderEndTag();
                }
            }

            //Minimize/Restore Button
            if (this.WebPartManager.DisplayMode.AllowPageDesign)
            {
                if (webPart.ChromeState == PartChromeState.Minimized)
                {
                    RenderButton(writer, clientID, "restore", "Maximize", Resources.ServerControls.WebPartChrome_RestoreWidget, webPart);
                }
                else
                {
                    RenderButton(writer, clientID, "minimize", "Minimize", Resources.ServerControls.WebPartChrome_MinimizeWidget, webPart);
                }
            }

            //Title Text 
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartTitleClientID(webPart));
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderTitleText(writer, webPart);

            //SubTitle hyperlink
            if (!String.IsNullOrEmpty(webPart.Subtitle))
            {
                //The RSS Feed icon (if subtitle = RSS)
                if (GlobalFunctions.SafeSubstring(webPart.Subtitle, GlobalFunctions.WEBPART_SUBTITLE_RSS.Length) == GlobalFunctions.WEBPART_SUBTITLE_RSS)
                {
                    //The RSS URL is passed in as part of the subtitle
                    string rssUrl = webPart.Subtitle.Substring(GlobalFunctions.WEBPART_SUBTITLE_RSS.Length);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, rssUrl);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "w4 h4 ml3 v-top");
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, "RSS Feed");   //Non Localized
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, UrlRewriterModule.ResolveImages("Images/action-rss.svg", webPart.Page));
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();  // IMG
                    writer.RenderEndTag();  // A
                }
                else
                {
                    string postbackControlId = Zone.UniqueID.Replace('_', '$');
                    string webPartManagerClientId = this.WebPartManager.ClientID;
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "u-btn br-pill mb0 ml4 px3 py1 fs-80"); // classes to define the button and its styling / position
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("$find('" + webPartManagerClientId + "').SubmitPage('{0}', '{2}:{1}');", postbackControlId, webPart.ID, "DrillDown"));
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write( webPart.Subtitle );
                    writer.RenderEndTag();
                    //A 
                }
            }
            writer.RenderEndTag();
            //TD 

            //Only display the buttons if in Design mode
            if (this.WebPartManager.DisplayMode.AllowPageDesign)
            {
                //Edit Settings Button 
                RenderButton(writer, clientID, "edit", "Settings", Resources.ServerControls.WebPartChrome_ChangeSettings, webPart);

                //Close Button 
                RenderButton(writer, clientID, "close", "Close", Resources.ServerControls.WebPartChrome_CloseWidget, webPart);
            }
            writer.RenderEndTag();
            //TR 
            writer.RenderEndTag();
            //Table 

            //Chrome Verbs Pop-up Menu 
            if (this.WebPartManager.DisplayMode.AllowPageDesign)
            {
                writer.AddAttribute("id", string.Format("{0}verbsMenu", clientID));
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
               
                //Now add the verbs
                WebPartVerbCollection webPartVerbs = this.GetWebPartVerbs(webPart);
                webPartVerbs = this.FilterWebPartVerbs(webPartVerbs, webPart);
                foreach (WebPartVerb verb in webPartVerbs)
                {
                    RenderVerb(writer, webPart, verb);
                }
                writer.RenderEndTag();
                //Div 
            }
        }
        private void RenderVerb(System.Web.UI.HtmlTextWriter writer, WebPart webpart, WebPartVerb verb)
        {
            //Get the client web part manager component
            string webPartManagerClientId = this.WebPartManager.ClientID;

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (String.IsNullOrEmpty(verb.ID))
            {
                //Build in verb
                writer.AddAttribute("onclick", string.Format("$find('" + webPartManagerClientId + "').SubmitPage('{0}', '{1}:{2}');", Zone.ClientID.Replace('_', '$'), verb.Text.ToLowerInvariant(), webpart.ID));
            }
            else
            {
                //Custom verb
                writer.AddAttribute("onclick", string.Format("$find('" + webPartManagerClientId + "').SubmitPage('{0}', 'partverb:{1}:{2}');", Zone.ClientID.Replace('_', '$'), verb.ID, webpart.ID));
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
            writer.AddStyleAttribute("white-space", "nowrap");
            if (!verb.Enabled)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            if (!string.IsNullOrEmpty(verb.ImageUrl))
            {
                ImageEx img = new ImageEx();
                img.ImageUrl = verb.ImageUrl;
                img.Width = new Unit(20);
                img.Height = new Unit(20);
                img.RenderControl(writer);
            }
            writer.Write(verb.Text);
            writer.RenderEndTag();
            //A 
            //DIV 
            writer.RenderEndTag();
        }
        public override void RenderWebPart(System.Web.UI.HtmlTextWriter writer, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            if ((webPart == null))
            {
                throw new ArgumentNullException("webPart");
            }
            bool flag = (this.Zone.LayoutOrientation == Orientation.Vertical);
            PartChromeType chromeType = this.Zone.GetEffectiveChromeType(webPart);
            Style style = this.CreateWebPartChromeStyle(webPart, chromeType);
            if (style != null && !String.IsNullOrEmpty(style.CssClass))
            {
                //We only want to write out the CSS class since it varies by mode (browse vs. design)
                writer.AddAttribute(HtmlTextWriterAttribute.Class, style.CssClass);
                //style.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (flag)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else if ((webPart.ChromeState != PartChromeState.Minimized))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            if (this.Zone.RenderClientScript)
            {
                //This ID is important for the draggability. 
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartChromeClientID(webPart));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "TitleBar");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            RenderTitleBar(writer, webPart);
            writer.RenderEndTag();
            //TD 
            writer.RenderEndTag();
            //TR 

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (webPart.ChromeState == PartChromeState.Minimized)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }
            Style partStyle = this.Zone.PartStyle;
            if (!partStyle.IsEmpty)
            {
                partStyle.AddAttributesToRender(writer, this.Zone);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderPartContents(writer, webPart);
            writer.RenderEndTag();
            //TD 
            writer.RenderEndTag();
            //TR 

            //Table 
            writer.RenderEndTag();
        } 
    }
}
