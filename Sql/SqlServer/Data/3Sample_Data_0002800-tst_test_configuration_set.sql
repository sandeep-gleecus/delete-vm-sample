/***************************************************************
**	Insert script for table TST_TEST_CONFIGURATION_SET
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CONFIGURATION_SET ON; 

INSERT INTO TST_TEST_CONFIGURATION_SET
(
TEST_CONFIGURATION_SET_ID, PROJECT_ID, NAME, DESCRIPTION, IS_ACTIVE, IS_DELETED, CREATION_DATE, LAST_UPDATED_DATE, CONCURRENCY_DATE
)
VALUES
(
1, 1, 'Target web browsers and operating systems', 'This set of data consists of all the web browsers and operating systems that the application needs to be tested with', 1, 0, DATEADD(day, -151, SYSUTCDATETIME()), DATEADD(day, -151, SYSUTCDATETIME()), DATEADD(day, -151, SYSUTCDATETIME())
),
(
2, 1, 'List of library information system logins and passwords', NULL, 1, 0, DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -150, SYSUTCDATETIME())
),
(
3, 1, 'Complete testing data, with browsers, operating systems and logins', 'This is the complete set of test data that we need to use. It comprises the logins, web browsers and operating systems', 1, 0, DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -150, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_TEST_CONFIGURATION_SET OFF; 

