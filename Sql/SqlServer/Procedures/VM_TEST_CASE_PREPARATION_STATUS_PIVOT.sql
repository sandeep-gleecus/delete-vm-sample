IF OBJECT_ID('VM_TEST_CASE_PREPARATION_STATUS_PIVOT') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_TEST_CASE_PREPARATION_STATUS_PIVOT]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2023.03.04
-- Description:	Build a dataset that dynamically collects data
-- Execution Example: EXEC VM_TEST_CASE_PREPARATION_STATUS_PIVOT 1
-- =============================================
CREATE PROCEDURE [dbo].[VM_TEST_CASE_PREPARATION_STATUS_PIVOT]
	@ProjectId INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	--build table variable to hold the names
	DECLARE @Names TABLE (Name VARCHAR(200))

	--Get unique instances of each TestCaseType
	INSERT INTO @Names
	SELECT DISTINCT NAME 
	FROM TST_TEST_CASE_TYPE 
	WHERE IS_ACTIVE = 1 AND PROJECT_TEMPLATE_ID = @ProjectId
	ORDER BY 1
	   
	--FORMAT THE COLUMNS FOR USE IN THE PIVOT TABLE
	DECLARE @cols varchar(200)
	SET @cols = NULL
	SELECT  @cols = COALESCE(@cols + '],[','') + [NAME] 
	FROM @Names
	SET @cols = '[' + @cols + ']'


	--BUILD THE PIVOT TABLE SQL
	DECLARE @sql VARCHAR(MAX)
	SET @sql = 'SELECT  [TestPreparationStatusKey], ' + @cols + ' FROM 
	(
	select tcps.[NAME] AS  [TestPreparationStatusKey], tc.TEST_CASE_PREPARATION_STATUS_ID, tct.[NAME] AS [TestCaseTypeName]
	from TST_TEST_CASE tc
	inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = ' + CONVERT(VARCHAR(16),@ProjectId) + '
	inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID AND tct.IS_ACTIVE = 1
	) x
	pivot 
	(
	COUNT(TEST_CASE_PREPARATION_STATUS_ID)
	for [TestCaseTypeName] in (' + @cols + ')
	) p'

	EXEC (@sql)

END
GO


