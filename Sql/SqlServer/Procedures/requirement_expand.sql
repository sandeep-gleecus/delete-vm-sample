-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Expands one level of children of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_EXPAND', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_EXPAND;
GO
CREATE PROCEDURE REQUIREMENT_EXPAND
	@UserId INT,
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	DECLARE @Length INT
	
	SET @Length = LEN(@IndentLevel)

	--We need to expand the immediate child items and make visible
	--Update settings
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND IS_DELETED = 0
			)
	--Insert settings
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
