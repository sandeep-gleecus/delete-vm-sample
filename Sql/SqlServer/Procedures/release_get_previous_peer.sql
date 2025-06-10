-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Returns the previous peer for the given indent level
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_PREVIOUS_PEER', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_PREVIOUS_PEER;
GO
CREATE PROCEDURE RELEASE_GET_PREVIOUS_PEER
	@ProjectId INT,
	@IndentLevel NVARCHAR(100),
	@IncludeDeleted BIT
AS
BEGIN
	--Find the first release that is just before the current one
	SELECT TOP 1 INDENT_LEVEL
	FROM TST_RELEASE
	WHERE LEN(INDENT_LEVEL) = LEN(@IndentLevel)
		AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
		AND PROJECT_ID = @ProjectId
		AND INDENT_LEVEL < @IndentLevel
	ORDER BY INDENT_LEVEL DESC
END
GO
