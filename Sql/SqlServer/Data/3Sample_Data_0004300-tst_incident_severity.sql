/***************************************************************
**	Insert script for table TST_INCIDENT_SEVERITY
***************************************************************/
SET IDENTITY_INSERT TST_INCIDENT_SEVERITY ON; 

INSERT INTO TST_INCIDENT_SEVERITY
(
SEVERITY_ID, PROJECT_TEMPLATE_ID, NAME, COLOR, IS_ACTIVE
)
VALUES
(
1, 1, '1 - Critical', 'f47457', 1
),
(
2, 1, '2 - High', 'f29e56', 1
),
(
3, 1, '3 - Medium', 'f5d857', 1
),
(
4, 1, '4 - Low', 'f4f356', 1
),
(
5, 2, '1 - Critical', 'f47457', 1
),
(
6, 2, '2 - High', 'f29e56', 1
),
(
7, 2, '3 - Medium', 'f5d857', 1
),
(
8, 2, '4 - Low', 'f4f356', 1
),
(
9, 3, '1 - Critical', 'f47457', 1
),
(
10, 3, '2 - High', 'f29e56', 1
),
(
11, 3, '3 - Medium', 'f5d857', 1
),
(
12, 3, '4 - Low', 'f4f356', 1
),
(
13, 4, '1 - Critical', 'f47457', 1
),
(
14, 4, '2 - High', 'f29e56', 1
),
(
15, 4, '3 - Medium', 'f5d857', 1
),
(
16, 4, '4 - Low', 'f4f356', 1
)
GO

SET IDENTITY_INSERT TST_INCIDENT_SEVERITY OFF; 

