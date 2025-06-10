/***************************************************************
**	Insert script for table TST_RISK_IMPACT
***************************************************************/
SET IDENTITY_INSERT TST_RISK_IMPACT ON; 

INSERT INTO TST_RISK_IMPACT
(
RISK_IMPACT_ID, PROJECT_TEMPLATE_ID, NAME, COLOR, IS_ACTIVE, POSITION, SCORE
)
VALUES
(
1, 1, 'Catastrophic', '32CD32', 1, 1, 5
),
(
2, 1, 'Critical', 'BF0000', 1, 2, 4
),
(
3, 1, 'Marginal', '1338BE', 1, 3, 3
),
(
4, 1, 'Negligible', 'FFFF33', 1, 4, 2
),
(
5, 2, 'Catastrophic', '32CD32', 1, 1, 5
),
(
6, 2, 'Critical', 'BF0000', 1, 2, 4
),
(
7, 2, 'Marginal', '1338BE', 1, 3, 3
),
(
8, 2, 'Negligible', 'FFFF33', 1, 4, 2
),
(
9, 3, 'Catastrophic', '32CD32', 1, 1, 5
),
(
10, 3, 'Critical', 'BF0000', 1, 2, 4
),
(
11, 3, 'Marginal', '1338BE', 1, 3, 3
),
(
12, 3, 'Negligible', 'FFFF33', 1, 4, 2
),
(
13, 4, 'Catastrophic', '32CD32', 1, 1, 5
),
(
14, 4, 'Critical', 'BF0000', 1, 2, 4
),
(
15, 4, 'Marginal', '1338BE', 1, 3, 3
),
(
16, 4, 'Negligible', 'FFFF33', 1, 4, 2
),
(
17, 1, 'Serious', 'FFA500', 1, 5, 1
),
(
18, 2, 'Serious', 'FFA500', 1, 5, 1
),
(
19, 3, 'Serious', 'FFA500', 1, 5, 1
),
(
20, 4, 'Serious', 'FFA500', 1, 5, 1
)

GO

SET IDENTITY_INSERT TST_RISK_IMPACT OFF; 

