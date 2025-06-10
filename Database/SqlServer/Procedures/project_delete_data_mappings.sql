-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Mappings
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_DATA_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_DATA_MAPPINGS;
GO
CREATE PROCEDURE PROJECT_DELETE_DATA_MAPPINGS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING WHERE PROJECT_ID = @ProjectId
END
GO
