/***************************************************************
**	Insert script for table TST_ARTIFACT_SOURCE_CODE_FILE
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_SOURCE_CODE_FILE ON; 

INSERT INTO TST_ARTIFACT_SOURCE_CODE_FILE
(
ARTIFACT_SOURCE_CODE_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, FILE_KEY, COMMENT, CREATION_DATE
)
VALUES
(
1, 1, 4, 'test://Server/Root/Files/Filename1.ext', NULL, DATEADD(day, -147, SYSUTCDATETIME())
),
(
2, 1, 4, 'test://Server/Root/Files/Filename8.ext', 'This document provides supporting information', DATEADD(day, -145, SYSUTCDATETIME())
),
(
3, 1, 5, 'test://Server/Root/Files/Filename4.ext', 'This document adds some additional information', DATEADD(day, -145, SYSUTCDATETIME())
),
(
4, 3, 7, 'test://Server/Root/Files/Filename1.ext', NULL, DATEADD(day, -140, SYSUTCDATETIME())
),
(
5, 3, 7, 'test://Server/Root/Files/Filename8.ext', 'This document adds some additional information', DATEADD(day, -139, SYSUTCDATETIME())
),
(
6, 3, 8, 'test://Server/Root/Files/Filename4.ext', 'Take a look at this document and see if it clarifies things', DATEADD(day, -141, SYSUTCDATETIME())
),
(
7, 6, 2, 'test://Server/Root/Files/Filename1.ext', 'Take a look at this document and see if it clarifies things', DATEADD(day, -148, SYSUTCDATETIME())
),
(
8, 6, 1, 'test://Server/Root/Files/Filename8.ext', NULL, DATEADD(day, -147, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_ARTIFACT_SOURCE_CODE_FILE OFF; 

