IF OBJECT_ID ( 'VW_ARTIFACT_LINK_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_ARTIFACT_LINK_LIST];
GO
CREATE VIEW [VW_ARTIFACT_LINK_LIST]
AS
    SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
    		ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ARL.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME,
    		STA.NAME AS ARTIFACT_STATUS_NAME, REQ.PROJECT_ID AS PROJECT_ID
    FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
    ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
    ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT REQ
    ON		ARL.DEST_ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
GO
