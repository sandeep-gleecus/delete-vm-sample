-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Deletes Project Template Custom Properties
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_DELETE_CUSTOM_PROPERTIES', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_DELETE_CUSTOM_PROPERTIES;
GO
CREATE PROCEDURE TEMPLATE_DELETE_CUSTOM_PROPERTIES
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;	
    --Now we need to delete all the custom properties and then custom lists. The dependent entities should then cascade
    DELETE FROM TST_CUSTOM_PROPERTY WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
    DELETE FROM TST_CUSTOM_PROPERTY_LIST WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
END
GO
