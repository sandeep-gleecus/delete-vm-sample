-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves a list of test-case execution status summary for a project / release
-- =====================================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT]
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	IF @ReleaseId IS NULL
	BEGIN
		--Create select command for retrieving the summary data - use outer join to ensure that we always
		--Return all execution status codes, so that the graph colors don't get mixed up!!
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID, MIN(EXE.NAME) AS EXECUTION_STATUS_NAME, COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE LEFT JOIN (SELECT TEST_CASE_ID, EXECUTION_STATUS_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0) AS TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		WHERE	EXE.EXECUTION_STATUS_ID <> 4 /* N/A */
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXECUTION_STATUS_ID

	END
	ELSE
	BEGIN
		--Populate list of child iterations if we have a release specified
		--if we have @ReleaseId = -2 it means only load in active releases
		IF @ReleaseId = -2
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM TST_RELEASE WHERE PROJECT_ID = @ProjectId AND RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) AND IS_DELETED = 0
		END
		ELSE
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
		END
		
		--Create select command for retrieving the summary data - use outer join to ensure that we always
        --Return all execution status codes, so that the graph colors don't get mixed up!!
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID,
				MIN(EXE.NAME) AS EXECUTION_STATUS_NAME,
				COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE
		LEFT JOIN (SELECT RTC.TEST_CASE_ID, RTC.EXECUTION_STATUS_ID
					FROM TST_RELEASE_TEST_CASE RTC
					INNER JOIN TST_TEST_CASE TST ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
					AND RTC.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
					WHERE TST.PROJECT_ID = @ProjectId
					AND TST.IS_DELETED = 0) AS TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		WHERE	EXE.EXECUTION_STATUS_ID <> 4 /* N/A */
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXECUTION_STATUS_ID
	END
END
GO
