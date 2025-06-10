using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Validation.Reports.Windward.Core.Model
{
    public class RequestVariable
    {
        public int RequestVariableId { get; set; }
        public int TemplateId { get; set; }
        public string VariableName { get; set; }
        public string Label { get; set; }
        public string DefaultValue { get; set; }
        public bool Required { get; set; }
        public int Position { get; set; }
    }
}
