using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    public partial class IncidentResolution : Entity
    {
        #region Lookup Properties

        /// <summary>
        /// Returns the Incident Resolution Creator's Name
        /// </summary>
        public string CreatorName
        {
            get
            {
                if (Creator != null && Creator.Profile != null)
                {
                    return Creator.Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
