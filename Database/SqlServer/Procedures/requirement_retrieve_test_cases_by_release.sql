-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of requirements and mapped test cases for a specific release
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN

	--Create select command for retrieving all the requirements in the project together with their mapped test cases
	SELECT RQT.REQUIREMENT_ID, RQT.TEST_CASE_ID
	FROM TST_REQUIREMENT_TEST_CASE AS RQT
		INNER JOIN TST_RELEASE_TEST_CASE AS RLT ON RQT.TEST_CASE_ID = RLT.TEST_CASE_ID
		INNER JOIN TST_TEST_CASE AS TSC ON RQT.TEST_CASE_ID = TSC.TEST_CASE_ID
		INNER JOIN TST_REQUIREMENT AS REQ ON RQT.REQUIREMENT_ID = REQ.REQUIREMENT_ID
	WHERE RLT.RELEASE_ID = @ReleaseId
		AND TSC.IS_DELETED = 0
		AND REQ.IS_DELETED = 0
		AND REQ.PROJECT_ID = @ProjectId
	ORDER BY RQT.REQUIREMENT_ID
END
GO
