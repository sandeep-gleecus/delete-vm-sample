-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Releases & Automation Hosts
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_RELEASES_AUTOMATION', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_RELEASES_AUTOMATION;
GO
CREATE PROCEDURE PROJECT_DELETE_RELEASES_AUTOMATION
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_RELEASE WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_AUTOMATION_HOST WHERE PROJECT_ID = @ProjectId
END
GO
