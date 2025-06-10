/***************************************************************
**	Insert script for table TST_WORKFLOW
***************************************************************/
SET IDENTITY_INSERT TST_WORKFLOW ON; 

INSERT INTO TST_WORKFLOW
(
WORKFLOW_ID, PROJECT_TEMPLATE_ID, NAME, IS_DEFAULT, IS_NOTIFY, IS_ACTIVE
)
VALUES
(
1, '1', 'Default Workflow', 1, 1, 1
),
(
2, '2', 'Default Workflow', 1, 0, 1
),
(
3, '3', 'Default Workflow', 1, 1, 1
),
(
4, '4', 'Default Workflow', 1, 1, 1
)
GO

SET IDENTITY_INSERT TST_WORKFLOW OFF; 

