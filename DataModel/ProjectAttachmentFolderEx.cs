using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extensions added to document folders
    /// </summary>
    public partial class ProjectAttachmentFolder : Entity
    {
        /// <summary>
        /// Allows document folders to be used in the same list as documents
        /// </summary>
        public string Filename
        {
            get
            {
                return this.Name;
            }
        }
    }
}
