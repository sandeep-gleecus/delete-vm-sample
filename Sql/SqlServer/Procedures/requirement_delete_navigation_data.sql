-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes the user navigation data for all requirements for a given user
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_DELETE_NAVIGATION_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_DELETE_NAVIGATION_DATA;
GO
CREATE PROCEDURE REQUIREMENT_DELETE_NAVIGATION_DATA
	@UserId INT
AS
BEGIN
	--Now delete the navigation data
    DELETE FROM TST_REQUIREMENT_USER WHERE USER_ID = @UserId
END
GO
