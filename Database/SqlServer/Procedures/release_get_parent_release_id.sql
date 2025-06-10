-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the parent release of the current release/iteration (if there is one)
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_PARENT_RELEASE_ID', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_PARENT_RELEASE_ID;
GO
CREATE PROCEDURE RELEASE_GET_PARENT_RELEASE_ID
	@ReleaseOrIterationId INT
AS
DECLARE
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
BEGIN
	--Get the indent level of the passed in release/iteration
	SELECT
		@IndentLevel = INDENT_LEVEL,
		@ProjectId = PROJECT_ID
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseOrIterationId
	
	--Get the parent of this
	SELECT RELEASE_ID
	FROM TST_RELEASE
	WHERE
		INDENT_LEVEL = SUBSTRING(@IndentLevel, 1, LEN(@IndentLevel)-3) AND
		PROJECT_ID = @ProjectId
END
GO
