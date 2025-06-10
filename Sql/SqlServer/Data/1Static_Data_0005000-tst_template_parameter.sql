/***************************************************************
**	Insert script for table TST_TEMPLATE_PARAMETER
***************************************************************/
SET IDENTITY_INSERT TST_TEMPLATE_PARAMETER ON; 

INSERT INTO TST_TEMPLATE_PARAMETER
(
PARAMETERID, TEMPLATEID, PARAMETERLABEL, PARAMETERTYPE
)
VALUES
(
2051, 3098, 'TestCaseID', 'NUMBER'
),
(
2052, 3099, 'TestCaseID', 'NUMBER'
),
(
2053, 3100, 'TestRunID', 'NUMBER'
),
(
2054, 3101, 'TestRunID', 'NUMBER'
),
(
2055, 3102, 'ProjectID', 'NUMBER'
),
(
2056, 3103, 'ProjectID', 'NUMBER'
)
GO

SET IDENTITY_INSERT TST_TEMPLATE_PARAMETER OFF; 

