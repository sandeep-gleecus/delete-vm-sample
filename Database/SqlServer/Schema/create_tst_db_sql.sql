-----------------------------------------------------------------------------------------------------
--Description:	Creates the ValidationMaster database
--				using SQL Server Authentication
--
--Author: 	OnShore Technology Group, Inc.
--
--Date:		9/20/2010
-----------------------------------------------------------------------------------------------------

--Create new database
CREATE DATABASE [[DBNAME]]
ON PRIMARY
(
  NAME = '[DBNAME]_Data',
  FILENAME = '[DBDATAPATH]\[DBNAME]Data.mdf',
  SIZE = [DATASIZE]MB,
  MAXSIZE = UNLIMITED,
  FILEGROWTH = [DATAGROWTH]MB
)
LOG ON
( NAME = '[DBNAME]_Log',
  FILENAME = '[DBDATAPATH]\[DBNAME]Log.ldf',
  SIZE = [LOGSIZE]MB,
  MAXSIZE = UNLIMITED,
  FILEGROWTH = [LOGGROWTH]MB
)
GO

--Set to not auto close
ALTER DATABASE [[DBNAME]]
SET AUTO_CLOSE OFF WITH NO_WAIT
GO

--Create Application Login and User Rights
USE [[DBNAME]]
CREATE LOGIN [[DBLOGIN]] WITH PASSWORD = '[DBPASSWORD]'
GO
USE [[DBNAME]]
CREATE USER [[DBUSER]] FOR LOGIN [[DBLOGIN]]
GO
USE [[DBNAME]]
EXEC sp_addrolemember 'db_owner', '[DBUSER]'
GO
