UPDATE TST_TEST_CASE
SET TEST_CASE_FOLDER_ID = NULL
FROM TST_TEST_CASE, TST_TEST_CASE_FOLDER
WHERE TST_TEST_CASE.TEST_CASE_FOLDER_ID=TST_TEST_CASE_FOLDER.TEST_CASE_FOLDER_ID AND TST_TEST_CASE.PROJECT_ID<>TST_TEST_CASE_FOLDER.PROJECT_ID