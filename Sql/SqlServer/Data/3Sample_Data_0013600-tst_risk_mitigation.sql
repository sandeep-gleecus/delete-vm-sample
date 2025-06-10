/***************************************************************
**	Insert script for table TST_RISK_MITIGATION
***************************************************************/
SET IDENTITY_INSERT TST_RISK_MITIGATION ON; 

INSERT INTO TST_RISK_MITIGATION
(
RISK_MITIGATION_ID, RISK_ID, POSITION, DESCRIPTION, IS_DELETED, CREATION_DATE, LAST_UPDATE_DATE, CONCURRENCY_DATE, IS_ACTIVE, REVIEW_DATE
)
VALUES
(
1, 1, 1, '<p>Discuss reducing the scope of release v1.1 with the customer to reduce the risk of a schedule overrun.</p>', 0, DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), 0, DATEADD(day,  1, SYSUTCDATETIME())
),
(
2, 1, 2, '<p>Add additional developers and testers onto the team to meet the original date.</p>', 0, DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), 0, DATEADD(day,  3, SYSUTCDATETIME())
),
(
3, 1, 3, '<p>See how firm the release date truly is with the customer before we get too close to make changes.</p>', 0, DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), 0, DATEADD(day,  7, SYSUTCDATETIME())
),
(
4, 2, 1, '<p>Increase marketing efforts to potential libraries and authors</p>', 0, DATEADD(day, -5, SYSUTCDATETIME()), DATEADD(day, -5, SYSUTCDATETIME()), DATEADD(day, -5, SYSUTCDATETIME()), 0, DATEADD(day,  3, SYSUTCDATETIME())
),
(
5, 2, 2, '<p>Look into acquiring an existing author database to avoid having to manually seed it.</p>', 0, DATEADD(day, -5, SYSUTCDATETIME()), DATEADD(day, -5, SYSUTCDATETIME()), DATEADD(day, -5, SYSUTCDATETIME()), 0, NULL
)
GO

SET IDENTITY_INSERT TST_RISK_MITIGATION OFF; 

