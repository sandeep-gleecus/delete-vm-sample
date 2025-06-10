-- =============================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the summary task progress and task estimated/actual effort for a particular
--					release/iteration
-- =============================================================================================================
IF OBJECT_ID ( 'RELEASE_REFRESH_PROGRESS_AND_EFFORT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REFRESH_PROGRESS_AND_EFFORT;
GO
CREATE PROCEDURE RELEASE_REFRESH_PROGRESS_AND_EFFORT
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeTaskEffort BIT,
	@IncludeIncidentEffort BIT,
	@IncludeTestCaseEffort BIT
AS
BEGIN
	DECLARE
		@Task_Count INT,
		@Task_PercentOnTime INT,
		@Task_PercentLateFinish INT,
		@Task_PercentNotStart1 INT,
		@Task_PercentNotStart2 INT,
		@Task_PercentLateStart INT,
		@Task_ProjectedEffort INT
		
	--This temp table is being used later to cache list of child releases	
	DECLARE @tmpChildReleases AS TABLE 
	(
		RELEASE_ID INT
	);
	
	--Get the list of child requirements from function
	DELETE FROM @tmpChildReleases;
	INSERT INTO @tmpChildReleases
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_CHILDREN(@ProjectId, @ReleaseId, 0);
		
	--First we need to update the effort values
	MERGE TST_RELEASE AS TARGET	
	USING (
		--We add on the requirements, tasks, incidents and test case efforts as appropriate
		
		SELECT	@ReleaseId AS RELEASE_ID,
				SUM(ISNULL(TASK_ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TASK_PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(TASK_REMAINING_EFFORT,ISNULL(TASK_ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(TASK_ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM
		(
		--Requirements
		--We exclude the summary items to avoid duplicated effort
		SELECT	SUM(ISNULL(REQ.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(REQ.ESTIMATED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(CASE WHEN REQ.REQUIREMENT_STATUS_ID IN
				(
					9, /*Tested*/
					10, /*Completed*/
					13 /*Released*/
				)
				THEN 0 ELSE ISNULL(REQ.ESTIMATED_EFFORT,0) END) AS TASK_REMAINING_EFFORT,
				0 AS TASK_ACTUAL_EFFORT
		FROM TST_REQUIREMENT REQ
		WHERE REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
		AND REQ.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND REQ.REQUIREMENT_STATUS_ID NOT IN (6 /* Rejected */, 8 /* Obsolete*/)
		AND REQ.PROJECT_ID = @ProjectId
		AND REQ.TASK_COUNT = 0
		UNION ALL
		--Tasks
		SELECT	SUM(ISNULL(TSK.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TSK.PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(TSK.REMAINING_EFFORT,ISNULL(TSK.ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(TSK.ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM TST_TASK TSK
		WHERE TSK.IS_DELETED = 0
		AND TSK.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND TSK.TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
		AND TSK.PROJECT_ID = @ProjectId
		AND @IncludeTaskEffort = 1
		UNION ALL
		--Incidents
		SELECT	SUM(ISNULL(INC.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(INC.PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(INC.REMAINING_EFFORT,ISNULL(INC.ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(INC.ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM TST_INCIDENT INC
		WHERE INC.IS_DELETED = 0
		AND INC.RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND INC.PROJECT_ID = @ProjectId
		AND @IncludeIncidentEffort = 1	
		UNION ALL
		--Test Cases
		SELECT	SUM(ISNULL(TST.ESTIMATED_DURATION,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TST.ESTIMATED_DURATION,0)) AS TASK_PROJECTED_EFFORT,
				SUM(CASE WHEN RTC.EXECUTION_STATUS_ID IN
				(
					2, /*Passed*/
					4 /*N/A*/
				)
				THEN 0 ELSE ISNULL(TST.ESTIMATED_DURATION,0) END) AS TASK_REMAINING_EFFORT,
				0 AS TASK_ACTUAL_EFFORT
		FROM TST_TEST_CASE TST
		INNER JOIN TST_RELEASE_TEST_CASE RTC ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE TST.IS_DELETED = 0
		AND RTC.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND TST.TEST_CASE_STATUS_ID NOT IN (3 /* Rejected */, 6 /* Obsolete*/)
		AND TST.PROJECT_ID = @ProjectId
		AND @IncludeTestCaseEffort = 1
		) AS TOTAL_EFFORT
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.TASK_ESTIMATED_EFFORT = SOURCE.TASK_ESTIMATED_EFFORT,
				TARGET.TASK_PROJECTED_EFFORT = SOURCE.TASK_PROJECTED_EFFORT,
				TARGET.TASK_REMAINING_EFFORT = SOURCE.TASK_REMAINING_EFFORT,
				TARGET.TASK_ACTUAL_EFFORT = SOURCE.TASK_ACTUAL_EFFORT,
				TARGET.AVAILABLE_EFFORT = TARGET.PLANNED_EFFORT - SOURCE.TASK_PROJECTED_EFFORT			
	WHEN NOT MATCHED BY SOURCE AND TARGET.RELEASE_ID = @ReleaseId THEN
			UPDATE
			SET	
				TARGET.TASK_ESTIMATED_EFFORT = NULL,
				TARGET.TASK_PROJECTED_EFFORT = NULL,
				TARGET.TASK_REMAINING_EFFORT = NULL,
				TARGET.TASK_ACTUAL_EFFORT = NULL,
				TARGET.AVAILABLE_EFFORT = TARGET.PLANNED_EFFORT;
				
		--Now we need to get a count of the tasks for the release and its children
		SELECT @Task_Count = COUNT(TASK_ID) FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
		SELECT @Task_ProjectedEffort = TASK_PROJECTED_EFFORT
		FROM TST_RELEASE
		WHERE RELEASE_ID = @ReleaseId;
		
		--If we have at least one task, need to update the Progress indicator percentages
		IF @Task_Count > 0
		BEGIN
			SELECT @Task_PercentLateFinish = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND TASK_STATUS_ID <> 5 /*Deferred*/
								AND COMPLETION_PERCENT < 100 AND (COMPLETION_PERCENT > 0 OR TASK_STATUS_ID = 2) AND END_DATE < GETUTCDATE()
			SELECT @Task_PercentNotStart1 = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT = 0 AND (START_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 5 /*Deferred*/) AND TASK_STATUS_ID IN (1,4,5)
			SELECT @Task_PercentLateStart = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT = 0 AND START_DATE < GETUTCDATE() AND TASK_STATUS_ID IN (1,4)		
			SELECT @Task_PercentOnTime = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT > 0 AND (END_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 3 /*Completed*/)		
			SET @Task_PercentNotStart2 = 100 - (ISNULL(@Task_PercentLateFinish,0) + ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentOnTime,0) + ISNULL(@Task_PercentLateStart,0))
		END
		ELSE
		BEGIN
			SET @Task_PercentOnTime = 0
			SET @Task_PercentLateFinish = 0
			SET @Task_PercentNotStart1 = 0
			SET @Task_PercentNotStart2 = 0
			SET @Task_PercentLateStart = 0
		END
		
		--Update the release
		UPDATE TST_RELEASE
			SET			
				TASK_COUNT = @Task_Count,
				TASK_PERCENT_ON_TIME = ISNULL(@Task_PercentOnTime,0),
				TASK_PERCENT_LATE_FINISH = ISNULL(@Task_PercentLateFinish,0),
				TASK_PERCENT_NOT_START = ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentNotStart2,0),
				TASK_PERCENT_LATE_START = ISNULL(@Task_PercentLateStart,0)			
		WHERE RELEASE_ID = @ReleaseId
END
GO
