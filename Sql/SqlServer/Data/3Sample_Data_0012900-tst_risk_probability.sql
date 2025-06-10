/***************************************************************
**	Insert script for table TST_RISK_PROBABILITY
***************************************************************/
SET IDENTITY_INSERT TST_RISK_PROBABILITY ON; 

INSERT INTO TST_RISK_PROBABILITY
(
RISK_PROBABILITY_ID, PROJECT_TEMPLATE_ID, NAME, COLOR, IS_ACTIVE, POSITION, SCORE
)
VALUES
(
1, 1, 'Certain', 'BF0000', 1, 1, 5
),
(
2, 1, 'Likely', '32CD32', 1, 2, 4
),
(
3, 1, 'Possible', 'FFA500', 1, 3, 3
),
(
4, 1, 'Unlikely', '1338BE', 1, 4, 2
),
(
5, 1, 'Rare', 'FFFF33', 1, 5, 1
),
(
6, 2, 'Certain', 'BF0000', 1, 1, 5
),
(
7, 2, 'Likely', '32CD32', 1, 2, 4
),
(
8, 2, 'Possible', 'FFA500', 1, 3, 3
),
(
9, 2, 'Unlikely', '1338BE', 1, 4, 2
),
(
10, 2, 'Rare', 'FFFF33', 1, 5, 1
),
(
11, 3, 'Certain', 'BF0000', 1, 1, 5
),
(
12, 3, 'Likely', '32CD32', 1, 2, 4
),
(
13, 3, 'Possible', 'FFA500', 1, 3, 3
),
(
14, 3, 'Unlikely', '1338BE', 1, 4, 2
),
(
15, 3, 'Rare', 'FFFF33', 1, 5, 1
),
(
16, 4, 'Certain', 'BF0000', 1, 1, 5
),
(
17, 4, 'Likely', '32CD32', 1, 2, 4
),
(
18, 4, 'Possible', 'FFA500', 1, 3, 3
),
(
19, 4, 'Unlikely', '1338BE', 1, 4, 2
),
(
20, 4, 'Rare', 'FFFF33', 1, 5, 1
)
GO

SET IDENTITY_INSERT TST_RISK_PROBABILITY OFF; 

