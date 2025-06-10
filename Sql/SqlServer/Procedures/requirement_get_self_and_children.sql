-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of requirements under a summary
--					requirement as well as the item itself
-- ================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_SELF_AND_CHILDREN', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_SELF_AND_CHILDREN;
GO
CREATE PROCEDURE REQUIREMENT_GET_SELF_AND_CHILDREN
	@ProjectId INT,
	@RequirementId INT,
	@IncludeDeleted BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_REQUIREMENT
	WHERE REQUIREMENT_ID = @RequirementId

	IF (@IndentLevel IS NULL)
	BEGIN
		SELECT NULL AS REQUIREMENT_ID
		RETURN
	END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of test cases and the folder itself
	SELECT REQUIREMENT_ID
	FROM TST_REQUIREMENT
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
	AND (REQUIREMENT_ID = @RequirementId OR LEN(INDENT_LEVEL) >= (@IndentLevelLength + 3))
	AND (IS_DELETED = 0 OR @IncludeDeleted = 1)
	ORDER BY INDENT_LEVEL
END
GO
