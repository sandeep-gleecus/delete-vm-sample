-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a count of requirements by scope level for a particular importance level
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE', 'P' ) IS NOT NULL 
    DROP PROCEDURE [REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE];
GO
CREATE PROCEDURE [REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE]
	@ProjectId INT,
	@ImportanceId INT,
	@ReleaseId INT
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

	--See if we have an importance specified	
	IF @ImportanceId IS NULL
	BEGIN
		--Get count for requirements that have no importance set
		SELECT	REQUIREMENT_STATUS_ID AS RequirementStatusId, COUNT(REQUIREMENT_ID) AS RequirementCount
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND IMPORTANCE_ID IS NULL
		AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
		AND IS_DELETED = 0
		GROUP BY REQUIREMENT_STATUS_ID
		ORDER BY REQUIREMENT_STATUS_ID
	END
	ELSE
	BEGIN				
		--Get count for requirements that do have the importance set
		SELECT	REQUIREMENT_STATUS_ID AS RequirementStatusId, COUNT(REQUIREMENT_ID) AS RequirementCount
		FROM	TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND	IMPORTANCE_ID = @ImportanceId
		AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
		AND IS_DELETED = 0
		GROUP BY REQUIREMENT_STATUS_ID
		ORDER BY REQUIREMENT_STATUS_ID
	END
END
GO
