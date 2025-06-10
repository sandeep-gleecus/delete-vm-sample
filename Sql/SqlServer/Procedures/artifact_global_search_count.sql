-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Counts the VW_ARTIFACT_LIST view for items, also filtering by user project/artifact-type
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_GLOBAL_SEARCH_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_GLOBAL_SEARCH_COUNT;
GO
CREATE PROCEDURE ARTIFACT_GLOBAL_SEARCH_COUNT
	@Keywords NVARCHAR(MAX),
	@ProjectArtifactList NVARCHAR(MAX)
AS
BEGIN
	--Declare keyword table
	DECLARE @KeywordTable TABLE
	(
		ITEM NVARCHAR(MAX)
	)
	--Populate
	INSERT @KeywordTable (ITEM)
	SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@Keywords, ',')
	
	--Get the list of artifacts that matches the search
	--and is an allowed project/artifact type id combination
	IF @ProjectArtifactList = '' OR @ProjectArtifactList IS NULL
	BEGIN
		SELECT COUNT (*) AS SEARCH_COUNT FROM VW_ARTIFACT_LIST VAL
		WHERE IS_DELETED = 0
		AND NOT EXISTS
		(
			SELECT NULL
			FROM    @KeywordTable KWD
			WHERE	VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
			AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
		)	
	END
	ELSE
	BEGIN
		--Declare
		DECLARE @ProjectArtifactTable TABLE
		(
			PROJECT_ID INT,
			ARTIFACT_TYPE_ID INT
		)
		DECLARE @ProjectTable TABLE
		(
			PROJECT_ID INT
		)
		--Populate
		INSERT @ProjectArtifactTable (PROJECT_ID, ARTIFACT_TYPE_ID)
		SELECT PROJECT_ID, ARTIFACT_TYPE_ID FROM FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE(@ProjectArtifactList)
		INSERT @ProjectTable (PROJECT_ID)
		SELECT DISTINCT PROJECT_ID FROM @ProjectArtifactTable
		
		SELECT COUNT (*) AS SEARCH_COUNT FROM VW_ARTIFACT_LIST VAL
		INNER JOIN @ProjectTable PAT ON VAL.PROJECT_ID = PAT.PROJECT_ID
		WHERE IS_DELETED = 0
		AND EXISTS
		(
			SELECT 1
			FROM    @ProjectArtifactTable PAL
			WHERE   VAL.PROJECT_ID = PAL.PROJECT_ID
			AND VAL.ARTIFACT_TYPE_ID = PAL.ARTIFACT_TYPE_ID
		)
		AND NOT EXISTS
		(
			SELECT NULL
			FROM    @KeywordTable KWD
			WHERE	VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
			AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
		)	
	END
END
GO
