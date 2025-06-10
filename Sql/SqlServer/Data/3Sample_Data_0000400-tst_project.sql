/***************************************************************
**	Insert script for table TST_PROJECT
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT ON; 

INSERT INTO TST_PROJECT
(
PROJECT_ID, NAME, PROJECT_TEMPLATE_ID, PROJECT_GROUP_ID, DESCRIPTION, WEBSITE, CREATION_DATE, IS_ACTIVE, WORKING_HOURS, WORKING_DAYS, NON_WORKING_HOURS, IS_TIME_TRACK_INCIDENTS, IS_TIME_TRACK_TASKS, IS_EFFORT_INCIDENTS, IS_EFFORT_TASKS, IS_EFFORT_TEST_CASES, IS_TASKS_AUTO_CREATE, REQ_DEFAULT_ESTIMATE, REQ_POINT_EFFORT, TASK_DEFAULT_EFFORT, IS_REQ_STATUS_BY_TASKS, IS_REQ_STATUS_BY_TEST_CASES, IS_REQ_STATUS_AUTO_PLANNED, PERCENT_COMPLETE, START_DATE, END_DATE, REQUIREMENT_COUNT
)
VALUES
(
1, 'Library Information System (Sample)', 1, 2, 'Sample application that allows users to manage books, authors and lending records for a typical branch library', 'www.libraryinformationsystem.org', DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 68, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, 35, SYSUTCDATETIME()), 22
),
(
2, 'Sample Empty Product 1', 1, 2, 'Sample application that is empty and can be used for training.', 'www.tempuri.org', DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
3, 'Sample Empty Product 2', 2, 2, 'Sample application that is empty and can be used for training.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
4, 'ERP: Financials', 2, 3, 'An example ERP product for example, Dynamics AX or SAP. This is the core financials system.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 59, DATEADD(day, -220, SYSUTCDATETIME()), DATEADD(day, 145, SYSUTCDATETIME()), 37
),
(
5, 'ERP: Human Resources', 2, 3, 'An example ERP product for example, SAP, Worksoft. This is the Human Resources (HR) system', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 25, DATEADD(day, -160, SYSUTCDATETIME()), DATEADD(day, 266, SYSUTCDATETIME()), 50
),
(
6, 'Company Website', 2, 4, 'A company web site, primarily static content with a lightweight content management system. The web site is developed using the Scrum agile methodology', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 26, DATEADD(day, -40, SYSUTCDATETIME()), DATEADD(day, 141, SYSUTCDATETIME()), 90
),
(
7, 'Customer Relationship Management (CRM)', 2, 4, 'A sales force / customer relationship system such as Salesforce or Dynamics 365 CRM implemented using waterfall.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, DATEADD(day, -90, SYSUTCDATETIME()), DATEADD(day, 275, SYSUTCDATETIME()), 58
)
GO

SET IDENTITY_INSERT TST_PROJECT OFF; 

