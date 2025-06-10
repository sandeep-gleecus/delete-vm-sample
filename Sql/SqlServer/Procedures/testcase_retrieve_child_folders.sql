-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Gets the list of all children of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_CHILD_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_CHILD_FOLDERS];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_CHILD_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(255)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_CASE_FOLDER_HIERARCHY WHERE TEST_CASE_FOLDER_ID = @FolderId;

	--Now get the child folders
	SELECT TEST_CASE_FOLDER_ID AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_CASE_FOLDER_HIERARCHY
	WHERE SUBSTRING(INDENT_LEVEL, 1, LEN(@IndentLevel)) = @IndentLevel
	AND (LEN(INDENT_LEVEL) > LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL, TEST_CASE_FOLDER_ID
END
GO
