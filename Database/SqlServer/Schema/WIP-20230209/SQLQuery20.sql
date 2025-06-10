DECLARE @cols AS NVARCHAR(MAX),
 @query AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(tct.[NAME]) 
  from TST_TEST_CASE tc inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID
  inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID
  group by tct.[NAME]--, tc.TEST_CASE_ID
  order by tct.[NAME]
  --order by tc.TEST_CASE_ID
 FOR XML PATH(''), TYPE
 ).value('.', 'NVARCHAR(MAX)') 
 ,1,1,'')

SELECT @cols
set @query = N'SELECT  [TestPreparationStatusKey], ' + @cols + N' from 
 (
 select tcps.[NAME] AS  [TestPreparationStatusKey], tc.TEST_CASE_PREPARATION_STATUS_ID, tct.[NAME] AS [TestCaseTypeName]
 from TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = 1
inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID
 ) x
 pivot 
 (
 COUNT(TEST_CASE_PREPARATION_STATUS_ID)
 for [TestCaseTypeName] in (' + @cols + N')
 ) p '

print @query
exec sp_executesql @query;



--TST_TEST_CASE_TYPE


-- select tcps.[NAME], tc.TEST_CASE_PREPARATION_STATUS_ID
-- from TST_TEST_CASE tc
--inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = 1


SELECT  [TestPreparationStatusKey], [Exploratory],[Functional],[Regression],[Scenario] from 
(
select tcps.[NAME] AS  [TestPreparationStatusKey], tc.TEST_CASE_PREPARATION_STATUS_ID, tct.[NAME] AS [TestCaseTypeName]
from TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = 1
inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID
) x
pivot 
(
COUNT(TEST_CASE_PREPARATION_STATUS_ID)
for [TestCaseTypeName] in ([Exploratory],[Functional],[Regression],[Scenario])
) p 
/*******************************************************************************************************************************************************/
SELECT  [TestPreparationStatusKey], [Exploratory],[Functional],[Regression],[Scenario] from 
(
select tcps.[NAME] AS  [TestPreparationStatusKey], tc.TEST_CASE_PREPARATION_STATUS_ID, tcps.[NAME] AS [TestCaseTypeName]
from TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = 1
inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID
) x
pivot 
(
COUNT(TEST_CASE_PREPARATION_STATUS_ID)
for [TestCaseTypeName] in ([Exploratory],[Functional],[Regression],[Scenario])
) p 
