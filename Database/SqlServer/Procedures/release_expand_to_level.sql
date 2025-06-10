-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Expands the hierarchy to a specific level
-- =============================================
IF OBJECT_ID ( 'RELEASE_EXPAND_TO_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_EXPAND_TO_LEVEL;
GO
CREATE PROCEDURE RELEASE_EXPAND_TO_LEVEL
	@UserId INT,
	@ProjectId INT,
	@Level INT
AS
BEGIN
	--Make all items that are the requested level or less visible, the others hidden
	
	--Show
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) <= (@Level * 3) 
			AND IS_DELETED = 0
			)
			
	--Hide
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 0
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3) 
			AND IS_DELETED = 0
			)
	
	--Those folder items that are less than the requested level only, expand
	
	--Expand
	UPDATE TST_RELEASE_USER
		SET IS_EXPANDED = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3) 
			AND IS_DELETED = 0
			AND IS_SUMMARY = 1
			)
			
	--Collapse
	UPDATE TST_RELEASE_USER
		SET IS_EXPANDED = 0
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND (LEN(INDENT_LEVEL) >= (@Level * 3) OR IS_SUMMARY = 0)
			AND IS_DELETED = 0
			)

	--Now do the inserts for the case where user has no existing settings
	--Visible and Expanded
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 1, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_SUMMARY = 1
			AND IS_DELETED = 0
			
	--Visible but not Expanded (2-cases)
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_SUMMARY = 0
			AND IS_DELETED = 0
			
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) = (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
			
	--Hidden and Collapsed
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 0, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
