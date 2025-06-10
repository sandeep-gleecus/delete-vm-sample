-- ================================================================ 
-- Author:			Inflectra Corporation 
-- Business Object: TestCase 
-- Description:		Adds any test cases attached to the requirement to the release 
-- Updated 11/3/2020 - [TK:2479] For Baseline History recording. 
-- Updated 2/20/2021 - Don't add the test case if not in the same project
-- ================================================================ 
IF OBJECT_ID ( 'TESTCASE_ADD_TO_REQUIREMENT_RELEASE', 'P' ) IS NOT NULL  
    DROP PROCEDURE TESTCASE_ADD_TO_REQUIREMENT_RELEASE
GO
CREATE PROCEDURE TESTCASE_ADD_TO_REQUIREMENT_RELEASE 
	@ProjectId INT, 
	@RequirementId INT, 
	@ReleaseId INT, 
	@RecordHistory BIT, 
	@UserId INT 
AS 
BEGIN 
	DECLARE 
		@IsIterationOrPhase BIT, /* Check if we may have a parent. */ 
		@IndentLevel NVARCHAR(MAX), /* The Indent level of the Release we're adding to. */ 
		@ParentReleaseId INT, /* The parent release ID (if any). */ 
		@ChangeSetDate DATETIME2, /* For re-selecting the Changeset ID. */ 
		@ChangeSetId BIGINT, /* The Changeset ID to insert the child records. */ 
		@ReleaseName NVARCHAR(MAX) /* The name of the Release, used for History. */ 
	 
	--See if we have an iteration 
	SELECT @IsIterationOrPhase = (CASE RELEASE_TYPE_ID 
									WHEN 3 THEN 1 
									WHEN 4 THEN 1 
									ELSE 0 END), 
		@IndentLevel = INDENT_LEVEL 
	FROM TST_RELEASE 
	WHERE RELEASE_ID = @ReleaseId; 
 
 	-- Add the Changeset for this item. 
	SELECT @ChangeSetId = 0;
	IF (@RecordHistory = 1)
	BEGIN 
		IF EXISTS (
			SELECT *
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND TST.PROJECT_ID = @ProjectId
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ReleaseId)
			)
		BEGIN
			-- Get the parent's Release name. 
			SELECT @ReleaseName = NAME 
				FROM TST_RELEASE 
				WHERE RELEASE_ID = @ReleaseId;
			-- Get the date. -- 
			SELECT @ChangeSetDate = GETUTCDATE(); 
			-- Insert our master Changeset. 
			INSERT INTO TST_HISTORY_CHANGESET (USER_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, CHANGE_DATE, CHANGETYPE_ID, PROJECT_ID, ARTIFACT_DESC) 
			VALUES (@UserId, 4 /* Release */, @ReleaseId, @ChangeSetDate, 13 /* Association Add */, @ProjectId, @ReleaseName);
			-- Get the Changeset ID we just wrote
			SET @ChangeSetId = @@IDENTITY;
			-- Now Write out the Association changes. 
			INSERT INTO TST_HISTORY_ASSOCIATION ( 
				CHANGESET_ID,  
				SOURCE_ARTIFACT_TYPE_ID,  
				SOURCE_ARTIFACT_ID,  
				DEST_ARTIFACT_TYPE_ID, 
				DEST_ARTIFACT_ID) 
				SELECT 
					@ChangeSetId, 
					4, /* Artifact Type: Release */ 
					@ReleaseId, 
					2, /* Artifact Type: Test Case */ 
					RTC.TEST_CASE_ID 
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ReleaseId);
		END
	END 

	--Insert into the TestCase<->Release Table. 
	INSERT INTO TST_RELEASE_TEST_CASE 
		(RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID) 
	 
		--Get the list of test cases that belong to this requirement (that are not deleted) 
		--they need to be in the same project
		SELECT @ReleaseId, TST.TEST_CASE_ID, 3 /*Not Run*/ AS EXECUTION_STATUS_ID 
		FROM TST_REQUIREMENT_TEST_CASE RTC 
		INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
		WHERE RTC.REQUIREMENT_ID = @RequirementId 
			AND TST.IS_DELETED = 0 
			AND TST.PROJECT_ID = @ProjectId
			AND RTC.TEST_CASE_ID NOT IN ( 
				SELECT TEST_CASE_ID 
				FROM TST_RELEASE_TEST_CASE 
				WHERE RELEASE_ID = @ReleaseId);
	 
	--Do the same for parent release if an iteration/phase 
	IF @IsIterationOrPhase = 1 
	BEGIN 
		SELECT @ParentReleaseId = RELEASE_ID 
		FROM TST_RELEASE 
		WHERE PROJECT_ID = @ProjectId 
		AND LEN(INDENT_LEVEL) = LEN(@IndentLevel) - 3 
		AND SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL;
		 
		IF @ParentReleaseId IS NOT NULL 
		BEGIN 
			-- Need to insert into the HISTORY table. 
			IF (@RecordHistory = 1)
			BEGIN 
				IF EXISTS (
					SELECT *
					FROM TST_REQUIREMENT_TEST_CASE RTC 
					INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
					WHERE RTC.REQUIREMENT_ID = @RequirementId 
						AND TST.IS_DELETED = 0 
						AND TST.PROJECT_ID = @ProjectId
						AND RTC.TEST_CASE_ID NOT IN ( 
							SELECT TEST_CASE_ID 
							FROM TST_RELEASE_TEST_CASE 
							WHERE RELEASE_ID = @ParentReleaseId)
					)
				BEGIN
					-- Get the parent's Release name. 
					SELECT @ReleaseName = NAME 
						FROM TST_RELEASE 
						WHERE RELEASE_ID = @ParentReleaseId;
					-- Get the date. -- 
					SET @ChangeSetDate = GETUTCDATE();
					-- Insert our master Changeset. 
					INSERT INTO TST_HISTORY_CHANGESET (USER_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, CHANGE_DATE, CHANGETYPE_ID, PROJECT_ID, ARTIFACT_DESC) 
					VALUES (@UserId, 4 /* Release */, @ParentReleaseId, @ChangeSetDate, 13 /* Association Add */, @ProjectId, @ReleaseName);
					-- Get the Changeset ID we just wrote.
					SET @ChangeSetId = @@IDENTITY;
					-- Now Write out the Association changes. 
					INSERT INTO TST_HISTORY_ASSOCIATION ( 
						CHANGESET_ID,  
						SOURCE_ARTIFACT_TYPE_ID,  
						SOURCE_ARTIFACT_ID,  
						DEST_ARTIFACT_TYPE_ID, 
						DEST_ARTIFACT_ID) 
						SELECT 
							@ChangeSetId, 
							4, /* Artifact Type: Release */ 
							@ParentReleaseId, 
							2, /* Artifact Type: Test Case */ 
							RTC.TEST_CASE_ID 
						FROM TST_REQUIREMENT_TEST_CASE RTC 
						INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
						WHERE RTC.REQUIREMENT_ID = @RequirementId 
							AND TST.IS_DELETED = 0 
							AND RTC.TEST_CASE_ID NOT IN ( 
								SELECT TEST_CASE_ID 
								FROM TST_RELEASE_TEST_CASE 
								WHERE RELEASE_ID = @ParentReleaseId);
				END
			END 

			--Insert into the TestCase<->Release table. 
			INSERT INTO TST_RELEASE_TEST_CASE 
				(RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID)
				--Get the list of test cases that belong to this requirement (that are not deleted) 
				SELECT @ParentReleaseId, TST.TEST_CASE_ID, 3 /*Not Run*/ AS EXECUTION_STATUS_ID 
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND TST.PROJECT_ID = @ProjectId
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ParentReleaseId);	 
		END 
	END 
END 
GO 
