-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes a Project Role
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_PROJECT_ROLE', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_PROJECT_ROLE;
GO
CREATE PROCEDURE PROJECT_DELETE_PROJECT_ROLE
	@ProjectRoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the dependent data first
    DELETE FROM TST_WORKFLOW_TRANSITION_ROLE WHERE PROJECT_ROLE_ID = @ProjectRoleId;
    DELETE FROM TST_PROJECT_USER WHERE PROJECT_ROLE_ID = @ProjectRoleId;
    DELETE FROM TST_PROJECT_ROLE_PERMISSION WHERE PROJECT_ROLE_ID = @ProjectRoleId;

	--Now delete the project role itself
    DELETE FROM TST_PROJECT_ROLE WHERE PROJECT_ROLE_ID = @ProjectRoleId;
END
GO
