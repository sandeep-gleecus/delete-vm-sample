-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Automation
-- Description:		Deletes an Automation Engine
-- =============================================
IF OBJECT_ID ( 'AUTOMATION_DELETE_ENGINE', 'P' ) IS NOT NULL 
    DROP PROCEDURE AUTOMATION_DELETE_ENGINE;
GO
CREATE PROCEDURE AUTOMATION_DELETE_ENGINE
	@AutomationEngineId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Remove references to the engine
	UPDATE TST_TEST_CASE SET AUTOMATION_ENGINE_ID = NULL WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;
	UPDATE TST_TEST_RUN SET AUTOMATION_ENGINE_ID = NULL WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;

	--Delete the engine
    DELETE FROM TST_AUTOMATION_ENGINE WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;
END
GO
