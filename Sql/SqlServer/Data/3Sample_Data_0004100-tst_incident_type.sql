/***************************************************************
**	Insert script for table TST_INCIDENT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_INCIDENT_TYPE ON; 

INSERT INTO TST_INCIDENT_TYPE
(
INCIDENT_TYPE_ID, PROJECT_TEMPLATE_ID, WORKFLOW_ID, NAME, IS_ACTIVE, IS_ISSUE, IS_RISK, IS_DEFAULT
)
VALUES
(
1, '1', 1, 'Incident', 1, 0, 0, 1
),
(
2, '1', 1, 'Bug', 1, 0, 0, 0
),
(
3, '1', 1, 'Enhancement', 1, 0, 0, 0
),
(
4, '1', 1, 'Issue', 1, 1, 0, 0
),
(
5, '1', 1, 'Training', 1, 0, 0, 0
),
(
6, '1', 1, 'Limitation', 1, 0, 0, 0
),
(
7, '1', 1, 'Change Request', 1, 0, 0, 0
),
(
8, '1', 1, 'Problem', 1, 0, 1, 0
),
(
9, '2', 2, 'Incident', 1, 0, 0, 1
),
(
10, '2', 2, 'Bug', 1, 0, 0, 0
),
(
11, '2', 2, 'Enhancement', 1, 0, 0, 0
),
(
12, '2', 2, 'Issue', 1, 1, 0, 0
),
(
13, '2', 2, 'Training', 1, 0, 0, 0
),
(
14, '2', 2, 'Limitation', 1, 0, 0, 0
),
(
15, '2', 2, 'Change Request', 1, 0, 0, 0
),
(
16, '2', 2, 'Problem', 1, 0, 1, 0
),
(
17, '3', 3, 'Incident', 1, 0, 0, 1
),
(
18, '3', 3, 'Bug', 1, 0, 0, 0
),
(
19, '3', 3, 'Enhancement', 1, 0, 0, 0
),
(
20, '3', 3, 'Issue', 1, 1, 0, 0
),
(
21, '3', 3, 'Training', 1, 0, 0, 0
),
(
22, '3', 3, 'Limitation', 1, 0, 0, 0
),
(
23, '3', 3, 'Change Request', 1, 0, 0, 0
),
(
24, '3', 3, 'Problem', 1, 0, 1, 0
),
(
25, '4', 4, 'Incident', 1, 0, 0, 1
),
(
26, '4', 4, 'Bug', 1, 0, 0, 0
),
(
27, '4', 4, 'Enhancement', 1, 0, 0, 0
),
(
28, '4', 4, 'Issue', 1, 1, 0, 0
),
(
29, '4', 4, 'Training', 1, 0, 0, 0
),
(
30, '4', 4, 'Limitation', 1, 0, 0, 0
),
(
31, '4', 4, 'Change Request', 1, 0, 0, 0
),
(
32, '4', 4, 'Problem', 1, 0, 1, 0
)
GO

SET IDENTITY_INSERT TST_INCIDENT_TYPE OFF; 

