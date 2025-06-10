-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes test coverage
-- =============================================
IF OBJECT_ID ( 'RELEASE_SAVE_TEST_COVERAGE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_SAVE_TEST_COVERAGE_DELETE;
GO
CREATE PROCEDURE RELEASE_SAVE_TEST_COVERAGE_DELETE
	@ReleaseId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TST_RELEASE_TEST_CASE
	WHERE TEST_CASE_ID = @TestCaseId AND RELEASE_ID = @ReleaseId;
END
GO
