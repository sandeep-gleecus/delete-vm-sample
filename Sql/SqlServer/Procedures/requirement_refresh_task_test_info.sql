-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Refreshes the summary task progress and test coverage for a particular
--					requirement in the requirements tree as well as its status. Also rolls up
--					the data to all the parents of the requirement as well.
--                  Updated and improved 1/3/2020 By SWB
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_REFRESH_TASK_TEST_INFO', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_REFRESH_TASK_TEST_INFO;
GO
CREATE PROCEDURE REQUIREMENT_REFRESH_TASK_TEST_INFO
	@ProjectId INT,
	@RequirementId INT /* Not Actually Used */,
	@IndentLevel NVARCHAR(100),
	@ChangeStatusFromTasks BIT,
	@ChangeStatusFromTestCases BIT
AS
BEGIN
	DECLARE
		@Length INT,
		@cRequirementId INT,
		@cIsSummary BIT,
		@cIndentLevel NVARCHAR(100),
		@cExistingStatusId INT,
		@ReqPointEffort INT,
		
		@TestCaseCount_Total INT,
		@TestCaseCount_Passed INT,
		@TestCaseCount_Failed INT,
		@TestCaseCount_Blocked INT,
		@TestCaseCount_Caution INT,
		
		@Task_Count INT,
		@Task_EstimatedEffort INT,
		@Task_ActualEffort INT,
		@Task_RemainingEffort INT,
		@Task_ProjectedEffort INT,
		
		@Task_PercentOnTime INT,
		@Task_PercentLateFinish INT,
		@Task_PercentNotStart1 INT,
		@Task_PercentNotStart2 INT,
		@Task_PercentLateStart INT,
		
		@Requirement_StatusId INT,
		@Requirement_EstimatedEffort INT,
		@Requirement_EstimatePoints DECIMAL(9,1),
		@Requirement_ChildCount INT,
		@ReqCount INT,
		@TaskCount INT
	
	--This temp table is being used later to cache list of child requirements	
	DECLARE @tmpChildRequirements AS TABLE 
	(
		REQUIREMENT_ID INT
	);
		
	--Store the length of the requirement's indent level
	SET @Length = LEN(@IndentLevel)
	
	--Get the current story/effort metric for this project
	SELECT @ReqPointEffort = REQ_POINT_EFFORT FROM TST_PROJECT WHERE PROJECT_ID = @ProjectId

	/*
	--Declare a temp table with an iterator for the list of requirements that are parents of the passed-in one or are the passed-in one
	--itself. We need to then update the test case and task status for each of them in turn
	--Loop through each row of the temp table using the iterator count 
	*/

	DECLARE @MaxIterator INT
    DECLARE @Iterator INT 

	CREATE TABLE #MY_REQUIREMENTS (Iterator INT IDENTITY(1, 1), REQUIREMENT_ID INT, IS_SUMMARY BIT, INDENT_LEVEL NVARCHAR(100), REQUIREMENT_STATUS_ID INT);

	INSERT INTO #MY_REQUIREMENTS
	SELECT  REQUIREMENT_ID, IS_SUMMARY, INDENT_LEVEL, REQUIREMENT_STATUS_ID	    
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
				AND ((LEN(INDENT_LEVEL) < LEN(@IndentLevel) AND IS_SUMMARY = 1) OR INDENT_LEVEL = @IndentLevel)
		ORDER BY INDENT_LEVEL DESC

SELECT @MaxIterator = MAX(Iterator), @Iterator = 1
FROM   #MY_REQUIREMENTS;

   WHILE @Iterator <= @MaxIterator 
   BEGIN 

		--Logging for timing
		--SELECT 'Start Iteration #' + CAST (@Iterator AS NVARCHAR(MAX)) +', DateTime=' +  CONVERT(NVARCHAR, GETDATE(), 114) AS 'DEBUG';
 
      --Need to set these @cRequirementId, @cIsSummary, @cIndentLevel, @cExistingStatusId

      SELECT @cRequirementId = REQUIREMENT_ID FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cIsSummary = IS_SUMMARY FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cIndentLevel = INDENT_LEVEL FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cExistingStatusId = REQUIREMENT_STATUS_ID FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

		--Loop through all the requirements
		
		--Get the list of child requirements from function
		DELETE FROM @tmpChildRequirements;
		INSERT INTO @tmpChildRequirements
		SELECT REQUIREMENT_ID FROM FN_REQUIREMENT_GET_SELF_AND_CHILDREN(@ProjectId,@cRequirementId);
		
		--Get the count of test cases that are linked to this requirement or its children
		SELECT @TestCaseCount_Total = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
										INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
										WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
										AND TST.IS_DELETED = 0
		IF @TestCaseCount_Total > 0
		BEGIN
			SELECT @TestCaseCount_Passed = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 2
			SELECT @TestCaseCount_Failed = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 1
			SELECT @TestCaseCount_Caution = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 6
			SELECT @TestCaseCount_Blocked = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 5
		END
		ELSE
		BEGIN
			SET @TestCaseCount_Passed = 0;
			SET @TestCaseCount_Failed = 0;
			SET @TestCaseCount_Caution = 0;
			SET @TestCaseCount_Blocked = 0;
		END;
			
		--Now we need to get a count of the tasks and total of the task effort for
		--the requirement and its children
		SELECT @Task_Count = COUNT(TASK_ID) FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
		--If we have at least one task, need to update the Progress indicator percentages and effort values
		IF @Task_Count > 0
		BEGIN
			SELECT @Task_EstimatedEffort = SUM(ISNULL(ESTIMATED_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_ActualEffort = SUM(ISNULL(ACTUAL_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_RemainingEffort = SUM(ISNULL(REMAINING_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_ProjectedEffort = SUM(ISNULL(PROJECTED_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
										
			SELECT @Task_PercentLateFinish = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID <> 5 /*Deferred*/
								AND COMPLETION_PERCENT < 100 AND (COMPLETION_PERCENT > 0 OR TASK_STATUS_ID = 2) AND END_DATE < GETUTCDATE()
			SELECT @Task_PercentNotStart1 = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT = 0 AND (START_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 5 /*Deferred*/) AND TASK_STATUS_ID IN (1,4,5)
			SELECT @Task_PercentLateStart = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT = 0 AND START_DATE < GETUTCDATE() AND TASK_STATUS_ID IN (1,4)		
			SELECT @Task_PercentOnTime = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT > 0 AND (END_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 3 /*Completed*/)		
			SET @Task_PercentNotStart2 = 100 - (ISNULL(@Task_PercentLateFinish,0) + ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentOnTime,0) + ISNULL(@Task_PercentLateStart,0))
		END
		ELSE
		BEGIN
			SET @Task_EstimatedEffort = NULL;
			SET @Task_ActualEffort = NULL;
			SET @Task_RemainingEffort = NULL;
			SET @Task_ProjectedEffort = NULL;

			SET @Task_PercentOnTime = 0
			SET @Task_PercentLateFinish = 0
			SET @Task_PercentNotStart1 = 0
			SET @Task_PercentNotStart2 = 0
			SET @Task_PercentLateStart = 0
		END
		
		--Update the requirement
		UPDATE TST_REQUIREMENT
			SET COVERAGE_COUNT_TOTAL = @TestCaseCount_Total,
			COVERAGE_COUNT_PASSED = @TestCaseCount_Passed,
			COVERAGE_COUNT_FAILED = @TestCaseCount_Failed,
			COVERAGE_COUNT_CAUTION = @TestCaseCount_Caution,
			COVERAGE_COUNT_BLOCKED = @TestCaseCount_Blocked,
			
			TASK_COUNT = @Task_Count,
			TASK_ESTIMATED_EFFORT = @Task_EstimatedEffort,
			TASK_REMAINING_EFFORT = @Task_RemainingEffort,
			TASK_ACTUAL_EFFORT = @Task_ActualEffort,
			TASK_PROJECTED_EFFORT = @Task_ProjectedEffort,
			
			TASK_PERCENT_ON_TIME = ISNULL(@Task_PercentOnTime,0),
			TASK_PERCENT_LATE_FINISH = ISNULL(@Task_PercentLateFinish,0),
			TASK_PERCENT_NOT_START = ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentNotStart2,0),
			TASK_PERCENT_LATE_START = ISNULL(@Task_PercentLateStart,0),
			
			ESTIMATED_EFFORT = (ESTIMATE_POINTS * @ReqPointEffort)
		WHERE REQUIREMENT_ID = @cRequirementId
		
		--If we have a summary, need to also update the Estimated Effort/Points from the child requirements
		IF @cIsSummary = 1
		BEGIN
			SELECT
				@Requirement_EstimatedEffort = SUM(ISNULL(ESTIMATED_EFFORT, 0)),
				@Requirement_EstimatePoints = SUM(ISNULL(ESTIMATE_POINTS, 0))
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
				AND IS_SUMMARY = 0
				AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel
				AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
			--Length check the estimate
			IF @Requirement_EstimatePoints > 99999999.9
			BEGIN
				SET @Requirement_EstimatePoints = 99999999.9
			END
			--Update the requirement
			UPDATE TST_REQUIREMENT
				SET ESTIMATED_EFFORT = @Requirement_EstimatedEffort,
				ESTIMATE_POINTS = @Requirement_EstimatePoints
			WHERE REQUIREMENT_ID = @cRequirementId
		END
		
		--Get the total count of child requirements
		SELECT @Requirement_ChildCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
				AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)

		--If we have any tasks or are a summary, need to update the status id
		--Now lets iterate through all the peers and calculate the rollup scope level
		--Generally the most advanced status (i.e. furthest in the workflow) will be reflected
		--in the rollup. However the one exception is that if we have one requirement in completed
		--status and the others in any other status then we always rollup to 'In Progress'
		--Also rejected and obsolete statuses don't rollup, so the parent will just show completed
		--unless all items are rejected/obsolete
		IF ((@cIsSummary = 1 AND @Requirement_ChildCount > 0) OR (@Task_Count > 0 AND @ChangeStatusFromTasks = 1) OR (@TestCaseCount_Total > 0 AND @ChangeStatusFromTestCases = 1))
		BEGIN
			SET @Requirement_StatusId = @cExistingStatusId
						
			--First we roll-up the status of summary requirements only
			IF @cIsSummary = 1 AND @Requirement_ChildCount > 0
			BEGIN
				SET @Requirement_StatusId = 1 /*Requested*/				
			
				--SELECT 'BEGIN:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
				--See if any child requirements = Evaluated
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 7
				IF @Requirement_StatusId IN (1) AND @ReqCount > 0 SET @Requirement_StatusId = 7

				--See if any child requirements = Accepted
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 5
				IF @Requirement_StatusId IN (1,7) AND @ReqCount > 0 SET @Requirement_StatusId = 5

				--See if any child requirements = Planned
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 2
				IF @Requirement_StatusId IN (1,7,5) AND @ReqCount > 0 SET @Requirement_StatusId = 2
				
				--See if any child requirements = In Progress
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
					AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
					AND REQUIREMENT_STATUS_ID = 3
				IF @Requirement_StatusId IN (1,7,5,2) AND @ReqCount > 0 SET @Requirement_StatusId = 3
				
				--See if any child requirements are Developed,Tested,Completed and we're still listed as requested/planned
				--If so, switch to in-progress
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (4,9,10)
				IF @Requirement_StatusId IN (1,2) AND @ReqCount > 0 SET @Requirement_StatusId = 3
				
				--If all child requirements are Developed/Tested/Completed (we can ignore obsolete/rejected) set to Developed
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (4,6,8,9,10)
				IF @ReqCount = @Requirement_ChildCount SET @Requirement_StatusId = 4
				
				--If all child requirements are Tested/Completed (we can ignore obsolete/rejected) set to Tested
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
					AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
					AND REQUIREMENT_STATUS_ID IN (9,10,6,8)
				IF (@ReqCount = @Requirement_ChildCount AND @TaskCount = @Task_Count) SET @Requirement_StatusId = 9

				--If all child requirements are Completed (we can ignore obsolete/rejected) set to Completed
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (10,6,8)	
				IF @ReqCount = @Requirement_ChildCount  SET @Requirement_StatusId = 10
				
				--If all child requirements are Rejected set to Rejected
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 6
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0) SET @Requirement_StatusId = 6

				--If all child requirements are Obsolete set to Obsolete
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 8
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 8

				--If all child requirements are 'Ready for Review' set to 'Ready for Review'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 11
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 11

				--If all child requirements are 'Ready for Test' set to 'Ready for Test'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 12
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 12

				--If all child requirements are 'Released' set to 'Released'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 13
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 13

				--If all child requirements are 'Design in Process' set to 'Design in Process'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 14
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 14

				--If all child requirements are 'Design Approval' set to 'Design Approval'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 15
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 15

				--If all child requirements are 'Documented' set to 'Documented'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 16
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 16
				
				--SELECT 'END:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
			END
			
			--Next we consider the status of tasks
			IF @Task_Count > 0 AND @ChangeStatusFromTasks = 1
			BEGIN
				--If any tasks exist in the Not-Started/Blocked statuses, set requirement status to Planned
				--unless we're in the Developed/Tested/Completed status, in which case, switch back to 'In Progress'
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (1,4)			
				IF @Requirement_StatusId IN (1,7,5) AND @TaskCount > 0 SET @Requirement_StatusId = 2
				IF @Requirement_StatusId IN (4,9,10) AND @TaskCount > 0 SET @Requirement_StatusId = 3

				--If any tasks exist in the In-Progress status, set the requirement status to In-Progress
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (2)		
				IF @Requirement_StatusId IN (1,7,5,2,4,9,10) AND @TaskCount > 0 SET @Requirement_StatusId = 3
				
				--See if any tasks exist in the Completed status, yet the requirement status was previously 'Planned'
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID = 3	
				IF @Requirement_StatusId = 2 AND @TaskCount > 0 SET @Requirement_StatusId = 3
						
				--If all tasks exist in a Completed status (or deferred since that's ignored), set Requirement to Developed
				--unless already in Tested/Completed/Obsolete status. We don't do this for summary tasks because that might override
				--the situation where we have child requirements with no tasks that are in an 'earlier' status
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (3,5)
				IF @Requirement_StatusId NOT IN (8,9,10) AND @TaskCount = @Task_Count AND @cIsSummary = 0 SET @Requirement_StatusId = 4				
			END
			--SELECT 'AFTER-TASKS:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)

			--Finally we consider the status of test cases
			IF @TestCaseCount_Total > 0 AND @ChangeStatusFromTestCases = 1
			BEGIN
				--If the current status is 'Developed' and all tests are passed, switch to 'Tested'
				IF @Requirement_StatusId = 4 AND @TestCaseCount_Total = @TestCaseCount_Passed AND @TestCaseCount_Total > 0
					SET @Requirement_StatusId = 9
			END
			--SELECT 'AFTER-TESTS:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
								
			--Update the requirement
			--SELECT 'UPDATE:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
			UPDATE TST_REQUIREMENT
				SET REQUIREMENT_STATUS_ID = @Requirement_StatusId
			WHERE REQUIREMENT_ID = @cRequirementId

		END ----If we have any tasks or are a summary, need to update the status id
		      --Bump up to get next row from temp table to fill in @cRequirementId, @cIsSummary, @cIndentLevel, @cExistingStatusId

			--Logging for timing
			--SELECT 'End Iteration #' + CAST (@Iterator AS NVARCHAR(MAX)) +', DateTime=' +  CONVERT(NVARCHAR, GETDATE(), 114) AS 'DEBUG';
			SET @Iterator = @Iterator +1;
   END --While
		
END  
GO
