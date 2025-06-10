//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace VMScheduler.Model
//{
//    public class ArchiveScheduler
//    {
//        [Key]
//        public int ScheduleId { get; set; }
//        public Guid ScheduleGroupId { get; set; }
//        public Guid TemplateId { get; set; }
//        public string TemplateName { get; set; }
//        public string Parameters { get; set; }
//        public string OutputType { get; set; }
//        public DateTime ScheduledTime { get; set; }
//        public string User { get; set; }
//        public string DeliveryType { get; set; } //   Attachment, Link
//        public string Status { get; set; }      //    Ready, In Process, Failed, Completed
//        public string TemplateUNID { get; set; }
//        public Guid SeriesId { get; set; }
//        public string EmployeeNumber { get; set; }
//        public string CallingApp { get; set; }
//        public string DeliveryLocation { get; set; }
//        public Guid DocumentSetId { get; set; }
//        public DateTime CreatedDate { get; set; }
//    }
//}
