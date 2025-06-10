-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Runs
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_TEST_RUNS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_TEST_RUNS;
GO
CREATE PROCEDURE PROJECT_DELETE_TEST_RUNS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --We need to delete all the test runs associated with the project
    --including all test run steps and user navigation data as well as any pending entries
    DELETE FROM TST_TEST_RUN_STEP WHERE TEST_RUN_ID IN (SELECT TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId))
    DELETE FROM TST_TEST_RUN WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_RUNS_PENDING WHERE PROJECT_ID = @ProjectId
END
GO
