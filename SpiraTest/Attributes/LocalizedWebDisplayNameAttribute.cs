using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Resources;

namespace Inflectra.SpiraTest.Web.Attributes
{
    /// <summary>
    /// Used in Web Parts that need to have user configurable properties that have localizable names
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LocalizedWebDisplayNameAttribute : WebDisplayNameAttribute
    {
        bool isLocalized;

        public LocalizedWebDisplayNameAttribute(string displayName)
            : base(displayName)
        {
        }

        /// <summary>
        /// The display name of the web part property
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (!isLocalized)
                {
                    this.DisplayNameValue = Resources.Main.ResourceManager.GetString(base.DisplayName, Resources.Main.Culture);
                    isLocalized = true;
                }
                return base.DisplayName;
            }
        }
    }
}