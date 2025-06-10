-- =====================================================================
-- Author:			Inflectra Corporation
-- =====================================================================
UPDATE TST_REPORT_AVAILABLE_SECTION
	SET REPORT_SECTION_ID = (SELECT REPORT_SECTION_ID FROM TST_REPORT_SECTION WHERE TOKEN = 'RiskDetails')
WHERE
	REPORT_SECTION_ID = (SELECT REPORT_SECTION_ID FROM TST_REPORT_SECTION WHERE TOKEN = 'RiskList') AND
	REPORT_ID = (SELECT REPORT_ID FROM TST_REPORT WHERE TOKEN = 'RiskDetailed')
GO
