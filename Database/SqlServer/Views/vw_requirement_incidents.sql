IF OBJECT_ID ( 'VW_REQUIREMENT_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REQUIREMENT_INCIDENTS];
GO
CREATE VIEW [VW_REQUIREMENT_INCIDENTS]
AS
	SELECT
		RTC.REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_REQUIREMENT_TEST_CASE AS RTC
		INNER JOIN TST_TEST_RUN AS TRN ON RTC.TEST_CASE_ID = TRN.TEST_CASE_ID
		INNER JOIN TST_TEST_RUN_STEP AS TRS ON TRN.TEST_RUN_ID = TRS.TEST_RUN_ID
		INNER JOIN TST_TEST_RUN_STEP_INCIDENT AS TRI ON TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID
		INNER JOIN TST_INCIDENT AS INC ON TRI.INCIDENT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	UNION

	SELECT
		ARL.SOURCE_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
		INNER JOIN TST_INCIDENT AS INC ON ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 1 AND ARL.DEST_ARTIFACT_TYPE_ID = 3

	UNION
	
	SELECT
		ARL.DEST_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
	INNER JOIN TST_INCIDENT AS INC ON ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
	INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID 
	
	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 1

GO
