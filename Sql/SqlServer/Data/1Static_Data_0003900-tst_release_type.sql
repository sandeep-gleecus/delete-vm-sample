/***************************************************************
**	Insert script for table TST_RELEASE_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_RELEASE_TYPE ON; 

INSERT INTO TST_RELEASE_TYPE
(
RELEASE_TYPE_ID, NAME, POSITION, IS_ACTIVE
)
VALUES
(
1, 'Major Release', 1, 1
),
(
2, 'Minor Release', 2, 1
),
(
3, 'Sprint', 3, 1
),
(
4, 'Phase', 4, 1
)
GO

SET IDENTITY_INSERT TST_RELEASE_TYPE OFF; 

