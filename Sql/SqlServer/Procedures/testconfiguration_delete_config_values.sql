-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestConfiguration
-- Description:		Populates the test configuration properties from the provided parameters
-- =====================================================================
IF OBJECT_ID ( 'TESTCONFIGURATION_DELETE_CONFIG_VALUES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCONFIGURATION_DELETE_CONFIG_VALUES];
GO
CREATE PROCEDURE [TESTCONFIGURATION_DELETE_CONFIG_VALUES]
	@TestConfigurationSetId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Delete the existing values
	DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION_SET WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
END
GO
