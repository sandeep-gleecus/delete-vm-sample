-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Retrieves the view permissions for a specific user
-- ================================================================
IF OBJECT_ID ( 'PROJECT_RETRIEVE_VIEW_PERMISSIONS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_RETRIEVE_VIEW_PERMISSIONS;
GO
CREATE PROCEDURE PROJECT_RETRIEVE_VIEW_PERMISSIONS
	@UserId INT
AS
BEGIN
	SELECT PRU.PROJECT_ID, PRP.ARTIFACT_TYPE_ID
	FROM VW_PROJECT_USER PRU INNER JOIN TST_PROJECT_ROLE_PERMISSION PRP
	ON	PRU.PROJECT_ROLE_ID = PRP.PROJECT_ROLE_ID
	WHERE	PRU.USER_ID = @UserId
	AND     PRP.PERMISSION_ID = 4 --View
	ORDER BY PRU.PROJECT_ID, PRP.ARTIFACT_TYPE_ID
END
GO
