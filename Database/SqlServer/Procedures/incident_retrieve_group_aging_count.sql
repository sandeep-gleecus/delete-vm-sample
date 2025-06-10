-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the count of open incidents in the group by age
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_GROUP_AGING_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_GROUP_AGING_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_GROUP_AGING_COUNT
	@ProjectGroupId INT
AS
BEGIN
    SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
    FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
				CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
				FROM TST_INCIDENT WHERE IS_DELETED = 0) AS INC INNER JOIN TST_INCIDENT_STATUS IST
    ON     INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID INNER JOIN TST_PROJECT PRJ
    ON     INC.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
    AND    PRJ.IS_ACTIVE = 1
    AND	IST.IS_OPEN_STATUS = 1
    GROUP BY Age
    ORDER BY Age ASC
END
GO
