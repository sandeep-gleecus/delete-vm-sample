/***************************************************************
**	Insert script for table TST_WORKFLOW_FIELD_STATE
***************************************************************/
SET IDENTITY_INSERT TST_WORKFLOW_FIELD_STATE ON; 

INSERT INTO TST_WORKFLOW_FIELD_STATE
(
WORKFLOW_FIELD_STATE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Inactive', 1
),
(
2, 'Required', 1
),
(
3, 'Hidden', 1
)
GO

SET IDENTITY_INSERT TST_WORKFLOW_FIELD_STATE OFF; 

