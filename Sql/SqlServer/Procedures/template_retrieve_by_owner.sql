-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TemplateManager
-- Description:		Retrieves the list of templates that the
--					specified user is the owner/admin for
-- ================================================================
IF OBJECT_ID ( 'TEMPLATE_RETRIEVE_BY_OWNER', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_RETRIEVE_BY_OWNER;
GO
CREATE PROCEDURE TEMPLATE_RETRIEVE_BY_OWNER
	@UserId INT
AS
BEGIN
	SELECT DISTINCT TMP.*
	FROM TST_PROJECT_TEMPLATE TMP
		INNER JOIN TST_PROJECT PRJ ON TMP.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		INNER JOIN TST_PROJECT_USER PRU ON PRJ.PROJECT_ID = PRU.PROJECT_ID
		INNER JOIN TST_PROJECT_ROLE PRR ON PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
	WHERE
		PRR.IS_TEMPLATE_ADMIN = 1 AND
		PRU.USER_ID = @UserId
	ORDER BY TMP.NAME, TMP.PROJECT_TEMPLATE_ID
END
GO
