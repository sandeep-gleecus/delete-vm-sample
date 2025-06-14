-- =====================================================================
-- Author:			Inflectra Corporation
-- Description:		Creates the SQL free text catalogs and indexes
-- =====================================================================

--See if Free Text Indexing is Installed
IF (0 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
	--Set the flag in global settings and don't create the catalogs/index
	IF EXISTS(SELECT VALUE FROM TST_GLOBAL_SETTING WHERE NAME = 'Database_UseFreeTextCatalogs')
	BEGIN
		UPDATE TST_GLOBAL_SETTING 
			SET [VALUE] = 
				(CASE FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')
					WHEN 1 THEN 'true'
					ELSE 'false'
				END)
		WHERE [NAME] = 'Database_UseFreeTextCatalogs'
	END
	ELSE
	BEGIN
		INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE)
		VALUES (
			'Database_UseFreeTextCatalogs',
			(CASE FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')
				WHEN 1 THEN 'true'
				ELSE 'false'
			END)
		)
	END
	SET NOEXEC ON
END
GO

-- See if we need to drop the existing catalogs
IF EXISTS (SELECT * FROM sysfulltextcatalogs ftc WHERE ftc.name = N'FT_ARTIFACTS')
BEGIN
    DROP FULLTEXT INDEX ON TST_REQUIREMENT;
    DROP FULLTEXT INDEX ON TST_TEST_CASE;
    DROP FULLTEXT INDEX ON TST_INCIDENT;
    DROP FULLTEXT INDEX ON TST_RELEASE;
    DROP FULLTEXT INDEX ON TST_TEST_RUN;
    DROP FULLTEXT INDEX ON TST_TASK;
    DROP FULLTEXT INDEX ON TST_TEST_STEP;
    DROP FULLTEXT INDEX ON TST_TEST_SET;
    DROP FULLTEXT INDEX ON TST_AUTOMATION_HOST;
    DROP FULLTEXT INDEX ON TST_ATTACHMENT;
    DROP FULLTEXT INDEX ON TST_BUILD;
    DROP FULLTEXT INDEX ON TST_RISK;
    DROP FULLTEXT CATALOG [FT_ARTIFACTS];

END
GO

IF EXISTS (SELECT * FROM sysfulltextcatalogs ftc WHERE ftc.name = N'FT_COMMENTS')
BEGIN
    DROP FULLTEXT INDEX ON TST_REQUIREMENT_DISCUSSION;
    DROP FULLTEXT INDEX ON TST_RELEASE_DISCUSSION;
    DROP FULLTEXT INDEX ON TST_TASK_DISCUSSION;
    DROP FULLTEXT INDEX ON TST_TEST_CASE_DISCUSSION;
    DROP FULLTEXT INDEX ON TST_TEST_SET_DISCUSSION;
    DROP FULLTEXT INDEX ON TST_INCIDENT_RESOLUTION;
    DROP FULLTEXT INDEX ON TST_RISK_DISCUSSION;
    DROP FULLTEXT CATALOG [FT_COMMENTS];
END
GO

-- First create the full-text catalogs
CREATE FULLTEXT CATALOG [FT_ARTIFACTS];
GO
CREATE FULLTEXT CATALOG [FT_COMMENTS];
GO

-- Next create the full-text indices
CREATE FULLTEXT INDEX ON TST_REQUIREMENT(NAME, DESCRIPTION)
	KEY INDEX XPKTST_REQUIREMENT
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_CASE(NAME, DESCRIPTION)
	KEY INDEX XPKTST_TEST_CASE
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_INCIDENT(NAME, DESCRIPTION)
	KEY INDEX XPKTST_INCIDENT
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_RELEASE(NAME, DESCRIPTION)
	KEY INDEX XPKTST_RELEASE
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_RUN(NAME, DESCRIPTION)
	KEY INDEX XPKTST_TEST_RUN
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TASK(NAME, DESCRIPTION)
	KEY INDEX XPKTST_TASK
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_STEP(DESCRIPTION, EXPECTED_RESULT, SAMPLE_DATA)
	KEY INDEX XPKTST_TEST_STEP
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_SET(NAME, DESCRIPTION)
	KEY INDEX PK_TST_TEST_SET
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_AUTOMATION_HOST(NAME, DESCRIPTION)
	KEY INDEX PK_TST_AUTOMATION_HOST
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_ATTACHMENT(FILENAME, DESCRIPTION)
	KEY INDEX XPKTST_ATTACHMENT
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_BUILD(NAME, DESCRIPTION)
	KEY INDEX PK_TST_BUILD
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_RISK(NAME, DESCRIPTION)
	KEY INDEX PK_TST_RISK
	ON [FT_ARTIFACTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_REQUIREMENT_DISCUSSION(TEXT)
	KEY INDEX PK_TST_REQUIREMENT_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_RELEASE_DISCUSSION(TEXT)
	KEY INDEX PK_TST_RELEASE_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TASK_DISCUSSION(TEXT)
	KEY INDEX PK_TST_TASK_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_CASE_DISCUSSION(TEXT)
	KEY INDEX PK_TST_TEST_CASE_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_TEST_SET_DISCUSSION(TEXT)
	KEY INDEX PK_TST_TEST_SET_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_INCIDENT_RESOLUTION(RESOLUTION)
	KEY INDEX PK_TST_INCIDENT_RESOLUTION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO
CREATE FULLTEXT INDEX ON TST_RISK_DISCUSSION(TEXT)
	KEY INDEX PK_TST_RISK_DISCUSSION
	ON [FT_COMMENTS]
	WITH STOPLIST = SYSTEM;
GO

--Turn execution back on
SET NOEXEC OFF
GO
