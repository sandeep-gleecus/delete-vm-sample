using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in validator to correctly handle the client-side validation of certain
    /// custom AJAX controls such as the DropDownList and DropDownHierarchy
    /// </summary>
    [ToolboxData("<{0}:RequiredFieldValidatorEx runat=server></{0}:RequiredFieldValidatorEx>")]
    public class RequiredFieldValidatorEx : RequiredFieldValidator
    {
        /// <summary>
        /// Handles the case of custom ajax controls that need different ids to get their values
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            //See if we have a known control that has to be handled differently
            Control control = this.FindControl(this.ControlToValidate);
            if (control != null)
            {
                if (control is DropDownHierarchy || control is DropDownListEx)
                {
                    string valueClientId = GetControlRenderID(this.ControlToValidate) + "_Value";
                    this.ControlToValidate = "";
                    Page page = control.Page;
                    if (ScriptManager.GetCurrent(this.Page) == null)
                    {
                        page.ClientScript.RegisterExpandoAttribute(this.ClientID, "controltovalidate", valueClientId, true);
                    }
                    else
                    {
                        ScriptManager.RegisterExpandoAttribute(this, this.ClientID, "controltovalidate", valueClientId, true);
                    }
                }
            }
            base.AddAttributesToRender(writer);
        }
    }
}
