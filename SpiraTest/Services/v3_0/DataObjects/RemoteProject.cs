using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a single Project in the system
    /// </summary>
    public class RemoteProject
    {
        /// <summary>
        /// The id of the project
        /// </summary>
        public Nullable<int> ProjectId;

        /// <summary>
        /// The name of the project
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the project
        /// </summary>
        public String Description;

        /// <summary>
        /// The url associated with the project
        /// </summary>
        public String Website;
        
        /// <summary>
        /// The date/time the project was created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// Whether the project is active or not
        /// </summary>
        public bool Active = true;

        /// <summary>
        /// How many working hours are in a day for this project
        /// </summary>
        public int WorkingHours;

        /// <summary>
        /// How many working days are in a week for this project
        /// </summary>
        public int WorkingDays;

        /// <summary>
        /// How many special non-working hours are there in a month in the project
        /// </summary>
        public int NonWorkingHours;
    }
}
