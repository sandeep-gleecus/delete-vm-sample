--select * from TST_CUSTOM_PROPERTY_VALUE

--select * from TST_CUSTOM_PROPERTY_LIST


SELECT  
tcps.NAME
--,*
--cpv.NAME,
--SUM(CASE WHEN CAST(cp.CUST_02 AS int) = 6 THEN 1 ELSE 0 END) AS FUNCTIONAL,
--SUM(CASE WHEN CAST(cp.CUST_02 AS int) = 7 THEN 1 ELSE 0 END) AS REGRESSION,
--SUM(CASE WHEN CAST(cp.CUST_02 AS int) = 8 THEN 1 ELSE 0 END) AS PERFORMANCE
FROM TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID
--join TST_ARTIFACT_CUSTOM_PROPERTY cp on tc.TEST_CASE_ID = cp.ARTIFACT_ID
--join TST_CUSTOM_PROPERTY_VALUE cpv on cpv.CUSTOM_PROPERTY_VALUE_ID = CAST(cp.CUST_04 AS int)
--join TST_CUSTOM_PROPERTY_VALUE cpv1 on cpv1.CUSTOM_PROPERTY_VALUE_ID = CAST(cp.CUST_02 AS int)
where tc.IS_DELETED = 0 
AND tcps.IS_ACTIVE = 1
--and cp.ARTIFACT_TYPE_ID =2 
--and cp.PROJECT_ID = 1 
--and cpv.CUSTOM_PROPERTY_LIST_ID = 9
GROUP BY tcps.NAME, tcps.POSITION
Order BY tcps.POSITION


SELECT  *
--tcps.NAME
--,'Not Recorded'
--,'Recorded - Failed'
--,'Recorded - Issue'
--,'Recorded - Passed'
--,Position
FROM
(
SELECT tcps.NAME
--,COUNT(tc.TEST_CASE_PREPARATION_STATUS_ID) cnt
,tc.TEST_CASE_PREPARATION_STATUS_ID
,POSITION
FROM TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID
where tc.IS_DELETED = 0 
AND tcps.IS_ACTIVE = 1
AND tc.PROJECT_ID = 1
GROUP BY tcps.NAME, tcps.POSITION
) t
PIVOT (
	COUNT(TEST_CASE_PREPARATION_STATUS_ID)
	FOR tcps.NAME IN ('Not Recorded','Recorded - Failed','Recorded - Issue','Recorded - Passed',Position)
) pvt;

Order BY tcps.POSITION






DECLARE @cols AS NVARCHAR(MAX),
    @query  AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(tcps.[NAME]) 
                    from  TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID
                    group by tcps.[NAME]--, tc.TEST_CASE_ID
                    order by tc.TEST_CASE_ID
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

SELECT @cols
set @query = N'SELECT ' + @cols + N' from 
             (
                select tc.TEST_CASE_PREPARATION_STATUS_ID, tcps.[NAME]
                from from  TST_TEST_CASE tc
inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID
            ) x
            pivot 
            (
                COUNT(tc.TEST_CASE_PREPARATION_STATUS_ID)
                for tcps.[NAME] in (' + @cols + N')
            ) p '

print  @query
exec sp_executesql @query;



SELECT [Recorded - Failed],[Not Recorded],[Recorded - Issue],[Recorded - Passed]from                
(                  select tc.NAME AS TestCaseName, tc.TEST_CASE_PREPARATION_STATUS_ID, tcps.[NAME]                  
from 
TST_TEST_CASE tc  inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID              ) x              
pivot               
(                  
COUNT(tc.TEST_CASE_PREPARATION_STATUS_ID)                  
for tcps.[NAME] in ([Recorded - Failed],[Not Recorded],[Not Recorded],[Recorded - Failed],[Recorded - Issue],[Recorded - Failed],[Not Recorded],[Not Recorded],[Recorded - Passed],[Recorded - Issue],[Recorded - Issue],[Recorded - Passed],[Recorded - Issue])              ) p 
