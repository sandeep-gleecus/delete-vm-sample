using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends LabelEx to handle the case of a rich-text control where you need to make sure that no
    /// dangerous client-side script gets emitted. We do that through a two-stage process:
    /// 1) We remove any script tags on the server side
    /// 2) We run the client-side globalFunctions.cleanHtml(node) to remove any inline event handlers
    /// </summary>
    public class RichTextLabel : LabelEx
    {
        /// <summary>
        /// Before rendering, strip out any SCRIPT tags from the text
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Text = GlobalFunctions.HtmlScrubInput(this.Text);

            //If multiline, specify the max height if specified
            if (this.MultiLine && !this.MaxHeight.IsEmpty)
            {
                this.Style.Add("max-height", this.MaxHeight.ToString());
            }

            //Register the client-code to remove any inline event handlers (onclick, etc.)
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID.ToString(), "globalFunctions.cleanHtml($get('" + this.ClientID + "')); \n", true);
        }

        /// <summary>
        /// Is this label a multiline (e.g. rich text read-only description)
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Is this label a multiline (e.g. rich text read-only description)"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool MultiLine
        {
            get
            {
                if (ViewState["MultiLine"] == null)
                {
                    return false;
                }
                else
                {
                    return ((bool)ViewState["MultiLine"]);
                }
            }
            set
            {
                ViewState["MultiLine"] = value;
            }
        }

        /// <summary>
        /// Is there a max-height for this control (used with multiline)
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Is there a max-height for this control (used with multiline)"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit MaxHeight
        {
            get
            {
                if (ViewState["MaxHeight"] != null)
                {
                    return ((Unit)ViewState["MaxHeight"]);
                }
                return Unit.Empty;
            }
            set
            {
                ViewState["MaxHeight"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (MultiLine)
                {
                    return HtmlTextWriterTag.Div;
                }
                else
                {
                    return base.TagKey;
                }
            }
        }
    }
}
