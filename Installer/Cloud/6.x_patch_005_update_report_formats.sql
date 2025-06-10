-- =====================================================================
-- Author:			Inflectra Corporation
-- =====================================================================
UPDATE TST_REPORT_FORMAT
SET NAME = 'MS-Word (legacy)', DESCRIPTION = 'Format that will open in MS-Word versions 2013 or earlier'
WHERE TOKEN = 'MsWord2003';
GO
UPDATE TST_REPORT_FORMAT
SET NAME = 'MS-Excel (data)', DESCRIPTION = 'Format optimized for data manipulation'
WHERE TOKEN = 'MsExcel2003';
GO
UPDATE TST_REPORT_FORMAT
SET NAME = 'MS-Word', DESCRIPTION = 'Format that will open in MS-Word versions 2015 or later'
WHERE TOKEN = 'MsWord2007';
GO
UPDATE TST_REPORT_FORMAT
SET NAME = 'MS-Excel (printable)', DESCRIPTION = 'Format that is optimized for printing and includes rich text formatting'
WHERE TOKEN = 'MsExcel2007';
GO

