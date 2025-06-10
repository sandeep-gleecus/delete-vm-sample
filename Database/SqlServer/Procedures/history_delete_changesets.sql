-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: History
-- Description:		Removes all changesets for the given Artifact Type and ID 
-- Remarks:			If you don't pass in an @ArtifactTypeId it will delete for the whole project
-- =============================================
IF OBJECT_ID ( 'HISTORY_DELETE_CHANGESETS', 'P' ) IS NOT NULL 
    DROP PROCEDURE HISTORY_DELETE_CHANGESETS;
GO
CREATE PROCEDURE HISTORY_DELETE_CHANGESETS
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@ProjectId INT
AS
BEGIN
	IF @ArtifactTypeId IS NULL
	BEGIN
		--Remove for the entire project
		
		--Unlink the revert references
		UPDATE TST_HISTORY_CHANGESET
			SET REVERT_ID = NULL
		WHERE REVERT_ID IN (
			SELECT CHANGESET_ID
			FROM TST_HISTORY_CHANGESET
			WHERE PROJECT_ID = @ProjectId
			)
		
		--Delete from the changeset table. Deletions cascade.
        DELETE FROM TST_HISTORY_CHANGESET
        WHERE PROJECT_ID = @ProjectId
	END
	ELSE
	BEGIN
		-- Remove for the specified artifact id/type
	
		--Unlink the revert references
		UPDATE TST_HISTORY_CHANGESET
			SET REVERT_ID = NULL
		WHERE REVERT_ID IN (
			SELECT CHANGESET_ID
			FROM TST_HISTORY_CHANGESET
			WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId
			)
		
		--Delete from the changeset table. Deletions cascade.
        DELETE FROM TST_HISTORY_CHANGESET
        WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId
	END
END
GO
