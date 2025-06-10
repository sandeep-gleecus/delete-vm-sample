-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a = on an NVARCHAR column
-- Remarks:			Used when filtering an decimal custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_EQUALS_DECIMAL' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_EQUALS_DECIMAL;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_EQUALS_DECIMAL
(
	@Operand NVARCHAR(MAX),
	@Constant DECIMAL(20,4)
)
RETURNS BIT
AS
BEGIN
	DECLARE @Result AS BIT
	SET @Result = 0
	IF ISNUMERIC(@Operand) = 1
	BEGIN
		IF (CAST(@Operand AS DECIMAL(20,4)) = @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
