-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: SystemManager
-- Description:		Checks to see if SQL Full Text Indexing is Installed
-- =============================================
IF OBJECT_ID ( 'SYSTEM_CHECK_FULLTEXT_INDEXING', 'P' ) IS NOT NULL 
    DROP PROCEDURE [SYSTEM_CHECK_FULLTEXT_INDEXING];
GO
CREATE PROCEDURE [SYSTEM_CHECK_FULLTEXT_INDEXING]
AS
BEGIN
	SELECT CAST (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS BIT) AS IS_FULL_TEXT_INSTALLED
END
GO
