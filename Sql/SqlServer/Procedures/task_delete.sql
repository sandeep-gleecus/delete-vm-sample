-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Deletes an Incident
-- =============================================
IF OBJECT_ID ( 'TASK_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TASK_DELETE;
GO
CREATE PROCEDURE TASK_DELETE
	@TaskId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete pull request info
	DELETE FROM TST_VERSION_CONTROL_PULL_REQUEST WHERE TASK_ID = @TaskId;
	--Delete user subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 6 AND ARTIFACT_ID = @TaskId);
	--Now delete the incident itself
    DELETE FROM TST_TASK WHERE TASK_ID = @TaskId;
END
GO
