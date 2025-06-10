using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validation.Reports.Windward.Core.Model
{
    [Table("TST_TEST_CASE")]
    public class TestCase
    {
        [Key()]
        public int TEST_CASE_ID { get; set; }

        public int EXECUTION_STATUS_ID { get; set; }
        public int? TEST_CASE_PRIORITY_ID { get; set; }
        public int PROJECT_ID { get; set; }
        public int AUTHOR_ID { get; set; }
        public int TEST_CASE_STATUS_ID { get; set; }
        public int TEST_CASE_TYPE_ID { get; set; }
        public int? TEST_CASE_FOLDER_ID { get; set; }
        public string NAME { get; set; }
        public int? OWNER_ID { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime? EXECUTION_DATE { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public DateTime LAST_UPDATE_DATE { get; set; }
        public int? AUTOMATION_ENGINE_ID { get; set; }
        public int? AUTOMATION_ATTACHMENT_ID { get; set; }
        public bool IS_ATTACHMENTS { get; set; }
        public bool IS_TEST_STEPS { get; set; }
        public int? ESTIMATED_DURATION { get; set; }
        public bool IS_DELETED { get; set; }
        public DateTime CONCURRENCY_DATE { get; set; }
        public int? ACTUAL_DURATION { get; set; }
        public string COMPONENT_IDS { get; set; }
        public bool IS_SUSPECT { get; set; }

    }

    public class TestCaseViewModel
    {
        public int  TestCaseId { get; set; }
        public string Name { get; set; }
    }
}