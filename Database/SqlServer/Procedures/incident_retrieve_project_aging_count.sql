-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the count of open incidents in the project by age
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_PROJECT_AGING_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_PROJECT_AGING_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_PROJECT_AGING_COUNT
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
		SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
		FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
					CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
					FROM TST_INCIDENT WHERE IS_DELETED = 0) AS INC
		INNER JOIN TST_INCIDENT_STATUS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
		WHERE	INC.PROJECT_ID = @ProjectId
		AND	IST.IS_OPEN_STATUS = 1
		GROUP BY Age
		ORDER BY Age ASC
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
		
		--Now get the age count
		SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
		FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
					CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
					FROM TST_INCIDENT
					WHERE IS_DELETED = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)) AS INC
		INNER JOIN TST_INCIDENT_STATUS IST
		ON     INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
		WHERE	INC.PROJECT_ID = @ProjectId
		AND	IST.IS_OPEN_STATUS = 1
		GROUP BY Age
		ORDER BY Age ASC
	END
END
GO
