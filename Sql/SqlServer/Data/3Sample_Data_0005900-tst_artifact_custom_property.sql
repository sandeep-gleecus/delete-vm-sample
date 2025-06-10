/***************************************************************
**	Insert script for table TST_ARTIFACT_CUSTOM_PROPERTY
***************************************************************/
INSERT INTO TST_ARTIFACT_CUSTOM_PROPERTY
(
ARTIFACT_ID, ARTIFACT_TYPE_ID, PROJECT_ID, CUST_01, CUST_02, CUST_03, CUST_04, CUST_05, CUST_06, CUST_07, CUST_08, CUST_09
)
VALUES
(
4, 1, 1, 'http://www.libraries.org', '2', NULL, NULL, '2012-07-03T16:25:12.000', '1', '1.234', NULL, NULL
),
(
2, 2, 1, 'http://www.libraryreferences.org', '6', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
6, 3, 1, 'This may be hard to reproduce', '9', NULL, 'N', '1', '2012-07-03T16:25:12.000', '1', '1', '1.234'
),
(
7, 3, 1, 'May be an array bounds issue', '13', '14,15,16', 'Y', '2', '2012-07-04T05:12:09.000', '2', '2', '0.998'
),
(
1, 4, 1, 'This is the first version of the system', '10', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
1, 5, 1, '14', '13', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
2, 5, 1, '15', '12', 'Testing with SP 2 applied', NULL, NULL, NULL, NULL, NULL, NULL
),
(
12, 5, 1, '16', '9', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
3, 7, 1, 'Some Data', '17', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
3, 8, 1, 'Need to test against the Win8 box', '13', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
4, 8, 1, 'Need to test against the Vista box', '10', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
1, 9, 1, '14', '13', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
2, 9, 1, '14', '10', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
3, 9, 1, '15', '10', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
4, 9, 1, '14', '12', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
2, 13, 1, '4', 'Need to review', NULL, NULL, NULL, NULL, NULL, NULL, NULL
),
(
1, 13, 1, '5', 'Draft mode only', NULL, NULL, NULL, NULL, NULL, NULL, NULL
)
GO

