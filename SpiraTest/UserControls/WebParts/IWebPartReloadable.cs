using System;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using System.Text;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts
{
    public interface IWebPartReloadable
    {
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider", AllowsMultipleConnections=true)]
        IWebPartReloadable GetReloadable();
        void LoadAndBindData();
    }
}
