using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validation.Reports.Windward.Core.Model
{
    [Table("TST_PROJECT")]
    public class Project
    {
        //public int ProjectId { get; set; }
        //public string Name { get; set; }
        //public bool IsActive { get; set; }
        [Key()]
        public int PROJECT_ID {get; set;} 
        public int PROJECT_GROUP_ID {get; set;} 
        public string NAME {get; set;} 
        public string DESCRIPTION {get; set;} 
        public DateTime CREATION_DATE {get; set;}  
        public string WEBSITE {get; set;} 
        public bool IS_ACTIVE {get; set;}
        public int WORKING_HOURS {get; set;}  
        public int WORKING_DAYS {get; set;} 
        public int NON_WORKING_HOURS {get; set;} 
        public bool IS_TIME_TRACK_INCIDENTS {get; set;}  
        public bool IS_TIME_TRACK_TASKS {get; set;} 
        public bool IS_EFFORT_INCIDENTS {get; set;} 
        public bool IS_EFFORT_TASKS {get; set;}  
        public bool IS_TASKS_AUTO_CREATE {get; set;}
        public decimal? REQ_DEFAULT_ESTIMATE {get; set;}
        public int REQ_POINT_EFFORT {get; set;} 
        public int? TASK_DEFAULT_EFFORT {get; set;} 
        public bool IS_REQ_STATUS_BY_TASKS {get; set;}
        public bool IS_REQ_STATUS_BY_TEST_CASES {get; set;} 
        public bool IS_EFFORT_TEST_CASES {get; set;} 
        public bool IS_REQ_STATUS_AUTO_PLANNED {get; set;}  
    }

    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
    }
}
