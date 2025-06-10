-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Deletes all the folders (no longer deletes attachmenet types since they are template based)
-- Remarks:			Typically used when deleting the entire project
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT;
GO
CREATE PROCEDURE ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT
	@ProjectId INT
AS
BEGIN
	--Now delete all the document folders in the project
    DELETE FROM TST_PROJECT_ATTACHMENT_FOLDER WHERE PROJECT_ID = @ProjectId;

END
GO
