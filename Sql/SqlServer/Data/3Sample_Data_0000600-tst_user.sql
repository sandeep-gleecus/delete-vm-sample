/***************************************************************
**	Insert script for table TST_USER
***************************************************************/
SET IDENTITY_INSERT TST_USER ON; 

INSERT INTO TST_USER
(
USER_ID, USER_NAME, PASSWORD, PASSWORD_SALT, EMAIL_ADDRESS, IS_ACTIVE, IS_APPROVED, IS_LOCKED, CREATION_DATE, LAST_LOGIN_DATE, PASSWORD_FORMAT, FAILED_PASSWORD_ATTEMPT_COUNT, FAILED_PASSWORD_ANSWER_ATTEMPT_COUNT, PASSWORD_QUESTION, PASSWORD_ANSWER, LDAP_DN, RSS_TOKEN, IS_LEGACY_FORMAT
)
VALUES
(
2, 'fredbloggs', 'EeZxIlwKF//cNP98N2M/RAIVnew=', 'lui8LATVu5iurfBcaRQjbQ==', 'fredbloggs@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), DATEADD(day, -1, SYSUTCDATETIME()), 1, 0, 0, 'What is 1+1?', '/YE8eBvAECNQD80jAQxJMabiO3s=', NULL, '{7A05FD06-83C3-4436-B37F-51BCF0060483}', 0
),
(
3, 'joesmith', 'EeZxIlwKF//cNP98N2M/RAIVnew=', 'lui8LATVu5iurfBcaRQjbQ==', 'joesmith@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 1, 0, 0, 'What is 1+1?', '/YE8eBvAECNQD80jAQxJMabiO3s=', NULL, '{7911E6B3-2C9E-4837-8B4E-96F3E2B37EFC}', 0
),
(
4, 'rogerramjet', 'BBA38D6AB96D97C2E2A617C240109A09', NULL, 'rogerramjet@mycompany.com', 0, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'What email address did you signup with?', '3CFC4B00A89915800C009B016F6FE7AF', NULL, '{FCEB9E59-7C99-4574-91EF-0734412D9711}', 1
),
(
5, 'rickypond', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'rickypond@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
6, 'donnaharkness', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'donnaharkness@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
7, 'jackvanstanten', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'jackvanstanten@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
8, 'rosesmith', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'rosesmith@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
9, 'roryjones', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'roryjones@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
10, 'marthanoble', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'marthanoble@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
11, 'bernardtyler', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'bernardtyler@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
12, 'henrycooper', 'mQoeYAffEiCBliN3avxK7LQDO/4=', 'lui8LATVu5iurfBcaRQjbQ==', 'henrycooper@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
),
(
13, 'amycribbins', 'EeZxIlwKF//cNP98N2M/RAIVnew=', 'lui8LATVu5iurfBcaRQjbQ==', 'amycribbins@mycompany.com', 1, 1, 0, DATEADD(day, -152, SYSUTCDATETIME()), NULL, 1, 0, 0, 'Not Set', 'EFiJ9PTeQ3M7pqq62W//GXwp2Wg=', NULL, NULL, 0
)
GO

SET IDENTITY_INSERT TST_USER OFF; 

