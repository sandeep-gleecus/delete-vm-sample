-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Deletes Project Template Configurable Types
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_DELETE_CONFIGURABLE_TYPES', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_DELETE_CONFIGURABLE_TYPES;
GO
CREATE PROCEDURE TEMPLATE_DELETE_CONFIGURABLE_TYPES
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Incident Types
	DELETE FROM TST_INCIDENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Task Types
	DELETE FROM TST_TASK_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Test Case Types
	DELETE FROM TST_TEST_CASE_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Requirement Types
	DELETE FROM TST_REQUIREMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Document Types
	DELETE FROM TST_DOCUMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Risk Types
	DELETE FROM TST_RISK_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
END
GO
