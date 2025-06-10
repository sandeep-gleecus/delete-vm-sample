-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes test coverage
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_SAVE_COVERAGE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_SAVE_COVERAGE_DELETE;
GO
CREATE PROCEDURE REQUIREMENT_SAVE_COVERAGE_DELETE
	@RequirementId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TST_REQUIREMENT_TEST_CASE
	WHERE TEST_CASE_ID = @TestCaseId AND REQUIREMENT_ID = @RequirementId;
END
GO
