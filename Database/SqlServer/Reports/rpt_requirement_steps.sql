IF OBJECT_ID ( 'RPT_REQUIREMENT_STEPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_STEPS];
GO
CREATE VIEW [RPT_REQUIREMENT_STEPS]
AS
	SELECT	RQS.*,
			REQ.NAME AS REQUIREMENT_NAME,
			REQ.LAST_UPDATE_DATE AS REQUIREMENT_LAST_UPDATE_DATE,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID AS PROJECT_PROJECT_GROUP_ID,
			PRJ.NAME AS PROJECT_NAME
            			
    FROM TST_REQUIREMENT_STEP RQS
		INNER JOIN TST_REQUIREMENT REQ ON RQS.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
