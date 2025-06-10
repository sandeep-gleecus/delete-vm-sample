/***************************************************************
**	Insert script for table TST_TEST_CASE_PARAMETER
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE_PARAMETER ON; 

INSERT INTO TST_TEST_CASE_PARAMETER
(
TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
)
VALUES
(
1, 16, 'url', 'http://www.libraryinformationsystem.com'
),
(
2, 17, 'login', NULL
),
(
3, 17, 'password', NULL
),
(
4, 16, 'browserName', 'browser'
),
(
5, 20, 'name', 'Roald Dahl'
),
(
6, 20, 'age', '105'
),
(
7, 21, 'name', 'Charlie and the Chocolate Factory'
),
(
8, 21, 'author', 'Roald Dahl'
),
(
9, 21, 'genre', 'Fantasy'
),
(
10, 16, 'operatingSystem', NULL
)
GO

SET IDENTITY_INSERT TST_TEST_CASE_PARAMETER OFF; 

