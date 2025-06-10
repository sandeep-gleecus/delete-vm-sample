-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of filtered releases with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_RETRIEVE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_RETRIEVE;
GO
CREATE PROCEDURE RELEASE_RETRIEVE
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@IncDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_RELEASE_LIST_INTERNAL REL LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID, IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE (RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) ' +
					'AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' + ISNULL(@Filters,'')
	IF @IncDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
DECLARE @FOLDER CHAR(1)
DECLARE @EXPANDED CHAR(1)
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
)
DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT RELEASE_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL

OPEN PagingCursor
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

SELECT REL.*,ISNULL(RLU.IS_EXPANDED, CASE REL.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RLU.IS_VISIBLE, 1) AS IS_VISIBLE FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REL.RELEASE_ID = tblPK.PK ' + @WHERE + '
ORDER BY REL.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
