IF OBJECT_ID ( 'VW_REQUIREMENT_LIST_INTERNAL', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REQUIREMENT_LIST_INTERNAL];
GO
--This is the same view as VW_REQUIREMENT_LIST except it doesn't have IS_VISIBLE and IS_EXPANDED
--as those are dynamically added by the stored procedures. This view is actually queries and the
--data is returned in the format expected by VW_REQUIREMENT_LIST
CREATE VIEW [VW_REQUIREMENT_LIST_INTERNAL]
AS
	SELECT
		REQ.REQUIREMENT_ID, REQ.AUTHOR_ID, REQ.OWNER_ID, REQ.RELEASE_ID, REQ.PROJECT_ID, REQ.REQUIREMENT_TYPE_ID,
		REQ.REQUIREMENT_STATUS_ID, REQ.COMPONENT_ID, REQ.IMPORTANCE_ID, REQ.NAME, REQ.CREATION_DATE, REQ.INDENT_LEVEL,
		REQ.DESCRIPTION, REQ.LAST_UPDATE_DATE, REQ.IS_SUMMARY, REQ.IS_ATTACHMENTS, REQ.COVERAGE_COUNT_TOTAL,
		REQ.COVERAGE_COUNT_PASSED, REQ.COVERAGE_COUNT_FAILED, REQ.COVERAGE_COUNT_CAUTION, REQ.COVERAGE_COUNT_BLOCKED,
		REQ.ESTIMATE_POINTS, REQ.ESTIMATED_EFFORT, REQ.TASK_COUNT, REQ.TASK_ESTIMATED_EFFORT, REQ.TASK_ACTUAL_EFFORT,
		REQ.TASK_PROJECTED_EFFORT, REQ.TASK_REMAINING_EFFORT, REQ.TASK_PERCENT_ON_TIME, REQ.TASK_PERCENT_LATE_FINISH,
		REQ.TASK_PERCENT_NOT_START, REQ.TASK_PERCENT_LATE_START, REQ.IS_DELETED, REQ.CONCURRENCY_DATE, REQ.RANK, 
		RST.NAME AS REQUIREMENT_STATUS_NAME,
		(CASE WHEN REQ.IS_SUMMARY = 1 THEN 'Epic' ELSE RTP.NAME END) AS REQUIREMENT_TYPE_NAME,
		CMP.NAME AS COMPONENT_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		IMP.NAME AS IMPORTANCE_NAME,
		IMP.COLOR AS IMPORTANCE_COLOR,
		IMP.SCORE AS IMPORTANCE_SCORE,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
		RTP.IS_STEPS AS REQUIREMENT_TYPE_IS_STEPS,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM TST_REQUIREMENT AS REQ
		INNER JOIN TST_REQUIREMENT_STATUS AS RST ON REQ.REQUIREMENT_STATUS_ID = RST.REQUIREMENT_STATUS_ID
		INNER JOIN TST_REQUIREMENT_TYPE AS RTP ON REQ.REQUIREMENT_TYPE_ID = RTP.REQUIREMENT_TYPE_ID
		LEFT JOIN TST_IMPORTANCE AS IMP ON REQ.IMPORTANCE_ID = IMP.IMPORTANCE_ID
		LEFT JOIN (SELECT * FROM TST_COMPONENT WHERE IS_DELETED = 0) AS CMP ON REQ.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 1) AS ACP ON REQ.REQUIREMENT_ID = ACP.ARTIFACT_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON REQ.AUTHOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON REQ.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_RELEASE AS REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
GO
