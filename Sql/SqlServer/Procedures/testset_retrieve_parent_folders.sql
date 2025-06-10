-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
--                  Updated and improved 2/7/2020 By SWB
-- =====================================================================
IF OBJECT_ID ( 'TESTSET_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TESTSET_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_SET_FOLDER_HIERARCHY WHERE TEST_SET_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TEST_SET_FOLDER_ID AS TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_SET_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
