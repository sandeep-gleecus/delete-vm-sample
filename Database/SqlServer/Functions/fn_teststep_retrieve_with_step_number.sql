-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of test steps for a test case, but includes the display position (step number)
-- Remarks:			Only used in the ArtifactLink business object so only returning the columns it needs
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_TESTSTEP_RETRIEVE_STEP_NUMBER' AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FN_TESTSTEP_RETRIEVE_STEP_NUMBER;
END
GO
CREATE FUNCTION FN_TESTSTEP_RETRIEVE_STEP_NUMBER
(
	@TestCaseId INT,
	@TestStepId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @StepNumber INT

	--Now get the position number (accounting for deletes) of this test step in the test case
    SELECT @StepNumber = STEP_NUMBER FROM
	(
   		SELECT TEST_STEP_ID,ROW_NUMBER() OVER(ORDER BY POSITION ASC) AS 'STEP_NUMBER'
		FROM TST_TEST_STEP
		WHERE TEST_CASE_ID = @TestCaseId
		AND	IS_DELETED = 0
	) STP WHERE TEST_STEP_ID = @TestStepId

	RETURN @StepNumber
END
GO
