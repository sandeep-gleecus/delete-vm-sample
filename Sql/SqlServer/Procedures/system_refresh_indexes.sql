-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: SystemManager
-- Description:		Refreshes the database indexes
-- =============================================
IF OBJECT_ID ( 'SYSTEM_REFRESH_INDEXES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [SYSTEM_REFRESH_INDEXES];
GO
CREATE PROCEDURE [SYSTEM_REFRESH_INDEXES]
AS
BEGIN
	--Simply calls the built in stored procedure to do this
	EXEC sp_MSforeachtable 'alter index all on ? rebuild'
END
GO
