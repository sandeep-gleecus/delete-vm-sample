-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a == on an NVARCHAR column
-- Remarks:			Used when filtering an integer custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_EQUALS_INT_LIST' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT_LIST;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT_LIST
(
	@Operand NVARCHAR(MAX), /* 1,2,3 or 0000000001,0000000002,0000000003 */
	@Constant INT
)
RETURNS BIT
AS
BEGIN
 	DECLARE
 		@Result AS BIT,
 		@Padded AS NVARCHAR(MAX)
	SET @Result = 0
	IF @Operand IS NOT NULL
	BEGIN
		--First test for unpadded
		IF CHARINDEX(',' + CAST(@Constant AS NVARCHAR) + ',', ',' + @Operand + ',') > 0
		BEGIN
			SET @Result = 1
		END
		--Next test for padded
		SET @Padded = REPLACE(STR(@Constant, 10), SPACE(1), '0')
		IF CHARINDEX(',' + @Padded + ',', ',' + @Operand + ',') > 0
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
