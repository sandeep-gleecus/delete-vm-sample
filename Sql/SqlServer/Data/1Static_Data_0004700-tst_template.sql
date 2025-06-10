/***************************************************************
**	Insert script for table TST_TEMPLATE
***************************************************************/
SET IDENTITY_INSERT TST_TEMPLATE ON; 

INSERT INTO TST_TEMPLATE
(
TEMPLATEID, TEMPLATENAME, ISCUSTOM, TEMPLATELOCATION, PODLOCATION, ACTIVE, CREATIONDATE, REPORTCATEGORY, CATEGORYGROUP
)
VALUES
(
3098, 'Draft-IQ-Document', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\Draft-IQ-Document.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\DraftTestDocument.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Cleaning Validation Reports', 1
),
(
3099, 'Draft-OQ-Document', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\Draft-OQ-Document.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\DraftTestDocument.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Software Assurance Reports', 2
),
(
3100, 'IQ-TestRunDocument', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\IQ-TestRunDocument.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\TestRunDocument.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Cleaning Validation Reports', 1
),
(
3101, 'OQ-TestRunDocument', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\OQ-TestRunDocument.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\TestRunDocument.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Software Assurance Reports', 2
),
(
3102, 'User Requirements Document', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\UserRequirementsDocument.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\UserRequirementsDocument.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Cleaning Validation Reports', 1
),
(
3103, 'Requirements Traceability Matrix', 1, 'C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\RequirementsTraceabilityMatrix.docx', 'C:\Program Files (x86)\ValidationMaster\reporting\podfiles\RequirementsTraceabilityMatrix.rdlx', 1, DATEADD(day, -420, SYSUTCDATETIME()), 'Cleaning Validation Reports', 1
)
GO

SET IDENTITY_INSERT TST_TEMPLATE OFF; 

