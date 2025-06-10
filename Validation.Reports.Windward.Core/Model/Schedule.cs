using System;
using System.ComponentModel.DataAnnotations;
using Validation.Reports.Windward.Core.Repository;

namespace Validation.Reports.Windward.Core.Model
{
    public class Schedule : ScheduleRepository<Schedule>
    {
        [Key]
        public int ScheduleId { get; set; }

        [Display(Name = "Delivery Type")]
        public string DeliveryType { get; set; }

        [Display(Name = "Output Type")]
        public string OutputType { get; set; }

        public string Parameters { get; set; }

        [Display(Name = "Schedule Date and Time(UTC)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm:ss}")] // "{0:MM/dd/yyyy}")]
        public DateTime ScheduledTime { get; set; }

        public string Status { get; set; }

        public int TemplateId { get; set; }

        [Display(Name = "Template Name")]
        public string TemplateName { get; set; }
      
        [Display(Name = "Calling App")]
        public string CallingApp { get; set; }

        [Display(Name = "Delivery Location")]
        public string DeliveryLocation { get; set; }

        [Display(Name = "User Folder")]
        public string UserFolder { get; set; }

        [Display(Name = "Output File Name")]
        public string OutputFileName { get; set; }

        public Guid ScheduleGroupId { get; set; }

        [Display(Name = "Created Date(UTC)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm:ss}")]

        public DateTime CreatedDate { get; set; }

        public void AddNewEntry()
        {
            Add(this);
            SaveChanges();
        }
    }

    //[DataContract]
    //public class ScheduleAPI
    //{
    //    [DataMember]
    //    public int ScheduleId { get; set; }

    //    [DataMember]
    //    [Display(Name = "Delivery Type")]
    //    public string DeliveryType { get; set; }

    //    [DataMember]
    //    [Display(Name = "Output Type")]
    //    public string OutputType { get; set; }

    //    [DataMember]
    //    public string Parameters { get; set; }
    //    public System.Guid ScheduleGroupId { get; set; }

    //    [DataMember]
    //    [Display(Name = "Schedule Date and Time")]
    //    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm:ss}")] // "{0:MM/dd/yyyy}")]
    //    public DateTime ScheduledTime { get; set; }

    //    [DataMember]
    //    public string Status { get; set; }

    //    [DataMember]
    //    public System.Guid TemplateId { get; set; }

    //    [DataMember]
    //    [Display(Name = "Template Name")]
    //    public string TemplateName { get; set; }

    //    [DataMember]
    //    public string User { get; set; }

    //    [DataMember]
    //    public string TemplateUNID { get; set; }

    //    [DataMember]
    //    public Guid SeriesId { get; set; }

    //    [DataMember]
    //    [Display(Name = "Employee Number")]
    //    public string EmployeeNumber { get; set; }

    //    [DataMember]
    //    [Display(Name = "Calling App")]
    //    public string CallingApp { get; set; }

    //    [DataMember]
    //    [Display(Name = "Delivery Location")]
    //    public string DeliveryLocation { get; set; }

    //    [DataMember]
    //    public Guid DocumentSetId { get; set; }

    //    [DataMember]
    //    [Display(Name = "Created Name")]
    //    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm:ss}")]
    //    public DateTime CreatedDate { get; set; }
    //}
}
