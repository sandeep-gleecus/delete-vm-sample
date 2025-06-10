-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Retrieves a list of the daily count of test-runs for a day in the specified timezone offset
-- =====================================================================================
IF OBJECT_ID ( 'TESTRUN_RETRIEVE_DAILY_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_RETRIEVE_DAILY_COUNT];
GO
CREATE PROCEDURE [TESTRUN_RETRIEVE_DAILY_COUNT]
	@ProjectId INT,
	@ReleaseId INT,
	@UtcOffsetHours INT,
	@UtcOffsetMinutes INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified
	IF @ReleaseId IS NOT NULL
	BEGIN
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	END
	
    --Create select command for retrieving the number of test runs per day
    --We need to reconstruct the dates to exclude the time component
    SELECT	TOP 5 TRN.EXECUTION_DATE, COUNT(TRN.TEST_RUN_ID) AS EXECUTION_COUNT
    FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute,@UtcOffsetMinutes,DATEADD(hour,@UtcOffsetHours,END_DATE)) AS FLOAT))AS DATETIME) AS EXECUTION_DATE,
			TEST_RUN_ID, TEST_CASE_ID, RELEASE_ID
			FROM TST_TEST_RUN
			WHERE EXECUTION_STATUS_ID <> 3 /* NotRun */) AS TRN INNER JOIN TST_TEST_CASE TST
    ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID
    WHERE
		TST.PROJECT_ID = @ProjectId AND
		TST.IS_DELETED = 0 AND
		(@ReleaseId IS NULL OR RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
    GROUP BY TRN.EXECUTION_DATE
    ORDER BY TRN.EXECUTION_DATE DESC
END
GO
