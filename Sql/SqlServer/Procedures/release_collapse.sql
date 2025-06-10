-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Collapses all the levels of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'RELEASE_COLLAPSE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_COLLAPSE;
GO
CREATE PROCEDURE RELEASE_COLLAPSE
	@UserId INT,
	@ProjectId INT,
	@SummaryReleaseId INT
AS
BEGIN
	DECLARE @Length INT,
	@NewIsVisible BIT,
	@NewIsExpanded BIT,
	@ReleaseId INT,
	@IsSummary BIT,
	@IsVisible BIT,
	@IsExpanded BIT,
	@ReleaseCount INT,
	@IndentLevel NVARCHAR(100),
	@IsParentSummary BIT,
	@IsParentExpanded BIT,
	@IsParentVisible BIT
				
	--First we need to retrieve the release to make sure it is an expanded summary one
	SET @IsParentSummary = (SELECT IS_SUMMARY FROM TST_RELEASE WHERE RELEASE_ID = @SummaryReleaseId)
	SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
	IF @ReleaseCount = 0
	BEGIN
		SET @IsParentExpanded = 1
		SET @IsParentVisible = 1
	END
	ELSE
	BEGIN
		SET @IsParentExpanded = (SELECT IS_EXPANDED FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
		SET @IsParentVisible = (SELECT IS_VISIBLE FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
	END
	SET @IndentLevel = (SELECT INDENT_LEVEL FROM TST_RELEASE WHERE RELEASE_ID = @SummaryReleaseId)

	IF @IsParentSummary = 1 AND @IsParentExpanded = 1
	BEGIN	
		SET @Length = LEN(@IndentLevel)

		--Collapse the parent folder to start with
		SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
		IF @ReleaseCount = 0
		BEGIN
			INSERT INTO TST_RELEASE_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID) VALUES (@UserId, 0, @IsParentVisible, @SummaryReleaseId)
		END
		ELSE
		BEGIN
			UPDATE TST_RELEASE_USER SET IS_EXPANDED = 0 WHERE (RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId);
		END
			
		--Get all its child items and make them non-visible, collapsing any folders as well

		--Update settings
		UPDATE TST_RELEASE_USER
			SET IS_VISIBLE = 0, IS_EXPANDED = 0
			WHERE USER_ID = @UserId
			AND RELEASE_ID IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND IS_DELETED = 0
				)
		--Insert settings
		INSERT INTO TST_RELEASE_USER
			(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
		SELECT @UserId, 0, 0, RELEASE_ID
		FROM TST_RELEASE
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND RELEASE_ID NOT IN (
					SELECT RELEASE_ID
					FROM TST_RELEASE_USER
					WHERE USER_ID = @UserId)
				AND IS_DELETED = 0
	END
END
GO
