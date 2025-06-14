BACKUP DATABASE [SpiraTest] TO  DISK = N'C:\SQL Server Databases\Backups\spiratest_db.bak' WITH NOFORMAT, INIT,  NAME = N'SpiraTest-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO
declare @backupSetId as int
select @backupSetId = position from msdb..backupset where database_name=N'SpiraTest' and backup_set_id=(select max(backup_set_id) from msdb..backupset where database_name=N'SpiraTest' )
if @backupSetId is null begin raiserror(N'Verify failed. Backup information for database ''SpiraTest'' not found.', 16, 1) end
RESTORE VERIFYONLY FROM  DISK = N'C:\SQL Server Databases\Backups\spiratest_db.bak' WITH  FILE = @backupSetId,  NOUNLOAD,  NOREWIND
GO
