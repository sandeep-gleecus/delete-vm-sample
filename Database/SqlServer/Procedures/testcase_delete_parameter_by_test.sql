-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes all the Test Case Parameters
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE_PARAMETER_BY_TEST', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE_PARAMETER_BY_TEST;
GO
CREATE PROCEDURE TESTCASE_DELETE_PARAMETER_BY_TEST
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the test configuration parameter values
	DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);
		
	--Delete the test set configuration parameters
	DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);
	
	--Delete the test step parameters
    DELETE FROM TST_TEST_STEP_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);

	--Delete the test set test case parameters
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);

	--Delete the test case parameters
    DELETE FROM TST_TEST_CASE_PARAMETER
	WHERE TEST_CASE_ID = @TestCaseId;
END
GO
