/***************************************************************
**	Insert script for table TST_WORKSPACE_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_WORKSPACE_TYPE ON; 

INSERT INTO TST_WORKSPACE_TYPE
(
WORKSPACE_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Product', 1
),
(
2, 'Program', 1
),
(
3, 'Template', 1
),
(
4, 'Portfolio', 1
),
(
5, 'Enterprise', 1
)
GO

SET IDENTITY_INSERT TST_WORKSPACE_TYPE OFF; 

