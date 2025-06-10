-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Updates the positional data for the release
-- =============================================
IF OBJECT_ID ( 'RELEASE_UPDATE_POSITIONAL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_UPDATE_POSITIONAL;
GO
CREATE PROCEDURE RELEASE_UPDATE_POSITIONAL
	@ReleaseId INT,
	@UserId INT,
	@IsExpanded BIT,
	@IsVisible BIT,
	@IsSummary BIT,
	@IndentLevel NVARCHAR(100)	
AS
	DECLARE @ReleaseCount INT
BEGIN
	--First update the release table itself
	UPDATE TST_RELEASE
	SET	INDENT_LEVEL = @IndentLevel,
		IS_SUMMARY = @IsSummary
	WHERE RELEASE_ID = @ReleaseId

	--Now insert/update the release user navigation metadata
    SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @ReleaseId AND USER_ID = @UserId);
    IF @ReleaseCount = 0 AND @UserId IS NOT NULL
    BEGIN
		INSERT INTO TST_RELEASE_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
		VALUES (@UserId, @IsExpanded, @IsVisible, @ReleaseId)
	END
    ELSE
    BEGIN
		UPDATE TST_RELEASE_USER
		SET	IS_EXPANDED = @IsExpanded,
			IS_VISIBLE = @IsVisible
		WHERE RELEASE_ID = @ReleaseId
		AND USER_ID = @UserId
	END
END
GO
