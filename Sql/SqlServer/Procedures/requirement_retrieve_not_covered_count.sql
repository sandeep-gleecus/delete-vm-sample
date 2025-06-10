-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Get the list of requirements not mapped to test cases that are part of the release
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@RequirementStatusesToIgnore NVARCHAR(MAX)
AS
	DECLARE @RequirementStatusesToIgnoreTable TABLE (STATUS_ID INT)
BEGIN
	--The requirements cannot be in one of the following statuses
	INSERT INTO @RequirementStatusesToIgnoreTable (STATUS_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementStatusesToIgnore, ',')
		
	--Create select command for retrieving all the requirements in the project together with their mapped test cases
	SELECT CAST (COUNT(REQUIREMENT_ID) AS FLOAT(53)) AS REQUIREMENT_COUNT
	FROM TST_REQUIREMENT
	WHERE PROJECT_ID = @ProjectId
	AND REQUIREMENT_ID NOT IN
		(SELECT REQUIREMENT_ID FROM TST_REQUIREMENT_TEST_CASE AS RQT INNER JOIN TST_RELEASE_TEST_CASE AS RLT
		ON RQT.TEST_CASE_ID = RLT.TEST_CASE_ID WHERE RLT.RELEASE_ID = @ReleaseId)
		AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND IS_DELETED = 0
END
GO
