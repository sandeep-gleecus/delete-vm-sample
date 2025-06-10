-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Inserts a Requirement
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_INSERT;
GO
CREATE PROCEDURE REQUIREMENT_INSERT
	@ProjectId INT,
	@ReleaseId INT,
	@StatusId INT,
	@TypeId INT,
	@AuthorId INT,
	@OwnerId INT,
	@ImportanceId INT,
	@ComponentId INT,
	@Name NVARCHAR(255),
	@Description NVARCHAR(MAX),
	@CreationDate DATETIME,
	@LastUpdateDate DATETIME,
	@ConcurrencyDate DATETIME,
	@IndentLevel NVARCHAR(100),
	@IsSummary BIT,
	@CoverageCountTotal INT,
	@CoverageCountPassed INT,
	@CoverageCountFailed INT,
	@CoverageCountCaution INT,
	@CoverageCountBlocked INT,
	@IsAttachments BIT,
	@TaskCount INT,
	@TaskPercentOnTime INT,
	@TaskPercentLateFinish INT,
	@TaskPercentNotStart INT,
	@TaskPercentLateStart INT,
	@EstimatePoints DECIMAL(9,1),
	@EstimatedEffort INT,
	@IsExpanded BIT,
	@IsVisible BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @NEW_REQUIREMENT_ID INT;
	
	--Insert into main table
    INSERT INTO TST_REQUIREMENT
		(PROJECT_ID, RELEASE_ID, REQUIREMENT_STATUS_ID, AUTHOR_ID, OWNER_ID, IMPORTANCE_ID, REQUIREMENT_TYPE_ID, COMPONENT_ID, 
		NAME, DESCRIPTION, CREATION_DATE, LAST_UPDATE_DATE, CONCURRENCY_DATE,
		INDENT_LEVEL, IS_SUMMARY, COVERAGE_COUNT_TOTAL,	COVERAGE_COUNT_PASSED,COVERAGE_COUNT_FAILED, COVERAGE_COUNT_CAUTION,
		COVERAGE_COUNT_BLOCKED,
		IS_ATTACHMENTS, TASK_COUNT, TASK_PERCENT_ON_TIME, TASK_PERCENT_LATE_FINISH,	TASK_PERCENT_NOT_START,
		TASK_PERCENT_LATE_START, ESTIMATE_POINTS, ESTIMATED_EFFORT)
	VALUES
		(@ProjectId,
		@ReleaseId,
		@StatusId,
		@AuthorId,
		@OwnerId,
		@ImportanceId,
		@TypeId,
		@ComponentId,
		@Name,
		@Description,
		@CreationDate,
		@LastUpdateDate,
		@ConcurrencyDate,
		@IndentLevel,
		@IsSummary,
		@CoverageCountTotal,
		@CoverageCountPassed,
		@CoverageCountFailed,
		@CoverageCountCaution,
		@CoverageCountBlocked,
		@IsAttachments,
		@TaskCount,
		@TaskPercentOnTime,
		@TaskPercentLateFinish,
		@TaskPercentNotStart,
		@TaskPercentLateStart,
		@EstimatePoints,
		@EstimatedEffort);
    SET @NEW_REQUIREMENT_ID = @@IDENTITY;
	
	--Insert into user navigation table
    INSERT INTO TST_REQUIREMENT_USER
		(REQUIREMENT_ID, USER_ID, IS_EXPANDED, IS_VISIBLE)
	VALUES (@NEW_REQUIREMENT_ID, @UserId, @IsExpanded, @IsVisible);

	--Return back the new primary key
    SELECT REQ.REQUIREMENT_ID
	FROM TST_REQUIREMENT REQ
	WHERE (REQ.REQUIREMENT_ID = @NEW_REQUIREMENT_ID);
END
GO
