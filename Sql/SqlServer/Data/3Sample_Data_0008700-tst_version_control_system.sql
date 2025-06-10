/***************************************************************
**	Insert script for table TST_VERSION_CONTROL_SYSTEM
***************************************************************/
SET IDENTITY_INSERT TST_VERSION_CONTROL_SYSTEM ON; 

INSERT INTO TST_VERSION_CONTROL_SYSTEM
(
VERSION_CONTROL_SYSTEM_ID, NAME, DESCRIPTION, IS_ACTIVE, CONNECTION_STRING, LOGIN, PASSWORD, DOMAIN, CUSTOM_01, CUSTOM_02, CUSTOM_03, CUSTOM_04, CUSTOM_05
)
VALUES
(
1, 'TestVersionControlProvider2', 'This provides the dummy version control provider used in testing', 1, 'test://MyRepository', 'fredbloggs', 'fredbloggs', NULL, NULL, NULL, NULL, NULL, NULL
)
GO

SET IDENTITY_INSERT TST_VERSION_CONTROL_SYSTEM OFF; 

