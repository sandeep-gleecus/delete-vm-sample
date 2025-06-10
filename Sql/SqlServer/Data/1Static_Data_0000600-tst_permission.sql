/***************************************************************
**	Insert script for table TST_PERMISSION
***************************************************************/
SET IDENTITY_INSERT TST_PERMISSION ON; 

INSERT INTO TST_PERMISSION
(
PERMISSION_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Permission_Create', 1
),
(
2, 'Permission_Modify', 1
),
(
3, 'Permission_Delete', 1
),
(
4, 'Permission_View', 1
),
(
5, 'Permission_LimitedModify', 1
),
(
6, 'Permission_BulkEdit', 1
)
GO

SET IDENTITY_INSERT TST_PERMISSION OFF; 

