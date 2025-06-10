/***************************************************************
**	Insert script for table TST_HISTORY_CHANGESET_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_HISTORY_CHANGESET_TYPE ON; 

INSERT INTO TST_HISTORY_CHANGESET_TYPE
(
CHANGETYPE_ID, CHANGE_NAME
)
VALUES
(
1, 'Modified'
),
(
2, 'Deleted'
),
(
3, 'Added'
),
(
4, 'Purged'
),
(
5, 'Rollback'
),
(
6, 'Undelete'
),
(
7, 'Imported'
),
(
8, 'Exported'
),
(
9, 'Deleted (via Parent)'
),
(
10, 'Added (via Parent)'
),
(
11, 'Purged (via Parent)'
),
(
12, 'Undelete (via Parent)'
),
(
13, 'Association Add'
),
(
14, 'Association Remove'
),
(
15, 'Association Modify'
)
GO

SET IDENTITY_INSERT TST_HISTORY_CHANGESET_TYPE OFF; 

