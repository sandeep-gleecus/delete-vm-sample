using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("CssUrl")]
    [ParseChildren(true, "InlineStyle")]
    [PersistChildren(false)]
    [ToolboxData("<{0}:Style runat=\"server\"></{0}:Style>")]
    [Themeable(true)]
    public class ThemeStyle : Control
    {
        public ThemeStyle()
        {
            // set default value... for some reason the DefaultValue attribute do
            // not set this as I would have expected.
            TargetMedia = "All";
        }

        #region Properties

        [Browsable(true)]
        [Category("Style sheet")]
        [DefaultValue("")]
        [Description("The url to the style sheet.")]
        [UrlProperty("*.css")]
        public string CssUrl
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Style sheet")]
        [DefaultValue("All")]
        [Description("The target media(s) of the style sheet. See http://www.w3.org/TR/REC-CSS2/media.html for more information.")]
        public string TargetMedia
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Style sheet")]
        [DefaultValue(EmbedType.Link)]
        [Description("Specify how to embed the style sheet on the page.")]
        public EmbedType Type
        {
            get; set;
        }

        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public string InlineStyle
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Conditional comment")]
        [DefaultValue("")]
        [Description("Specifies a conditional comment expression to wrap the style sheet in. See http://msdn.microsoft.com/en-us/library/ms537512.aspx")]
        public string ConditionalCommentExpression
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Conditional comment")]
        [DefaultValue(CommentType.DownlevelHidden)]
        [Description("Whether to reveal the conditional comment expression to downlevel browsers. Default is to hide. See http://msdn.microsoft.com/en-us/library/ms537512.aspx")]
        public CommentType ConditionalCommentType
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Behavior")]
        public override string SkinID { get; set; }

        #endregion

        protected override void Render(HtmlTextWriter writer)
        {            
            // add empty line to make output pretty
            writer.WriteLine();

            // prints out begin condition comment tag
            if (!string.IsNullOrEmpty(ConditionalCommentExpression))
                writer.WriteLine(ConditionalCommentType == CommentType.DownlevelRevealed ? "<!{0}>" : "<!--{0}>",
                                 ConditionalCommentExpression);

            if (!string.IsNullOrEmpty(CssUrl))
            {               
                // add shared attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");

                // render either import or link tag
                if (Type == EmbedType.Link)
                {
                    //Add on a version number to the link URL
                    string clientUrl = ResolveUrl(CssUrl);
                    // <link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\" />
                    if (clientUrl.EndsWith(".css"))
                    {
                        clientUrl += "?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER;
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, clientUrl);
                    writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                    writer.AddAttribute("media", TargetMedia);
                    writer.RenderBeginTag(HtmlTextWriterTag.Link);
                    writer.RenderEndTag();
                }
                else
                {
                    // <style type="text/css">@import "modern.css" screen;</style>
                    writer.RenderBeginTag(HtmlTextWriterTag.Style);
                    writer.Write("@import \"{0}\" {1};", ResolveUrl(CssUrl), TargetMedia);
                    writer.RenderEndTag();
                }
            }

            if(!string.IsNullOrEmpty(InlineStyle))
            {
                // <style type="text/css">... inline style ... </style>
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                writer.Write(InlineStyle);
                writer.RenderEndTag();
            }

            // prints out end condition comment tag
            if (!string.IsNullOrEmpty(ConditionalCommentExpression))
            {
                // add empty line to make output pretty
                writer.WriteLine();
                writer.WriteLine(ConditionalCommentType == CommentType.DownlevelRevealed ? "<![endif]>" : "<![endif]-->");
            }
        }
    }
    
    public enum EmbedType
    {        
        Link = 0,
        Import = 1,
    }

    public enum CommentType
    {
        DownlevelHidden = 0,
        DownlevelRevealed = 1
    }
}
