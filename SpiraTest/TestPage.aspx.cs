using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// details of a particular task and handling updates
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Tasks, null, "Task-Tracking", "TaskDetails_Title")]
    public partial class TestPage : PageLayout
    {

    }
}
