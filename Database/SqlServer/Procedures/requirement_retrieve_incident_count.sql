-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of incidents mapped against requirement
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_INCIDENT_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_INCIDENT_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_INCIDENT_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@NumberOfRows INT,
	@OnlyIncludeWithOpenIncidents BIT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified
	IF @ReleaseId IS NOT NULL
	BEGIN
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	END
	
	--Create select command for retrieving the the list of open incidents and total incidents for requirements
	--This needs to be a union between the incidents linked via test runs and those statically linked
	SELECT TOP(@NumberOfRows) REQ.REQUIREMENT_ID AS RequirementId, REQ.NAME AS Name, REQ.INDENT_LEVEL AS IndentLevel,
		REQ.IS_SUMMARY AS IsSummary,
		RIN1.INCIDENT_COUNT AS IncidentTotalCount, RIN2.INCIDENT_COUNT AS IncidentOpenCount
	FROM TST_REQUIREMENT REQ LEFT JOIN (
		SELECT REQUIREMENT_ID, COUNT(INCIDENT_ID) AS INCIDENT_COUNT
		FROM VW_REQUIREMENT_INCIDENTS
		WHERE (@ReleaseId IS NULL OR DETECTED_RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
		GROUP BY REQUIREMENT_ID) RIN1
	ON REQ.REQUIREMENT_ID = RIN1.REQUIREMENT_ID LEFT JOIN (
		SELECT REQUIREMENT_ID, COUNT(INCIDENT_ID) AS INCIDENT_COUNT
		FROM VW_REQUIREMENT_INCIDENTS
		WHERE IS_OPEN_STATUS = 1 AND (@ReleaseId IS NULL OR DETECTED_RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
		GROUP BY REQUIREMENT_ID) RIN2
	ON REQ.REQUIREMENT_ID = RIN2.REQUIREMENT_ID
	--Create the appropriate clause for determining if we show all incident or just open incident rows
	WHERE ((@OnlyIncludeWithOpenIncidents = 1 AND RIN2.INCIDENT_COUNT IS NOT NULL) OR (@OnlyIncludeWithOpenIncidents = 0 AND RIN1.INCIDENT_COUNT IS NOT NULL))
	AND REQ.PROJECT_ID = @ProjectId AND REQ.IS_DELETED = 0
	ORDER BY IncidentOpenCount DESC, IncidentTotalCount DESC, RequirementId ASC
END
GO
