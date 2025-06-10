/***************************************************************
**	Insert script for table TST_AUTOMATION_ENGINE
***************************************************************/
SET IDENTITY_INSERT TST_AUTOMATION_ENGINE ON; 

INSERT INTO TST_AUTOMATION_ENGINE
(
AUTOMATION_ENGINE_ID, NAME, DESCRIPTION, TOKEN, IS_ACTIVE
)
VALUES
(
1, 'Rapise', 'Engine that integrates with Inflectra Rapise', 'Rapise', 1
),
(
2, 'Quick Test Pro', 'Engine that integrates with HP Quick Test Pro 9.0+', 'QuickTestPro9', 1
),
(
3, 'SmarteScript', 'Engine that integrates with SmarteSoft SmarteScript 5.5+', 'SmarteScript5', 0
),
(
4, 'TestComplete', 'Engine that integrates with SmartBear TestComplete 11.0+', 'TestComplete11', 0
),
(
5, 'Selenium WebDriver', 'Engine that integrates with Selenium 2.0 WebDriver', 'Selenium2', 1
),
(
6, 'Command-Line', 'Engine that will execute a generic command-line executable', 'CommandLine', 0
),
(
7, 'LoadRunner', ' Engine that integrates with HP LoadRunner 11.0+', 'LoadRunner11', 0
),
(
8, 'SOAP-UI', 'Engine that integrates with SmartBear SOAP-UI', 'SoapUI', 0
),
(
9, 'FitNesse', 'Engine that integrates with FitNesse framework', 'FitNesse', 0
),
(
10, 'NeoLoad', 'Engine that integrates with NeoTys NeoLoad', 'NeoLoad', 1
),
(
11, 'TestPartner', 'Engine that integrates with MicroFocus TestPartner', 'TestPartner', 0
),
(
12, 'Bad Boy', 'Engine that integrates with Bad Boy', 'BadBoy2', 0
),
(
13, 'JMeter', 'Engine that integrates with Apache JMeter', 'JMeter2', 0
),
(
14, 'Ranorex', 'Engine that integrates with Ranorex', 'RanorexEngine', 0
),
(
15, 'IBM RFT', 'Engine that integrates with IBM Rational Functional Tester', 'RTFAutomationEngine', 0
)
GO

SET IDENTITY_INSERT TST_AUTOMATION_ENGINE OFF; 

