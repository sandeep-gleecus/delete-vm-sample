-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Deletes an attachment
-- Remarks:			Only pass a project ID if you just want to remove from the project and not the entire system
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_DELETE;
GO
CREATE PROCEDURE ATTACHMENT_DELETE
	@AttachmentId INT,
	@ProjectId INT
AS
BEGIN
	IF @ProjectId IS NULL
	BEGIN
		--Unlink from any test cases (used for automation scripts)
		UPDATE TST_TEST_CASE SET AUTOMATION_ATTACHMENT_ID = NULL WHERE AUTOMATION_ATTACHMENT_ID = @AttachmentId
		
		--Now we need to remove the reference to the file from project attachment table
		DELETE FROM TST_PROJECT_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId

		--Delete any tags associated with the attachment
		DELETE FROM TST_ARTIFACT_TAGS WHERE ARTIFACT_ID = @AttachmentId AND ARTIFACT_TYPE_ID = /*Document*/13

		--Finally delete the attachment record itself
		DELETE FROM TST_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId
	END
	ELSE
	BEGIN
		--Unlink from any test cases (used for automation scripts)
		UPDATE TST_TEST_CASE SET AUTOMATION_ATTACHMENT_ID = NULL WHERE AUTOMATION_ATTACHMENT_ID = @AttachmentId AND PROJECT_ID = @ProjectId
			
        --We just delete the association to the project
		DELETE FROM TST_PROJECT_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId AND PROJECT_ID = @ProjectId
	END
END
GO
