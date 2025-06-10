-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Updates the rank for passed in incident ids
-- =============================================
IF OBJECT_ID ( 'INCIDENT_UPDATE_RANKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_UPDATE_RANKS;
GO
CREATE PROCEDURE INCIDENT_UPDATE_RANKS
	@ProjectId INT,
	@IncidentIds NVARCHAR(MAX),
	@ExistingRank INT
AS
	DECLARE @IncidentIdsTable TABLE (INCIDENT_ID INT)
BEGIN
	--Get the list of incident IDs
	INSERT INTO @IncidentIdsTable (INCIDENT_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@IncidentIds, ',')

	IF @ExistingRank IS NOT NULL
	BEGIN
		--Increment the incidents that have a higher rank
		UPDATE TST_INCIDENT
			SET RANK = RANK + 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK > @ExistingRank
				AND INCIDENT_ID NOT IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
				
		--Decrement the requirements that have a lower or equal rank
		UPDATE TST_INCIDENT
			SET RANK = RANK - 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK <= @ExistingRank
				AND INCIDENT_ID NOT IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
				
		-- Set the selected incidents to this rank
		UPDATE TST_INCIDENT
			SET RANK = @ExistingRank
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND INCIDENT_ID IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
	END
	ELSE
	BEGIN
		-- Set the selected incidents to the lowest non-null rank
		UPDATE TST_INCIDENT
			SET RANK = 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND INCIDENT_ID IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
	END	
END
GO
