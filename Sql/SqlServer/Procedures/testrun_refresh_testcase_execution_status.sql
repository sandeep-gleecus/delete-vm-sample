-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Refreshes the execution status and last-run date of the test cases linked to
--					the test runs being updated
-- Remarks:			This version is used when we have a test runs pending id
-- =======================================================
IF OBJECT_ID ( 'TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS];
GO
CREATE PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS]
	@ProjectId INT,
	@TestRunsPendingId INT
AS
BEGIN
	DECLARE @ReleaseId INT
	DECLARE  @ParentsAndSelf TABLE
	(
		RELEASE_ID INT
	)
		
	SET NOCOUNT ON;
	SET ANSI_WARNINGS OFF;

	--First get the release id of the release/iteration that we were executed against
	SELECT TOP (1) @ReleaseId = RELEASE_ID
	FROM TST_TEST_RUN
	WHERE TEST_RUNS_PENDING_ID = @TestRunsPendingId AND RELEASE_ID IS NOT NULL

	--Populate list of self and parent releases that we need to consider
	INSERT @ParentsAndSelf (RELEASE_ID)
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS (@ProjectId, @ReleaseId)

	--Next update the test step status from the test run steps	
	--We need to do before we do the test case itself because otherwise the execution date will have changed
	UPDATE TST_TEST_STEP
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID
	FROM
		TST_TEST_STEP TSP
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TSP.TEST_STEP_ID = TRS.TEST_STEP_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	INNER JOIN
		TST_TEST_CASE TST ON TRN.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId

    --Now we need to select the list of test runs and update the 'last run' information
    --for the underlying test cases (provided we have a more recent end-date that the last-execution-date).
    --For test runs that are marked as not-run, we don't modify the test case status
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID		
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId
	
    --Next need to take into account any linked test cases, since they will need to update the linked test case instead.
	--We no longer updated teh status of the linked test cases because they don't have test runs
	--and there are difficulties trying to update the execution status using an aggregation function
	--where the linked test cases have multiple steps with different execution statuses
	/*UPDATE TST_TEST_CASE
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID,
		EXECUTION_DATE = TRS.END_DATE,
		ACTUAL_DURATION = TRS.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TRS.TEST_CASE_ID = TST.TEST_CASE_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.END_DATE IS NOT NULL AND
		(TRS.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId*/

	--Now we need to update the execution info for the release/test case information		
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON
			TST.TEST_CASE_ID = TRN.TEST_CASE_ID	AND
			TST.RELEASE_ID IN (SELECT RELEASE_ID FROM @ParentsAndSelf)
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId

END
GO
