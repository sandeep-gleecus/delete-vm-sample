/***************************************************************
**	Insert script for table TST_ARTIFACT_FIELD_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_FIELD_TYPE ON; 

INSERT INTO TST_ARTIFACT_FIELD_TYPE
(
ARTIFACT_FIELD_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Text', 1
),
(
2, 'Lookup', 1
),
(
3, 'DateTime', 1
),
(
4, 'Identifier', 1
),
(
5, 'Equalizer', 1
),
(
6, 'NameDescription', 1
),
(
7, 'CustomPropertyLookup', 1
),
(
8, 'Integer', 1
),
(
9, 'TimeInterval', 1
),
(
10, 'Flag', 1
),
(
11, 'HierarchyLookup', 1
),
(
12, 'Html', 1
),
(
13, 'Decimal', 1
),
(
14, 'CustomPropertyMultiList', 1
),
(
15, 'CustomPropertyDate', 1
),
(
16, 'MultiList', 1
)
GO

SET IDENTITY_INSERT TST_ARTIFACT_FIELD_TYPE OFF; 

