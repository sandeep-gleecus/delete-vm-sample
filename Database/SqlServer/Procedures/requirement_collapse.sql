-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Collapses all the levels of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_COLLAPSE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COLLAPSE;
GO
CREATE PROCEDURE REQUIREMENT_COLLAPSE
	@UserId INT,
	@ProjectId INT,
	@SummaryRequirementId INT
AS
BEGIN
	DECLARE @Length INT,
	@NewIsVisible BIT,
	@NewIsExpanded BIT,
	@RequirementId INT,
	@IsSummary BIT,
	@IsVisible BIT,
	@IsExpanded BIT,
	@RequirementCount INT,
	@IndentLevel NVARCHAR(100),
	@ParentIsSummary BIT,
	@ParentIsExpanded BIT,
	@ParentIsVisible BIT
				
	--First we need to retrieve the test-case to make sure it is an expanded summary one
	SET @ParentIsSummary = (SELECT IS_SUMMARY FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @SummaryRequirementId)
	SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
	IF @RequirementCount = 0
	BEGIN
		SET @ParentIsExpanded = 1
		SET @ParentIsVisible = 1
	END
	ELSE
	BEGIN
		SET @ParentIsExpanded = (SELECT IS_EXPANDED FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
		SET @ParentIsVisible = (SELECT IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
	END
	SET @IndentLevel = (SELECT INDENT_LEVEL FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @SummaryRequirementId)

	IF @ParentIsSummary = 1 AND @ParentIsExpanded = 1
	BEGIN	
		SET @Length = LEN(@IndentLevel)

		--Collapse the parent folder to start with
		SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
		IF @RequirementCount = 0
		BEGIN
			INSERT INTO TST_REQUIREMENT_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID) VALUES (@UserId, 0, @ParentIsVisible, @SummaryRequirementId)
		END
		ELSE
		BEGIN
			UPDATE TST_REQUIREMENT_USER SET IS_EXPANDED = 0 WHERE (REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId);
		END
			
		--Get all its child items and make them non-visible, collapsing any folders as well

		--Update settings
		UPDATE TST_REQUIREMENT_USER
			SET IS_VISIBLE = 0, IS_EXPANDED = 0
			WHERE USER_ID = @UserId
			AND REQUIREMENT_ID IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND IS_DELETED = 0
				)
		--Insert settings
		INSERT INTO TST_REQUIREMENT_USER
			(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
		SELECT @UserId, 0, 0, REQUIREMENT_ID
		FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND REQUIREMENT_ID NOT IN (
					SELECT REQUIREMENT_ID
					FROM TST_REQUIREMENT_USER
					WHERE USER_ID = @UserId)
				AND IS_DELETED = 0
	END
END
GO
