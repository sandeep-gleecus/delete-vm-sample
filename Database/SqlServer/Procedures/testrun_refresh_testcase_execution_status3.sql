-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Refreshes the execution status and last-run date of the specified test cases
-- Remarks:			This version is used when we have a test case id
-- =======================================================
IF OBJECT_ID ( 'TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3];
GO
CREATE PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3]
	@ProjectId INT,
	@TestCaseId INT
AS
BEGIN		
	SET NOCOUNT ON;

    --First update the test case execution data from the most recent run
    UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE TEST_CASE_ID = @TestCaseId;
	
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		(SELECT TRN1.TEST_CASE_ID, TRN1.EXECUTION_STATUS_ID, TRN1.END_DATE, TRN1.ACTUAL_DURATION
		FROM TST_TEST_RUN TRN1
		INNER JOIN
			(SELECT TOP 1 TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId AND EXECUTION_STATUS_ID <> 3 /* Not Run */ ORDER BY END_DATE DESC, TEST_RUN_ID DESC) TRN2
		ON TRN1.TEST_RUN_ID = TRN2.TEST_RUN_ID) TRN
	ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID;
	
	--Next update the test step status from the most recent test run
	UPDATE TST_TEST_STEP
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID
	FROM
		TST_TEST_STEP TSP
	INNER JOIN
		(SELECT TRS.TEST_STEP_ID, TRS.EXECUTION_STATUS_ID
		FROM TST_TEST_RUN_STEP TRS
		INNER JOIN
			(SELECT TOP 1 TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId AND EXECUTION_STATUS_ID <> 3 /* Not Run */ ORDER BY END_DATE DESC, TEST_RUN_ID DESC) TRN
		ON TRN.TEST_RUN_ID = TRS.TEST_RUN_ID) TRS
	ON TSP.TEST_STEP_ID	= TRS.TEST_STEP_ID;
	
	--Now we need to update the execution info for the release/test case information	
	--Create the CTE to get the most recent test run per release
	--Use the CROSS APPLY to handle rollup release considerations
    UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE TEST_CASE_ID = @TestCaseId;
	
	WITH CTE AS
	(
		SELECT T1.TEST_RUN_ID, T2.RELEASE_ID, T1.END_DATE, T1.EXECUTION_STATUS_ID, T1.ACTUAL_DURATION, ROW_NUMBER() OVER
		(
			PARTITION BY T2.RELEASE_ID
			ORDER BY END_DATE DESC, TEST_RUN_ID DESC
		) AS TRN
		FROM TST_TEST_RUN T1
		CROSS APPLY dbo.FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS(@ProjectId, T1.RELEASE_ID) AS T2
		WHERE T1.TEST_CASE_ID = @TestCaseId AND T1.EXECUTION_STATUS_ID <> 3 /* Not Run */	
	)	
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN3.END_DATE,
		EXECUTION_STATUS_ID = TRN3.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN3.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		(SELECT TRN1.TEST_CASE_ID, TRN2.RELEASE_ID, TRN1.EXECUTION_STATUS_ID, TRN1.END_DATE, TRN1.ACTUAL_DURATION
		FROM TST_TEST_RUN TRN1
		INNER JOIN
			(		
				SELECT TEST_RUN_ID, RELEASE_ID, END_DATE, EXECUTION_STATUS_ID, ACTUAL_DURATION FROM CTE
				WHERE TRN = 1
			) TRN2
		ON TRN1.TEST_RUN_ID = TRN2.TEST_RUN_ID
		) TRN3
	ON TST.TEST_CASE_ID = TRN3.TEST_CASE_ID 
	AND TST.RELEASE_ID = TRN3.RELEASE_ID
END
GO
