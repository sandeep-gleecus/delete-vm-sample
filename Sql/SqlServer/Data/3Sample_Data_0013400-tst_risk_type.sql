/***************************************************************
**	Insert script for table TST_RISK_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_RISK_TYPE ON; 

INSERT INTO TST_RISK_TYPE
(
RISK_TYPE_ID, NAME, IS_ACTIVE, IS_DEFAULT, PROJECT_TEMPLATE_ID, RISK_WORKFLOW_ID
)
VALUES
(
1, 'Business', 1, 1, 1, 1
),
(
2, 'Technical', 1, 0, 1, 1
),
(
3, 'Financial', 1, 0, 1, 1
),
(
4, 'Schedule', 1, 0, 1, 1
),
(
5, 'Other', 1, 0, 1, 1
),
(
6, 'Business', 1, 1, 2, 2
),
(
7, 'Technical', 1, 0, 2, 2
),
(
8, 'Financial', 1, 0, 2, 2
),
(
9, 'Schedule', 1, 0, 2, 2
),
(
10, 'Other', 1, 0, 2, 2
),
(
11, 'Business', 1, 1, 3, 3
),
(
12, 'Technical', 1, 0, 3, 3
),
(
13, 'Financial', 1, 0, 3, 3
),
(
14, 'Schedule', 1, 0, 3, 3
),
(
15, 'Other', 1, 0, 3, 3
),
(
16, 'Business', 1, 1, 4, 4
),
(
17, 'Technical', 1, 0, 4, 4
),
(
18, 'Financial', 1, 0, 4, 4
),
(
19, 'Schedule', 1, 0, 4, 4
),
(
20, 'Other', 1, 0, 4, 4
)
GO

SET IDENTITY_INSERT TST_RISK_TYPE OFF; 

