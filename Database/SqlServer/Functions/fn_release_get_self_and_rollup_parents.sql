-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of parent releases that a release/iteration/phase
--					should roll up to as well as the item itself
-- Remarks:			Used in cases where you can't use a stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS;
END
GO
CREATE FUNCTION FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS
(
	@ProjectId INT,
	@ReleaseId INT
)
RETURNS @RollupReleases TABLE
(
	RELEASE_ID INT
)
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT
	DECLARE @MajorReleaseIndentLevel NVARCHAR(100)

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

	--Get the lowest-indent parent major release (including self)
	SELECT TOP(1) @MajorReleaseIndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(@IndentLevel, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) < @IndentLevelLength))
	AND RELEASE_TYPE_ID = 1 /* Major Release */
	ORDER BY INDENT_LEVEL DESC
			
	--Now get the list of all parents and the item itself,
	--Do not include any indent higher than the lowest major release
	INSERT INTO @RollupReleases
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(@IndentLevel, LEN(INDENT_LEVEL)) = INDENT_LEVEL
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) < @IndentLevelLength))
		AND (INDENT_LEVEL >= @MajorReleaseIndentLevel OR @MajorReleaseIndentLevel IS NULL)
		ORDER BY INDENT_LEVEL

	RETURN
END
GO
