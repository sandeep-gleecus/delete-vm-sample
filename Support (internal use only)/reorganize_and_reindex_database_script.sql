    SET NOCOUNT ON
    GO

    DECLARE @FillFactor TINYINT
    SELECT @FillFactor = 80
    DECLARE @StartTime DATETIME
    SELECT @StartTime = GETDATE()

    IF object_id('tempdb..#TablesToRebuildIndex') IS NOT NULL
    BEGIN
                DROP TABLE #TablesToRebuildIndex
    END

    DECLARE @NumTables VARCHAR(20)
    SELECT s.[Name] AS SchemaName
                ,t.[name] AS TableName
                ,SUM(p.rows) AS RowsInTable
                ,MAX(indexstats.avg_fragmentation_in_percent) AS Fragmentation
    INTO #TablesToRebuildIndex
    FROM sys.schemas s
    LEFT JOIN sys.tables t ON s.schema_id = t.schema_id
    LEFT JOIN sys.partitions p ON t.object_id = p.object_id
    LEFT JOIN sys.allocation_units a ON p.partition_id = a.container_id
    INNER JOIN sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS indexstats ON t.[object_id] = indexstats.[object_id]
    WHERE p.index_id IN (0,1)
                AND p.rows IS NOT NULL
                AND a.type = 1
                AND indexstats.avg_fragmentation_in_percent > 5
    GROUP BY s.[Name] ,t.[name]
    SELECT @NumTables = @@ROWCOUNT

    DECLARE RebuildIndex CURSOR FOR
    SELECT ROW_NUMBER() OVER (ORDER BY ttus.RowsInTable)
                ,ttus.SchemaName
                ,ttus.TableName
                ,ttus.RowsInTable
                ,ttus.Fragmentation
    FROM #TablesToRebuildIndex AS ttus
    ORDER BY ttus.RowsInTable

    OPEN RebuildIndex

    DECLARE @TableNumber VARCHAR(20)
    DECLARE @SchemaName NVARCHAR(128)
    DECLARE @tableName NVARCHAR(128)
    DECLARE @RowsInTable VARCHAR(20)
    DECLARE @Statement NVARCHAR(300)
    DECLARE @Status NVARCHAR(300)
    DECLARE @Fragmentation INT

    FETCH NEXT
    FROM RebuildIndex
    INTO @TableNumber
                ,@SchemaName
                ,@tablename
                ,@RowsInTable
                ,@Fragmentation

    WHILE (@@FETCH_STATUS = 0)
    BEGIN
		IF @Fragmentation >= 30
		BEGIN
                SET @Status = 'Table ' + @TableNumber + ' of ' + @NumTables + ': Rebuilding indexes on ' + @SchemaName + '.' + @tablename + ' (' + @RowsInTable + ' rows)'
                RAISERROR (@Status,0,1)       WITH NOWAIT

                SET @Statement = 'ALTER INDEX ALL ON [' + @SchemaName + '].[' + @tablename + '] REBUILD WITH (FILLFACTOR = ' + CONVERT(VARCHAR(3), @FillFactor) + ' )'
                EXEC sp_executesql @Statement
		END
		ELSE
		BEGIN
                SET @Status = 'Table ' + @TableNumber + ' of ' + @NumTables + ': Reorganize indexes on ' + @SchemaName + '.' + @tablename + ' (' + @RowsInTable + ' rows)'
                RAISERROR (@Status,0,1)       WITH NOWAIT

                SET @Statement = 'ALTER INDEX ALL ON [' + @SchemaName + '].[' + @tablename + '] REORGANIZE'
                EXEC sp_executesql @Statement
		END

                FETCH NEXT
                FROM RebuildIndex
                INTO @TableNumber
                            ,@SchemaName
                            ,@tablename
                            ,@RowsInTable
                            ,@Fragmentation
    END
    CLOSE RebuildIndex
    DEALLOCATE RebuildIndex
    DROP TABLE #TablesToRebuildIndex
    PRINT 'Total Elapsed Time: ' + CONVERT(VARCHAR(100), DATEDIFF(minute, @StartTime, GETDATE())) + ' minutes'
    GO