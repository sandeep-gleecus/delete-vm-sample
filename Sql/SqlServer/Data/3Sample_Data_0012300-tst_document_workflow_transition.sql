/***************************************************************
**	Insert script for table TST_DOCUMENT_WORKFLOW_TRANSITION
***************************************************************/
SET IDENTITY_INSERT TST_DOCUMENT_WORKFLOW_TRANSITION ON; 

INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION
(
WORKFLOW_TRANSITION_ID, DOCUMENT_WORKFLOW_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID
)
VALUES
(
1, 1, 'Review Document', 0, 1, 0, 1, 2
),
(
2, 1, 'Return to Draft', 1, 1, 0, 2, 1
),
(
3, 1, 'Approve Document', 0, 1, 0, 2, 3
),
(
4, 1, 'Reject Document', 0, 1, 0, 2, 5
),
(
5, 1, 'Complete Document', 0, 1, 0, 3, 4
),
(
6, 1, 'Return to Review', 1, 1, 0, 3, 2
),
(
7, 1, 'Return to Review', 1, 1, 0, 5, 2
),
(
8, 1, 'Retire Document', 1, 1, 0, 4, 6
),
(
9, 1, 'Return to Review', 1, 1, 0, 6, 2
),
(
10, 1, 'Return to Review', 1, 1, 0, 4, 2
),
(
11, 2, 'Review Document', 0, 1, 0, 7, 8
),
(
12, 2, 'Return to Draft', 1, 1, 0, 8, 7
),
(
13, 2, 'Approve Document', 0, 1, 0, 8, 9
),
(
14, 2, 'Reject Document', 0, 1, 0, 8, 11
),
(
15, 2, 'Complete Document', 0, 1, 0, 9, 10
),
(
16, 2, 'Return to Review', 1, 1, 0, 9, 8
),
(
17, 2, 'Return to Review', 1, 1, 0, 11, 8
),
(
18, 2, 'Retire Document', 1, 1, 0, 10, 12
),
(
19, 2, 'Return to Review', 1, 1, 0, 12, 8
),
(
20, 2, 'Return to Review', 1, 1, 0, 10, 8
),
(
21, 1, 'Checkout', 0, 1, 0, 3, 13
),
(
22, 2, 'Checkout', 0, 1, 0, 9, 14
),
(
23, 1, 'Checkin', 0, 1, 0, 13, 3
),
(
24, 2, 'Checkin', 0, 1, 0, 14, 9
),
(
25, 3, 'Review Document', 1, 0, 0, 15, 16
),
(
26, 3, 'Approve Document', 0, 1, 1, 16, 17
),
(
27, 3, 'Reject Document', 0, 1, 0, 16, 19
),
(
28, 3, 'Return to Draft', 0, 1, 0, 16, 15
),
(
29, 3, 'Checkout', 0, 1, 0, 17, 21
),
(
30, 3, 'Complete Document', 0, 1, 1, 17, 18
),
(
31, 3, 'Return to Draft', 0, 1, 0, 17, 16
),
(
32, 3, 'Retire Document', 0, 1, 1, 18, 20
),
(
33, 3, 'Return to Review', 0, 1, 0, 18, 16
),
(
34, 3, 'Return to Review', 0, 1, 0, 19, 16
),
(
35, 3, 'Return to Review', 0, 1, 0, 20, 16
),
(
36, 3, 'Checkin', 0, 1, 0, 21, 17
),
(
37, 4, 'Review Document', 0, 1, 0, 22, 23
),
(
41, 4, 'Checkout', 1, 1, 0, 24, 28
),
(
42, 4, 'Completed', 1, 1, 0, 24, 25
),
(
48, 4, 'Checkin', 1, 1, 0, 28, 24
),
(
49, 4, 'Approved', 1, 1, 0, 22, 24
),
(
52, 4, 'Completed', 1, 1, 0, 22, 25
),
(
53, 4, 'Checkout', 1, 1, 0, 22, 28
),
(
54, 4, 'Draft', 1, 1, 0, 24, 22
),
(
55, 4, 'Draft', 1, 1, 0, 25, 22
),
(
56, 4, 'Approved', 1, 1, 0, 25, 24
),
(
57, 4, 'Checkout', 1, 1, 0, 25, 28
),
(
58, 4, 'Draft', 1, 1, 0, 28, 22
),
(
59, 4, 'Completed', 1, 1, 0, 28, 25
)
GO

SET IDENTITY_INSERT TST_DOCUMENT_WORKFLOW_TRANSITION OFF; 

