-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Updates the positional data for the requirement
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_UPDATE_POSITIONAL', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_UPDATE_POSITIONAL;
GO
CREATE PROCEDURE REQUIREMENT_UPDATE_POSITIONAL
	@RequirementId INT,
	@UserId INT,
	@IsExpanded BIT,
	@IsVisible BIT,
	@IsSummary BIT,
	@IndentLevel NVARCHAR(100)	
AS
	DECLARE @RequirementCount INT
BEGIN
	--First update the requirement table itself
	UPDATE TST_REQUIREMENT
	SET	INDENT_LEVEL = @IndentLevel,
		IS_SUMMARY = @IsSummary
	WHERE REQUIREMENT_ID = @RequirementId

	--Now insert/update the requirement user navigation metadata
    SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @RequirementId AND USER_ID = @UserId);
    IF @RequirementCount = 0 AND @UserId IS NOT NULL
    BEGIN
		INSERT INTO TST_REQUIREMENT_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
		VALUES (@UserId, @IsExpanded, @IsVisible, @RequirementId)
	END
    ELSE
    BEGIN
		UPDATE TST_REQUIREMENT_USER
		SET	IS_EXPANDED = @IsExpanded,
			IS_VISIBLE = @IsVisible
		WHERE REQUIREMENT_ID = @RequirementId
		AND USER_ID = @UserId
	END
END
GO
