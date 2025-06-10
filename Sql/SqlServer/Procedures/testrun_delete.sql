-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Deletes by ID
-- =============================================
IF OBJECT_ID ( 'TESTRUN_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_DELETE];
GO
CREATE PROCEDURE [TESTRUN_DELETE]
	@TestRunId INT
AS
BEGIN
	SET NOCOUNT ON;
	-- Cascades delete the link from test run steps to incidents
		
	--Delete the test run and its steps
	DELETE FROM TST_TEST_RUN_STEP WHERE TEST_RUN_ID = @TestRunId;
	DELETE FROM TST_TEST_RUN WHERE TEST_RUN_ID = @TestRunId;
END
GO
