IF OBJECT_ID ('VW_HISTORYCHANGE_LIST', 'V') IS NOT NULL 
    DROP VIEW [VW_HISTORYCHANGE_LIST];
GO
CREATE VIEW [dbo].[VW_HISTORYCHANGE_LIST]
AS
	SELECT 
		HC.*, HD.ARTIFACT_HISTORY_ID, HD.FIELD_NAME, HD.FIELD_CAPTION,
		HD.OLD_VALUE, HD.NEW_VALUE,
		HD.OLD_VALUE_INT, HD.NEW_VALUE_INT,
		HD.OLD_VALUE_DATE, HD.NEW_VALUE_DATE,HD.FIELD_ID,PR.NAME AS PROJECTNAME,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME,
		AT.NAME AS ARTIFACT_TYPE_NAME
		
	FROM [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC
	INNER JOIN [ValidationMasterAudit].dbo.TST_HISTORY_DETAIL AS HD ON HC.CHANGESET_ID = HD.CHANGESET_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
		INNER JOIN TST_PROJECT AS PR ON HC.PROJECT_ID = PR.PROJECT_ID
GO
