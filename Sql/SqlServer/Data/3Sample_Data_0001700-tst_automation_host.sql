/***************************************************************
**	Insert script for table TST_AUTOMATION_HOST
***************************************************************/
SET IDENTITY_INSERT TST_AUTOMATION_HOST ON; 

INSERT INTO TST_AUTOMATION_HOST
(
AUTOMATION_HOST_ID, PROJECT_ID, NAME, DESCRIPTION, TOKEN, IS_ACTIVE, IS_ATTACHMENTS, LAST_UPDATE_DATE, CONCURRENCY_DATE, LAST_CONTACT_DATE
)
VALUES
(
1, 1, 'Windows 8 Host', 'Windows 8 with IE10, Firefox 14, Chrome and Safari 5', 'Win8', 1, 0, DATEADD(day, -83, SYSUTCDATETIME()), DATEADD(day, -83, SYSUTCDATETIME()), DATEADD(day, -18, SYSUTCDATETIME())
),
(
2, 1, 'Windows Vista Host #1', 'Windows Vista x86 Bit Edition with IE8, Firefox 3, Chrome and Safari 4', 'WinVista1', 1, 0, DATEADD(day, -82, SYSUTCDATETIME()), DATEADD(day, -82, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME())
),
(
3, 1, 'Windows Vista Host #2', 'Windows Vista x64 Bit Edition with IE8, Firefox 3, Chrome and Safari 4', 'WinVista2', 1, 0, DATEADD(day, -82, SYSUTCDATETIME()), DATEADD(day, -82, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME())
),
(
4, 1, 'Windows 7 Host', 'Windows 7 with IE9 Beta 2, Firefox 3, Chrome and Safari 4', 'Win7', 1, 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -8, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_AUTOMATION_HOST OFF; 

