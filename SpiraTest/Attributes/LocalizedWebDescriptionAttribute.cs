using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Attributes
{
    /// <summary>
    /// Used in Web Parts that need to have user configurable properties that have localizable descriptions
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LocalizedWebDescriptionAttribute : WebDescriptionAttribute
    {
        bool isLocalized;

        public LocalizedWebDescriptionAttribute(string description)
            : base(description)
        {
        }

        /// <summary>
        /// The display name of the web part property
        /// </summary>
        public override string Description
        {
            get
            {
                if (!isLocalized)
                {
                    this.DescriptionValue = Resources.Main.ResourceManager.GetString(base.Description, Resources.Main.Culture);
                    isLocalized = true;
                }
                return base.Description;
            }
        }
    }
}