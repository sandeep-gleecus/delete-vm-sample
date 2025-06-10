/***************************************************************
**	Insert script for table TST_TEST_RUN_STEP
***************************************************************/
SET IDENTITY_INSERT TST_TEST_RUN_STEP ON; 

INSERT INTO TST_TEST_RUN_STEP
(
TEST_RUN_STEP_ID, TEST_RUN_ID, TEST_STEP_ID, TEST_CASE_ID, EXECUTION_STATUS_ID, DESCRIPTION, POSITION, EXPECTED_RESULT, SAMPLE_DATA, ACTUAL_RESULT, ACTUAL_DURATION, START_DATE, END_DATE
)
VALUES
(
1, 9, 19, 17, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, NULL, 600, DATEADD(minute, -211110, SYSUTCDATETIME()), DATEADD(minute, -211100, SYSUTCDATETIME())
),
(
2, 9, 2, 2, 2, 'User clicks link to create book', 2, 'User taken to first screen in wizard', NULL, NULL, 1800, DATEADD(minute, -211100, SYSUTCDATETIME()), DATEADD(minute, -211070, SYSUTCDATETIME())
),
(
3, 9, 3, 2, 1, 'User enters books name and author, then clicks Next', 3, 'User taken to next screen in wizard', 'Macbeth, William Shakespeare', 'An error page is displayed - "No such object or with block variable at line 473"', 2100, DATEADD(minute, -211070, SYSUTCDATETIME()), DATEADD(minute, -211035, SYSUTCDATETIME())
),
(
4, 3, 19, 17, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, 'As expected', 900, DATEADD(minute, -211080, SYSUTCDATETIME()), DATEADD(minute, -211065, SYSUTCDATETIME())
),
(
5, 3, 2, 2, 2, 'User clicks link to create book', 2, 'User taken to first screen in wizard', NULL, 'As expected', 600, DATEADD(minute, -211065, SYSUTCDATETIME()), DATEADD(minute, -211055, SYSUTCDATETIME())
),
(
6, 3, 3, 2, 2, 'User enters books name and author, then clicks Next', 3, 'User taken to next screen in wizard', 'Macbeth, William Shakespeare', 'As expected', 1500, DATEADD(minute, -211055, SYSUTCDATETIME()), DATEADD(minute, -211030, SYSUTCDATETIME())
),
(
7, 3, 4, 2, 2, 'User chooses book''s genre and sub-genre from list', 4, 'User sees screen displaying all entered information', 'Play, Tragedy', 'As expected', 1500, DATEADD(minute, -211030, SYSUTCDATETIME()), DATEADD(minute, -211005, SYSUTCDATETIME())
),
(
8, 3, 5, 2, 2, 'User clicks submit button', 5, 'Confirmation screen is displayed', NULL, 'As expected', 900, DATEADD(minute, -211005, SYSUTCDATETIME()), DATEADD(minute, -210989, SYSUTCDATETIME())
),
(
9, 11, 19, 17, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, NULL, 1500, DATEADD(minute, -196680, SYSUTCDATETIME()), DATEADD(minute, -196655, SYSUTCDATETIME())
),
(
10, 11, 7, 4, 2, 'User clicks link to create author', 2, 'User taken to first screen in wizard', NULL, NULL, 1500, DATEADD(minute, -196655, SYSUTCDATETIME()), DATEADD(minute, -196630, SYSUTCDATETIME())
),
(
11, 11, 8, 4, 2, 'User enters authors name and age', 3, 'User taken to next screen in wizard', 'MartinAMis, 39', NULL, 1500, DATEADD(minute, -196630, SYSUTCDATETIME()), DATEADD(minute, -196605, SYSUTCDATETIME())
),
(
12, 11, 9, 4, 2, 'User associates books with author', 4, 'User sees screen displaying all entered information', 'London Fields, Money, Informational', NULL, 600, DATEADD(minute, -196605, SYSUTCDATETIME()), DATEADD(minute, -196595, SYSUTCDATETIME())
),
(
13, 11, 10, 4, 1, 'User clicks submit button', 5, 'Confirmation screen is displayed', NULL, 'The confirmation screen doesn''t appear, instead you see a blank screen', 300, DATEADD(minute, -196595, SYSUTCDATETIME()), DATEADD(minute, -196589, SYSUTCDATETIME())
),
(
16, 12, 19, 17, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, NULL, 900, DATEADD(minute, -200940, SYSUTCDATETIME()), DATEADD(minute, -200925, SYSUTCDATETIME())
),
(
17, 12, 21, 3, 2, 'User clicks link to view existing books', 2, 'List of active books in system displayed', NULL, NULL, 900, DATEADD(minute, -200925, SYSUTCDATETIME()), DATEADD(minute, -200910, SYSUTCDATETIME())
),
(
18, 12, 22, 3, 6, 'User clicks on link to edit a specific book', 3, 'User taken to edit book details screen', NULL, 'Screen loads correctly, but is very slow', 1200, DATEADD(minute, -200910, SYSUTCDATETIME()), DATEADD(minute, -200889, SYSUTCDATETIME())
),
(
19, 2, 19, 17, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, NULL, 1800, DATEADD(minute, -202380, SYSUTCDATETIME()), DATEADD(minute, -202350, SYSUTCDATETIME())
),
(
20, 2, 24, 5, 2, 'User clicks link to view existing authors', 2, 'List of active authors in system displayed', NULL, NULL, 1500, DATEADD(minute, -202350, SYSUTCDATETIME()), DATEADD(minute, -202325, SYSUTCDATETIME())
),
(
21, 2, 25, 5, 5, 'User clicks on link to edit a specific author', 3, 'User taken to edit author details screen', NULL, 'Cannot get to screen as the create authors failed, so no authors in list', 2400, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
22, 17, 23, 4, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, 'As expected', 600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
23, 17, 7, 4, 2, 'User clicks link to create author', 2, 'User taken to first screen in wizard', NULL, 'As expected', 300, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
24, 17, 8, 4, 2, 'User enters authors name and age', 3, 'User taken to next screen in wizard', 'MartinAMis, 39', 'As expected', 900, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
25, 17, 9, 4, 2, 'User associates books with author', 4, 'User sees screen displaying all entered information', NULL, 'As expected', 900, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
26, 17, 10, 4, 1, 'User clicks submit button', 5, 'Confirmation screen is displayed', NULL, 'Clicking the submit button�yields a 403 forbidden error', 1500, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
27, 18, 23, 2, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, 'As expected', 600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
28, 18, 2, 2, 2, 'User clicks link to create book', 2, 'User taken to first screen in wizard', 'Macbeth, William Shakespeare', 'As expected', 300, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
29, 18, 3, 2, 2, 'User enters books name and author, then clicks Next', 3, 'User taken to next screen in wizard', NULL, 'As expected', 600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
30, 18, 4, 2, 2, 'User chooses book''s genre and sub-genre from list', 4, 'User sees screen displaying all entered information', NULL, 'As expected', 500, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
31, 18, 5, 2, 2, 'User clicks submit button', 5, 'Confirmation screen is displayed', NULL, 'As expected', 900, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
32, 19, 23, 5, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, 'As expected', 600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
33, 19, 24, 5, 2, 'User clicks link to view existing authors', 2, 'List of active authors in system displayed', NULL, 'As expected', 300, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
34, 19, 25, 5, 6, 'User clicks on link to edit a specific author', 3, 'User taken to edit author details screen', NULL, 'Clicking on link is clunky, it takes too long to open', 1200, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
35, 20, 23, 3, 2, 'User logs in to application', 1, 'User taken to main menu screen', NULL, 'As expected', 600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
36, 20, 21, 3, 2, 'User clicks on link to view existing books', 2, 'List of active books in system displayed', NULL, 'As expected', 300, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
37, 20, 22, 3, 2, 'User clicks on link to edit a specific book', 3, 'User taken to edit book details screen', NULL, 'As expected', 400, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
38, 24, 28, 9, 2, 'Call ''Ability to create new author''', 1, 'Tests that the user can create a new author record in the system', NULL, 'As expected', 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
39, 24, 29, 9, 2, 'Call ''Ability to edit existing author''', 2, 'Tests that the user can login, view the details of an author and then if he/she desires, make the necessary changes', NULL, 'As expected', 2400, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
40, 21, 26, 8, 2, 'Call ''Ability to create new book''', 1, 'Tests that the user can create a new book in the system', NULL, 'As expected', 1600, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
41, 21, 27, 8, 1, 'Call ''Ability to edit existing book''', 2, 'Tests that the user can login, view the details of a book, and then if he/she desires, make the necessary changes', NULL, 'Clicking the edit button�yields a 403 forbidden error', 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
42, 7, 45, 13, 2, '<p>Explore adding a new book and new author</p>', 1, '<p>Check for (examples):</p><ul><li>Cannot add a book with a blank author</li><li>Can add an author with no books</li><li>Can add a book with an author that has just been added</li><li>Can view newly added book page</li></ul>', NULL, '<p>All works as expected</p>', 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
43, 6, 17, 16, 2, 'User opens up browser and enters application URL: http://www.libraryinformationsystem.org', 1, 'The browser loads the login web page', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
44, 6, 19, 17, 2, 'User logs in to application', 2, 'User taken to main menu screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
45, 6, 21, 3, 2, 'User clicks link to view existing books', 3, 'List of active books in system displayed', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
46, 6, 22, 3, 2, 'User clicks on link to edit a specific book', 4, 'User taken to edit book details screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
48, 14, 45, 13, 2, '<p>Explore adding a new book and new author</p>', 1, '<p>Check for (examples):</p><ul><li>Cannot add a book with a blank author</li><li>Can add an author with no books</li><li>Can add a book with an author that has just been added</li><li>Can view newly added book page</li></ul>', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
49, 22, 17, 16, 2, 'User opens up browser and enters application URL: http://www.libraryinformationsystem.org', 1, 'The browser loads the login web page', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
50, 22, 19, 17, 2, 'User logs in to application', 2, 'User taken to main menu screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
51, 22, 7, 4, 2, 'User clicks link to create author', 3, 'User taken to first screen in wizard', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
52, 22, 8, 4, 2, 'User enters authors name and age', 4, 'User taken to next screen in wizard', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
53, 22, 9, 4, 2, 'User associates books with author', 5, 'User sees screen displaying all entered information', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
54, 22, 10, 4, 2, 'User clicks submit button', 6, 'Confirmation screen is displayed', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
55, 23, 17, 16, 2, 'User opens up Internet Explorer and enters application URL: http://www.libraryinformationsystem.org', 1, 'The browser loads the login web page', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
56, 23, 19, 17, 2, 'User logs in to application', 2, 'User taken to main menu screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
57, 23, 2, 2, 2, 'User clicks link to create book', 3, 'User taken to first screen in wizard', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
58, 23, 3, 2, 2, 'User enters books name and author, then clicks Next', 4, 'User taken to next screen in wizard', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
59, 23, 4, 2, 2, 'User chooses book''s genre and sub-genre from list', 5, 'User sees screen displaying all entered information', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
60, 23, 5, 2, 2, 'User clicks submit button', 6, 'Confirmation screen is displayed', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
61, 23, 17, 16, 2, 'User opens up Internet Explorer and enters application URL: http://www.libraryinformationsystem.org', 7, 'The browser loads the login web page', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
62, 23, 19, 17, 2, 'User logs in to application', 8, 'User taken to main menu screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
63, 23, 21, 3, 2, 'User clicks link to view existing books', 9, 'List of active books in system displayed', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
),
(
64, 23, 22, 3, 2, 'User clicks on link to edit a specific book', 10, 'User taken to edit book details screen', NULL, NULL, 2100, DATEADD(minute, -202325, SYSUTCDATETIME()), DATEADD(minute, -202284, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_TEST_RUN_STEP OFF; 

