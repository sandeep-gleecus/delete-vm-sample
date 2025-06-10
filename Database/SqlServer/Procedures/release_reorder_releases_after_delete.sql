-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		This function reorders a section of the release tree after a delete
--					operation It syncs up the 'indent' level string with the actual
--					normalized data
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_REORDER_RELEASES_AFTER_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REORDER_RELEASES_AFTER_DELETE;
GO
CREATE PROCEDURE RELEASE_REORDER_RELEASES_AFTER_DELETE
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
							dbo.FN_GLOBAL_DECREMENT_INDENT_LEVEL(SUBSTRING(INDENT_LEVEL, (@Length-3) + 1, 3)) + 
							SUBSTRING(INDENT_LEVEL, @Length + 1, LEN(INDENT_LEVEL) - @Length)
	WHERE
		PROJECT_ID = @ProjectId AND
		SUBSTRING(INDENT_LEVEL, 1, @Length-3) = @ParentIndentLevel AND
		SUBSTRING(INDENT_LEVEL, (@Length - 3) + 1, 3) > @ItemIndentElement

END
GO
