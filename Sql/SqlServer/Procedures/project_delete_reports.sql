-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Reports
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_REPORTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_REPORTS;
GO
CREATE PROCEDURE PROJECT_DELETE_REPORTS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Everything else cascades
	DELETE FROM TST_REPORT_SAVED WHERE PROJECT_ID = @ProjectId
END
GO
