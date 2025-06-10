-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Logger
-- Description:		Deletes old events
-- =============================================
IF OBJECT_ID ( 'LOGGER_DELETE_OLD', 'P' ) IS NOT NULL 
    DROP PROCEDURE LOGGER_DELETE_OLD;
GO
CREATE PROCEDURE LOGGER_DELETE_OLD
	@LastDateToKeep DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any event that is too old
    DELETE FROM TST_EVENT WHERE EVENT_TIME_UTC < @LastDateToKeep
END
GO
