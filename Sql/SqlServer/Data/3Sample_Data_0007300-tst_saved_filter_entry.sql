/***************************************************************
**	Insert script for table TST_SAVED_FILTER_ENTRY
***************************************************************/
INSERT INTO TST_SAVED_FILTER_ENTRY
(
SAVED_FILTER_ID, ENTRY_KEY, ENTRY_VALUE, ENTRY_TYPE_CODE
)
VALUES
(
1, 'ImportanceId', '1', 9
),
(
1, 'CoverageId', '1', 9
),
(
2, 'ExecutionStatusId', '1', 9
),
(
2, 'ActiveYn', 'Y', 18
),
(
3, 'SortExpression', 'CreationDate ASC', 18
),
(
3, 'IncidentStatusId', '1', 9
),
(
4, 'SortExpression', 'PriorityName ASC', 18
),
(
4, 'IncidentStatusId', '8', 9
),
(
5, 'SortExpression', 'TaskStatusName ASC', 18
),
(
5, 'ProgressId', '4', 9
),
(
5, 'TaskPriorityId', '2', 9
),
(
6, 'ExecutionStatusId', '1', 9
)
GO

