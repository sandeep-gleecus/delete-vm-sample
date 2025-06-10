-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the total estimated effort for any requirement
--					that does not have any tasks but is part of the release
-- ================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_RELEASE_ESTIMATE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_RELEASE_ESTIMATE;
GO
CREATE PROCEDURE REQUIREMENT_GET_RELEASE_ESTIMATE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--We exclude the summary items to avoid duplicated effort
	SELECT SUM(REQ.ESTIMATED_EFFORT) AS ESTIMATED_EFFORT
	FROM TST_REQUIREMENT REQ
	WHERE REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
	AND REQ.RELEASE_ID = @ReleaseId
	AND REQ.TASK_COUNT = 0
END
GO
