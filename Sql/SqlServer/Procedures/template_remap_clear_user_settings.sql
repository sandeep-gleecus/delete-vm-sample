-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Clears the user settings that would be affected by a change in project template
-- Remarks:			
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_CLEAR_USER_SETTINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_CLEAR_USER_SETTINGS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_CLEAR_USER_SETTINGS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Collections
	DELETE FROM TST_PROJECT_COLLECTION_ENTRY WHERE PROJECT_ID = @ProjectId
	
	--Columns
	DELETE FROM TST_USER_ARTIFACT_FIELD WHERE PROJECT_ID = @ProjectId
	DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId
	
	--Saved Filters and Reports
	DELETE FROM TST_SAVED_FILTER WHERE PROJECT_ID = @ProjectId
	DELETE FROM TST_REPORT_SAVED WHERE PROJECT_ID = @ProjectId

END
GO
