-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Converts a list of values to a table
-- Remarks:			Used in when you need to pass a list of values to a stored proc
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_CONVERT_LIST_TO_TABLE' AND xtype = 'TF' )
BEGIN
	DROP FUNCTION FN_GLOBAL_CONVERT_LIST_TO_TABLE;
END
GO
CREATE FUNCTION FN_GLOBAL_CONVERT_LIST_TO_TABLE
(
	@List NVARCHAR(MAX),
	@Delim NCHAR
)
RETURNS @ParsedList TABLE
(
	ITEM NVARCHAR(MAX)
)
AS
BEGIN
	DECLARE @item NVARCHAR(MAX), @Pos INT
	SET @List = LTRIM(RTRIM(@List))+ @Delim
	SET @Pos = CHARINDEX(@Delim, @List, 1)
	WHILE @Pos > 0
	BEGIN
		SET @item = LTRIM(RTRIM(LEFT(@List, @Pos - 1)))
		IF @item <> ''
		BEGIN
			INSERT INTO @ParsedList (ITEM)
			VALUES (CAST(@item AS NVARCHAR(MAX)))
		END
		SET @List = RIGHT(@List, LEN(@List) - @Pos)
		SET @Pos = CHARINDEX(@Delim, @List, 1)
	END
	RETURN
END
GO
