/***************************************************************
**	Insert script for table TST_REPORT_CATEGORY
***************************************************************/
SET IDENTITY_INSERT TST_REPORT_CATEGORY ON; 

INSERT INTO TST_REPORT_CATEGORY
(
REPORT_CATEGORY_ID, NAME, POSITION, ARTIFACT_TYPE_ID, IS_ACTIVE, WORKSPACE_TYPE_ID
)
VALUES
(
1, 'Cleaning Validation Reports', 100, 1, 1, 1
),
(
2, 'Software Assurance Reports', 200, 2, 1, 1
),
(
3, 'Incident Reports', 300, 3, 1, 1
),
(
4, 'Task Reports', 400, 6, 0, 1
),
(
5, 'Tracking & Release Metric Reports', 500, 4, 1, 1
),
(
6, 'Assessment Reports', 600, 14, 1, 1
),
(7, N'21 CFR PART 11 Audit Trail Reports', 700, 16, 1, 1),
(8,  N'All Audit Trail', 800, 19, 0, 1),
(11,  N'All Admin Audit Trail', 900,  20,0, 1),
(12,  N'All User Audit Trail', 1000,22, 0, 1),
(13, N'System Usage Reports', 1100, 34,  1, 1),
(14, N'Project Audit Trail', 1200, 49, 0, 1),
(15,  N'Audit Trail', 1300, 50, 0, 1),
(16,  N'Admin Audit Trail', 1400,  51,0, 1),
(17,  N'User Audit Trail', 1500,52, 0, 1),
(18,  N'Ad Hoc Reports', 1600,NULL, 1, 1),
(19,  N'Computer Systems Validation Reports', 1700,NULL, 1, 1),
(20,  N'Validation Scorecard Reports', 1800,NULL, 1, 1),
(21,  N'Graphical Reports', 1900,NULL, 0, 1),
(23,  N'Equipment Validation Reports', 2000,NULL, 1, 1),
(24,  N'Laboratory Validation Reports', 2100,NULL, 1, 1),
(25,  N'Method Validation Reports', 2200,NULL, 1, 1),
(26,  N'Periodic Review Validation Reports', 2300,NULL, 1, 1),
(27,  N'Process Validation Reports', 2400,NULL, 1, 1),
(28,  N'Project Management Reports', 2500,NULL, 1, 1),
(30,  N'System Inventory Reports', 2600,NULL, 1, 1),
(31,  N'XML Reports', 2700,NULL, 1, 1),
(1018,  N'Custom Reports', 2800,NULL, 1, 1)
GO

SET IDENTITY_INSERT TST_REPORT_CATEGORY OFF; 

