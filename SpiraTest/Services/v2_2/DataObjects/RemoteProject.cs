using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a single Project in the system
    /// </summary>
    public class RemoteProject
    {
        public Nullable<int> ProjectId;
        public string Name;
        public String Description;
        public String Website;
        public DateTime CreationDate;
        public bool Active;
        public int WorkingHours;
        public int WorkingDays;
        public int NonWorkingHours;
    }
}
