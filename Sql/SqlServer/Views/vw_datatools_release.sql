IF OBJECT_ID ( 'VW_DATATOOLS_RELEASE', 'V' ) IS NOT NULL 
    DROP VIEW [VW_DATATOOLS_RELEASE];
GO

CREATE VIEW [VW_DATATOOLS_RELEASE]
AS
	SELECT RELEASE_ID, IS_SUMMARY, INDENT_LEVEL, IS_DELETED, PROJECT_ID
	FROM TST_RELEASE
GO
