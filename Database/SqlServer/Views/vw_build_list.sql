IF OBJECT_ID ( 'VW_BUILD_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_BUILD_LIST];
GO
CREATE VIEW [VW_BUILD_LIST]
AS
	SELECT	BLD.BUILD_ID, BLD.BUILD_STATUS_ID, BLD.RELEASE_ID, BLD.PROJECT_ID, BLD.NAME, BLD.IS_DELETED,
			BLD.CREATION_DATE, BLD.LAST_UPDATE_DATE, BLS.NAME AS BUILD_STATUS_NAME,
			REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER, PRJ.NAME AS PROJECT_NAME,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE, PRJ.PROJECT_GROUP_ID
	FROM	TST_BUILD BLD
	INNER JOIN TST_BUILD_STATUS AS BLS ON BLD.BUILD_STATUS_ID = BLS.BUILD_STATUS_ID
	INNER JOIN TST_RELEASE AS REL ON BLD.RELEASE_ID = REL.RELEASE_ID
	INNER JOIN TST_PROJECT AS PRJ ON BLD.PROJECT_ID = PRJ.PROJECT_ID
GO
