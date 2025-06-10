-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes a Test Case
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE;
GO
CREATE PROCEDURE TESTCASE_DELETE
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete user subscriptions.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 2 AND ARTIFACT_ID = @TestCaseId);
	--Now delete the test case itself
	DELETE FROM TST_TEST_CASE_SIGNATURE WHERE TEST_CASE_ID = @TestCaseId;
    DELETE FROM TST_TEST_CASE WHERE TEST_CASE_ID = @TestCaseId;
END
GO
