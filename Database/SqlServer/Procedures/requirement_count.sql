-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the count of requirements for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_COUNT
	@UserId INT,
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
	IF @UserId IS NULL OR @UserId < 1
	BEGIN
		SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ 
WHERE	REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND    (REQ.IS_SUMMARY = 1 OR (1=1 ' + @Filters + ')) ' + @DeletedClause
	END
	ELSE
	BEGIN
		SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ LEFT JOIN (SELECT * FROM TST_REQUIREMENT_USER WHERE USER_ID = ' + CAST(@UserId AS NVARCHAR) + ') AS RQU
ON		REQ.REQUIREMENT_ID = RQU.REQUIREMENT_ID
WHERE	(RQU.IS_VISIBLE = 1 OR RQU.IS_VISIBLE IS NULL)
AND    REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND    (REQ.IS_SUMMARY = 1 OR (1=1 ' + @Filters + ')) ' + @DeletedClause
	END
	EXEC (@SQL)
END
GO
