/***************************************************************
**	Insert script for table TST_THEME_PRIORITY
***************************************************************/
SET IDENTITY_INSERT TST_THEME_PRIORITY ON; 

INSERT INTO TST_THEME_PRIORITY
(
THEME_PRIORITY_ID, NAME, COLOR, IS_ACTIVE, SCORE
)
VALUES
(
1, '1 - Critical', 'f47457', 1, 1
),
(
2, '2 - High', 'f29e56', 1, 2
),
(
3, '3 - Medium', 'f5d857', 1, 3
),
(
4, '4 - Low', 'f4f356', 1, 4
)
GO

SET IDENTITY_INSERT TST_THEME_PRIORITY OFF; 

