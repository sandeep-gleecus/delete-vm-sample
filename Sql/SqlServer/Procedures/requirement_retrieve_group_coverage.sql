-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of total requirement coverage for a project group
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_GROUP_COVERAGE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_GROUP_COVERAGE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_GROUP_COVERAGE
	@ProjectGroupId INT,
	@RequirementStatusesToIgnore NVARCHAR(MAX),
	@ActiveReleasesOnly BIT,
	@IncludeDeleted BIT
AS
	DECLARE @RequirementStatusesToIgnoreTable TABLE (STATUS_ID INT)
BEGIN

	--The requirements cannot be in one of the following statuses
	INSERT INTO @RequirementStatusesToIgnoreTable (STATUS_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementStatusesToIgnore, ',')

	--Create select command for retrieving the total number of requirements per coverage status
	--(i.e. sum of coverage per requirement normalized by the count for that requirement
	SELECT	1 AS CoverageStatusOrder, 'Passed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_PASSED AS FLOAT(53)) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	2 AS CoverageStatusOrder, 'Failed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_FAILED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	3 AS CoverageStatusOrder, 'Blocked' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_BLOCKED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	4 AS CoverageStatusOrder, 'Caution' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_CAUTION AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	5 AS CoverageStatusOrder, 'Not Run' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE (CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) - CAST (COVERAGE_COUNT_PASSED AS FLOAT(53)) - CAST (COVERAGE_COUNT_CAUTION AS FLOAT(53)) - CAST (COVERAGE_COUNT_BLOCKED AS FLOAT(53)) - CAST (COVERAGE_COUNT_FAILED AS FLOAT(53))) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	6 AS CoverageStatusOrder, 'Not Covered' AS CoverageStatus, CAST (COUNT(REQUIREMENT_ID) - SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE 1 END) AS FLOAT(53)) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0);
END
GO
