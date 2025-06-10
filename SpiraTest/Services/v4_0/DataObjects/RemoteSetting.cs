using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Stores a single system setting entry
    /// </summary>
    public class RemoteSetting
    {
        /// <summary>
        /// The name of the setting
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the setting
        /// </summary>
        public string Value;
    }
}
