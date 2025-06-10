-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a <= on an NVARCHAR column
-- Remarks:			Used when filtering an integer custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_LESS_THAN_INT' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_INT;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_INT
(
	@Operand NVARCHAR(MAX),
	@Constant INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @Result AS BIT
	SET @Result = 0
	IF dbo.FN_GLOBAL_TRY_GET_INT(@Operand) IS NOT NULL
	BEGIN
		IF (CAST(@Operand AS INT) <= @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
