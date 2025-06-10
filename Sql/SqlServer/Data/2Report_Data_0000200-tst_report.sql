/***************************************************************
**	Insert script for table TST_REPORT
***************************************************************/
SET IDENTITY_INSERT TST_REPORT ON; 

INSERT INTO TST_REPORT
(
REPORT_ID, REPORT_CATEGORY_ID, TOKEN, NAME, DESCRIPTION, HEADER, FOOTER, IS_ACTIVE
)
VALUES
(
1, 1, 'RequirementSummary', 'Requirements Summary', 'This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement''s details and coverage status are displayed in a summary list form.', '<p>This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement''s details and coverage status are displayed in a summary list form.</p>', NULL, 1
),
(
2, 1, 'RequirementDetailed', 'Requirements Detailed', 'This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement''s details and coverage status are displayed, along with sub-tables containing the list of covering test cases, linked incidents/requirements, attached documents, associated tasks, linked artifacts and the change history', '<p>This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement''s details and coverage status are displayed, along with sub-tables containing the list of covering test cases, linked incidents/requirements, attached documents, associated tasks, linked artifacts and the change history</p>', NULL, 1
),
(
3, 1, 'RequirementPlan', 'Requirements Plan', 'This report displays a complete work breakdown structure of the project, including all requirements and tasks organized by schedule.', '<p>This report displays a complete work breakdown structure of the project, including all requirements and tasks organized by schedule.</p>', NULL, 1
),
(
4, 2, 'TestCaseSummary', 'Test Case Summary', 'This report displays all of the test cases defined for the current project in the order they appear in the test case list. The test case''s details and execution status are displayed in a summary list form.', '<p>This report displays all of the test cases defined for the current project in the order they appear in the test case list. The test case''s details and execution status are displayed in a summary list form.</p>', NULL, 1
),
(
5, 2, 'TestCaseDetailed', 'Test Case Detailed', 'This report displays all of the test cases defined for the current project in the order they appear in the test case list. The test case''s details and execution status are displayed, along with sub-tables containing the list of test steps, test runs, attached documents, the change history, and a list of any associated open incidents', '<p>This report displays all of the test cases defined for the current project in the order they appear in the test case list. The test case''s details and execution status are displayed, along with sub-tables containing the list of test steps, test runs, attached documents, the change history, and a list of any associated open incidents</p>', NULL, 1
),
(
6, 2, 'TestSetSummary', 'Test Set Summary', 'This report displays all of the test sets defined for the current project in the order they appear in the test set list. The test set''s details and execution status are displayed in a summary list form.', '<p>This report displays all of the test sets defined for the current project in the order they appear in the test set list. The test set''s details and execution status are displayed in a summary list form.</p>', NULL, 1
),
(
7, 2, 'TestSetDetailed', 'Test Set Detailed', 'This report displays all of the test sets defined for the current project in the order they appear in the test set list. The test set''s details and execution status are displayed, along with sub-tables containing the list of test cases, test runs, attached documents, and the change history', '<p>This report displays all of the test sets defined for the current project in the order they appear in the test set list. The test set''s details and execution status are displayed, along with sub-tables containing the list of test cases, test runs, attached documents, and the change history</p>', NULL, 1
),
(
8, 2, 'TestPrintable', 'Printable Test Scripts', 'This report is useful when you want to be able to conduct the testing activities offline on paper, or when testers need paper copies of the test script in addition to using the online test execution wizard. It displays all of the test cases defined for the current project in the order they appear in the test case list together with their detailed test steps and a list of any attached documents', '<p>This report is useful when you want to be able to conduct the testing activities offline on paper, or when testers need paper copies of the test script in addition to using the online test execution wizard. It displays all of the test cases defined for the current project in the order they appear in the test case list together with their detailed test steps and a list of any attached documents</p>', NULL, 1
),
(
9, 2, 'TestRunSummary', 'Test Run Summary', 'This report displays all of the test runs defined for the current project in date order (most recent first). The test run''s details and execution status are displayed in a summary list form.', '<p>This report displays all of the test runs defined for the current project in date order (most recent first). The test run''s details and execution status are displayed in a summary list form.</p>', NULL, 1
),
(
10, 2, 'TestRunDetailed', 'Test Run Detailed', 'This report displays all of the test runs defined for the current project in date order (most recent first). The test run''s details and execution status are displayed, along with sub-tables containing the list of test run steps, and a list of any associated open incidents', '<p>This report displays all of the test runs defined for the current project in date order (most recent first). The test run''s details and execution status are displayed, along with sub-tables containing the list of test run steps, and a list of any associated open incidents</p>', NULL, 1
),
(
11, 3, 'IncidentSummary', 'Incident Summary', 'This report displays all of the incidents tracked for the current project. The incident''s details are displayed  in a summary list form.', '<p>This report displays all of the incidents tracked for the current project. The incident''s details are displayed  in a summary list form.</p>', NULL, 1
),
(
12, 3, 'IncidentDetailed', 'Incident Detailed', 'This report displays all of the incidents tracked for the current project. The incident''s details are displayed, along with tables containing the list of resolutions as well as a tabular list of attached documents, linked requirements/incidents and the change history', '<p>This report displays all of the incidents tracked for the current project. The incident''s details are displayed, along with tables containing the list of resolutions as well as a tabular list of attached documents, linked requirements/incidents and the change history</p>', NULL, 1
),
(
13, 5, 'TaskSummary', 'Task Summary', 'This report displays all of the tasks tracked for the current project. The task''s details are displayed in a summary list form.', '<p>This report displays all of the tasks tracked for the current project. The task''s details are displayed in a summary list form.</p>', NULL, 1
),
(
14, 5, 'TaskDetailed', 'Task Detailed', 'This report displays all of the tasks tracked for the current project. The task''s details are displayed, along with a tabular list of attached documents and the change history', '<p>This report displays all of the tasks tracked for the current project. The task''s details are displayed, along with a tabular list of attached documents and the change history</p>', NULL, 1
),
(
15, 5, 'ReleaseSummary', 'Release Summary', 'This report displays all of the releases and iterations defined for the current project in the order they appear in the release/iteration hierarchy. The release''s details are displayed in a summary list form.', '<p>This report displays all of the releases and iterations defined for the current project in the order they appear in the release/iteration hierarchy. The release''s details are displayed in a summary list form.</p>', NULL, 1
),
(
16, 5, 'ReleaseDetailed', 'Release Detailed', 'This report displays all of the releases and iterations defined for the current project in the order they appear in the release/iteration hierarchy. The release''s details are displayed, along with sub-tables containing the list of mapped test cases, test runs, associated incidents, attached documents, scheduled tasks and the change history', '<p>This report displays all of the releases and iterations defined for the current project in the order they appear in the release/iteration hierarchy. The release''s details are displayed, along with sub-tables containing the list of mapped test cases, test runs, associated incidents, attached documents, scheduled tasks and the change history</p>', NULL, 1
),
(
17, 5, 'ReleasePlan', 'Release Plan', 'This report displays a complete work breakdown structure of the project, including all releases, iterations tasks and incidents organized by schedule.', '<p>This report displays a complete work breakdown structure of the project, including all releases, iterations tasks and incidents organized by schedule.</p>', NULL, 1
),
(
18, 1, 'RequirementTrace', 'Requirements Traceability', 'This report displays a matrix of the requirements in the system with their list of covering test cases and associated, mapped requirements', '<p>This report displays a matrix of the requirements in the system with their list of covering test cases and associated, mapped requirements</p>', NULL, 1
),
(
19, 2, 'TestCaseTrace', 'Test Case Traceability', 'This report displays a matrix of the test cases in the system with the list of mapped releases, incidents and test sets', '<p>This report displays a matrix of the test cases in the system with the list of mapped releases, incidents and test sets</p>', NULL, 1
),
(
20, 6, 'RiskSummary', 'Risk Summary', 'This report displays all of the risks tracked for the current project. The risks are displayed in a summary table form.', '<p>This report displays all of the risks tracked for the current project. The risks are displayed in a summary table form.</p>', NULL, 1
),
(
21, 6, 'RiskDetailed', 'Risk Detailed', 'This report displays all of the risks tracked for the current project. The risks are displayed, along with a tabular list of mitigations, tasks, comments, attached documents, and change history', '<p>This report displays all of the risks tracked for the current project. The risks are displayed, along with a tabular list of mitigations, tasks, comments, attached documents, and change history</p>', NULL, 1
),
(22, 15, N'AllHistoryList', N'Project Audit Trail', N'This report displays all of the history tracked for the current project.', N'<p>This report displays all of the history tracked for the current project.</p>', NULL, 1),
 (23, 15, N'AllAuditList', N'Audit Trail', N'This report displays audit history tracked for all the changes done for all the artifacts in multiple Projects.', N'<p>This report displays audit history tracked for all the changes done for all the artifacts in multiple Projects.</p>', NULL, 1),
 (24, 15, N'AllAdminAuditList', N'Admin Audit Trail', N'This report displays audit history tracked for all the System Administrative Activities performed by the Admin role User.', N'<p>This report displays audit history tracked for all the System Administrative Activities performed by the Admin role User.</p>', NULL, 1),
 (25, 15, N'AllUserAuditList', N'User Audit Trail', N'This report displays audit history tracked for all the User account changes performed by the Admin role User.', N'  <p>This report displays audit history tracked for all the User account changes performed by the Admin role User.</p>', NULL, 1),
 (26, 11, NULL, N'Test Report', N'Test Report1', N'<p>Test12</p>
', N'<p>Test1</p>
', 1),
 (28, 13, N'SystemUsageReport', N'System Usage Report', N'This report displays system usage tracked.', N'<p>This report displays system usage tracked.</p>', NULL, 1),
(32, 7, N'ProjectAuditTrail', N'Project Audit Trail', N'This report displays all of the history tracked for the current project.', N'<p>This report displays all of the history tracked for the current project.</p>', NULL, 1),
 (33, 7, N'AuditTrail', N'Audit Trail', N'This report displays audit history tracked for all the changes done for all the artifacts in multiple Projects.', N'<p>This report displays audit history tracked for all the changes done for all the artifacts in multiple Projects.</p>', NULL, 1),
 (34, 7, N'AdminAuditTrail', N'Admin Audit Trail', N'This report displays audit history tracked for all the System Administrative Activities performed by the Admin role User.', N'<p>This report displays audit history tracked for all the System Administrative Activities performed by the Admin role User.</p>', NULL, 1),
 (35, 7, N'UserAuditTrail', N'User Audit Trail', N'This report displays audit history tracked for all the User account changes performed by the Admin role User.', N'  <p>This report displays audit history tracked for all the User account changes performed by the Admin role User.</p>', NULL, 1),

  (37, 18, N'AdHocReports', N'Ad Hoc Reports', N'This report displays Ad Hoc Reports for the current project.', N'<p>This report displays Ad Hoc Reports for the current project.</p>', NULL, 0),
(38, 19, N'Computer Systems ValidationReports', N'Computer Systems Validation Reports', N'This report displays Computer Systems Validation Reports for the current project.	', N'<p>This report displays Computer Systems Validation Reports for the current project.</p>', NULL, 0),
 (39, 20, N'Validation ScorecardReports', N'Validation Scorecard Reports', N'This report displays Validation Scorecard Reports for the current project.', N'<p>This report displays Validation Scorecard Reports for the current project.</p>', NULL, 0),
 (41, 23, N'Equipment ValidationReports', N'Equipment Validation Reports', N'This report displays Equipment Validation Reports for the current project.', N'<p>This report displays Equipment Validation Reports for the current project.</p>', NULL, 0),
 (42, 24, N'Laboratory ValidationReports', N'Laboratory Validation Reports', N'This report displays Laboratory Validation Reports for the current project.', N'<p>This report displays Laboratory Validation Reports for the current project.</p>', NULL, 0),

(43,	25,	N'Method ValidationReports', N'Method Validation Reports', N'This report displays Method Validation Reports for the current project.', N'<p>This report displays Method Validation Reports for the current project.</p>', NULL, 0),
(44,	26,	N'Periodic Review ValidationReports', N'Periodic Review Validation Reports', N'This report displays Periodic Review Validation Reports for the current project.', N'<p>This report displays Periodic Review Validation Reports for the current project.</p>', NULL, 0),
(45,	27,	N'Process ValidationReports', N'Process Validation Reports', N'This report displays Process Validation Reports for the current project.', N'<p>This report displays Process Validation Reports for the current project.</p>', NULL, 0),
(46,	28,	N'Project ManagementReports', N'Project Management Reports', N'This report displays Project Management Reports for the current project.', N'<p>This report displays Project Management Reports for the current project.</p>', NULL, 0),
(47,	30,	N'System InventoryReports', N'System Inventory Reports', N'This report displays System Inventory Reports for the current project.', N'<p>This report displays System Inventory Reports for the current project.</p>', NULL, 0),
(48,	31,	N'XMLReports', N'XML Reports', N'This report displays XML Reports for the current project.', N'<p>This report displays XML Reports for the current project.</p>', NULL, 0),
(49,	1018,	N'CustomReports', N'Custom Reports', N'This report displays Custom Reports for the current project.', N'<p>This report displays Custom Reports for the current project.</p>', NULL, 0),
(50, 7, N'OldProjectAuditTrail', N'Old Project Audit Trail Report', N'This report displays all of the old history tracked for the current project.', N'<p>This report displays all of the old history tracked for the current project.</p>', NULL, 1)
GO

SET IDENTITY_INSERT TST_REPORT OFF; 

