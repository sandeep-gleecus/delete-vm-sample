-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Incidents
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_INCIDENTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_INCIDENTS;
GO
CREATE PROCEDURE PROJECT_DELETE_INCIDENTS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_INCIDENT_RESOLUTION WHERE INCIDENT_ID IN (SELECT INCIDENT_ID FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_PLACEHOLDER WHERE PROJECT_ID = @ProjectId
END
GO
