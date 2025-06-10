-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of releases for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_RETRIEVE_CUSTOM', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_RETRIEVE_CUSTOM;
GO
CREATE PROCEDURE RELEASE_RETRIEVE_CUSTOM
	@UserId INT,
	@ProjectId INT,
	@FilterSort NVARCHAR(MAX),
	@NumRows INT,
	@IncludeDeleted BIT,
	@OnlyShowVisible BIT
AS
	DECLARE @TOP NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE_ORDER_BY NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_RELEASE_LIST_INTERNAL REL LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	IF @OnlyShowVisible = 1
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE (RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) '
	END
	ELSE
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE 1=1 '
	END

	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND REL.IS_DELETED = 0 '
	END
	
	IF @ProjectId IS NOT NULL
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' '
	END
	
	IF @FilterSort IS NOT NULL AND LEN(@FilterSort) > 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND ' + @FilterSort + ' '
	END
	
	--See if we have to limit the number of rows retrieved
	SET @TOP = ''
	IF @NumRows IS NOT NULL
	BEGIN
		SET @TOP = 'TOP ' + CAST(@NumRows AS NVARCHAR)
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
SELECT ' + @TOP + ' ISNULL(RLU.IS_EXPANDED, CASE REL.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RLU.IS_VISIBLE, 1) AS IS_VISIBLE, REL.*
FROM ' + @TABLES + ' ' + @WHERE_ORDER_BY
	EXEC (@SQL)
END
GO
