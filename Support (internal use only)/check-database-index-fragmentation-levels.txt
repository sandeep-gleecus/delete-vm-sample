DECLARE @DBNAME VARCHAR(130);
SET @DBNAME = 'MYDBNAME';

DECLARE @DBID INT;
SET @DBID = DB_ID(@DBNAME);

SELECT
OBJECT_ID AS objectID
, index_id AS indexID
, avg_fragmentation_in_percent AS fragmentation
, page_count 
INTO #indexDefragList
FROM
sys.dm_db_index_physical_stats 
(@DBID, NULL, NULL , NULL, N'Limited')
WHERE
index_id > 0
OPTION (MaxDop 1);

SELECT
i.[name] as indexname,
d.fragmentation,
d.page_count
FROM
#indexDefragList d
INNER JOIN sys.indexes i
ON d.objectid = i.object_id
ORDER BY 
d.fragmentation DESC

DROP TABLE #indexDefragList