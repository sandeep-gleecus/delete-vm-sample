/***************************************************************
**	Insert script for table TST_DOCUMENT_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_DOCUMENT_STATUS ON; 

INSERT INTO TST_DOCUMENT_STATUS
(
DOCUMENT_STATUS_ID, PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT
)
VALUES
(
1, 1, 'Draft', 1, 1, 1, 1
),
(
2, 1, 'Under Review', 2, 1, 1, 0
),
(
3, 1, 'Approved', 3, 1, 1, 0
),
(
4, 1, 'Completed', 4, 1, 0, 0
),
(
5, 1, 'Rejected', 5, 1, 0, 0
),
(
6, 1, 'Retired', 6, 1, 0, 0
),
(
13, 1, 'Checked Out', 7, 1, 1, 0
),
(
7, 2, 'Draft', 1, 1, 1, 1
),
(
8, 2, 'Under Review', 2, 1, 1, 0
),
(
9, 2, 'Approved', 3, 1, 1, 0
),
(
10, 2, 'Completed', 4, 1, 0, 0
),
(
11, 2, 'Rejected', 5, 1, 0, 0
),
(
12, 2, 'Retired', 6, 1, 0, 0
),
(
14, 2, 'Checked Out', 7, 1, 1, 0
),
(
15, 3, 'Draft', 1, 1, 1, 1
),
(
16, 3, 'Under Review', 2, 1, 1, 0
),
(
17, 3, 'Approved', 3, 1, 1, 0
),
(
18, 3, 'Completed', 4, 1, 0, 0
),
(
19, 3, 'Rejected', 5, 1, 0, 0
),
(
20, 3, 'Retired', 6, 1, 0, 0
),
(
21, 3, 'Checked Out', 7, 1, 1, 0
),
(
22, 4, 'Draft', 1, 1, 1, 1
),
(
23, 4, 'Under Review', 2, 0, 1, 0
),
(
24, 4, 'Approved', 3, 1, 1, 0
),
(
25, 4, 'Completed', 4, 1, 0, 0
),
(
26, 4, 'Rejected', 5, 0, 0, 0
),
(
27, 4, 'Retired', 6, 0, 0, 0
),
(
28, 4, 'Checked Out', 7, 1, 1, 0
)
GO

SET IDENTITY_INSERT TST_DOCUMENT_STATUS OFF; 

