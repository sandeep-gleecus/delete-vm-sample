/***************************************************************
**	Insert script for table TST_ARTIFACT_FIELD
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_FIELD ON; 

INSERT INTO TST_ARTIFACT_FIELD
(
ARTIFACT_FIELD_ID, ARTIFACT_TYPE_ID, NAME, LOOKUP_PROPERTY, CAPTION, ARTIFACT_FIELD_TYPE_ID, IS_WORKFLOW_CONFIG, IS_LIST_CONFIG, IS_LIST_DEFAULT, LIST_DEFAULT_POSITION, IS_ACTIVE, IS_DATA_MAPPING, IS_REPORT, IS_NOTIFY, IS_HISTORY_RECORDED, DESCRIPTION
)
VALUES
(
1, 3, 'SeverityId', 'SeverityName', 'Severity', 2, 1, 1, 0, 5, 1, 1, 1, 1, 1, 'The artifact''s Severity.'
),
(
2, 3, 'PriorityId', 'PriorityName', 'Priority', 2, 1, 1, 1, 4, 1, 1, 1, 1, 1, 'The artifact''s Priority.'
),
(
3, 3, 'IncidentStatusId', 'IncidentStatusName', 'Status', 2, 0, 1, 1, 3, 1, 1, 1, 0, 1, '#NAME?'
),
(
4, 3, 'IncidentTypeId', 'IncidentTypeName', 'Type', 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 'The Incident type.'
),
(
5, 3, 'OpenerId', 'OpenerName', 'Detected By', 2, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The name of the Opener.'
),
(
6, 3, 'OwnerId', 'OwnerName', 'Owned By', 2, 1, 1, 1, 8, 1, 0, 1, 1, 1, 'The name of the Owner.'
),
(
7, 3, 'DetectedReleaseId', 'DetectedReleaseVersionNumber', 'Detected Release', 11, 1, 1, 0, 11, 1, 0, 1, 1, 1, 'The detected release name.'
),
(
8, 3, 'ResolvedReleaseId', 'ResolvedReleaseVersionNumber', 'Planned Release', 11, 1, 1, 0, 12, 1, 0, 1, 1, 1, 'The resolved release name.'
),
(
9, 3, 'VerifiedReleaseId', 'VerifiedReleaseVersionNumber', 'Verified Release', 11, 1, 1, 0, 13, 1, 0, 1, 1, 1, 'The verified release name.'
),
(
10, 3, 'Name', NULL, 'Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the artifact.'
),
(
11, 3, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
12, 3, 'Resolution', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'Comments posted to the Incident.'
),
(
13, 3, 'CreationDate', NULL, 'Detected On', 3, 0, 1, 1, 7, 1, 0, 1, 0, 0, 'The creation date of the Incident.'
),
(
14, 3, 'ClosedDate', NULL, 'Closed On', 3, 1, 1, 0, 9, 1, 0, 1, 1, 1, 'The artifact closed date.'
),
(
15, 3, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 10, 1, 0, 1, 0, 0, 'The last time this Incident was updated.'
),
(
16, 1, 'RequirementStatusId', 'RequirementStatusName', 'Status', 2, 0, 1, 1, 5, 1, 1, 1, 1, 1, 'The current Status'
),
(
17, 1, 'AuthorId', 'AuthorName', 'Author', 2, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The name of the Author'
),
(
18, 1, 'ImportanceId', 'ImportanceName', 'Importance', 2, 1, 1, 1, 4, 1, 1, 1, 1, 1, 'The current Importance'
),
(
19, 1, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 1, 1, 1, 7, 1, 0, 1, 1, 1, 'The Requirement''s Assigned Release.'
),
(
20, 1, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 12, 1, 0, 1, 0, 0, 'The Creation date.'
),
(
21, 1, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 13, 1, 0, 1, 0, 0, 'The last updated date.'
),
(
22, 2, 'AuthorId', 'AuthorName', 'Author', 2, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The name of the Author.'
),
(
23, 2, 'OwnerId', 'OwnerName', 'Owner', 2, 1, 1, 1, 3, 1, 0, 1, 1, 1, 'The name or the Owner.'
),
(
24, 2, 'TestCasePriorityId', 'TestCasePriorityName', 'Priority', 2, 1, 1, 0, 5, 1, 1, 1, 1, 1, 'The priority of the Test Case.'
),
(
25, 2, 'ExecutionDate', NULL, 'Last Executed', 3, 0, 1, 1, 4, 1, 0, 1, 1, 0, 'The last execution date.'
),
(
26, 2, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 9, 1, 0, 1, 0, 0, 'The Creation date.'
),
(
27, 2, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 8, 1, 0, 1, 0, 0, 'The last updated date.'
),
(
28, 2, 'EstimatedDuration', NULL, 'Est. Dur.', 9, 1, 1, 0, 10, 1, 0, 1, 1, 1, 'The estimated duration, in HH:MM.'
),
(
29, 2, 'ActiveYn', NULL, 'Active', 10, 0, 1, 1, 7, 0, 0, 1, 1, 0, 'If the Test Case is active or not.'
),
(
30, 4, 'CreatorId', 'CreatorName', 'Creator', 2, 1, 1, 0, 15, 1, 0, 1, 1, 1, 'The name of the Creator.'
),
(
31, 4, 'VersionNumber', NULL, 'Version #', 1, 1, 1, 1, 2, 1, 0, 1, 1, 1, 'The version number of the release.'
),
(
32, 4, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 16, 1, 0, 1, 0, 0, 'The creation date'
),
(
33, 4, 'ActiveYn', NULL, 'Active', 10, 0, 1, 0, 16, 0, 0, 1, 1, 0, 'If the Release is active or not.'
),
(
34, 5, 'Name', NULL, 'Test Run Name', 6, 0, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the Test Run.'
),
(
35, 5, 'TestRunTypeId', 'TestRunTypeName', 'Type', 2, 0, 1, 1, 4, 1, 0, 1, 1, 0, 'The type/'
),
(
36, 5, 'TesterId', 'TesterName', 'Tester', 2, 0, 1, 1, 6, 1, 0, 1, 1, 1, 'The tester''s ID.'
),
(
37, 5, 'ExecutionStatusId', 'ExecutionStatusName', 'Status', 2, 0, 1, 1, 8, 1, 0, 1, 1, 0, 'The execution status.'
),
(
38, 5, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 0, 1, 1, 7, 1, 0, 1, 1, 1, 'The release the test was run against.'
),
(
39, 5, 'EstimatedDuration', NULL, 'Est. Dur.', 9, 0, 1, 1, 9, 1, 0, 1, 1, 1, 'The estimated duration, in HH:MM.'
),
(
40, 5, 'ActualDuration', NULL, 'Act. Dur.', 9, 0, 1, 1, 10, 1, 0, 1, 1, 1, 'The actual duration, in HH:MM'
),
(
41, 5, 'EndDate', NULL, 'Execution Date', 3, 0, 1, 1, 2, 1, 0, 1, 1, 0, 'The end date.'
),
(
42, 5, 'RunnerName', NULL, 'Runner Name', 1, 0, 1, 0, 5, 1, 0, 1, 1, 0, 'The runner''s name.'
),
(
43, 4, 'IterationYn', NULL, 'Iteration?', 10, 0, 1, 1, 11, 0, 0, 1, 1, 0, 'If the Release is an iteration.'
),
(
44, 2, 'ExecutionStatusId', 'ExecutionStatusName', 'Execution Status', 5, 1, 1, 1, 2, 1, 0, 1, 1, 1, 'The last execution status.'
),
(
45, 3, 'StartDate', NULL, 'Started On', 3, 1, 1, 0, 13, 1, 0, 1, 1, 1, 'The start date of the artifact.'
),
(
46, 3, 'CompletionPercent', NULL, '% Complete', 8, 0, 1, 0, 14, 1, 0, 1, 1, 1, 'The percent complete.'
),
(
47, 3, 'EstimatedEffort', NULL, 'Est. Effort', 9, 1, 1, 0, 15, 1, 0, 1, 1, 1, 'The estimated effort, in HH:MM.'
),
(
48, 3, 'ActualEffort', NULL, 'Actual Effort', 9, 1, 1, 0, 16, 1, 0, 1, 1, 1, 'The actual effort, in HH:MM.'
),
(
49, 8, 'CreatorId', 'CreatorName', 'Creator', 2, 0, 1, 0, 10, 1, 0, 1, 1, 1, 'The Creator of the Test Set.'
),
(
50, 8, 'OwnerId', 'OwnerName', 'Owner', 2, 0, 1, 1, 6, 1, 0, 1, 1, 1, 'The Owner of the Test Set.'
),
(
51, 8, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 0, 1, 1, 4, 1, 0, 1, 1, 1, 'The associated Release Name.'
),
(
52, 8, 'TestSetStatusId', 'TestSetStatusName', 'Status', 2, 0, 1, 1, 7, 1, 0, 1, 1, 1, 'The last execution status.'
),
(
53, 8, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 8, 1, 0, 1, 0, 0, 'The creation date.'
),
(
54, 8, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 9, 1, 0, 1, 0, 0, 'The last updated date.'
),
(
55, 8, 'ExecutionDate', NULL, 'Last Executed', 3, 0, 1, 1, 5, 1, 0, 1, 1, 0, 'The last execution date.'
),
(
56, 8, 'PlannedDate', NULL, 'Planned Date', 3, 0, 1, 1, 3, 1, 0, 1, 1, 1, 'The planned date for the Test Set.'
),
(
57, 6, 'TaskStatusId', 'TaskStatusName', 'Status', 2, 0, 1, 1, 4, 1, 1, 1, 1, 1, 'The current status of the Task.'
),
(
58, 6, 'OwnerId', 'OwnerName', 'Owner', 2, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The Owner of the artifact.'
),
(
59, 6, 'TaskPriorityId', 'TaskPriorityName', 'Priority', 2, 1, 1, 1, 5, 1, 1, 1, 1, 1, 'The priority of the Task.'
),
(
60, 6, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 13, 1, 0, 1, 0, 0, 'The creation date.'
),
(
61, 6, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 14, 1, 0, 1, 0, 0, 'The last updated date.'
),
(
62, 6, 'StartDate', NULL, 'Start Date', 3, 1, 1, 0, 8, 1, 0, 1, 1, 1, 'The start date of the artifact.'
),
(
63, 6, 'EndDate', NULL, 'End Date', 3, 1, 1, 0, 9, 1, 0, 1, 1, 1, 'The end date of the artifact.'
),
(
64, 6, 'CompletionPercent', NULL, '% Complete', 8, 0, 1, 0, 10, 1, 0, 1, 1, 0, 'The percent complete.'
),
(
65, 6, 'EstimatedEffort', NULL, 'Est. Effort', 9, 1, 1, 0, 11, 1, 0, 1, 1, 1, 'The estimated effort, in HH:MM.'
),
(
66, 6, 'ActualEffort', NULL, 'Actual Effort', 9, 1, 1, 0, 12, 1, 0, 1, 1, 1, 'The actual effort, in HH:MM.'
),
(
67, 6, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 1, 1, 1, 7, 1, 0, 1, 1, 1, 'The associated Release Name.'
),
(
68, 1, 'CoverageId', NULL, 'Test Coverage', 5, 0, 1, 1, 2, 1, 0, 1, 0, 0, NULL
),
(
69, 1, 'ProgressId', NULL, 'Task Progress', 5, 0, 1, 1, 3, 1, 0, 1, 0, 0, NULL
),
(
70, 1, 'EstimatedEffort', NULL, 'Est. Effort', 9, 0, 1, 0, 8, 1, 0, 1, 0, 1, 'Assigned Requirement''s planned effort in HH:MM'
),
(
71, 1, 'TaskEstimatedEffort', NULL, 'Task Effort', 9, 0, 1, 0, 9, 1, 0, 1, 1, 0, 'Assigned Task''s planned effort in HH:MM'
),
(
72, 1, 'TaskActualEffort', NULL, 'Actual Effort', 9, 0, 1, 0, 10, 1, 0, 1, 1, 0, 'Assigned Task''s actual effort in HH:MM'
),
(
73, 1, 'OwnerId', 'OwnerName', 'Owner', 2, 1, 1, 0, 11, 1, 0, 1, 1, 1, 'The name of the Owner'
),
(
74, 4, 'CoverageId', NULL, 'Test Status', 5, 0, 1, 1, 4, 1, 0, 1, 0, 0, NULL
),
(
75, 4, 'ProgressId', NULL, 'Task Progress', 5, 0, 1, 1, 5, 1, 0, 1, 0, 0, NULL
),
(
76, 4, 'PlannedEffort', NULL, 'Plan Effort', 9, 0, 1, 0, 8, 1, 0, 1, 1, 0, 'The planned effort in HH:MM.'
),
(
77, 4, 'AvailableEffort', NULL, 'Available Effort', 9, 0, 1, 0, 9, 1, 0, 1, 1, 0, 'The available effort in HH:MM.'
),
(
78, 4, 'TaskEstimatedEffort', NULL, 'Task Effort', 9, 0, 1, 0, 10, 1, 0, 1, 1, 0, 'The Task''s estimated effort in HH:MM.'
),
(
79, 4, 'TaskActualEffort', NULL, 'Actual Effort', 9, 0, 1, 0, 11, 1, 0, 1, 1, 0, 'The Task''s actual effort in HH:MM.'
),
(
80, 4, 'StartDate', NULL, 'Start Date', 3, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The start date of the artifact.'
),
(
81, 4, 'EndDate', NULL, 'End Date', 3, 1, 1, 1, 7, 1, 0, 1, 1, 1, 'The end date of the artifact.'
),
(
82, 4, 'ResourceCount', NULL, '# Resources', 13, 1, 1, 0, 13, 1, 0, 1, 1, 1, 'The number or resources assigned.'
),
(
83, 4, 'DaysNonWorking', NULL, '# Non Work Days', 13, 1, 1, 0, 14, 1, 0, 1, 1, 1, 'The number of non-work days.'
),
(
84, 5, 'TestSetId', 'TestSetName', 'Test Set', 11, 0, 1, 1, 3, 1, 0, 1, 1, 1, 'The test set''s ID.'
),
(
85, 1, 'RequirementId', NULL, 'Requirement #', 4, 0, 1, 1, 19, 1, 0, 1, 0, 0, 'The ID of the Requirement'
),
(
86, 1, 'Name', NULL, 'Requirement Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'Name of the Requirement'
),
(
87, 2, 'TestCaseId', NULL, 'Test #', 4, 0, 1, 1, 13, 1, 0, 1, 0, 0, NULL
),
(
88, 2, 'Name', NULL, 'Test Case Name', 6, 0, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the artifact.'
),
(
89, 2, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 0, 0, 0, NULL, 1, 0, 1, 0, 0, 'The associated Release Name.'
),
(
90, 2, 'ExecutionStatusId', NULL, 'Execution Status', 5, 0, 0, 0, NULL, 0, 0, 0, 0, 0, '(Duplicate, no longer user)'
),
(
91, 8, 'TestSetId', NULL, 'Test Set #', 4, 0, 1, 1, 17, 1, 0, 1, 0, 0, NULL
),
(
92, 8, 'Name', NULL, 'Test Set Name', 6, 0, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the Test Set.'
),
(
93, 8, 'ExecutionStatusId', NULL, 'Execution Status', 5, 0, 1, 1, 2, 1, 0, 1, 0, 0, NULL
),
(
94, 3, 'IncidentId', NULL, 'Incident #', 4, 0, 1, 1, 21, 1, 0, 1, 0, 0, 'The Incident''s ID.'
),
(
95, 6, 'TaskId', NULL, 'Task #', 4, 0, 1, 1, 19, 1, 0, 1, 0, 0, NULL
),
(
96, 6, 'Name', NULL, 'Task Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the artifact.'
),
(
97, 6, 'ProgressId', NULL, 'Progress', 5, 0, 1, 1, 2, 1, 0, 1, 0, 0, NULL
),
(
98, 4, 'ReleaseId', NULL, 'Release #', 4, 0, 1, 1, 20, 1, 0, 1, 0, 0, NULL
),
(
99, 4, 'Name', NULL, 'Release Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the Release.'
),
(
100, 5, 'TestRunId', NULL, 'Test Run #', 4, 0, 1, 1, 13, 1, 0, 1, 0, 0, NULL
),
(
101, 7, 'ExpectedResult', NULL, 'Expected Result', 12, 0, 1, 1, 2, 1, 0, 0, 1, 1, 'The Expected Result'
),
(
102, 7, 'SampleData', NULL, 'Sample Data', 12, 0, 1, 1, 3, 1, 0, 0, 1, 1, 'The Sample Data'
),
(
103, 7, 'ExecutionStatusId', 'ExecutionStatusName', 'Status', 2, 0, 1, 1, 4, 1, 0, 0, 1, 1, 'The execution status.'
),
(
104, 1, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'Descriptive text'
),
(
105, 2, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
106, 6, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
107, 4, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
108, 8, 'Description', NULL, 'Description', 12, 0, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
109, 8, 'EstimatedDuration', NULL, 'Est. Dur.', 9, 0, 1, 0, 11, 1, 0, 1, 1, 0, 'The estimated duration, in HH:MM'
),
(
110, 8, 'ActualDuration', NULL, 'Act. Dur.', 9, 0, 1, 0, 12, 1, 0, 1, 1, 0, 'The actual duration, in HH:MM'
),
(
111, 9, 'AutomationHostId', NULL, 'Host #', 4, 0, 1, 1, 5, 1, 0, 0, 0, 0, NULL
),
(
112, 9, 'Name', NULL, 'Host Name', 6, 0, 1, 1, 1, 1, 0, 0, 0, 1, 'The display name of the host'
),
(
113, 9, 'Description', NULL, 'Description', 12, 0, 0, 0, NULL, 1, 0, 0, 0, 1, 'The description of the host'
),
(
114, 9, 'Token', NULL, 'Token', 1, 0, 1, 1, 2, 1, 0, 0, 0, 1, 'The unique token of the host'
),
(
115, 9, 'IsActive', NULL, 'Active', 10, 0, 1, 1, 3, 1, 0, 0, 0, 1, 'Is the host active'
),
(
116, 9, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 1, 4, 1, 0, 0, 0, 0, NULL
),
(
117, 2, 'AutomationEngineId', 'AutomationEngineName', 'Automation Engine', 2, 1, 1, 0, 11, 1, 0, 0, 0, 1, NULL
),
(
118, 8, 'TestRunTypeId', 'TestRunTypeName', 'Test Run Type', 2, 0, 1, 0, 13, 1, 0, 0, 0, 1, NULL
),
(
119, 8, 'AutomationHostId', 'AutomationHostName', 'Automation Host', 2, 0, 1, 0, 14, 1, 0, 1, 0, 1, NULL
),
(
120, 5, 'AutomationHostId', 'AutomationHostName', 'Automation Host', 2, 0, 1, 0, 11, 1, 0, 1, 0, 0, NULL
),
(
121, 1, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'The requirement comments'
),
(
122, 2, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'The test case comments'
),
(
123, 4, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'The release comments'
),
(
124, 6, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'The task comments'
),
(
125, 8, 'Comments', NULL, 'Comments', 1, 0, 0, 0, NULL, 1, 0, 0, 1, 0, 'The test set comments'
),
(
126, 3, 'ProjectedEffort', NULL, 'Projected Effort', 9, 0, 1, 0, 17, 1, 0, 1, 0, 1, 'The projected incident effort'
),
(
127, 3, 'RemainingEffort', NULL, 'Remaining Effort', 9, 1, 1, 0, 18, 1, 0, 1, 1, 1, 'The remaining incident effort'
),
(
128, 6, 'CreatorId', 'CreatorName', 'Creator', 2, 1, 1, 0, 16, 1, 0, 1, 1, 1, 'The person who created the task'
),
(
129, 6, 'ProjectedEffort', NULL, 'Proj. Effort', 9, 0, 1, 0, 14, 1, 0, 1, 0, 0, 'The projected task effort'
),
(
130, 6, 'RemainingEffort', NULL, 'Remaining', 9, 1, 1, 0, 15, 1, 0, 1, 1, 1, 'The remaining task effort'
),
(
131, 1, 'TaskRemainingEffort', NULL, 'Remaining Effort', 9, 0, 1, 0, 14, 1, 0, 1, 1, 0, 'Assigned Task''s remaining effort'
),
(
132, 1, 'TaskProjectedEffort', NULL, 'Projected Effort', 9, 0, 1, 0, 15, 1, 0, 1, 1, 0, 'Assigned Task''s projected effort'
),
(
133, 4, 'TaskRemainingEffort', NULL, 'Remaining Effort', 9, 0, 1, 0, 18, 1, 0, 1, 1, 0, 'Assigned Task''s remaining effort'
),
(
134, 4, 'TaskProjectedEffort', NULL, 'Projected Effort', 9, 0, 1, 0, 19, 1, 0, 1, 1, 0, 'Assigned Task''s projected effort'
),
(
135, 8, 'RecurrenceId', 'RecurrenceName', 'Recurrence', 2, 0, 1, 0, 15, 1, 0, 1, 1, 1, 'The recurrence pattern of the test set'
),
(
136, 3, 'BuildId', 'BuildName', 'Fixed Build', 2, 1, 1, 0, 19, 1, 0, 1, 1, 0, 'The build that the incident was fixed in'
),
(
137, 5, 'BuildId', 'BuildName', 'Build', 2, 0, 1, 0, 12, 1, 0, 1, 1, 0, 'The test run build'
),
(
138, 3, 'ProgressId', NULL, 'Progress', 5, 0, 1, 1, 20, 1, 0, 1, 0, 0, 'The Progress of the Incident.'
),
(
139, 6, 'RequirementId', 'RequirementName', 'Requirement', 11, 1, 1, 0, 18, 1, 0, 1, 0, 1, 'The task''s requirement'
),
(
140, 1, 'RequirementTypeId', 'RequirementTypeName', 'Type', 2, 1, 1, 1, 16, 1, 1, 1, 1, 1, 'The requirement type'
),
(
141, 1, 'ComponentId', 'ComponentName', 'Component', 2, 1, 1, 0, 17, 1, 1, 1, 1, 1, 'The component the requirement is a part of'
),
(
142, 1, 'EstimatePoints', NULL, 'Estimate', 13, 1, 1, 0, 18, 1, 0, 1, 1, 1, 'The estimate in story/function points'
),
(
143, 12, 'RequirementStepId', NULL, 'Step #', 4, 0, 0, 0, NULL, 1, 0, 0, 0, 0, 'The requirement step id'
),
(
144, 12, 'Description', NULL, 'Description', 12, 0, 0, 0, NULL, 1, 0, 0, 0, 1, 'The description of the step'
),
(
145, 6, 'TaskTypeId', 'TaskTypeName', 'Type', 2, 1, 1, 1, 3, 1, 1, 1, 1, 1, 'The type of task'
),
(
146, 7, 'Description', NULL, 'Description', 12, 0, 1, 1, 1, 1, 0, 0, 1, 1, 'Description of the Test Step'
),
(
147, 6, 'ComponentId', 'ComponentName', 'Component', 2, 1, 1, 0, 20, 1, 1, 1, 1, 0, 'The component the task is a part of'
),
(
148, 3, 'ComponentIds', NULL, 'Component', 16, 1, 1, 0, 22, 1, 1, 1, 1, 1, 'The component(s) the incident is a part of'
),
(
149, 13, 'Filename', NULL, 'Document Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'The filename or URL of the attachment'
),
(
150, 13, 'DocumentTypeId', 'DocumentTypeName', 'Type', 2, 1, 1, 1, 2, 1, 0, 1, 1, 1, 'The type of document'
),
(
151, 13, 'Size', NULL, 'Size', 8, 0, 1, 1, 3, 1, 0, 1, 0, 0, 'The size of the document'
),
(
152, 13, 'EditorId', 'EditorName', 'Edited By', 2, 1, 1, 1, 4, 1, 0, 1, 1, 1, 'The person who last edited'
),
(
153, 13, 'EditedDate', NULL, 'Edited On', 3, 0, 1, 1, 5, 1, 0, 1, 0, 0, 'The date it was last edited'
),
(
154, 13, 'AuthorId', 'AuthorName', 'Author', 2, 1, 1, 1, 6, 1, 0, 1, 1, 1, 'The person who created the document'
),
(
155, 13, 'AttachmentId', NULL, 'ID', 4, 0, 1, 1, 11, 1, 0, 1, 0, 0, 'The ID of the document'
),
(
156, 13, 'CurrentVersion', NULL, 'Current Version', 1, 0, 1, 0, 7, 1, 0, 1, 1, 1, 'The currently active version'
),
(
157, 13, 'UploadDate', NULL, 'Uploaded On', 3, 0, 1, 0, 8, 1, 0, 1, 0, 0, 'The date it was uploaded'
),
(
158, 13, 'Filetype', NULL, 'File Type', 1, 0, 0, 0, NULL, 1, 0, 1, 0, 0, 'The physical file type'
),
(
159, 13, 'Tags', NULL, 'Tags', 1, 1, 1, 0, 9, 1, 0, 1, 1, 1, 'Any meta-tags for the document'
),
(
160, 13, 'ProjectAttachmentFolderId', NULL, 'Folder', 8, 1, 0, 0, NULL, 1, 0, 0, 0, 0, 'The folder the document is in'
),
(
161, 13, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The description of the artifact.'
),
(
162, 4, 'ReleaseStatusId', 'ReleaseStatusName', 'Status', 2, 0, 1, 1, 17, 1, 0, 1, 1, 1, 'The status of the release'
),
(
163, 4, 'ReleaseTypeId', 'ReleaseTypeName', 'Type', 2, 0, 1, 1, 12, 1, 0, 1, 1, 1, 'The type of the release'
),
(
164, 4, 'OwnerId', 'OwnerName', 'Owned By', 2, 1, 1, 0, 21, 1, 0, 1, 1, 1, 'The owner of the release'
),
(
165, 4, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 0, 22, 1, 0, 1, 0, 0, 'The last time this Release was updated.'
),
(
166, 2, 'TestCaseStatusId', 'TestCaseStatusName', 'Status', 2, 1, 1, 1, 7, 1, 1, 1, 1, 1, 'The status of the test case'
),
(
167, 2, 'TestCaseTypeId', 'TestCaseTypeName', 'Type', 2, 1, 1, 1, 12, 1, 1, 1, 1, 1, 'The type of the test case'
),
(
168, 2, 'ComponentIds', NULL, 'Component', 16, 1, 1, 0, 14, 1, 1, 1, 1, 1, 'The component(s) of the test case'
),
(
169, 2, 'ActualDuration', NULL, 'Act. Dur.', 9, 1, 1, 0, 15, 1, 0, 1, 0, 0, 'The actual duration, in HH:MM'
),
(
170, 2, 'IsSuspect', NULL, 'Suspect?', 10, 0, 1, 0, 16, 1, 0, 1, 1, 1, 'Have the requirements changed'
),
(
171, 8, 'BuildExecuteTimeInterval', NULL, 'Build Execute Interval', 8, 0, 1, 0, 16, 1, 0, 1, 1, 1, 'How long after a build it should execute'
),
(
172, 5, 'Description', NULL, 'Description', 12, 0, 0, 0, NULL, 1, 0, 0, 1, 1, 'Test run description'
),
(
173, 2, 'IsTestSteps', NULL, 'Test Steps?', 10, 1, 1, 0, 17, 1, 0, 1, 1, 1, 'Does the test case have steps'
),
(
174, 7, 'TestStepId', NULL, 'Step #', 4, 0, 1, 1, 5, 1, 0, 0, 0, 0, 'The id of the test step'
),
(
175, 8, 'IsAutoScheduled', NULL, 'Schedule on Build', 10, 0, 1, 0, 17, 1, 0, 1, 1, 1, 'Is the test set auto-scheduled on successful build'
),
(
176, 8, 'TestCaseReleaseId', NULL, 'Test Execution Release', 11, 0, 0, 0, NULL, 1, 0, 1, 0, 0, 'The release filter for test set execution data'
),
(
177, 8, 'TestConfigurationSetId', 'TestConfigurationSetName', 'Configuration', 2, 0, 1, 0, 18, 1, 0, 1, 1, 1, 'The test configuration to execute against'
),
(
178, 13, 'DocumentStatusId', 'DocumentStatusName', 'Status', 2, 0, 1, 1, 10, 1, 0, 1, 1, 1, 'The status of the document'
),
(
179, 13, 'DocumentVersions', NULL, 'Versions', 10, 1, 0, 0, NULL, 1, 0, 0, 0, 0, 'The versions functionality of the document'
),
(
180, 13, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'The document comments'
),
(
181, 15, 'RiskMitigationId', NULL, 'Step #', 4, 0, 0, 0, NULL, 1, 0, 0, 0, 0, 'The risk mitigation id'
),
(
182, 15, 'Description', NULL, 'Description', 12, 0, 0, 0, NULL, 1, 0, 0, 0, 1, 'The description of the mitigation'
),
(
183, 15, 'ReviewDate', NULL, 'Review Date', 3, 0, 0, 0, NULL, 1, 0, 0, 0, 1, 'The review date of the mitigation'
),
(
184, 14, 'RiskProbabilityId', 'RiskProbabilityName', 'Probability', 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 'The risk probability'
),
(
185, 14, 'RiskImpactId', 'RiskImpactName', 'Impact', 2, 1, 1, 1, 3, 1, 1, 1, 1, 1, 'The risk impact'
),
(
186, 14, 'RiskStatusId', 'RiskStatusName', 'Status', 2, 0, 1, 1, 6, 1, 1, 1, 1, 1, 'The risk status'
),
(
187, 14, 'RiskTypeId', 'RiskTypeName', 'Type', 2, 1, 1, 1, 5, 1, 1, 1, 1, 1, 'The risk type'
),
(
188, 14, 'CreatorId', 'CreatorName', 'Created By', 2, 1, 1, 0, 11, 1, 0, 1, 1, 1, 'The person who opened the risk'
),
(
189, 14, 'OwnerId', 'OwnerName', 'Owned By', 2, 1, 1, 1, 7, 1, 0, 1, 1, 1, 'The person who owns the risk'
),
(
190, 14, 'ReleaseId', 'ReleaseVersionNumber', 'Release', 11, 1, 1, 0, 9, 1, 0, 1, 1, 1, 'The release the risk relates to'
),
(
191, 14, 'ComponentId', 'ComponentName', 'Component', 2, 1, 1, 0, 10, 1, 1, 1, 1, 1, 'The component the risk relates to'
),
(
192, 14, 'Name', NULL, 'Name', 6, 1, 1, 1, 1, 1, 0, 1, 1, 1, 'The name of the risk'
),
(
193, 14, 'Description', NULL, 'Description', 12, 1, 0, 0, NULL, 1, 0, 0, 1, 1, 'The descripton of the risk'
),
(
194, 14, 'Comments', NULL, 'Comments', 1, 1, 0, 0, NULL, 1, 0, 0, 1, 0, 'Comments posted to the risk'
),
(
195, 14, 'CreationDate', NULL, 'Created On', 3, 0, 1, 0, 12, 1, 0, 1, 0, 0, 'The creation date of the risk'
),
(
196, 14, 'ClosedDate', NULL, 'Closed On', 3, 1, 1, 0, 13, 1, 0, 1, 1, 1, 'The date/time the risk was closed'
),
(
197, 14, 'LastUpdateDate', NULL, 'Last Modified', 3, 0, 1, 1, 8, 1, 0, 1, 0, 0, 'The last time the risk was updated'
),
(
198, 14, 'ReviewDate', NULL, 'Review Date', 3, 1, 1, 0, 14, 1, 0, 1, 1, 1, 'The next review date of the risk'
),
(
199, 14, 'RiskExposure', NULL, 'Exposure', 8, 1, 1, 1, 4, 1, 0, 1, 0, 1, 'The exposure of the risk'
),
(
200, 14, 'RiskId', NULL, 'Risk #', 4, 0, 1, 1, 15, 1, 0, 1, 0, 0, 'The ID of the risk'
),
(
201, 6, 'RiskId', 'RiskName', 'Risk', 11, 0, 0, 0, NULL, 1, 0, 0, 0, 0, 'The task''s risk'
),
(
202, 4, 'RequirementCount', NULL, 'Req. Count', 8, 1, 1, 0, 22, 1, 0, 1, 1, 1, 'The number of requirements in the release/sprint'
),
(
203, 4, 'RequirementPoints', NULL, 'Req. Points', 13, 1, 1, 0, 23, 1, 0, 1, 1, 1, 'The count of requirements points'
),
(
204, 4, 'CompletionId', NULL, 'Req. Completion', 5, 0, 1, 1, 3, 1, 0, 1, 1, 0, 'The % complete of the release by requirement'
),
(
205, 4, 'PlannedPoints', NULL, 'Planned Points', 13, 1, 1, 0, 24, 1, 0, 1, 1, 1, 'The planned story points'
),
(
206, 4, 'PeriodicReviewDate', NULL, 'Periodic Review Date', 3, 1, 1, 1, 11, 1, 0, 1, 1, 1, 'Used for Periodic Review Alerts'
),
(
207, 2, 'TestCasePreparationStatusId', 'TestCasePreparationStatus', 'Test Case Preparation Status', 2, 1, 1, 0, 18, 1, 1, 1, 1, 1, 'The preparation status of the Test Case.'
),
(
208, 4, 'PeriodicReviewAlertId', 'Name', 'PeriodicReviewAlertId', 2, 1, 1, 1, 11, 1, 0, 0, 1, 1, 'PeriodicReviewAlertId'
),
(
209,16,	'ChangeSetId',	'NULL',	'History',	4,	1,	1,	1,	15,	1,	1,	1,	1,	1,	'History List Details'
),
(
210,	21,	'Name',	'NULL',	'Name',	1,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of Portfolio'
),
(
211,	21,	'Description',	NULL,	'Description',	12,	1,	1,	1,	NULL,	1,	0,	1,	1,	1,	'The description of Portfolio'
),
(
214,	21,	'IsActive',	NULL,	'Active',	10,	1,	0,	0,	17,	1,	0,	1,	1,	1,	'Active of Portfolio'
),
(
215,	21,	'PortfolioId',	NULL,	'PortfolioId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID of the Portfolio'
),
(
216,	-12,	'ProjectGroupId',	NULL,	'ProjectGroupId	',4,	0,	1,	1,	15,	1,	0,	1,	0,	1,	'The ID of the Program'
),
(
217,	-12,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of the Program'
),
(
218,	-12,	'Description',	NULL,	'Description',	6,	1,	0,	0,	NULL,	1,	0,	0,	1,	1,	'The Description of the program'
),
(
219,	-12,	'PortfolioId',	'PortfolioType',	'Portfolio',	2,	1,	1,	1,	6,	1,	1,	1,	1,	1,	'The Portfolio Type of Program'
),
(
220,	-12,	'ProjectTemplateId',	'ProjectTemplateType',	'ProjectTemplate',	2,	1,	1,	1,	6,	1,	1,	1,	1	,1	,'The Project TemplateId of the program'
),
(
221,	-12,	'Website',	NULL,	'Website',	6	,1,	1,	1,	1,	1,	0	,1,	1	,1,'The website of Program'
),
(
222,	-12,	'IsActive',	NULL,	'Active',	10,	1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'The Project Group Active'
),
(
223,	-12,	'IsDefault',	NULL,	'Default',	10,	1,	0,	0,	NULL,	1,	0,	0,	1,	1,	'The Project Group Default Value'
),
(
224,	23,	'Name',	NULL,	'Template Name',	6,	1,	1,	1,	1,	1,	0	,1	,1	,1,	'The Name of the Project Template'
),
(
226,	23,	'Description',	NULL,	'Description',	6,	1,	1,	1,	1,	1	,0	,1	,1	,1,	'The Description of the Project Template'
),
(
227,	23,	'IsActive',	NULL,	'IsActive',	10,	1,	0,	0,	NULL,	1,	0,	0,	1,	1,	'The Active of Project Template'
),
(
228,	23,	'Status',	NULL,	'Status',	10	,1,	0,	0	,NULL,	1,	0,	0,	1,	1,	'The status of the Project Template'
),
(
229,	-1,	'Name',	NULL,	'Project Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of the Project'
),
(
230,	-1,	'Template',	'TemplateType',	'Project Template',	2,	1,	1,	1,	6,	1,	1,	1,	1,	1,	'The Template of the Project'
),
(
231,	-1,	'ProjectGroupId',	'ProgramType',	'Program',	2,	1,	1,	1,	6,	1,	1,	1,	1,	1,	'The Program for Project'
),
(
232,	-1,	'Website',	NULL,	'Website',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The website for Project'
),
(
234,	-1,	'Baseline',	NULL,	'Baseline',	10	,1,	0,	0,	NULL,	1,	0,	0	,0,	1	,'The Baseline for Project'
),
(
236,	-1,	'IsActive',	NULL,	'Active',	10,	1,	0,	0,	NULL,	1,	0,	1,	1,	1,	'The Active Project'
),
(
237,	23,	'ProjectTemplateId',	NULL,	'ProjectTemplateId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID of the Template'
),
(
240,	-1,	'ProjectId',	NULL,	'ProjectId',	4,	0,	1	,1	,15,	1,	0,	1,	0,	0,	'The Id of the Project'
),
(
241,	-1,	'Description',	NULL,	'Description',	6,	1,	0,	0,	NULL,	1,	0	,1,	1,	1,	'The Description of the project'
),
(
242,	24,	'ProjectRoleId',	NULL,'Project Role Id',	4,	0	,1	,1,	15,	1,	0,	1,	0,	0,	'The Id of the Project Role'
),
(
243,	24,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The name of the Project Role'
),
(
245,	24,	'Description',	NULL,	'Description',	6,	1,	1	,1	,1,	1,	0,	1,	1,	1,	'The Description for Project Role'
),
(
246,	24,	'IsAdmin',	NULL,	'Project Admin',	10,	1,	0	,0	,NULL,	1,	0	,0	,0	,1,	'The Project Admin for Project Role'
),
(
247	,24,	'IsTemplateAdmin',	NULL,	'Template Admin',	10	,1,	0,	0,	NULL,	1	,0	,0	,0,	1,	'The Template Admin for Project Role'
),
(
248,	24,	'IsLimitedView',	NULL,	'Limited View',	10,	1,	0,	0,	NULL,	1	,0	,0	,0,	1,	'The Limited view for Project Role'
),
(
249,	24,	'IsDiscussionsAdd',	NULL,	'Discussion Add',	10,	1,	0,	0,	NULL,	1,	0,	0,	0	,1,	'The Discussion View for Project Role'
),
(
250,	24,	'IsSourceCodeView',	NULL,	'Source Code View',	10,	1	,0,	0,	NULL,	1	,0	,0,	0	,1	,'Source code view for Project Role'
),
(
251,	24,	'IsSourceCodeEdit',	NULL,	'Source Code Edit',	10,	1,	0,	0,	NULL,	1,	0,	0,	0	,1	,'Source Code Edit for Project Role'
),
(
252,	24,	'IsDocumentFoldersEdit'	,NULL,	'Document Folder View',	10,	1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'Document Folder view for Project Role'
),
(
253,	25,	'ProjectRoleId',	NULL,	'Project Role Id',	4	,0	,1,	1	,15	,1	,0	,1,	0	,1	,'The Id for Project Role'
),
(
254	,25,	'ArtifactTypeId',	NULL,	'ArtifactType Id',	4,	0,	1,	1	,15,	1,	0	,1,	0,	1,	'The ID for ArtifactID in ProjectRole Permission'
),
(
256,	25,	'RolePermissions',	NULL,	'RolePermissions',	4,	0,	1,	1,	15,	1,	0,	1,	0,	1,	'The RolePermissions'
),
(
257	,26	,'OAuthProviderId',	NULL,	'OAuthProviderId',	6,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID for OAuth Provider'
),
(
258	,26,	'ClientId',	NULL,	'ClientId',	6,	1,	1,	1,	1,	1,	0	,1	,1,	1,	'The Client Id for OAuth Provider'
),
(
259,	26,	'ClientSecret',	NULL,	'ClientSecret',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Client Secret Key for OAuth Provider'
),
(
260,	26,	'AuthorizationUrl',	NULL,	'AuthorizationUrl',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Authorization url for OAuth Provider'
),
(
261,	26,	'TokenUrl',	NULL,	'TokenUrl',	6,	1,	1,	1,	1	,1	,0,	1,	1,	1	,'The Token Url for OAuth Provider'
),
(
262,	26	,'ProfileUrl',	NULL,	'ProfileUrl',	6,	1,	1	,1,	1	,1,	0,	1	,1	,1,	'The Profile Url for OAuth Provider'
),
(
263,	26,	'IsActive',	NULL,	'Active',	10,	1,	0,	0,	NULL,	1,	0,	0,	0	,1,	'IS Active for OAuth Provider'
),
(
264,	28,	'VersionControlSystemId',	NULL,	'VersionControlSystemId', 	4,	0,	1,	1,	15,	1,	0,	1	,0	,0	,'The ID VersionControlSystem'
),
(
265	,28,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of Versio Control System'
),
(
266,	28,	'Description',	NULL,	'Description',	6,	1,	1,	1,	1	,1	,0	,1,	1,	1,	'The Description for Version Control System'
),
(
267,	28,	'ConnectionString',	NULL,	'ConnectionString',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The ConnectionString for Version Control System'
),
(
268,	28,	'Domain',	NULL,	'Domain',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Domain name for Version Control System'
),
(
269,	28,	'Custom01',	NULL,	'Custom01',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'Custom1 Property'
),
(
270,	28,	'Custom02',	NULL,	'Custom02',	6,	1,	1,	1,	1	,1	,0,	1	,1	,1	,'Custom2 Property'
),
(
271,	28,	'Custom03',	NULL,	'Custom03',	6,	1,	1,	1,	1,	1	,0	,1	,1,	1,	'Custom3 Property'
),
(
272,	28,	'Custom04',	NULL,	'Custom04',	6,	1,	1	,1,	1,	1,	0,	1,	1,	1	,'Custom4 Property'
),
(
273,	28,	'Custom05',	NULL,	'Custom05',	6,	1,	1,	1,	1	,1	,0	,1,	1,	1	,'Custom5 Property'
),
(
275,	28,	'IsActive',	NULL,	'IsActive',	10,	1	,0,	0,	1,	1,	0,	0,	0,	1,	'Is Active for Version control system'
),
(
276,	29,	'DataSyncSystemId',	NULL,	'DataSyncSystemId',	4	,0	,1,	1,	15,	1,	0,	1,	0,	0,	'The ID for DataSyncSystem'
),
(
277,	29,'DataSyncStatusId',	NULL,	'DataSyncStatusId',	4	,0	,1	,1	,15	,1,	0	,1	,0,	0,	'The ID for DataSyncStatus'
),
(
278,	29,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The name for Data Sync System'
),
(
279,	29,	'Description',	NULL,	'Description',	6,	1,	1	,1,	1,	1	,0	,1,	1,	1,	'The Description for Data sync system'
),
(
280,	29,	'ConnectionString',	NULL	,'ConnectionString',	6,	1,	1,	1	,1	,1,	0,	1,	1,	1,	'Connection String'
),
(
281,	29,	'ExternalLogin',	NULL,	'ExternalLogin',	6,	1	,1,	1,	1,	1,	0	,1,	1	,1,'External Login'
),
(
282,	29,	'TimeOffsetHours',	NULL,	'TimeOffsetHours',	4,	0,	1,	1	,15	,1	,0	,1,	0,	1	,'Time Offset Hours'
),
(
283,	29,	'Custom01',	NULL,	'Custom01',	6,	0,	1,	1	,1,	1	,0,	1	,1,	1,	'Custom01'
),
(
284,	29,	'Custom02',	NULL,	'Custom2',	6,	1,	1	,1	,1,	1	,0	,1,	1	,1	,'Custom02 property'
),
(
286,	29,	'Custom03',	NULL,	'Custom3',	6	,1,	1,	1,	1,	1,	0,	1,	1	,1,	'Custom3 Property'
),
(
287,	29,	'Custom04',	NULL,	'Custom4',	6	,1,	1,	1,	1,	1,	0	,1,	1	,1,	'Custom4 Property'
),
(
288,	29,	'Custom05',	NULL,	'Custom5',	6,	1	,1	,1,	1	,1,	0,	1,	1,	1,	'Custom5 Property'
),
(
289,	29,	'AutoMapUsersYn',	NULL,	'AutoMapUsersYn',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'AutoMapUsersYn'
),
(
290,	29,	'Caption',	NULL,	'Caption',	6,	1	,1,	1	,1	,1,	0,	1,	1,	1,	'Caption'
),
(
291,	29,	'IsActive',	NULL,	'IsActive',	10,	1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'Data Sync System'
),
(
292,	10,	'AutomationEngineId',	NULL,	'AutomationEngineId',	4	,0,	1,	1	,15,	1,	0,	1	,0,	0,	'The ID for AutomationEngine'
),
(
293,	10,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of Automation Engine'
),
(
294,	10,	'Description',	NULL,	'Description',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Description of Automation Engine'
),
(
295,	10,	'Token',	NULL,	'Token',	6,	1	,1,	1,	1	,1,	0,	1,	1	,1	,'The Token of Automation Engine'
),
(
297,	10,	'IsDeleted',	NULL,	'IsDeleted',	10	,1,	0,	0,	NULL,	1,	0	,0	,0	,1	,'IsDeleted in Automation Engine'
),
(
298	,10,	'IsActive',	NULL,	'IsActive',	10	,1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'IsActive in Automation Engine'
),
(
299,	30,	'ReportId',	NULL,	'ReportId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID for Report'
),
(
300,	30,	'ReportCategoryId',	NULL,	'ReportCategoryId',	4	,0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID for Report Category'
),
(
302,	30,	'Token',	NULL,	'Token',	6,	1	,1	,1	,1,	1	,0,	1,	1,	1,	'The Token for Report'
),
(
303,	30,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'Name for Report'
),
(
304,	30,	'Description',	NULL,	'Description',	6,	1,	1	,1	,1,	1,	0,	1	,1,	1,	'Description for Report'
),
(
305,	30,	'Header',	NULL,	'Header',	6,	1	,1,	1,	1	,1	,0	,1	,1	,1,	'Header of the Report'
),
(
306,	30,	'Footer',	NULL,	'Footer',	6,	1	,1,	1	,1	,1	,0,	1,	1,	1,	'Footer of the Report'
),
(
307,	30,	'IsActive',	NULL,	'IsActive',	10,	1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'IsActive of Report'
),
(
308	,31,	'GraphId',	NULL,	'GraphId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID of the Graph'
),
(
309,	31,	'GraphTypeId',	NULL,	'GraphTypeId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	0,	'The ID of the GraphType'
),
(
310,	31,	'Position',	NULL,	'Position',	4,	0	,1,	1	,15,	1,	0,	1,	0	,1,	'The Position of the Graph'
),
(
311,	31,	'Name',	NULL,	'Name',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Name of the Graph'
),
(
312,	31,	'Description',	NULL,	'Description',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'The Description of the Graph'
),
(
313,	31,	'Query',	NULL,	'Query',	6,	1,	1,	1,	1	,1	,0,	1,	1,	1,	'The Query of the Graph'
),
(
314,	31,	'IsActive',	NULL,	'IsActive',	10,	1,	0,	0,	NULL,	1,	0,	0,	0,	1,	'IsActive of Graph'
),
(
315,	32,	'ReportId',	NULL,	'ReportId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	1,	'The ID Report section Instance'
),
(
316	,32,	'ReportSectionId',	NULL,	'ReportSectionId',	4,	0,	1,	1,	15,	1	,0,	1,	0,	1,	'The Id of the Report Section'
),
(
317	,32,	'Position',	NULL,	'Position',	4,	0,	1,	1,	15	,1,	0,	1,	0,	1,	'The Position Report section'
),
(
318,	32,	'ReportAvailableSectionId',	NULL,	'ReportAvailableSectionId',	4,	0,	1,	1,	15,	1,	0,	1,	0,	1,	'The Id for ReportAvailableSection'
),
(
319	,32	,'Header',	NULL,	'Header',	6,	1,	1,	1,	1	,1	,0,	1,	1,	1,	'Header for Report Section'
),
(
320,	32,	'Footer',	NULL,	'Footer',	6,	1,	1,	1,	1	,1	,0,	1,	1,	1,	'Footer of the Report Section'
),(
321	,32,	'Template',	NULL,	'Template',	6,	1,	1,	1,	1,	1	,0	,1,	1	,1	,'Template of the Report Section'
),
(
322,	32,	'SectionInstances',	NULL,	'SectionInstances',	4,	0	,1,	1,	15,	1	,0	,1	,0,	1,	'Section Instances'
),
(
323	,33,	'ReportCustomSectionId',	NULL,	'ReportCustomSectionId',	4,	0	,1	,1	,15	,1	,0	,1,	0	,1,	'The ID of Report Custom'
),
(
324,	33,	'ReportId',	NULL	,'ReportId',	4,	0,	1,	1,	15,	1,	0,	1,	0	,1,	'The Report Id for Report Custom'
),
(
325,	33,	'Position',	NULL,	'Position',	4	,0,	1,	1,	15,	1	,0	,1,	0,	1,	'Position of Report Custom'
),
(
326,	33,	'Name',	NULL,	'Name',	6,	1	,1	,1	,1	,1,	0,	1,	1,	1,	'The Name of Report Custom'
),
(
327,	33,	'Description',	NULL,	'Description',	6	,1	,1	,1	,1,	1	,0	,1	,1,	1,	'The Description of Report Custom'
),
(
328,	33,	'Template',	NULL,	'Template',	6,	1	,1,	1,	1,	1,	0,	1,	1,	1	,'The Template of Report Custom'
),
(
330,	33,	'Query',	NULL,	'Query',	6	,1,	1,	1,	1	,1	,0,	1,	1,	1,	'The Query of Report Custom'
),
(
331,	33,	'Header',	NULL,	'Header',	6,	1,	1,	1,	1,	1,	0,	1,	1,	1,	'the Header of Report Custom'
),
(
332,	33,	'Footer',	NULL,	'Footer',	6	,1,	1	,1,	1	,1,	0	,1,	1	,1	,'The Footer for Report Custom'
),
(
333	,33,	'IsActive',	NULL,	'IsActive',	10,	0	,1,	0,	NULL,	1,	0,	1,	1,	1,	'IsActive of Report Custom'
),
(
334,	34,	'SystemUsageReport',	NULL,	'Year',	4	,1	,1	,1	,15,	1	,0	,1	,1	,1,	'The System Usage Report'
),
(335, 25,'ProjectRole', NULL,	'Project Role', 4,	1,	1,	1,	15,1,	0,	1,	1, 1,	'The Project Role'),
(336,	24,	'IsActive', NULL,	'Project Role Active', 10,	1,	1,	0,	0,	1,	0,	0,	1, 1,	'The Project Role Active'),
(337,	35,	'ProjectGroupId', NULL,	'Project Group Id', 4,	1,	1,	0,	0,	1,	0,	0,	1, 1,	'The ID of the ProjectGroup'),
(338,	35,	'UserId', NULL,	'User Id', 4,	1,	1,	0,	0,	1,	0,	0,	1, 1,	'The ID of the User'),
(339,	35,	'ProjectGroupRoleId', NULL,	'Project Group Role Id', 4,	1,	1,	0,	0,	1,	0,	0,	1, 1,	'The ID of the ProjectGroupRole'),
(340,	36,	'ProjectId',	NULL, 'Project Id', 4,1	,1,	1,	9, 1,	0,1,	1,	1,	'The Project Id for Project tag Frequency'),
(341,	36,	'Name',	NULL, 'Name', 6,1	,1,	1,	0, 1,	0,1,	1,	1,	'Name of tag'),
(342,	36,	'Frequency',	NULL, 'Frequency', 4,1	,1,	1,	9, 1,	0,1,	1,	1,	'Frequency of Tag'),
(344,	37,'DiscussionId', NULL,'Discussion Id',4, 1,	1,1,9,	1,	0,		0,	1,	1,	'The ID of the Discussion'),
(345,	37,'ArtifactId', NULL,'Artifact Id',4, 1,	1,1,9,	1,	0,		0,	1,	1,	'The ID of the Discussion Artifact'),
(347,	37,'Comment', NULL,'Text',1, 1,	1,1,NULL,	1,	0,		0,	1,	1,	'The Description of comments'),
(348,	37,'CreationDate', NULL,'Creation Date',3, 1,	1,1,NULL,	1,	1,		0,	1,	1,	'The created date of Document comment'),
(349,	37,'CreatorId', NULL,'Creator Id',8, 1,	1,1,9,	1,	0,		0,	1,	1,	'The ID of creator of document'),
(350,	39,'ArtifactLinkTypeId', 'The Id of Artifact LinkType','Artifact Link Id',4, 1,	1,1,9,	1,	0,		0,	1,	1,	'The Id of Artifact LinkType'),
(351,	39,'Comment', NULL,'Comment',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'Comments of Incident Artifact link'),
(353,	40,'ArtifactId', NULL,'Artifact Comment',4, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'Comments of Incident Artifact link'), 
(354,	40,'Comment', NULL,'ReleaseComments',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The comments for Release'),
(355,	41,'Comment', NULL,'TestCaseComment',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Test Case Comment'),
(356,	42,'Comment', NULL,'TaskComments',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Task Comments'),
(357,	43,'Comment', NULL,'RiskComments',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Risk Comments'),
(358,	41,'ArtifactId', NULL,'ArtifactId',4, 1,	1,0,15,	1,	0,		0,	1,	1,	'The Testcase ArtifactId'), 
(359,	42,'ArtifactId', NULL,'ArtifactId',4, 1,	1,0,15,	1,	0,		0,	1,	1,	'The Task ArtifactId'), 
(360,	43,'ArtifactId', NULL,'ArtifactId',4, 1,	1,0,15,	1,	0,		0,	1,	1,	'The Risk ArtifactId'), 
(361,	44,'AtifactId', NULL,'Requirement Discussion ArtifactId',4, 1,	1,1,15,	1,	0,		0,	1,	1,	'The Artifact Id for Requirement Discussion'),
(362,	44,'Comment', NULL,'Comment',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Comments for Requirement Discussion'),
(363,	45,'ArtifactId', NULL,'TestSet Discussion ArtifactId',4, 1,	1,1,15,	1,	0,		0,	1,	1,	'The ArtifactId for TestSet Discussion'), 
(364,	45,'Comment', NULL,'Comment',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Comment for TestSet Discussion'),
(365,	48,'IncidentId', NULL,'IncidentId',4, 1,	1,0,15,	1,	0,		0,	1,	1,	'The Incident ID for Resolution'), 
(366,	48,'ArtifactId', NULL,'ArtifactId',4, 1,	1,0,15,	1,	0,		0,	1,	1,	'The Id for Incident Resolution'), 
(367,	48,'Resolution', NULL,'Resolution',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Incident Resolution'),
(368,	19,'ChangesetId', NULL,'AllAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'All Audit Trail Details'),
(369,	20,'ChangesetId', NULL,'AllAdminAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'All Admin Audit Trail Detail'),
(370,	22,'ChangesetId', NULL,'AllUserAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'All User Audit Trail Details'),
(371,	49,'ChangesetId', NULL,'ProjectHistoryAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'Project Audit Trail Details'),
(372,	50,'ChangesetId', NULL,'AuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'Audit Trail Details'),
(373,	51,'ChangesetId', NULL,'AdminAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'Admin Audit Trail Details'),
(374,	52,'ChangesetId', NULL,'UserAuditTrail',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'User Audit Trail Details'),
(375,	53,'TestCaseParameterId', NULL,'TestCaseParameterId',4, 1,	1,1,15,	1,	1,		1,	1,	0,	'The Test case Parameter Id'),
(376,	53,'TestCaseId', NULL,'TestCaseId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The Test Case Id for TestCaseParameter'),
(377,	53,'Name', NULL,'Name',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The name of TestCase parameter'),
(378,	53,'DefaultValue', NULL,'DefaultValue',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Default value fore TestCase Parameter'),
(379,	54,'TestCaseParameterId', NULL,'TestCaseParameterId',4, 1,	1,1,15,	1,	1,		1,	1,	0,	'The ID of setId TestCase Parameter'),
(380,	54,'TestSetId', NULL,'TestSetId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID for Test Set'),
(381,	54,'Value', NULL,'Value',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The value for Test Set Parameter'),
(382,	38,'BaselineId', NULL,'BaselineId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID for Project Baseline'),
(383,	38,'Name', NULL,'Name',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Name of Baseline'),
(385,	38,'Description', NULL,'Description',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Description for Baseline'),
(
386	,38,	'IsActive',	NULL,	'IsActive',	10,	0	,1,	0,	NULL,	1,	0,	1,	1,	1,	'IsActive for Baseline'
),
(387,	55,'TestConfigurationSetId', NULL,'TestConfigurationSetId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID for Test Configuration Set'),
(388,	55,'ProjectId', NULL,'ProjectId',8, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ProjectId for TestConfigurationSet'),
(390,	55,'Name', NULL,'Name',1, 1,	1,0,1,	1,	0,		0,	1,	1,	'The Name of Test Configuration Set'),
(391,	55,'Description', NULL,'Description',1, 1,	1,0,1,	1,	0,		0,	1,	1,	'The Description for Test Configuration Set'),
(
392	,55,	'IsActive',	NULL,	'IsActive',	10,	0	,1,	0,	1,	1,	0,	1,	1,	1,	'Test Configuration Set Active'
),
(
393	,55,	'IsDeleted',	NULL,	'IsDeleted',	10,	0	,1,	0,	1,	1,	0,	1,	1,	1,	'Test Configuration Set Delete'
),
(394,	56,'ReleaseId', NULL,'ReleaseId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The releaseId for release Testcase'),
(395,	56,'TestCaseId', NULL,'TestCaseId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The TestcaseId for Release Testcase'),
(396,	57,'EventId', NULL,'EventId',4, 1,	1,1,15,	1,	1,		1,	1,	0,	'The Id for Event log'),
(397,	57,'EventTypeId', NULL,'EventTypeId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The EventType Id for Event log'),
(398,	27,'FiletypeId', NULL,'FiletypeId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID for the File Type'),
(399,	27,'Mime', NULL,'Mime',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The mime for File Type'),
(400,	27,'Icon', NULL,'Icon',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Icon for File Type'),
(401,	27,'FileExtension', NULL,'FileExtension',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The FileExtension for File Type'),
(402,	27,'Description', NULL,'Description',1, 1,	1,0,NULL,	1,	0,		0,	1,	1,	'The Description for File Type'),
(
403, 16, 'ProjectHistoryChangeYear', 'Year', 'Project Audit Trail By Year', 2, 1, 1, 1, 5, 1, 1, 1, 1, 1, 'Year'
),
(
404, 16, 'ProjectHistoryChangeMonth', 'Month', 'Project Audit Trail By Month', 2, 1, 1, 1, 5, 1, 1, 1, 1, 1, 'Month'
),
(
405, 58, 'AttachmentVersionId', NULL, 'Attachment Version Id', 4, 0, 1, 1, 15, 1, 0, 1, 0, 0, 'The ID of the Attachment Version'
),
(406,	59,'TestCaseSignatureId', NULL,'TestCaseSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo TestCase Signature'),
(407,	59,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Test Case Signature'),
(408,	60,'RequirementSignatureId', NULL,'RequirementSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Requirement Signature'),
(409,	60,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Requirement Signature'),
(410,	1,'ApproveStatus', NULL,'ApprovedStatus',12, 1,	1,1,20,	1,	1,		0,	1,	1,	'The Approved Status for Requirement'),

(411,	61,'ReleaseSignatureId', NULL,'ReleaseSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Release Signature'),
(412,	61,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Release Signature'),
(413,	62,'DocumentSignatureId', NULL,'DocumentSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Document Signature'),
(414,	62,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Document Signature'),
(415,	63,'IncidentSignatureId', NULL,'IncidentSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Incident Signature'),
(416,	63,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Incident Signature'),
(417,	64,'RiskSignatureId', NULL,'RiskSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Risk Signature'),
(418,	64,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Risk Signature'),
(419,	65,'TaskSignatureId', NULL,'TaskSignatureId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'The ID fo Task Signature'),
(420,	65,'StatusId', NULL,'StatusId',4, 1,	1,1,15,	1,	1,		1,	1,	1,	'StatusId for Task Signature')
GO

SET IDENTITY_INSERT TST_ARTIFACT_FIELD OFF; 

