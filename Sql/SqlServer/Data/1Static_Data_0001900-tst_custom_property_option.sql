/***************************************************************
**	Insert script for table TST_CUSTOM_PROPERTY_OPTION
***************************************************************/
SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_OPTION ON; 

INSERT INTO TST_CUSTOM_PROPERTY_OPTION
(
CUSTOM_PROPERTY_OPTION_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'AllowEmpty', 1
),
(
2, 'MaxLength', 1
),
(
3, 'MinLength', 1
),
(
4, 'RichText', 1
),
(
5, 'Default', 1
),
(
6, 'MaxValue', 1
),
(
7, 'MinValue', 1
),
(
8, 'Precision', 1
)
GO

SET IDENTITY_INSERT TST_CUSTOM_PROPERTY_OPTION OFF; 

