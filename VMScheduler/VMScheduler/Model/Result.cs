using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMScheduler.Model
{
    public class Result
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Status { get; set; }
        public string Parameters { get; set; }
        public string Filename { get; set; }
        public string DeliveryLocation { get; set; }
        public int ScheduleId { get; set; }
        public string OutputFileName { get; set; }
    }
}
