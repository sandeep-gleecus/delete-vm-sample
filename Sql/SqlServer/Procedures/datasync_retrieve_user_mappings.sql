-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a list containing all the data-mappings for a specific user
--					Also returns unmapped system records
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_USER_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_USER_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_USER_MAPPINGS]
	@UserId INT
AS
BEGIN
    SELECT	DSS.DATA_SYNC_SYSTEM_ID, @UserId AS USER_ID, DUM.EXTERNAL_KEY, DSS.NAME AS DATA_SYNC_SYSTEM_NAME,
			(CASE DSS.CAPTION WHEN NULL THEN DSS.NAME ELSE DSS.CAPTION END) AS DATA_SYNC_SYSTEM_DISPLAY_NAME
    FROM	(SELECT * FROM TST_DATA_SYNC_USER_MAPPING
			WHERE USER_ID = @UserId) DUM RIGHT JOIN TST_DATA_SYNC_SYSTEM DSS
    ON     DUM.DATA_SYNC_SYSTEM_ID = DSS.DATA_SYNC_SYSTEM_ID
    ORDER BY DSS.NAME
END
GO
