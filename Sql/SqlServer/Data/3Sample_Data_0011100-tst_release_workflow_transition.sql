/***************************************************************
**	Insert script for table TST_RELEASE_WORKFLOW_TRANSITION
***************************************************************/
SET IDENTITY_INSERT TST_RELEASE_WORKFLOW_TRANSITION ON; 

INSERT INTO TST_RELEASE_WORKFLOW_TRANSITION
(
WORKFLOW_TRANSITION_ID, RELEASE_WORKFLOW_ID, INPUT_RELEASE_STATUS_ID, OUTPUT_RELEASE_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED
)
VALUES
(
1, 1, 1, 2, 'Start Release', 1, 1, 0
),
(
2, 1, 2, 3, 'Finish Release', 1, 1, 0
),
(
3, 1, 3, 4, 'Close Release', 0, 1, 0
),
(
4, 1, 1, 5, 'Defer Release', 0, 1, 0
),
(
5, 1, 1, 6, 'Cancel Release', 0, 1, 0
),
(
6, 1, 2, 5, 'Defer Release', 0, 1, 0
),
(
7, 1, 2, 6, 'Cancel Release', 0, 1, 0
),
(
8, 1, 3, 2, 'Continue Release', 1, 1, 0
),
(
9, 1, 4, 3, 'Open Release', 1, 1, 0
),
(
10, 1, 5, 2, 'Continue Release', 1, 1, 0
),
(
11, 1, 6, 1, 'Uncancel Release', 1, 1, 0
),
(
12, 2, 1, 2, 'Start Release', 1, 1, 0
),
(
13, 2, 2, 3, 'Finish Release', 1, 1, 0
),
(
14, 2, 3, 4, 'Close Release', 0, 1, 0
),
(
15, 2, 1, 5, 'Defer Release', 0, 1, 0
),
(
16, 2, 1, 6, 'Cancel Release', 0, 1, 0
),
(
17, 2, 2, 5, 'Defer Release', 0, 1, 0
),
(
18, 2, 2, 6, 'Cancel Release', 0, 1, 0
),
(
19, 2, 3, 2, 'Continue Release', 1, 1, 0
),
(
20, 2, 4, 3, 'Open Release', 1, 1, 0
),
(
21, 2, 5, 2, 'Continue Release', 1, 1, 0
),
(
22, 2, 6, 1, 'Uncancel Release', 1, 1, 0
),
(
23, 3, 1, 2, 'Start Release', 1, 0, 0
),
(
24, 3, 2, 3, 'Finish Release', 0, 1, 1
),
(
25, 3, 3, 4, 'Close Release', 0, 0, 1
),
(
26, 3, 1, 5, 'Defer Release', 1, 0, 0
),
(
27, 3, 1, 6, 'Cancel Release', 1, 0, 0
),
(
28, 3, 2, 5, 'Defer Release', 0, 1, 0
),
(
29, 3, 2, 6, 'Cancel Release', 0, 1, 0
),
(
30, 3, 3, 2, 'Continue Release', 0, 0, 0
),
(
31, 3, 4, 3, 'Open Release', 0, 0, 1
),
(
32, 3, 5, 2, 'Continue Release', 0, 0, 0
),
(
33, 3, 6, 1, 'Uncancel Release', 0, 0, 0
),
(
34, 4, 1, 2, 'In Progress', 1, 1, 0
),
(
35, 4, 2, 3, 'Completed', 1, 1, 0
),
(
36, 4, 3, 4, 'Closed', 1, 1, 0
),
(
38, 4, 1, 6, 'Cancelled', 1, 1, 0
),
(
40, 4, 2, 6, 'Cancelled', 1, 1, 0
),
(
41, 4, 3, 2, 'In Progress', 1, 1, 0
),
(
42, 4, 4, 3, 'Completed', 1, 1, 0
),
(
43, 4, 5, 2, 'In Progress', 1, 1, 0
),
(
44, 4, 6, 1, 'Planned', 1, 1, 0
),
(
45, 4, 1, 3, 'Completed', 1, 1, 0
),
(
46, 4, 1, 4, 'Closed', 1, 1, 0
),
(
47, 4, 1, 5, 'Deferred', 1, 1, 1
),
(
48, 4, 2, 1, 'Planned', 1, 1, 0
),
(
49, 4, 2, 4, 'Closed', 1, 1, 0
),
(
50, 4, 2, 5, 'Deferred', 1, 1, 0
),
(
51, 4, 3, 1, 'Planned', 1, 1, 0
),
(
52, 4, 3, 5, 'Deferred', 1, 1, 0
),
(
53, 4, 3, 6, 'Cancelled', 1, 1, 0
),
(
54, 4, 4, 1, 'Planned', 1, 1, 0
),
(
55, 4, 4, 2, 'In Progress', 1, 1, 0
),
(
57, 4, 4, 5, 'Deferred', 1, 1, 0
),
(
58, 4, 4, 6, 'Cancelled', 1, 1, 0
),
(
59, 4, 5, 1, 'Planned', 1, 1, 0
),
(
60, 4, 5, 3, 'Completed', 1, 1, 0
),
(
61, 4, 5, 4, 'Closed', 1, 1, 0
),
(
62, 4, 5, 6, 'Cancelled', 1, 1, 0
),
(
63, 4, 6, 2, 'In Progress', 1, 1, 0
),
(
64, 4, 6, 3, 'Completed', 1, 1, 0
),
(
65, 4, 6, 4, 'Closed', 1, 1, 0
),
(
66, 4, 6, 5, 'Deferred', 1, 1, 0
)
GO

SET IDENTITY_INSERT TST_RELEASE_WORKFLOW_TRANSITION OFF; 

