-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: UserManager
-- Description:		Deletes/unassociates all the related information to a demo user
-- =============================================
IF OBJECT_ID ( 'USER_DELETE_RELATED', 'P' ) IS NOT NULL 
    DROP PROCEDURE USER_DELETE_RELATED;
GO
CREATE PROCEDURE USER_DELETE_RELATED
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
    
    --Delete the contacts and instant messages
    DELETE TST_MESSAGE WHERE SENDER_USER_ID = @UserId
    DELETE TST_MESSAGE WHERE RECIPIENT_USER_ID = @UserId
    DELETE TST_USER_CONTACT WHERE CREATOR_USER_ID = @UserId
    DELETE TST_USER_CONTACT WHERE CONTACT_USER_ID = @UserId
    
    --Move any references to the user to the system administrator user
    UPDATE TST_TASK SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TASK SET OWNER_ID = NULL WHERE OWNER_ID = @UserId
    UPDATE TST_INCIDENT SET OPENER_ID = 1 WHERE OPENER_ID = @UserId
    UPDATE TST_INCIDENT SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_INCIDENT_RESOLUTION SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TEST_CASE SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_TEST_CASE SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_TEST_SET SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_TEST_SET SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_REQUIREMENT SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_RELEASE SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TEST_RUN SET TESTER_ID = 1 WHERE TESTER_ID = @UserId
    UPDATE TST_TEST_RUN SET TESTER_ID = 1 WHERE TESTER_ID = @UserId
    UPDATE TST_ATTACHMENT SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_ATTACHMENT SET EDITOR_ID = 1 WHERE EDITOR_ID = @UserId
    UPDATE TST_ATTACHMENT_VERSION SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_HISTORY_CHANGESET SET USER_ID = 1 WHERE USER_ID = @UserId
    UPDATE TST_ARTIFACT_LINK SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId

    --Delete any user project settings
    DELETE FROM TST_TEST_RUNS_PENDING WHERE TESTER_ID = @UserId
    DELETE FROM TST_PROJECT_COLLECTION_ENTRY WHERE USER_ID = @UserId
    DELETE FROM TST_USER_COLLECTION_ENTRY WHERE USER_ID = @UserId
    DELETE FROM TST_USER_ARTIFACT_FIELD WHERE USER_ID = @UserId
    DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE USER_ID = @UserId
END
GO
