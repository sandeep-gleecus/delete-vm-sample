using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validation.Reports.Windward.Core.Model
{
    [Table("TST_TEST_SET")]
    public class TestSet
    {
        [Key()]
        public int TEST_SET_ID { get; set; }
        public int PROJECT_ID { get; set; }
        public int? RELEASE_ID { get; set; }
        public int TEST_SET_STATUS_ID { get; set; }
        public int CREATOR_ID { get; set; }
        public int? OWNER_ID { get; set; }
        public int? AUTOMATION_HOST_ID { get; set; }
        public int? TEST_RUN_TYPE_ID { get; set; }
        public int? RECURRENCE_ID { get; set; }
        public int? TEST_SET_FOLDER_ID { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public DateTime? PLANNED_DATE { get; set; }
        public DateTime LAST_UPDATE_DATE { get; set; }
        public bool IS_ATTACHMENTS { get; set; }
        public bool IS_DELETED { get; set; }
        public DateTime CONCURRENCY_DATE { get; set; }
        public int? BUILD_EXECUTE_TIME_INTERVAL { get; set; }
        public int? ESTIMATED_DURATION { get; set; }
        public int? ACTUAL_DURATION { get; set; }
        public int COUNT_PASSED { get; set; }
        public int COUNT_FAILED { get; set; }
        public int COUNT_CAUTION { get; set; }
        public int COUNT_BLOCKED { get; set; }
        public int COUNT_NOT_RUN { get; set; }
        public int COUNT_NOT_APPLICABLE { get; set; }
        public DateTime? EXECUTION_DATE { get; set; }
        public bool IS_DYNAMIC { get; set; }
        public string DYNAMIC_QUERY { get; set; }
        public bool IS_AUTO_SCHEDULED { get; set; }
        public int? TEST_CONFIGURATION_SET_ID { get; set; }
    }
}
