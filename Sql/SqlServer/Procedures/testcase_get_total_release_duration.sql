-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves the total estimated testing duration for
--					a release/iteration. Does not include child iterations
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_GET_TOTAL_RELEASE_DURATION', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_GET_TOTAL_RELEASE_DURATION;
GO
CREATE PROCEDURE TESTCASE_GET_TOTAL_RELEASE_DURATION
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	SELECT SUM(TST.ESTIMATED_DURATION) AS ESTIMATED_DURATION
	FROM TST_TEST_CASE TST
	INNER JOIN TST_RELEASE_TEST_CASE RTC ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE TST.IS_DELETED = 0
	AND RTC.RELEASE_ID = @ReleaseId
END
GO
