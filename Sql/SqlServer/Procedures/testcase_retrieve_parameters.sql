-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves the list of parameters for a test case
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_PARAMETERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_RETRIEVE_PARAMETERS;
GO
CREATE PROCEDURE TESTCASE_RETRIEVE_PARAMETERS
	@TestCaseId INT,
	@IncludeInherited BIT,
	@IncludeAlreadySet BIT
AS
DECLARE
	@ProjectId INT
BEGIN
	--First get the project ID of the test case
	SELECT @ProjectId = PROJECT_ID FROM TST_TEST_CASE WHERE TEST_CASE_ID = @TestCaseId;

	--Do we want to query multiple-levels of linking
	IF @IncludeInherited = 1
	BEGIN
		--Do we want to include the values set already at lower-levels
		IF @IncludeAlreadySet = 1
		BEGIN
			SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
			FROM TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET
			WHERE	TEST_CASE_ID = @TestCaseId AND PROJECT_ID = @ProjectId
			ORDER BY NAME, TEST_CASE_PARAMETER_ID	
		END
		ELSE
		BEGIN
			SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
			FROM TST_TEST_CASE_PARAMETER_HIERARCHY
			WHERE	TEST_CASE_ID = @TestCaseId AND PROJECT_ID = @ProjectId
			ORDER BY NAME, TEST_CASE_PARAMETER_ID			
		END
	END
	ELSE
	BEGIN
        SELECT	TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
        FROM	TST_TEST_CASE_PARAMETER
        WHERE	TEST_CASE_ID = @TestCaseId
        ORDER BY NAME, TEST_CASE_PARAMETER_ID
	END
END
GO
