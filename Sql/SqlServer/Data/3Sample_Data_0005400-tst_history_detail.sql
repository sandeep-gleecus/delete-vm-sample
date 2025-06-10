/***************************************************************
**	Insert script for table TST_HISTORY_DETAIL
***************************************************************/
INSERT INTO TST_HISTORY_DETAIL
(
FIELD_NAME, FIELD_CAPTION, OLD_VALUE, OLD_VALUE_INT, OLD_VALUE_DATE, NEW_VALUE, NEW_VALUE_INT, NEW_VALUE_DATE, CHANGESET_ID, FIELD_ID, CUSTOM_PROPERTY_ID
)
VALUES
(
'VersionNumber', 'Version Number', '1.0.0', NULL, NULL, '1.0.0.0', NULL, NULL, 1, 31, NULL
),
(
'OwnerId', 'Owner', NULL, NULL, NULL, 'Fred Bloggs', 2, NULL, 2, 23, NULL
),
(
'ExpectedResult', 'Expected Result', NULL, NULL, NULL, 'User taken to first screen', NULL, NULL, 3, 101, NULL
),
(
'RequirementStatusId', 'Status', 'Requested', 1, NULL, 'In Progress', 3, NULL, 4, 16, NULL
),
(
'IncidentStatusId', 'Status', 'New', 1, NULL, 'Open', 2, NULL, 5, 3, NULL
),
(
'IncidentTypeId', 'Type', 'Incident', 1, NULL, 'Bug', 2, NULL, 6, 4, NULL
),
(
'IncidentTypeId', 'Type', 'Incident', 1, NULL, 'Bug', 2, NULL, 7, 4, NULL
),
(
'IncidentStatusId', 'Status', 'New', 1, NULL, 'Open', 2, NULL, 8, 3, NULL
),
(
'Name', 'Name', 'Library System v1.0.0', NULL, NULL, 'Library System Release 1', NULL, NULL, 9, 99, NULL
),
(
'Name', 'Name', 'Need to create new book', NULL, NULL, 'Ability to create new book', NULL, NULL, 10, 88, NULL
),
(
'ExpectedResult', 'Expected Result', 'User taken to first screen', NULL, NULL, 'User taken to next screen in wizard', NULL, NULL, 11, 101, NULL
),
(
'RequirementStatusId', 'Status', 'In Progress', 3, NULL, 'Developed', 4, NULL, 12, 16, NULL
),
(
'IncidentStatusId', 'Status', 'Open', 2, NULL, 'Assigned', 3, NULL, 13, 3, NULL
)
GO

