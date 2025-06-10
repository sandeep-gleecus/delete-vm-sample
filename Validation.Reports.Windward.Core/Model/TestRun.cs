using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validation.Reports.Windward.Core.Model
{
    [Table("TST_TEST_RUN")]
    public class TestRun
    {
        [Key()]
        public int TEST_RUN_ID { get; set; }
        public int TEST_CASE_ID { get; set; }
        public int TEST_RUN_TYPE_ID { get; set; }
        public int? RELEASE_ID { get; set; }
        public int? TEST_SET_ID { get; set; }
        public int? TEST_SET_TEST_CASE_ID { get; set; }
        public int TESTER_ID { get; set; }
        public int? TEST_RUNS_PENDING_ID { get; set; }
        public int EXECUTION_STATUS_ID { get; set; }
        public DateTime START_DATE { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime? END_DATE { get; set; }
        public int? AUTOMATION_HOST_ID { get; set; }
        public int? AUTOMATION_ENGINE_ID { get; set; }
        public int? BUILD_ID { get; set; }
        public int? TEST_RUN_FORMAT_ID { get; set; }
        public string RUNNER_NAME { get; set; }
        public int? ESTIMATED_DURATION { get; set; }
        public int? ACTUAL_DURATION { get; set; }
        public int? RUNNER_ASSERT_COUNT { get; set; }
        public string RUNNER_TEST_NAME { get; set; }
        public string RUNNER_MESSAGE { get; set; }
        public string RUNNER_STACK_TRACE { get; set; }
        public bool IS_ATTACHMENTS { get; set; }
        public DateTime CONCURRENCY_DATE { get; set; }

    }
}