IF OBJECT_ID ( 'VW_TESTSET_RELEASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_RELEASE_LIST];
GO
CREATE VIEW [VW_TESTSET_RELEASE_LIST]
AS
	SELECT
		TSE.TEST_SET_ID, TSE.PROJECT_ID, TSE.RELEASE_ID, TSE.TEST_SET_STATUS_ID,
		TSE.CREATOR_ID, TSE.OWNER_ID, TSE.AUTOMATION_HOST_ID, TSE.TEST_RUN_TYPE_ID,
		TSE.RECURRENCE_ID, TSE.TEST_SET_FOLDER_ID, TSE.NAME, TSE.DESCRIPTION,
		TSE.CREATION_DATE, TSE.PLANNED_DATE, TSE.LAST_UPDATE_DATE,
		TSE.IS_ATTACHMENTS, TSE.IS_DELETED, TSE.CONCURRENCY_DATE,
		TSE.BUILD_EXECUTE_TIME_INTERVAL, TSE.ESTIMATED_DURATION,
		RTX.ACTUAL_DURATION,
		ISNULL(RTX.COUNT_PASSED, 0) AS COUNT_PASSED,
		ISNULL(RTX.COUNT_FAILED, 0) AS COUNT_FAILED,
		ISNULL(RTX.COUNT_CAUTION, 0) AS COUNT_CAUTION,
		ISNULL(RTX.COUNT_BLOCKED, 0) AS COUNT_BLOCKED,
		ISNULL(RTX.COUNT_NOT_RUN, ISNULL(TSE.COUNT_PASSED + TSE.COUNT_FAILED + TSE.COUNT_CAUTION + TSE.COUNT_BLOCKED + TSE.COUNT_NOT_RUN + TSE.COUNT_NOT_APPLICABLE,0)) AS COUNT_NOT_RUN,
		ISNULL(RTX.COUNT_NOT_APPLICABLE, 0) AS COUNT_NOT_APPLICABLE,
		RTX.EXECUTION_DATE,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		TSS.NAME AS TEST_SET_STATUS_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME, 
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		PRJ.IS_ACTIVE AS IS_PROJECT_ACTIVE,
		AHT.NAME AS AUTOMATION_HOST_NAME,
		TRT.NAME AS TEST_RUN_TYPE_NAME,
		REC.NAME AS RECURRENCE_NAME,
		TCS.NAME AS TEST_CONFIGURATION_SET_NAME,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99,
		RTX.RELEASE_ID AS DISPLAY_RELEASE_ID
	FROM TST_TEST_SET AS TSE
		INNER JOIN (
			SELECT
				REL.RELEASE_ID, TSE.TEST_SET_ID,
				RTX.ACTUAL_DURATION, RTX.EXECUTION_DATE,
				RTX.COUNT_PASSED,
				RTX.COUNT_FAILED,
				RTX.COUNT_CAUTION,
				RTX.COUNT_BLOCKED,
				RTX.COUNT_NOT_RUN,			
				RTX.COUNT_NOT_APPLICABLE			
			FROM TST_RELEASE REL
			CROSS JOIN TST_TEST_SET TSE
			FULL OUTER JOIN TST_RELEASE_TEST_SET RTX
				ON REL.RELEASE_ID = RTX.RELEASE_ID
				AND TSE.TEST_SET_ID = RTX.TEST_SET_ID
		) AS RTX ON TSE.TEST_SET_ID = RTX.TEST_SET_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSE.CREATOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSE.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_TEST_SET_STATUS AS TSS ON TSE.TEST_SET_STATUS_ID = TSS.TEST_SET_STATUS_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 8) AS ACP ON TSE.TEST_SET_ID = ACP.ARTIFACT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSE.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSE.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_AUTOMATION_HOST AS AHT ON TSE.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID
		LEFT JOIN TST_TEST_RUN_TYPE AS TRT ON TSE.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID
		LEFT JOIN TST_RECURRENCE AS REC ON TSE.RECURRENCE_ID = REC.RECURRENCE_ID
		LEFT JOIN TST_TEST_CONFIGURATION_SET AS TCS ON TSE.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID
GO
