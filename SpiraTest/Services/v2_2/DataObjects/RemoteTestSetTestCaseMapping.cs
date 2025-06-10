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
    /// Stores the mapping/coverage between Test Sets and Test Cases
    /// </summary>

    public class RemoteTestSetTestCaseMapping
    {
        public int TestSetId;
        public int TestCaseId;
    }
}
