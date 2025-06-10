using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in change password box to better report back validation messages
    /// </summary>
    [ToolboxData("<{0}:ChangePasswordEx runat=server></{0}:ChangePasswordEx>")]
    public class ChangePasswordEx : ChangePassword
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ServerControls.ChangePasswordEx::";

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangePasswordEx()
            : base()
        {
        }
    }
}