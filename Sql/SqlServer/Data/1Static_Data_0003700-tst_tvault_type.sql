/***************************************************************
**	Insert script for table TST_TVAULT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_TVAULT_TYPE ON; 

INSERT INTO TST_TVAULT_TYPE
(
TVAULT_TYPE_ID, NAME, TOKEN, IS_ACTIVE
)
VALUES
(
1, 'Subversion', 'svn', 1
),
(
2, 'Git', 'git', 1
)
GO

SET IDENTITY_INSERT TST_TVAULT_TYPE OFF; 

