/***************************************************************
**	Insert script for table TST_CUSTOM_PROPERTY
***************************************************************/
SET IDENTITY_INSERT TST_CUSTOM_PROPERTY ON; 

INSERT INTO TST_CUSTOM_PROPERTY
(
CUSTOM_PROPERTY_ID, PROJECT_TEMPLATE_ID, ARTIFACT_TYPE_ID, CUSTOM_PROPERTY_TYPE_ID, NAME, PROPERTY_NUMBER, IS_DELETED, CUSTOM_PROPERTY_LIST_ID, LEGACY_NAME
)
VALUES
(
1, 1, 1, 1, 'URL', 1, 0, NULL, 'TEXT_01'
),
(
2, 1, 1, 6, 'Difficulty', 2, 0, 1, 'LIST_01'
),
(
3, 1, 1, 6, 'Classification', 3, 0, 2, 'LIST_02'
),
(
4, 1, 2, 1, 'URL', 1, 0, NULL, 'TEXT_01'
),
(
5, 1, 2, 6, 'Test Type', 2, 0, 3, 'LIST_01'
),
(
6, 1, 3, 1, 'Notes', 1, 0, NULL, 'TEXT_02'
),
(
7, 1, 3, 7, 'Operating System', 2, 0, 4, 'LIST_02'
),
(
8, 1, 4, 1, 'Notes', 1, 0, NULL, 'TEXT_02'
),
(
9, 1, 4, 7, 'Operating System', 2, 0, 4, 'LIST_02'
),
(
10, 1, 5, 7, 'Web Browser', 1, 0, 5, 'LIST_01'
),
(
11, 1, 5, 7, 'Operating System', 2, 0, 4, 'LIST_02'
),
(
12, 1, 5, 1, 'Notes', 3, 0, NULL, 'TEXT_01'
),
(
13, 1, 7, 1, 'Additional Data', 1, 0, NULL, 'TEXT_01'
),
(
14, 1, 7, 6, 'Step Type', 2, 0, 6, 'LIST_01'
),
(
15, 1, 8, 1, 'Notes', 1, 0, NULL, 'TEXT_01'
),
(
16, 1, 8, 7, 'Operating System', 2, 0, 4, 'LIST_01'
),
(
17, 1, 9, 7, 'Web Browser', 1, 0, 5, 'LIST_01'
),
(
18, 1, 9, 7, 'Operating System', 2, 0, 4, 'LIST_02'
),
(
19, 1, 1, 1, 'Notes', 4, 0, NULL, 'TEXT_02'
),
(
20, 1, 1, 5, 'Review Date', 5, 0, NULL, NULL
),
(
21, 1, 2, 8, 'Review Owner', 3, 0, NULL, NULL
),
(
22, 1, 3, 7, 'Web Browser', 3, 0, 5, NULL
),
(
23, 1, 3, 4, 'Internal?', 4, 0, NULL, NULL
),
(
24, 1, 3, 2, 'Ranking', 5, 0, NULL, NULL
),
(
25, 1, 9, 3, 'OS Version', 3, 0, NULL, NULL
),
(
26, 1, 3, 5, 'Review Date', 6, 0, NULL, NULL
),
(
27, 1, 3, 6, 'Difficulty', 7, 0, 1, 'LIST_01'
),
(
28, 1, 3, 8, 'Reviewer', 8, 0, NULL, NULL
),
(
29, 1, 3, 3, 'Decimal', 9, 0, NULL, NULL
),
(
30, 1, 6, 6, 'Difficulty', 1, 0, 1, 'LIST_01'
),
(
31, 1, 6, 8, 'Reviewer', 2, 0, NULL, NULL
),
(
32, 1, 1, 2, 'Ranking', 6, 0, NULL, NULL
),
(
33, 1, 1, 3, 'Decimal', 7, 0, NULL, NULL
),
(
34, 1, 13, 6, 'Classification', 1, 0, 2, 'LIST_01'
),
(
35, 1, 13, 1, 'Notes', 2, 0, NULL, 'TEXT_01'
)
GO

SET IDENTITY_INSERT TST_CUSTOM_PROPERTY OFF; 

