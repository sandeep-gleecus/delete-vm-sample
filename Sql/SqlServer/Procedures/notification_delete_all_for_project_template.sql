-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Notification
-- Description:		Deletes all the notification events and templates for a specific project template
-- =============================================
IF OBJECT_ID ( 'NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE', 'P' ) IS NOT NULL 
    DROP PROCEDURE NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE;
GO
CREATE PROCEDURE NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete all the notification events
	DELETE FROM TST_NOTIFICATION_EVENT WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId

    --Delete all the notification templates
    DELETE FROM TST_NOTIFICATION_ARTIFACT_TEMPLATE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
END
GO
