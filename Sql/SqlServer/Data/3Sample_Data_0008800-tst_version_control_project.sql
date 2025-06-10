/***************************************************************
**	Insert script for table TST_VERSION_CONTROL_PROJECT
***************************************************************/
INSERT INTO TST_VERSION_CONTROL_PROJECT
(
VERSION_CONTROL_SYSTEM_ID, PROJECT_ID, IS_ACTIVE, CONNECTION_STRING, LOGIN, PASSWORD, DOMAIN, CUSTOM_01, CUSTOM_02, CUSTOM_03, CUSTOM_04, CUSTOM_05
)
VALUES
(
1, 1, 1, 'test://MyRepository', 'joesmith', 'joesmith', 'MyCompany', 'testvalue', NULL, NULL, NULL, NULL
),
(
1, 2, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
1, 3, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
)
GO

