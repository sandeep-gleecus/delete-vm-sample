-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Expands one level of children of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'RELEASE_EXPAND', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_EXPAND;
GO
CREATE PROCEDURE RELEASE_EXPAND
	@UserId INT,
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	DECLARE @Length INT
	
	SET @Length = LEN(@IndentLevel)

	--We need to expand the immediate child items and make visible
	--Update settings
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND IS_DELETED = 0
			)
	--Insert settings
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
