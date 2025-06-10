/***************************************************************
**	Insert script for table TST_WORKFLOW_TRANSITION_ROLE_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_WORKFLOW_TRANSITION_ROLE_TYPE ON; 

INSERT INTO TST_WORKFLOW_TRANSITION_ROLE_TYPE
(
WORKFLOW_TRANSITION_ROLE_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Execute', 1
),
(
2, 'Notify', 1
)
GO

SET IDENTITY_INSERT TST_WORKFLOW_TRANSITION_ROLE_TYPE OFF; 

