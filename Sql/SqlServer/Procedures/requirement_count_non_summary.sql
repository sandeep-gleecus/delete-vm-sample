-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the count of non-summary requirements for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_COUNT_NON_SUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COUNT_NON_SUMMARY;
GO
CREATE PROCEDURE REQUIREMENT_COUNT_NON_SUMMARY
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@IncludeDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @DeletedClause NVARCHAR(MAX)
BEGIN
	SET @DeletedClause = ''
	IF @IncludeDeleted = 0
	BEGIN
		SET @DeletedClause = 'AND REQ.IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ 
WHERE	REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND		REQ.IS_SUMMARY = 0 ' + @Filters + ' ' + @DeletedClause

	EXEC (@SQL)
END
GO
