-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
--                  Updated and improved 2/7/2020 By SWB
-- =====================================================================
IF OBJECT_ID ( 'TASK_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TASK_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TASK_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TASK_FOLDER_HIERARCHY WHERE TASK_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TASK_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
