IF OBJECT_ID ( 'RPT_PROJECTGROUPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECTGROUPS];
GO
CREATE VIEW [RPT_PROJECTGROUPS]
AS
	SELECT	PRG.*
	FROM TST_PROJECT_GROUP AS PRG
	WHERE PRG.IS_ACTIVE = 1
GO
