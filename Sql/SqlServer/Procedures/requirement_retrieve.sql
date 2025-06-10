-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of filtered requirements with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@OnlyExpanded BIT,
	@IncludeDeleted BIT
AS
	DECLARE @ExpandedClause NVARCHAR(MAX)
	DECLARE @NormalizingClause NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_REQUIREMENT_LIST_INTERNAL REQ LEFT JOIN (SELECT REQUIREMENT_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RQU ON REQ.REQUIREMENT_ID = RQU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE (RQU.IS_VISIBLE = 1 OR RQU.IS_VISIBLE IS NULL) ' +
					'AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' +
					'AND (REQ.IS_SUMMARY = 1 OR (1=1 ' + ISNULL(@Filters,'') + '))'
	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND REQ.IS_DELETED = 0'
	END

	--Create the appropriate clause for removing unnecessary folders depending on whether
	--we need to remove all non-matching folders or just ones that are expanded
	IF @OnlyExpanded = 1
	BEGIN
		SET @ExpandedClause = 'AND @PREVINDENT <> '''' AND (LEFT(@PREVINDENT,LEN(@INDENT)) = @INDENT OR @EXPANDED = 0)'
	END
	IF @OnlyExpanded = 0
	BEGIN
		SET @ExpandedClause = 'AND @PREVINDENT <> '''' AND LEFT(@PREVINDENT,LEN(@INDENT)) = @INDENT'
	END

	--Only remove filters if we have a filter set (since expensive operation)
	IF @Filters = '' OR @Filters IS NULL
	BEGIN
		SET @NormalizingClause =
'DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL'
	END
	ELSE
	BEGIN
		SET @NormalizingClause =
'DECLARE NormalizingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID, IS_SUMMARY, INDENT_LEVEL, RQU.IS_EXPANDED
FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL

OPEN NormalizingCursor
FETCH LAST FROM NormalizingCursor INTO @PK, @SUMMARY, @INDENT, @EXPANDED

SET @PREVINDENT = ''''
SET NOCOUNT ON
WHILE @@FETCH_STATUS = 0
BEGIN
	IF @SUMMARY = 0
	BEGIN
		INSERT @tblNormalized (PK,INDENT) VALUES (@PK,@INDENT)
		SET @PREVINDENT = @INDENT
	END
	IF @SUMMARY = 1 ' + @ExpandedClause + '
	BEGIN
		INSERT @tblNormalized (PK,INDENT)  VALUES (@PK,@INDENT)
	END
	FETCH PRIOR FROM NormalizingCursor INTO @PK, @SUMMARY, @INDENT, @EXPANDED
END

CLOSE       NormalizingCursor
DEALLOCATE  NormalizingCursor

DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT PK FROM @tblNormalized ORDER BY INDENT'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
DECLARE @SUMMARY BIT
DECLARE @EXPANDED BIT
DECLARE @INDENT NVARCHAR(100)
DECLARE @PREVINDENT  NVARCHAR(100)
SET @PageSize = ' + CAST(@NumRows AS NVARCHAR) + '

DECLARE @PK INT
DECLARE @tblNormalized TABLE
(
	PK INT NOT NULL PRIMARY KEY,
	INDENT NVARCHAR(100) NOT NULL
)
DECLARE @tblPK TABLE
(
	PK INT NOT NULL PRIMARY KEY
) ' + @NormalizingClause + ' ' +

'OPEN PagingCursor
FETCH RELATIVE ' + CAST(@StartRow AS NVARCHAR) + ' FROM PagingCursor INTO @PK

SET NOCOUNT ON
WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
	INSERT @tblPK (PK)  VALUES (@PK)
	FETCH NEXT FROM PagingCursor INTO @PK
	SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT ISNULL(RQU.IS_EXPANDED, CASE REQ.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RQU.IS_VISIBLE, 1) AS IS_VISIBLE, REQ.* FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REQ.REQUIREMENT_ID = tblPK.PK ' + @WHERE + '
ORDER BY REQ.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
