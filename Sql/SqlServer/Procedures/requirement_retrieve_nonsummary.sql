-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of filtered non-summary requirements with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_NONSUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_NONSUMMARY;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_NONSUMMARY
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@IncludeDeleted BIT
AS
	DECLARE @ExpandedClause NVARCHAR(MAX)
	DECLARE @NormalizingClause NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_REQUIREMENT_LIST REQ'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' +
					'AND IS_SUMMARY = 0' + @Filters
	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND REQ.IS_DELETED = 0'
	END

	SET @NormalizingClause =
'DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL'

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
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

SELECT CAST(0 AS BIT) AS IS_EXPANDED, CAST(1 AS BIT) AS IS_VISIBLE, REQ.* FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REQ.REQUIREMENT_ID = tblPK.PK ' + @WHERE + '
ORDER BY REQ.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
