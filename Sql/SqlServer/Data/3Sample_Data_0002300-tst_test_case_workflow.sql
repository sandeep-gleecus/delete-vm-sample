/***************************************************************
**	Insert script for table TST_TEST_CASE_WORKFLOW
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE_WORKFLOW ON; 

INSERT INTO TST_TEST_CASE_WORKFLOW
(
TEST_CASE_WORKFLOW_ID, PROJECT_TEMPLATE_ID, NAME, IS_DEFAULT, IS_ACTIVE
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

SET IDENTITY_INSERT TST_TEST_CASE_WORKFLOW OFF; 

