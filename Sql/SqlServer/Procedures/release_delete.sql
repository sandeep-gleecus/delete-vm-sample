-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes a Release
-- =============================================
IF OBJECT_ID ( 'RELEASE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_DELETE;
GO
CREATE PROCEDURE RELEASE_DELETE
	@ReleaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any data-mapping entries
	DELETE FROM TST_DATA_SYNC_ARTIFACT_MAPPING
	WHERE ARTIFACT_TYPE_ID = 4 /* Release */
	AND ARTIFACT_ID = @ReleaseId
	
	--Have to set the 3 Incident release fields to null
	--Cannot use cascades because of the multiple field issue
	UPDATE TST_INCIDENT SET DETECTED_RELEASE_ID = NULL WHERE DETECTED_RELEASE_ID = @ReleaseId;
	UPDATE TST_INCIDENT SET RESOLVED_RELEASE_ID = NULL WHERE RESOLVED_RELEASE_ID = @ReleaseId;
	UPDATE TST_INCIDENT SET VERIFIED_RELEASE_ID = NULL WHERE VERIFIED_RELEASE_ID = @ReleaseId;
	
	--Have to unset any builds linked to the release, cannot use cascades due to
	--SQL Server limitations
	UPDATE TST_INCIDENT SET RESOLVED_BUILD_ID = NULL WHERE RESOLVED_BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	UPDATE TST_INCIDENT SET DETECTED_BUILD_ID = NULL WHERE DETECTED_BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	UPDATE TST_TEST_RUN SET BUILD_ID = NULL WHERE BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	
	--Now delete the release itself
    DELETE FROM TST_RELEASE WHERE RELEASE_ID = @ReleaseId;
END
GO
