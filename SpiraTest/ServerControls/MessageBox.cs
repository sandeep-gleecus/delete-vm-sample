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
    /// This class displays the message box used to display informational, warning and error messages on the page.
    /// It can be called by either client (AJAX) or server-side code
    /// </summary>
    [
    DefaultProperty("Text"),
    ToolboxData("<{0}:MessageBox runat=server></{0}:MessageBox>")
    ]
    public class MessageBox : WebControl, ITextControl
    {
        #region Enumerations

        /// <summary>
        /// The type of message being displayed
        /// </summary>
        public enum MessageType
        {
            Error = 1,
            Information = 2,
            Success = 3,
            Warning = 4
        }

        #endregion

        /// <summary>
        /// The type of message being displayed (default is error)
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(MessageType.Error),
        Localizable(false)
        ]
        public MessageType Type
        {
            get
            {
                if (ViewState["Type"] == null)
                {
                    return MessageType.Error;
                }
                else
                {
                    return (MessageType)ViewState["Type"];
                }
            }

            set
            {
                ViewState["Type"] = value;
            }
        }

        /// <summary>
        /// The text of the message being displayed
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                //If we reset the text, also reset the type
                if (String.IsNullOrEmpty(value))
                {
                    this.Type = MessageType.Error;
                }
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// The css class to use when displaying an informational message
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string InformationCssClass
        {
            get
            {
                String s = (String)ViewState["InformationCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["InformationCssClass"] = value;
            }
        }

        /// <summary>
        /// The css class to use when displaying an error
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string ErrorCssClass
        {
            get
            {
                String s = (String)ViewState["ErrorCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["ErrorCssClass"] = value;
            }
        }

        /// <summary>
        /// The css class to use when displaying success
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string SuccessCssClass
        {
            get
            {
                String s = (String)ViewState["SuccessCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["SuccessCssClass"] = value;
            }
        }

        /// <summary>
        /// The css class to use when displaying a warning
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string WarningCssClass
        {
            get
            {
                String s = (String)ViewState["WarningCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["WarningCssClass"] = value;
            }
        }

        /// <summary>
        /// The css class to use when no message is to be displayed
        /// We always render the DIV so that client side code can make it visible
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string HiddenCssClass
        {
            get
            {
                String s = (String)ViewState["HiddenCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["HiddenCssClass"] = value;
            }
        }

        /// <summary>
        /// Renders the message box and dismissable button
        /// </summary>
        /// <param name="output">The output writer</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            //Render the dismiss button
            //<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
            output.AddAttribute("type", "button");
            output.AddAttribute("class", "close");
            output.AddAttribute("data-hide", "alert");
            output.AddAttribute("aria-label", "Close");
            output.RenderBeginTag(HtmlTextWriterTag.Button);
            output.RenderBeginTag(HtmlTextWriterTag.Span);
            output.AddAttribute("aria-hidden", "true");
            output.Write("&times;");
            output.RenderEndTag();
            output.RenderEndTag();

            //Render the text
            output.WriteBeginTag("span");
            output.WriteAttribute("id", this.ClientID + "_text");
            output.Write(">");
            output.Write(Text);
            output.WriteEndTag("span");
        }

        /// <summary>
        /// Adds the javascript code for dismissing tooltips
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Display the code to handle persistent tooltip
            string script = @"
(function () {
    var closeButtons = document.querySelectorAll('.close[data-hide]');
    if (closeButtons) {
        for (var i = 0; i < closeButtons.length; i++) {
            // try catch used here to deal with closure issues - without it only the final [i] gets used on the parent node - we need each element in the loop to be handled individually
            try{throw i}
            catch(ii) {
                closeButtons[ii].addEventListener('click', function (i) {
                    closeButtons[ii].parentNode.className = 'alert alert-hidden';
                    console.log(closeButtons[ii].parentNode.id , closeButtons[ii].parentElement.className);
                }, false);
            }
        }
    }
})();
";
            this.Page.ClientScript.RegisterStartupScript(typeof(MessageBox), "AlertDismiss", script, true);
        }

        /// <summary>
        /// Determine what css class to apply to the message box
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);

            //If we have a message then change the CSS, otherwise render the hidden CSS
            if (String.IsNullOrEmpty(this.Text))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.HiddenCssClass);
            }
            else
            {
                //Set the appropriate css class
                if (this.Type == MessageType.Error)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.ErrorCssClass);
                }
                if (this.Type == MessageType.Information)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.InformationCssClass);
                }
                if (this.Type == MessageType.Warning)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.WarningCssClass);
                }
                if (this.Type == MessageType.Success)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.SuccessCssClass);
                }
            }

            //Add the ARIA role
            writer.AddAttribute("role", "alert");
        }

        /// <summary>
        /// Changes the base tag to a DIV instead of a span
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }
    }
}
