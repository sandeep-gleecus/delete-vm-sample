-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object:	ArtifactManager
-- Description:		Converts a list of project id:artifact ids (1:2,1:3, etc.) to a table
-- Remarks:			Used in the global search to limit permissions
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE' AND xtype = 'TF' )
BEGIN
	DROP FUNCTION FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE;
END
GO
CREATE FUNCTION FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE
(
	@List NVARCHAR(MAX)
)
RETURNS @ParsedList TABLE
(
	PROJECT_ID INT,
	ARTIFACT_TYPE_ID INT
)
AS
BEGIN
	DECLARE @Item NVARCHAR(MAX), @ProjectId NVARCHAR(MAX), @ArtifactTypeId NVARCHAR(MAX), @Pos INT, @Pos2 INT
	SET @List = LTRIM(RTRIM(@List))+ ','
	SET @Pos = CHARINDEX(',', @List, 1)
	WHILE @Pos > 0
	BEGIN
		SET @Item = LTRIM(RTRIM(LEFT(@List, @Pos - 1)))
		SET @Pos2 = CHARINDEX(':', @Item)
		IF @Pos2 > 0
		BEGIN
			SET @ProjectId = LTRIM(RTRIM(LEFT(@Item, @Pos2 - 1)))
			SET @ArtifactTypeId = LTRIM(RTRIM(RIGHT(@Item, LEN(@Item) - @Pos2)))
			IF @ProjectId <> '' AND @ArtifactTypeId <> ''
			BEGIN
				INSERT INTO @ParsedList (PROJECT_ID, ARTIFACT_TYPE_ID)
				VALUES (CAST(@ProjectId AS INT), CAST(@ArtifactTypeId AS INT))
			END
		END
		SET @List = RIGHT(@List, LEN(@List) - @Pos)
		SET @Pos = CHARINDEX(',', @List, 1)
	END
	RETURN
END
GO
