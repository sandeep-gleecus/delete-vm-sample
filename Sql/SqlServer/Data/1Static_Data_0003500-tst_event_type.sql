/***************************************************************
**	Insert script for table TST_EVENT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_EVENT_TYPE ON; 

INSERT INTO TST_EVENT_TYPE
(
EVENT_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Error', 1
),
(
2, 'Warning', 1
),
(
4, 'Information', 1
),
(
8, 'Success Audit', 1
),
(
16, 'Failure Audit', 1
)
GO

SET IDENTITY_INSERT TST_EVENT_TYPE OFF; 

