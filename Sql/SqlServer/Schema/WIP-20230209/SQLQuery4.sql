select so.name, sc.name, * from sysobjects so 
inner join syscolumns sc on so.id = sc.id 
where so.name = 'tst_usage_log'

select so.name, sc.name, * from sysobjects so 
inner join syscolumns sc on so.id = sc.id 
where so.name = 'tst_schedules'


select schema_name(tab.schema_id) as [schema_name], 
    pk.[name] as pk_name,
    ic.index_column_id as column_id,
    col.[name] as column_name, 
    tab.[name] as table_name
from sys.tables tab
    inner join sys.indexes pk
        on tab.object_id = pk.object_id 
        and pk.is_primary_key = 1
    inner join sys.index_columns ic
        on ic.object_id = pk.object_id
        and ic.index_id = pk.index_id
    inner join sys.columns col
        on pk.object_id = col.object_id
        and col.column_id = ic.column_id
--WHERE tab.name in ('tst_usage_log','tst_schedules')
order by schema_name(tab.schema_id),
    pk.[name],
    ic.index_column_id


SELECT * FROM information_schema.key_column_usage 
--WHERE TABLE_NAME IN  ('tst_usage_log','tst_schedules', 'tst_template')
WHERE COLUMN_NAME  in ('ScheduleId','TemplateId','LogId')
ORDER BY 7,6

SELECT * FROM TST_GLOBAL_SETTING

