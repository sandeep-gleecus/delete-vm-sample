osql /E /S MACHINENAME\SQLEXPRESS /i "C:\SQL Server Databases\backup_spiratest_db.sql" /o "C:\SQL Server Databases\backup_spiratest_db.log"
copy "C:\SQL Server Databases\Backups\spiratest_db.bak" "\\RemoteServer\Backup\spiratest_db_%date:~0,3%.bak" /Y
