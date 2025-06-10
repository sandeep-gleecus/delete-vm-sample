-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Updates the rank for passed in requirement ids
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_UPDATE_RANKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_UPDATE_RANKS;
GO
CREATE PROCEDURE REQUIREMENT_UPDATE_RANKS
	@ProjectId INT,
	@RequirementIds NVARCHAR(MAX),
	@ExistingRank INT
AS
	DECLARE @RequirementIdsTable TABLE (REQUIREMENT_ID INT)
BEGIN
	--Get the list of requirement IDs
	INSERT INTO @RequirementIdsTable (REQUIREMENT_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementIds, ',')

	IF @ExistingRank IS NOT NULL
	BEGIN
		--Increment the requirements that have a higher rank
		UPDATE TST_REQUIREMENT
			SET RANK = RANK + 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK > @ExistingRank
				AND REQUIREMENT_ID NOT IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
				
		--Decrement the requirements that have a lower or equal rank
		UPDATE TST_REQUIREMENT
			SET RANK = RANK - 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK <= @ExistingRank
				AND REQUIREMENT_ID NOT IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
				
		-- Set the selected requirements to this rank
		UPDATE TST_REQUIREMENT
			SET RANK = @ExistingRank
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
	END
	ELSE
	BEGIN
		-- Set the selected requirements to the lowest non-null rank
		UPDATE TST_REQUIREMENT
			SET RANK = 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
	END	
END
GO
