using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    public partial class WorkflowFieldState
    {
        //Workflow field state types
        public enum WorkflowFieldStateEnum
        {
            Inactive = 1,
            Required = 2,
            Hidden = 3
        }
    }
}
