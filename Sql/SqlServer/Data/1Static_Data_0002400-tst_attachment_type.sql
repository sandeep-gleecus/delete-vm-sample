/***************************************************************
**	Insert script for table TST_ATTACHMENT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_ATTACHMENT_TYPE ON; 

INSERT INTO TST_ATTACHMENT_TYPE
(
ATTACHMENT_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'File', 1
),
(
2, 'URL', 1
),
(
3, 'HTML', 1
),
(
4, 'Markdown', 1
)
GO

SET IDENTITY_INSERT TST_ATTACHMENT_TYPE OFF; 

