/***************************************************************
**	Insert script for table TST_CUSTOM_PROPERTY_VALUE
***************************************************************/
SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_VALUE ON; 

INSERT INTO TST_CUSTOM_PROPERTY_VALUE
(
CUSTOM_PROPERTY_VALUE_ID, CUSTOM_PROPERTY_LIST_ID, NAME, IS_ACTIVE, IS_DELETED
)
VALUES
(
1, 1, 'Difficult', 1, 0
),
(
2, 1, 'Moderate', 1, 0
),
(
3, 1, 'Easy', 1, 0
),
(
4, 2, 'Statutory', 1, 0
),
(
5, 2, 'Regulatory', 1, 0
),
(
6, 3, 'Functional Test', 1, 0
),
(
7, 3, 'Regression Test', 1, 0
),
(
8, 3, 'Performance Test', 1, 0
),
(
9, 4, 'Linux', 1, 0
),
(
10, 4, 'Mac OS X', 1, 0
),
(
11, 4, 'Windows Server', 1, 0
),
(
12, 4, 'Windows 8', 1, 0
),
(
13, 4, 'Windows 10', 1, 0
),
(
14, 5, 'Internet Explorer', 1, 0
),
(
15, 5, 'Firefox', 1, 0
),
(
16, 5, 'Chrome', 1, 0
),
(
17, 5, 'Safari', 1, 0
),
(
18, 6, 'Scenario Step', 1, 0
),
(
19, 6, 'Verification Point', 1, 0
),
(
20, 4, 'iOS', 1, 0
),
(
21, 4, 'Android', 1, 0
),
(
22, 7, 'librarian', 1, 0
),
(
23, 7, 'borrower', 1, 0
),
(
24, 7, 'admin', 1, 0
),
(
25, 8, 'librarian', 1, 0
),
(
26, 8, 'borrower', 1, 0
),
(
27, 8, 'PleaseChange', 1, 0
),
(
28, 6, 'Basic Function Step', 1, 0
),
(
29, 6, 'None', 1, 0
),
(
30, 6, 'Non-Proving Step', 1, 0
),
(
31, 6, 'Proving Step', 1, 0
),
(
32, 6, 'Supporting Step', 1, 0
)
GO

SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_VALUE OFF; 

