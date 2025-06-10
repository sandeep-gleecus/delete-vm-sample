-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Removes an attachment from an artifact
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_REMOVE_FROM_ARTIFACT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_REMOVE_FROM_ARTIFACT;
GO
CREATE PROCEDURE ATTACHMENT_REMOVE_FROM_ARTIFACT
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@AttachmentId INT,
	@ProjectId INT
AS
BEGIN
	DELETE FROM TST_ARTIFACT_ATTACHMENT
	WHERE (ATTACHMENT_ID = @AttachmentId OR @AttachmentId IS NULL)
		AND (ARTIFACT_ID = @ArtifactId OR @ArtifactId IS NULL)
		AND (ARTIFACT_TYPE_ID = @ArtifactTypeId OR @ArtifactTypeId IS NULL)
		AND (PROJECT_ID = @ProjectId OR @ProjectId IS NULL)
END
GO
