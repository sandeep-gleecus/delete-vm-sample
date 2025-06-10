-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of items under a release
--					as well as the release itself
-- ================================================================
IF OBJECT_ID ( 'RELEASE_GET_SELF_AND_CHILDREN', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_SELF_AND_CHILDREN;
GO
CREATE PROCEDURE RELEASE_GET_SELF_AND_CHILDREN
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeMajorBranches BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT
	DECLARE @ChildMajorReleases TABLE
	(
		INDENT_LEVEL NVARCHAR(100)
	)

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId
    	IF (@IndentLevel IS NULL)
    	BEGIN
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	IF @IncludeMajorBranches = 1
	BEGIN
		--Now get the list of all children and the release itself
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
		ORDER BY INDENT_LEVEL
	END
	ELSE
	BEGIN
		--Get a list of all the child major releases (that we need to ignore)
		INSERT INTO @ChildMajorReleases
			SELECT INDENT_LEVEL
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
			AND LEN(INDENT_LEVEL) > @IndentLevelLength
			AND RELEASE_TYPE_ID = 1 /* Major Release */
				
		--Now get the list of all children and the release itself
		--ignoring those that are parents of the child major releases
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
		AND RELEASE_ID NOT IN (
			SELECT REL.RELEASE_ID FROM TST_RELEASE REL INNER JOIN @ChildMajorReleases MAJ
			ON dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(REL.INDENT_LEVEL, MAJ.INDENT_LEVEL, 100) = 1
			WHERE REL.PROJECT_ID = @ProjectId AND REL.RELEASE_ID <> @ReleaseId)
		ORDER BY INDENT_LEVEL	
	END
END
GO
