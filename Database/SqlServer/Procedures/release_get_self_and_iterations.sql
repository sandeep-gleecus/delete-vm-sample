-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of iterations under a release
--					as well as the release itself
-- ================================================================
IF OBJECT_ID ( 'RELEASE_GET_SELF_AND_ITERATIONS', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_SELF_AND_ITERATIONS;
GO
CREATE PROCEDURE RELEASE_GET_SELF_AND_ITERATIONS
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeDeleted BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId

	IF (@IndentLevel IS NULL)
	BEGIN
		--Need to have the column returns, so just use a fake query
		SELECT RELEASE_ID FROM TST_RELEASE WHERE RELEASE_ID = -1
		RETURN
	END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of iterations and the parent release itself
	SELECT RELEASE_ID
	FROM TST_RELEASE
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
	AND (RELEASE_ID = @ReleaseId OR (RELEASE_TYPE_ID = 3 /* Iteration */ AND LEN(INDENT_LEVEL) = (@IndentLevelLength + 3)))
	AND (IS_DELETED = 0 OR @IncludeDeleted = 1)
	ORDER BY INDENT_LEVEL
END
GO
