/***************************************************************
**	Insert script for table TST_PROJECT_GROUP
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_GROUP ON; 

INSERT INTO TST_PROJECT_GROUP
(
PROJECT_GROUP_ID, NAME, DESCRIPTION, WEBSITE, IS_ACTIVE, IS_DEFAULT, PERCENT_COMPLETE, PROJECT_TEMPLATE_ID, PORTFOLIO_ID, START_DATE, END_DATE, REQUIREMENT_COUNT
)
VALUES
(
5, 'Clinical Trials', 'TBD', NULL, 1, 0, 0, 2, 2, NULL, NULL, 0
),
(
6, 'Medical Systems', 'TBD', NULL, 1, 0, 0, 2, 2, NULL, NULL, 0
),
(
7, 'Back Office', 'TBD', NULL, 1, 0, 0, 1, 3, NULL, NULL, 0
),
(
8, 'Customer Experience', 'TBD', NULL, 1, 0, 0, 1, 3, NULL, NULL, 0
),
(
9, 'Aviation Platform', 'TBD', NULL, 1, 0, 0, 2, 4, NULL, NULL, 0
),
(
10, 'Space Platform', 'TBD', NULL, 1, 0, 0, 2, 4, NULL, NULL, 0
),
(
11, 'Inventory Systems', 'TBD', NULL, 1, 0, 0, 2, 5, NULL, NULL, 0
),
(
12, 'Production Systems', 'TBD', NULL, 1, 0, 0, 2, 5, NULL, NULL, 0
)
GO

SET IDENTITY_INSERT TST_PROJECT_GROUP OFF; 

