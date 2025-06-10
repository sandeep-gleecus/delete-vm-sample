/***************************************************************
**	Insert script for table TST_REPORT_ELEMENT
***************************************************************/
SET IDENTITY_INSERT TST_REPORT_ELEMENT ON; 

INSERT INTO TST_REPORT_ELEMENT
(
REPORT_ELEMENT_ID, TOKEN, NAME, DESCRIPTION, IS_ACTIVE, ARTIFACT_TYPE_ID
)
VALUES
(
1, 'Attachments', 'List of Attached Documents', NULL, 1, NULL
),
(
2, 'History', 'Artifact Change History', NULL, 1, NULL
),
(
3, 'Requirements', 'Linked Requirements', NULL, 1, 1
),
(
4, 'TestCoverage', 'Test Case Coverage', NULL, 1, 2
),
(
5, 'TestSteps', 'Test Steps', NULL, 1, 7
),
(
6, 'TestRuns', 'Test Runs', NULL, 1, 5
),
(
7, 'Incidents', 'Linked Incidents', NULL, 1, 3
),
(
8, 'Tasks', 'Associated Tasks', NULL, 1, 6
),
(
9, 'TestSets', 'Test Sets', NULL, 1, 8
),
(
10, 'Releases', 'Releases', NULL, 1, 4
),
(
11, 'Builds', 'Builds', NULL, 1, NULL
),
(
12, 'SourceCode', 'Source Code', NULL, 1, NULL
),
(
13, 'Mitigations', 'Mitigations', NULL, 1, NULL
),
(14, N'AllHistoryList', N'Project Audit Trail', NULL, 1, 16),
 (15, N'AllAuditList', N'Audit Trail', NULL, 1, 19),
 (16, N'AllAdminAuditList', N'Admin Audit Trail', NULL, 1, 20),
 (17, N'AllUserAuditList', N'User Audit Trail', NULL, 1, 22),
 (18, N'SystemUsageReport', N'System Usage Report', NULL, 1, 34),
 (20, N'ProjectAuditTrail', N'Project Audit Trail Report', NULL, 1, 49),
 (21, N'AuditTrail', N'AuditTrailReport', NULL, 1, 50),
 (23, N'AdminAuditTrail', N'AdminAuditTrailReport', NULL, 1, 51),
 (25, N'UserAuditTrail', N'UserAuditTrailReport', NULL, 1, 52),
 (26, N'OldProjectAuditTrail', N'OldProjectAuditTrail', NULL, 1, 49)
GO

SET IDENTITY_INSERT TST_REPORT_ELEMENT OFF; 
