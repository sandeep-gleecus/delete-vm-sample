/***************************************************************
**	Insert script for table TST_PROJECT_ATTACHMENT_FOLDER
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_ATTACHMENT_FOLDER ON; 

INSERT INTO TST_PROJECT_ATTACHMENT_FOLDER
(
PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, NAME
)
VALUES
(
1, 1, NULL, 'Root Folder'
),
(
2, 1, 1, 'Specifications'
),
(
3, 1, 1, 'Screen Captures'
),
(
4, 1, 1, 'Test Results'
),
(
5, 1, 4, 'Error Messages'
),
(
6, 1, 4, 'Web Links'
),
(
7, 1, 1, 'Design Documents'
),
(
8, 1, 1, 'Misc Documents'
),
(
9, 2, NULL, 'Root Folder'
),
(
10, 3, NULL, 'Root Folder'
),
(
11, 4, NULL, 'Root Folder'
),
(
12, 1, 1, 'Test Scripts'
),
(
14, 1, 1, 'CreateNewAuthor'
),
(
15, 1, 1, 'CreateNewBook'
),
(
16, 1, 1, 'EditExistingAuthor'
),
(
17, 1, 1, 'EditExistingBook'
),
(
18, 5, NULL, 'Root Folder'
),
(
19, 6, NULL, 'Root Folder'
),
(
20, 7, NULL, 'Root Folder'
),
(
21, 1, 1, 'Diagrams'
)
GO

SET IDENTITY_INSERT TST_PROJECT_ATTACHMENT_FOLDER OFF; 

