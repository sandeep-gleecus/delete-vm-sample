/***************************************************************
**	Insert script for table TST_ATTACHMENT
***************************************************************/
SET IDENTITY_INSERT TST_ATTACHMENT ON; 

INSERT INTO TST_ATTACHMENT
(
ATTACHMENT_ID, ATTACHMENT_TYPE_ID, AUTHOR_ID, EDITOR_ID, DOCUMENT_STATUS_ID, FILENAME, DESCRIPTION, UPLOAD_DATE, EDITED_DATE, SIZE, CURRENT_VERSION, CONCURRENCY_DATE
)
VALUES
(
1, 1, 2, 3, 3, 'Book Management Functional Spec.doc', 'This document outlines the functional specification for the book management part of the library management system.', DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -130, SYSUTCDATETIME()), 285, '2', DATEADD(day, -129, SYSUTCDATETIME())
),
(
2, 1, 2, 2, 2, 'Library System Performance Metrics.xls', NULL, DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 15, '1.1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
3, 1, 2, 2, 4, 'Error Logging-in Screen-shot.gif', 'Captured screen-shot of the error that was raised when attempting to log in to the library application', DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -148, SYSUTCDATETIME()), 48, '1', DATEADD(day, -148, SYSUTCDATETIME())
),
(
4, 1, 2, 2, 4, 'Error Stacktrace.doc', 'Stack trace captured from error on page', DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -148, SYSUTCDATETIME()), 24, '1', DATEADD(day, -148, SYSUTCDATETIME())
),
(
5, 1, 2, 2, 4, 'Cannot Sort Screenshot.ppt', NULL, DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -148, SYSUTCDATETIME()), 181, '1', DATEADD(day, -148, SYSUTCDATETIME())
),
(
6, 1, 2, 2, 4, 'Web Page capture.htm', 'Captured web-page HTML from page display when rendered on the Mozilla browser', DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -148, SYSUTCDATETIME()), 88, '1', DATEADD(day, -148, SYSUTCDATETIME())
),
(
7, 1, 2, 3, 4, 'Sequence Diagram for Book Mgt.pdf', 'Sequence diagram in UML format that provides additional detail surrounding the book managament use-case / test case', DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -141, SYSUTCDATETIME()), 35, '1.2', DATEADD(day, -141, SYSUTCDATETIME())
),
(
8, 1, 3, 2, 4, 'Author Management Screen Wireframe.vsd', 'Outlines the user interface design for the author management screens minus the graphical branding', DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 533, '2.1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
9, 1, 3, 3, 4, 'Date Editing Screenshot.jpg', 'Screenshot when editing the data in the system. Ilustrates the lack of usability in the current approach', DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -143, SYSUTCDATETIME()), 281, '1', DATEADD(day, -143, SYSUTCDATETIME())
),
(
10, 1, 3, 3, 4, 'Bug Stack Trace.txt', NULL, DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -143, SYSUTCDATETIME()), 1, '1', DATEADD(day, -143, SYSUTCDATETIME())
),
(
11, 1, 3, 2, 1, 'Book Management Screen Wireframe.ai', NULL, DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -114, SYSUTCDATETIME()), 392, '1.1', DATEADD(day, -114, SYSUTCDATETIME())
),
(
12, 1, 2, 2, 4, 'Use Case Diagram.vsd', 'UML use case diagram that provides additional information on the use case and associated test steps', DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -143, SYSUTCDATETIME()), 43, '1', DATEADD(day, -143, SYSUTCDATETIME())
),
(
13, 1, 3, 3, 4, 'Graphical Design Mockups.psd', NULL, DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -143, SYSUTCDATETIME()), 1009, '1', DATEADD(day, -143, SYSUTCDATETIME())
),
(
14, 1, 2, 2, 4, 'Expected Result Screenshot.png', 'Captured screen shot of how we expect the page to look', DATEADD(day, -129, SYSUTCDATETIME()), DATEADD(day, -129, SYSUTCDATETIME()), 314, '1', DATEADD(day, -129, SYSUTCDATETIME())
),
(
15, 2, 2, 2, 4, 'http://www.inflectra.com', 'URL to related information', DATEADD(day, -129, SYSUTCDATETIME()), DATEADD(day, -129, SYSUTCDATETIME()), 0, '1', DATEADD(day, -129, SYSUTCDATETIME())
),
(
16, 2, 2, 2, 4, 'file:///c:/users/fredbloggs/appdata/local/test_scripts/test_1', NULL, DATEADD(day, -129, SYSUTCDATETIME()), DATEADD(day, -129, SYSUTCDATETIME()), 0, '1', DATEADD(day, -129, SYSUTCDATETIME())
),
(
17, 1, 2, 2, 4, 'Web 01 SmarteATM Login.ses', NULL, DATEADD(day, -129, SYSUTCDATETIME()), DATEADD(day, -129, SYSUTCDATETIME()), 100, '1', DATEADD(day, -129, SYSUTCDATETIME())
),
(
19, 1, 2, 2, 4, 'Repository.json', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 1, '2', DATEADD(day, -112, SYSUTCDATETIME())
),
(
20, 1, 2, 2, 4, 'CreateNewAuthor.js', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 1, '1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
21, 1, 2, 2, 4, 'CreateNewAuthor.objects.js', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 11, '1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
22, 1, 2, 2, 4, 'CreateNewAuthor.objects.metadata', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 87, '1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
23, 1, 2, 2, 4, 'CreateNewAuthor.sstest', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 2, '1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
24, 1, 2, 2, 4, 'CreateNewAuthor.user.js', NULL, DATEADD(day, -112, SYSUTCDATETIME()), DATEADD(day, -112, SYSUTCDATETIME()), 1, '1', DATEADD(day, -112, SYSUTCDATETIME())
),
(
26, 1, 2, 2, 4, 'Repository.json', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 1, '2', DATEADD(day, -107, SYSUTCDATETIME())
),
(
27, 1, 2, 2, 4, 'CreateNewBook.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', DATEADD(day, -107, SYSUTCDATETIME())
),
(
28, 1, 2, 2, 4, 'CreateNewBook.objects.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 10, '1', DATEADD(day, -107, SYSUTCDATETIME())
),
(
29, 1, 2, 2, 4, 'CreateNewBook.objects.metadata', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 87, '1', DATEADD(day, -107, SYSUTCDATETIME())
),
(
30, 1, 2, 2, 4, 'CreateNewBook.sstest', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', DATEADD(day, -107, SYSUTCDATETIME())
),
(
31, 1, 2, 2, 4, 'CreateNewBook.user.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', DATEADD(day, -107, SYSUTCDATETIME())
),
(
33, 1, 2, 2, 4, 'Repository.json', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 1, '2', DATEADD(day, -106, SYSUTCDATETIME())
),
(
34, 1, 2, 2, 4, 'EditExistingAuthor.js', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 1, '1', DATEADD(day, -106, SYSUTCDATETIME())
),
(
35, 1, 2, 2, 4, 'EditExistingAuthor.objects.js', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 11, '1', DATEADD(day, -106, SYSUTCDATETIME())
),
(
36, 1, 2, 2, 4, 'EditExistingAuthor.objects.metadata', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 87, '1', DATEADD(day, -106, SYSUTCDATETIME())
),
(
37, 1, 2, 2, 4, 'EditExistingAuthor.sstest', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 2, '1', DATEADD(day, -106, SYSUTCDATETIME())
),
(
38, 1, 2, 2, 4, 'EditExistingAuthor.user.js', NULL, DATEADD(day, -106, SYSUTCDATETIME()), DATEADD(day, -106, SYSUTCDATETIME()), 1, '1', DATEADD(day, -106, SYSUTCDATETIME())
),
(
40, 1, 2, 2, 4, 'Repository.json', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 1, '2', DATEADD(day, -102, SYSUTCDATETIME())
),
(
41, 1, 2, 2, 4, 'EditExistingBook.js', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 1, '1', DATEADD(day, -102, SYSUTCDATETIME())
),
(
42, 1, 2, 2, 4, 'EditExistingBook.objects.js', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 10, '1', DATEADD(day, -102, SYSUTCDATETIME())
),
(
43, 1, 2, 2, 4, 'EditExistingBook.objects.metadata', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 87, '1', DATEADD(day, -102, SYSUTCDATETIME())
),
(
44, 1, 2, 2, 4, 'EditExistingBook.sstest', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 2, '1', DATEADD(day, -102, SYSUTCDATETIME())
),
(
45, 1, 2, 2, 4, 'EditExistingBook.user.js', NULL, DATEADD(day, -102, SYSUTCDATETIME()), DATEADD(day, -102, SYSUTCDATETIME()), 2, '1', DATEADD(day, -102, SYSUTCDATETIME())
),
(
46, 1, 2, 2, 1, 'Test Plan.md', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', DATEADD(day, -103, SYSUTCDATETIME())
),
(
47, 1, 2, 2, 1, 'Test Areas.html', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', DATEADD(day, -103, SYSUTCDATETIME())
),
(
48, 1, 2, 2, 1, 'Create Author Scenario.feature', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', DATEADD(day, -103, SYSUTCDATETIME())
),
(
49, 1, 2, 2, 1, 'Library Brainstorm.mindmap', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '8', DATEADD(day, -103, SYSUTCDATETIME())
),
(
50, 1, 2, 2, 1, 'Library Requirements Overview.orgchart', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '5', DATEADD(day, -103, SYSUTCDATETIME())
),
(
51, 1, 2, 2, 1, 'Requirements Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '9', DATEADD(day, -103, SYSUTCDATETIME())
),
(
52, 1, 2, 2, 1, 'Releases Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '5', DATEADD(day, -103, SYSUTCDATETIME())
),
(
53, 1, 2, 2, 1, 'Documents Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '6', DATEADD(day, -103, SYSUTCDATETIME())
),
(
54, 1, 2, 2, 1, 'Test Cases Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '5', DATEADD(day, -103, SYSUTCDATETIME())
),
(
55, 1, 2, 2, 1, 'Incidents Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '8', DATEADD(day, -103, SYSUTCDATETIME())
),
(
56, 1, 2, 2, 1, 'Tasks Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '4', DATEADD(day, -103, SYSUTCDATETIME())
),
(
57, 1, 2, 2, 1, 'Risks Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), DATEADD(day, -103, SYSUTCDATETIME()), 1, '5', DATEADD(day, -103, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_ATTACHMENT OFF; 

