-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Retrieves the project group summary status for tasks
-- =============================================
IF OBJECT_ID ( 'TASK_RETRIEVE_GROUP_SUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY;
GO
CREATE PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY
	@ProjectGroupId INT,
	@ActiveReleasesOnly BIT
AS
BEGIN
	SELECT	1 AS ProgressOrderId, 'On Schedule' AS ProgressCaption, SUM((REL.TASK_PERCENT_ON_TIME * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	2 AS ProgressOrderId, 'Late Finish' AS ProgressCaption, SUM((REL.TASK_PERCENT_LATE_FINISH * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	3 AS ProgressOrderId, 'Late Start' AS ProgressCaption, SUM((REL.TASK_PERCENT_LATE_START * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	4 AS ProgressOrderId, 'Not Started' AS ProgressCaption, SUM((REL.TASK_PERCENT_NOT_START * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
END
GO
