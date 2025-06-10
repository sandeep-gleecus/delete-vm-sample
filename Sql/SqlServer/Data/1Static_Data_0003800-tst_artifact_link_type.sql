/***************************************************************
**	Insert script for table TST_ARTIFACT_LINK_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_LINK_TYPE ON; 

INSERT INTO TST_ARTIFACT_LINK_TYPE
(
ARTIFACT_LINK_TYPE_ID, NAME, REVERSE_NAME, IS_ACTIVE
)
VALUES
(
1, 'Related-to', 'Related-to', 1
),
(
2, 'Depends-on', 'Prerequisite-for', 1
),
(
3, 'Implicit', 'Implicit', 0
),
(
4, 'Source Code Commit', 'Source Code Commit', 0
),
(
5, 'Gantt: Finish-to-Start', 'Gantt: Finish-to-Start', 1
),
(
6, 'Gantt: Start-to-Start', 'Gantt: Start-to-Start', 1
),
(
7, 'Gantt: Finish-to-Finish', 'Gantt: Finish-to-Finish', 1
),
(
8, 'Gantt: Start-to-Finish', 'Gantt: Start-to-Finish', 1
)
GO

SET IDENTITY_INSERT TST_ARTIFACT_LINK_TYPE OFF; 

