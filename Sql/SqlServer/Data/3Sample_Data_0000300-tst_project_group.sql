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
2, 'Sample Program', 'Contains products related to customers, relationships and contacts', 'www.libraryinformationsystem.org', 1, 0, 68, 1, NULL, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, 35, SYSUTCDATETIME()), 22
),
(
3, 'Corporate Systems', 'TBD', NULL, 1, 0, 39, 1, 1, DATEADD(day, -220, SYSUTCDATETIME()), DATEADD(day, 266, SYSUTCDATETIME()), 87
),
(
4, 'Sales and Marketing', 'TBD', NULL, 1, 0, 15, 1, 1, DATEADD(day, -90, SYSUTCDATETIME()), DATEADD(day, 275, SYSUTCDATETIME()), 148
)
GO

SET IDENTITY_INSERT TST_PROJECT_GROUP OFF; 

