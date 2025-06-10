/***************************************************************
**	Insert script for table TST_ATTACHMENT_VERSION
***************************************************************/
SET IDENTITY_INSERT TST_ATTACHMENT_VERSION ON; 

INSERT INTO TST_ATTACHMENT_VERSION
(
ATTACHMENT_VERSION_ID, ATTACHMENT_ID, AUTHOR_ID, FILENAME, DESCRIPTION, UPLOAD_DATE, SIZE, VERSION_NUMBER, IS_CURRENT
)
VALUES
(
1, 1, 2, 'Book Management Functional Spec.doc', 'This document outlines the functional specification for the book management part of the library management system.', DATEADD(day, -151, SYSUTCDATETIME()), 138, '2', 1
),
(
2, 2, 2, 'Library System Performance Metrics.xls', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 15, '1.1', 1
),
(
3, 3, 2, 'Error Logging-in Screen-shot.gif', 'Captured screen-shot of the error that was raised when attempting to log in to the library application', DATEADD(day, -149, SYSUTCDATETIME()), 92, '1', 1
),
(
4, 4, 2, 'Error Stacktrace.doc', 'Stack trace captured from error on page', DATEADD(day, -149, SYSUTCDATETIME()), 24, '1', 1
),
(
5, 5, 2, 'Cannot Sort Screenshot.ppt', NULL, DATEADD(day, -149, SYSUTCDATETIME()), 11, '1', 1
),
(
6, 6, 2, 'Web Page capture.htm', 'Captured web-page HTML from page display when rendered on the Mozilla browser', DATEADD(day, -149, SYSUTCDATETIME()), 88, '1', 1
),
(
7, 7, 2, 'Sequence Diagram for Book Mgt.pdf', 'Sequence diagram in UML format that provides additional detail surrounding the book managament use-case / test case', DATEADD(day, -149, SYSUTCDATETIME()), 35, '1.2', 1
),
(
8, 8, 3, 'Author Management Screen Wireframe.vsd', 'Outlines the user interface design for the author management screens minus the graphical branding', DATEADD(day, -144, SYSUTCDATETIME()), 423, '2.1', 1
),
(
9, 9, 3, 'Date Editing Screenshot.jpg', 'Screenshot when editing the data in the system. Ilustrates the lack of usability in the current approach', DATEADD(day, -144, SYSUTCDATETIME()), 130, '1', 1
),
(
10, 10, 3, 'Bug Stack Trace.txt', NULL, DATEADD(day, -144, SYSUTCDATETIME()), 1, '1', 1
),
(
11, 11, 3, 'Book Management Screen Wireframe.ai', NULL, DATEADD(day, -144, SYSUTCDATETIME()), 270, '1.1', 1
),
(
12, 12, 2, 'Use Case Diagram.vsd', 'UML use case diagram that provides additional information on the use case and associated test steps', DATEADD(day, -144, SYSUTCDATETIME()), 46, '1', 1
),
(
13, 13, 3, 'Graphical Design Mockups.psd', NULL, DATEADD(day, -144, SYSUTCDATETIME()), 660, '1', 1
),
(
14, 14, 2, 'Expected Result Screenshot.png', 'Captured screen shot of how we expect the page to look', DATEADD(day, -130, SYSUTCDATETIME()), 41, '1', 1
),
(
15, 15, 2, 'http://www.inflectra.com', 'URL to related information', DATEADD(day, -130, SYSUTCDATETIME()), 0, '1', 1
),
(
16, 1, 2, 'Book Management Functional Spec.doc', 'This document outlines the functional specification for the book management part of the library management system.', DATEADD(day, -130, SYSUTCDATETIME()), 138, '1', 0
),
(
17, 2, 3, 'Library System Performance Metrics.xls', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 12, '1', 0
),
(
18, 7, 2, 'Sequence Diagram for Book Mgt.pdf', 'Sequence diagram in UML format that provides additional detail surrounding the book managament use-case / test case', DATEADD(day, -144, SYSUTCDATETIME()), 30, '1.1', 0
),
(
19, 7, 3, 'Sequence Diagram for Book Mgt.pdf', 'Sequence diagram in UML format that provides additional detail surrounding the book managament use-case / test case', DATEADD(day, -142, SYSUTCDATETIME()), 28, '1', 0
),
(
20, 8, 3, 'Author Management Screen Wireframe.vsd', 'Outlines the user interface design for the author management screens minus the graphical branding', DATEADD(day, -129, SYSUTCDATETIME()), 423, '2', 0
),
(
21, 8, 2, 'Author Management Screen Wireframe.vsd', 'Outlines the user interface design for the author management screens minus the graphical branding', DATEADD(day, -113, SYSUTCDATETIME()), 423, '1', 0
),
(
22, 11, 2, 'Book Management Screen Wireframe.ai', NULL, DATEADD(day, -115, SYSUTCDATETIME()), 270, '1', 0
),
(
23, 16, 2, 'file:///c:/users/fredbloggs/appdata/local/test_scripts/test_1', NULL, DATEADD(day, -130, SYSUTCDATETIME()), 0, '1', 1
),
(
24, 17, 2, 'Web 01 SmarteATM Login.ses', NULL, DATEADD(day, -130, SYSUTCDATETIME()), 100, '1', 1
),
(
26, 19, 2, 'Repository.json', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 1, '1', 0
),
(
27, 20, 2, 'CreateNewAuthor.js', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 1, '1', 1
),
(
28, 21, 2, 'CreateNewAuthor.objects.js', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 11, '1', 1
),
(
29, 22, 2, 'CreateNewAuthor.objects.metadata', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 87, '1', 1
),
(
30, 23, 2, 'CreateNewAuthor.sstest', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 2, '1', 1
),
(
31, 24, 2, 'CreateNewAuthor.user.js', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 1, '1', 1
),
(
32, 19, 2, 'Repository.json', NULL, DATEADD(day, -113, SYSUTCDATETIME()), 1, '2', 1
),
(
34, 26, 2, 'Repository.json', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 1, '1', 0
),
(
35, 27, 2, 'CreateNewBook.js', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 1, '1', 1
),
(
36, 28, 2, 'CreateNewBook.objects.js', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 10, '1', 1
),
(
37, 29, 2, 'CreateNewBook.objects.metadata', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 87, '1', 1
),
(
38, 30, 2, 'CreateNewBook.sstest', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 1, '1', 1
),
(
39, 31, 2, 'CreateNewBook.user.js', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 1, '1', 1
),
(
40, 26, 2, 'Repository.json', NULL, DATEADD(day, -108, SYSUTCDATETIME()), 1, '2', 1
),
(
42, 33, 2, 'Repository.json', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', 0
),
(
43, 34, 2, 'EditExistingAuthor.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', 1
),
(
44, 35, 2, 'EditExistingAuthor.objects.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 11, '1', 1
),
(
45, 36, 2, 'EditExistingAuthor.objects.metadata', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 87, '1', 1
),
(
46, 37, 2, 'EditExistingAuthor.sstest', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 2, '1', 1
),
(
47, 38, 2, 'EditExistingAuthor.user.js', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 1, '1', 1
),
(
48, 33, 2, 'Repository.json', NULL, DATEADD(day, -107, SYSUTCDATETIME()), 1, '2', 1
),
(
50, 40, 2, 'Repository.json', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 0
),
(
51, 41, 2, 'EditExistingBook.js', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
52, 42, 2, 'EditExistingBook.objects.js', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 10, '1', 1
),
(
53, 43, 2, 'EditExistingBook.objects.metadata', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 87, '1', 1
),
(
54, 44, 2, 'EditExistingBook.sstest', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 2, '1', 1
),
(
55, 45, 2, 'EditExistingBook.user.js', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 2, '1', 1
),
(
56, 40, 2, 'Repository.json', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '2', 1
),
(
57, 46, 2, 'Test Plan.md', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
58, 47, 2, 'Test Areas.html', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
59, 48, 2, 'Create Author Scenario.feature', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
60, 49, 2, 'Library Brainstorm.mindmap', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
61, 50, 2, 'Library Requirements Overview.orgchart', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
62, 51, 2, 'Requirements Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
63, 52, 2, 'Releases Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
64, 53, 2, 'Documents Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
65, 54, 2, 'Test Cases Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
66, 55, 2, 'Incidents Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
67, 56, 2, 'Tasks Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
),
(
68, 57, 2, 'Risks Workflow.diagram', NULL, DATEADD(day, -103, SYSUTCDATETIME()), 1, '1', 1
)
GO

SET IDENTITY_INSERT TST_ATTACHMENT_VERSION OFF; 

