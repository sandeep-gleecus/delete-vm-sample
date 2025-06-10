-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Sets
-- =============================================
IF OBJECT_ID ( 'MIGRATION_REFRESH_PROJECT_TEST_CACHE', 'P' ) IS NOT NULL 
    DROP PROCEDURE MIGRATION_REFRESH_PROJECT_TEST_CACHE;
GO
CREATE PROCEDURE MIGRATION_REFRESH_PROJECT_TEST_CACHE
	@ProjectId INT
AS
DECLARE
	@TestCaseId INT,
	@TestCaseFolderId INT,
	@TestSetId INT,
	@TestSetFolderId INT
BEGIN
	SET NOCOUNT ON;
	
	--First refresh the test cases
	DECLARE TestCaseCursor CURSOR LOCAL FOR
		SELECT TEST_CASE_ID
		FROM TST_TEST_CASE
		WHERE PROJECT_ID = @ProjectId
		ORDER BY TEST_CASE_ID
		
	--Loop
	OPEN TestCaseCursor   
	FETCH NEXT FROM TestCaseCursor INTO @TestCaseId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3 @ProjectId, @TestCaseId
		FETCH NEXT FROM TestCaseCursor INTO @TestCaseId
	END   

	--Clean up
	CLOSE TestCaseCursor   
	DEALLOCATE TestCaseCursor
	
	--Next refresh the test case folders in reverse hierarchy order
	CREATE TABLE #tblTestCaseFolders
	(
		TEST_CASE_FOLDER_ID INT,
		PROJECT_ID INT,
		NAME NVARCHAR(255),
		PARENT_TEST_CASE_FOLDER_ID INT,
		HIERARCHY_LEVEL INT,
		INDENT_LEVEL NVARCHAR(100)
	)
	INSERT INTO #tblTestCaseFolders(TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	EXEC TESTCASE_RETRIEVE_FOLDER_HIERARCHY @ProjectId
	
	DECLARE TestCaseFolderCursor CURSOR LOCAL FOR
		SELECT TEST_CASE_FOLDER_ID
		FROM #tblTestCaseFolders
		ORDER BY INDENT_LEVEL DESC, TEST_CASE_FOLDER_ID ASC
		
	--Loop
	OPEN TestCaseFolderCursor   
	FETCH NEXT FROM TestCaseFolderCursor INTO @TestCaseFolderId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTCASE_REFRESH_FOLDER_EXECUTION_STATUS @ProjectId, @TestCaseFolderId
		FETCH NEXT FROM TestCaseFolderCursor INTO @TestCaseFolderId
	END   

	--Clean up
	CLOSE TestCaseFolderCursor   
	DEALLOCATE TestCaseFolderCursor
	
	--First refresh the test sets	
	DECLARE TestSetCursor CURSOR LOCAL FOR
		SELECT TEST_SET_ID
		FROM TST_TEST_SET
		WHERE PROJECT_ID = @ProjectId
		ORDER BY TEST_SET_ID
		
	--Loop
	OPEN TestSetCursor   
	FETCH NEXT FROM TestSetCursor INTO @TestSetId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTSET_REFRESH_EXECUTION_DATA @ProjectId, @TestSetId
		FETCH NEXT FROM TestSetCursor INTO @TestSetId
	END   

	--Clean up
	CLOSE TestSetCursor   
	DEALLOCATE TestSetCursor
	
	--Next refresh the test set folders in reverse hierarchy order
	CREATE TABLE #tblTestSetFolders
	(		
		TEST_SET_FOLDER_ID INT,
		PROJECT_ID INT,
		NAME NVARCHAR(255),
		PARENT_TEST_SET_FOLDER_ID INT,
		HIERARCHY_LEVEL INT,
		INDENT_LEVEL NVARCHAR(100)
	)
	INSERT INTO #tblTestSetFolders(TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	EXEC TESTSET_RETRIEVE_FOLDER_HIERARCHY @ProjectId

	DECLARE TestSetFolderCursor CURSOR LOCAL FOR
		SELECT TEST_SET_FOLDER_ID
		FROM #tblTestSetFolders
		ORDER BY INDENT_LEVEL DESC, TEST_SET_FOLDER_ID ASC
		
	--Loop
	OPEN TestSetFolderCursor   
	FETCH NEXT FROM TestSetFolderCursor INTO @TestSetFolderId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTSET_REFRESH_FOLDER_EXECUTION_STATUS @ProjectId, @TestSetFolderId
		FETCH NEXT FROM TestSetFolderCursor INTO @TestSetFolderId
	END   

	--Clean up
	CLOSE TestSetFolderCursor   
	DEALLOCATE TestSetFolderCursor
END
GO
