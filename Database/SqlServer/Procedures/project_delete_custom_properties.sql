-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Custom Properties
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_CUSTOM_PROPERTIES', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_CUSTOM_PROPERTIES;
GO
CREATE PROCEDURE PROJECT_DELETE_CUSTOM_PROPERTIES
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	
    --Now we need to delete all the custom properties and then custom lists. The dependent entities should then cascade
    DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId

    --Now we need to delete all the user membership and user project settings
    DELETE FROM TST_USER_ARTIFACT_FIELD WHERE PROJECT_ID = @ProjectId
    
    --Delete all of the artifact custom property records
    DELETE FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId
END
GO
