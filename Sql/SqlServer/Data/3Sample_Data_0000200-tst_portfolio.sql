/***************************************************************
**	Insert script for table TST_PORTFOLIO
***************************************************************/
SET IDENTITY_INSERT TST_PORTFOLIO ON; 

INSERT INTO TST_PORTFOLIO
(
PORTFOLIO_ID, NAME, DESCRIPTION, IS_ACTIVE, START_DATE, END_DATE, PERCENT_COMPLETE, REQUIREMENT_COUNT
)
VALUES
(
1, 'Core Services', 'Contains core programs and products that cover specific types of application and systems that are not industry specific.', 1, DATEADD(day, -220, SYSUTCDATETIME()), DATEADD(day, 275, SYSUTCDATETIME()), 23, 235
)
GO

SET IDENTITY_INSERT TST_PORTFOLIO OFF; 

