/***************************************************************
**	Insert script for table TST_REQUIREMENT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_REQUIREMENT_TYPE ON; 

INSERT INTO TST_REQUIREMENT_TYPE
(
REQUIREMENT_TYPE_ID, NAME, ICON, IS_ACTIVE, IS_DEFAULT, IS_STEPS, REQUIREMENT_WORKFLOW_ID, PROJECT_TEMPLATE_ID, IS_KEY_TYPE
)
VALUES
(
-1, 'Epic', 'artifact-RequirementSummary.svg', 1, 0, 0, NULL, NULL, 0
)
GO

SET IDENTITY_INSERT TST_REQUIREMENT_TYPE OFF; 

