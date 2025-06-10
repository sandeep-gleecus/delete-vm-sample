-- ============================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: N/A
-- Description:		Populates over the initial set of releases, projects, programs and portfolio completion data
-- =============================================================================================================
IF OBJECT_ID ( 'MIGRATION_POPULATE_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE MIGRATION_POPULATE_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE MIGRATION_POPULATE_REQUIREMENT_COMPLETION
AS
DECLARE
	@ProjectId INT,
	@ReleaseId INT
BEGIN
	SET NOCOUNT ON;
	
	--First loop through all the projects
	DECLARE ProjectCursor CURSOR LOCAL FOR
		SELECT PROJECT_ID
		FROM TST_PROJECT
		ORDER BY PROJECT_ID
		
	--Loop
	OPEN ProjectCursor   
	FETCH NEXT FROM ProjectCursor INTO @ProjectId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		--Now loop through all the releases in the project
		DECLARE ReleaseCursor CURSOR LOCAL FOR
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			ORDER BY RELEASE_ID
			
		--Loop
		OPEN ReleaseCursor   
		FETCH NEXT FROM ReleaseCursor INTO @ReleaseId
		WHILE @@FETCH_STATUS = 0   
		BEGIN
			EXEC RELEASE_REFRESH_REQUIREMENT_COMPLETION @ProjectId, @ReleaseId
			FETCH NEXT FROM ReleaseCursor INTO @ReleaseId
		END   
		
		CLOSE ReleaseCursor
		DEALLOCATE ReleaseCursor	
	
		EXEC PROJECT_REFRESH_REQUIREMENT_COMPLETION @ProjectId
		FETCH NEXT FROM ProjectCursor INTO @ProjectId
	END   

	--Clean up
	CLOSE ProjectCursor   
	DEALLOCATE ProjectCursor
END
GO
