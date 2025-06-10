-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes a Requirement
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_DELETE;
GO
CREATE PROCEDURE REQUIREMENT_DELETE
	@RequirementId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete user notification subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 1 AND ARTIFACT_ID = @RequirementId);
	--Delete any linked test steps
	DELETE FROM TST_REQUIREMENT_TEST_STEP WHERE REQUIREMENT_ID = @RequirementId;
	DELETE FROM TST_REQUIREMENT_SIGNATURE WHERE REQUIREMENT_ID = @RequirementId;
	--Now delete the requirement itself
    DELETE FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @RequirementId;
END
GO
