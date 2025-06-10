/***************************************************************
**	Insert script for table TST_PERIODIC_REVIEW_ALERT
***************************************************************/
SET IDENTITY_INSERT TST_PERIODIC_REVIEW_ALERT ON; 

INSERT INTO TST_PERIODIC_REVIEW_ALERT
(
ALERTID, NAME, POSITION, ALERTINDAYS, ISACTIVE
)
VALUES
(
1, 'In 30 Days', 1, 30, 1
),
(
2, 'In 60 Days', 3, 60, 1
),
(
3, 'In 90 Days', 3, 90, 1
)
GO

SET IDENTITY_INSERT TST_PERIODIC_REVIEW_ALERT OFF; 

