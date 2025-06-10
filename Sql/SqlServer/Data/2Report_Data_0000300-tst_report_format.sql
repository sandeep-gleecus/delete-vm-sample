/***************************************************************
**	Insert script for table TST_REPORT_FORMAT
***************************************************************/
SET IDENTITY_INSERT TST_REPORT_FORMAT ON; 

INSERT INTO TST_REPORT_FORMAT
(
REPORT_FORMAT_ID, TOKEN, NAME, DESCRIPTION, ICON_FILENAME, CONTENT_TYPE, CONTENT_DISPOSITION, IS_ACTIVE
)
VALUES
(
1, 'Html', 'HTML', 'Format that will display in a web browser natively', 'HTML.svg', 'text/html', NULL, 1
),
(
2, 'MsWord2003', 'MS-Word (legacy)', 'Format that will open in MS-Word versions 2013 or earlier', 'Word-Xml.svg', 'application/msword', 'attachment; filename=Report.doc', 0
),
(
3, 'MsExcel2003', 'MS-Excel (data)', 'Format optimized for data manipulation', 'Excel-Xml.svg', 'application/msexcel', 'attachment; filename=Report.xls', 1
),
(
4, 'MsProj2003', 'MS-Project', 'Format that will load into MS-Project versions 2003 or later', 'MS-Project.svg', 'application/msproject', 'attachment; filename=Report.xml', 0
),
(
5, 'Xml', 'XML', 'Raw XML output useful when developing new reports or exporting to other systems', 'XML.svg', 'text/xml', NULL, 1
),
(
6, 'MsWord2007', 'MS-Word', 'Format that will open in MS-Word versions 2015 or later', 'Word.svg', 'application/msword', 'attachment; filename=Report.doc', 1
),
(
7, 'MsExcel2007', 'MS-Excel (printable)', 'Format that is optimized for printing and includes rich text formatting', 'Excel.svg', 'application/msexcel', 'attachment; filename=Report.xls', 1
),
(
8, 'Pdf', 'PDF', 'Format that will open with Adobe Acrobat Reader (or equivalent)', 'Acrobat.svg', 'application/pdf', 'xsl-fo', 1
)
GO

SET IDENTITY_INSERT TST_REPORT_FORMAT OFF; 

