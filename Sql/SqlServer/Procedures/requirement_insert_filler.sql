-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Inserts a new 'filler' requirement when we're fixing any hierarchy errors
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_INSERT_FILLER', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_INSERT_FILLER;
GO
CREATE PROCEDURE REQUIREMENT_INSERT_FILLER
	@ProjectId INT,
	@Name NVARCHAR(255),
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	--Now insert the filler requirement
    INSERT INTO TST_REQUIREMENT (
		AUTHOR_ID, 
		PROJECT_ID, 
		REQUIREMENT_STATUS_ID, 
		REQUIREMENT_TYPE_ID, 
		[NAME], 
		CREATION_DATE, 
		INDENT_LEVEL, 
		LAST_UPDATE_DATE, 
		CONCURRENCY_DATE, 
		IS_SUMMARY, 
		IS_ATTACHMENTS, 
		COVERAGE_COUNT_TOTAL, 
		COVERAGE_COUNT_PASSED, 
		COVERAGE_COUNT_FAILED, 
		COVERAGE_COUNT_CAUTION, 
		COVERAGE_COUNT_BLOCKED, 
		TASK_COUNT, 
		TASK_PERCENT_ON_TIME, 
		TASK_PERCENT_LATE_FINISH, 
		TASK_PERCENT_NOT_START, 
		TASK_PERCENT_LATE_START)
	VALUES (
		1,				--Owner
		@ProjectId,		--Project ID
		1,				--Status (Requested)
		-1,				--Type ('Epic')
		@Name,			--Name
		GETUTCDATE(),	--Creation Date
		@IndentLevel,	--Indent Level
		GETUTCDATE(),	--Last Updated Date
		GETUTCDATE(),	--Concurrency Date
		1,				--IsSummary
		0,				--IsAttachments
		0,				--Total Converage Count
		0,				--Passed Coverage Count
		0,				--Failed Coverage Count
		0,				--Caution Coverage Count
		0,				--Blocked Coverage Count
		0,				--Task Count
		0,				--Task % On Time
		0,				--Task % Late Finish
		0,				--Task % Not Start
		0)				--Task % Late Start
END
GO
