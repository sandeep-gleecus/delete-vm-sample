/***************************************************************
**	Insert script for table TST_TEST_CASE
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE ON; 

INSERT INTO TST_TEST_CASE
(
TEST_CASE_ID, NAME, DESCRIPTION, PROJECT_ID, TEST_CASE_FOLDER_ID, EXECUTION_STATUS_ID, AUTHOR_ID, OWNER_ID, TEST_CASE_STATUS_ID, TEST_CASE_TYPE_ID, CREATION_DATE, LAST_UPDATE_DATE, CONCURRENCY_DATE, EXECUTION_DATE, TEST_CASE_PRIORITY_ID, ESTIMATED_DURATION, ACTUAL_DURATION, IS_ATTACHMENTS, IS_TEST_STEPS, AUTOMATION_ENGINE_ID, AUTOMATION_ATTACHMENT_ID, COMPONENT_IDS, IS_SUSPECT
)
VALUES
(
2, 'Ability to create new book', 'Tests that the user can create a new book in the system', 1, 1, 1, 2, 2, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, 0, SYSUTCDATETIME()), 1, 10, 70, 1, 1, 1, 30, '1', 0
),
(
3, 'Ability to edit existing book', 'Tests that the user can login, view the details of a book, and then if he/she desires, make the necessary changes', 1, 1, 1, 2, 2, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, 0, SYSUTCDATETIME()), 1, 5, 70, 0, 1, 1, 44, '1', 0
),
(
4, 'Ability to create new author', 'Tests that the user can create a new author record in the system', 1, 1, 6, 2, 3, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, 0, SYSUTCDATETIME()), 1, 8, 70, 1, 1, 1, 23, '1', 0
),
(
5, 'Ability to edit existing author', 'Tests that the user can login, view the details of an author and then if he/she desires, make the necessary changes', 1, 1, 6, 2, 3, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -10, SYSUTCDATETIME()), 2, 5, 65, 0, 1, 1, 37, '2', 0
),
(
6, 'Ability to reassign book to different author', NULL, 1, 1, 2, 2, 3, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 2, 8, 90, 0, 0, NULL, NULL, '2', 0
),
(
8, 'Book management', NULL, 1, 2, 2, 3, NULL, 5, 7, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -10, SYSUTCDATETIME()), 2, 4, 0, 0, 1, 2, 16, '1,3', 0
),
(
9, 'Author management', NULL, 1, 2, 2, 3, NULL, 5, 7, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -9, SYSUTCDATETIME()), 2, 4, 90, 0, 1, 3, 17, '2,3', 0
),
(
12, 'Person loses book and needs to report loss', NULL, 1, 4, 3, 3, NULL, 2, 8, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, 3, NULL, NULL, 0, 1, NULL, NULL, '1', 0
),
(
13, 'Adding new book and author to library', NULL, 1, 4, 2, 3, NULL, 5, 12, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 3, NULL, 1, 0, 1, NULL, NULL, '1,2', 0
),
(
16, 'Open Up Web Browser', 'Tests that the user can get to the URL', 1, 5, 3, 2, NULL, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, NULL, 0
),
(
17, 'Login to Application', 'Tests that the user can login successfully', 1, 5, 3, 2, NULL, 5, 3, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, NULL, 0
),
(
18, 'Adding new author and book', 'This test case tests that you can add a <b>new author</b> and <b>new book</b> at the same time in <i>one session</i>.<br>', 1, 3, 3, 2, NULL, 1, 8, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, '1,2', 0
),
(
19, 'Adding multiple new books', NULL, 1, 3, 3, 2, NULL, 1, 8, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, '1', 0
),
(
20, 'Create Author', NULL, 1, 5, 3, 3, NULL, 1, 8, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, '2', 0
),
(
21, 'Create Book', NULL, 1, 5, 3, 3, NULL, 1, 8, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 1, NULL, NULL, '1', 0
)
GO

SET IDENTITY_INSERT TST_TEST_CASE OFF; 

