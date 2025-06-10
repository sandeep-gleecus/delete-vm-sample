-- =================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Deletes a Test Case in a Test Set
-- ==================================================
IF OBJECT_ID ( 'TESTSET_DELETE_TESTCASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTSET_DELETE_TESTCASE;
GO
CREATE PROCEDURE TESTSET_DELETE_TESTCASE
	@TestSetTestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;

	--Remove the link to the test set from related entities
	UPDATE TST_TEST_RUN SET TEST_SET_TEST_CASE_ID = NULL WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;

	--Delete the varies entries
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;
    DELETE FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;
END
GO
