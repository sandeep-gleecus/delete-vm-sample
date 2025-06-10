/***************************************************************
**	Insert script for table TST_RECURRENCE
***************************************************************/
SET IDENTITY_INSERT TST_RECURRENCE ON; 

INSERT INTO TST_RECURRENCE
(
RECURRENCE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Hourly', 1
),
(
2, 'Daily', 1
),
(
3, 'Weekly', 1
),
(
4, 'Monthly', 1
),
(
5, 'Quarterly', 1
),
(
6, 'Yearly', 1
)
GO

SET IDENTITY_INSERT TST_RECURRENCE OFF; 

