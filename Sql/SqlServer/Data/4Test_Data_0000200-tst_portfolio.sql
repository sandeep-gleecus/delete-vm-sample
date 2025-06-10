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
2, 'Industry: Life Sciences', 'TBD', 1, NULL, NULL, 0, 0
),
(
3, 'Industry: Financial Services', 'TBD', 1, NULL, NULL, 0, 0
),
(
4, 'Industry: Aerospace', 'TBD', 1, NULL, NULL, 0, 0
),
(
5, 'Industry: Manufacturing', 'TBD', 1, NULL, NULL, 0, 0
)
GO

SET IDENTITY_INSERT TST_PORTFOLIO OFF; 

