/***************************************************************
**	Insert script for table TST_NOTIFICATION_EVENT
***************************************************************/
SET IDENTITY_INSERT TST_NOTIFICATION_EVENT ON; 

INSERT INTO TST_NOTIFICATION_EVENT
(
NOTIFICATION_EVENT_ID, PROJECT_TEMPLATE_ID, ARTIFACT_TYPE_ID, IS_ARTIFACT_CREATION, IS_ACTIVE, NAME, EMAIL_SUBJECT
)
VALUES
(
1, 2, 3, 1, 1, 'Incident: Newly Created', 'A new ${IncidentTypeId} has been opened in ${ProjectName}'
),
(
2, 2, 3, 0, 1, 'Incident: Owner Assigned', 'This ${IncidentTypeId} has been assigned to you in ${ProjectName}'
),
(
3, 2, 1, 1, 0, 'Requirement: Newly Created', 'A new Requirement has been opened in ${ProjectName}'
),
(
4, 2, 1, 0, 1, 'Requirement: Owner Assigned / Importance', 'This Requirement ${Name} has changed'
),
(
5, 2, 2, 0, 1, 'Test Case: Executed', 'This Test Case has been executed as ${ExecutionStatusId} in ${ProjectName}'
),
(
6, 2, 6, 1, 0, 'Task: Newly Created', 'A new Task has been opened in ${ProjectName}'
),
(
7, 2, 6, 0, 1, 'Task: Owner or Priority Changed', 'This Task has been updated in ${ProjectName}'
),
(
8, 2, 8, 1, 0, 'Test Set: Newly Created', 'A new Test Set has been created in ${ProjectName}'
),
(
9, 2, 8, 0, 1, 'Test Set: Execution Changed', 'This Test Set has been executed as ${TestSetStatusId} in ${ProjectName}'
),
(
10, 1, 3, 1, 1, 'Incident: Newly Created', 'A new ${IncidentTypeId} has been opened in ${ProjectName}'
),
(
11, 1, 3, 0, 1, 'Incident: Owner Assigned', 'This ${IncidentTypeId} has been assigned to you in ${ProjectName}'
),
(
12, 1, 1, 1, 0, 'Requirement: Newly Created', 'A new Requirement has been opened in ${ProjectName}'
),
(
13, 1, 1, 0, 1, 'Requirement: Owner Assigned / Importance', 'This Requirement ${Name} has changed'
),
(
14, 1, 2, 0, 1, 'Test Case: Executed', 'This Test Case has been executed as ${ExecutionStatusId} in ${ProjectName}'
),
(
15, 1, 6, 1, 0, 'Task: Newly Created', 'A new Task has been opened in ${ProjectName}'
),
(
16, 1, 6, 0, 1, 'Task: Owner or Priority Changed', 'This Task has been updated in ${ProjectName}'
),
(
17, 1, 8, 1, 0, 'Test Set: Newly Created', 'A new Test Set has been created in ${ProjectName}'
),
(
18, 1, 8, 0, 1, 'Test Set: Execution Changed', 'This Test Set has been executed as ${TestSetStatusId} in ${ProjectName}'
),
(
19, 3, 3, 1, 1, 'Incident: Newly Created', 'A new ${IncidentTypeId} has been opened in ${ProjectName}'
),
(
20, 3, 3, 0, 1, 'Incident: Owner Assigned', 'This ${IncidentTypeId} has been assigned to you in ${ProjectName}'
),
(
21, 3, 1, 1, 0, 'Requirement: Newly Created', 'A new Requirement has been opened in ${ProjectName}'
),
(
22, 3, 1, 0, 1, 'Requirement: Owner Assigned / Importance', 'This Requirement ${Name} has changed'
),
(
23, 3, 2, 0, 1, 'Test Case: Executed', 'This Test Case has been executed as ${ExecutionStatusId} in ${ProjectName}'
),
(
24, 3, 6, 1, 0, 'Task: Newly Created', 'A new Task has been opened in ${ProjectName}'
),
(
25, 3, 6, 0, 1, 'Task: Owner or Priority Changed', 'This Task has been updated in ${ProjectName}'
),
(
26, 3, 8, 1, 0, 'Test Set: Newly Created', 'A new Test Set has been created in ${ProjectName}'
),
(
27, 3, 8, 0, 1, 'Test Set: Execution Changed', 'This Test Set has been executed as ${TestSetStatusId} in ${ProjectName}'
),
(
28, 4, 3, 1, 1, 'Incident: Newly Created', 'A new ${IncidentTypeId} has been opened in ${ProjectName}'
),
(
29, 4, 3, 0, 1, 'Incident: Owner Assigned', 'This ${IncidentTypeId} has been assigned to you in ${ProjectName}'
),
(
30, 4, 1, 1, 0, 'Requirement: Newly Created', 'A new Requirement has been opened in ${ProjectName}'
),
(
31, 4, 1, 0, 1, 'Requirement: Owner Assigned / Importance', 'This Requirement ${Name} has changed'
),
(
32, 4, 2, 0, 1, 'Test Case: Executed', 'This Test Case has been executed as ${ExecutionStatusId} in ${ProjectName}'
),
(
33, 4, 6, 1, 0, 'Task: Newly Created', 'A new Task has been opened in ${ProjectName}'
),
(
34, 4, 6, 0, 1, 'Task: Owner or Priority Changed', 'This Task has been updated in ${ProjectName}'
),
(
35, 4, 8, 1, 0, 'Test Set: Newly Created', 'A new Test Set has been created in ${ProjectName}'
),
(
36, 4, 8, 0, 1, 'Test Set: Execution Changed', 'This Test Set has been executed as ${TestSetStatusId} in ${ProjectName}'
)
GO

SET IDENTITY_INSERT TST_NOTIFICATION_EVENT OFF; 

