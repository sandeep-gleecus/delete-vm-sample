using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extension methods/properties on the ReportElement entity
    /// </summary>
    public partial class ReportElement : Entity
    {
        /// <summary>
        /// Gets the first section id associated with this element
        /// </summary>
        public int ReportSectionId
        {
            get
            {
                if (this.Sections.Count < 1)
                {
                    return -1;
                }
                return this.Sections[0].ReportSectionId;
            }
        }

        /// <summary>
        /// Gets the first section name associated with this element
        /// </summary>
        public string SectionName
        {
            get
            {
                if (this.Sections.Count < 1)
                {
                    return "";
                }
                return this.Sections[0].Name;
            }
        }
    }
}
