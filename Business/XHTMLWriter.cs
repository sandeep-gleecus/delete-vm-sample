using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Overrides the standard XML text writer to always return a full closing tag
    /// (e.g. <DIV></DIV> instead of <DIV />) which is needed for the help/reports systems to work correctly
    /// </summary>
    /// <remarks>BR tags are left the way they are to avoid double-spacing</remarks>
    public class XHTMLWriter : System.Xml.XmlTextWriter
    {
        string lastStartElement = "";
        public XHTMLWriter(string filename, System.Text.Encoding encoding)
            : base(filename, encoding)
        {
        }
        public XHTMLWriter(System.IO.Stream stream, System.Text.Encoding encoding)
            : base(stream, encoding)
        {
        }
        public XHTMLWriter(System.IO.TextWriter textwriter)
            : base(textwriter)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.lastStartElement = localName;
            base.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteEndElement()
        {
            if (this.lastStartElement.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "br")
            {
                base.WriteEndElement();
            }
            else
            {
                base.WriteFullEndElement();
            }
        }
    }
}
