/***************************************************************
**	Insert script for table TST_PROJECT_GROUP_ROLE
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_GROUP_ROLE ON; 

INSERT INTO TST_PROJECT_GROUP_ROLE
(
PROJECT_GROUP_ROLE_ID, NAME, DESCRIPTION, ACTIVE_YN
)
VALUES
(
1, 'Program Owner', 'Can add/remove projects to the program as well as view the program dashboard', 'Y'
),
(
2, 'Executive', 'Can view the program dashboard as well as view any of the projects in the program', 'Y'
)
GO

SET IDENTITY_INSERT TST_PROJECT_GROUP_ROLE OFF; 

