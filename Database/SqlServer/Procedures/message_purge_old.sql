-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Message
-- Description:		Purges/deletes old messages
-- =============================================
IF OBJECT_ID ( 'MESSAGE_PURGE_OLD', 'P' ) IS NOT NULL 
    DROP PROCEDURE MESSAGE_PURGE_OLD;
GO
CREATE PROCEDURE MESSAGE_PURGE_OLD
	@PurgeDateTime DATETIME,
	@IncludeUnread BIT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any message that is too old
	IF @IncludeUnread = 1
	BEGIN
		DELETE FROM TST_MESSAGE
		WHERE LAST_UPDATE_DATE < @PurgeDateTime
	END
	ELSE
	BEGIN
		DELETE FROM TST_MESSAGE
		WHERE LAST_UPDATE_DATE < @PurgeDateTime
			AND (IS_READ = 1 OR IS_DELETED = 1)
	END
END
GO
