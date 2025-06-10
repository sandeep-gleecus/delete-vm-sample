-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_CASE_FOLDER_HIERARCHY WHERE TEST_CASE_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TEST_CASE_FOLDER_ID AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_CASE_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
