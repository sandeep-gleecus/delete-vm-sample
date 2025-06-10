using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a single filter that can be applied to a Retrieve query
    /// </summary>
    public class RemoteFilter
    {
        public string PropertyName;
        public Nullable<int> IntValue;
        public String StringValue;
        public MultiValueFilter MultiValue;
        public DateRange DateRangeValue;
    }
}
