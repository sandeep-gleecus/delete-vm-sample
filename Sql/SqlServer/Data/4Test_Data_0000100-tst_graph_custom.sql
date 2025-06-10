/***************************************************************
**	Insert script for table TST_GRAPH_CUSTOM
***************************************************************/
SET IDENTITY_INSERT TST_GRAPH_CUSTOM ON; 

INSERT INTO TST_GRAPH_CUSTOM
(
GRAPH_ID, GRAPH_TYPE_ID, NAME, POSITION, DESCRIPTION, IS_ACTIVE, QUERY
)
VALUES
(
1000, 4, 'Test Graph 1', 1, NULL, 1, 'select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId}
group by R.EXECUTION_STATUS_NAME'
),
(
1001, 4, 'Test Graph 2', 2, NULL, 1, 'select R.INCIDENT_STATUS_NAME, COUNT (R.INCIDENT_ID) as COUNT
from SpiraTestEntities.R_Incidents as R
where R.PROJECT_ID = ${ProjectId}
group by R.INCIDENT_STATUS_NAME
'
)
GO

SET IDENTITY_INSERT TST_GRAPH_CUSTOM OFF; 

