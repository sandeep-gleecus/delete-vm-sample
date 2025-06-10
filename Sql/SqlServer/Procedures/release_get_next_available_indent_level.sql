-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Returns the next available indent level (for new inserts)
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL;
GO
CREATE PROCEDURE RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL
	@ProjectId INT,
	@UserId INT,
	@IgnoreLastInserted BIT
AS
BEGIN
	--See if they want to insert at the root level or directly under the last inserted item
	IF @IgnoreLastInserted = 1
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_RELEASE REL
			LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = @UserId) AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) AND
			LEN(INDENT_LEVEL) = 3
		ORDER BY REL.INDENT_LEVEL DESC, REL.RELEASE_ID
	END
	ELSE
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_RELEASE REL
			LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = @UserId) AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL)
		ORDER BY REL.INDENT_LEVEL DESC, REL.RELEASE_ID
	END
END
GO
