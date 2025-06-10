-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a count of incidents by status for a particular severity level
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_COUNT_BY_SEVERITY', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_SEVERITY;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_SEVERITY
	@ProjectId INT,
	@ReleaseId INT,
	@SeverityId INT,
	@IncidentTypeId INT,
	@UseResolvedRelease BIT,
	@IncludeDeleted BIT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
        SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
        FROM	TST_INCIDENT
        WHERE	PROJECT_ID = @ProjectId
        AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
        AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
        AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
        GROUP BY INCIDENT_STATUS_ID
        ORDER BY INCIDENT_STATUS_ID
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		IF @UseResolvedRelease = 1
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
		ELSE
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
	END
END
GO
