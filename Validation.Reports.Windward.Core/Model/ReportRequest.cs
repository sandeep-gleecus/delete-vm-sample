using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inflectra.SpiraTest.DataModel;

namespace Validation.Reports.Windward.Core.Model
{
    public class ReportRequest
    {
        public Template TemplateDefinition { get; set; }
        public List<RequestVariable> Variables { get; set; }
    }
}
