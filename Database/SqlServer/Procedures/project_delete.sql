-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes a Project
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE;
GO
CREATE PROCEDURE PROJECT_DELETE
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --Unset any user's 'last opened project' if set
	UPDATE TST_USER_PROFILE SET LAST_OPENED_PROJECT_ID = NULL WHERE LAST_OPENED_PROJECT_ID = @ProjectId

	--Delete any project settings
	DELETE FROM TST_PROJECT_SETTING_VALUE WHERE PROJECT_ID = @ProjectId

    --Finally delete the project itself
    DELETE FROM TST_PROJECT WHERE PROJECT_ID = @ProjectId
END
GO
