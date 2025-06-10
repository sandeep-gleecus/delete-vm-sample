-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes a Test Step
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE_STEP', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE_STEP;
GO
CREATE PROCEDURE TESTCASE_DELETE_STEP
	@TestStepId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Unlink or delete the varies entries
	DELETE FROM TST_REQUIREMENT_TEST_STEP WHERE TEST_STEP_ID = @TestStepId;
    DELETE FROM TST_TEST_STEP WHERE TEST_STEP_ID = @TestStepId;
END
GO
