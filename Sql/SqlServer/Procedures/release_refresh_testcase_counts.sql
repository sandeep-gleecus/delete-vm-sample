-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the release-test-case counts
-- =======================================================
IF OBJECT_ID ( 'RELEASE_REFRESH_TESTCASE_COUNTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [RELEASE_REFRESH_TESTCASE_COUNTS];
GO
CREATE PROCEDURE [RELEASE_REFRESH_TESTCASE_COUNTS]
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN		
	SET NOCOUNT ON;

	--Now we need to update the execution info for the release/test case information	
	--Create the CTE to get the most recent test run per release
	--Use the CROSS APPLY to handle rollup release considerations
    UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE RELEASE_ID = @ReleaseId;
	
	WITH CTE AS
	(
		SELECT T1.TEST_RUN_ID, T2.RELEASE_ID, T1.END_DATE, T1.EXECUTION_STATUS_ID, T1.ACTUAL_DURATION, ROW_NUMBER() OVER
		(
			PARTITION BY T2.RELEASE_ID
			ORDER BY END_DATE DESC
		) AS TRN
		FROM TST_TEST_RUN T1
		CROSS APPLY dbo.FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS(@ProjectId, T1.RELEASE_ID) AS T2
		WHERE T1.RELEASE_ID = @ReleaseId AND T1.EXECUTION_STATUS_ID <> 3 /* Not Run */	
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
