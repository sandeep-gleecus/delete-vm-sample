-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Compares two indent levels using a maximum length, unless either indent level is actually shorter
-- Remarks:			Only used in the REQUIREMENT_FOCUS_ON stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_INDENT_LEVEL_COMPARE' AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FN_GLOBAL_INDENT_LEVEL_COMPARE;
END
GO
CREATE FUNCTION FN_GLOBAL_INDENT_LEVEL_COMPARE
(
	@IndentLevel1 NVARCHAR(100),
	@IndentLevel2 NVARCHAR(100),
	@Length INT
)
RETURNS BIT
AS
BEGIN
	DECLARE
		@RealLength INT,
		@Result BIT
	
	--See if either indent level is shorter
	SET @RealLength = @Length
	IF LEN(@IndentLevel1) < @RealLength
	BEGIN
		SET @RealLength = LEN(@IndentLevel1)
	END
	IF LEN(@IndentLevel2) < @RealLength
	BEGIN
		SET @RealLength = LEN(@IndentLevel2)
	END
	
	--Do the actual comparison
	SET @Result = 0
	IF SUBSTRING(@IndentLevel1, 1, @RealLength) = SUBSTRING(@IndentLevel2, 1, @RealLength)
	BEGIN
		SET @Result = 1
	END

	RETURN @Result
END
GO
