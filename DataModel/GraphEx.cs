using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the Graph entity
    /// </summary>
    public partial class Graph : Entity
    {
        #region Enumerations

        /// <summary>
        /// The different types of graph
        /// </summary>
        public enum GraphTypeEnum
        {
            DateRangeGraphs = 1,
            SummaryGraphs = 2,
            SnapshotGraphs = 3,
            CustomGraphs = 4
        }

        /// <summary>
        /// The different types of graph series
        /// </summary>
        /// <remarks>
        /// Used in the snapshot graphs only, where we can have series of different types
        /// </remarks>
        public enum GraphSeriesTypeEnum
        {
            Bar = 1,
            Line = 2,
            CumulativeBar = 3
        }

        /// <summary>
        /// The list of standard graphs
        /// </summary>
        public enum GraphEnum
        {
            None = 0,
            IncidentSummary = 1,
            IncidentProgressRate = 2,
            IncidentCumulativeCount = 3,
            IncidentOpenCount = 4,
            IncidentAging = 5,
            IncidentTurnaround = 6,
            RequirementSummary = 7,
            RequirementCoverage = 8,
            TestCaseSummary = 9,
            TestRunSummary = 10,
            TestRunProgressRate = 11,
            TaskSummary = 12,
            TaskVelocity = 13,
            TaskBurnUp = 14,
            TaskBurnDown = 15,
            TestSetSummary = 16,
            RequirementVelocity = 17,
            RequirementBurnUp = 18,
            RequirementBurnDown = 19,
            TestCasesStatusRate = 20,
            TestCasesStatusRateCumulative = 21,
            IncidentCountByStatus = 22,
			TestPreparationSummary = 24,
			CustomGraph = 25
		}

        /// <summary>
        /// The time interval for various graphs
        /// </summary>
        public enum ReportingIntervalEnum
        {
            Daily,
            Weekly
        }

        #endregion

    }
}
