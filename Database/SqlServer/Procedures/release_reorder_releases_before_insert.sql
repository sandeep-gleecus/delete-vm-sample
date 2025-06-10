-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		This function reorders a section of the release tree before an
--					insert operation so that there is space in the releases indent-level
--					scheme for the new item
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_REORDER_RELEASES_BEFORE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REORDER_RELEASES_BEFORE_INSERT;
GO
CREATE PROCEDURE RELEASE_REORDER_RELEASES_BEFORE_INSERT
	@UserId INT,
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	DECLARE
		@Length INT,
		@ParentIndentLevel NVARCHAR(100),
		@ItemIndentElement NVARCHAR(100);

	--First split the indent-level into the parent section and then the section that is to be modified
	SET @Length = LEN(@IndentLevel);
	SET @ParentIndentLevel = SUBSTRING(@IndentLevel, 1, @Length-3);
	SET @ItemIndentElement = SUBSTRING(@IndentLevel, (@Length-3) + 1, 3);

	--Update all the subsequent items that have a common parent that need to be updated
	UPDATE TST_RELEASE
		SET INDENT_LEVEL =	SUBSTRING(INDENT_LEVEL, 1, @Length-3) +
							dbo.FN_GLOBAL_INCREMENT_INDENT_LEVEL(SUBSTRING(INDENT_LEVEL, (@Length-3) + 1, 3)) + 
							SUBSTRING(INDENT_LEVEL, @Length + 1, LEN(INDENT_LEVEL) - @Length)
	WHERE
		PROJECT_ID = @ProjectId AND
		SUBSTRING(INDENT_LEVEL, 1, @Length-3) = @ParentIndentLevel AND
		SUBSTRING(INDENT_LEVEL, (@Length - 3) + 1, 3) >= @ItemIndentElement

END
GO
