-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes the user navigation data for all releases for a given user
-- =============================================
IF OBJECT_ID ( 'RELEASE_DELETE_NAVIGATION_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_DELETE_NAVIGATION_DATA;
GO
CREATE PROCEDURE RELEASE_DELETE_NAVIGATION_DATA
	@UserId INT
AS
BEGIN
	--Now delete the navigation data
    DELETE FROM TST_RELEASE_USER WHERE USER_ID = @UserId
END
GO
