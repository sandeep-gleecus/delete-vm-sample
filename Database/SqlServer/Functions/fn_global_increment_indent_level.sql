-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Increments an indent level (e.g. AAAABAAAC + 1 => AAAABAAAD)
-- Remarks:			Primarily used in requirements and release stored procedures
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_INCREMENT_INDENT_LEVEL' AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FN_GLOBAL_INCREMENT_INDENT_LEVEL;
END
GO
CREATE FUNCTION FN_GLOBAL_INCREMENT_INDENT_LEVEL
(
	@InputIndentLevel NVARCHAR(100)
)
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE
		@OutputIndentLevel NVARCHAR(100),
		@position INT,
		@char NCHAR;
		
	--Loop through each character
	SET @OutputIndentLevel = @InputIndentLevel;
	SET @position = LEN(@OutputIndentLevel);
	WHILE (@position > 0)
	BEGIN
		SET @char = SUBSTRING(@OutputIndentLevel, @position, 1);
		--See if we have position overflow case
        IF(@char!='Z')
        BEGIN
			SET @char=NCHAR(ASCII(@char)+1);
			SET @OutputIndentLevel = STUFF(@OutputIndentLevel,@position,1,@char);
			SET @position = 0;	--Break
		END
		ELSE
		BEGIN
			SET @OutputIndentLevel = STUFF(@OutputIndentLevel,@position,1,'A');
			SET @position = @position - 1;	--Next Level
		END
	END

	RETURN @OutputIndentLevel;
END
GO
