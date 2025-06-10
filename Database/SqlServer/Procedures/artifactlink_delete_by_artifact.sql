-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactLink
-- Description:		Deletes by artifact id
-- =============================================
IF OBJECT_ID ( 'ARTIFACTLINK_DELETE_BY_ARTIFACT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACTLINK_DELETE_BY_ARTIFACT;
GO
CREATE PROCEDURE ARTIFACTLINK_DELETE_BY_ARTIFACT
	@ArtifactTypeId INT,
	@ArtifactId INT
AS
BEGIN
	SET NOCOUNT ON;
	--First delete all artifact links that have the artifact as the source id
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId AND SOURCE_ARTIFACT_ID = @ArtifactId;
	--Now delete all artifact links that have the artifact as the destination id
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId AND DEST_ARTIFACT_ID = @ArtifactId
END
GO
