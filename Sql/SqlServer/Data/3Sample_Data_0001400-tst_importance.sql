/***************************************************************
**	Insert script for table TST_IMPORTANCE
***************************************************************/
SET IDENTITY_INSERT TST_IMPORTANCE ON; 

INSERT INTO TST_IMPORTANCE
(
IMPORTANCE_ID, PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE
)
VALUES
(
1, 1, '1 - Critical', 1, 'f47457', 1
),
(
2, 1, '2 - High', 1, 'f29e56', 2
),
(
3, 1, '3 - Medium', 1, 'f5d857', 3
),
(
4, 1, '4 - Low', 1, 'f4f356', 4
),
(
5, 2, '1 - Critical', 1, 'f47457', 1
),
(
6, 2, '2 - High', 1, 'f29e56', 2
),
(
7, 2, '3 - Medium', 1, 'f5d857', 3
),
(
8, 2, '4 - Low', 1, 'f4f356', 4
),
(
9, 3, '1 - Critical', 1, 'f47457', 1
),
(
10, 3, '2 - High', 1, 'f29e56', 2
),
(
11, 3, '3 - Medium', 1, 'f5d857', 3
),
(
12, 3, '4 - Low', 1, 'f4f356', 4
),
(
13, 4, '1 - Critical', 1, 'f47457', 1
),
(
14, 4, '2 - High', 1, 'f29e56', 2
),
(
15, 4, '3 - Medium', 1, 'f5d857', 3
),
(
16, 4, '4 - Low', 1, 'f4f356', 4
)
GO

SET IDENTITY_INSERT TST_IMPORTANCE OFF; 

