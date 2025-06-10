-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of iterations under a release
--					as well as the release itself
-- Remarks:			Used in cases where you can't use a stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_RELEASE_GET_SELF_AND_ITERATIONS' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_RELEASE_GET_SELF_AND_ITERATIONS;
END
GO
CREATE FUNCTION FN_RELEASE_GET_SELF_AND_ITERATIONS
(
	@ProjectId INT,
	@ReleaseId INT
)
RETURNS @ReleaseAndIterations TABLE
(
	RELEASE_ID INT
)
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
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of iterations and the release itself and populate table
	INSERT INTO @ReleaseAndIterations
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (RELEASE_TYPE_ID = 3/*Iteration*/ AND LEN(INDENT_LEVEL) = (@IndentLevelLength + 3)))
		ORDER BY INDENT_LEVEL
	RETURN
END
GO
