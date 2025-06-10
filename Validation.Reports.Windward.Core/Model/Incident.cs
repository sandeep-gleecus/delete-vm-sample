using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validation.Reports.Windward.Core.Model
{
    [Table("TST_INCIDENT")]
    public class Incident
    {
        [Key()]
        public int INCIDENT_ID { get; set; }
        public int? SEVERITY_ID { get; set; }
        public int? DETECTED_RELEASE_ID { get; set; }
        public int? PRIORITY_ID { get; set; }
        public int PROJECT_ID { get; set; }
        public int INCIDENT_STATUS_ID { get; set; }
        public int INCIDENT_TYPE_ID { get; set; }
        public int OPENER_ID { get; set; }
        public int? OWNER_ID { get; set; }
        public string NAME { get; set; }
        public int? RESOLVED_RELEASE_ID { get; set; }
        public string DESCRIPTION { get; set; }
        public int? VERIFIED_RELEASE_ID { get; set; }
        public int? BUILD_ID { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public DateTime LAST_UPDATE_DATE { get; set; }
        public bool IS_ATTACHMENTS { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? CLOSED_DATE { get; set; }
        public int COMPLETION_PERCENT { get; set; }
        public int? ESTIMATED_EFFORT { get; set; }
        public int? ACTUAL_EFFORT { get; set; }
        public int? PROJECTED_EFFORT { get; set; }
        public int? REMAINING_EFFORT { get; set; }
        public bool IS_DELETED { get; set; }
        public string COMPONENT_IDS { get; set; }
        public DateTime CONCURRENCY_DATE { get; set; }
        public int? RANK { get; set; }
    }
}