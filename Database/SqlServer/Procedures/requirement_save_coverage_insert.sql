-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Inserts test coverage
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_SAVE_COVERAGE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_SAVE_COVERAGE_INSERT;
GO
CREATE PROCEDURE REQUIREMENT_SAVE_COVERAGE_INSERT
	@RequirementId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO TST_REQUIREMENT_TEST_CASE (REQUIREMENT_ID, TEST_CASE_ID)
	VALUES (@RequirementId, @TestCaseId);
END
GO
