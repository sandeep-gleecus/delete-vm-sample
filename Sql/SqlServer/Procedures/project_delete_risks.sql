-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Risks
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_RISKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_RISKS;
GO
CREATE PROCEDURE PROJECT_DELETE_RISKS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE TST_TASK SET RISK_ID = NULL WHERE RISK_ID IN (SELECT RISK_ID FROM TST_RISK WHERE PROJECT_ID = @ProjectId)
	DELETE FROM TST_RISK WHERE PROJECT_ID = @ProjectId
END
GO
