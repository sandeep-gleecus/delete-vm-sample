-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a list containing all the data-mappings for a specific artifact
--					Also returns unmapped system records
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS]
	@ProjectId INT,
	@ArtifactTypeId INT,
	@ArtifactId INT
AS
BEGIN
    SELECT	DSS.DATA_SYNC_SYSTEM_ID, @ArtifactTypeId AS ARTIFACT_TYPE_ID, @ArtifactId AS ARTIFACT_ID,
			@ProjectId AS PROJECT_ID, DAM.EXTERNAL_KEY, DSS.NAME AS DATA_SYNC_SYSTEM_NAME
    FROM	(SELECT * FROM TST_DATA_SYNC_ARTIFACT_MAPPING WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId AND PROJECT_ID = @ProjectId)
			DAM RIGHT JOIN TST_DATA_SYNC_PROJECT DSP
    ON     DAM.DATA_SYNC_SYSTEM_ID = DSP.DATA_SYNC_SYSTEM_ID AND DAM.PROJECT_ID = DSP.PROJECT_ID INNER JOIN TST_DATA_SYNC_SYSTEM DSS
    ON     DSP.DATA_SYNC_SYSTEM_ID = DSS.DATA_SYNC_SYSTEM_ID
    WHERE  DSP.ACTIVE_YN = 'Y'
    AND    DSP.PROJECT_ID = @ProjectId
    AND    @ArtifactTypeId IN (SELECT ARTIFACT_TYPE_ID FROM TST_ARTIFACT_TYPE WHERE IS_DATA_SYNC = 1)
    ORDER BY DSS.NAME
END
GO
