/***************************************************************
**	Insert script for table TST_CUSTOM_PROPERTY_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_TYPE ON; 

INSERT INTO TST_CUSTOM_PROPERTY_TYPE
(
CUSTOM_PROPERTY_TYPE_ID, NAME, SYSTEM_TYPE, IS_ACTIVE, ARTIFACT_FIELD_TYPE_ID
)
VALUES
(
1, 'Text', 'System.String', 1, 1
),
(
2, 'Integer', 'System.Int32', 1, 8
),
(
3, 'Decimal', 'System.Decimal', 1, 13
),
(
4, 'Boolean', 'System.Boolean', 1, 10
),
(
5, 'Date', 'System.DateTime', 1, 15
),
(
6, 'List', 'System.Int32', 1, 7
),
(
7, 'MultiList', 'System.Collections.Generic.List`1[System.Int32]', 1, 14
),
(
8, 'User', 'System.Int32', 1, 7
)
GO

SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_TYPE OFF; 

