using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions
    /// </summary>
    public partial class NotificationEvent : Entity
    {
        //The different artifact notification types
        public enum ProjectArtifactNotifyTypeEnum
        {
            Creator = 1,
            Owner = 2
        }
    }
}
