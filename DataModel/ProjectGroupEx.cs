using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the ProjectGroup entity
    /// </summary>
    public partial class ProjectGroup : Entity
    {
        #region Enumerations

        /// <summary>
        /// The different project group roles
        /// </summary>
        public enum ProjectGroupRoleEnum
        {
            GroupOwner = 1,
            Executive = 2
        }

        #endregion

		/// <summary>
		/// Returns the portfolio name of the program
		/// </summary>
		public string PortfolioName
		{
			get
			{
				if (Portfolio == null)
				{
					return "";
				}
				else
				{
					return Portfolio.Name;
				}
			}
		}
	}
}
