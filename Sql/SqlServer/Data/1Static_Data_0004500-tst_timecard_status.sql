/***************************************************************
**	Insert script for table TST_TIMECARD_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_TIMECARD_STATUS ON; 

INSERT INTO TST_TIMECARD_STATUS
(
TIMECARD_STATUS_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Draft', 1
),
(
2, 'Submitted', 1
),
(
3, 'Approved', 1
),
(
4, 'Rejected', 1
)
GO

SET IDENTITY_INSERT TST_TIMECARD_STATUS OFF; 

