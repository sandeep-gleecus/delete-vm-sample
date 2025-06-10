-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Updates the attachment flag of an artifact
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_UPDATE_ARTIFACT_FLAG', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_UPDATE_ARTIFACT_FLAG;
GO
CREATE PROCEDURE ATTACHMENT_UPDATE_ARTIFACT_FLAG
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@HasAttachments BIT
AS
BEGIN
    IF @ArtifactTypeId = 1
	BEGIN
		UPDATE TST_REQUIREMENT SET IS_ATTACHMENTS = @HasAttachments WHERE REQUIREMENT_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 2
	BEGIN
		UPDATE TST_TEST_CASE SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_CASE_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 4
	BEGIN
		UPDATE TST_RELEASE SET IS_ATTACHMENTS = @HasAttachments WHERE RELEASE_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 3
	BEGIN
        UPDATE TST_INCIDENT SET IS_ATTACHMENTS = @HasAttachments WHERE INCIDENT_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 6
	BEGIN
        UPDATE TST_TASK SET IS_ATTACHMENTS = @HasAttachments WHERE TASK_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 7
	BEGIN
        UPDATE TST_TEST_STEP SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_STEP_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 8
	BEGIN
        UPDATE TST_TEST_SET SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_SET_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 5
	BEGIN
        UPDATE TST_TEST_RUN SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_RUN_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 9
	BEGIN
        UPDATE TST_AUTOMATION_HOST SET IS_ATTACHMENTS = @HasAttachments WHERE AUTOMATION_HOST_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 14
	BEGIN
        UPDATE TST_RISK SET IS_ATTACHMENTS = @HasAttachments WHERE RISK_ID = @ArtifactId
	END
END
GO
