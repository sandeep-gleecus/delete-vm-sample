-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Focuses the hierarchy on a specific branch
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_FOCUS_ON', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_FOCUS_ON;
GO
CREATE PROCEDURE REQUIREMENT_FOCUS_ON
	@UserId INT,
	@RequirementId INT
AS
BEGIN
	DECLARE
		@IndentLevel NVARCHAR(100),
		@IsSummary BIT,
		@ProjectId INT
	
	--First we need to get the indent-level, project id and summary flag of the passed-in requirement
	SELECT @IndentLevel = INDENT_LEVEL, @IsSummary = IS_SUMMARY, @ProjectId = PROJECT_ID
	FROM TST_REQUIREMENT
	WHERE REQUIREMENT_ID = @RequirementId

	--Update the visible flags
	--Show
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND (dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1)
			AND IS_DELETED = 0
			)
			
	--Hide
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 0
			AND IS_DELETED = 0
			)
	
	--Update the expand/collapse flag
	
	--Expand
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 1
			AND IS_DELETED = 0
			)
			
	--Collapse
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 0
			AND IS_DELETED = 0
			)

	--Now do the inserts for the case where user has no existing settings
	--Visible and Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 1, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 1
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Visible but not Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 0
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Hidden and Collapsed
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 0, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 0
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
	END
GO
