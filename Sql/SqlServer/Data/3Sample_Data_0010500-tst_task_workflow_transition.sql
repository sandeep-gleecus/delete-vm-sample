/***************************************************************
**	Insert script for table TST_TASK_WORKFLOW_TRANSITION
***************************************************************/
SET IDENTITY_INSERT TST_TASK_WORKFLOW_TRANSITION ON; 

INSERT INTO TST_TASK_WORKFLOW_TRANSITION
(
WORKFLOW_TRANSITION_ID, TASK_WORKFLOW_ID, INPUT_TASK_STATUS_ID, OUTPUT_TASK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED
)
VALUES
(
1, 1, 1, 2, 'Start Task', 0, 1, 0
),
(
2, 1, 1, 5, 'Defer Task', 1, 1, 0
),
(
3, 1, 2, 3, 'Complete Task', 0, 1, 0
),
(
4, 1, 2, 5, 'Defer Task', 1, 1, 0
),
(
5, 1, 2, 4, 'Block Task', 0, 1, 0
),
(
6, 1, 3, 2, 'Reopen Task', 1, 1, 0
),
(
7, 1, 5, 2, 'Resume Task', 1, 1, 0
),
(
8, 1, 4, 2, 'Unblock Task', 0, 1, 0
),
(
9, 2, 1, 2, 'Start Task', 0, 1, 0
),
(
10, 2, 1, 5, 'Defer Task', 1, 1, 0
),
(
11, 2, 2, 3, 'Complete Task', 0, 1, 0
),
(
12, 2, 2, 5, 'Defer Task', 1, 1, 0
),
(
13, 2, 2, 4, 'Block Task', 0, 1, 0
),
(
14, 2, 3, 2, 'Reopen Task', 1, 1, 0
),
(
15, 2, 5, 2, 'Resume Task', 1, 1, 0
),
(
16, 2, 4, 2, 'Unblock Task', 0, 1, 0
),
(
33, 1, 2, 1, 'Restart Development', 0, 1, 0
),
(
34, 1, 5, 1, 'Undefer Task', 0, 1, 0
),
(
35, 2, 2, 1, 'Restart Development', 0, 1, 0
),
(
36, 2, 5, 1, 'Undefer Task', 0, 1, 0
),
(
37, 3, 1, 2, 'Start Task', 0, 1, 0
),
(
38, 3, 1, 5, 'Defer Task', 1, 1, 0
),
(
39, 3, 2, 3, 'Complete Task', 0, 1, 0
),
(
40, 3, 2, 5, 'Defer Task', 0, 1, 0
),
(
41, 3, 2, 4, 'Block Task', 0, 1, 0
),
(
42, 3, 3, 2, 'Reopen Task', 0, 0, 0
),
(
43, 3, 5, 2, 'Resume Task', 0, 1, 0
),
(
44, 3, 4, 2, 'Unblock Task', 0, 1, 0
),
(
45, 3, 2, 1, 'Restart Development', 0, 1, 0
),
(
46, 3, 5, 1, 'Undefer Task', 0, 1, 0
),
(
47, 3, 1, 6, 'Reject Task', 0, 1, 0
),
(
48, 3, 6, 1, 'Reopen Task', 0, 0, 0
),
(
49, 3, 1, 7, 'Duplicate Task', 0, 1, 0
),
(
50, 3, 2, 7, 'Duplicate Task', 0, 1, 0
),
(
51, 3, 7, 1, 'Reopen Task', 0, 0, 0
),
(
52, 3, 3, 9, 'Mark as Obsolete', 0, 0, 0
),
(
53, 3, 9, 2, 'Reopen Task', 0, 0, 0
),
(
54, 3, 1, 8, 'Start Review', 1, 0, 0
),
(
55, 3, 8, 2, 'Start Task', 0, 1, 0
),
(
56, 4, 1, 2, 'In Progress', 1, 1, 0
),
(
58, 4, 2, 3, 'Completed', 1, 1, 0
),
(
61, 4, 3, 2, 'In Progress', 1, 1, 0
),
(
64, 4, 2, 1, 'Not Started', 1, 1, 0
),
(
66, 4, 4, 1, 'Return to Not Started', 0, 1, 0
),
(
67, 4, 5, 1, 'Return to Not Started', 0, 1, 0
),
(
69, 4, 7, 1, 'Return to Not Started', 0, 1, 0
),
(
70, 4, 8, 1, 'Return to Not Started', 0, 1, 0
),
(
71, 4, 9, 1, 'Return to Not Started', 0, 1, 0
),
(
72, 4, 1, 3, 'Completed', 1, 1, 0
),
(
73, 4, 3, 1, 'Not Started', 1, 1, 0
),
(
74, 4, 1, 6, 'Rejected', 1, 1, 0
),
(
75, 4, 2, 6, 'Rejected', 1, 1, 0
),
(
76, 4, 3, 6, 'Rejected', 1, 1, 0
),
(
77, 4, 6, 1, 'Not Started', 1, 1, 0
),
(
78, 4, 6, 2, 'In Progress', 1, 1, 0
),
(
79, 4, 6, 3, 'Completed', 1, 1, 0
)
GO

SET IDENTITY_INSERT TST_TASK_WORKFLOW_TRANSITION OFF; 

