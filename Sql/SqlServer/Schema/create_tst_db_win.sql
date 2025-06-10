-----------------------------------------------------------------------------------------------------
--Description:	Creates the SpiraTest/Plan/Team database
--				using Windows Authentication
--
--Author: 	Inflectra Corporation
--
--Date:		11/27/2024
-----------------------------------------------------------------------------------------------------

--Create new database
CREATE DATABASE [[AUDITDBNAME]]
ON PRIMARY
(
  NAME = '[AUDITDBNAME]_Data',
  FILENAME = '[DBDATAPATH]\[AUDITDBNAME]Data.mdf',
  SIZE = [DATASIZE]MB,
  MAXSIZE = UNLIMITED,
  FILEGROWTH = [DATAGROWTH]MB
)
LOG ON
( NAME = '[AUDITDBNAME]_Log',
  FILENAME = '[DBDATAPATH]\[AUDITDBNAME]Log.ldf',
  SIZE = [LOGSIZE]MB,
  MAXSIZE = UNLIMITED,
  FILEGROWTH = [LOGGROWTH]MB
)
GO

--Set to not auto close
ALTER DATABASE [[AUDITDBNAME]]
SET AUTO_CLOSE OFF WITH NO_WAIT
GO

--Create Application Login and User Rights
USE [[AUDITDBNAME]]
CREATE LOGIN [[WINLOGIN]] FROM WINDOWS
GO

--Need this code when migrating from v2.X instances of Spira--
EXEC sp_grantlogin '[WINLOGIN]'
GO

USE [[AUDITDBNAME]]
CREATE USER [[WINLOGIN]] FOR LOGIN [[WINLOGIN]]
GO

USE [[AUDITDBNAME]]
EXEC sp_addrolemember 'db_owner', '[WINLOGIN]'
GO


-----------------------------------------------------------------------------------------------------
--Description:	Creates the SpiraTest/Plan/Team database
--				using Windows Authentication
--
--Author: 	Inflectra Corporation
--
--Date:		9/5/2012
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
CREATE LOGIN [[WINLOGIN]] FROM WINDOWS
GO

--Need this code when migrating from v2.X instances of Spira--
EXEC sp_grantlogin '[WINLOGIN]'
GO

USE [[DBNAME]]
CREATE USER [[WINLOGIN]] FOR LOGIN [[WINLOGIN]]
GO

USE [[DBNAME]]
EXEC sp_addrolemember 'db_owner', '[WINLOGIN]'
GO
