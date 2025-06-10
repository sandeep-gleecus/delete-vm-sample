-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Inserts test coverage
-- =============================================
IF OBJECT_ID ( 'RELEASE_SAVE_TEST_COVERAGE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_SAVE_TEST_COVERAGE_INSERT;
GO
CREATE PROCEDURE RELEASE_SAVE_TEST_COVERAGE_INSERT
	@ReleaseId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO TST_RELEASE_TEST_CASE (RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID)
	VALUES (@ReleaseId, @TestCaseId, 3 /*Not Run*/ );
END
GO
