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
8, 'Pharmaceuticals', 2, 5, 'Example of testing a new pharmaceutical product.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
9, 'Clinical Medical Devices', 2, 5, 'Example of designing, testing and validating a new clinical medical device. A good example of when you need to follow 21 CFR Part 11.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
10, 'Patient Records Management', 2, 6, 'Sample electronic medical records (EMR) product.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
11, 'Pharmacy System', 2, 6, 'Example of testing and upgrading a hospital''s pharmacy management system', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
12, 'Core Banking (T24)', 2, 7, 'Sample mainframe core banking application, in this case Temenos T24.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
13, 'Billing System', 2, 7, 'An example electronic paperless billing system that integrates with the core banking application, but is separate.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
14, 'Web Portal', 2, 8, 'Sample retail bank website customer portal that lets customers of the bank login to view their balance, make transfers and other common transactions. Developed in-house using an agile methodology.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
15, 'Mobile Banking', 3, 8, 'Sample retail bank native mobile application. This product comes in two flavors: iOS and Android and lets customers view their balances and make some simple transactions.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
16, 'Flight Control System', 2, 9, 'An autonomous unmanned aerial vehicle (UAV) embedded system that handles the flight control of the drone. Developed using a traditional V-Model methodology.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
17, 'Flight Planning System', 2, 9, 'A flight mapping and planning system that is used to provide route and guidance information to a UAV.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
18, 'Guidance System', 2, 10, 'Sample software used in the guidance system of a multi-stage satellite launching platform.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
19, 'Telemetry System', 2, 10, 'Sample software used in the telemetry system of a multi-stage satellite platform.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
20, 'ERP: Inventory Management', 2, 11, 'An example inventory management system used by a typical manufacturing company to track individual items of inventory.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
21, 'ERP: Warehouse Management', 2, 11, 'An example warehouse management system used by a typical manufacturing company to store inventory.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
22, 'ERP: Discrete Manufacturing', 2, 12, 'An example inventory management system used by a typical manufacturing company to track and manage discrete production.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
),
(
23, 'ERP: Continuous Manufacturing', 2, 12, 'An example inventory management system used by a typical manufacturing company to track and manage continuous production.', NULL, DATEADD(day, -151, SYSUTCDATETIME()), 1, 8, 5, 0, 1, 1, 1, 1, 0, 1, 1, 480, 360, 1, 1, 1, 0, NULL, NULL, 0
)
GO

SET IDENTITY_INSERT TST_PROJECT OFF; 

