-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the count of releases for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_COUNT;
GO
CREATE PROCEDURE RELEASE_COUNT
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
		SET @DeletedClause = 'AND REL.IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	IF @UserId IS NULL OR @UserId < 1
	BEGIN
		SET @SQL = '
SELECT	COUNT(REL.RELEASE_ID) AS ARTIFACT_COUNT
FROM	VW_RELEASE_LIST REL 
WHERE	REL.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
' + ISNULL(@Filters,'') + ' ' + @DeletedClause
	END
	ELSE
	BEGIN
		SET @SQL = '
SELECT	COUNT(REL.RELEASE_ID) AS ARTIFACT_COUNT
FROM	VW_RELEASE_LIST REL LEFT JOIN (SELECT * FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST(@UserId AS NVARCHAR) + ') AS RLU
ON		REL.RELEASE_ID = RLU.RELEASE_ID
WHERE	(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL)
AND    REL.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
' + ISNULL(@Filters,'') + ' ' + @DeletedClause
	END
	EXEC (@SQL)
END
GO
