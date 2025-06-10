using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single Project in the system
    /// </summary>
    public class RemoteProject
    {
        /// <summary>
        /// The id of the project (integer)
        /// </summary>
        public Nullable<int> ProjectId;

        /// <summary>
        /// The id of the project template (null on create, means create a new template)
        /// </summary>
        public int? ProjectTemplateId;

        /// <summary>
        /// The id of the project group / program (null on create, means use default group)
        /// </summary>
        public int? ProjectGroupId;

        /// <summary>
        /// The name of the project (string)
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the project (string)
        /// </summary>
        public String Description;

        /// <summary>
        /// The url associated with the project (string)
        /// </summary>
        public String Website;
        
        /// <summary>
        /// The date/time the project was created (datetime)
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// Whether the project is active or not (boolean)
        /// </summary>
        public bool Active = true;

        /// <summary>
        /// How many working hours are in a day for this project (integer)
        /// </summary>
        public int WorkingHours;

        /// <summary>
        /// How many working days are in a week for this project (integer)
        /// </summary>
        public int WorkingDays;

        /// <summary>
        /// How many special non-working hours are there in a month in the project (integer)
        /// </summary>
        public int NonWorkingHours;

        /// <summary>
        /// The start date of the project for planning purposes
        /// </summary>
        public DateTime? StartDate;

        /// <summary>
        /// The end date of the project for planning purposes
        /// </summary>
        public DateTime? EndDate;

        /// <summary>
        /// The percentage complete of the project
        /// </summary>
        public int PercentComplete;
    }
}
