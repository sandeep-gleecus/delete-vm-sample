-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves a requirement and all its child requirements
-- Remarks:			Used in cases where you can't use a stored procedure.
--					Does not include deleted requirements
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_REQUIREMENT_GET_SELF_AND_CHILDREN' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_REQUIREMENT_GET_SELF_AND_CHILDREN;
END
GO
CREATE FUNCTION FN_REQUIREMENT_GET_SELF_AND_CHILDREN
(
	@ProjectId INT,
	@RequirementId INT
)
RETURNS @RequirementAndChildren TABLE
(
	REQUIREMENT_ID INT
)
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
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of children and the item itself and populate table
	INSERT INTO @RequirementAndChildren
		SELECT REQUIREMENT_ID
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (REQUIREMENT_ID = @RequirementId OR (LEN(INDENT_LEVEL) > (@IndentLevelLength)))
		AND IS_DELETED = 0
		ORDER BY INDENT_LEVEL
	RETURN
END
GO
