-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ManagerBase
-- Description:		Gets the database server properties for the admin screen
-- =====================================================================
IF OBJECT_ID ( 'SYSINFO', 'P' ) IS NOT NULL 
    DROP PROCEDURE SYSINFO;
GO
CREATE PROCEDURE SYSINFO
AS
BEGIN
	SELECT
		CAST(SERVERPROPERTY('BuildClrVersion') AS NVARCHAR) AS BuildClrVersion,
		CAST(SERVERPROPERTY('Collation') AS NVARCHAR) AS Collation,
		CAST(SERVERPROPERTY('CollationId') AS BIGINT) AS CollationId,
		CAST(SERVERPROPERTY('ComparisonStyle') AS BIGINT) AS ComparisonStyle,
		CAST(SERVERPROPERTY('ComputerNamePhysicalNetBIOS') AS NVARCHAR) AS ComputerNamePhysicalNetBIOS,
		CAST(SERVERPROPERTY('Edition') AS NVARCHAR) AS Edition,
		CAST(SERVERPROPERTY('EditionId') AS BIGINT) AS EditionId,
		CAST(SERVERPROPERTY('EngineEdition') AS INT) AS EngineEdition,
		CAST(SERVERPROPERTY('HadrManagerStatus') AS SMALLINT) AS HadrManagerStatus,
		CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR) AS InstanceDefaultDataPath,
		CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS NVARCHAR) AS InstanceDefaultLogPath,
		CAST(SERVERPROPERTY('InstanceName') AS NVARCHAR) AS InstanceName,
		CAST(SERVERPROPERTY('IsAdvancedAnalyticsInstalled') AS BIT) AS IsAdvancedAnalyticsInstalled,
		CAST(SERVERPROPERTY('IsClustered') AS BIT) AS IsClustered,
		CAST(SERVERPROPERTY('IsFullTextInstalled') AS BIT) AS IsFullTextInstalled,
		CAST(SERVERPROPERTY('IsHadrEnabled') AS BIT) AS IsHadrEnabled,
		CAST(SERVERPROPERTY('IsIntegratedSecurityOnly') AS BIT) AS IsIntegratedSecurityOnly,
		CAST(SERVERPROPERTY('IsLocalDB') AS BIT) AS IsLocalDB,
		CAST(SERVERPROPERTY('IsPolybaseInstalled') AS BIT) AS IsPolybaseInstalled,
		CAST(SERVERPROPERTY('IsSingleUser') AS BIT) AS IsSingleUser,
		CAST(SERVERPROPERTY('IsXTPSupported') AS BIT) AS IsXTPSupported,
		CAST(SERVERPROPERTY('LCID') AS INT) AS LCID,
		CAST(SERVERPROPERTY('LicenseType') AS NVARCHAR) AS LicenseType,
		CAST(SERVERPROPERTY('MachineName') AS NVARCHAR) AS MachineName,
		CAST(SERVERPROPERTY('NumLicenses') AS INT) AS NumLicenses,
		CAST(SERVERPROPERTY('ProcessID') AS BIGINT) AS ProcessID,
		CAST(SERVERPROPERTY('ProductBuild') AS BIGINT) AS ProductBuild,
		CAST(SERVERPROPERTY('ProductBuildType') AS NVARCHAR) AS ProductBuildType,
		CAST(SERVERPROPERTY('ProductLevel') AS NVARCHAR) AS ProductLevel,
		CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) AS ProductMajorVersion,
		CAST(SERVERPROPERTY('ProductMinorVersion') AS INT) AS ProductMinorVersion,
		CAST(SERVERPROPERTY('ProductUpdateLevel') AS NVARCHAR) AS ProductUpdateLevel,
		CAST(SERVERPROPERTY('ProductUpdateReference') AS NVARCHAR) AS ProductUpdateReference,
		CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR) AS ProductVersion,
		CAST(SERVERPROPERTY('ResourceLastUpdateDateTime') AS DATETIME) AS ResourceLastUpdateDateTime,
		CAST(SERVERPROPERTY('ResourceVersion') AS NVARCHAR) AS ResourceVersion,
		CAST(SERVERPROPERTY('ServerName') AS NVARCHAR) AS ServerName,
		CAST(SERVERPROPERTY('SqlCharSet') AS SMALLINT) AS SqlCharSet,
		CAST(SERVERPROPERTY('SqlCharSetName') AS NVARCHAR) AS SqlCharSetName,
		CAST(SERVERPROPERTY('SqlSortOrder') AS SMALLINT) AS SqlSortOrder,
		CAST(SERVERPROPERTY('SqlSortOrderName') AS NVARCHAR) AS SqlSortOrderName,
		CAST(SERVERPROPERTY('FilestreamShareName') AS NVARCHAR) AS FilestreamShareName,
		CAST(SERVERPROPERTY('FilestreamConfiguredLevel') AS INT) AS FilestreamConfiguredLevel,
		CAST(SERVERPROPERTY('FilestreamEffectiveLevel') AS INT) AS FilestreamEffectiveLevel,
		@@SERVERNAME as SysServerName,
		@@VERSION as SysFullVersion
END
GO
