/***************************************************************
**	Insert script for table TST_RISK_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_RISK_STATUS ON; 

INSERT INTO TST_RISK_STATUS
(
RISK_STATUS_ID, NAME, IS_ACTIVE, IS_DEFAULT, IS_OPEN, POSITION, PROJECT_TEMPLATE_ID
)
VALUES
(
1, 'Identified', 1, 1, 1, 1, 1
),
(
2, 'Analyzed', 1, 0, 1, 2, 1
),
(
3, 'Evaluated', 1, 0, 1, 3, 1
),
(
4, 'Open', 1, 0, 1, 4, 1
),
(
5, 'Closed', 1, 0, 0, 5, 1
),
(
6, 'Rejected', 1, 0, 0, 6, 1
),
(
7, 'Identified', 1, 1, 1, 1, 2
),
(
8, 'Analyzed', 1, 0, 1, 2, 2
),
(
9, 'Evaluated', 1, 0, 1, 3, 2
),
(
10, 'Open', 1, 0, 1, 4, 2
),
(
11, 'Closed', 1, 0, 0, 5, 2
),
(
12, 'Rejected', 1, 0, 0, 6, 2
),
(
13, 'Identified', 1, 1, 1, 1, 3
),
(
14, 'Analyzed', 1, 0, 1, 2, 3
),
(
15, 'Evaluated', 1, 0, 1, 3, 3
),
(
16, 'Open', 1, 0, 1, 4, 3
),
(
17, 'Closed', 1, 0, 0, 5, 3
),
(
18, 'Rejected', 1, 0, 0, 6, 3
),
(
19, 'Identified', 1, 1, 1, 1, 4
),
(
20, 'Analyzed', 1, 0, 1, 2, 4
),
(
21, 'Evaluated', 0, 0, 1, 3, 4
),
(
22, 'Open', 1, 0, 1, 4, 4
),
(
23, 'Closed', 1, 0, 0, 5, 4
),
(
24, 'Rejected', 1, 0, 0, 6, 4
)
GO

SET IDENTITY_INSERT TST_RISK_STATUS OFF; 

