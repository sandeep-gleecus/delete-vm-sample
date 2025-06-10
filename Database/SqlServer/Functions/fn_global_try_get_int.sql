-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Safely checks to see if a string can be converted to INT and returns it if so
-- Remarks:			Unlike ISNUMERIC handles the case of decimals, money, etc.
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_TRY_GET_INT' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_GLOBAL_TRY_GET_INT;
END
GO
CREATE FUNCTION FN_GLOBAL_TRY_GET_INT
(
	@Value NVARCHAR(MAX)
)
RETURNS INT
AS
BEGIN
    SET @Value = REPLACE(@Value, ',', '')
    IF ISNUMERIC(@Value + 'e0') = 0 RETURN NULL
    IF ( CHARINDEX('.', @Value) > 0 AND CONVERT(INT, PARSENAME(@Value, 1)) <> 0 ) RETURN NULL
    DECLARE @I INT
    SET @I =
        CASE
        WHEN CHARINDEX('.', @Value) > 0 THEN CONVERT(INT, PARSENAME(@Value, 2))
        ELSE CONVERT(INT, @Value)
        END
    IF ABS(@I) > 2147483647 RETURN NULL
    RETURN @I
END
GO
