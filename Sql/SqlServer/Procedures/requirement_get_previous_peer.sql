-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Returns the previous peer for the given indent level
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_PREVIOUS_PEER', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_PREVIOUS_PEER;
GO
CREATE PROCEDURE REQUIREMENT_GET_PREVIOUS_PEER
	@ProjectId INT,
	@IndentLevel NVARCHAR(100),
	@IncludeDeleted BIT
AS
BEGIN
	--Find the first requirement that is just before the current one
	SELECT TOP 1 INDENT_LEVEL
	FROM TST_REQUIREMENT
	WHERE LEN(INDENT_LEVEL) = LEN(@IndentLevel)
		AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
		AND PROJECT_ID = @ProjectId
		AND INDENT_LEVEL < @IndentLevel
	ORDER BY INDENT_LEVEL DESC
END
GO
