-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Deletes a Test Set and associated data
-- =======================================================
IF OBJECT_ID ( 'TESTSET_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTSET_DELETE;
GO
CREATE PROCEDURE TESTSET_DELETE
	@TestSetId INT
AS
BEGIN
	SET NOCOUNT ON;

	--Delete from user subscriptions.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 8 AND ARTIFACT_ID = @TestSetId);

	--Delete the test cases in the set, not-cascadable
	DELETE FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_ID = @TestSetId;

	--Delete the test set. Other needed deletes are set to cascade.
	DELETE FROM TST_TEST_SET WHERE TEST_SET_ID = @TestSetId;
END
GO
