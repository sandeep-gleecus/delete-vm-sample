-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Automation
-- Description:		Deletes an Automation Host
-- =============================================
IF OBJECT_ID ( 'AUTOMATION_DELETE_HOST', 'P' ) IS NOT NULL 
    DROP PROCEDURE AUTOMATION_DELETE_HOST;
GO
CREATE PROCEDURE AUTOMATION_DELETE_HOST
	@AutomationHostId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Remove references to the engine
	UPDATE TST_TEST_SET SET AUTOMATION_HOST_ID = NULL WHERE AUTOMATION_HOST_ID = @AutomationHostId;
	UPDATE TST_TEST_RUN SET AUTOMATION_HOST_ID = NULL WHERE AUTOMATION_HOST_ID = @AutomationHostId;

	--Delete the engine
    DELETE FROM TST_AUTOMATION_HOST WHERE AUTOMATION_HOST_ID = @AutomationHostId;
END
GO
