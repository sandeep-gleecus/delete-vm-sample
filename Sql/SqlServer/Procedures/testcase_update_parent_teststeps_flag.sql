-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Changes the steps flag of all the test cases that link to a test case
-- =============================================
IF OBJECT_ID ( 'TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG;
GO
CREATE PROCEDURE TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG
	@ProjectId INT,
	@LinkedTestCaseId INT,	/* The test case that is linked to */
	@LinkedTestCaseDeleted BIT	/* Has this test case been deleted or not */
AS
BEGIN
	UPDATE TC
		SET TC.IS_TEST_STEPS = ~VW.IS_TEST_STEPS	
	FROM TST_TEST_CASE TC INNER JOIN 
	(
		SELECT
			STP.TEST_CASE_ID,
			SUM(CASE WHEN (STP.LINKED_TEST_CASE_ID <> @LinkedTestCaseId OR @LinkedTestCaseDeleted = 0 OR STP.LINKED_TEST_CASE_ID IS NULL) THEN 1 ELSE 0 END) AS STEP_COUNT,
			TST.IS_TEST_STEPS
		FROM TST_TEST_STEP STP INNER JOIN TST_TEST_CASE TST
		ON	STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE
			STP.IS_DELETED = 0 AND
			STP.LINKED_TEST_CASE_ID = @LinkedTestCaseId AND
			TST.PROJECT_ID = @ProjectId
		GROUP BY STP.TEST_CASE_ID, TST.IS_TEST_STEPS) VW
	ON VW.TEST_CASE_ID = TC.TEST_CASE_ID
	WHERE ((VW.IS_TEST_STEPS = 1 AND VW.STEP_COUNT = 0)
	OR	  (VW.IS_TEST_STEPS = 0 AND VW.STEP_COUNT > 0))
	AND	TC.PROJECT_ID = @ProjectId
END
GO
