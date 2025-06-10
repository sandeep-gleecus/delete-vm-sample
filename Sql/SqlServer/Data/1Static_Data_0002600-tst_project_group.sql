/***************************************************************
**	Insert script for table TST_PROJECT_GROUP
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_GROUP ON; 

INSERT INTO TST_PROJECT_GROUP
(
PROJECT_GROUP_ID, NAME, DESCRIPTION, WEBSITE, IS_ACTIVE, IS_DEFAULT, PERCENT_COMPLETE, REQUIREMENT_COUNT
)
VALUES
(
1, '(Default Program)', 'The default program that installs with the system.', NULL, 1, 1, 0, 0
)
GO

SET IDENTITY_INSERT TST_PROJECT_GROUP OFF; 

