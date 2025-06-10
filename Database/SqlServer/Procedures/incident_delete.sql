-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Deletes an Incident
-- =============================================
IF OBJECT_ID ( 'INCIDENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_DELETE;
GO
CREATE PROCEDURE INCIDENT_DELETE
	@IncidentId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the resolutions
    DELETE FROM TST_INCIDENT_RESOLUTION WHERE INCIDENT_ID = @IncidentId;
	--Delete user subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 3 AND ARTIFACT_ID = @IncidentId);
	--Now delete the incident itself
    DELETE FROM TST_INCIDENT WHERE INCIDENT_ID = @IncidentId;
END
GO
