/***************************************************************
**	Insert script for table TST_THEME_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_THEME_STATUS ON; 

INSERT INTO TST_THEME_STATUS
(
THEME_STATUS_ID, NAME, POSITION, IS_ACTIVE
)
VALUES
(
1, 'Requested', 1, 1
),
(
2, 'Planned', 5, 1
),
(
3, 'In Progress', 6, 1
),
(
4, 'Developed', 7, 1
),
(
5, 'Accepted', 4, 1
),
(
6, 'Rejected', 3, 1
),
(
7, 'Under Review', 2, 1
),
(
8, 'Obsolete', 10, 1
),
(
9, 'Tested', 8, 1
),
(
10, 'Completed', 9, 1
)
GO

SET IDENTITY_INSERT TST_THEME_STATUS OFF; 

