/***************************************************************
**	Insert script for table TST_TASK_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_TASK_TYPE ON; 

INSERT INTO TST_TASK_TYPE
(
TASK_TYPE_ID, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, NAME, POSITION, IS_ACTIVE, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST
)
VALUES
(
1, 1, 1, 'Development', 1, 1, 1, 0, 0
),
(
2, 1, 1, 'Testing', 2, 1, 0, 0, 0
),
(
3, 1, 1, 'Management', 3, 1, 0, 0, 0
),
(
4, 1, 1, 'Infrastructure', 4, 1, 0, 0, 0
),
(
5, 1, 1, 'Other', 5, 1, 0, 0, 0
),
(
6, 2, 2, 'Development', 1, 1, 1, 0, 0
),
(
7, 2, 2, 'Testing', 2, 1, 0, 0, 0
),
(
8, 2, 2, 'Management', 3, 1, 0, 0, 0
),
(
9, 2, 2, 'Infrastructure', 4, 1, 0, 0, 0
),
(
10, 2, 2, 'Other', 5, 1, 0, 0, 0
),
(
11, 1, 1, 'Code Review', 6, 1, 0, 1, 0
),
(
12, 2, 2, 'Code Review', 6, 1, 0, 1, 0
),
(
13, 1, 1, 'Pull Request', 7, 1, 0, 0, 1
),
(
14, 2, 2, 'Pull Request', 7, 1, 0, 0, 1
),
(
15, 3, 3, 'Development', 0, 1, 1, 0, 0
),
(
16, 3, 3, 'Testing', 0, 1, 0, 0, 0
),
(
17, 3, 3, 'Management', 0, 1, 0, 0, 0
),
(
18, 3, 3, 'Infrastructure', 0, 1, 0, 0, 0
),
(
19, 3, 3, 'Other', 0, 1, 0, 0, 0
),
(
20, 3, 3, 'Code Review', 0, 1, 0, 0, 0
),
(
21, 3, 3, 'Pull Request', 0, 1, 0, 0, 0
),
(
22, 4, 4, 'Development', 0, 1, 1, 0, 0
),
(
23, 4, 4, 'Testing', 0, 1, 0, 0, 0
),
(
24, 4, 4, 'Management', 0, 1, 0, 0, 0
),
(
25, 4, 4, 'Infrastructure', 0, 1, 0, 0, 0
),
(
26, 4, 4, 'Other', 0, 1, 0, 0, 0
),
(
27, 4, 4, 'Code Review', 0, 1, 0, 0, 0
),
(
28, 4, 4, 'Pull Request', 0, 1, 0, 0, 0
)
GO

SET IDENTITY_INSERT TST_TASK_TYPE OFF; 

