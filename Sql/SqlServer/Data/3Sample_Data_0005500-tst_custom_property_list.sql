/***************************************************************
**	Insert script for table TST_CUSTOM_PROPERTY_LIST
***************************************************************/
SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_LIST ON; 

INSERT INTO TST_CUSTOM_PROPERTY_LIST
(
CUSTOM_PROPERTY_LIST_ID, PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_SORTED_ON_VALUE
)
VALUES
(
1, 1, 'Difficulty', 1, 1
),
(
2, 1, 'Classification', 1, 1
),
(
3, 1, 'Test Type', 1, 1
),
(
4, 1, 'Operating System', 1, 1
),
(
5, 1, 'Web Browser', 1, 1
),
(
6, 1, 'Step Type', 1, 1
),
(
7, 1, 'Logins', 1, 1
),
(
8, 1, 'Passwords', 1, 1
)
GO

SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_LIST OFF; 

