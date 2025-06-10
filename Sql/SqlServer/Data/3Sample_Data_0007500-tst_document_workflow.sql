/***************************************************************
**	Insert script for table TST_DOCUMENT_WORKFLOW
***************************************************************/
SET IDENTITY_INSERT TST_DOCUMENT_WORKFLOW ON; 

INSERT INTO TST_DOCUMENT_WORKFLOW
(
DOCUMENT_WORKFLOW_ID, PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT
)
VALUES
(
1, 1, 'Default Workflow', 1, 1
),
(
2, 2, 'Default Workflow', 1, 1
),
(
3, 3, 'Default Workflow', 1, 1
),
(
4, 4, 'Default Workflow', 1, 1
)
GO

SET IDENTITY_INSERT TST_DOCUMENT_WORKFLOW OFF; 

