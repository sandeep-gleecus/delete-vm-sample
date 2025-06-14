-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a count of incidents that are in an open or closed status
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@UseResolvedRelease BIT
AS
BEGIN
	IF @ReleaseId IS NULL
	BEGIN
		SELECT	INC.INCIDENT_STATUS_IS_OPEN_STATUS AS IS_OPEN_STATUS, ISNULL(COUNT(INC.INCIDENT_ID),0) AS INCIDENT_COUNT
		FROM	VW_INCIDENT_LIST INC
		WHERE	PROJECT_ID = @ProjectId
		AND IS_DELETED = 0
		GROUP BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	    ORDER BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
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

		SELECT	INC.INCIDENT_STATUS_IS_OPEN_STATUS AS IS_OPEN_STATUS, ISNULL(COUNT(INC.INCIDENT_ID),0) AS INCIDENT_COUNT
		FROM	VW_INCIDENT_LIST INC
		WHERE	PROJECT_ID = @ProjectId
		AND ((@UseResolvedRelease = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
			OR (@UseResolvedRelease = 1 AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)))
		AND IS_DELETED = 0
		GROUP BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	    ORDER BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	END
END
GO
