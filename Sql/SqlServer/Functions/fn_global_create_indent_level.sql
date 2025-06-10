-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Creates an indent level component ('AAA') from a number
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CREATE_INDENT_LEVEL' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CREATE_INDENT_LEVEL;
END
GO
CREATE FUNCTION FN_CREATE_INDENT_LEVEL
(
	@RowNumber INT
)
RETURNS NVARCHAR(3) 
AS
BEGIN
	DECLARE @IndentLevel AS NVARCHAR(3)

	SET @IndentLevel =	NCHAR((@RowNumber-1) / power(26,2) % 26 + 65) COLLATE Latin1_General_BIN +
						NCHAR((@RowNumber-1) / 26 % 26 + 65) COLLATE Latin1_General_BIN +
						NCHAR((@RowNumber-1) % 26 + 65) COLLATE Latin1_General_BIN

	RETURN @IndentLevel
END
GO
