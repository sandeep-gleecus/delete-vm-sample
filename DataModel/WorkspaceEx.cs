using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the Workspace virtual object
    /// </summary>
    public partial class Workspace : Entity
    {
        #region Enumerations

        /// <summary>
        /// The different Workspace roles
        /// </summary>
        public enum WorkspaceTypeEnum
        {
            Product = 1,
            Program = 2,
            ProjectTemplate = 3,
            Portfolio = 4,
            Enterprise = 5,
        }

        #endregion
    }
}
