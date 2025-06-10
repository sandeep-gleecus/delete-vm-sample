using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extension Properties
    /// </summary>
    public partial class HistoryChangeView : Entity
    {
        /// <summary>
        /// The user that changed it
        /// </summary>
        public int ChangerId
        {
            get
            {
                return this.UserId;
            }
            set
            {
                this.UserId = value;
            }
        }

        /// <summary>
        /// The type of change set
        /// </summary>
        public int ChangeSetTypeId
        {
            get
            {
                return this.ChangeTypeId;
            }
            set
            {
                this.ChangeTypeId = value;
            }
        }

        /// <summary>
        /// The type of change
        /// </summary>
        public string ChangeSetTypeName
        {
            get
            {
                return this.ChangeName;
            }
            set
            {
                this.ChangeName = value;
            }
        }
    }
}
