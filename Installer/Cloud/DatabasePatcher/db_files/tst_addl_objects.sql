-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object:	ArtifactManager
-- Description:		Converts a list of project id:artifact ids (1:2,1:3, etc.) to a table
-- Remarks:			Used in the global search to limit permissions
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE' AND xtype = 'TF' )
BEGIN
	DROP FUNCTION FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE;
END
GO
CREATE FUNCTION FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE
(
	@List NVARCHAR(MAX)
)
RETURNS @ParsedList TABLE
(
	PROJECT_ID INT,
	ARTIFACT_TYPE_ID INT
)
AS
BEGIN
	DECLARE @Item NVARCHAR(MAX), @ProjectId NVARCHAR(MAX), @ArtifactTypeId NVARCHAR(MAX), @Pos INT, @Pos2 INT
	SET @List = LTRIM(RTRIM(@List))+ ','
	SET @Pos = CHARINDEX(',', @List, 1)
	WHILE @Pos > 0
	BEGIN
		SET @Item = LTRIM(RTRIM(LEFT(@List, @Pos - 1)))
		SET @Pos2 = CHARINDEX(':', @Item)
		IF @Pos2 > 0
		BEGIN
			SET @ProjectId = LTRIM(RTRIM(LEFT(@Item, @Pos2 - 1)))
			SET @ArtifactTypeId = LTRIM(RTRIM(RIGHT(@Item, LEN(@Item) - @Pos2)))
			IF @ProjectId <> '' AND @ArtifactTypeId <> ''
			BEGIN
				INSERT INTO @ParsedList (PROJECT_ID, ARTIFACT_TYPE_ID)
				VALUES (CAST(@ProjectId AS INT), CAST(@ArtifactTypeId AS INT))
			END
		END
		SET @List = RIGHT(@List, LEN(@List) - @Pos)
		SET @Pos = CHARINDEX(',', @List, 1)
	END
	RETURN
END
GO
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
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a == on an NVARCHAR column
-- Remarks:			Used when filtering an integer custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_EQUALS_INT' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT
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
		IF (CAST(@Operand AS INT) = @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a == on an NVARCHAR column
-- Remarks:			Used when filtering an integer custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_EQUALS_INT_LIST' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT_LIST;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_EQUALS_INT_LIST
(
	@Operand NVARCHAR(MAX), /* 1,2,3 or 0000000001,0000000002,0000000003 */
	@Constant INT
)
RETURNS BIT
AS
BEGIN
 	DECLARE
 		@Result AS BIT,
 		@Padded AS NVARCHAR(MAX)
	SET @Result = 0
	IF @Operand IS NOT NULL
	BEGIN
		--First test for unpadded
		IF CHARINDEX(',' + CAST(@Constant AS NVARCHAR) + ',', ',' + @Operand + ',') > 0
		BEGIN
			SET @Result = 1
		END
		--Next test for padded
		SET @Padded = REPLACE(STR(@Constant, 10), SPACE(1), '0')
		IF CHARINDEX(',' + @Padded + ',', ',' + @Operand + ',') > 0
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a >= on an NVARCHAR column
-- Remarks:			Used when filtering a datetime custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME
(
	@Operand NVARCHAR(MAX),
	@Constant DATETIME,
	@UtcOffsetHours INT,
	@UtcOffsetMins INT,
	@ConsiderTimes BIT
)
RETURNS BIT
AS
BEGIN
	DECLARE
		@Result AS BIT,
		@DateTime AS DATETIME
	SET @Result = 0
	--Style 126 = yyyy-mm-ddThh:mi:ss.mmm (ISO8601)
	IF ISDATE(@Operand) = 0 RETURN NULL
	SET @DateTime = CONVERT(DATETIME, @Operand, 126)
	IF @ConsiderTimes = 1
	BEGIN
		IF DATEADD(minute,@UtcOffsetMins,DATEADD(hour,@UtcOffsetHours,@DateTime)) >= @Constant
		BEGIN
			SET @Result = 1
		END
	END
	ELSE
	BEGIN
		IF CAST(FLOOR(CAST(DATEADD(minute,@UtcOffsetMins,DATEADD(hour,@UtcOffsetHours,@DateTime)) AS FLOAT))AS DATETIME) >= @Constant
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a >= on an NVARCHAR column
-- Remarks:			Used when filtering an decimal custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_GREATER_THAN_DECIMAL' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_DECIMAL;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_DECIMAL
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
		IF (CAST(@Operand AS DECIMAL(20,4)) >= @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a >= on an NVARCHAR column
-- Remarks:			Used when filtering an integer custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_GREATER_THAN_INT' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_INT;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_GREATER_THAN_INT
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
		IF (CAST(@Operand AS INT) >= @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a <= on an NVARCHAR column
-- Remarks:			Used when filtering a datetime custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME
(
	@Operand NVARCHAR(MAX),
	@Constant DATETIME,
	@UtcOffsetHours INT,
	@UtcOffsetMins INT,
	@ConsiderTimes BIT
)
RETURNS BIT
AS
BEGIN
	DECLARE
		@Result AS BIT,
		@DateTime AS DATETIME
	SET @Result = 0
	--Style 126 = yyyy-mm-ddThh:mi:ss.mmm (ISO8601)
	IF ISDATE(@Operand) = 0 RETURN NULL
	SET @DateTime = CONVERT(DATETIME, @Operand, 126)
	IF @ConsiderTimes = 1
	BEGIN
		IF DATEADD(minute,@UtcOffsetMins,DATEADD(hour,@UtcOffsetHours,@DateTime)) <= @Constant
		BEGIN
			SET @Result = 1
		END
	END
	ELSE
	BEGIN
		IF CAST(FLOOR(CAST(DATEADD(minute,@UtcOffsetMins,DATEADD(hour,@UtcOffsetHours,@DateTime)) AS FLOAT))AS DATETIME) <= @Constant
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Does a <= on an NVARCHAR column
-- Remarks:			Used when filtering a decimal custom property
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_CUSTOM_PROPERTY_LESS_THAN_DECIMAL' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_DECIMAL;
END
GO
CREATE FUNCTION FN_CUSTOM_PROPERTY_LESS_THAN_DECIMAL
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
		IF (CAST(@Operand AS DECIMAL(20,4)) <= @Constant)
		BEGIN
			SET @Result = 1
		END
	END
	RETURN @Result
END
GO
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
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Decrements an indent level (e.g. AAAABAAAC - 1 => AAAABAAAB)
-- Remarks:			Primarily used in requirements and release stored procedures
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_DECREMENT_INDENT_LEVEL' AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FN_GLOBAL_DECREMENT_INDENT_LEVEL;
END
GO
CREATE FUNCTION FN_GLOBAL_DECREMENT_INDENT_LEVEL
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
        IF(@char!='A')
        BEGIN
			SET @char=NCHAR(ASCII(@char)-1);
			SET @OutputIndentLevel = STUFF(@OutputIndentLevel,@position,1,@char);
			SET @position = 0;	--Break
		END
		ELSE
		BEGIN
			SET @OutputIndentLevel = STUFF(@OutputIndentLevel,@position,1,'Z');
			SET @position = @position - 1;	--Next Level
		END
	END

	RETURN @OutputIndentLevel;
END
GO
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
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Safely checks to see if a string can be converted to INT and returns it if so
-- Remarks:			Unlike ISNUMERIC handles the case of decimals, money, etc.
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_GLOBAL_TRY_GET_INT' AND xtype = 'FN' )
BEGIN
	DROP FUNCTION FN_GLOBAL_TRY_GET_INT;
END
GO
CREATE FUNCTION FN_GLOBAL_TRY_GET_INT
(
	@Value NVARCHAR(MAX)
)
RETURNS INT
AS
BEGIN
    SET @Value = REPLACE(@Value, ',', '')
    IF ISNUMERIC(@Value + 'e0') = 0 RETURN NULL
    IF ( CHARINDEX('.', @Value) > 0 AND CONVERT(INT, PARSENAME(@Value, 1)) <> 0 ) RETURN NULL
    DECLARE @I INT
    SET @I =
        CASE
        WHEN CHARINDEX('.', @Value) > 0 THEN CONVERT(INT, PARSENAME(@Value, 2))
        ELSE CONVERT(INT, @Value)
        END
    IF ABS(@I) > 2147483647 RETURN NULL
    RETURN @I
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of items under a release
--					as well as the release itself
-- Remarks:			Used in cases where you can't use a stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_RELEASE_GET_SELF_AND_CHILDREN' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_RELEASE_GET_SELF_AND_CHILDREN;
END
GO
CREATE FUNCTION FN_RELEASE_GET_SELF_AND_CHILDREN
(
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeMajorBranches BIT
)
RETURNS @ReleaseAndIterations TABLE
(
	RELEASE_ID INT
)
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT
	DECLARE @ChildMajorReleases TABLE
	(
		INDENT_LEVEL NVARCHAR(100)
	)

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId
    	IF (@IndentLevel IS NULL)
    	BEGIN
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	IF @IncludeMajorBranches = 1
	BEGIN
		--Now get the list of all children and the release itself and populate table
		INSERT INTO @ReleaseAndIterations
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
			AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
			ORDER BY INDENT_LEVEL
	END
	ELSE
	BEGIN
		--Get a list of all the child major releases (that we need to ignore)
		INSERT INTO @ChildMajorReleases
			SELECT INDENT_LEVEL
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
			AND LEN(INDENT_LEVEL) > @IndentLevelLength
			AND RELEASE_TYPE_ID = 1 /* Major Release */
				
		--Now get the list of all children and the release itself and populate table
		INSERT INTO @ReleaseAndIterations
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
			AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
			ORDER BY INDENT_LEVEL
			
		--Prune child major branches
		DELETE FROM @ReleaseAndIterations
		WHERE RELEASE_ID IN (
			SELECT REL.RELEASE_ID FROM TST_RELEASE REL INNER JOIN @ChildMajorReleases MAJ
			ON dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(REL.INDENT_LEVEL, MAJ.INDENT_LEVEL, 100) = 1
			WHERE REL.PROJECT_ID = @ProjectId AND REL.RELEASE_ID <> @ReleaseId)
	END
	RETURN
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of iterations under a release
--					as well as the release itself
-- Remarks:			Used in cases where you can't use a stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_RELEASE_GET_SELF_AND_ITERATIONS' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_RELEASE_GET_SELF_AND_ITERATIONS;
END
GO
CREATE FUNCTION FN_RELEASE_GET_SELF_AND_ITERATIONS
(
	@ProjectId INT,
	@ReleaseId INT
)
RETURNS @ReleaseAndIterations TABLE
(
	RELEASE_ID INT
)
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId
    	IF (@IndentLevel IS NULL)
    	BEGIN
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of iterations and the release itself and populate table
	INSERT INTO @ReleaseAndIterations
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (RELEASE_TYPE_ID = 3/*Iteration*/ AND LEN(INDENT_LEVEL) = (@IndentLevelLength + 3)))
		ORDER BY INDENT_LEVEL
	RETURN
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of parent releases that a release/iteration/phase
--					should roll up to as well as the item itself
-- Remarks:			Used in cases where you can't use a stored procedure
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS;
END
GO
CREATE FUNCTION FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS
(
	@ProjectId INT,
	@ReleaseId INT
)
RETURNS @RollupReleases TABLE
(
	RELEASE_ID INT
)
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT
	DECLARE @MajorReleaseIndentLevel NVARCHAR(100)

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId
	IF (@IndentLevel IS NULL)
	BEGIN
		RETURN
	END
	SET @IndentLevelLength = LEN(@IndentLevel)

	--Get the lowest-indent parent major release (including self)
	SELECT TOP(1) @MajorReleaseIndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(@IndentLevel, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) < @IndentLevelLength))
	AND RELEASE_TYPE_ID = 1 /* Major Release */
	ORDER BY INDENT_LEVEL DESC
			
	--Now get the list of all parents and the item itself,
	--Do not include any indent higher than the lowest major release
	INSERT INTO @RollupReleases
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(@IndentLevel, LEN(INDENT_LEVEL)) = INDENT_LEVEL
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) < @IndentLevelLength))
		AND (INDENT_LEVEL >= @MajorReleaseIndentLevel OR @MajorReleaseIndentLevel IS NULL)
		ORDER BY INDENT_LEVEL

	RETURN
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves a requirement and all its child requirements
-- Remarks:			Used in cases where you can't use a stored procedure.
--					Does not include deleted requirements
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_REQUIREMENT_GET_SELF_AND_CHILDREN' AND xtype = 'TF' )
BEGIN
    DROP FUNCTION FN_REQUIREMENT_GET_SELF_AND_CHILDREN;
END
GO
CREATE FUNCTION FN_REQUIREMENT_GET_SELF_AND_CHILDREN
(
	@ProjectId INT,
	@RequirementId INT
)
RETURNS @RequirementAndChildren TABLE
(
	REQUIREMENT_ID INT
)
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_REQUIREMENT
	WHERE REQUIREMENT_ID = @RequirementId
    	IF (@IndentLevel IS NULL)
    	BEGIN
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of children and the item itself and populate table
	INSERT INTO @RequirementAndChildren
		SELECT REQUIREMENT_ID
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (REQUIREMENT_ID = @RequirementId OR (LEN(INDENT_LEVEL) > (@IndentLevelLength)))
		AND IS_DELETED = 0
		ORDER BY INDENT_LEVEL
	RETURN
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Description:		Retrieves the list of test steps for a test case, but includes the display position (step number)
-- Remarks:			Only used in the ArtifactLink business object so only returning the columns it needs
-- ================================================================
IF EXISTS(SELECT * FROM sysobjects WHERE name = 'FN_TESTSTEP_RETRIEVE_STEP_NUMBER' AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FN_TESTSTEP_RETRIEVE_STEP_NUMBER;
END
GO
CREATE FUNCTION FN_TESTSTEP_RETRIEVE_STEP_NUMBER
(
	@TestCaseId INT,
	@TestStepId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @StepNumber INT

	--Now get the position number (accounting for deletes) of this test step in the test case
    SELECT @StepNumber = STEP_NUMBER FROM
	(
   		SELECT TEST_STEP_ID,ROW_NUMBER() OVER(ORDER BY POSITION ASC) AS 'STEP_NUMBER'
		FROM TST_TEST_STEP
		WHERE TEST_CASE_ID = @TestCaseId
		AND	IS_DELETED = 0
	) STP WHERE TEST_STEP_ID = @TestStepId

	RETURN @StepNumber
END
GO
IF OBJECT_ID ( 'VW_RELEASE_LIST_INTERNAL', 'V' ) IS NOT NULL 
    DROP VIEW VW_RELEASE_LIST_INTERNAL;
GO

CREATE VIEW VW_RELEASE_LIST_INTERNAL
AS
	
SELECT
	REL.*,
	(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
	(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
	(REL.VERSION_NUMBER + ' - ' + REL.NAME) AS FULL_NAME, 
	RTY.NAME AS RELEASE_TYPE_NAME, RST.NAME AS RELEASE_STATUS_NAME, BCH.NAME AS BRANCH_NAME,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
FROM TST_RELEASE AS REL
	INNER JOIN TST_RELEASE_TYPE RTY ON REL.RELEASE_TYPE_ID = RTY.RELEASE_TYPE_ID
	INNER JOIN TST_RELEASE_STATUS RST ON REL.RELEASE_STATUS_ID = RST.RELEASE_STATUS_ID
	INNER JOIN TST_USER_PROFILE USR1 ON REL.CREATOR_ID = USR1.USER_ID
	LEFT JOIN TST_USER_PROFILE USR2 ON REL.OWNER_ID = USR2.USER_ID
	LEFT JOIN TST_VERSION_CONTROL_BRANCH BCH ON REL.BRANCH_ID = BCH.BRANCH_ID
	LEFT JOIN (
		SELECT *
		FROM TST_ARTIFACT_CUSTOM_PROPERTY
		WHERE ARTIFACT_TYPE_ID = 4) AS ACP ON REL.RELEASE_ID = ACP.ARTIFACT_ID

GO
IF OBJECT_ID ('VW_ADMINHISTORYCHANGE_LIST', 'V') IS NOT NULL 
    DROP VIEW [VW_ADMINHISTORYCHANGE_LIST];
GO

CREATE VIEW [dbo].[VW_ADMINHISTORYCHANGE_LIST]
AS
	SELECT 
		HC.*,
		HD.ADMIN_HISTORY_DETAIL_ID
      ,HD.ADMIN_ARTIFACT_FIELD_NAME
      ,HD.OLD_VALUE
      ,HD.ADMIN_ARTIFACT_FIELD_CAPTION
      ,HD.NEW_VALUE
      ,HD.OLD_VALUE_INT
      ,HD.OLD_VALUE_DATE
      ,HD.NEW_VALUE_INT
      ,HD.NEW_VALUE_DATE
      ,HD.ADMIN_CHANGESET_ID
      ,HD.ADMIN_ARTIFACT_FIELD_ID
      ,HD.ADMIN_CUSTOM_PROPERTY_ID,
	  AD.NAME AS ADMIN_SECTION_NAME,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME
		
	FROM [ValidationMasterAudit].dbo.TST_ADMIN_HISTORY_CHANGESET_AUDIT AS HC
		INNER JOIN [ValidationMasterAudit].dbo.TST_ADMIN_HISTORY_DETAILS_AUDIT AS HD ON HC.CHANGESET_ID = HD.ADMIN_CHANGESET_ID
		INNER JOIN [ValidationMasterAudit].dbo.TST_ADMIN_SECTION_AUDIT AS AD ON HC.ADMIN_SECTION_ID = AD.ADMIN_SECTION_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.HISTORY_CHANGESET_TYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.ADMIN_USER_ID = US.USER_ID
GO/* ---------------------------------------------------------------------- */
/* Add View "VW_ARTIFACT_ATTACHMENT"                                */
/* ---------------------------------------------------------------------- */

IF OBJECT_ID ( 'VW_ARTIFACT_ATTACHMENT', 'V' ) IS NOT NULL 
    DROP VIEW [VW_ARTIFACT_ATTACHMENT];
GO

CREATE VIEW [dbo].[VW_ARTIFACT_ATTACHMENT]
AS
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_REQUIREMENT OBJ ON AAT.ARTIFACT_ID = OBJ.REQUIREMENT_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.AUTHOR_ID = USR.USER_ID INNER JOIN
                         TST_REQUIREMENT_STATUS STA ON OBJ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 1
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_TEST_CASE OBJ ON AAT.ARTIFACT_ID = OBJ.TEST_CASE_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.AUTHOR_ID = USR.USER_ID INNER JOIN
                         TST_EXECUTION_STATUS STA ON OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 2
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.OPENER_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_INCIDENT OBJ ON AAT.ARTIFACT_ID = OBJ.INCIDENT_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.OPENER_ID = USR.USER_ID INNER JOIN
                         TST_INCIDENT_STATUS STA ON OBJ.INCIDENT_STATUS_ID = STA.INCIDENT_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 3
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.CREATOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         '-' AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_RELEASE OBJ ON AAT.ARTIFACT_ID = OBJ.RELEASE_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.CREATOR_ID = USR.USER_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 4
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.TESTER_ID AS CREATOR_ID, OBJ.START_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_TEST_RUN OBJ ON AAT.ARTIFACT_ID = OBJ.TEST_RUN_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.TESTER_ID = USR.USER_ID INNER JOIN
                         TST_EXECUTION_STATUS STA ON OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 5
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         ATC.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, ATC.IS_DELETED AS IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_TASK OBJ ON AAT.ARTIFACT_ID = OBJ.TASK_ID INNER JOIN
                         TST_ATTACHMENT ATC ON AAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID INNER JOIN
                         TST_USER_PROFILE USR ON ATC.AUTHOR_ID = USR.USER_ID INNER JOIN
                         TST_TASK_STATUS STA ON OBJ.TASK_STATUS_ID = STA.TASK_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 6
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, ('Test Case ' + CAST(OBJ.TEST_CASE_ID AS NVARCHAR) 
                         + ' Test Step ' + CAST(OBJ.POSITION AS NVARCHAR)) AS ARTIFACT_NAME, '' AS COMMENT, ATC.AUTHOR_ID AS CREATOR_ID, ATC.UPLOAD_DATE AS CREATION_DATE, 
                         (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME, 
                         ATC.IS_DELETED AS IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_TEST_STEP OBJ ON AAT.ARTIFACT_ID = OBJ.TEST_STEP_ID INNER JOIN
                         TST_ATTACHMENT ATC ON AAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID INNER JOIN
                         TST_USER_PROFILE USR ON ATC.AUTHOR_ID = USR.USER_ID INNER JOIN
                         TST_EXECUTION_STATUS STA ON OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 7
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 
                         OBJ.CREATOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, 
                         STA.NAME AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_TEST_SET OBJ ON AAT.ARTIFACT_ID = OBJ.TEST_SET_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.CREATOR_ID = USR.USER_ID INNER JOIN
                         TST_TEST_SET_STATUS STA ON OBJ.TEST_SET_STATUS_ID = STA.TEST_SET_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 8
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 1 AS CREATOR_ID, 
                         LAST_UPDATE_DATE AS CREATION_DATE, '-' AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, '-' AS ARTIFACT_STATUS_NAME, IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_AUTOMATION_HOST OBJ ON AAT.ARTIFACT_ID = OBJ.AUTOMATION_HOST_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 9
UNION
SELECT        (AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.CREATOR_ID, 
                         OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL, '')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME, 
                         IS_DELETED
FROM            TST_ARTIFACT_ATTACHMENT AAT INNER JOIN
                         TST_ARTIFACT_TYPE ART ON AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN
                         TST_RISK OBJ ON AAT.ARTIFACT_ID = OBJ.RISK_ID INNER JOIN
                         TST_USER_PROFILE USR ON OBJ.CREATOR_ID = USR.USER_ID INNER JOIN
                         TST_RISK_STATUS STA ON OBJ.RISK_STATUS_ID = STA.RISK_STATUS_ID
WHERE        AAT.ARTIFACT_TYPE_ID = 14
GOIF OBJECT_ID ( 'VW_ARTIFACT_LINK_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_ARTIFACT_LINK_LIST];
GO
CREATE VIEW [VW_ARTIFACT_LINK_LIST]
AS
    SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
    		ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ARL.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME,
    		STA.NAME AS ARTIFACT_STATUS_NAME, REQ.PROJECT_ID AS PROJECT_ID
    FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
    ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
    ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT REQ
    ON		ARL.DEST_ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
GO
IF OBJECT_ID ( 'VW_ARTIFACT_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_ARTIFACT_LIST];
GO
CREATE VIEW [VW_ARTIFACT_LIST]
AS
	-- Artifact Records
	SELECT	1 AS ARTIFACT_TYPE_ID, ART.REQUIREMENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_REQUIREMENT ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	2 AS ARTIFACT_TYPE_ID, ART.TEST_CASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_TEST_CASE ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	3 AS ARTIFACT_TYPE_ID, ART.INCIDENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_INCIDENT ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	4 AS ARTIFACT_TYPE_ID, ART.RELEASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_RELEASE ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	5 AS ARTIFACT_TYPE_ID, TRN.TEST_RUN_ID AS ARTIFACT_ID, TST.PROJECT_ID, TRN.NAME,
			TST.DESCRIPTION, TST.IS_DELETED, TRN.START_DATE AS CREATION_DATE,
			TRN.END_DATE AS LAST_UPDATE_DATE, PRJ.NAME AS PROJECT_NAME, 0 AS RANK
	FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_CASE TST
	ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID INNER JOIN TST_PROJECT PRJ
	ON		TST.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	6 AS ARTIFACT_TYPE_ID, ART.TASK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_TASK ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	7 AS ARTIFACT_TYPE_ID, STP.TEST_STEP_ID AS ARTIFACT_ID, TST.PROJECT_ID, TST.NAME,
			STP.DESCRIPTION, STP.IS_DELETED, STP.LAST_UPDATE_DATE AS CREATION_DATE,
			STP.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_TEST_STEP STP INNER JOIN TST_TEST_CASE TST
	ON		STP.TEST_CASE_ID = TST.TEST_CASE_ID INNER JOIN TST_PROJECT PRJ
	ON		TST.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	8 AS ARTIFACT_TYPE_ID, ART.TEST_SET_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_TEST_SET ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	9 AS ARTIFACT_TYPE_ID, ART.AUTOMATION_HOST_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.LAST_UPDATE_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_AUTOMATION_HOST ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	13 AS ARTIFACT_TYPE_ID, ART.ATTACHMENT_ID AS ARTIFACT_ID, PRA.PROJECT_ID, ART.FILENAME,
			ART.DESCRIPTION, CAST (0 AS BIT) AS IS_DELETED, ART.UPLOAD_DATE AS CREATION_DATE, ART.EDITED_DATE AS LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_ATTACHMENT ART INNER JOIN TST_PROJECT_ATTACHMENT PRA ON ART.ATTACHMENT_ID = PRA.ATTACHMENT_ID
			INNER JOIN TST_PROJECT PRJ ON PRA.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	14 AS ARTIFACT_TYPE_ID, ART.RISK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
			ART.DESCRIPTION, ART.IS_DELETED, ART.CREATION_DATE, ART.LAST_UPDATE_DATE,
			PRJ.NAME PROJECT_NAME, 0 AS RANK
	FROM	TST_RISK ART INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.IS_ACTIVE = 1
	UNION ALL
	SELECT	-3 AS ARTIFACT_TYPE_ID, USR.USER_ID AS ARTIFACT_ID, -1 AS PROJECT_ID,
			(RTRIM(USP.FIRST_NAME + ' ' + ISNULL(USP.MIDDLE_INITIAL,'')) + ' ' + USP.LAST_NAME) AS NAME,
			'' AS DESCRIPTION, CAST (0 AS BIT) AS IS_DELETED, USR.CREATION_DATE, USP.LAST_UPDATE_DATE, '' AS PROJECT_NAME, 0 AS RANK
	FROM	TST_USER_PROFILE USP INNER JOIN TST_USER USR ON USP.USER_ID = USR.USER_ID
	WHERE	USR.IS_ACTIVE = 1 AND USR.IS_APPROVED = 1
GO
IF OBJECT_ID ( 'VW_ARTIFACT_SOURCE_CODE_REVISION_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_ARTIFACT_SOURCE_CODE_REVISION_LIST];
GO
CREATE VIEW [VW_ARTIFACT_SOURCE_CODE_REVISION_LIST]
AS
	SELECT	ASR.ARTIFACT_SOURCE_CODE_REVISION_ID, ASR.ARTIFACT_TYPE_ID, ASR.ARTIFACT_ID, ASR.REVISION_KEY, ASR.COMMENT, ASR.CREATION_DATE, TYP.NAME AS ARTIFACT_TYPE_NAME, TYP.PREFIX AS ARTIFACT_TYPE_PREFIX
	FROM	TST_ARTIFACT_SOURCE_CODE_REVISION ASR
	INNER JOIN TST_ARTIFACT_TYPE TYP ON ASR.ARTIFACT_TYPE_ID = TYP.ARTIFACT_TYPE_ID	
	UNION ALL
	SELECT	-1 AS ARTIFACT_SOURCE_CODE_REVISION_ID, SCA.ARTIFACT_TYPE_ID, SCA.ARTIFACT_ID, SCC.REVISION_KEY, SCC.MESSAGE AS COMMENT, SCC.UPDATE_DATE AS CREATION_DATE, TYP.NAME AS ARTIFACT_TYPE_NAME, TYP.PREFIX AS ARTIFACT_TYPE_PREFIX
	FROM	TST_SOURCE_CODE_COMMIT_ARTIFACT SCA
	INNER JOIN TST_SOURCE_CODE_COMMIT SCC ON
		SCA.REVISION_ID = SCC.REVISION_ID AND
		SCA.VERSION_CONTROL_SYSTEM_ID = SCC.VERSION_CONTROL_SYSTEM_ID AND
		SCA.PROJECT_ID = SCC.PROJECT_ID
	INNER JOIN TST_ARTIFACT_TYPE TYP ON SCA.ARTIFACT_TYPE_ID = TYP.ARTIFACT_TYPE_ID	
GO
IF OBJECT_ID ( 'VW_ATTACHMENT_VERSION_LIST', 'V' ) IS NOT NULL 
    DROP VIEW VW_ATTACHMENT_VERSION_LIST;
GO

CREATE VIEW VW_ATTACHMENT_VERSION_LIST
AS
	SELECT	ATV.*, 
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS AUTHOR_NAME,
			ATC.ATTACHMENT_TYPE_ID
	FROM	TST_ATTACHMENT_VERSION ATV INNER JOIN TST_USER_PROFILE USR
	ON		ATV.AUTHOR_ID = USR.USER_ID INNER JOIN TST_ATTACHMENT ATC
	ON		ATV.ATTACHMENT_ID = ATC.ATTACHMENT_ID
GO
IF OBJECT_ID ( 'VW_AUTOMATIONHOST_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_AUTOMATIONHOST_LIST];
GO
CREATE VIEW [VW_AUTOMATIONHOST_LIST]
AS
	SELECT ATH.*,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_AUTOMATION_HOST AS ATH
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 9
			) AS ACP ON ATH.AUTOMATION_HOST_ID = ACP.ARTIFACT_ID
GO
IF OBJECT_ID ( 'VW_BUILD_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_BUILD_LIST];
GO
CREATE VIEW [VW_BUILD_LIST]
AS
	SELECT	BLD.BUILD_ID, BLD.BUILD_STATUS_ID, BLD.RELEASE_ID, BLD.PROJECT_ID, BLD.NAME, BLD.IS_DELETED,
			BLD.CREATION_DATE, BLD.LAST_UPDATE_DATE, BLS.NAME AS BUILD_STATUS_NAME,
			REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER, PRJ.NAME AS PROJECT_NAME,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE, PRJ.PROJECT_GROUP_ID
	FROM	TST_BUILD BLD
	INNER JOIN TST_BUILD_STATUS AS BLS ON BLD.BUILD_STATUS_ID = BLS.BUILD_STATUS_ID
	INNER JOIN TST_RELEASE AS REL ON BLD.RELEASE_ID = REL.RELEASE_ID
	INNER JOIN TST_PROJECT AS PRJ ON BLD.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'VW_DATATOOLS_RELEASE', 'V' ) IS NOT NULL 
    DROP VIEW [VW_DATATOOLS_RELEASE];
GO

CREATE VIEW [VW_DATATOOLS_RELEASE]
AS
	SELECT RELEASE_ID, IS_SUMMARY, INDENT_LEVEL, IS_DELETED, PROJECT_ID
	FROM TST_RELEASE
GO
IF OBJECT_ID ( 'VW_DATATOOLS_REQUIREMENT', 'V' ) IS NOT NULL 
    DROP VIEW [VW_DATATOOLS_REQUIREMENT];
GO

CREATE VIEW [VW_DATATOOLS_REQUIREMENT]
AS
	SELECT REQUIREMENT_ID, IS_SUMMARY, INDENT_LEVEL, IS_DELETED, PROJECT_ID
	FROM TST_REQUIREMENT
GO
IF OBJECT_ID ( 'VW_EXECUTION_STATUS_ACTIVE', 'V' ) IS NOT NULL 
    DROP VIEW [VW_EXECUTION_STATUS_ACTIVE];
GO
CREATE VIEW [VW_EXECUTION_STATUS_ACTIVE]
AS
SELECT EXECUTION_STATUS_ID, NAME
FROM TST_EXECUTION_STATUS
WHERE EXECUTION_STATUS_ID <> 4 AND IS_ACTIVE = 1
GO
IF OBJECT_ID ('VW_HISTORYCHANGE_LIST', 'V') IS NOT NULL 
    DROP VIEW [VW_HISTORYCHANGE_LIST];
GO
CREATE VIEW [dbo].[VW_HISTORYCHANGE_LIST]
AS
	SELECT 
		HC.*, HD.ARTIFACT_HISTORY_ID, HD.FIELD_NAME, HD.FIELD_CAPTION,
		HD.OLD_VALUE, HD.NEW_VALUE,
		HD.OLD_VALUE_INT, HD.NEW_VALUE_INT,
		HD.OLD_VALUE_DATE, HD.NEW_VALUE_DATE,HD.FIELD_ID,PR.NAME AS PROJECTNAME,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME,
		AT.NAME AS ARTIFACT_TYPE_NAME
		
	FROM [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC
	INNER JOIN [ValidationMasterAudit].dbo.TST_HISTORY_DETAIL AS HD ON HC.CHANGESET_ID = HD.CHANGESET_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
		INNER JOIN TST_PROJECT AS PR ON HC.PROJECT_ID = PR.PROJECT_ID
GO
IF OBJECT_ID ('VW_HISTORY_CHANGESET_NET_CHANGES', 'V') IS NOT NULL 
    DROP VIEW [VW_HISTORY_CHANGESET_NET_CHANGES];
GO

CREATE VIEW [dbo].[VW_HISTORY_CHANGESET_NET_CHANGES]
AS
	SELECT 
		MAX(HC.CHANGESET_ID) AS CHANGESET_ID,
		MAX(HC.USER_ID) AS USER_ID,
		HC.ARTIFACT_TYPE_ID,
		HC.ARTIFACT_ID,
		MAX(HC.CHANGE_DATE) AS CHANGE_DATE,
		HCT.CHANGETYPE_CANONICAL_ID AS CHANGETYPE_ID,
		HC.PROJECT_ID,
		MAX(HC.ARTIFACT_DESC) AS ARTIFACT_NAME,
		MAX(HT.CHANGE_NAME) AS CHANGETYPE_NAME,
		MAX((RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME)) AS USER_NAME,
		MAX(AT.NAME) AS ARTIFACT_TYPE_NAME,
		BL.BASELINE_ID AS BASELINE_ID,
		MAX(BL.NAME) AS BASELINE_NAME		
	FROM  [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC
		INNER JOIN (
			SELECT
				CHANGETYPE_ID,
				CHANGE_NAME,
				CHANGETYPE_CANONICAL_ID = CASE CHANGETYPE_ID
					WHEN 1 THEN 1	/* Modified -> Modified */
					WHEN 2 THEN 2	/* Deleted -> Deleted */
					WHEN 3 THEN 3	/* Added -> Added */
									/* Purged -> */
					WHEN 5 THEN 1	/* Rollback -> Modified */
					WHEN 6 THEN 6	/* Undelete -> Undeleted */
					WHEN 7 THEN 3	/* Imported -> Added */
									/* Exported -> */
					WHEN 9 THEN 2	/* Deleted_Parent -> Deleted */
					WHEN 10 THEN 3	/* Added_Parent -> Added */
									/* Purged_parent -> */
					WHEN 12 THEN 6	/* Undelete_Parent -> Undelete */
					WHEN 13 THEN 13	/* Assc_Add -> Assc_Add */
					WHEN 14 THEN 14	/* Assc_Del -> Assc_Del */
					WHEN 15 THEN 1	/* Assc_Mod -> Modified */
				END
			FROM TST_HISTORY_CHANGESET_TYPE
			) AS HCT ON HC.CHANGETYPE_ID = HCT.CHANGETYPE_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HCT.CHANGETYPE_CANONICAL_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
		INNER JOIN (
			SELECT 
				HC.CHANGESET_ID,
				MIN(BL.CHANGESET_ID) AS BASELINE_CHANGESET_ID
			FROM  [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC
			LEFT JOIN TST_PROJECT_BASELINE AS BL ON HC.CHANGESET_ID <= BL.CHANGESET_ID
			GROUP BY 
				HC.CHANGESET_ID) AS BLC ON HC.CHANGESET_ID = BLC.CHANGESET_ID
		INNER JOIN TST_PROJECT_BASELINE BL ON BL.CHANGESET_ID = BASELINE_CHANGESET_ID
	GROUP BY
		HC.ARTIFACT_TYPE_ID,
		HC.ARTIFACT_ID,
		BL.BASELINE_ID,
		HC.PROJECT_ID,
		HCT.CHANGETYPE_CANONICAL_ID
GO

IF OBJECT_ID ('VW_HISTORY_CHANGESET_NET_CHANGES_SQUASHED', 'V') IS NOT NULL 
    DROP VIEW [VW_HISTORY_CHANGESET_NET_CHANGES_SQUASHED];
GO

CREATE VIEW [dbo].[VW_HISTORY_CHANGESET_NET_CHANGES_SQUASHED]
AS
	SELECT
		BASELINE_ID,
		MAX(BASELINE_NAME) AS BASELINE_NAME,
		ARTIFACT_TYPE_ID,
		ARTIFACT_ID,
		MAX(CHANGESET_ID) AS CHANGESET_ID,
		MAX(USER_ID) AS USER_ID,
		MAX(CHANGE_DATE) AS CHANGE_DATE,
		PROJECT_ID,
		MAX(ARTIFACT_NAME) AS ARTIFACT_NAME,
		MAX(USER_NAME) AS USER_NAME,
		MAX(ARTIFACT_TYPE_NAME) AS ARTIFACT_TYPE_NAME,
		STUFF(
			(SELECT ',' + CHANGETYPE_NAME 
			 FROM VW_HISTORY_CHANGESET_NET_CHANGES INNR
				 WHERE
					INNR.ARTIFACT_TYPE_ID = OUTR.ARTIFACT_TYPE_ID AND
					INNR.ARTIFACT_ID = OUTR.ARTIFACT_ID AND
					INNR.BASELINE_ID = OUTR.BASELINE_ID
			 FOR XML PATH('')
		), 1,1,'') AS CHANGE_TYPES
	FROM VW_HISTORY_CHANGESET_NET_CHANGES OUTR
	GROUP BY
		ARTIFACT_TYPE_ID,
		ARTIFACT_ID,
		BASELINE_ID,
		PROJECT_ID
GO
IF OBJECT_ID ( 'VW_HISTORY_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_HISTORY_LIST];
GO

CREATE VIEW [dbo].[VW_HISTORY_LIST]
AS
	SELECT
		HD.ARTIFACT_HISTORY_ID, HD.FIELD_NAME, HD.FIELD_CAPTION,
		HD.OLD_VALUE, HD.NEW_VALUE,
		HD.OLD_VALUE_INT, HD.NEW_VALUE_INT,
		HD.OLD_VALUE_DATE, HD.NEW_VALUE_DATE,
		HD.CHANGESET_ID, HD.FIELD_ID, CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME, HC.CHANGETYPE_ID, AF.ARTIFACT_FIELD_TYPE_ID
	FROM  [ValidationMasterAudit].dbo.TST_HISTORY_DETAIL AS HD
	INNER JOIN  [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC ON HD.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	LEFT JOIN TST_ARTIFACT_FIELD AS AF ON HD.FIELD_ID = AF.ARTIFACT_FIELD_ID
	UNION ALL
	SELECT
		HP.HISTORY_POSITION_ID AS ARTIFACT_HISTORY_ID,
		'_Position' AS FIELD_NAME,
		(ART.NAME + ' [' + ART.PREFIX + ':' + CAST(HP.CHILD_ARTIFACT_ID AS NVARCHAR) + '] Position') AS FIELD_CAPTION,
		CAST (HP.OLD_POSITION AS NVARCHAR) AS OLD_VALUE,
		CAST (HP.NEW_POSITION AS NVARCHAR) AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HP.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_POSITION AS HP
	INNER JOIN [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC ON HP.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE ART ON HP.CHILD_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
	UNION ALL
	SELECT
		HA.ASSOCIATION_HISTORY_ID AS ARTIFACT_HISTORY_ID,
		'_Association' AS FIELD_NAME,
		HT.CHANGE_NAME AS FIELD_CAPTION,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Remove*/14 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS OLD_VALUE,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Add*/13 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HA.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_ASSOCIATION AS HA
	INNER JOIN [ValidationMasterAudit].dbo.TST_HISTORY_CHANGESET AS HC ON HA.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE SART ON HA.SOURCE_ARTIFACT_TYPE_ID = SART.ARTIFACT_TYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE DART ON HA.DEST_ARTIFACT_TYPE_ID = DART.ARTIFACT_TYPE_ID
GO
IF OBJECT_ID ( 'VW_INCIDENT_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_INCIDENT_LIST];
GO
CREATE VIEW [VW_INCIDENT_LIST]
AS
	SELECT	INC.*,
			PRI.NAME AS PRIORITY_NAME,
			PRI.COLOR AS PRIORITY_COLOR,
			SEV.NAME AS SEVERITY_NAME,
			SEV.COLOR AS SEVERITY_COLOR,
			IST.NAME AS INCIDENT_STATUS_NAME,
			IST.IS_OPEN_STATUS AS INCIDENT_STATUS_IS_OPEN_STATUS,
			ITP.NAME AS INCIDENT_TYPE_NAME,
			(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS OPENER_NAME,
			(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
            REL1.VERSION_NUMBER AS DETECTED_RELEASE_VERSION_NUMBER,
            REL2.VERSION_NUMBER AS RESOLVED_RELEASE_VERSION_NUMBER,
            REL3.VERSION_NUMBER AS VERIFIED_RELEASE_VERSION_NUMBER,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID,
			ITP.IS_ISSUE AS INCIDENT_TYPE_IS_ISSUE,
			BLD.NAME AS RESOLVED_BUILD_NAME,		
			ITP.IS_RISK AS INCIDENT_TYPE_IS_RISK,
			PRJ.NAME AS PROJECT_NAME,
            ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
            ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
            ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM	TST_INCIDENT INC LEFT JOIN TST_INCIDENT_PRIORITY PRI
	ON		INC.PRIORITY_ID = PRI.PRIORITY_ID INNER JOIN TST_INCIDENT_TYPE ITP
    ON		INC.INCIDENT_TYPE_ID = ITP.INCIDENT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR2
    ON		INC.OWNER_ID = USR2.USER_ID INNER JOIN TST_INCIDENT_STATUS IST
    ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID INNER JOIN TST_USER_PROFILE USR1
    ON		INC.OPENER_ID = USR1.USER_ID INNER JOIN TST_PROJECT PRJ
	ON		INC.PROJECT_ID = PRJ.PROJECT_ID LEFT JOIN TST_INCIDENT_SEVERITY SEV
    ON		INC.SEVERITY_ID = SEV.SEVERITY_ID LEFT JOIN TST_RELEASE REL1
    ON		INC.DETECTED_RELEASE_ID = REL1.RELEASE_ID LEFT JOIN TST_RELEASE REL2
    ON		INC.RESOLVED_RELEASE_ID = REL2.RELEASE_ID LEFT JOIN TST_RELEASE REL3
    ON		INC.VERIFIED_RELEASE_ID = REL3.RELEASE_ID LEFT JOIN TST_BUILD BLD
    ON		INC.RESOLVED_BUILD_ID = BLD.BUILD_ID LEFT JOIN (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 3) ACP
    ON		INC.INCIDENT_ID = ACP.ARTIFACT_ID
GO
IF OBJECT_ID ( 'VW_OLD_HISTORY_CHANGESET_NET_CHANGES', 'V' ) IS NOT NULL 
    DROP VIEW [VW_OLD_HISTORY_CHANGESET_NET_CHANGES];
GO
CREATE VIEW [dbo].[VW_OLD_HISTORY_CHANGESET_NET_CHANGES]
AS
	SELECT 
		MAX(HC.CHANGESET_ID) AS CHANGESET_ID,
		MAX(HC.USER_ID) AS USER_ID,
		HC.ARTIFACT_TYPE_ID,
		HC.ARTIFACT_ID,
		MAX(HC.CHANGE_DATE) AS CHANGE_DATE,
		HCT.CHANGETYPE_CANONICAL_ID AS CHANGETYPE_ID,
		HC.PROJECT_ID,
		MAX(HC.ARTIFACT_DESC) AS ARTIFACT_NAME,
		MAX(HT.CHANGE_NAME) AS CHANGETYPE_NAME,
		MAX((RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME)) AS USER_NAME,
		MAX(AT.NAME) AS ARTIFACT_TYPE_NAME,
		BL.BASELINE_ID AS BASELINE_ID,
		MAX(BL.NAME) AS BASELINE_NAME		
	FROM TST_HISTORY_CHANGESET AS HC
		INNER JOIN (
			SELECT
				CHANGETYPE_ID,
				CHANGE_NAME,
				CHANGETYPE_CANONICAL_ID = CASE CHANGETYPE_ID
					WHEN 1 THEN 1	/* Modified -> Modified */
					WHEN 2 THEN 2	/* Deleted -> Deleted */
					WHEN 3 THEN 3	/* Added -> Added */
									/* Purged -> */
					WHEN 5 THEN 1	/* Rollback -> Modified */
					WHEN 6 THEN 6	/* Undelete -> Undeleted */
					WHEN 7 THEN 3	/* Imported -> Added */
									/* Exported -> */
					WHEN 9 THEN 2	/* Deleted_Parent -> Deleted */
					WHEN 10 THEN 3	/* Added_Parent -> Added */
									/* Purged_parent -> */
					WHEN 12 THEN 6	/* Undelete_Parent -> Undelete */
					WHEN 13 THEN 13	/* Assc_Add -> Assc_Add */
					WHEN 14 THEN 14	/* Assc_Del -> Assc_Del */
					WHEN 15 THEN 1	/* Assc_Mod -> Modified */
				END
			FROM TST_HISTORY_CHANGESET_TYPE
			) AS HCT ON HC.CHANGETYPE_ID = HCT.CHANGETYPE_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HCT.CHANGETYPE_CANONICAL_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
		INNER JOIN (
			SELECT 
				HC.CHANGESET_ID,
				MIN(BL.CHANGESET_ID) AS BASELINE_CHANGESET_ID
			FROM TST_HISTORY_CHANGESET AS HC
			LEFT JOIN TST_PROJECT_BASELINE AS BL ON HC.CHANGESET_ID <= BL.CHANGESET_ID
			GROUP BY 
				HC.CHANGESET_ID) AS BLC ON HC.CHANGESET_ID = BLC.CHANGESET_ID
		INNER JOIN TST_PROJECT_BASELINE BL ON BL.CHANGESET_ID = BASELINE_CHANGESET_ID
	GROUP BY
		HC.ARTIFACT_TYPE_ID,
		HC.ARTIFACT_ID,
		BL.BASELINE_ID,
		HC.PROJECT_ID,
		HCT.CHANGETYPE_CANONICAL_ID
GO

IF OBJECT_ID ( 'VW_OLD_HISTORY_CHANGE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_OLD_HISTORY_CHANGE_LIST];
GO

CREATE VIEW [dbo].[VW_OLD_HISTORY_CHANGE_LIST]
AS
	SELECT 
		HC.*,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME,
		AT.NAME AS ARTIFACT_TYPE_NAME
		
	FROM TST_HISTORY_CHANGESET AS HC
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
GO
IF OBJECT_ID ( 'VW_OLD_HISTORY_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_OLD_HISTORY_LIST];
GO
CREATE VIEW [dbo].[VW_OLD_HISTORY_LIST]
AS
	SELECT
		HD.ARTIFACT_HISTORY_ID, HD.FIELD_NAME, HD.FIELD_CAPTION,
		HD.OLD_VALUE, HD.NEW_VALUE,
		HD.OLD_VALUE_INT, HD.NEW_VALUE_INT,
		HD.OLD_VALUE_DATE, HD.NEW_VALUE_DATE,
		HD.CHANGESET_ID, HD.FIELD_ID, CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME, HC.CHANGETYPE_ID, AF.ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_DETAIL AS HD
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HD.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	LEFT JOIN TST_ARTIFACT_FIELD AS AF ON HD.FIELD_ID = AF.ARTIFACT_FIELD_ID
	UNION ALL
	SELECT
		HP.HISTORY_POSITION_ID AS ARTIFACT_HISTORY_ID,
		'_Position' AS FIELD_NAME,
		(ART.NAME + ' [' + ART.PREFIX + ':' + CAST(HP.CHILD_ARTIFACT_ID AS NVARCHAR) + '] Position') AS FIELD_CAPTION,
		CAST (HP.OLD_POSITION AS NVARCHAR) AS OLD_VALUE,
		CAST (HP.NEW_POSITION AS NVARCHAR) AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HP.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_POSITION AS HP
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HP.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE ART ON HP.CHILD_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
	UNION ALL
	SELECT
		HA.ASSOCIATION_HISTORY_ID AS ARTIFACT_HISTORY_ID,
		'_Association' AS FIELD_NAME,
		HT.CHANGE_NAME AS FIELD_CAPTION,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Remove*/14 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS OLD_VALUE,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Add*/13 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HA.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_ASSOCIATION AS HA
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HA.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE SART ON HA.SOURCE_ARTIFACT_TYPE_ID = SART.ARTIFACT_TYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE DART ON HA.DEST_ARTIFACT_TYPE_ID = DART.ARTIFACT_TYPE_ID
GOIF OBJECT_ID ( 'VW_PROJECT_ATTACHMENT_FOLDER_HIERARCHY', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PROJECT_ATTACHMENT_FOLDER_HIERARCHY];
GO
CREATE VIEW [VW_PROJECT_ATTACHMENT_FOLDER_HIERARCHY]
AS
SELECT PROJECT_ATTACHMENT_FOLDER_ID AS PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, NAME, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
FROM TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY
GO
/* ---------------------------------------------------------------------- */
/* Add View "VW_PROJECT_ATTACHMENT_LIST"                                */
/* ---------------------------------------------------------------------- */

IF OBJECT_ID ( 'VW_PROJECT_ATTACHMENT_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PROJECT_ATTACHMENT_LIST];
GO

CREATE VIEW [dbo].[VW_PROJECT_ATTACHMENT_LIST]
AS
SELECT        PAT.ATTACHMENT_ID, PAT.PROJECT_ID, PAT.DOCUMENT_TYPE_ID, PAT.PROJECT_ATTACHMENT_FOLDER_ID, PAT.IS_KEY_DOCUMENT, ATC.ATTACHMENT_TYPE_ID, ATC.AUTHOR_ID, ATC.EDITOR_ID, ATC.FILENAME, 
                         ATC.DESCRIPTION, ATC.UPLOAD_DATE, ATC.EDITED_DATE, ATC.SIZE, ATC.CURRENT_VERSION, TGS.TAGS, ATC.CONCURRENCY_DATE, ATC.DOCUMENT_STATUS_ID, 
                         DCS.IS_OPEN_STATUS AS DOCUMENT_STATUS_IS_OPEN_STATUS, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE, PRJ.PROJECT_GROUP_ID, PRJ.NAME AS PROJECT_NAME, 
                         RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL, '')) + ' ' + USR1.LAST_NAME AS AUTHOR_NAME, RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL, '')) 
                         + ' ' + USR2.LAST_NAME AS EDITOR_NAME, ATT.NAME AS ATTACHMENT_TYPE_NAME, PAE.NAME AS DOCUMENT_TYPE_NAME, DCS.NAME AS DOCUMENT_STATUS_NAME, ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, 
                         ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10, ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, 
                         ACP.CUST_19, ACP.CUST_20, ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30, ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, 
                         ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40, ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, 
                         ACP.CUST_49, ACP.CUST_50, ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60, ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, 
                         ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70, ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, 
                         ACP.CUST_79, ACP.CUST_80, ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90, ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, 
                         ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99, ATC.IS_DELETED
FROM            dbo.TST_PROJECT_ATTACHMENT AS PAT INNER JOIN
                         dbo.TST_ATTACHMENT AS ATC ON PAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID INNER JOIN
                         dbo.TST_USER_PROFILE AS USR1 ON ATC.AUTHOR_ID = USR1.USER_ID INNER JOIN
                         dbo.TST_USER_PROFILE AS USR2 ON ATC.EDITOR_ID = USR2.USER_ID INNER JOIN
                         dbo.TST_ATTACHMENT_TYPE AS ATT ON ATC.ATTACHMENT_TYPE_ID = ATT.ATTACHMENT_TYPE_ID INNER JOIN
                         dbo.TST_DOCUMENT_TYPE AS PAE ON PAT.DOCUMENT_TYPE_ID = PAE.DOCUMENT_TYPE_ID INNER JOIN
                         dbo.TST_DOCUMENT_STATUS AS DCS ON ATC.DOCUMENT_STATUS_ID = DCS.DOCUMENT_STATUS_ID INNER JOIN
                         dbo.TST_PROJECT_ATTACHMENT_FOLDER AS PAF ON PAT.PROJECT_ATTACHMENT_FOLDER_ID = PAF.PROJECT_ATTACHMENT_FOLDER_ID INNER JOIN
                         dbo.TST_PROJECT AS PRJ ON PAT.PROJECT_ID = PRJ.PROJECT_ID LEFT OUTER JOIN
                             (SELECT        ARTIFACT_ID, ARTIFACT_TYPE_ID, TAGS, PROJECT_ID
                               FROM            dbo.TST_ARTIFACT_TAGS
                               WHERE        (ARTIFACT_TYPE_ID = 13)) AS TGS ON ATC.ATTACHMENT_ID = TGS.ARTIFACT_ID LEFT OUTER JOIN
                             (SELECT        ARTIFACT_ID, ARTIFACT_TYPE_ID, PROJECT_ID, CUST_01, CUST_02, CUST_03, CUST_04, CUST_05, CUST_06, CUST_07, CUST_08, CUST_09, CUST_10, CUST_11, CUST_12, CUST_13, CUST_14, CUST_15, 
                                                         CUST_16, CUST_17, CUST_18, CUST_19, CUST_20, CUST_21, CUST_22, CUST_23, CUST_24, CUST_25, CUST_26, CUST_27, CUST_28, CUST_29, CUST_30, CUST_31, CUST_32, CUST_33, CUST_34, CUST_35, 
                                                         CUST_36, CUST_37, CUST_38, CUST_39, CUST_40, CUST_41, CUST_42, CUST_43, CUST_44, CUST_45, CUST_46, CUST_47, CUST_48, CUST_49, CUST_50, CUST_51, CUST_52, CUST_53, CUST_54, CUST_55, 
                                                         CUST_56, CUST_57, CUST_58, CUST_59, CUST_60, CUST_61, CUST_62, CUST_63, CUST_64, CUST_65, CUST_66, CUST_67, CUST_68, CUST_69, CUST_70, CUST_71, CUST_72, CUST_73, CUST_74, CUST_75, 
                                                         CUST_76, CUST_77, CUST_78, CUST_79, CUST_80, CUST_81, CUST_82, CUST_83, CUST_84, CUST_85, CUST_86, CUST_87, CUST_88, CUST_89, CUST_90, CUST_91, CUST_92, CUST_93, CUST_94, CUST_95, 
                                                         CUST_96, CUST_97, CUST_98, CUST_99
                               FROM            dbo.TST_ARTIFACT_CUSTOM_PROPERTY
                               WHERE        (ARTIFACT_TYPE_ID = 13)) AS ACP ON ATC.ATTACHMENT_ID = ACP.ARTIFACT_ID
GOIF OBJECT_ID ( 'VW_PROJECT_GROUP_USER', 'V' ) IS NOT NULL 
    DROP VIEW VW_PROJECT_GROUP_USER;
GO

CREATE VIEW VW_PROJECT_GROUP_USER
AS
    SELECT	PGU.PROJECT_GROUP_ID, PGU.USER_ID, PGU.PROJECT_GROUP_ROLE_ID, PRG.NAME AS PROJECT_GROUP_NAME,
			PGR.NAME AS PROJECT_GROUP_ROLE_NAME, PRG.IS_ACTIVE, USR.USER_NAME,
			(RTRIM(UPL.FIRST_NAME + ' ' + ISNULL(UPL.MIDDLE_INITIAL,'')) + ' ' + UPL.LAST_NAME) AS FULL_NAME,
			PRG.PORTFOLIO_ID
    FROM	TST_PROJECT_GROUP PRG INNER JOIN TST_PROJECT_GROUP_USER PGU
    ON		PRG.PROJECT_GROUP_ID = PGU.PROJECT_GROUP_ID INNER JOIN TST_USER USR
    ON		PGU.USER_ID = USR.USER_ID INNER JOIN TST_PROJECT_GROUP_ROLE PGR
    ON		PGU.PROJECT_GROUP_ROLE_ID = PGR.PROJECT_GROUP_ROLE_ID INNER JOIN TST_USER_PROFILE UPL
    ON		USR.USER_ID = UPL.USER_ID
GO
IF OBJECT_ID ( 'VW_PROJECT_LIST', 'V' ) IS NOT NULL 
    DROP VIEW VW_PROJECT_LIST;
GO

CREATE VIEW VW_PROJECT_LIST
AS
SELECT
	PRJ.*, PRG.NAME AS PROJECT_GROUP_NAME, PRT.NAME AS PROJECT_TEMPLATE_NAME
FROM TST_PROJECT AS PRJ
	INNER JOIN TST_PROJECT_GROUP PRG ON PRJ.PROJECT_GROUP_ID = PRG.PROJECT_GROUP_ID
	INNER JOIN TST_PROJECT_TEMPLATE PRT ON PRJ.PROJECT_TEMPLATE_ID = PRT.PROJECT_TEMPLATE_ID
GO
IF OBJECT_ID ( 'VW_NOTIFICATION_USER_SUBSCRIPTION_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_NOTIFICATION_USER_SUBSCRIPTION_LIST];
GO
CREATE VIEW [VW_NOTIFICATION_USER_SUBSCRIPTION_LIST]
AS
SELECT	NUS.*, REQ.NAME AS ARTIFACT_NAME, REQ.DESCRIPTION AS ARTIFACT_DESCRIPTION, REQ.LAST_UPDATE_DATE, REQ.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN TST_REQUIREMENT REQ
ON		NUS.ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_PROJECT PRJ
ON		REQ.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 1
UNION
SELECT	NUS.*, TST.NAME AS ARTIFACT_NAME, TST.DESCRIPTION AS ARTIFACT_DESCRIPTION, TST.LAST_UPDATE_DATE, TST.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN TST_TEST_CASE TST
ON		NUS.ARTIFACT_ID = TST.TEST_CASE_ID INNER JOIN TST_PROJECT PRJ
ON		TST.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 2
UNION
SELECT	NUS.*, TSE.NAME AS ARTIFACT_NAME, TSE.DESCRIPTION AS ARTIFACT_DESCRIPTION, TSE.LAST_UPDATE_DATE, TSE.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN TST_TEST_SET TSE
ON		NUS.ARTIFACT_ID = TSE.TEST_SET_ID INNER JOIN TST_PROJECT PRJ
ON		TSE.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 8
UNION
SELECT	NUS.*, INC.NAME AS ARTIFACT_NAME, INC.DESCRIPTION AS ARTIFACT_DESCRIPTION, INC.LAST_UPDATE_DATE, INC.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN TST_INCIDENT INC
ON		NUS.ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_PROJECT PRJ
ON		INC.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 3
UNION
SELECT	NUS.*, TSK.NAME AS ARTIFACT_NAME, TSK.DESCRIPTION AS ARTIFACT_DESCRIPTION, TSK.LAST_UPDATE_DATE, TSK.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN TST_TASK TSK
ON		NUS.ARTIFACT_ID = TSK.TASK_ID INNER JOIN TST_PROJECT PRJ
ON		TSK.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 6
UNION
SELECT	NUS.*, DOC.FILENAME AS ARTIFACT_NAME, DOC.DESCRIPTION AS ARTIFACT_DESCRIPTION, DOC.EDITED_DATE AS LAST_UPDATE_DATE, DOC.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
FROM	TST_NOTIFICATION_USER_SUBSCRIPTION NUS INNER JOIN VW_PROJECT_ATTACHMENT_LIST DOC
ON		NUS.ARTIFACT_ID = DOC.ATTACHMENT_ID INNER JOIN TST_PROJECT PRJ
ON		DOC.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_ARTIFACT_TYPE ART
ON		NUS.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
WHERE	PRJ.IS_ACTIVE = 1 AND NUS.ARTIFACT_TYPE_ID = 13
GO
IF OBJECT_ID ( 'VW_PROJECT_RESOURCES', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PROJECT_RESOURCES];
GO
CREATE VIEW [VW_PROJECT_RESOURCES]
AS
SELECT	PRU.PROJECT_ID, PRU.USER_ID, MIN(USP.FIRST_NAME + ' ' + USP.LAST_NAME) AS FULL_NAME,
		MIN(PRU.PROJECT_ROLE_ID) AS PROJECT_ROLE_ID, MIN(PRR.NAME) AS PROJECT_ROLE_NAME,
		SUM(ISNULL(RES.INCIDENT_EFFORT,0)) AS INCIDENT_EFFORT, SUM(ISNULL(RES.REQ_TASK_EFFORT,0)) AS REQ_TASK_EFFORT,
		SUM(ISNULL(RES.INCIDENT_EFFORT_OPEN,0)) AS INCIDENT_EFFORT_OPEN, SUM(ISNULL(RES.REQ_TASK_EFFORT_OPEN,0)) AS REQ_TASK_EFFORT_OPEN
FROM (
	--We want the totals for open and all incident
	SELECT  INC.PROJECT_ID, INC.OWNER_ID AS USER_ID, 
			INC.PROJECTED_EFFORT AS INCIDENT_EFFORT, 0 AS REQ_TASK_EFFORT,
			(CASE WHEN IST.IS_OPEN_STATUS = 1 THEN INC.PROJECTED_EFFORT ELSE 0 END) AS INCIDENT_EFFORT_OPEN,
			0 AS REQ_TASK_EFFORT_OPEN
	FROM	TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
	ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
	WHERE	INC.IS_DELETED = 0
	UNION ALL
	--We want the totals for incomplete and all tasks (ignore deferred)
	SELECT  TSK.PROJECT_ID, TSK.OWNER_ID AS USER_ID,
			0 AS INCIDENT_EFFORT, TSK.PROJECTED_EFFORT AS REQ_TASK_EFFORT,
			0 AS INCIDENT_EFFORT_OPEN,
			(CASE WHEN TSK.TASK_STATUS_ID <> 3 THEN TSK.PROJECTED_EFFORT ELSE 0 END) AS REQ_TASK_EFFORT_OPEN
	FROM	TST_TASK TSK
	WHERE	TSK.IS_DELETED = 0 AND TSK.TASK_STATUS_ID <> 5
	UNION ALL
	--We need to get the totals for in-progress and completed requirements (ignore rejected,under review, requested)
	--Ignore any requirements that do have a task effort set
	SELECT  REQ.PROJECT_ID, REQ.OWNER_ID AS USER_ID,
			0 AS INCIDENT_EFFORT, REQ.ESTIMATED_EFFORT AS REQ_TASK_EFFORT,
			0 AS INCIDENT_EFFORT_OPEN,
			(CASE WHEN REQ.REQUIREMENT_STATUS_ID IN (2,3,5) THEN REQ.ESTIMATED_EFFORT ELSE 0 END) AS REQ_TASK_EFFORT_OPEN
	FROM	TST_REQUIREMENT REQ
	WHERE	REQ.IS_DELETED = 0 AND REQ.REQUIREMENT_STATUS_ID IN (2,3,4,5,8,9,10) AND
			(REQ.TASK_PROJECTED_EFFORT IS NULL OR REQ.TASK_PROJECTED_EFFORT = 0)
	) AS RES RIGHT JOIN TST_PROJECT_USER PRU
ON	RES.PROJECT_ID = PRU.PROJECT_ID AND RES.USER_ID = PRU.USER_ID INNER JOIN TST_USER USR
ON	PRU.USER_ID = USR.USER_ID INNER JOIN TST_USER_PROFILE USP
ON	USR.USER_ID = USP.USER_ID INNER JOIN TST_PROJECT_ROLE PRR
ON	PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
WHERE USR.IS_ACTIVE = 1
GROUP BY PRU.USER_ID, PRU.PROJECT_ID
GO
--This view is now only used to display the shape of the data, the query is executed from
--the TASK_RETRIEVE_GROUP_SUMMARY_BY_PROJECT stored procedure instead
IF OBJECT_ID ( 'VW_PROJECT_TASK_PROGRESS', 'V' ) IS NOT NULL 
    DROP VIEW VW_PROJECT_TASK_PROGRESS;
GO
CREATE VIEW VW_PROJECT_TASK_PROGRESS
AS
SELECT        
         PRJ2.PROJECT_GROUP_ID, PRJ2.PROJECT_ID, PRJ2.NAME AS PROJECT_NAME, PRJ2.DESCRIPTION AS PROJECT_DESCRIPTION, ISNULL(PTP.TASK_COUNT, 0) AS TASK_COUNT, PTP.RELEASE_ID, 
                         PTP.TASK_PERCENT_ON_TIME, PTP.TASK_PERCENT_LATE_FINISH, PTP.TASK_PERCENT_NOT_START, PTP.TASK_PERCENT_LATE_START, PTP.TASK_ESTIMATED_EFFORT, PTP.TASK_ACTUAL_EFFORT, 
                         PTP.TASK_REMAINING_EFFORT, PTP.TASK_PROJECTED_EFFORT
FROM            (SELECT        PRJ.PROJECT_ID, REL.RELEASE_ID, SUM(REL.TASK_COUNT) AS TASK_COUNT, ISNULL(AVG(REL.TASK_PERCENT_ON_TIME), 0) AS TASK_PERCENT_ON_TIME, ISNULL(AVG(REL.TASK_PERCENT_LATE_FINISH), 0) 
                                                    AS TASK_PERCENT_LATE_FINISH, ISNULL(AVG(REL.TASK_PERCENT_NOT_START), 0) AS TASK_PERCENT_NOT_START, ISNULL(AVG(REL.TASK_PERCENT_LATE_START), 0) AS TASK_PERCENT_LATE_START, 
                                                    ISNULL(SUM(REL.TASK_ESTIMATED_EFFORT), 0) AS TASK_ESTIMATED_EFFORT, ISNULL(SUM(REL.TASK_ACTUAL_EFFORT), 0) AS TASK_ACTUAL_EFFORT, ISNULL(SUM(REL.TASK_REMAINING_EFFORT), 0) 
                                                    AS TASK_REMAINING_EFFORT, ISNULL(SUM(REL.TASK_PROJECTED_EFFORT), 0) AS TASK_PROJECTED_EFFORT
                          FROM            dbo.TST_RELEASE AS REL INNER JOIN
                                                    dbo.TST_PROJECT AS PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
                          WHERE        (REL.RELEASE_TYPE_ID = 1) AND (REL.TASK_COUNT > 0) AND (REL.IS_DELETED = 0) AND (PRJ.IS_ACTIVE = 1) OR
                                                    (REL.TASK_COUNT > 0) AND (REL.IS_DELETED = 0) AND (PRJ.IS_ACTIVE = 1) AND (LEN(REL.INDENT_LEVEL) = 3)
                          GROUP BY PRJ.PROJECT_ID, REL.RELEASE_ID) AS PTP RIGHT OUTER JOIN
                         dbo.TST_PROJECT AS PRJ2 ON PRJ2.PROJECT_ID = PTP.PROJECT_ID
WHERE        (PRJ2.IS_ACTIVE = 1)
GO
IF OBJECT_ID ( 'VW_PROJECT_USER', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PROJECT_USER];
GO
CREATE VIEW [VW_PROJECT_USER]
AS
SELECT	PRU.USER_ID, PRU.PROJECT_ID, PRU.PROJECT_ROLE_ID, PRJ.NAME AS PROJECT_NAME,
		(RTRIM(UPL.FIRST_NAME + ' ' + ISNULL(UPL.MIDDLE_INITIAL,'')) + ' ' + UPL.LAST_NAME) AS FULL_NAME,
		USR.USER_NAME, PRR.NAME AS PROJECT_ROLE_NAME,
		UPL.FIRST_NAME, UPL.MIDDLE_INITIAL, UPL.LAST_NAME, USR.LDAP_DN, USR.EMAIL_ADDRESS, PRR.IS_ADMIN, USR.IS_ACTIVE, PRR.IS_TEMPLATE_ADMIN,
		PRJ.PROJECT_TEMPLATE_ID
FROM	TST_PROJECT PRJ INNER JOIN TST_PROJECT_USER PRU
ON		PRJ.PROJECT_ID = PRU.PROJECT_ID INNER JOIN TST_USER USR
ON		PRU.USER_ID = USR.USER_ID INNER JOIN TST_PROJECT_ROLE PRR
ON		PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID INNER JOIN TST_USER_PROFILE UPL
ON		USR.USER_ID = UPL.USER_ID
WHERE	PRJ.IS_ACTIVE = 1
UNION ALL
SELECT	PGU.USER_ID, PRJ.PROJECT_ID, PRR.PROJECT_ROLE_ID, PRJ.NAME AS PROJECT_NAME,
		(RTRIM(UPL.FIRST_NAME + ' ' + ISNULL(UPL.MIDDLE_INITIAL,'')) + ' ' + UPL.LAST_NAME) AS FULL_NAME,
		USR.USER_NAME, PRR.NAME AS PROJECT_ROLE_NAME,
		UPL.FIRST_NAME, UPL.MIDDLE_INITIAL, UPL.LAST_NAME, USR.LDAP_DN, USR.EMAIL_ADDRESS, CAST (0 AS BIT) AS IS_ADMIN, USR.IS_ACTIVE, CAST (0 AS BIT) AS IS_TEMPLATE_ADMIN,
		PRJ.PROJECT_TEMPLATE_ID
FROM	TST_PROJECT PRJ INNER JOIN TST_PROJECT_GROUP_USER PGU
ON		PRJ.PROJECT_GROUP_ID = PGU.PROJECT_GROUP_ID INNER JOIN TST_USER USR
ON		PGU.USER_ID = USR.USER_ID INNER JOIN TST_PROJECT_ROLE PRR
ON		PRR.PROJECT_ROLE_ID = 5 INNER JOIN TST_USER_PROFILE UPL
ON		USR.USER_ID = UPL.USER_ID
WHERE	PRJ.IS_ACTIVE = 1
AND		PGU.USER_ID NOT IN (SELECT USER_ID FROM TST_PROJECT_USER PRU2 WHERE PRU2.PROJECT_ID = PRJ.PROJECT_ID)
GO

IF OBJECT_ID ( '[VW_PROJECT_USERS]', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PROJECT_USERS];
GO
CREATE VIEW [VW_PROJECT_USERS]
AS
SELECT	PRU.USER_ID, PRJ.PROJECT_ID, PRJ.PROJECT_GROUP_ID, PRJ.NAME, PRJ.DESCRIPTION, PRJ.WEBSITE, PRJ.CREATION_DATE, PRJ.IS_ACTIVE, 
	PRJ.WORKING_HOURS, PRJ.WORKING_DAYS, PRJ.NON_WORKING_HOURS, PRJ.IS_TIME_TRACK_INCIDENTS, 	PRJ.IS_TIME_TRACK_TASKS, PRJ.IS_EFFORT_INCIDENTS, PRJ.IS_EFFORT_TASKS,
	PRJ.IS_TASKS_AUTO_CREATE, PRJ.REQ_DEFAULT_ESTIMATE, PRJ.REQ_POINT_EFFORT, PRJ.TASK_DEFAULT_EFFORT,
	PRG.NAME AS PROJECT_GROUP_NAME, PRG.PORTFOLIO_ID, PRT.NAME AS PORTFOLIO_NAME, PRJ.PROJECT_TEMPLATE_ID
FROM	TST_PROJECT PRJ INNER JOIN TST_PROJECT_USER PRU
ON		PRJ.PROJECT_ID = PRU.PROJECT_ID INNER JOIN TST_PROJECT_GROUP PRG
ON		PRJ.PROJECT_GROUP_ID = PRG.PROJECT_GROUP_ID LEFT JOIN TST_PORTFOLIO PRT
ON		PRG.PORTFOLIO_ID = PRT.PORTFOLIO_ID
WHERE	PRJ.IS_ACTIVE = 1
UNION ALL
SELECT	PGU.USER_ID, PRJ.PROJECT_ID, PRJ.PROJECT_GROUP_ID, PRJ.NAME, PRJ.DESCRIPTION, PRJ.WEBSITE, PRJ.CREATION_DATE, PRJ.IS_ACTIVE, 
	PRJ.WORKING_HOURS, PRJ.WORKING_DAYS, PRJ.NON_WORKING_HOURS, PRJ.IS_TIME_TRACK_INCIDENTS, 	PRJ.IS_TIME_TRACK_TASKS, PRJ.IS_EFFORT_INCIDENTS, PRJ.IS_EFFORT_TASKS,
	PRJ.IS_TASKS_AUTO_CREATE, PRJ.REQ_DEFAULT_ESTIMATE, PRJ.REQ_POINT_EFFORT, PRJ.TASK_DEFAULT_EFFORT,
	PRG.NAME AS PROJECT_GROUP_NAME, PRG.PORTFOLIO_ID, PRT.NAME AS PORTFOLIO_NAME, PRJ.PROJECT_TEMPLATE_ID
FROM	TST_PROJECT PRJ INNER JOIN TST_PROJECT_GROUP PRG
ON		PRJ.PROJECT_GROUP_ID = PRG.PROJECT_GROUP_ID INNER JOIN TST_PROJECT_GROUP_USER PGU
ON		PRG.PROJECT_GROUP_ID = PGU.PROJECT_GROUP_ID LEFT JOIN TST_PORTFOLIO PRT
ON		PRG.PORTFOLIO_ID = PRT.PORTFOLIO_ID
WHERE	PRJ.IS_ACTIVE = 1 AND PGU.USER_ID NOT IN (SELECT USER_ID FROM TST_PROJECT_USER PRU2 WHERE PRU2.PROJECT_ID = PRJ.PROJECT_ID)
GO
IF OBJECT_ID ( 'VW_PULL_REQUEST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_PULL_REQUEST];
GO
CREATE VIEW [VW_PULL_REQUEST]
AS
	SELECT	
	TSK.TASK_ID,
	TSK.PROJECT_ID,
	TSK.NAME,
	TSK.DESCRIPTION,
	TSK.TASK_FOLDER_ID,
	TSK.TASK_STATUS_ID,
	TSK.TASK_TYPE_ID,
	TSK.TASK_PRIORITY_ID,
	TSK.CREATOR_ID,
	TSK.OWNER_ID,
	TSK.RELEASE_ID,
	TSK.CREATION_DATE,
	TSK.LAST_UPDATE_DATE,
	TSK.CONCURRENCY_DATE,
	TSK.IS_ATTACHMENTS,
	TSK.IS_DELETED,	
	TKS.NAME AS TASK_STATUS_NAME, 
	(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
	(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
	TKP.NAME AS TASK_PRIORITY_NAME,
	TKP.COLOR AS TASK_PRIORITY_COLOR,
	PRJ.NAME AS PROJECT_NAME, 
	PRJ.PROJECT_TEMPLATE_ID,
	REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
	TKT.NAME AS TASK_TYPE_NAME,
	VCBS.BRANCH_ID AS SOURCE_BRANCH_ID,
	VCBS.NAME AS SOURCE_BRANCH_NAME,
	VCBD.BRANCH_ID AS DEST_BRANCH_ID,
	VCBD.NAME AS DEST_BRANCH_NAME
    FROM TST_TASK AS TSK
		INNER JOIN TST_TASK_STATUS AS TKS ON TSK.TASK_STATUS_ID = TKS.TASK_STATUS_ID
		INNER JOIN TST_TASK_TYPE AS TKT ON TSK.TASK_TYPE_ID = TKT.TASK_TYPE_ID
		LEFT JOIN TST_TASK_PRIORITY AS TKP ON TSK.TASK_PRIORITY_ID = TKP.TASK_PRIORITY_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSK.OWNER_ID = USR2.USER_ID 
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSK.CREATOR_ID = USR1.USER_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSK.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSK.RELEASE_ID = REL.RELEASE_ID
		LEFT JOIN TST_VERSION_CONTROL_PULL_REQUEST AS VCP ON TSK.TASK_ID = VCP.TASK_ID
		LEFT JOIN TST_VERSION_CONTROL_BRANCH AS VCBS ON VCP.SOURCE_BRANCH_ID = VCBS.BRANCH_ID
		LEFT JOIN TST_VERSION_CONTROL_BRANCH AS VCBD ON VCP.DEST_BRANCH_ID = VCBD.BRANCH_ID
	WHERE
		PRJ.IS_ACTIVE = 1 AND
		TSK.IS_DELETED = 0 AND
		TKT.IS_PULL_REQUEST = 1
GO
IF OBJECT_ID ( 'VW_RELEASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_RELEASE_LIST];
GO
CREATE VIEW [VW_RELEASE_LIST]
AS
	SELECT
		CAST (1 AS BIT) AS IS_EXPANDED,
		CAST (1 AS BIT) AS IS_VISIBLE,
		REL.*
	FROM VW_RELEASE_LIST_INTERNAL REL
GO
IF OBJECT_ID ( 'VW_RELEASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_RELEASE_LIST];
GO
CREATE VIEW [VW_RELEASE_LIST]
AS
	SELECT
		CAST (1 AS BIT) AS IS_EXPANDED,
		CAST (1 AS BIT) AS IS_VISIBLE,
		REL.*
	FROM VW_RELEASE_LIST_INTERNAL REL
GO
IF OBJECT_ID ( 'VW_REPORT_SAVED', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REPORT_SAVED];
GO
CREATE VIEW [VW_REPORT_SAVED]
AS
SELECT	RPS.REPORT_SAVED_ID, RPS.REPORT_ID, RPS.REPORT_FORMAT_ID, RPS.USER_ID, RPS.PROJECT_ID, RPS.NAME,
		RPS.PARAMETERS, RPS.IS_SHARED , RPS.CREATION_DATE, PRJ.NAME AS PROJECT_NAME, 
		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS USER_NAME
FROM	TST_REPORT_SAVED RPS INNER JOIN TST_USER_PROFILE USR
ON		RPS.USER_ID = USR.USER_ID LEFT JOIN TST_PROJECT PRJ
ON		RPS.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'VW_REQUIREMENT_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REQUIREMENT_INCIDENTS];
GO
CREATE VIEW [VW_REQUIREMENT_INCIDENTS]
AS
	SELECT
		RTC.REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_REQUIREMENT_TEST_CASE AS RTC
		INNER JOIN TST_TEST_RUN AS TRN ON RTC.TEST_CASE_ID = TRN.TEST_CASE_ID
		INNER JOIN TST_TEST_RUN_STEP AS TRS ON TRN.TEST_RUN_ID = TRS.TEST_RUN_ID
		INNER JOIN TST_TEST_RUN_STEP_INCIDENT AS TRI ON TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID
		INNER JOIN TST_INCIDENT AS INC ON TRI.INCIDENT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	UNION

	SELECT
		ARL.SOURCE_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
		INNER JOIN TST_INCIDENT AS INC ON ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 1 AND ARL.DEST_ARTIFACT_TYPE_ID = 3

	UNION
	
	SELECT
		ARL.DEST_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
	INNER JOIN TST_INCIDENT AS INC ON ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
	INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID 
	
	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 1

GO
IF OBJECT_ID ( 'VW_REQUIREMENT_LIST_INTERNAL', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REQUIREMENT_LIST_INTERNAL];
GO
--This is the same view as VW_REQUIREMENT_LIST except it doesn't have IS_VISIBLE and IS_EXPANDED
--as those are dynamically added by the stored procedures. This view is actually queries and the
--data is returned in the format expected by VW_REQUIREMENT_LIST
CREATE VIEW [VW_REQUIREMENT_LIST_INTERNAL]
AS
	SELECT
		REQ.REQUIREMENT_ID, REQ.AUTHOR_ID, REQ.OWNER_ID, REQ.RELEASE_ID, REQ.PROJECT_ID, REQ.REQUIREMENT_TYPE_ID,
		REQ.REQUIREMENT_STATUS_ID, REQ.COMPONENT_ID, REQ.IMPORTANCE_ID, REQ.NAME, REQ.CREATION_DATE, REQ.INDENT_LEVEL,
		REQ.DESCRIPTION, REQ.LAST_UPDATE_DATE, REQ.IS_SUMMARY, REQ.IS_ATTACHMENTS, REQ.COVERAGE_COUNT_TOTAL,
		REQ.COVERAGE_COUNT_PASSED, REQ.COVERAGE_COUNT_FAILED, REQ.COVERAGE_COUNT_CAUTION, REQ.COVERAGE_COUNT_BLOCKED,
		REQ.ESTIMATE_POINTS, REQ.ESTIMATED_EFFORT, REQ.TASK_COUNT, REQ.TASK_ESTIMATED_EFFORT, REQ.TASK_ACTUAL_EFFORT,
		REQ.TASK_PROJECTED_EFFORT, REQ.TASK_REMAINING_EFFORT, REQ.TASK_PERCENT_ON_TIME, REQ.TASK_PERCENT_LATE_FINISH,
		REQ.TASK_PERCENT_NOT_START, REQ.TASK_PERCENT_LATE_START, REQ.IS_DELETED, REQ.APPROVED_STATUS, REQ.CONCURRENCY_DATE, REQ.RANK, 
		RST.NAME AS REQUIREMENT_STATUS_NAME,
		(CASE WHEN REQ.IS_SUMMARY = 1 THEN 'Epic' ELSE RTP.NAME END) AS REQUIREMENT_TYPE_NAME,
		CMP.NAME AS COMPONENT_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		IMP.NAME AS IMPORTANCE_NAME,
		IMP.COLOR AS IMPORTANCE_COLOR,
		IMP.SCORE AS IMPORTANCE_SCORE,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
		RTP.IS_STEPS AS REQUIREMENT_TYPE_IS_STEPS,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM TST_REQUIREMENT AS REQ
		INNER JOIN TST_REQUIREMENT_STATUS AS RST ON REQ.REQUIREMENT_STATUS_ID = RST.REQUIREMENT_STATUS_ID
		INNER JOIN TST_REQUIREMENT_TYPE AS RTP ON REQ.REQUIREMENT_TYPE_ID = RTP.REQUIREMENT_TYPE_ID
		LEFT JOIN TST_IMPORTANCE AS IMP ON REQ.IMPORTANCE_ID = IMP.IMPORTANCE_ID
		LEFT JOIN (SELECT * FROM TST_COMPONENT WHERE IS_DELETED = 0) AS CMP ON REQ.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 1) AS ACP ON REQ.REQUIREMENT_ID = ACP.ARTIFACT_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON REQ.AUTHOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON REQ.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_RELEASE AS REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'VW_REQUIREMENT_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_REQUIREMENT_LIST];
GO
CREATE VIEW [VW_REQUIREMENT_LIST]
AS
	SELECT
		CAST (0 AS BIT) AS IS_EXPANDED,
		CAST (0 AS BIT) AS IS_VISIBLE,
		REQ.*
	FROM VW_REQUIREMENT_LIST_INTERNAL REQ
GO
IF OBJECT_ID ( 'VW_RISK_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_RISK_LIST];
GO
CREATE VIEW [VW_RISK_LIST]
AS
	SELECT	RSK.*,
			RPB.NAME AS RISK_PROBABILITY_NAME,
			RPB.COLOR AS RISK_PROBABILITY_COLOR,
			RPB.SCORE AS RISK_PROBABILITY_SCORE,
			RIM.NAME AS RISK_IMPACT_NAME,
			RIM.COLOR AS RISK_IMPACT_COLOR,
			RIM.SCORE AS RISK_IMPACT_SCORE,
			(RPB.SCORE * RIM.SCORE) AS RISK_EXPOSURE,
			IST.NAME AS RISK_STATUS_NAME,
			IST.IS_OPEN AS RISK_STATUS_IS_OPEN,
			ITP.NAME AS RISK_TYPE_NAME,
			(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
			(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
            REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
            REL.NAME AS RELEASE_NAME,
            CMP.NAME AS COMPONENT_NAME,
            PGL.NAME AS GOAL_NAME,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID AS PROJECT_PROJECT_GROUP_ID,
			PRJ.NAME AS PROJECT_NAME,
            ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
            ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
            ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_RISK RSK
		LEFT JOIN TST_RISK_PROBABILITY RPB ON RSK.RISK_PROBABILITY_ID = RPB.RISK_PROBABILITY_ID
		INNER JOIN TST_RISK_TYPE ITP ON RSK.RISK_TYPE_ID = ITP.RISK_TYPE_ID
		LEFT JOIN TST_USER_PROFILE USR2 ON RSK.OWNER_ID = USR2.USER_ID
		INNER JOIN TST_RISK_STATUS IST ON RSK.RISK_STATUS_ID = IST.RISK_STATUS_ID
		INNER JOIN TST_USER_PROFILE USR1 ON	RSK.CREATOR_ID = USR1.USER_ID
		INNER JOIN TST_PROJECT PRJ ON RSK.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RISK_IMPACT RIM ON RSK.RISK_IMPACT_ID = RIM.RISK_IMPACT_ID
		LEFT JOIN TST_RELEASE REL ON RSK.RELEASE_ID = REL.RELEASE_ID
		LEFT JOIN TST_COMPONENT CMP ON RSK.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN TST_PROJECT_GOAL PGL ON RSK.GOAL_ID = PGL.GOAL_ID
		LEFT JOIN (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 14) ACP ON RSK.RISK_ID = ACP.ARTIFACT_ID
GO
IF OBJECT_ID ( 'VW_TASK_FOLDER_HIERARCHY', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TASK_FOLDER_HIERARCHY];
GO
CREATE VIEW [VW_TASK_FOLDER_HIERARCHY]
AS
SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
FROM TST_TASK_FOLDER_HIERARCHY
GO
IF OBJECT_ID ( 'VW_TASK_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TASK_LIST];
GO
CREATE VIEW [VW_TASK_LIST]
AS
	SELECT	
	TSK.*,
	TKS.NAME AS TASK_STATUS_NAME, 
	(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
	(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
	TKP.NAME AS TASK_PRIORITY_NAME,
	TKP.COLOR AS TASK_PRIORITY_COLOR,
	PRJ.NAME AS PROJECT_NAME, 
	REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
	REQ.NAME AS REQUIREMENT_NAME,
	TKT.NAME AS TASK_TYPE_NAME,
	REQ.COMPONENT_ID,
	CMP.NAME AS COMPONENT_NAME,
	RSK.NAME AS RISK_NAME,
	TKT.IS_PULL_REQUEST,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_TASK AS TSK
		INNER JOIN TST_TASK_STATUS AS TKS ON TSK.TASK_STATUS_ID = TKS.TASK_STATUS_ID
		INNER JOIN TST_TASK_TYPE AS TKT ON TSK.TASK_TYPE_ID = TKT.TASK_TYPE_ID
		LEFT JOIN TST_TASK_PRIORITY AS TKP ON TSK.TASK_PRIORITY_ID = TKP.TASK_PRIORITY_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSK.OWNER_ID = USR2.USER_ID 
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSK.CREATOR_ID = USR1.USER_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSK.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSK.RELEASE_ID = REL.RELEASE_ID
		LEFT JOIN TST_REQUIREMENT AS REQ ON TSK.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		LEFT JOIN TST_RISK AS RSK ON TSK.RISK_ID = RSK.RISK_ID
		LEFT JOIN (SELECT * FROM TST_COMPONENT WHERE IS_DELETED = 0) AS CMP ON REQ.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 6
			) AS ACP ON TSK.TASK_ID = ACP.ARTIFACT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'VW_TEST_CASE_FOLDER_HIERARCHY', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TEST_CASE_FOLDER_HIERARCHY];
GO
CREATE VIEW [VW_TEST_CASE_FOLDER_HIERARCHY]
AS
SELECT TEST_CASE_FOLDER_ID AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
FROM TST_TEST_CASE_FOLDER_HIERARCHY
GO
IF OBJECT_ID ( 'VW_TESTCASE_FOLDER_RELEASE', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTCASE_FOLDER_RELEASE];
GO
CREATE VIEW [VW_TESTCASE_FOLDER_RELEASE]
AS
	SELECT
		TST.TEST_CASE_FOLDER_ID, TST.PARENT_TEST_CASE_FOLDER_ID, TST.PROJECT_ID,
		TST.NAME, TST.DESCRIPTION, TST.ESTIMATED_DURATION, TST.LAST_UPDATE_DATE,
		RTC.EXECUTION_DATE, RTC.ACTUAL_DURATION, RTC.RELEASE_ID,
		RTC.COUNT_PASSED,
		RTC.COUNT_FAILED,
		RTC.COUNT_BLOCKED,
		RTC.COUNT_CAUTION,
		RTC.COUNT_NOT_RUN,
		RTC.COUNT_NOT_APPLICABLE
	FROM
		TST_TEST_CASE_FOLDER TST
		INNER JOIN TST_RELEASE_TEST_CASE_FOLDER RTC ON TST.TEST_CASE_FOLDER_ID = RTC.TEST_CASE_FOLDER_ID
		
		
GO
IF OBJECT_ID ( 'VW_TESTCASE_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTCASE_INCIDENTS];
GO
CREATE VIEW [VW_TESTCASE_INCIDENTS]
AS
SELECT	TRN.TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
UNION
SELECT	TSP.TEST_CASE_ID AS TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_STEP TSP INNER JOIN TST_ARTIFACT_LINK ARL 
ON		TSP.TEST_STEP_ID = ARL.SOURCE_ARTIFACT_ID INNER JOIN TST_INCIDENT INC
ON		ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 7 AND ARL.DEST_ARTIFACT_TYPE_ID = 3
UNION
SELECT	TSP.TEST_CASE_ID AS TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_STEP TSP INNER JOIN TST_ARTIFACT_LINK ARL 
ON		TSP.TEST_STEP_ID = ARL.DEST_ARTIFACT_ID INNER JOIN TST_INCIDENT INC
ON		ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 7
GO
IF OBJECT_ID ( 'VW_TESTCASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTCASE_LIST];
GO
CREATE VIEW [VW_TESTCASE_LIST]
AS
	SELECT
		TST.*,
		PRJ.NAME AS PROJECT_NAME,
		EXE.NAME AS EXECUTION_STATUS_NAME,
		TP.NAME AS TEST_CASE_PRIORITY_NAME,
		TP.COLOR AS TEST_CASE_PRIORITY_COLOR,
		TCPS.NAME AS TEST_CASE_PREPARATION_STATUS,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		AEN.NAME AS AUTOMATION_ENGINE_NAME,
		STA.NAME AS TEST_CASE_STATUS_NAME,
		TYP.NAME AS TEST_CASE_TYPE_NAME,
		PRJ.IS_ACTIVE AS IS_PROJECT_ACTIVE,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM
		TST_TEST_CASE TST
			LEFT JOIN (
				SELECT *
				FROM TST_ARTIFACT_CUSTOM_PROPERTY
				WHERE ARTIFACT_TYPE_ID = 2
			) AS ACP
		ON TST.TEST_CASE_ID = ACP.ARTIFACT_ID
			INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN TST_EXECUTION_STATUS AS EXE ON TST.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
			INNER JOIN TST_USER_PROFILE AS USR1 ON TST.AUTHOR_ID = USR1.USER_ID
			INNER JOIN TST_TEST_CASE_STATUS STA ON TST.TEST_CASE_STATUS_ID = STA.TEST_CASE_STATUS_ID
			INNER JOIN TST_TEST_CASE_TYPE TYP ON TST.TEST_CASE_TYPE_ID = TYP.TEST_CASE_TYPE_ID
			LEFT JOIN TST_USER_PROFILE AS USR2 ON TST.OWNER_ID = USR2.USER_ID
			LEFT JOIN TST_TEST_CASE_PRIORITY AS TP ON TST.TEST_CASE_PRIORITY_ID = TP.TEST_CASE_PRIORITY_ID
			LEFT JOIN TST_TEST_CASE_PREPARATION_STATUS AS TCPS ON TST.TEST_CASE_PREPARATION_STATUS_ID = TCPS.TEST_CASE_PREPARATION_STATUS_ID
			LEFT JOIN TST_AUTOMATION_ENGINE AS AEN ON TST.AUTOMATION_ENGINE_ID = AEN.AUTOMATION_ENGINE_ID
GO
IF OBJECT_ID ( 'VW_TESTCASE_RELEASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTCASE_RELEASE_LIST];
GO
CREATE VIEW [VW_TESTCASE_RELEASE_LIST]
AS
	SELECT
		TST.TEST_CASE_ID, TST.TEST_CASE_PRIORITY_ID, TST.PROJECT_ID, TST.AUTHOR_ID,
		TST.TEST_CASE_STATUS_ID, TST.TEST_CASE_TYPE_ID, TST.TEST_CASE_FOLDER_ID,
		TST.NAME, TST.OWNER_ID, TST.DESCRIPTION, TST.CREATION_DATE, TST.LAST_UPDATE_DATE,
		TST.AUTOMATION_ENGINE_ID, TST.AUTOMATION_ATTACHMENT_ID, TST.IS_ATTACHMENTS,
		TST.IS_TEST_STEPS, TST.ESTIMATED_DURATION, TST.IS_DELETED, TST.CONCURRENCY_DATE,
		TST.COMPONENT_IDS,
		RTC.EXECUTION_STATUS_ID, RTC.EXECUTION_DATE, RTC.ACTUAL_DURATION, RTC.RELEASE_ID,
		EXE.NAME AS EXECUTION_STATUS_NAME,
		TP.NAME AS TEST_CASE_PRIORITY_NAME, TP.COLOR AS TEST_CASE_PRIORITY_COLOR,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		AEN.NAME AS AUTOMATION_ENGINE_NAME,
		STA.NAME AS TEST_CASE_STATUS_NAME,
		TYP.NAME AS TEST_CASE_TYPE_NAME,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM
		TST_TEST_CASE TST
			LEFT JOIN (
				SELECT *
				FROM TST_ARTIFACT_CUSTOM_PROPERTY
				WHERE ARTIFACT_TYPE_ID = 2
			) AS ACP
		ON TST.TEST_CASE_ID = ACP.ARTIFACT_ID
		INNER JOIN TST_RELEASE_TEST_CASE RTC ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID		
		INNER JOIN TST_EXECUTION_STATUS AS EXE ON RTC.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON TST.AUTHOR_ID = USR1.USER_ID
		INNER JOIN TST_TEST_CASE_STATUS STA ON TST.TEST_CASE_STATUS_ID = STA.TEST_CASE_STATUS_ID
		INNER JOIN TST_TEST_CASE_TYPE TYP ON TST.TEST_CASE_TYPE_ID = TYP.TEST_CASE_TYPE_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TST.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_TEST_CASE_PRIORITY AS TP ON TST.TEST_CASE_PRIORITY_ID = TP.TEST_CASE_PRIORITY_ID
		LEFT JOIN TST_AUTOMATION_ENGINE AS AEN ON TST.AUTOMATION_ENGINE_ID = AEN.AUTOMATION_ENGINE_ID
GO
IF OBJECT_ID ( 'VW_TESTRUNSTEP_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTRUNSTEP_INCIDENTS];
GO
CREATE VIEW [VW_TESTRUNSTEP_INCIDENTS]
AS
SELECT	TRS.TEST_RUN_STEP_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN_STEP TRS INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
GO
IF OBJECT_ID ( 'VW_TESTRUNSTEP_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTRUNSTEP_LIST];
GO
CREATE VIEW [VW_TESTRUNSTEP_LIST]
AS
	SELECT
		TRS.*,
		EXE.NAME AS EXECUTION_STATUS_NAME,
		INC.INCIDENT_COUNT
	FROM
		TST_TEST_RUN_STEP TRS
			LEFT JOIN (
				SELECT COUNT(INCIDENT_ID) AS INCIDENT_COUNT, TEST_RUN_STEP_ID
				FROM TST_TEST_RUN_STEP_INCIDENT
                GROUP BY TEST_RUN_STEP_ID
			) AS INC
			ON INC.TEST_RUN_STEP_ID = TRS.TEST_RUN_STEP_ID
		INNER JOIN TST_EXECUTION_STATUS AS EXE
			ON TRS.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
GO
IF OBJECT_ID ( 'VW_TESTRUN_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTRUN_INCIDENTS];
GO
CREATE VIEW [VW_TESTRUN_INCIDENTS]
AS
SELECT	TRN.TEST_RUN_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
GO
IF OBJECT_ID ( 'VW_TESTRUN_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTRUN_LIST];
GO
CREATE VIEW [VW_TESTRUN_LIST]
AS
	SELECT	TRN.*, EXE.NAME AS EXECUTION_STATUS_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS TESTER_NAME,
			REL.NAME AS RELEASE_NAME, REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER, TSE.NAME AS TEST_SET_NAME,
			TRT.NAME AS TEST_RUN_TYPE_NAME,
			AHT.NAME AS AUTOMATION_HOST_NAME,
			BLD.NAME AS BUILD_NAME,
			TST.PROJECT_ID, TST.IS_DELETED As TESTCASEDELETED,
			ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
			ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
			ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_CASE TST
	ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID INNER JOIN TST_EXECUTION_STATUS EXE
	ON		TRN.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID INNER JOIN TST_USER_PROFILE USR
	ON		TRN.TESTER_ID = USR.USER_ID LEFT JOIN TST_RELEASE REL
	ON		TRN.RELEASE_ID = REL.RELEASE_ID LEFT JOIN TST_TEST_SET TSE
	ON		TRN.TEST_SET_ID = TSE.TEST_SET_ID INNER JOIN TST_TEST_RUN_TYPE TRT
	ON		TRN.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID LEFT JOIN
				(SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 5) ACP
	ON		TRN.TEST_RUN_ID = ACP.ARTIFACT_ID LEFT JOIN TST_AUTOMATION_HOST AHT
	ON		TRN.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID LEFT JOIN TST_BUILD BLD
    ON		TRN.BUILD_ID = BLD.BUILD_ID
	WHERE	TST.IS_DELETED = 0
GO
IF OBJECT_ID ( 'VW_TESTRUNS_PENDING_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTRUNS_PENDING_LIST];
GO
CREATE VIEW [VW_TESTRUNS_PENDING_LIST]
AS
SELECT	TRP.TEST_RUNS_PENDING_ID, TRP.PROJECT_ID, TRP.TEST_SET_ID, TRP.TESTER_ID, TRP.NAME,
		TRP.CREATION_DATE, TRP.LAST_UPDATE_DATE, TRP.COUNT_PASSED, TRP.COUNT_FAILED, TRP.COUNT_BLOCKED,
		TRP.COUNT_CAUTION, TRP.COUNT_NOT_RUN, TRP.COUNT_NOT_APPLICABLE, TSE.NAME AS TEST_SET_NAME, PRJ.NAME AS PROJECT_NAME,
		CAST(1 AS BIT) AS IS_PRIMARY_OWNER
FROM	TST_TEST_RUNS_PENDING TRP INNER JOIN TST_PROJECT PRJ
ON		TRP.PROJECT_ID = PRJ.PROJECT_ID LEFT JOIN TST_TEST_SET TSE
ON		TRP.TEST_SET_ID = TSE.TEST_SET_ID
WHERE	PRJ.IS_ACTIVE = 1
UNION
SELECT	TRP.TEST_RUNS_PENDING_ID, TRP.PROJECT_ID, TRP.TEST_SET_ID, TRN.TESTER_ID, TRP.NAME,
		TRP.CREATION_DATE, TRP.LAST_UPDATE_DATE, TRP.COUNT_PASSED, TRP.COUNT_FAILED, TRP.COUNT_BLOCKED,
		TRP.COUNT_CAUTION, TRP.COUNT_NOT_RUN, TRP.COUNT_NOT_APPLICABLE, TSE.NAME AS TEST_SET_NAME, PRJ.NAME AS PROJECT_NAME,
		CAST(0 AS BIT) AS IS_PRIMARY_OWNER
FROM	TST_TEST_RUNS_PENDING TRP INNER JOIN TST_PROJECT PRJ
ON		TRP.PROJECT_ID = PRJ.PROJECT_ID LEFT JOIN TST_TEST_SET TSE
ON		TRP.TEST_SET_ID = TSE.TEST_SET_ID INNER JOIN TST_TEST_RUN TRN
ON		TRN.TEST_RUNS_PENDING_ID = TRP.TEST_RUNS_PENDING_ID
WHERE	PRJ.IS_ACTIVE = 1
AND		TRN.TESTER_ID <> TRP.TESTER_ID
GO
IF OBJECT_ID ( 'VW_TEST_SET_FOLDER_HIERARCHY', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TEST_SET_FOLDER_HIERARCHY];
GO
CREATE VIEW [VW_TEST_SET_FOLDER_HIERARCHY]
AS
SELECT TEST_SET_FOLDER_ID AS TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
FROM TST_TEST_SET_FOLDER_HIERARCHY
GO
IF OBJECT_ID ( 'VW_TESTSET_FOLDER_RELEASE', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_FOLDER_RELEASE];
GO
CREATE VIEW [VW_TESTSET_FOLDER_RELEASE]
AS
	SELECT
		ISNULL(TSE.TEST_SET_FOLDER_ID, 0) AS TEST_SET_FOLDER_ID,
		ISNULL(REL.RELEASE_ID, 0) AS DISPLAY_RELEASE_ID,
		TSE.PARENT_TEST_SET_FOLDER_ID, TSE.PROJECT_ID,
		TSE.NAME, TSE.DESCRIPTION, TSE.ESTIMATED_DURATION, TSE.LAST_UPDATE_DATE,
		RTX.EXECUTION_DATE, RTX.ACTUAL_DURATION,
		ISNULL(RTX.COUNT_PASSED, 0) AS COUNT_PASSED,
		ISNULL(RTX.COUNT_FAILED, 0) AS COUNT_FAILED,
		ISNULL(RTX.COUNT_CAUTION, 0) AS COUNT_CAUTION,
		ISNULL(RTX.COUNT_BLOCKED, 0) AS COUNT_BLOCKED,
		ISNULL(RTX.COUNT_NOT_RUN, ISNULL(TSE.COUNT_PASSED + TSE.COUNT_FAILED + TSE.COUNT_CAUTION + TSE.COUNT_BLOCKED + TSE.COUNT_NOT_RUN + TSE.COUNT_NOT_APPLICABLE,0)) AS COUNT_NOT_RUN,
		ISNULL(RTX.COUNT_NOT_APPLICABLE, 0) AS COUNT_NOT_APPLICABLE,
		ISNULL(TSE.CREATION_DATE, GETDATE()) AS CREATION_DATE
	FROM
		TST_TEST_SET_FOLDER TSE
		CROSS JOIN TST_RELEASE REL
		FULL OUTER JOIN TST_RELEASE_TEST_SET_FOLDER RTX
			ON TSE.TEST_SET_FOLDER_ID = RTX.TEST_SET_FOLDER_ID
			AND REL.RELEASE_ID = RTX.RELEASE_ID		
GO
IF OBJECT_ID ( 'VW_TESTSET_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_INCIDENTS];
GO
CREATE VIEW [VW_TESTSET_INCIDENTS]
AS
SELECT	TRN.TEST_SET_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	TRN.TEST_SET_ID IS NOT NULL
GO
IF OBJECT_ID ( 'VW_TESTSET_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_LIST];
GO
CREATE VIEW [VW_TESTSET_LIST]
AS
	SELECT
		TSE.*,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		TSS.NAME AS TEST_SET_STATUS_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME, 
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		PRJ.IS_ACTIVE AS IS_PROJECT_ACTIVE,
		AHT.NAME AS AUTOMATION_HOST_NAME,
		TRT.NAME AS TEST_RUN_TYPE_NAME,
		REC.NAME AS RECURRENCE_NAME,
		TCS.NAME AS TEST_CONFIGURATION_SET_NAME,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM TST_TEST_SET AS TSE
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSE.CREATOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSE.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_TEST_SET_STATUS AS TSS ON TSE.TEST_SET_STATUS_ID = TSS.TEST_SET_STATUS_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 8) AS ACP ON TSE.TEST_SET_ID = ACP.ARTIFACT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSE.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSE.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_AUTOMATION_HOST AS AHT ON TSE.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID
		LEFT JOIN TST_TEST_RUN_TYPE AS TRT ON TSE.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID
		LEFT JOIN TST_RECURRENCE AS REC ON TSE.RECURRENCE_ID = REC.RECURRENCE_ID
		LEFT JOIN TST_TEST_CONFIGURATION_SET AS TCS ON TSE.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID
GO
IF OBJECT_ID ( 'VW_TESTSET_RELEASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_RELEASE_LIST];
GO
CREATE VIEW [VW_TESTSET_RELEASE_LIST]
AS
	SELECT
		TSE.TEST_SET_ID, TSE.PROJECT_ID, TSE.RELEASE_ID, TSE.TEST_SET_STATUS_ID,
		TSE.CREATOR_ID, TSE.OWNER_ID, TSE.AUTOMATION_HOST_ID, TSE.TEST_RUN_TYPE_ID,
		TSE.RECURRENCE_ID, TSE.TEST_SET_FOLDER_ID, TSE.NAME, TSE.DESCRIPTION,
		TSE.CREATION_DATE, TSE.PLANNED_DATE, TSE.LAST_UPDATE_DATE,
		TSE.IS_ATTACHMENTS, TSE.IS_DELETED, TSE.CONCURRENCY_DATE,
		TSE.BUILD_EXECUTE_TIME_INTERVAL, TSE.ESTIMATED_DURATION,
		RTX.ACTUAL_DURATION,
		ISNULL(RTX.COUNT_PASSED, 0) AS COUNT_PASSED,
		ISNULL(RTX.COUNT_FAILED, 0) AS COUNT_FAILED,
		ISNULL(RTX.COUNT_CAUTION, 0) AS COUNT_CAUTION,
		ISNULL(RTX.COUNT_BLOCKED, 0) AS COUNT_BLOCKED,
		ISNULL(RTX.COUNT_NOT_RUN, ISNULL(TSE.COUNT_PASSED + TSE.COUNT_FAILED + TSE.COUNT_CAUTION + TSE.COUNT_BLOCKED + TSE.COUNT_NOT_RUN + TSE.COUNT_NOT_APPLICABLE,0)) AS COUNT_NOT_RUN,
		ISNULL(RTX.COUNT_NOT_APPLICABLE, 0) AS COUNT_NOT_APPLICABLE,
		RTX.EXECUTION_DATE,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		TSS.NAME AS TEST_SET_STATUS_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME, 
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		PRJ.IS_ACTIVE AS IS_PROJECT_ACTIVE,
		AHT.NAME AS AUTOMATION_HOST_NAME,
		TRT.NAME AS TEST_RUN_TYPE_NAME,
		REC.NAME AS RECURRENCE_NAME,
		TCS.NAME AS TEST_CONFIGURATION_SET_NAME,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99,
		RTX.RELEASE_ID AS DISPLAY_RELEASE_ID
	FROM TST_TEST_SET AS TSE
		INNER JOIN (
			SELECT
				REL.RELEASE_ID, TSE.TEST_SET_ID,
				RTX.ACTUAL_DURATION, RTX.EXECUTION_DATE,
				RTX.COUNT_PASSED,
				RTX.COUNT_FAILED,
				RTX.COUNT_CAUTION,
				RTX.COUNT_BLOCKED,
				RTX.COUNT_NOT_RUN,			
				RTX.COUNT_NOT_APPLICABLE			
			FROM TST_RELEASE REL
			CROSS JOIN TST_TEST_SET TSE
			FULL OUTER JOIN TST_RELEASE_TEST_SET RTX
				ON REL.RELEASE_ID = RTX.RELEASE_ID
				AND TSE.TEST_SET_ID = RTX.TEST_SET_ID
		) AS RTX ON TSE.TEST_SET_ID = RTX.TEST_SET_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSE.CREATOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSE.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_TEST_SET_STATUS AS TSS ON TSE.TEST_SET_STATUS_ID = TSS.TEST_SET_STATUS_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 8) AS ACP ON TSE.TEST_SET_ID = ACP.ARTIFACT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSE.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSE.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_AUTOMATION_HOST AS AHT ON TSE.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID
		LEFT JOIN TST_TEST_RUN_TYPE AS TRT ON TSE.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID
		LEFT JOIN TST_RECURRENCE AS REC ON TSE.RECURRENCE_ID = REC.RECURRENCE_ID
		LEFT JOIN TST_TEST_CONFIGURATION_SET AS TCS ON TSE.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID
GO
IF OBJECT_ID ( 'VW_TESTSET_TESTCASE_LIST_INTERNAL', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_TESTCASE_LIST_INTERNAL];
GO
CREATE VIEW [VW_TESTSET_TESTCASE_LIST_INTERNAL]
AS
	SELECT
		TSC.*,
		TSE.PROJECT_ID, TST.NAME, TST.DESCRIPTION, TST.AUTHOR_ID, TST.AUTHOR_NAME,
		TST.CREATION_DATE, TST.LAST_UPDATE_DATE, TST.CONCURRENCY_DATE, 
		TST.ESTIMATED_DURATION, TST.TEST_CASE_PRIORITY_ID, TST.TEST_CASE_PRIORITY_NAME, TST.TEST_CASE_PRIORITY_COLOR,
		TST.TEST_CASE_TYPE_ID, TST.TEST_CASE_STATUS_ID, TST.TEST_CASE_TYPE_NAME, TST.TEST_CASE_STATUS_NAME,
		TST.IS_ATTACHMENTS, TST.IS_TEST_STEPS, TSE.RELEASE_ID,
		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS OWNER_NAME,
		TSE.IS_DELETED AS IS_TEST_SET_DELETED,
		TST.IS_DELETED AS IS_TEST_CASE_DELETED,
		TST.CUST_01, TST.CUST_02, TST.CUST_03, TST.CUST_04, TST.CUST_05, TST.CUST_06, TST.CUST_07, TST.CUST_08, TST.CUST_09, TST.CUST_10,
		TST.CUST_11, TST.CUST_12, TST.CUST_13, TST.CUST_14, TST.CUST_15, TST.CUST_16, TST.CUST_17, TST.CUST_18, TST.CUST_19, TST.CUST_20,
		TST.CUST_21, TST.CUST_22, TST.CUST_23, TST.CUST_24, TST.CUST_25, TST.CUST_26, TST.CUST_27, TST.CUST_28, TST.CUST_29, TST.CUST_30,
		TST.CUST_31, TST.CUST_32, TST.CUST_33, TST.CUST_34, TST.CUST_35, TST.CUST_36, TST.CUST_37, TST.CUST_38, TST.CUST_39, TST.CUST_40,
		TST.CUST_41, TST.CUST_42, TST.CUST_43, TST.CUST_44, TST.CUST_45, TST.CUST_46, TST.CUST_47, TST.CUST_48, TST.CUST_49, TST.CUST_50,
		TST.CUST_51, TST.CUST_52, TST.CUST_53, TST.CUST_54, TST.CUST_55, TST.CUST_56, TST.CUST_57, TST.CUST_58, TST.CUST_59, TST.CUST_60,
		TST.CUST_61, TST.CUST_62, TST.CUST_63, TST.CUST_64, TST.CUST_65, TST.CUST_66, TST.CUST_67, TST.CUST_68, TST.CUST_69, TST.CUST_70,
		TST.CUST_71, TST.CUST_72, TST.CUST_73, TST.CUST_74, TST.CUST_75, TST.CUST_76, TST.CUST_77, TST.CUST_78, TST.CUST_79, TST.CUST_80,
		TST.CUST_81, TST.CUST_82, TST.CUST_83, TST.CUST_84, TST.CUST_85, TST.CUST_86, TST.CUST_87, TST.CUST_88, TST.CUST_89, TST.CUST_90,
		TST.CUST_91, TST.CUST_92, TST.CUST_93, TST.CUST_94, TST.CUST_95, TST.CUST_96, TST.CUST_97, TST.CUST_98, TST.CUST_99

	FROM TST_TEST_SET_TEST_CASE AS TSC
		INNER JOIN TST_TEST_SET AS TSE ON TSC.TEST_SET_ID = TSE.TEST_SET_ID
		INNER JOIN VW_TESTCASE_LIST AS TST ON TSC.TEST_CASE_ID = TST.TEST_CASE_ID
		LEFT JOIN TST_USER_PROFILE USR ON TSC.OWNER_ID = USR.USER_ID
GO
IF OBJECT_ID ( 'VW_TESTSET_TESTCASE_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSET_TESTCASE_LIST];
GO
CREATE VIEW [VW_TESTSET_TESTCASE_LIST]
AS
	SELECT
		TSC.*,
		CAST (NULL AS INT) AS EXECUTION_STATUS_ID,
		CAST (NULL AS NVARCHAR) AS EXECUTION_STATUS_NAME,
		CAST (NULL AS DATETIME) AS EXECUTION_DATE,
		CAST (NULL AS INT) AS ACTUAL_DURATION
	FROM VW_TESTSET_TESTCASE_LIST_INTERNAL AS TSC
GO
IF OBJECT_ID ( 'VW_TESTSTEP_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSTEP_INCIDENTS];
GO
CREATE VIEW [VW_TESTSTEP_INCIDENTS]
AS
SELECT	TRS.TEST_STEP_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN_STEP TRS INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	TRS.TEST_STEP_ID IS NOT NULL
UNION
SELECT	ARL.SOURCE_ARTIFACT_ID AS TEST_STEP_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_INCIDENT INC
ON		ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 7 AND ARL.DEST_ARTIFACT_TYPE_ID = 3
UNION
SELECT	ARL.DEST_ARTIFACT_ID AS TEST_STEP_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_INCIDENT INC
ON		ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 7
GO
IF OBJECT_ID ( 'VW_TESTSTEP_LIST', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TESTSTEP_LIST];
GO
CREATE VIEW [VW_TESTSTEP_LIST]
AS
	SELECT
		STP.*,
		EXE.NAME AS EXECUTION_STATUS_NAME,
		LTC.NAME AS LINKED_TEST_CASE_NAME,
		LTC.IS_DELETED AS IS_LINKED_TEST_CASE_DELETED,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM
		TST_TEST_STEP STP
			LEFT JOIN (
				SELECT *
				FROM TST_ARTIFACT_CUSTOM_PROPERTY
				WHERE ARTIFACT_TYPE_ID = 7 /* Test Step */
			) AS ACP
		ON STP.TEST_STEP_ID = ACP.ARTIFACT_ID
		INNER JOIN TST_EXECUTION_STATUS AS EXE ON STP.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		LEFT JOIN TST_TEST_CASE AS LTC ON STP.LINKED_TEST_CASE_ID = LTC.TEST_CASE_ID
GO
IF OBJECT_ID ('VW_USERHISTORYCHANGE_LIST', 'V') IS NOT NULL 
    DROP VIEW [VW_USERHISTORYCHANGE_LIST];
GO

CREATE VIEW [dbo].[VW_USERHISTORYCHANGE_LIST]
AS
	SELECT 
		HC.*,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME
		
	FROM [ValidationMasterAudit].dbo.TST_USER_HISTORY_CHANGESET_AUDIT AS HC
		INNER JOIN [ValidationMasterAudit].dbo.TST_ADMIN_SECTION_AUDIT AS AD ON HC.ADMIN_SECTION_ID = AD.ADMIN_SECTION_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.HISTORY_CHANGESET_TYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
GOIF OBJECT_ID ( 'RPT_ARTIFACT_ASSOCIATION', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_ARTIFACT_ASSOCIATION];
GO
CREATE VIEW [RPT_ARTIFACT_ASSOCIATION]
AS
    SELECT	ARL.*,
			ART1.NAME AS SOURCE_ARTIFACT_TYPE_NAME,
			ART2.NAME AS DEST_ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.NAME AS ARTIFACT_LINK_TYPE_NAME
    FROM	TST_ARTIFACT_LINK ARL
    INNER JOIN TST_ARTIFACT_TYPE ART1 ON ARL.SOURCE_ARTIFACT_TYPE_ID = ART1.ARTIFACT_TYPE_ID
    INNER JOIN TST_ARTIFACT_TYPE ART2 ON ARL.DEST_ARTIFACT_TYPE_ID = ART2.ARTIFACT_TYPE_ID    
    LEFT JOIN TST_USER_PROFILE USR ON ARL.CREATOR_ID = USR.USER_ID
    INNER JOIN TST_ARTIFACT_LINK_TYPE ALT ON ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID
GO
IF OBJECT_ID ( 'RPT_ARTIFACT_ATTACHMENT', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_ARTIFACT_ATTACHMENT];
GO
CREATE VIEW [RPT_ARTIFACT_ATTACHMENT]
AS
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_REQUIREMENT OBJ
ON		AAT.ARTIFACT_ID = OBJ.REQUIREMENT_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.AUTHOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT_STATUS STA
ON		OBJ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 1
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_TEST_CASE OBJ
ON		AAT.ARTIFACT_ID = OBJ.TEST_CASE_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.AUTHOR_ID = USR.USER_ID INNER JOIN TST_EXECUTION_STATUS STA
ON		OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 2
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.OPENER_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_INCIDENT OBJ
ON		AAT.ARTIFACT_ID = OBJ.INCIDENT_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.OPENER_ID = USR.USER_ID INNER JOIN TST_INCIDENT_STATUS STA
ON		OBJ.INCIDENT_STATUS_ID = STA.INCIDENT_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 3
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.CREATOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, '-' AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_RELEASE OBJ
ON		AAT.ARTIFACT_ID = OBJ.RELEASE_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.CREATOR_ID = USR.USER_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 4
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.TESTER_ID AS CREATOR_ID, OBJ.START_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_TEST_RUN OBJ
ON		AAT.ARTIFACT_ID = OBJ.TEST_RUN_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.TESTER_ID = USR.USER_ID INNER JOIN TST_EXECUTION_STATUS STA
ON		OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 5
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, ATC.AUTHOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_TASK OBJ
ON		AAT.ARTIFACT_ID = OBJ.TASK_ID INNER JOIN TST_ATTACHMENT ATC
ON		AAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID INNER JOIN TST_USER_PROFILE USR
ON		ATC.AUTHOR_ID = USR.USER_ID INNER JOIN TST_TASK_STATUS STA
ON		OBJ.TASK_STATUS_ID = STA.TASK_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 6
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, ('Test Case ' + CAST(OBJ.TEST_CASE_ID AS NVARCHAR)+ ' Test Step ' + CAST(OBJ.POSITION AS NVARCHAR)) AS ARTIFACT_NAME, '' AS COMMENT, ATC.AUTHOR_ID AS CREATOR_ID, ATC.UPLOAD_DATE AS CREATION_DATE,
		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_TEST_STEP OBJ
ON		AAT.ARTIFACT_ID = OBJ.TEST_STEP_ID INNER JOIN TST_ATTACHMENT ATC
ON		AAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID INNER JOIN TST_USER_PROFILE USR
ON		ATC.AUTHOR_ID = USR.USER_ID INNER JOIN TST_EXECUTION_STATUS STA
ON		OBJ.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 7
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, OBJ.CREATOR_ID AS CREATOR_ID, OBJ.CREATION_DATE, (RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, STA.NAME AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_TEST_SET OBJ
ON		AAT.ARTIFACT_ID = OBJ.TEST_SET_ID INNER JOIN TST_USER_PROFILE USR
ON		OBJ.CREATOR_ID = USR.USER_ID INNER JOIN TST_TEST_SET_STATUS STA
ON		OBJ.TEST_SET_STATUS_ID = STA.TEST_SET_STATUS_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 8
UNION
SELECT	(AAT.ARTIFACT_ID * 10 + AAT.ARTIFACT_TYPE_ID) AS ARTIFACT_LINK_ID, AAT.ARTIFACT_TYPE_ID, AAT.ARTIFACT_ID, ART.NAME AS ARTIFACT_TYPE_NAME, OBJ.NAME AS ARTIFACT_NAME, '' AS COMMENT, 1 AS CREATOR_ID, LAST_UPDATE_DATE AS CREATION_DATE, '-' AS CREATOR_NAME,
		AAT.ATTACHMENT_ID, AAT.PROJECT_ID, '-' AS ARTIFACT_STATUS_NAME
FROM	TST_ARTIFACT_ATTACHMENT AAT INNER JOIN TST_ARTIFACT_TYPE ART
ON		AAT.ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID INNER JOIN TST_AUTOMATION_HOST OBJ
ON		AAT.ARTIFACT_ID = OBJ.AUTOMATION_HOST_ID
WHERE	AAT.ARTIFACT_TYPE_ID = 9
GO
IF OBJECT_ID ( 'RPT_ARTIFACT_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_ARTIFACT_TYPES];
GO
CREATE VIEW [RPT_ARTIFACT_TYPES]
AS
	SELECT	ART.NAME, ART.ARTIFACT_TYPE_ID, ART.PREFIX
	FROM TST_ARTIFACT_TYPE AS ART
	WHERE ART.IS_GLOBAL_ITEM = 0
GO
IF OBJECT_ID ( 'RPT_ATTACHMENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_ATTACHMENTS];
GO
CREATE VIEW [RPT_ATTACHMENTS]
AS
	SELECT	PAT.*, 
			ATC.ATTACHMENT_TYPE_ID, ATC.AUTHOR_ID, ATC.EDITOR_ID, ATC.FILENAME, 
			ATC.DESCRIPTION, ATC.UPLOAD_DATE, ATC.EDITED_DATE, ATC.SIZE, ATC.CURRENT_VERSION, TGS.TAGS, 
			ATC.CONCURRENCY_DATE, ATC.DOCUMENT_STATUS_ID,
			DCS.IS_OPEN_STATUS AS DOCUMENT_STATUS_IS_OPEN_STATUS, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE, PRJ.PROJECT_GROUP_ID, PRJ.NAME AS PROJECT_NAME,
			(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
			(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS EDITOR_NAME, 
			ATT.NAME AS ATTACHMENT_TYPE_NAME, PAE.NAME DOCUMENT_TYPE_NAME, DCS.NAME AS DOCUMENT_STATUS_NAME, PAF.NAME AS PROJECT_ATTACHMENT_FOLDER_NAME,
			ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
			ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
			ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM	TST_PROJECT_ATTACHMENT PAT
		INNER JOIN TST_ATTACHMENT ATC ON PAT.ATTACHMENT_ID = ATC.ATTACHMENT_ID
		INNER JOIN TST_USER_PROFILE USR1 ON	ATC.AUTHOR_ID = USR1.USER_ID
		INNER JOIN TST_USER_PROFILE USR2 ON ATC.EDITOR_ID = USR2.USER_ID
		INNER JOIN TST_ATTACHMENT_TYPE ATT ON ATC.ATTACHMENT_TYPE_ID = ATT.ATTACHMENT_TYPE_ID
		INNER JOIN TST_DOCUMENT_TYPE PAE ON PAT.DOCUMENT_TYPE_ID = PAE.DOCUMENT_TYPE_ID
		INNER JOIN TST_DOCUMENT_STATUS DCS ON ATC.DOCUMENT_STATUS_ID = DCS.DOCUMENT_STATUS_ID
		INNER JOIN TST_PROJECT_ATTACHMENT_FOLDER PAF ON PAT.PROJECT_ATTACHMENT_FOLDER_ID = PAF.PROJECT_ATTACHMENT_FOLDER_ID 
		INNER JOIN TST_PROJECT AS PRJ ON PAT.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_TAGS
			WHERE ARTIFACT_TYPE_ID = 13
			) AS TGS ON ATC.ATTACHMENT_ID = TGS.ARTIFACT_ID 
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 13
			) AS ACP ON ATC.ATTACHMENT_ID = ACP.ARTIFACT_ID	
GO
IF OBJECT_ID ( 'RPT_ATTACHMENT_FOLDERS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_ATTACHMENT_FOLDERS];
GO
CREATE VIEW [RPT_ATTACHMENT_FOLDERS]
AS
	SELECT
		FOL.*,
		PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
	FROM
		TST_PROJECT_ATTACHMENT_FOLDER FOL
		INNER JOIN TST_PROJECT AS PRJ ON FOL.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_AUTOMATIONHOSTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_AUTOMATIONHOSTS];
GO
CREATE VIEW [RPT_AUTOMATIONHOSTS]
AS
	SELECT ATH.*,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99,
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
    FROM TST_AUTOMATION_HOST AS ATH
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 9
			) AS ACP ON ATH.AUTOMATION_HOST_ID = ACP.ARTIFACT_ID
	INNER JOIN TST_PROJECT AS PRJ ON ATH.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_BASELINES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_BASELINES];
GO
CREATE VIEW [RPT_BASELINES]
AS
	SELECT
		B.*,
		U1.USER_NAME BASELINE_CREATOR_LOGIN,
		S.USER_ID CHANGESET_CREATOR_ID,
		U2.USER_NAME CHANGESET_CREATOR_LOGIN,
		S.ARTIFACT_TYPE_ID ARTIFACT_TYPE_ID,
		A.NAME ARTIFACT_TYPE_NAME,
		S.CHANGE_DATE CHANGESET_DATE,
		S.CHANGETYPE_ID CHANGESET_TYPE_ID,
		T.CHANGE_NAME CHANGESET_TYPE_NAME
	FROM
		TST_PROJECT_BASELINE B
		INNER JOIN TST_HISTORY_CHANGESET AS S ON B.CHANGESET_ID = S.CHANGESET_ID
		INNER JOIN TST_ARTIFACT_TYPE AS A ON A.ARTIFACT_TYPE_ID = S.ARTIFACT_TYPE_ID
		INNER JOIN TST_USER AS U1 ON U1.USER_ID = B.CREATOR_USER_ID
		INNER JOIN TST_USER AS U2 ON U2.USER_ID = S.USER_ID
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS T ON T.CHANGETYPE_ID = S.CHANGETYPE_ID
GO
IF OBJECT_ID ( 'RPT_BUILDS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_BUILDS];
GO
CREATE VIEW [RPT_BUILDS]
AS
	SELECT	BLD.*, REL.NAME AS RELEASE_NAME, REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
			PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID	
	FROM TST_BUILD AS BLD
		INNER JOIN TST_RELEASE AS REL ON BLD.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON BLD.PROJECT_ID = PRJ.PROJECT_ID
	WHERE PRJ.IS_ACTIVE = 1 AND REL.IS_DELETED = 0 AND BLD.IS_DELETED = 0
GO
IF OBJECT_ID ( 'RPT_COMMENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_COMMENTS];
GO
CREATE VIEW [RPT_COMMENTS]
AS
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_REQUIREMENT_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 1
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_TEST_CASE_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 2
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.INCIDENT_ID AS ARTIFACT_ID, CMT.CREATOR_ID, CMT.RESOLUTION AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_INCIDENT_RESOLUTION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 3
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_RELEASE_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 4
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_TASK_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 6
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_TEST_SET_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 8
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATON_DATE AS CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_DOCUMENT_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 13
		WHERE CMT.IS_DELETED = 0
	UNION
	SELECT	ART.ARTIFACT_TYPE_ID, CMT.ARTIFACT_ID, CMT.CREATOR_ID, CMT.TEXT AS COMMENT_TEXT, CMT.CREATION_DATE,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME, ART.NAME AS ARTIFACT_TYPE_NAME
	FROM TST_RISK_DISCUSSION CMT
		INNER JOIN TST_USER_PROFILE USR ON USR.USER_ID = CMT.CREATOR_ID
		INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = 14
		WHERE CMT.IS_DELETED = 0
GO
IF OBJECT_ID ( 'RPT_COMPONENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_COMPONENTS];
GO
CREATE VIEW [RPT_COMPONENTS]
AS
	SELECT CMP.*,
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
    FROM TST_COMPONENT AS CMP
	INNER JOIN TST_PROJECT AS PRJ ON CMP.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_CUSTOM_LISTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_CUSTOM_LISTS];
GO
CREATE VIEW [RPT_CUSTOM_LISTS]
AS
	SELECT	CPL.*, PRJ.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE
	FROM TST_CUSTOM_PROPERTY_LIST AS CPL
		INNER JOIN TST_PROJECT PRJ ON PRJ.PROJECT_TEMPLATE_ID = CPL.PROJECT_TEMPLATE_ID
GO
IF OBJECT_ID ( 'RPT_CUSTOM_LIST_VALUES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_CUSTOM_LIST_VALUES];
GO
CREATE VIEW [RPT_CUSTOM_LIST_VALUES]
AS
	SELECT	CPV.*, PRJ.PROJECT_ID, CPL.NAME AS CUSTOM_PROPERTY_LIST_NAME, CPL.IS_ACTIVE AS CUSTOM_PROPERTY_LIST_IS_ACTIVE, PRJ.NAME AS PROJECT_NAME, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE
	FROM TST_CUSTOM_PROPERTY_VALUE AS CPV 
		INNER JOIN TST_CUSTOM_PROPERTY_LIST AS CPL ON CPV.CUSTOM_PROPERTY_LIST_ID = CPL.CUSTOM_PROPERTY_LIST_ID
		INNER JOIN TST_PROJECT PRJ ON PRJ.PROJECT_TEMPLATE_ID = CPL.PROJECT_TEMPLATE_ID
GO
IF OBJECT_ID ( 'RPT_CUSTOM_PROPERTY_DEFINITIONS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_CUSTOM_PROPERTY_DEFINITIONS];
GO
CREATE VIEW [RPT_CUSTOM_PROPERTY_DEFINITIONS]
AS
	SELECT	CPT.*, PRJ.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE, ART.NAME AS ARTIFACT_TYPE_NAME,
		CPY.NAME AS CUSTOM_PROPERTY_TYPE_NAME, CPL.NAME AS CUSTOM_PROPERTY_LIST_NAME
	FROM TST_CUSTOM_PROPERTY AS CPT
	INNER JOIN TST_PROJECT PRJ ON PRJ.PROJECT_TEMPLATE_ID = CPT.PROJECT_TEMPLATE_ID
	INNER JOIN TST_ARTIFACT_TYPE ART ON ART.ARTIFACT_TYPE_ID = CPT.ARTIFACT_TYPE_ID
	INNER JOIN TST_CUSTOM_PROPERTY_TYPE CPY ON CPY.CUSTOM_PROPERTY_TYPE_ID = CPT.CUSTOM_PROPERTY_TYPE_ID
	LEFT JOIN TST_CUSTOM_PROPERTY_LIST CPL ON CPT.CUSTOM_PROPERTY_LIST_ID = CPL.CUSTOM_PROPERTY_LIST_ID
GO
IF OBJECT_ID ( 'RPT_DOCUMENT_STATUSES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_DOCUMENT_STATUSES];
GO
CREATE VIEW [RPT_DOCUMENT_STATUSES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_DOCUMENT_STATUS AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_DOCUMENT_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_DOCUMENT_TYPES];
GO
CREATE VIEW [RPT_DOCUMENT_TYPES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_DOCUMENT_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_EVENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_EVENTS];
GO
CREATE VIEW [RPT_EVENTS]
AS
	SELECT EVT.*, TYP.NAME AS EVENT_TYPE_NAME
    FROM TST_EVENT AS EVT
    INNER JOIN TST_EVENT_TYPE TYP ON EVT.EVENT_TYPE_ID = TYP.EVENT_TYPE_ID
GO
IF OBJECT_ID ('RPT_HISTORYCHANGESETS', 'V') IS NOT NULL 
    DROP VIEW [RPT_HISTORYCHANGESETS];
GO
CREATE VIEW [RPT_HISTORYCHANGESETS]
AS
	SELECT 
		HC.*,
		HT.CHANGE_NAME AS CHANGETYPE_NAME,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS USER_NAME,
		AT.NAME AS ARTIFACT_TYPE_NAME
		
	FROM TST_HISTORY_CHANGESET AS HC
		INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
		INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
		INNER JOIN TST_ARTIFACT_TYPE AS AT ON HC.ARTIFACT_TYPE_ID = AT.ARTIFACT_TYPE_ID
GO
IF OBJECT_ID ( 'RPT_HISTORYDETAILS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_HISTORYDETAILS];
GO
CREATE VIEW [RPT_HISTORYDETAILS]
AS
	SELECT
		HD.ARTIFACT_HISTORY_ID, HD.FIELD_NAME, HD.FIELD_CAPTION,
		HD.OLD_VALUE, HD.NEW_VALUE,
		HD.OLD_VALUE_INT, HD.NEW_VALUE_INT,
		HD.OLD_VALUE_DATE, HD.NEW_VALUE_DATE,
		HD.CHANGESET_ID, HD.FIELD_ID, CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME, HC.CHANGETYPE_ID, AF.ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_DETAIL AS HD
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HD.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	LEFT JOIN TST_ARTIFACT_FIELD AS AF ON HD.FIELD_ID = AF.ARTIFACT_FIELD_ID
	UNION ALL
	SELECT
		HP.HISTORY_POSITION_ID AS ARTIFACT_HISTORY_ID,
		'_Position' AS FIELD_NAME,
		(ART.NAME + ' [' + ART.PREFIX + ':' + CAST(HP.CHILD_ARTIFACT_ID AS NVARCHAR) + '] Position') AS FIELD_CAPTION,
		CAST (HP.OLD_POSITION AS NVARCHAR) AS OLD_VALUE,
		CAST (HP.NEW_POSITION AS NVARCHAR) AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HP.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_POSITION AS HP
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HP.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE ART ON HP.CHILD_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID
	UNION ALL
	SELECT
		HA.ASSOCIATION_HISTORY_ID AS ARTIFACT_HISTORY_ID,
		'_Association' AS FIELD_NAME,
		HT.CHANGE_NAME AS FIELD_CAPTION,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Remove*/14 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS OLD_VALUE,
		CASE WHEN HC.CHANGETYPE_ID = /*Association Add*/13 THEN (SART.NAME + ' [' + SART.PREFIX + ':' + CAST(HA.SOURCE_ARTIFACT_ID AS NVARCHAR) + '] -> ' + DART.NAME + ' [' + DART.PREFIX + ':' + CAST(HA.DEST_ARTIFACT_ID AS NVARCHAR) + ']') ELSE '' END AS NEW_VALUE,
		NULL AS OLD_VALUE_INT, NULL AS NEW_VALUE_INT,
		NULL AS OLD_VALUE_DATE, NULL AS NEW_VALUE_DATE,
		HA.CHANGESET_ID,
		NULL AS FIELD_ID,
		NULL AS CUSTOM_PROPERTY_ID,
		HC.ARTIFACT_ID, HC.USER_ID, HC.ARTIFACT_TYPE_ID, HC.CHANGE_DATE,
		(RTRIM(US.FIRST_NAME + ' ' + ISNULL(US.MIDDLE_INITIAL,'')) + ' ' + US.LAST_NAME) AS CHANGER_NAME,
		HT.CHANGE_NAME,
		HC.CHANGETYPE_ID,
		NULL AS ARTIFACT_FIELD_TYPE_ID
	FROM TST_HISTORY_ASSOCIATION AS HA
	INNER JOIN TST_HISTORY_CHANGESET AS HC ON HA.CHANGESET_ID = HC.CHANGESET_ID
	INNER JOIN TST_USER_PROFILE AS US ON HC.USER_ID = US.USER_ID
	INNER JOIN TST_HISTORY_CHANGESET_TYPE AS HT ON HC.CHANGETYPE_ID = HT.CHANGETYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE SART ON HA.SOURCE_ARTIFACT_TYPE_ID = SART.ARTIFACT_TYPE_ID
	INNER JOIN TST_ARTIFACT_TYPE DART ON HA.DEST_ARTIFACT_TYPE_ID = DART.ARTIFACT_TYPE_ID
GO
IF OBJECT_ID ( 'RPT_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_INCIDENTS];
GO
CREATE VIEW [RPT_INCIDENTS]
AS
	SELECT	INC.*,
			PRI.NAME AS PRIORITY_NAME,
			PRI.COLOR AS PRIORITY_COLOR,
			SEV.NAME AS SEVERITY_NAME,
			SEV.COLOR AS SEVERITY_COLOR,
			IST.NAME AS INCIDENT_STATUS_NAME,
			IST.IS_OPEN_STATUS AS INCIDENT_STATUS_IS_OPEN_STATUS,
			ITP.NAME AS INCIDENT_TYPE_NAME,
			(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS OPENER_NAME,
			(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
            REL1.VERSION_NUMBER AS DETECTED_RELEASE_VERSION_NUMBER,
            REL2.VERSION_NUMBER AS RESOLVED_RELEASE_VERSION_NUMBER,
            REL3.VERSION_NUMBER AS VERIFIED_RELEASE_VERSION_NUMBER,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID,
			ITP.IS_ISSUE AS INCIDENT_TYPE_IS_ISSUE,
			BLD1.NAME AS DETECTED_BUILD_NAME,		
			BLD2.NAME AS RESOLVED_BUILD_NAME,		
			ITP.IS_RISK AS INCIDENT_TYPE_IS_RISK,
			PRJ.NAME AS PROJECT_NAME,
            ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
            ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
            ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM	TST_INCIDENT INC LEFT JOIN TST_INCIDENT_PRIORITY PRI
	ON		INC.PRIORITY_ID = PRI.PRIORITY_ID LEFT JOIN TST_INCIDENT_TYPE ITP
    ON		INC.INCIDENT_TYPE_ID = ITP.INCIDENT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR2
    ON		INC.OWNER_ID = USR2.USER_ID INNER JOIN TST_INCIDENT_STATUS IST
    ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID INNER JOIN TST_USER_PROFILE USR1
    ON		INC.OPENER_ID = USR1.USER_ID INNER JOIN TST_PROJECT PRJ
	ON		INC.PROJECT_ID = PRJ.PROJECT_ID LEFT JOIN TST_INCIDENT_SEVERITY SEV
    ON		INC.SEVERITY_ID = SEV.SEVERITY_ID LEFT JOIN TST_RELEASE REL1
    ON		INC.DETECTED_RELEASE_ID = REL1.RELEASE_ID LEFT JOIN TST_RELEASE REL2
    ON		INC.RESOLVED_RELEASE_ID = REL2.RELEASE_ID LEFT JOIN TST_RELEASE REL3
    ON		INC.VERIFIED_RELEASE_ID = REL3.RELEASE_ID LEFT JOIN TST_BUILD BLD1
    ON		INC.DETECTED_BUILD_ID = BLD1.BUILD_ID  LEFT JOIN TST_BUILD BLD2
    ON		INC.RESOLVED_BUILD_ID = BLD2.BUILD_ID LEFT JOIN (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 3) ACP
    ON		INC.INCIDENT_ID = ACP.ARTIFACT_ID
GO
IF OBJECT_ID ( 'RPT_INCIDENT_PRIORITIES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_INCIDENT_PRIORITIES];
GO
CREATE VIEW [RPT_INCIDENT_PRIORITIES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_INCIDENT_PRIORITY AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_INCIDENT_SEVERITIES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_INCIDENT_SEVERITIES];
GO
CREATE VIEW [RPT_INCIDENT_SEVERITIES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_INCIDENT_SEVERITY AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_INCIDENT_STATUSES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_INCIDENT_STATUSES];
GO
CREATE VIEW [RPT_INCIDENT_STATUSES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_INCIDENT_STATUS AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_INCIDENT_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_INCIDENT_TYPES];
GO
CREATE VIEW [RPT_INCIDENT_TYPES]
AS
    SELECT	PROP.INCIDENT_TYPE_ID, PROP.PROJECT_TEMPLATE_ID, PROP.WORKFLOW_ID, PROP.NAME, PROP.IS_ACTIVE, PROP.IS_ISSUE, PROP.IS_DEFAULT, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_INCIDENT_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_PORTFOLIOS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PORTFOLIOS];
GO
CREATE VIEW [RPT_PORTFOLIOS]
AS
	SELECT	PRT.*
	FROM TST_PORTFOLIO AS PRT
	WHERE PRT.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_PROJECTGROUPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECTGROUPS];
GO
CREATE VIEW [RPT_PROJECTGROUPS]
AS
	SELECT	PRG.*
	FROM TST_PROJECT_GROUP AS PRG
	WHERE PRG.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_PROJECTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECTS];
GO
CREATE VIEW [RPT_PROJECTS]
AS
	SELECT	PRJ.*, PRG.NAME AS PROJECT_GROUP_NAME, PRG.DESCRIPTION AS PROJECT_GROUP_DESCRIPTION
			
	FROM TST_PROJECT AS PRJ
		INNER JOIN TST_PROJECT_GROUP AS PRG ON PRJ.PROJECT_GROUP_ID = PRG.PROJECT_GROUP_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_PROJECTTEMPLATES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECTTEMPLATES];
GO
CREATE VIEW [RPT_PROJECTTEMPLATES]
AS
	SELECT	PRT.*
	FROM TST_PROJECT_TEMPLATE AS PRT
	WHERE PRT.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_PROJECT_MEMBERSHIP', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECT_MEMBERSHIP];
GO
CREATE VIEW [RPT_PROJECT_MEMBERSHIP]
AS
	SELECT	PUSER.*, PRJ.NAME AS PROJECT_NAME, ROLE.NAME AS PROJECT_ROLE_NAME, USR.USER_NAME, UPL.FIRST_NAME, UPL.LAST_NAME, UPL.DEPARTMENT, UPL.IS_ADMIN AS IS_SYSTEM_ADMIN
	FROM TST_PROJECT_USER AS PUSER 
	LEFT JOIN TST_PROJECT PRJ ON PUSER.PROJECT_ID = PRJ.PROJECT_ID
	LEFT JOIN TST_PROJECT_ROLE ROLE ON PUSER.PROJECT_ROLE_ID = ROLE.PROJECT_ROLE_ID
	LEFT JOIN TST_USER USR ON PUSER.USER_ID = USR.USER_ID
	INNER JOIN TST_USER_PROFILE AS UPL ON USR.USER_ID = UPL.USER_ID
GO
IF OBJECT_ID ( 'RPT_PROJECT_RESOURCES_RELEASE', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECT_RESOURCES_RELEASE];
GO
CREATE VIEW [RPT_PROJECT_RESOURCES_RELEASE]
AS
SELECT  PRJ.PROJECT_ID, INC.OWNER_ID AS USER_ID, REL.RELEASE_ID,
		SUM(INC.ESTIMATED_EFFORT) AS INCIDENT_EFFORT, 0 AS TASK_EFFORT
FROM	TST_PROJECT PRJ INNER JOIN TST_INCIDENT INC
ON		INC.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_RELEASE REL
ON		INC.RESOLVED_RELEASE_ID = REL.RELEASE_ID
WHERE	INC.OWNER_ID IS NOT NULL
AND		INC.IS_DELETED = 0
GROUP BY INC.OWNER_ID, PRJ.PROJECT_ID, REL.RELEASE_ID
UNION
SELECT  PRJ.PROJECT_ID, TSK.OWNER_ID AS USER_ID, REL.RELEASE_ID,
		0 AS INCIDENT_EFFORT, SUM(TSK.ESTIMATED_EFFORT) AS TASK_EFFORT
FROM	TST_PROJECT PRJ INNER JOIN TST_TASK TSK
ON		TSK.PROJECT_ID = PRJ.PROJECT_ID INNER JOIN TST_RELEASE REL
ON		TSK.RELEASE_ID = REL.RELEASE_ID
WHERE	TSK.OWNER_ID IS NOT NULL
AND		TSK.IS_DELETED = 0
GROUP BY TSK.OWNER_ID, PRJ.PROJECT_ID, REL.RELEASE_ID
GO
IF OBJECT_ID ( 'RPT_PROJECT_ROLES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_PROJECT_ROLES];
GO
CREATE VIEW [RPT_PROJECT_ROLES]
AS
    SELECT	ROLE.*, RLP.ARTIFACT_TYPE_ID, TYPE.NAME AS ARTIFACT, PRM.NAME AS PERMISSION
    FROM TST_PROJECT_ROLE AS ROLE
    LEFT JOIN TST_PROJECT_ROLE_PERMISSION AS RLP ON ROLE.PROJECT_ROLE_ID = RLP.PROJECT_ROLE_ID
    LEFT JOIN TST_ARTIFACT_TYPE AS TYPE ON RLP.ARTIFACT_TYPE_ID = TYPE.ARTIFACT_TYPE_ID
    INNER JOIN TST_PERMISSION AS PRM ON RLP.PERMISSION_ID = PRM.PERMISSION_ID
GO
IF OBJECT_ID ( 'RPT_RELEASES', 'V' ) IS NOT NULL 
    DROP VIEW RPT_RELEASES;
GO

CREATE VIEW RPT_RELEASES
AS
	
SELECT
	REL.*,
	(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
	(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
	(REL.VERSION_NUMBER + ' - ' + REL.NAME) AS FULL_NAME, 
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
	RTY.NAME AS RELEASE_TYPE_NAME, RST.NAME AS RELEASE_STATUS_NAME,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
	ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
FROM TST_RELEASE AS REL
	INNER JOIN TST_RELEASE_TYPE RTY ON REL.RELEASE_TYPE_ID = RTY.RELEASE_TYPE_ID
	INNER JOIN TST_RELEASE_STATUS RST ON REL.RELEASE_STATUS_ID = RST.RELEASE_STATUS_ID
	INNER JOIN TST_USER_PROFILE USR1 ON REL.CREATOR_ID = USR1.USER_ID
	LEFT JOIN TST_USER_PROFILE USR2 ON REL.OWNER_ID = USR2.USER_ID
	LEFT JOIN (
		SELECT *
		FROM TST_ARTIFACT_CUSTOM_PROPERTY
		WHERE ARTIFACT_TYPE_ID = 4) AS ACP ON REL.RELEASE_ID = ACP.ARTIFACT_ID
	INNER JOIN TST_PROJECT AS PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID

GO
IF OBJECT_ID ( 'RPT_RELEASE_TESTCASES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RELEASE_TESTCASES];
GO
CREATE VIEW [RPT_RELEASE_TESTCASES]
AS
	SELECT RTC.*, REL.NAME AS RELEASE_NAME, REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER, TST.NAME AS TEST_CASE_NAME,
		REL.RELEASE_TYPE_ID, REL.PROJECT_ID, PRJ.NAME AS PROJECT_NAME, RTY.NAME AS RELEASE_TYPE_NAME
	FROM TST_RELEASE_TEST_CASE RTC
		INNER JOIN TST_RELEASE REL ON RTC.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
		INNER JOIN TST_RELEASE_TYPE RTY ON REL.RELEASE_TYPE_ID = RTY.RELEASE_TYPE_ID
GO
IF OBJECT_ID ( 'RPT_REQUIREMENT_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_INCIDENTS];
GO
CREATE VIEW [RPT_REQUIREMENT_INCIDENTS]
AS
	SELECT
		RTC.REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_REQUIREMENT_TEST_CASE AS RTC
		INNER JOIN TST_TEST_RUN AS TRN ON RTC.TEST_CASE_ID = TRN.TEST_CASE_ID
		INNER JOIN TST_TEST_RUN_STEP AS TRS ON TRN.TEST_RUN_ID = TRS.TEST_RUN_ID
		INNER JOIN TST_TEST_RUN_STEP_INCIDENT AS TRI ON TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID
		INNER JOIN TST_INCIDENT AS INC ON TRI.INCIDENT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	UNION

	SELECT
		ARL.SOURCE_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
		INNER JOIN TST_INCIDENT AS INC ON ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
		INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID

	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 1 AND ARL.DEST_ARTIFACT_TYPE_ID = 3

	UNION
	
	SELECT
		ARL.DEST_ARTIFACT_ID AS REQUIREMENT_ID,
		INC.INCIDENT_ID,
		INC.DETECTED_RELEASE_ID,
		IST.IS_OPEN_STATUS

	FROM TST_ARTIFACT_LINK AS ARL
	INNER JOIN TST_INCIDENT AS INC ON ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID AND INC.IS_DELETED = 0
	INNER JOIN TST_INCIDENT_STATUS AS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID 
	
	WHERE ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 1

GO
IF OBJECT_ID ( 'RPT_REQUIREMENT_STEPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_STEPS];
GO
CREATE VIEW [RPT_REQUIREMENT_STEPS]
AS
	SELECT	RQS.*,
			REQ.NAME AS REQUIREMENT_NAME,
			REQ.LAST_UPDATE_DATE AS REQUIREMENT_LAST_UPDATE_DATE,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID AS PROJECT_PROJECT_GROUP_ID,
			PRJ.NAME AS PROJECT_NAME
            			
    FROM TST_REQUIREMENT_STEP RQS
		INNER JOIN TST_REQUIREMENT REQ ON RQS.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_REQUIREMENT_TESTCASES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_TESTCASES];
GO
CREATE VIEW [RPT_REQUIREMENT_TESTCASES]
AS
	SELECT RTC.*, REQ.NAME AS REQUIREMENT_NAME, TST.NAME AS TEST_CASE_NAME,
		REQ.PROJECT_ID, PRJ.NAME AS PROJECT_NAME
	FROM TST_REQUIREMENT_TEST_CASE RTC
		INNER JOIN TST_REQUIREMENT REQ ON RTC.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_REQUIREMENT_TESTSTEPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_TESTSTEPS];
GO
CREATE VIEW [RPT_REQUIREMENT_TESTSTEPS]
AS
	SELECT RTS.*, REQ.NAME AS REQUIREMENT_NAME, STP.POSITION, STP.DESCRIPTION AS STEP_DESCRIPTION, STP.EXPECTED_RESULT, STP.SAMPLE_DATA,
		REQ.PROJECT_ID, PRJ.NAME AS PROJECT_NAME
	FROM TST_REQUIREMENT_TEST_STEP RTS
		INNER JOIN TST_REQUIREMENT REQ ON RTS.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		INNER JOIN TST_TEST_STEP STP ON RTS.TEST_STEP_ID = STP.TEST_STEP_ID
		INNER JOIN TST_PROJECT AS PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_REQUIREMENT_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENT_TYPES];
GO
CREATE VIEW [RPT_REQUIREMENT_TYPES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_REQUIREMENT_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_REQUIREMENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_REQUIREMENTS];
GO
CREATE VIEW [RPT_REQUIREMENTS]
AS
	SELECT
		REQ.*,
		RST.NAME AS REQUIREMENT_STATUS_NAME,
		RTP.NAME AS REQUIREMENT_TYPE_NAME,
		CMP.NAME AS COMPONENT_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		IMP.NAME AS IMPORTANCE_NAME,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM TST_REQUIREMENT AS REQ
		INNER JOIN TST_REQUIREMENT_STATUS AS RST ON REQ.REQUIREMENT_STATUS_ID = RST.REQUIREMENT_STATUS_ID
		INNER JOIN TST_REQUIREMENT_TYPE AS RTP ON REQ.REQUIREMENT_TYPE_ID = RTP.REQUIREMENT_TYPE_ID
		LEFT JOIN TST_IMPORTANCE AS IMP ON REQ.IMPORTANCE_ID = IMP.IMPORTANCE_ID
		LEFT JOIN TST_COMPONENT AS CMP ON REQ.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 1) AS ACP ON REQ.REQUIREMENT_ID = ACP.ARTIFACT_ID
		INNER JOIN TST_USER_PROFILE AS USR1 ON REQ.AUTHOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON REQ.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_RELEASE AS REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_RISKS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISKS];
GO
CREATE VIEW [RPT_RISKS]
AS
	SELECT	RSK.*,
			RPB.NAME AS RISK_PROBABILITY_NAME,
			RPB.COLOR AS RISK_PROBABILITY_COLOR,
			RPB.SCORE AS RISK_PROBABILITY_SCORE,
			RIM.NAME AS RISK_IMPACT_NAME,
			RIM.COLOR AS RISK_IMPACT_COLOR,
			RIM.SCORE AS RISK_IMPACT_SCORE,
			(RPB.SCORE * RIM.SCORE) AS RISK_EXPOSURE,
			IST.NAME AS RISK_STATUS_NAME,
			IST.IS_OPEN AS RISK_STATUS_IS_OPEN,
			ITP.NAME AS RISK_TYPE_NAME,
			(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
			(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
            REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
            REL.NAME AS RELEASE_NAME,
            CMP.NAME AS COMPONENT_NAME,
            PGL.NAME AS GOAL_NAME,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID AS PROJECT_PROJECT_GROUP_ID,
			PRJ.NAME AS PROJECT_NAME,
            ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
            ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
            ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_RISK RSK
		LEFT JOIN TST_RISK_PROBABILITY RPB ON RSK.RISK_PROBABILITY_ID = RPB.RISK_PROBABILITY_ID
		INNER JOIN TST_RISK_TYPE ITP ON RSK.RISK_TYPE_ID = ITP.RISK_TYPE_ID
		LEFT JOIN TST_USER_PROFILE USR2 ON RSK.OWNER_ID = USR2.USER_ID
		INNER JOIN TST_RISK_STATUS IST ON RSK.RISK_STATUS_ID = IST.RISK_STATUS_ID
		INNER JOIN TST_USER_PROFILE USR1 ON	RSK.CREATOR_ID = USR1.USER_ID
		INNER JOIN TST_PROJECT PRJ ON RSK.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RISK_IMPACT RIM ON RSK.RISK_IMPACT_ID = RIM.RISK_IMPACT_ID
		LEFT JOIN TST_RELEASE REL ON RSK.RELEASE_ID = REL.RELEASE_ID
		LEFT JOIN TST_COMPONENT CMP ON RSK.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN TST_PROJECT_GOAL PGL ON RSK.GOAL_ID = PGL.GOAL_ID
		LEFT JOIN (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 14) ACP ON RSK.RISK_ID = ACP.ARTIFACT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_RISK_IMPACTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISK_IMPACTS];
GO
CREATE VIEW [RPT_RISK_IMPACTS]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_RISK_IMPACT AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_RISK_MITIGATIONS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISK_MITIGATIONS];
GO
CREATE VIEW [RPT_RISK_MITIGATIONS]
AS
	SELECT	RSM.*,
			RSK.NAME AS RISK_NAME,
			RSK.REVIEW_DATE AS RISK_REVIEW_DATE,
			PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE,
			PRJ.PROJECT_GROUP_ID AS PROJECT_PROJECT_GROUP_ID,
			PRJ.NAME AS PROJECT_NAME
            			
    FROM TST_RISK_MITIGATION RSM
		INNER JOIN TST_RISK RSK ON RSM.RISK_ID = RSK.RISK_ID
		INNER JOIN TST_PROJECT PRJ ON RSK.PROJECT_ID = PRJ.PROJECT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_RISK_PROBABILITIES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISK_PROBABILITIES];
GO
CREATE VIEW [RPT_RISK_PROBABILITIES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_RISK_PROBABILITY AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_RISK_STATUSES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISK_STATUSES];
GO
CREATE VIEW [RPT_RISK_STATUSES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_RISK_STATUS AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_RISK_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_RISK_TYPES];
GO
CREATE VIEW [RPT_RISK_TYPES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_RISK_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_TASKS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TASKS];
GO
CREATE VIEW [RPT_TASKS]
AS
	SELECT	
	TSK.*,
	TKS.NAME AS TASK_STATUS_NAME, 
	(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
	(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME,
	TKP.NAME AS TASK_PRIORITY_NAME,
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
	REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
	REQ.NAME AS REQUIREMENT_NAME,
	TKT.NAME AS TASK_TYPE_NAME,
	REQ.COMPONENT_ID,
	CMP.NAME AS COMPONENT_NAME,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_TASK AS TSK
		INNER JOIN TST_TASK_STATUS AS TKS ON TSK.TASK_STATUS_ID = TKS.TASK_STATUS_ID
		INNER JOIN TST_TASK_TYPE AS TKT ON TSK.TASK_TYPE_ID = TKT.TASK_TYPE_ID
		LEFT JOIN TST_TASK_PRIORITY AS TKP ON TSK.TASK_PRIORITY_ID = TKP.TASK_PRIORITY_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSK.OWNER_ID = USR2.USER_ID 
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSK.CREATOR_ID = USR1.USER_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSK.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE AS REL ON TSK.RELEASE_ID = REL.RELEASE_ID
		LEFT JOIN TST_REQUIREMENT AS REQ ON TSK.REQUIREMENT_ID = REQ.REQUIREMENT_ID
		LEFT JOIN TST_COMPONENT AS CMP ON REQ.COMPONENT_ID = CMP.COMPONENT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 6
			) AS ACP ON TSK.TASK_ID = ACP.ARTIFACT_ID
	WHERE PRJ.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_TASK_PRIORITIES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TASK_PRIORITIES];
GO
CREATE VIEW [RPT_TASK_PRIORITIES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_TASK_PRIORITY AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_TASK_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TASK_TYPES];
GO
CREATE VIEW [RPT_TASK_TYPES]
AS
    SELECT	PROP.TASK_TYPE_ID, PROP.PROJECT_TEMPLATE_ID, PROP.TASK_WORKFLOW_ID, PROP.NAME, PROP.POSITION, PROP.IS_ACTIVE, PROP.IS_DEFAULT, PROP.IS_PULL_REQUEST, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_TASK_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_TESTCASES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTCASES];
GO
CREATE VIEW [RPT_TESTCASES]
AS
	SELECT
		TST.*,
		EXE.NAME AS EXECUTION_STATUS_NAME,
		TP.NAME AS TEST_CASE_PRIORITY_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS AUTHOR_NAME,
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		AEN.NAME AS AUTOMATION_ENGINE_NAME,
		STA.NAME AS TEST_CASE_STATUS_NAME,
		TYP.NAME AS TEST_CASE_TYPE_NAME,
		PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM
		TST_TEST_CASE TST
			LEFT JOIN (
				SELECT *
				FROM TST_ARTIFACT_CUSTOM_PROPERTY
				WHERE ARTIFACT_TYPE_ID = 2
			) AS ACP
		ON TST.TEST_CASE_ID = ACP.ARTIFACT_ID
			INNER JOIN TST_EXECUTION_STATUS AS EXE ON TST.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
			INNER JOIN TST_USER_PROFILE AS USR1 ON TST.AUTHOR_ID = USR1.USER_ID
			INNER JOIN TST_TEST_CASE_STATUS STA ON TST.TEST_CASE_STATUS_ID = STA.TEST_CASE_STATUS_ID
			INNER JOIN TST_TEST_CASE_TYPE TYP ON TST.TEST_CASE_TYPE_ID = TYP.TEST_CASE_TYPE_ID
			LEFT JOIN TST_USER_PROFILE AS USR2 ON TST.OWNER_ID = USR2.USER_ID
			LEFT JOIN TST_TEST_CASE_PRIORITY AS TP ON TST.TEST_CASE_PRIORITY_ID = TP.TEST_CASE_PRIORITY_ID
			LEFT JOIN TST_AUTOMATION_ENGINE AS AEN ON TST.AUTOMATION_ENGINE_ID = AEN.AUTOMATION_ENGINE_ID
			INNER JOIN TST_PROJECT AS PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_TESTCASE_FOLDERS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTCASE_FOLDERS];
GO
CREATE VIEW [RPT_TESTCASE_FOLDERS]
AS
	SELECT
		TST.*,
		PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
	FROM
		TST_TEST_CASE_FOLDER TST
		INNER JOIN TST_PROJECT AS PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_TESTCASE_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTCASE_INCIDENTS];
GO
CREATE VIEW [RPT_TESTCASE_INCIDENTS]
AS
SELECT	TRN.TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
UNION
SELECT	TSP.TEST_CASE_ID AS TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_STEP TSP INNER JOIN TST_ARTIFACT_LINK ARL 
ON		TSP.TEST_STEP_ID = ARL.SOURCE_ARTIFACT_ID INNER JOIN TST_INCIDENT INC
ON		ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 7 AND ARL.DEST_ARTIFACT_TYPE_ID = 3
UNION
SELECT	TSP.TEST_CASE_ID AS TEST_CASE_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_STEP TSP INNER JOIN TST_ARTIFACT_LINK ARL 
ON		TSP.TEST_STEP_ID = ARL.DEST_ARTIFACT_ID INNER JOIN TST_INCIDENT INC
ON		ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = 3 AND ARL.DEST_ARTIFACT_TYPE_ID = 7
GO
IF OBJECT_ID ( 'RPT_TESTCASE_TYPES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTCASE_TYPES];
GO
CREATE VIEW [RPT_TESTCASE_TYPES]
AS
    SELECT	PROP.*, TEMP.NAME AS PROJECT_TEMPLATE_NAME
	FROM TST_TEST_CASE_TYPE AS PROP
	INNER JOIN TST_PROJECT_TEMPLATE AS TEMP ON PROP.PROJECT_TEMPLATE_ID = TEMP.PROJECT_TEMPLATE_ID
    WHERE PROP.IS_ACTIVE = 1
GO
IF OBJECT_ID ( 'RPT_TESTRUNS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTRUNS];
GO
CREATE VIEW [RPT_TESTRUNS]
AS
	SELECT	TRN.*, EXE.NAME AS EXECUTION_STATUS_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS TESTER_NAME,
			REL.NAME AS RELEASE_NAME, REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER, TSE.NAME AS TEST_SET_NAME,
			TRT.NAME AS TEST_RUN_TYPE_NAME,
			AHT.NAME AS AUTOMATION_HOST_NAME,
			BLD.NAME AS BUILD_NAME,
			TST.PROJECT_ID, TST.IS_DELETED As TESTCASEDELETED,
			PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
			ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
			ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
			ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
			ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
			ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
			ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
			ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
			ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
			ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
			ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_CASE TST
	ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID INNER JOIN TST_EXECUTION_STATUS EXE
	ON		TRN.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID INNER JOIN TST_USER_PROFILE USR
	ON		TRN.TESTER_ID = USR.USER_ID LEFT JOIN TST_RELEASE REL
	ON		TRN.RELEASE_ID = REL.RELEASE_ID LEFT JOIN TST_TEST_SET TSE
	ON		TRN.TEST_SET_ID = TSE.TEST_SET_ID INNER JOIN TST_TEST_RUN_TYPE TRT
	ON		TRN.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID LEFT JOIN
				(SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 5) ACP
	ON		TRN.TEST_RUN_ID = ACP.ARTIFACT_ID LEFT JOIN TST_AUTOMATION_HOST AHT
	ON		TRN.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID LEFT JOIN TST_BUILD BLD
    ON		TRN.BUILD_ID = BLD.BUILD_ID INNER JOIN TST_PROJECT AS PRJ
    ON		TST.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	TST.IS_DELETED = 0
GO
IF OBJECT_ID ( 'RPT_TESTRUNSTEPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTRUNSTEPS];
GO
CREATE VIEW [RPT_TESTRUNSTEPS]
AS
	SELECT	
	TRS.*,
	EXE.NAME AS EXECUTION_STATUS_NAME,
	TST.NAME AS TEST_CASE_NAME, TST.PROJECT_ID, 
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID

    FROM TST_TEST_RUN_STEP AS TRS
		INNER JOIN TST_EXECUTION_STATUS AS EXE ON TRS.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		INNER JOIN TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
		INNER JOIN TST_TEST_CASE AS TST ON TRN.TEST_CASE_ID = TST.TEST_CASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
	WHERE PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0
GO
IF OBJECT_ID ( 'RPT_TESTRUN_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTRUN_INCIDENTS];
GO
CREATE VIEW [RPT_TESTRUN_INCIDENTS]
AS
SELECT	TRN.TEST_RUN_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
GO
IF OBJECT_ID ( 'RPT_TESTSETS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTSETS];
GO
CREATE VIEW [RPT_TESTSETS]
AS
	SELECT
		TSE.*,
		REL.VERSION_NUMBER AS RELEASE_VERSION_NUMBER,
		PRJ.NAME AS PROJECT_NAME,
		ISNULL(TSC.TEST_CASE_COUNT,0) AS TEST_CASE_COUNT,
		TSS.NAME AS TEST_SET_STATUS_NAME,
		(RTRIM(USR1.FIRST_NAME + ' ' + ISNULL(USR1.MIDDLE_INITIAL,'')) + ' ' + USR1.LAST_NAME) AS CREATOR_NAME, 
		(RTRIM(USR2.FIRST_NAME + ' ' + ISNULL(USR2.MIDDLE_INITIAL,'')) + ' ' + USR2.LAST_NAME) AS OWNER_NAME,
		(CASE WHEN PRJ.IS_ACTIVE = 1 THEN 'Y' ELSE 'N' END) AS PROJECT_ACTIVE_YN,
		AHT.NAME AS AUTOMATION_HOST_NAME,
		TRT.NAME AS TEST_RUN_TYPE_NAME,
		REC.NAME AS RECURRENCE_NAME,
		PRJ.PROJECT_GROUP_ID,
        ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
        ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
        ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
		ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
		ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
		ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
		ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
		ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
		ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
		ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
	FROM TST_TEST_SET AS TSE
		INNER JOIN TST_USER_PROFILE AS USR1 ON TSE.CREATOR_ID = USR1.USER_ID
		LEFT JOIN TST_USER_PROFILE AS USR2 ON TSE.OWNER_ID = USR2.USER_ID
		LEFT JOIN TST_TEST_SET_STATUS AS TSS ON TSE.TEST_SET_STATUS_ID = TSS.TEST_SET_STATUS_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE ARTIFACT_TYPE_ID = 8) AS ACP ON TSE.TEST_SET_ID = ACP.ARTIFACT_ID
		LEFT JOIN (
			SELECT COUNT(TEST_CASE_ID) AS TEST_CASE_COUNT,
			TEST_SET_ID
			FROM TST_TEST_SET_TEST_CASE GROUP BY TEST_SET_ID) AS TSC ON TSE.TEST_SET_ID = TSC.TEST_SET_ID
		LEFT JOIN TST_RELEASE AS REL ON TSE.RELEASE_ID = REL.RELEASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSE.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_AUTOMATION_HOST AS AHT ON TSE.AUTOMATION_HOST_ID = AHT.AUTOMATION_HOST_ID
		LEFT JOIN TST_TEST_RUN_TYPE AS TRT ON TSE.TEST_RUN_TYPE_ID = TRT.TEST_RUN_TYPE_ID
		LEFT JOIN TST_RECURRENCE AS REC ON TSE.RECURRENCE_ID = REC.RECURRENCE_ID
GO
IF OBJECT_ID ( 'RPT_TESTSET_FOLDERS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTSET_FOLDERS];
GO
CREATE VIEW [RPT_TESTSET_FOLDERS]
AS
	SELECT
		TSF.*,
		PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
	FROM
		TST_TEST_SET_FOLDER TSF
		INNER JOIN TST_PROJECT AS PRJ ON TSF.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_TESTSET_INCIDENTS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTSET_INCIDENTS];
GO
CREATE VIEW [RPT_TESTSET_INCIDENTS]
AS
SELECT	TRN.TEST_SET_ID, INC.INCIDENT_ID, INC.DETECTED_RELEASE_ID, INC.RESOLVED_RELEASE_ID, INC.VERIFIED_RELEASE_ID, IST.IS_OPEN_STATUS
FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_INCIDENT_STATUS IST
ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
WHERE	TRN.TEST_SET_ID IS NOT NULL
GO
IF OBJECT_ID ( 'RPT_TESTSET_TESTCASES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTSET_TESTCASES];
GO
CREATE VIEW [RPT_TESTSET_TESTCASES]
AS
	SELECT TSC.*, TSE.NAME AS TEST_SET_NAME, TST.NAME AS TEST_CASE_NAME,
		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS OWNER_NAME,
		TSE.PROJECT_ID, PRJ.NAME AS PROJECT_NAME
	FROM TST_TEST_SET_TEST_CASE TSC
		LEFT JOIN TST_USER_PROFILE USR ON TSC.OWNER_ID = USR.USER_ID
		INNER JOIN TST_TEST_SET TSE ON TSC.TEST_SET_ID = TSE.TEST_SET_ID
		INNER JOIN TST_TEST_CASE TST ON TSC.TEST_CASE_ID = TST.TEST_CASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TSE.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_TESTSTEPS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TESTSTEPS];
GO
CREATE VIEW [RPT_TESTSTEPS]
AS
	SELECT	
	STP.*,
	EXE.NAME AS EXECUTION_STATUS_NAME,
	TST.NAME AS TEST_CASE_NAME, TST.PROJECT_ID, 
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID,
    ACP.CUST_01, ACP.CUST_02, ACP.CUST_03, ACP.CUST_04, ACP.CUST_05, ACP.CUST_06, ACP.CUST_07, ACP.CUST_08, ACP.CUST_09, ACP.CUST_10,
    ACP.CUST_11, ACP.CUST_12, ACP.CUST_13, ACP.CUST_14, ACP.CUST_15, ACP.CUST_16, ACP.CUST_17, ACP.CUST_18, ACP.CUST_19, ACP.CUST_20,
    ACP.CUST_21, ACP.CUST_22, ACP.CUST_23, ACP.CUST_24, ACP.CUST_25, ACP.CUST_26, ACP.CUST_27, ACP.CUST_28, ACP.CUST_29, ACP.CUST_30,
	ACP.CUST_31, ACP.CUST_32, ACP.CUST_33, ACP.CUST_34, ACP.CUST_35, ACP.CUST_36, ACP.CUST_37, ACP.CUST_38, ACP.CUST_39, ACP.CUST_40,
	ACP.CUST_41, ACP.CUST_42, ACP.CUST_43, ACP.CUST_44, ACP.CUST_45, ACP.CUST_46, ACP.CUST_47, ACP.CUST_48, ACP.CUST_49, ACP.CUST_50,
	ACP.CUST_51, ACP.CUST_52, ACP.CUST_53, ACP.CUST_54, ACP.CUST_55, ACP.CUST_56, ACP.CUST_57, ACP.CUST_58, ACP.CUST_59, ACP.CUST_60,
	ACP.CUST_61, ACP.CUST_62, ACP.CUST_63, ACP.CUST_64, ACP.CUST_65, ACP.CUST_66, ACP.CUST_67, ACP.CUST_68, ACP.CUST_69, ACP.CUST_70,
	ACP.CUST_71, ACP.CUST_72, ACP.CUST_73, ACP.CUST_74, ACP.CUST_75, ACP.CUST_76, ACP.CUST_77, ACP.CUST_78, ACP.CUST_79, ACP.CUST_80,
	ACP.CUST_81, ACP.CUST_82, ACP.CUST_83, ACP.CUST_84, ACP.CUST_85, ACP.CUST_86, ACP.CUST_87, ACP.CUST_88, ACP.CUST_89, ACP.CUST_90,
	ACP.CUST_91, ACP.CUST_92, ACP.CUST_93, ACP.CUST_94, ACP.CUST_95, ACP.CUST_96, ACP.CUST_97, ACP.CUST_98, ACP.CUST_99
    FROM TST_TEST_STEP AS STP
		INNER JOIN TST_EXECUTION_STATUS AS EXE ON STP.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		INNER JOIN TST_TEST_CASE AS TST ON STP.TEST_CASE_ID = TST.TEST_CASE_ID
		INNER JOIN TST_PROJECT AS PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN (
			SELECT *
			FROM TST_ARTIFACT_CUSTOM_PROPERTY
			WHERE ARTIFACT_TYPE_ID = 7
			) AS ACP ON STP.TEST_STEP_ID = ACP.ARTIFACT_ID
	WHERE PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0
GO
IF OBJECT_ID ( 'RPT_TEST_CONFIGURATION_ENTRIES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TEST_CONFIGURATION_ENTRIES];
GO
CREATE VIEW [RPT_TEST_CONFIGURATION_ENTRIES]
AS
	SELECT TCE.*,
	TCS.NAME AS TEST_CONFIGURATION_SET_NAME,
	TCS.IS_ACTIVE AS IS_TEST_CONFIGURATION_SET_ACTIVE,
	CPV.CUSTOM_PROPERTY_VALUE_ID,
	TCP.NAME AS TEST_CASE_PARAMETER_NAME,
	CPV.NAME AS TEST_CASE_PARAMETER_VALUE,
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
    FROM TST_TEST_CONFIGURATION AS TCE
    INNER JOIN TST_TEST_CONFIGURATION_SET TCS ON TCE.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID
	INNER JOIN TST_TEST_CONFIGURATION_SET_PARAMETER AS TSP ON TSP.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID
	INNER JOIN TST_TEST_CASE_PARAMETER AS TCP ON TCP.TEST_CASE_PARAMETER_ID = TSP.TEST_CASE_PARAMETER_ID
    INNER JOIN TST_TEST_CONFIGURATION_PARAMETER_VALUE AS TPV ON
		TCE.TEST_CONFIGURATION_ID = TPV.TEST_CONFIGURATION_ID AND
		TCE.TEST_CONFIGURATION_SET_ID = TCS.TEST_CONFIGURATION_SET_ID AND
		TPV.TEST_CASE_PARAMETER_ID = TSP.TEST_CASE_PARAMETER_ID
	INNER JOIN TST_CUSTOM_PROPERTY_VALUE CPV ON TPV.CUSTOM_PROPERTY_VALUE_ID = CPV.CUSTOM_PROPERTY_VALUE_ID
	INNER JOIN TST_PROJECT AS PRJ ON TCS.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_TEST_CONFIGURATION_SETS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_TEST_CONFIGURATION_SETS];
GO
CREATE VIEW [RPT_TEST_CONFIGURATION_SETS]
AS
	SELECT TCS.*,
	PRJ.NAME AS PROJECT_NAME, PRJ.PROJECT_GROUP_ID
    FROM TST_TEST_CONFIGURATION_SET AS TCS
	INNER JOIN TST_PROJECT AS PRJ ON TCS.PROJECT_ID = PRJ.PROJECT_ID
GO
IF OBJECT_ID ( 'RPT_USERS', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_USERS];
GO
CREATE VIEW [RPT_USERS]
AS
	SELECT	USR.USER_ID, USR.USER_NAME, USR.EMAIL_ADDRESS, USR.IS_ACTIVE, USR.CREATION_DATE,
			USR.LDAP_DN, UPL.FIRST_NAME, UPL.LAST_NAME, UPL.MIDDLE_INITIAL, UPL.DEPARTMENT,
			UPL.LAST_UPDATE_DATE, UPL.TIMEZONE, UPL.LAST_OPENED_PROJECT_ID, USR.IS_APPROVED,
			USR.LAST_LOGIN_DATE, USR.LAST_ACTIVITY_DATE
	FROM TST_USER AS USR
		INNER JOIN TST_USER_PROFILE AS UPL ON USR.USER_ID = UPL.USER_ID
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactLink
-- Description:		Deletes by artifact id
-- =============================================
IF OBJECT_ID ( 'ARTIFACTLINK_DELETE_BY_ARTIFACT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACTLINK_DELETE_BY_ARTIFACT;
GO
CREATE PROCEDURE ARTIFACTLINK_DELETE_BY_ARTIFACT
	@ArtifactTypeId INT,
	@ArtifactId INT
AS
BEGIN
	SET NOCOUNT ON;
	--First delete all artifact links that have the artifact as the source id
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId AND SOURCE_ARTIFACT_ID = @ArtifactId;
	--Now delete all artifact links that have the artifact as the destination id
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId AND DEST_ARTIFACT_ID = @ArtifactId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactLink
-- Description:		Retrieves by artifact for all dest types
-- =============================================
IF OBJECT_ID ( 'ARTIFACTLINK_RETRIEVE_BY_ARTIFACT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACTLINK_RETRIEVE_BY_ARTIFACT;
GO
CREATE PROCEDURE ARTIFACTLINK_RETRIEVE_BY_ARTIFACT
	@ArtifactTypeId INT,
	@ArtifactId INT
AS
BEGIN

	--Direct link to a Requirement
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		REQ.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT REQ
	ON		ARL.DEST_ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 1
    AND    REQ.IS_DELETED = 0
    UNION
	--Direct link from a Requirement
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		REQ.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT REQ
	ON		ARL.SOURCE_ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
	WHERE	ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
	AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 1
    AND    REQ.IS_DELETED = 0
	UNION
	--Direct link to an Incident
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, INC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		INC.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_INCIDENT INC
	ON		ARL.DEST_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_INCIDENT_STATUS STA
    ON		INC.INCIDENT_STATUS_ID = STA.INCIDENT_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 3
    AND		INC.IS_DELETED = 0
    UNION
    --Direct link from an Incident
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, INC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		INC.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_INCIDENT INC
	ON		ARL.SOURCE_ARTIFACT_ID = INC.INCIDENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_INCIDENT_STATUS STA
    ON		INC.INCIDENT_STATUS_ID = STA.INCIDENT_STATUS_ID
	WHERE	ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
	AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 3
    AND    INC.IS_DELETED = 0
    UNION
	--Direct link to a Risk
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, RSK.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		RSK.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_RISK RSK
	ON		ARL.DEST_ARTIFACT_ID = RSK.RISK_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_RISK_STATUS STA
    ON		RSK.RISK_STATUS_ID = STA.RISK_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 14
    AND		RSK.IS_DELETED = 0
    UNION
    --Direct link from a Risk
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, RSK.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		RSK.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_RISK RSK
	ON		ARL.SOURCE_ARTIFACT_ID = RSK.RISK_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_RISK_STATUS STA
    ON		RSK.RISK_STATUS_ID = STA.RISK_STATUS_ID
	WHERE	ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
	AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 14
    AND     RSK.IS_DELETED = 0
    UNION
	--Direct link to a Test Case
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TSC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TSC.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TEST_CASE TSC
	ON		ARL.DEST_ARTIFACT_ID = TSC.TEST_CASE_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_TEST_CASE_STATUS STA
    ON		TSC.TEST_CASE_STATUS_ID = STA.TEST_CASE_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 2
    AND		TSC.IS_DELETED = 0
    UNION
    --Direct link from a Test Case
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TSC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TSC.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TEST_CASE TSC
	ON		ARL.SOURCE_ARTIFACT_ID = TSC.TEST_CASE_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_TEST_CASE_STATUS STA
    ON		TSC.TEST_CASE_STATUS_ID = STA.TEST_CASE_STATUS_ID
	WHERE	ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
	AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 2
    AND     TSC.IS_DELETED = 0
    UNION
	--Direct link to a Task
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TSK.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TSK.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TASK TSK
	ON		ARL.DEST_ARTIFACT_ID = TSK.TASK_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_TASK_STATUS STA
    ON		TSK.TASK_STATUS_ID = STA.TASK_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 6
    AND		TSK.IS_DELETED = 0
    UNION
    --Direct link from a Task
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TSK.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TSK.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TASK TSK
	ON		ARL.SOURCE_ARTIFACT_ID = TSK.TASK_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_TASK_STATUS STA
    ON		TSK.TASK_STATUS_ID = STA.TASK_STATUS_ID
	WHERE	ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
	AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 6
    AND		TSK.IS_DELETED = 0
    UNION
    --Direct link from a Task to a Test Run
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TRN.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TST.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TEST_RUN TRN
	ON		ARL.DEST_ARTIFACT_ID = TRN.TEST_RUN_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_EXECUTION_STATUS STA
    ON		TRN.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID INNER JOIN TST_TEST_CASE TST
    ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 5
    AND		TST.IS_DELETED = 0
    UNION
	--Direct link to a Task
	SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
			ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, TSK.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME,
    		TSK.PROJECT_ID
	FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
	ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TASK TSK
	ON		ARL.DEST_ARTIFACT_ID = TSK.TASK_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_TASK_STATUS STA
    ON		TSK.TASK_STATUS_ID = STA.TASK_STATUS_ID
	WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND		ARL.SOURCE_ARTIFACT_ID = @ArtifactId
	AND		ARL.DEST_ARTIFACT_TYPE_ID = 6
    AND		TSK.IS_DELETED = 0

	--Now we need to handle any 'implicit/indirect' joins

	--For the case of Requirements linked to Incidents (and vice-versa) we need to create a special
	--query that adds the list of incidents linked indirectly to requirements via  test runs and
	--test case coverage relationships. To distinguish them from each other we use a negative number
	--for the artifact-link-id to denote that it's not a direct link that can be edited.
	UNION
	SELECT	-INC.INCIDENT_ID AS ARTIFACT_LINK_ID, INC.INCIDENT_ID AS ARTIFACT_ID, ART.ARTIFACT_TYPE_ID,
			INC.OPENER_ID AS CREATOR_ID, INC.CREATION_DATE, 'Test Run: ' + TRN.NAME AS COMMENT, INC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
			(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
			ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, INC.PROJECT_ID
	FROM	TST_REQUIREMENT REQ INNER JOIN TST_REQUIREMENT_TEST_CASE RTC
	ON		REQ.REQUIREMENT_ID = RTC.REQUIREMENT_ID INNER JOIN TST_TEST_RUN TRN
	ON		RTC.TEST_CASE_ID = TRN.TEST_CASE_ID INNER JOIN TST_TEST_RUN_STEP TRS
	ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI	
    ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
    ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_USER_PROFILE USR
	ON		INC.OPENER_ID = USR.USER_ID INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ART.ARTIFACT_TYPE_ID = 3 INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ALT.ARTIFACT_LINK_TYPE_ID = 3 INNER JOIN TST_INCIDENT_STATUS STA
    ON		INC.INCIDENT_STATUS_ID = STA.INCIDENT_STATUS_ID
	WHERE	@ArtifactTypeId = 1
	AND		REQ.REQUIREMENT_ID = @ArtifactId
    AND		INC.IS_DELETED = 0
	
    --For incidents we also need to pull back any test runs (test run steps) as well as
    --directly linked test steps
    --To avoid the test run id potentially colliding with the requirement id we
    --multiply by 10 and add the artifact type id (1)
    UNION
    SELECT	((-REQ.REQUIREMENT_ID * 10) + 1) AS ARTIFACT_LINK_ID, REQ.REQUIREMENT_ID AS ARTIFACT_ID, ART.ARTIFACT_TYPE_ID,
    		REQ.AUTHOR_ID AS CREATOR_ID, REQ.CREATION_DATE, 'Test Run: ' + TRN.NAME AS COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
			ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, REQ.PROJECT_ID
    FROM	TST_REQUIREMENT REQ INNER JOIN TST_REQUIREMENT_TEST_CASE RTC
    ON		REQ.REQUIREMENT_ID = RTC.REQUIREMENT_ID INNER JOIN TST_TEST_RUN TRN
    ON		RTC.TEST_CASE_ID = TRN.TEST_CASE_ID INNER JOIN TST_TEST_RUN_STEP TRS
    ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI	
    ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
    ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_USER_PROFILE USR
    ON		REQ.AUTHOR_ID = USR.USER_ID INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ART.ARTIFACT_TYPE_ID = 1 INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ALT.ARTIFACT_LINK_TYPE_ID = 3 INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
    WHERE	@ArtifactTypeId = 3
    AND		INC.INCIDENT_ID = @ArtifactId
    AND		REQ.IS_DELETED = 0
    UNION
    SELECT	((-TRS.TEST_RUN_ID * 10) + 5) AS ARTIFACT_LINK_ID, TRS.TEST_RUN_ID AS ARTIFACT_ID, ART.ARTIFACT_TYPE_ID,
    		TRN.TESTER_ID AS CREATOR_ID, TRN.START_DATE, 'Test Run: ' + TRN.NAME AS COMMENT, TRN.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
			ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, TST.PROJECT_ID
    FROM	TST_TEST_RUN_STEP TRS INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
    ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
    ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_TEST_RUN TRN
    ON		TRS.TEST_RUN_ID = TRN.TEST_RUN_ID INNER JOIN TST_USER_PROFILE USR
    ON		TRN.TESTER_ID = USR.USER_ID INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ART.ARTIFACT_TYPE_ID = 5 INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ALT.ARTIFACT_LINK_TYPE_ID = 3 INNER JOIN TST_EXECUTION_STATUS STA
    ON		TRN.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID INNER JOIN TST_TEST_CASE TST
    ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID
    WHERE	@ArtifactTypeId = 3
    AND		INC.INCIDENT_ID = @ArtifactId
    UNION
    SELECT	ARL.ARTIFACT_LINK_ID, ARL.SOURCE_ARTIFACT_ID AS ARTIFACT_ID, ARL.SOURCE_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
    		ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, (TST.NAME + ' (Step ' + CAST (dbo.FN_TESTSTEP_RETRIEVE_STEP_NUMBER(TST.TEST_CASE_ID, STP.TEST_STEP_ID) AS NVARCHAR) + ')') AS ARTIFACT_NAME,
    		ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
			ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, TST.PROJECT_ID
    FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
    ON		ARL.SOURCE_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
    ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_TEST_STEP STP
    ON		ARL.SOURCE_ARTIFACT_ID = STP.TEST_STEP_ID INNER JOIN TST_TEST_CASE TST
    ON		STP.TEST_CASE_ID = TST.TEST_CASE_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ALT.ARTIFACT_LINK_TYPE_ID = 1 INNER JOIN TST_EXECUTION_STATUS STA
    ON		STP.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
    WHERE	@ArtifactTypeId = 3
    AND		ARL.DEST_ARTIFACT_TYPE_ID = @ArtifactTypeId
    AND		ARL.DEST_ARTIFACT_ID = @ArtifactId
    AND		ARL.SOURCE_ARTIFACT_TYPE_ID = 7
    AND		STP.IS_DELETED = 0
    AND		TST.IS_DELETED = 0
	
    --For test steps we also need to pull back any incidents linked indirectly via
    --test runs (test run steps) as well as direct links.
    --To distinguish them from each other we use a negative number
    --for the artifact-link-id to denote that it's not a direct link that can be edited.
    UNION
    SELECT	-INC.INCIDENT_ID AS ARTIFACT_LINK_ID, INC.INCIDENT_ID AS ARTIFACT_ID, ART.ARTIFACT_TYPE_ID,
    		INC.OPENER_ID AS CREATOR_ID, INC.CREATION_DATE, 'Test Run: ' + TRN.NAME AS COMMENT, INC.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
			ALT.ARTIFACT_LINK_TYPE_ID, ALT.REVERSE_NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, INC.PROJECT_ID
    FROM	TST_TEST_RUN TRN INNER JOIN TST_TEST_RUN_STEP TRS
    ON		TRN.TEST_RUN_ID = TRS.TEST_RUN_ID INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
    ON		TRS.TEST_RUN_STEP_ID = TRI.TEST_RUN_STEP_ID INNER JOIN TST_INCIDENT INC
    ON		TRI.INCIDENT_ID = INC.INCIDENT_ID INNER JOIN TST_USER_PROFILE USR
    ON		INC.OPENER_ID = USR.USER_ID INNER JOIN TST_ARTIFACT_TYPE ART
	ON		ART.ARTIFACT_TYPE_ID = 3 INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ALT.ARTIFACT_LINK_TYPE_ID = 3 INNER JOIN TST_EXECUTION_STATUS STA
    ON		TRN.EXECUTION_STATUS_ID = STA.EXECUTION_STATUS_ID
    WHERE	@ArtifactTypeId = 7
    AND		TRS.TEST_STEP_ID = @ArtifactId
    AND		INC.IS_DELETED = 0
    
	-- Finally we sort the result

	ORDER BY
		ARL.CREATION_DATE DESC,
		ARL.ARTIFACT_LINK_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactLink
-- Description:		Retrieves by artifact when dest type specified
-- =============================================
IF OBJECT_ID ( 'ARTIFACTLINK_RETRIEVE_BY_ARTIFACT_WITH_DEST_TYPE', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACTLINK_RETRIEVE_BY_ARTIFACT_WITH_DEST_TYPE;
GO
CREATE PROCEDURE ARTIFACTLINK_RETRIEVE_BY_ARTIFACT_WITH_DEST_TYPE
	@SourceArtifactTypeId INT,
	@SourceArtifactId INT,
	@DestArtifactTypeId INT
AS
BEGIN
    SELECT	ARL.ARTIFACT_LINK_ID, ARL.DEST_ARTIFACT_ID AS ARTIFACT_ID, ARL.DEST_ARTIFACT_TYPE_ID AS ARTIFACT_TYPE_ID,
    		ARL.CREATOR_ID, ARL.CREATION_DATE, ARL.COMMENT, REQ.NAME AS ARTIFACT_NAME, ART.NAME AS ARTIFACT_TYPE_NAME,
    		(RTRIM(USR.FIRST_NAME + ' ' + ISNULL(USR.MIDDLE_INITIAL,'')) + ' ' + USR.LAST_NAME) AS CREATOR_NAME,
    		ALT.ARTIFACT_LINK_TYPE_ID, ALT.NAME AS ARTIFACT_LINK_TYPE_NAME, STA.NAME AS ARTIFACT_STATUS_NAME, REQ.PROJECT_ID
    FROM	TST_ARTIFACT_LINK ARL INNER JOIN TST_ARTIFACT_TYPE ART
    ON		ARL.DEST_ARTIFACT_TYPE_ID = ART.ARTIFACT_TYPE_ID LEFT JOIN TST_USER_PROFILE USR
    ON		ARL.CREATOR_ID = USR.USER_ID INNER JOIN TST_REQUIREMENT REQ
    ON		ARL.DEST_ARTIFACT_ID = REQ.REQUIREMENT_ID INNER JOIN TST_ARTIFACT_LINK_TYPE ALT
    ON		ARL.ARTIFACT_LINK_TYPE_ID = ALT.ARTIFACT_LINK_TYPE_ID INNER JOIN TST_REQUIREMENT_STATUS STA
    ON		REQ.REQUIREMENT_STATUS_ID = STA.REQUIREMENT_STATUS_ID
    WHERE	ARL.SOURCE_ARTIFACT_TYPE_ID = @SourceArtifactTypeId
    AND	ARL.SOURCE_ARTIFACT_ID = @SourceArtifactId
    AND	ARL.DEST_ARTIFACT_TYPE_ID = @DestArtifactTypeId
    ORDER BY ARL.CREATION_DATE DESC, ARL.ARTIFACT_LINK_ID
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Queries the VW_ARTIFACT_LIST view for items, also filtering by user project/artifact-type
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_GLOBAL_SEARCH', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_GLOBAL_SEARCH;
GO
CREATE PROCEDURE ARTIFACT_GLOBAL_SEARCH
	@Keywords NVARCHAR(MAX),
	@ProjectArtifactList NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT
AS
BEGIN
	--Declare keyword table
	DECLARE @KeywordTable TABLE
	(
		ITEM NVARCHAR(MAX)
	)
	--Populate
	INSERT @KeywordTable (ITEM)
	SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@Keywords, ',')
	
	IF @ProjectArtifactList = '' OR @ProjectArtifactList IS NULL
	BEGIN
		--Get the list of artifacts that matches the search
		SELECT TOP(@NumRows) * FROM
		(
			SELECT 
				VAL.ARTIFACT_ID, VAL.ARTIFACT_TYPE_ID, VAL.PROJECT_ID, VAL.NAME,
				SUBSTRING(VAL.DESCRIPTION,1,3999) AS DESCRIPTION, VAL.IS_DELETED,
				VAL.CREATION_DATE, VAL.PROJECT_NAME,
				VAL.LAST_UPDATE_DATE, VAL.RANK, ROW_NUMBER() OVER
				(ORDER BY VAL.LAST_UPDATE_DATE DESC) AS ROW_NUM
			FROM VW_ARTIFACT_LIST VAL
			WHERE IS_DELETED = 0
			AND NOT EXISTS
			(
				SELECT NULL
				FROM    @KeywordTable KWD
				WHERE   VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
				AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
			)
		) AS VAL
		WHERE ROW_NUM >= @StartRow
		ORDER BY VAL.LAST_UPDATE_DATE DESC
	END
	ELSE
	BEGIN
		--Declare
		DECLARE @ProjectArtifactTable TABLE
		(
			PROJECT_ID INT,
			ARTIFACT_TYPE_ID INT
		)
		DECLARE @ProjectTable TABLE
		(
			PROJECT_ID INT
		)
		--Populate
		INSERT @ProjectArtifactTable (PROJECT_ID, ARTIFACT_TYPE_ID)
		SELECT PROJECT_ID, ARTIFACT_TYPE_ID FROM FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE(@ProjectArtifactList)
		INSERT @ProjectTable (PROJECT_ID)
		SELECT DISTINCT PROJECT_ID FROM @ProjectArtifactTable
		
		--Get the list of artifacts that matches the search
		--and is an allowed project/artifact type id combination
		SELECT TOP(@NumRows) * FROM
		(
			SELECT 
				VAL.ARTIFACT_ID, VAL.ARTIFACT_TYPE_ID, VAL.PROJECT_ID, VAL.NAME,
				SUBSTRING(VAL.DESCRIPTION,1,3999) AS DESCRIPTION, VAL.IS_DELETED,
				VAL.CREATION_DATE, VAL.PROJECT_NAME,
				VAL.LAST_UPDATE_DATE, VAL.RANK, ROW_NUMBER() OVER
				(ORDER BY VAL.LAST_UPDATE_DATE DESC) AS ROW_NUM
			FROM VW_ARTIFACT_LIST VAL
			INNER JOIN @ProjectTable PAT ON VAL.PROJECT_ID = PAT.PROJECT_ID
			WHERE IS_DELETED = 0
			AND EXISTS
			(
				SELECT 1
				FROM    @ProjectArtifactTable PAL
				WHERE   VAL.PROJECT_ID = PAL.PROJECT_ID
				AND VAL.ARTIFACT_TYPE_ID = PAL.ARTIFACT_TYPE_ID
			)
			AND NOT EXISTS
			(
				SELECT NULL
				FROM    @KeywordTable KWD
				WHERE   VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
				AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
			)
		) AS VAL
		WHERE ROW_NUM >= @StartRow
		ORDER BY VAL.LAST_UPDATE_DATE DESC
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Counts the VW_ARTIFACT_LIST view for items, also filtering by user project/artifact-type
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_GLOBAL_SEARCH_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_GLOBAL_SEARCH_COUNT;
GO
CREATE PROCEDURE ARTIFACT_GLOBAL_SEARCH_COUNT
	@Keywords NVARCHAR(MAX),
	@ProjectArtifactList NVARCHAR(MAX)
AS
BEGIN
	--Declare keyword table
	DECLARE @KeywordTable TABLE
	(
		ITEM NVARCHAR(MAX)
	)
	--Populate
	INSERT @KeywordTable (ITEM)
	SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@Keywords, ',')
	
	--Get the list of artifacts that matches the search
	--and is an allowed project/artifact type id combination
	IF @ProjectArtifactList = '' OR @ProjectArtifactList IS NULL
	BEGIN
		SELECT COUNT (*) AS SEARCH_COUNT FROM VW_ARTIFACT_LIST VAL
		WHERE IS_DELETED = 0
		AND NOT EXISTS
		(
			SELECT NULL
			FROM    @KeywordTable KWD
			WHERE	VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
			AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
		)	
	END
	ELSE
	BEGIN
		--Declare
		DECLARE @ProjectArtifactTable TABLE
		(
			PROJECT_ID INT,
			ARTIFACT_TYPE_ID INT
		)
		DECLARE @ProjectTable TABLE
		(
			PROJECT_ID INT
		)
		--Populate
		INSERT @ProjectArtifactTable (PROJECT_ID, ARTIFACT_TYPE_ID)
		SELECT PROJECT_ID, ARTIFACT_TYPE_ID FROM FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE(@ProjectArtifactList)
		INSERT @ProjectTable (PROJECT_ID)
		SELECT DISTINCT PROJECT_ID FROM @ProjectArtifactTable
		
		SELECT COUNT (*) AS SEARCH_COUNT FROM VW_ARTIFACT_LIST VAL
		INNER JOIN @ProjectTable PAT ON VAL.PROJECT_ID = PAT.PROJECT_ID
		WHERE IS_DELETED = 0
		AND EXISTS
		(
			SELECT 1
			FROM    @ProjectArtifactTable PAL
			WHERE   VAL.PROJECT_ID = PAL.PROJECT_ID
			AND VAL.ARTIFACT_TYPE_ID = PAL.ARTIFACT_TYPE_ID
		)
		AND NOT EXISTS
		(
			SELECT NULL
			FROM    @KeywordTable KWD
			WHERE	VAL.NAME NOT LIKE '%' + KWD.ITEM + '%'
			AND (VAL.DESCRIPTION IS NULL OR SUBSTRING(VAL.DESCRIPTION,1,3999) NOT LIKE '%' + KWD.ITEM + '%')
		)	
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Queries the VW_ARTIFACT_LIST view for items, also filtering by user project/artifact-type
--					This version uses the SQL freetext indexes
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_GLOBAL_SEARCH_FREETEXT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT;
GO
CREATE PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT
	@SearchString NVARCHAR(4000),
	@ProjectArtifactList NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT
AS
BEGIN
	--Dummy Proc Definition
	SELECT 1 FROM DUMMY
END
GO
--See if Free Text Indexing is Installed
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
	EXEC('
ALTER PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT
	@SearchString NVARCHAR(4000),
	@ProjectArtifactList NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT
AS
BEGIN
	--Declare
	DECLARE @ProjectArtifactTable TABLE
	(
		PROJECT_ID INT,
		ARTIFACT_TYPE_ID INT
	)

	IF @ProjectArtifactList <> '''' AND @ProjectArtifactList IS NOT NULL
	BEGIN
		--Populate
		INSERT @ProjectArtifactTable (PROJECT_ID, ARTIFACT_TYPE_ID)
		SELECT PROJECT_ID, ARTIFACT_TYPE_ID FROM FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE(@ProjectArtifactList)
	END

	SELECT	ARTIFACT_TYPE_ID, ARTIFACT_ID, PROJECT_ID, NAME, DESCRIPTION, CREATION_DATE, LAST_UPDATE_DATE, PROJECT_NAME, RANK, CAST (0 AS BIT) AS IS_DELETED
	FROM
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY RANK DESC,  ARTIFACT_TYPE_ID, ARTIFACT_ID) AS ROW_NUM, ARTIFACT_TYPE_ID, ARTIFACT_ID, PROJECT_ID, NAME, DESCRIPTION, CREATION_DATE, LAST_UPDATE_DATE, PROJECT_NAME, RANK
		FROM
		(
			SELECT	1 AS ARTIFACT_TYPE_ID, ART.REQUIREMENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_REQUIREMENT ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_REQUIREMENT, *, @SearchString) AS CT
				ON ART.REQUIREMENT_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	2 AS ARTIFACT_TYPE_ID, ART.TEST_CASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_TEST_CASE ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_TEST_CASE, *, @SearchString) AS CT
				ON ART.TEST_CASE_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	3 AS ARTIFACT_TYPE_ID, ART.INCIDENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_INCIDENT ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_INCIDENT, *, @SearchString) AS CT
				ON ART.INCIDENT_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	4 AS ARTIFACT_TYPE_ID, ART.RELEASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_RELEASE ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_RELEASE, *, @SearchString) AS CT
				ON ART.RELEASE_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	5 AS ARTIFACT_TYPE_ID, ART.TEST_RUN_ID AS ARTIFACT_ID, TST.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.START_DATE AS CREATION_DATE, ART.END_DATE AS LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_TEST_RUN ART
				INNER JOIN FREETEXTTABLE (TST_TEST_RUN, *, @SearchString) AS CT ON ART.TEST_RUN_ID = CT.[KEY] 
				INNER JOIN TST_TEST_CASE TST ON ART.TEST_CASE_ID = TST.TEST_CASE_ID
				INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID				 					
			WHERE	PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0
			UNION
			SELECT	6 AS ARTIFACT_TYPE_ID, ART.TASK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_TASK ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_TASK, *, @SearchString) AS CT
				ON ART.TASK_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	7 AS ARTIFACT_TYPE_ID, ART.TEST_STEP_ID AS ARTIFACT_ID, TST.PROJECT_ID, TST.NAME,
					ART.DESCRIPTION, TST.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_TEST_STEP ART
				INNER JOIN FREETEXTTABLE (TST_TEST_STEP, *, @SearchString) AS CT ON ART.TEST_STEP_ID = CT.[KEY] 
				INNER JOIN TST_TEST_CASE TST ON ART.TEST_CASE_ID = TST.TEST_CASE_ID
				INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID				 					
			WHERE	PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0 AND ART.IS_DELETED = 0
			UNION
			SELECT	8 AS ARTIFACT_TYPE_ID, ART.TEST_SET_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_TEST_SET ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_TEST_SET, *, @SearchString) AS CT
				ON ART.TEST_SET_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	9 AS ARTIFACT_TYPE_ID, ART.AUTOMATION_HOST_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.LAST_UPDATE_DATE AS CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_AUTOMATION_HOST ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_AUTOMATION_HOST, *, @SearchString) AS CT
				ON ART.AUTOMATION_HOST_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
			UNION
			SELECT	13 AS ARTIFACT_TYPE_ID, ART.ATTACHMENT_ID AS ARTIFACT_ID, PAT.PROJECT_ID, ART.FILENAME AS NAME,
					ART.DESCRIPTION, ART.UPLOAD_DATE AS CREATION_DATE, ART.EDITED_DATE AS LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_ATTACHMENT ART
				INNER JOIN TST_PROJECT_ATTACHMENT PAT ON ART.ATTACHMENT_ID = PAT.ATTACHMENT_ID
				INNER JOIN TST_PROJECT PRJ ON PAT.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_ATTACHMENT, *, @SearchString) AS CT
				ON ART.ATTACHMENT_ID = CT.[KEY]  
			WHERE	PRJ.IS_ACTIVE = 1
			UNION
			SELECT	14 AS ARTIFACT_TYPE_ID, ART.RISK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
					ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
			FROM	TST_RISK ART
				INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
				INNER JOIN FREETEXTTABLE (TST_RISK, *, @SearchString) AS CT
				ON ART.RISK_ID = CT.[KEY]  					
			WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		)
		AS RES
		WHERE @ProjectArtifactList = '''' OR @ProjectArtifactList IS NULL OR EXISTS
		(
			SELECT 1
			FROM    @ProjectArtifactTable PAL
			WHERE   RES.PROJECT_ID = PAL.PROJECT_ID
			AND		RES.ARTIFACT_TYPE_ID = PAL.ARTIFACT_TYPE_ID
		)
	) AS RES
	WHERE	ROW_NUM >= @StartRow AND ROW_NUM < (@StartRow + @NumRows)
	ORDER BY ROW_NUM	
END
');
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Counts the VW_ARTIFACT_LIST view for items, also filtering by user project/artifact-type
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_GLOBAL_SEARCH_FREETEXT_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT_COUNT;
GO
CREATE PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT_COUNT
	@SearchString NVARCHAR(4000),
	@ProjectArtifactList NVARCHAR(MAX)
AS
BEGIN
	--Dummy Proc Definition
	SELECT 1 FROM DUMMY
END
GO
--See if Free Text Indexing is Installed
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
	EXEC('
ALTER PROCEDURE ARTIFACT_GLOBAL_SEARCH_FREETEXT_COUNT
	@SearchString NVARCHAR(4000),
	@ProjectArtifactList NVARCHAR(MAX)
AS
BEGIN
	--Declare
	DECLARE @ProjectArtifactTable TABLE
	(
		PROJECT_ID INT,
		ARTIFACT_TYPE_ID INT
	)

	IF @ProjectArtifactList <> '''' AND @ProjectArtifactList IS NOT NULL
	BEGIN
		--Populate
		INSERT @ProjectArtifactTable (PROJECT_ID, ARTIFACT_TYPE_ID)
		SELECT PROJECT_ID, ARTIFACT_TYPE_ID FROM FN_ARTIFACT_PROJECTS_TYPES_TO_TABLE(@ProjectArtifactList)
	END
	
	SELECT	COUNT(*) AS SEARCH_COUNT
	FROM
	(
		SELECT	1 AS ARTIFACT_TYPE_ID, ART.REQUIREMENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_REQUIREMENT ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_REQUIREMENT, *, @SearchString) AS CT
			ON ART.REQUIREMENT_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	2 AS ARTIFACT_TYPE_ID, ART.TEST_CASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_TEST_CASE ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_TEST_CASE, *, @SearchString) AS CT
			ON ART.TEST_CASE_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	3 AS ARTIFACT_TYPE_ID, ART.INCIDENT_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_INCIDENT ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_INCIDENT, *, @SearchString) AS CT
			ON ART.INCIDENT_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	4 AS ARTIFACT_TYPE_ID, ART.RELEASE_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_RELEASE ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_RELEASE, *, @SearchString) AS CT
			ON ART.RELEASE_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	5 AS ARTIFACT_TYPE_ID, ART.TEST_RUN_ID AS ARTIFACT_ID, TST.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.START_DATE AS CREATION_DATE, ART.END_DATE AS LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_TEST_RUN ART
			INNER JOIN FREETEXTTABLE (TST_TEST_RUN, *, @SearchString) AS CT ON ART.TEST_RUN_ID = CT.[KEY] 
			INNER JOIN TST_TEST_CASE TST ON ART.TEST_CASE_ID = TST.TEST_CASE_ID
			INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID				 					
		WHERE	PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0
		UNION
		SELECT	6 AS ARTIFACT_TYPE_ID, ART.TASK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_TASK ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_TASK, *, @SearchString) AS CT
			ON ART.TASK_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	7 AS ARTIFACT_TYPE_ID, ART.TEST_STEP_ID AS ARTIFACT_ID, TST.PROJECT_ID, TST.NAME,
				ART.DESCRIPTION, TST.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_TEST_STEP ART
			INNER JOIN FREETEXTTABLE (TST_TEST_STEP, *, @SearchString) AS CT ON ART.TEST_STEP_ID = CT.[KEY] 
			INNER JOIN TST_TEST_CASE TST ON ART.TEST_CASE_ID = TST.TEST_CASE_ID
			INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID				 					
		WHERE	PRJ.IS_ACTIVE = 1 AND TST.IS_DELETED = 0 AND ART.IS_DELETED = 0
		UNION
		SELECT	8 AS ARTIFACT_TYPE_ID, ART.TEST_SET_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_TEST_SET ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_TEST_SET, *, @SearchString) AS CT
			ON ART.TEST_SET_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	9 AS ARTIFACT_TYPE_ID, ART.AUTOMATION_HOST_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.LAST_UPDATE_DATE AS CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_AUTOMATION_HOST ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_AUTOMATION_HOST, *, @SearchString) AS CT
			ON ART.AUTOMATION_HOST_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
		UNION
		SELECT	13 AS ARTIFACT_TYPE_ID, ART.ATTACHMENT_ID AS ARTIFACT_ID, PAT.PROJECT_ID, ART.FILENAME AS NAME,
				ART.DESCRIPTION, ART.UPLOAD_DATE AS CREATION_DATE, ART.EDITED_DATE AS LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_ATTACHMENT ART
			INNER JOIN TST_PROJECT_ATTACHMENT PAT ON ART.ATTACHMENT_ID = PAT.ATTACHMENT_ID
			INNER JOIN TST_PROJECT PRJ ON PAT.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_ATTACHMENT, *, @SearchString) AS CT
			ON ART.ATTACHMENT_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1
		UNION
		SELECT	14 AS ARTIFACT_TYPE_ID, ART.RISK_ID AS ARTIFACT_ID, ART.PROJECT_ID, ART.NAME,
				ART.DESCRIPTION, ART.CREATION_DATE, ART.LAST_UPDATE_DATE, PRJ.NAME PROJECT_NAME, CT.RANK
		FROM	TST_RISK ART
			INNER JOIN TST_PROJECT PRJ ON ART.PROJECT_ID = PRJ.PROJECT_ID
			INNER JOIN FREETEXTTABLE (TST_RISK, *, @SearchString) AS CT
			ON ART.RISK_ID = CT.[KEY]  					
		WHERE	PRJ.IS_ACTIVE = 1 AND ART.IS_DELETED = 0
	)
	AS RES
	WHERE @ProjectArtifactList = '''' OR @ProjectArtifactList IS NULL OR EXISTS
	(
		SELECT 1
		FROM    @ProjectArtifactTable PAL
		WHERE   RES.PROJECT_ID = PAL.PROJECT_ID
		AND		RES.ARTIFACT_TYPE_ID = PAL.ARTIFACT_TYPE_ID
	)
END
');
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Retrieves the list of fields and custom properties configured
--					for a specific artifact and project
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_RETRIEVE_ALL_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_RETRIEVE_ALL_FIELDS;
GO
CREATE PROCEDURE ARTIFACT_RETRIEVE_ALL_FIELDS
	@ProjectId INT,
	@ArtifactTypeId INT,
	@CustumPropPrefix NVARCHAR(50)
AS
BEGIN
	--Get the list of fields and custom properties
	SELECT	ARF.NAME, ARF.CAPTION, ARF.ARTIFACT_FIELD_TYPE_ID, ARF.LOOKUP_PROPERTY, ARF.IS_HISTORY_RECORDED
	FROM	TST_ARTIFACT_FIELD ARF
	WHERE 	ARF.IS_ACTIVE = 1
	AND	ARF.ARTIFACT_TYPE_ID = @ArtifactTypeId
	UNION
	SELECT	@CustumPropPrefix + REPLACE(STR(CPR.PROPERTY_NUMBER, 2), SPACE(1), '0') AS NAME, CPR.NAME AS CAPTION,
			CPT.ARTIFACT_FIELD_TYPE_ID, NULL AS LOOKUP_PROPERTY, CAST(0 AS BIT) AS IS_HISTORY_RECORDED
	FROM	TST_CUSTOM_PROPERTY CPR
			INNER JOIN TST_CUSTOM_PROPERTY_TYPE CPT ON CPR.CUSTOM_PROPERTY_TYPE_ID = CPT.CUSTOM_PROPERTY_TYPE_ID
			INNER JOIN TST_PROJECT PRJ ON CPR.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
	WHERE 	CPR.IS_DELETED = 0
	AND	CPR.ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND PRJ.PROJECT_ID = @ProjectId
	ORDER BY NAME
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ArtifactManager
-- Description:		Retrieves the list of fields and their position for
--					a specific artifact, user and project
-- =====================================================================
IF OBJECT_ID ( 'ARTIFACT_RETRIEVE_LIST_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE ARTIFACT_RETRIEVE_LIST_FIELDS;
GO
CREATE PROCEDURE ARTIFACT_RETRIEVE_LIST_FIELDS
	@ProjectId INT,
	@UserId INT,
	@ArtifactTypeId INT,
	@CustumPropPrefix NVARCHAR(50)
AS
BEGIN
	--Get the list of fields and custom properties
	SELECT	ARF.NAME, ARF.CAPTION, ARTIFACT_FIELD_TYPE_ID, ISNULL(UAF.IS_VISIBLE, ARF.IS_LIST_DEFAULT) AS IS_VISIBLE,
			ISNULL(UAF.LIST_POSITION, ARF.LIST_DEFAULT_POSITION) AS LIST_POSITION, ARF.LOOKUP_PROPERTY, ARF.IS_HISTORY_RECORDED,
			UAF.WIDTH
	FROM	TST_ARTIFACT_FIELD ARF LEFT JOIN
		(SELECT ARTIFACT_FIELD_ID, IS_VISIBLE, LIST_POSITION, WIDTH FROM TST_USER_ARTIFACT_FIELD WHERE PROJECT_ID = @ProjectId AND USER_ID = @UserId) UAF
	ON		ARF.ARTIFACT_FIELD_ID = UAF.ARTIFACT_FIELD_ID
	WHERE 	ARF.IS_ACTIVE = 1
	AND 	ARF.IS_LIST_CONFIG = 1
	AND	ARF.ARTIFACT_TYPE_ID = @ArtifactTypeId
	UNION
	SELECT	@CustumPropPrefix + REPLACE(STR(CPR.PROPERTY_NUMBER, 2), SPACE(1), '0') AS NAME, CPR.NAME AS CAPTION,  CPT.ARTIFACT_FIELD_TYPE_ID,
			ISNULL(UCP.IS_VISIBLE, 0) AS IS_VISIBLE, ISNULL(UCP.LIST_POSITION, (CPR.PROPERTY_NUMBER + 50)) AS LIST_POSITION, NULL AS LOOKUP_PROPERTY,
			CAST(0 AS BIT) AS IS_HISTORY_RECORDED, UCP.WIDTH
	FROM	TST_CUSTOM_PROPERTY CPR INNER JOIN TST_CUSTOM_PROPERTY_TYPE CPT
	ON		CPR.CUSTOM_PROPERTY_TYPE_ID = CPT.CUSTOM_PROPERTY_TYPE_ID LEFT JOIN
		(SELECT CUSTOM_PROPERTY_ID, IS_VISIBLE, LIST_POSITION, WIDTH FROM TST_USER_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId AND USER_ID = @UserId) UCP
	ON		UCP.CUSTOM_PROPERTY_ID = CPR.CUSTOM_PROPERTY_ID INNER JOIN TST_PROJECT PRJ
	ON		CPR.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
	WHERE 	CPR.IS_DELETED = 0
	AND	CPR.ARTIFACT_TYPE_ID = @ArtifactTypeId
	AND PRJ.PROJECT_ID = @ProjectId
	ORDER BY IS_VISIBLE, LIST_POSITION
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Deletes an attachment
-- Remarks:			Only pass a project ID if you just want to remove from the project and not the entire system
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_DELETE;
GO
CREATE PROCEDURE ATTACHMENT_DELETE
	@AttachmentId INT,
	@ProjectId INT
AS
BEGIN
	IF @ProjectId IS NULL
	BEGIN
		--Unlink from any test cases (used for automation scripts)
		UPDATE TST_TEST_CASE SET AUTOMATION_ATTACHMENT_ID = NULL WHERE AUTOMATION_ATTACHMENT_ID = @AttachmentId
		
		--Now we need to remove the reference to the file from project attachment table
		DELETE FROM TST_PROJECT_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId

		--Delete any tags associated with the attachment
		DELETE FROM TST_ARTIFACT_TAGS WHERE ARTIFACT_ID = @AttachmentId AND ARTIFACT_TYPE_ID = /*Document*/13

		--Finally delete the attachment record itself
		DELETE FROM TST_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId
	END
	ELSE
	BEGIN
		--Unlink from any test cases (used for automation scripts)
		UPDATE TST_TEST_CASE SET AUTOMATION_ATTACHMENT_ID = NULL WHERE AUTOMATION_ATTACHMENT_ID = @AttachmentId AND PROJECT_ID = @ProjectId
			
        --We just delete the association to the project
		DELETE FROM TST_PROJECT_ATTACHMENT WHERE ATTACHMENT_ID = @AttachmentId AND PROJECT_ID = @ProjectId
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Deletes all the folders (no longer deletes attachmenet types since they are template based)
-- Remarks:			Typically used when deleting the entire project
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT;
GO
CREATE PROCEDURE ATTACHMENT_DELETE_TYPES_FOLDERS_FOR_PROJECT
	@ProjectId INT
AS
BEGIN
	--Now delete all the document folders in the project
    DELETE FROM TST_PROJECT_ATTACHMENT_FOLDER WHERE PROJECT_ID = @ProjectId;

END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Refreshes the folder hierarchy for the specified project
-- =====================================================================
IF OBJECT_ID ( 'ATTACHMENT_REFRESH_FOLDER_HIERARCHY', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_REFRESH_FOLDER_HIERARCHY;
GO
CREATE PROCEDURE ATTACHMENT_REFRESH_FOLDER_HIERARCHY
	@ProjectId INT
AS
BEGIN
	--First delete the existing folders
	DELETE FROM TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY
	WHERE PROJECT_ID = @ProjectId;
	
	WITH PROJECT_ATTACHMENT_FOLDER_HIERARCHY (PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, NAME, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	AS
	(
		SELECT	TKF.PROJECT_ATTACHMENT_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_PROJECT_ATTACHMENT_FOLDER_ID, 1 AS HIERARCHY_LEVEL,
				CAST(dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS NVARCHAR(MAX)) AS INDENT_LEVEL
		FROM TST_PROJECT_ATTACHMENT_FOLDER TKF
		WHERE TKF.PARENT_PROJECT_ATTACHMENT_FOLDER_ID IS NULL AND TKF.PROJECT_ID = @ProjectId
		UNION ALL
		SELECT	TKF.PROJECT_ATTACHMENT_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_PROJECT_ATTACHMENT_FOLDER_ID, (CTE.HIERARCHY_LEVEL + 1) AS HIERARCHY_LEVEL,
				CTE.INDENT_LEVEL + dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS INDENT_LEVEL
		FROM TST_PROJECT_ATTACHMENT_FOLDER TKF
		INNER JOIN PROJECT_ATTACHMENT_FOLDER_HIERARCHY CTE ON TKF.PARENT_PROJECT_ATTACHMENT_FOLDER_ID = CTE.PROJECT_ATTACHMENT_FOLDER_ID
		WHERE TKF.PARENT_PROJECT_ATTACHMENT_FOLDER_ID IS NOT NULL AND TKF.PROJECT_ID = @ProjectId
	)
	INSERT INTO TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY (PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, NAME, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	SELECT ISNULL(PROJECT_ATTACHMENT_FOLDER_ID, 0) AS PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, NAME, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM PROJECT_ATTACHMENT_FOLDER_HIERARCHY
	ORDER BY PROJECT_ID, INDENT_LEVEL COLLATE Latin1_General_BIN
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Removes an attachment from an artifact
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_REMOVE_FROM_ARTIFACT', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_REMOVE_FROM_ARTIFACT;
GO
CREATE PROCEDURE ATTACHMENT_REMOVE_FROM_ARTIFACT
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@AttachmentId INT,
	@ProjectId INT
AS
BEGIN
	DELETE FROM TST_ARTIFACT_ATTACHMENT
	WHERE (ATTACHMENT_ID = @AttachmentId OR @AttachmentId IS NULL)
		AND (ARTIFACT_ID = @ArtifactId OR @ArtifactId IS NULL)
		AND (ARTIFACT_TYPE_ID = @ArtifactTypeId OR @ArtifactTypeId IS NULL)
		AND (PROJECT_ID = @ProjectId OR @ProjectId IS NULL)
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
--                  Updated and improved 2/7/2020 By SWB
-- =====================================================================
IF OBJECT_ID ( 'ATTACHMENT_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [ATTACHMENT_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [ATTACHMENT_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY WHERE PROJECT_ATTACHMENT_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT PROJECT_ATTACHMENT_FOLDER_ID AS PROJECT_ATTACHMENT_FOLDER_ID, PROJECT_ID, NAME, PARENT_PROJECT_ATTACHMENT_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Attachment
-- Description:		Updates the attachment flag of an artifact
-- =============================================
IF OBJECT_ID ( 'ATTACHMENT_UPDATE_ARTIFACT_FLAG', 'P' ) IS NOT NULL 
    DROP PROCEDURE ATTACHMENT_UPDATE_ARTIFACT_FLAG;
GO
CREATE PROCEDURE ATTACHMENT_UPDATE_ARTIFACT_FLAG
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@HasAttachments BIT
AS
BEGIN
    IF @ArtifactTypeId = 1
	BEGIN
		UPDATE TST_REQUIREMENT SET IS_ATTACHMENTS = @HasAttachments WHERE REQUIREMENT_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 2
	BEGIN
		UPDATE TST_TEST_CASE SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_CASE_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 4
	BEGIN
		UPDATE TST_RELEASE SET IS_ATTACHMENTS = @HasAttachments WHERE RELEASE_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 3
	BEGIN
        UPDATE TST_INCIDENT SET IS_ATTACHMENTS = @HasAttachments WHERE INCIDENT_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 6
	BEGIN
        UPDATE TST_TASK SET IS_ATTACHMENTS = @HasAttachments WHERE TASK_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 7
	BEGIN
        UPDATE TST_TEST_STEP SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_STEP_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 8
	BEGIN
        UPDATE TST_TEST_SET SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_SET_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 5
	BEGIN
        UPDATE TST_TEST_RUN SET IS_ATTACHMENTS = @HasAttachments WHERE TEST_RUN_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 9
	BEGIN
        UPDATE TST_AUTOMATION_HOST SET IS_ATTACHMENTS = @HasAttachments WHERE AUTOMATION_HOST_ID = @ArtifactId
	END
    ELSE IF @ArtifactTypeId = 14
	BEGIN
        UPDATE TST_RISK SET IS_ATTACHMENTS = @HasAttachments WHERE RISK_ID = @ArtifactId
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Automation
-- Description:		Deletes an Automation Engine
-- =============================================
IF OBJECT_ID ( 'AUTOMATION_DELETE_ENGINE', 'P' ) IS NOT NULL 
    DROP PROCEDURE AUTOMATION_DELETE_ENGINE;
GO
CREATE PROCEDURE AUTOMATION_DELETE_ENGINE
	@AutomationEngineId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Remove references to the engine
	UPDATE TST_TEST_CASE SET AUTOMATION_ENGINE_ID = NULL WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;
	UPDATE TST_TEST_RUN SET AUTOMATION_ENGINE_ID = NULL WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;

	--Delete the engine
    DELETE FROM TST_AUTOMATION_ENGINE WHERE AUTOMATION_ENGINE_ID = @AutomationEngineId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Automation
-- Description:		Deletes an Automation Host
-- =============================================
IF OBJECT_ID ( 'AUTOMATION_DELETE_HOST', 'P' ) IS NOT NULL 
    DROP PROCEDURE AUTOMATION_DELETE_HOST;
GO
CREATE PROCEDURE AUTOMATION_DELETE_HOST
	@AutomationHostId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Remove references to the engine
	UPDATE TST_TEST_SET SET AUTOMATION_HOST_ID = NULL WHERE AUTOMATION_HOST_ID = @AutomationHostId;
	UPDATE TST_TEST_RUN SET AUTOMATION_HOST_ID = NULL WHERE AUTOMATION_HOST_ID = @AutomationHostId;

	--Delete the engine
    DELETE FROM TST_AUTOMATION_HOST WHERE AUTOMATION_HOST_ID = @AutomationHostId;
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a list containing all the data-mappings for a specific artifact
--					Also returns unmapped system records
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_ARTIFACT_MAPPINGS]
	@ProjectId INT,
	@ArtifactTypeId INT,
	@ArtifactId INT
AS
BEGIN
    SELECT	DSS.DATA_SYNC_SYSTEM_ID, @ArtifactTypeId AS ARTIFACT_TYPE_ID, @ArtifactId AS ARTIFACT_ID,
			@ProjectId AS PROJECT_ID, DAM.EXTERNAL_KEY, DSS.NAME AS DATA_SYNC_SYSTEM_NAME
    FROM	(SELECT * FROM TST_DATA_SYNC_ARTIFACT_MAPPING WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId AND PROJECT_ID = @ProjectId)
			DAM RIGHT JOIN TST_DATA_SYNC_PROJECT DSP
    ON     DAM.DATA_SYNC_SYSTEM_ID = DSP.DATA_SYNC_SYSTEM_ID AND DAM.PROJECT_ID = DSP.PROJECT_ID INNER JOIN TST_DATA_SYNC_SYSTEM DSS
    ON     DSP.DATA_SYNC_SYSTEM_ID = DSS.DATA_SYNC_SYSTEM_ID
    WHERE  DSP.ACTIVE_YN = 'Y'
    AND    DSP.PROJECT_ID = @ProjectId
    AND    @ArtifactTypeId IN (SELECT ARTIFACT_TYPE_ID FROM TST_ARTIFACT_TYPE WHERE IS_DATA_SYNC = 1)
    ORDER BY DSS.NAME
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a dataset containing the list of custom property values and their mappings
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_CUSTOM_PROPERTY_VALUE_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_CUSTOM_PROPERTY_VALUE_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_CUSTOM_PROPERTY_VALUE_MAPPINGS]
	@DataSyncSystemId INT,
	@ProjectId INT,
	@CustomListId INT,
	@IncludeUnmapped BIT
AS
BEGIN
	IF @IncludeUnmapped = 1
	BEGIN
		SELECT	@DataSyncSystemId AS DATA_SYNC_SYSTEM_ID, @ProjectId AS PROJECT_ID, CPV.CUSTOM_PROPERTY_VALUE_ID,
               CVM.EXTERNAL_KEY, CPV.NAME AS CUSTOM_PROPERTY_VALUE_NAME, CPV.IS_ACTIVE
		FROM	(SELECT * FROM TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING
				WHERE DATA_SYNC_SYSTEM_ID = @DataSyncSystemId
				AND PROJECT_ID = @ProjectId) CVM RIGHT JOIN TST_CUSTOM_PROPERTY_VALUE CPV
		ON     CVM.CUSTOM_PROPERTY_VALUE_ID = CPV.CUSTOM_PROPERTY_VALUE_ID
		WHERE  CPV.CUSTOM_PROPERTY_LIST_ID = @CustomListId AND CPV.IS_DELETED = 0
		ORDER BY CPV.NAME
	END
	ELSE
	BEGIN
		SELECT	CVM.DATA_SYNC_SYSTEM_ID, CVM.PROJECT_ID, CVM.CUSTOM_PROPERTY_VALUE_ID,
               CVM.EXTERNAL_KEY, CPV.NAME AS CUSTOM_PROPERTY_VALUE_NAME, CPV.IS_ACTIVE
		FROM	TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING CVM INNER JOIN TST_CUSTOM_PROPERTY_VALUE CPV
		ON     CVM.CUSTOM_PROPERTY_VALUE_ID = CPV.CUSTOM_PROPERTY_VALUE_ID
		WHERE  CVM.DATA_SYNC_SYSTEM_ID = @DataSyncSystemId
		AND    CVM.PROJECT_ID = @ProjectId
		AND    CPV.CUSTOM_PROPERTY_LIST_ID = @CustomListId
		ORDER BY CPV.NAME
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a dataset containing the list of field values and their mappings
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_FIELD_VALUE_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_FIELD_VALUE_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_FIELD_VALUE_MAPPINGS]
	@DataSyncSystemId INT,
	@ProjectId INT,
	@ArtifactFieldId INT,
	@IncludeUnmapped BIT
AS
BEGIN
	DECLARE @tblLookupData TABLE
	(
	  ID INT, 
	  NAME NVARCHAR(255),
	  IS_ACTIVE BIT
	)
	
	/* The lookup to use depends on the artifact field */
	IF @ArtifactFieldId = 1
	BEGIN
		--Incident Severity
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT INC.SEVERITY_ID, INC.NAME, INC.IS_ACTIVE
		FROM TST_INCIDENT_SEVERITY INC
			INNER JOIN TST_PROJECT PRJ ON INC.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 2
	BEGIN
		--Incident Priority
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT INC.PRIORITY_ID, INC.NAME, INC.IS_ACTIVE
		FROM TST_INCIDENT_PRIORITY INC
			INNER JOIN TST_PROJECT PRJ ON INC.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 3
	BEGIN
		--Incident Status
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT INC.INCIDENT_STATUS_ID, INC.NAME, INC.IS_ACTIVE
		FROM TST_INCIDENT_STATUS INC
			INNER JOIN TST_PROJECT PRJ ON INC.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 4
	BEGIN
		--Incident Type
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT INC.INCIDENT_TYPE_ID, INC.NAME, INC.IS_ACTIVE
		FROM TST_INCIDENT_TYPE INC
			INNER JOIN TST_PROJECT PRJ ON INC.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 148 OR @ArtifactFieldId = 141 OR @ArtifactFieldId = 168
	BEGIN
		--Incident/Requirement/Test Case Component
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT COMPONENT_ID, NAME, IS_ACTIVE
		FROM TST_COMPONENT
		WHERE PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 57
	BEGIN
		--Task Status
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TASK_STATUS_ID, NAME, IS_ACTIVE FROM TST_TASK_STATUS
	END
	IF @ArtifactFieldId = 59
	BEGIN
		--Task Priority
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TSK.TASK_PRIORITY_ID, TSK.NAME, TSK.IS_ACTIVE FROM TST_TASK_PRIORITY TSK
			INNER JOIN TST_PROJECT PRJ ON TSK.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 145
	BEGIN
		--Task Type
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TSK.TASK_TYPE_ID, TSK.NAME, TSK.IS_ACTIVE FROM TST_TASK_TYPE TSK
			INNER JOIN TST_PROJECT PRJ ON TSK.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 18
	BEGIN
		--Requirement Importance
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT REQ.IMPORTANCE_ID, REQ.NAME, REQ.IS_ACTIVE FROM TST_IMPORTANCE REQ
			INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 16
	BEGIN
		--Requirement Status
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT REQUIREMENT_STATUS_ID, NAME, IS_ACTIVE FROM TST_REQUIREMENT_STATUS
	END
	IF @ArtifactFieldId = 140
	BEGIN
		--Requirement Type
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT REQ.REQUIREMENT_TYPE_ID, REQ.NAME, REQ.IS_ACTIVE FROM TST_REQUIREMENT_TYPE REQ
			INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 24
	BEGIN
		--Test Case Priority
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TST.TEST_CASE_PRIORITY_ID, TST.NAME, TST.IS_ACTIVE FROM TST_TEST_CASE_PRIORITY TST
			INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END
	IF @ArtifactFieldId = 166
	BEGIN
		--Test Case Status
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TEST_CASE_STATUS_ID, NAME, IS_ACTIVE FROM TST_TEST_CASE_STATUS
	END
	IF @ArtifactFieldId = 167
	BEGIN
		--Test Case Type
		INSERT INTO @tblLookupData (ID, NAME, IS_ACTIVE)
		SELECT TST.TEST_CASE_TYPE_ID, TST.NAME, TST.IS_ACTIVE FROM TST_TEST_CASE_TYPE TST
			INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		WHERE PRJ.PROJECT_ID = @ProjectId 
	END

	IF @IncludeUnmapped = 1
	BEGIN
		SELECT	@DataSyncSystemId AS DATA_SYNC_SYSTEM_ID, @ProjectId AS PROJECT_ID, @ArtifactFieldId AS ARTIFACT_FIELD_ID,
				LKP.ID AS ARTIFACT_FIELD_VALUE, FVM.EXTERNAL_KEY, ISNULL(FVM.PRIMARY_YN, 'Y') AS PRIMARY_YN,
				LKP.NAME AS ARTIFACT_FIELD_VALUE_NAME, LKP.IS_ACTIVE
		FROM
			(SELECT * FROM TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING 
			WHERE ARTIFACT_FIELD_ID = @ArtifactFieldId
			AND DATA_SYNC_SYSTEM_ID = @DataSyncSystemId
			AND PROJECT_ID = @ProjectId) FVM
		RIGHT JOIN @tblLookupData LKP ON FVM.ARTIFACT_FIELD_VALUE = LKP.ID
		ORDER BY LKP.NAME, FVM.ARTIFACT_FIELD_ID
	END
	ELSE
	BEGIN
		SELECT	FVM.DATA_SYNC_SYSTEM_ID, FVM.PROJECT_ID, FVM.ARTIFACT_FIELD_ID, FVM.ARTIFACT_FIELD_VALUE,
                FVM.EXTERNAL_KEY, FVM.PRIMARY_YN, LKP.NAME AS ARTIFACT_FIELD_VALUE_NAME, LKP.IS_ACTIVE
		FROM	TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING FVM INNER JOIN @tblLookupData LKP
		ON     FVM.ARTIFACT_FIELD_VALUE = LKP.ID
		WHERE  FVM.DATA_SYNC_SYSTEM_ID = @DataSyncSystemId
		AND    FVM.PROJECT_ID = @ProjectId
		AND    FVM.ARTIFACT_FIELD_ID = @ArtifactFieldId
		ORDER BY LKP.NAME, FVM.ARTIFACT_FIELD_ID
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: DataSyncManager
-- Description:		Retrieves a list containing all the data-mappings for a specific user
--					Also returns unmapped system records
-- =====================================================================
IF OBJECT_ID ( 'DATA_SYNC_RETRIEVE_USER_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [DATA_SYNC_RETRIEVE_USER_MAPPINGS];
GO
CREATE PROCEDURE [DATA_SYNC_RETRIEVE_USER_MAPPINGS]
	@UserId INT
AS
BEGIN
    SELECT	DSS.DATA_SYNC_SYSTEM_ID, @UserId AS USER_ID, DUM.EXTERNAL_KEY, DSS.NAME AS DATA_SYNC_SYSTEM_NAME,
			(CASE DSS.CAPTION WHEN NULL THEN DSS.NAME ELSE DSS.CAPTION END) AS DATA_SYNC_SYSTEM_DISPLAY_NAME
    FROM	(SELECT * FROM TST_DATA_SYNC_USER_MAPPING
			WHERE USER_ID = @UserId) DUM RIGHT JOIN TST_DATA_SYNC_SYSTEM DSS
    ON     DUM.DATA_SYNC_SYSTEM_ID = DSS.DATA_SYNC_SYSTEM_ID
    ORDER BY DSS.NAME
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: History
-- Description:		Removes all changesets for the given Artifact Type and ID 
-- Remarks:			If you don't pass in an @ArtifactTypeId it will delete for the whole project
-- =============================================
IF OBJECT_ID ( 'HISTORY_DELETE_CHANGESETS', 'P' ) IS NOT NULL 
    DROP PROCEDURE HISTORY_DELETE_CHANGESETS;
GO
CREATE PROCEDURE HISTORY_DELETE_CHANGESETS
	@ArtifactTypeId INT,
	@ArtifactId INT,
	@ProjectId INT
AS
BEGIN
	IF @ArtifactTypeId IS NULL
	BEGIN
		--Remove for the entire project
		
		--Unlink the revert references
		UPDATE TST_HISTORY_CHANGESET
			SET REVERT_ID = NULL
		WHERE REVERT_ID IN (
			SELECT CHANGESET_ID
			FROM TST_HISTORY_CHANGESET
			WHERE PROJECT_ID = @ProjectId
			)
		
		--Delete from the changeset table. Deletions cascade.
        DELETE FROM TST_HISTORY_CHANGESET
        WHERE PROJECT_ID = @ProjectId
	END
	ELSE
	BEGIN
		-- Remove for the specified artifact id/type
	
		--Unlink the revert references
		UPDATE TST_HISTORY_CHANGESET
			SET REVERT_ID = NULL
		WHERE REVERT_ID IN (
			SELECT CHANGESET_ID
			FROM TST_HISTORY_CHANGESET
			WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId
			)
		
		--Delete from the changeset table. Deletions cascade.
        DELETE FROM TST_HISTORY_CHANGESET
        WHERE ARTIFACT_TYPE_ID = @ArtifactTypeId AND ARTIFACT_ID = @ArtifactId
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Deletes an Incident
-- =============================================
IF OBJECT_ID ( 'INCIDENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_DELETE;
GO
CREATE PROCEDURE INCIDENT_DELETE
	@IncidentId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the resolutions
    DELETE FROM TST_INCIDENT_RESOLUTION WHERE INCIDENT_ID = @IncidentId;
	--Delete user subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 3 AND ARTIFACT_ID = @IncidentId);
	--Now delete the incident itself
    DELETE FROM TST_INCIDENT WHERE INCIDENT_ID = @IncidentId;
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the list of incidents for a given test case
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_BY_TEST_CASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_BY_TEST_CASE;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_BY_TEST_CASE
	@TestCaseId INT,
	@IsOpenOnly BIT
AS
BEGIN
	SELECT	INC.*
	FROM	VW_INCIDENT_LIST INC INNER JOIN TST_TEST_RUN_STEP_INCIDENT TRI
	ON		INC.INCIDENT_ID = TRI.INCIDENT_ID INNER JOIN TST_TEST_RUN_STEP TRS
	ON		TRI.TEST_RUN_STEP_ID = TRS.TEST_RUN_STEP_ID INNER JOIN TST_TEST_RUN TRN
	ON		TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	WHERE	TRN.TEST_CASE_ID = @TestCaseId
	AND		INC.IS_DELETED = 0
	AND		(@IsOpenOnly = 0 OR INC.INCIDENT_STATUS_IS_OPEN_STATUS = 1)
	UNION
    SELECT DISTINCT  INC.*
    FROM	VW_INCIDENT_LIST INC INNER JOIN TST_ARTIFACT_LINK ARL
    ON		INC.INCIDENT_ID = ARL.DEST_ARTIFACT_ID INNER JOIN TST_TEST_STEP STP
    ON		ARL.SOURCE_ARTIFACT_ID = STP.TEST_STEP_ID
    WHERE	STP.TEST_CASE_ID = @TestCaseId
    AND    INC.IS_DELETED = 0
    AND    ARL.SOURCE_ARTIFACT_TYPE_ID = 7 /* Test Step */
    AND    ARL.DEST_ARTIFACT_TYPE_ID = 3 /* Incident */ 
    AND		(@IsOpenOnly = 0 OR INC.INCIDENT_STATUS_IS_OPEN_STATUS = 1)
	ORDER BY INC.PRIORITY_NAME, INC.INCIDENT_ID
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a count of incidents by status for a particular priority level
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_COUNT_BY_PRIORITY', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_PRIORITY;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_PRIORITY
	@ProjectId INT,
	@ReleaseId INT,
	@PriorityId INT,
	@IncidentTypeId INT,
	@UseResolvedRelease BIT,
	@IncludeDeleted BIT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
        SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
        FROM	TST_INCIDENT
        WHERE	PROJECT_ID = @ProjectId
        AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
        AND	(PRIORITY_ID = @PriorityId OR (@PriorityId IS NULL AND PRIORITY_ID IS NULL))
        AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
        GROUP BY INCIDENT_STATUS_ID
        ORDER BY INCIDENT_STATUS_ID
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		IF @UseResolvedRelease = 1
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(PRIORITY_ID = @PriorityId OR (@PriorityId IS NULL AND PRIORITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
		ELSE
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(PRIORITY_ID = @PriorityId OR (@PriorityId IS NULL AND PRIORITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a count of incidents by status for a particular severity level
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_COUNT_BY_SEVERITY', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_SEVERITY;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_COUNT_BY_SEVERITY
	@ProjectId INT,
	@ReleaseId INT,
	@SeverityId INT,
	@IncidentTypeId INT,
	@UseResolvedRelease BIT,
	@IncludeDeleted BIT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
        SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
        FROM	TST_INCIDENT
        WHERE	PROJECT_ID = @ProjectId
        AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
        AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
        AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
        GROUP BY INCIDENT_STATUS_ID
        ORDER BY INCIDENT_STATUS_ID
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		IF @UseResolvedRelease = 1
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
		ELSE
		BEGIN
			SELECT	INCIDENT_STATUS_ID AS IncidentStatusId, COUNT(INCIDENT_ID) AS IncidentCount
			FROM	TST_INCIDENT
			WHERE	PROJECT_ID = @ProjectId
			AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
			AND	(SEVERITY_ID = @SeverityId OR (@SeverityId IS NULL AND SEVERITY_ID IS NULL))
			AND (@IncidentTypeId IS NULL OR INCIDENT_TYPE_ID = @IncidentTypeId)
			AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)        
			GROUP BY INCIDENT_STATUS_ID
			ORDER BY INCIDENT_STATUS_ID
		END
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the count of open incidents in the group by age
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_GROUP_AGING_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_GROUP_AGING_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_GROUP_AGING_COUNT
	@ProjectGroupId INT
AS
BEGIN
    SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
    FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
				CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
				FROM TST_INCIDENT WHERE IS_DELETED = 0) AS INC INNER JOIN TST_INCIDENT_STATUS IST
    ON     INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID INNER JOIN TST_PROJECT PRJ
    ON     INC.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
    AND    PRJ.IS_ACTIVE = 1
    AND	IST.IS_OPEN_STATUS = 1
    GROUP BY Age
    ORDER BY Age ASC
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a count of incidents that are in an open or closed status
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_OPEN_CLOSED_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@UseResolvedRelease BIT
AS
BEGIN
	IF @ReleaseId IS NULL
	BEGIN
		SELECT	INC.INCIDENT_STATUS_IS_OPEN_STATUS AS IS_OPEN_STATUS, ISNULL(COUNT(INC.INCIDENT_ID),0) AS INCIDENT_COUNT
		FROM	VW_INCIDENT_LIST INC
		WHERE	PROJECT_ID = @ProjectId
		AND IS_DELETED = 0
		GROUP BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	    ORDER BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		SELECT	INC.INCIDENT_STATUS_IS_OPEN_STATUS AS IS_OPEN_STATUS, ISNULL(COUNT(INC.INCIDENT_ID),0) AS INCIDENT_COUNT
		FROM	VW_INCIDENT_LIST INC
		WHERE	PROJECT_ID = @ProjectId
		AND ((@UseResolvedRelease = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
			OR (@UseResolvedRelease = 1 AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)))
		AND IS_DELETED = 0
		GROUP BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	    ORDER BY INC.INCIDENT_STATUS_IS_OPEN_STATUS
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves a summary count of open incidents by either priority or severity
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_OPEN_COUNT_BY_PRIORITY_SEVERITY', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_OPEN_COUNT_BY_PRIORITY_SEVERITY;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_OPEN_COUNT_BY_PRIORITY_SEVERITY
	@ProjectId INT,
	@ReleaseId INT,
	@UseSeverity BIT,
	@UseResolvedRelease BIT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
		IF @UseSeverity = 1
		BEGIN
			SELECT COUNT(INC.INCIDENT_ID) AS Count, ISNULL(SEV.NAME,'(None)') AS PrioritySeverityName, SEV.COLOR AS PrioritySeverityColor
			FROM TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
			ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID LEFT JOIN TST_INCIDENT_SEVERITY SEV
			ON INC.SEVERITY_ID = SEV.SEVERITY_ID
			WHERE IST.IS_OPEN_STATUS = 1
			AND INC.PROJECT_ID = @ProjectId
			AND INC.IS_DELETED = 0
			GROUP BY SEV.NAME, SEV.COLOR
			ORDER BY SEV.NAME
		END
		ELSE
		BEGIN
			SELECT COUNT(INC.INCIDENT_ID) AS Count, ISNULL(PRI.NAME,'(None)') AS PrioritySeverityName, PRI.COLOR AS PrioritySeverityColor
			FROM TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
			ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID LEFT JOIN TST_INCIDENT_PRIORITY PRI
			ON INC.PRIORITY_ID = PRI.PRIORITY_ID
			WHERE IST.IS_OPEN_STATUS = 1
			AND INC.PROJECT_ID = @ProjectId
			AND INC.IS_DELETED = 0
			GROUP BY PRI.NAME, PRI.COLOR
			ORDER BY PRI.NAME
		END
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		IF @UseSeverity = 1
		BEGIN
			SELECT COUNT(INC.INCIDENT_ID) AS Count, ISNULL(SEV.NAME,'(None)') AS PrioritySeverityName, SEV.COLOR AS PrioritySeverityColor
			FROM TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
			ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID LEFT JOIN TST_INCIDENT_SEVERITY SEV
			ON INC.SEVERITY_ID = SEV.SEVERITY_ID
			WHERE IST.IS_OPEN_STATUS = 1
			AND INC.PROJECT_ID = @ProjectId
			AND INC.IS_DELETED = 0
			AND ((@UseResolvedRelease = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
				OR (@UseResolvedRelease = 1 AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)))
			GROUP BY SEV.NAME, SEV.COLOR
			ORDER BY SEV.NAME
		END
		ELSE
		BEGIN
			SELECT COUNT(INC.INCIDENT_ID) AS Count, ISNULL(PRI.NAME,'(None)') AS PrioritySeverityName, PRI.COLOR AS PrioritySeverityColor
			FROM TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
			ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID LEFT JOIN TST_INCIDENT_PRIORITY PRI
			ON INC.PRIORITY_ID = PRI.PRIORITY_ID
			WHERE IST.IS_OPEN_STATUS = 1
			AND INC.PROJECT_ID = @ProjectId
			AND INC.IS_DELETED = 0
			AND ((@UseResolvedRelease = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
				OR (@UseResolvedRelease = 1 AND RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)))
			GROUP BY PRI.NAME, PRI.COLOR
			ORDER BY PRI.NAME
		END
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the count of open incidents in the project by age
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_PROJECT_AGING_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_PROJECT_AGING_COUNT;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_PROJECT_AGING_COUNT
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
		SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
		FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
					CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
					FROM TST_INCIDENT WHERE IS_DELETED = 0) AS INC
		INNER JOIN TST_INCIDENT_STATUS IST ON INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
		WHERE	INC.PROJECT_ID = @ProjectId
		AND	IST.IS_OPEN_STATUS = 1
		GROUP BY Age
		ORDER BY Age ASC
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
		
		--Now get the age count
		SELECT	Age, COUNT(INC.INCIDENT_ID) AS OpenCount
		FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETUTCDATE()) As Age,
					CLOSED_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID
					FROM TST_INCIDENT
					WHERE IS_DELETED = 0 AND DETECTED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)) AS INC
		INNER JOIN TST_INCIDENT_STATUS IST
		ON     INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
		WHERE	INC.PROJECT_ID = @ProjectId
		AND	IST.IS_OPEN_STATUS = 1
		GROUP BY Age
		ORDER BY Age ASC
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Retrieves the test coverage for all closed incidents
-- =====================================================================
IF OBJECT_ID ( 'INCIDENT_RETRIEVE_TEST_COVERAGE', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_RETRIEVE_TEST_COVERAGE;
GO
CREATE PROCEDURE INCIDENT_RETRIEVE_TEST_COVERAGE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
		SELECT COUNT(TIN2.TEST_CASE_ID) AS TestCount, EXE.EXECUTION_STATUS_ID AS ExecutionStatusId, EXE.NAME AS ExecutionStatusName
		FROM VW_EXECUTION_STATUS_ACTIVE EXE LEFT JOIN
			(SELECT TIN.TEST_CASE_ID, TST.EXECUTION_STATUS_ID
			FROM VW_TESTCASE_INCIDENTS TIN INNER JOIN TST_TEST_CASE TST
			ON TIN.TEST_CASE_ID = TST.TEST_CASE_ID
			WHERE TIN.IS_OPEN_STATUS = 0
			AND TST.PROJECT_ID = @ProjectId) TIN2
		ON EXE.EXECUTION_STATUS_ID = TIN2.EXECUTION_STATUS_ID
		GROUP BY EXE.EXECUTION_STATUS_ID, EXE.NAME
		ORDER BY EXE.EXECUTION_STATUS_ID
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		SELECT COUNT(TIN2.TEST_CASE_ID) AS TestCount, EXE.EXECUTION_STATUS_ID AS ExecutionStatusId, EXE.NAME AS ExecutionStatusName
		FROM VW_EXECUTION_STATUS_ACTIVE EXE LEFT JOIN
			(SELECT TIN.TEST_CASE_ID, ISNULL(TRN.EXECUTION_STATUS_ID,3) AS EXECUTION_STATUS_ID
			FROM VW_TESTCASE_INCIDENTS TIN INNER JOIN TST_TEST_CASE TST
			ON TIN.TEST_CASE_ID = TST.TEST_CASE_ID LEFT JOIN
				(SELECT RUN1.TEST_CASE_ID, MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID
				FROM TST_TEST_RUN RUN1 LEFT JOIN
					(SELECT TEST_CASE_ID, MAX(END_DATE) AS END_DATE
					FROM TST_TEST_RUN WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
					GROUP BY TEST_CASE_ID) RUN2
				ON RUN1.TEST_CASE_ID = RUN2.TEST_CASE_ID AND RUN1.END_DATE = RUN2.END_DATE
				WHERE RUN1.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
				GROUP BY RUN1.TEST_CASE_ID) TRN
			ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID INNER JOIN TST_RELEASE_TEST_CASE RTC
			ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID AND TIN.RESOLVED_RELEASE_ID = RTC.RELEASE_ID
			WHERE TIN.IS_OPEN_STATUS = 0
			AND TST.PROJECT_ID = @ProjectId
			AND TIN.RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)) TIN2
		ON EXE.EXECUTION_STATUS_ID = TIN2.EXECUTION_STATUS_ID
		GROUP BY EXE.EXECUTION_STATUS_ID, EXE.NAME
		ORDER BY EXE.EXECUTION_STATUS_ID
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Updates the rank for passed in incident ids
-- =============================================
IF OBJECT_ID ( 'INCIDENT_UPDATE_RANKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE INCIDENT_UPDATE_RANKS;
GO
CREATE PROCEDURE INCIDENT_UPDATE_RANKS
	@ProjectId INT,
	@IncidentIds NVARCHAR(MAX),
	@ExistingRank INT
AS
	DECLARE @IncidentIdsTable TABLE (INCIDENT_ID INT)
BEGIN
	--Get the list of incident IDs
	INSERT INTO @IncidentIdsTable (INCIDENT_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@IncidentIds, ',')

	IF @ExistingRank IS NOT NULL
	BEGIN
		--Increment the incidents that have a higher rank
		UPDATE TST_INCIDENT
			SET RANK = RANK + 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK > @ExistingRank
				AND INCIDENT_ID NOT IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
				
		--Decrement the requirements that have a lower or equal rank
		UPDATE TST_INCIDENT
			SET RANK = RANK - 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK <= @ExistingRank
				AND INCIDENT_ID NOT IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
				
		-- Set the selected incidents to this rank
		UPDATE TST_INCIDENT
			SET RANK = @ExistingRank
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND INCIDENT_ID IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
	END
	ELSE
	BEGIN
		-- Set the selected incidents to the lowest non-null rank
		UPDATE TST_INCIDENT
			SET RANK = 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND INCIDENT_ID IN (SELECT INCIDENT_ID FROM @IncidentIdsTable)
	END	
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Logger
-- Description:		Deletes old events
-- =============================================
IF OBJECT_ID ( 'LOGGER_DELETE_OLD', 'P' ) IS NOT NULL 
    DROP PROCEDURE LOGGER_DELETE_OLD;
GO
CREATE PROCEDURE LOGGER_DELETE_OLD
	@LastDateToKeep DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any event that is too old
    DELETE FROM TST_EVENT WHERE EVENT_TIME_UTC < @LastDateToKeep
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Message
-- Description:		Purges/deletes old messages
-- =============================================
IF OBJECT_ID ( 'MESSAGE_PURGE_OLD', 'P' ) IS NOT NULL 
    DROP PROCEDURE MESSAGE_PURGE_OLD;
GO
CREATE PROCEDURE MESSAGE_PURGE_OLD
	@PurgeDateTime DATETIME,
	@IncludeUnread BIT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any message that is too old
	IF @IncludeUnread = 1
	BEGIN
		DELETE FROM TST_MESSAGE
		WHERE LAST_UPDATE_DATE < @PurgeDateTime
	END
	ELSE
	BEGIN
		DELETE FROM TST_MESSAGE
		WHERE LAST_UPDATE_DATE < @PurgeDateTime
			AND (IS_READ = 1 OR IS_DELETED = 1)
	END
END
GO
-- ============================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: N/A
-- Description:		Populates over the initial set of releases, projects, programs and portfolio completion data
-- =============================================================================================================
IF OBJECT_ID ( 'MIGRATION_POPULATE_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE MIGRATION_POPULATE_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE MIGRATION_POPULATE_REQUIREMENT_COMPLETION
AS
DECLARE
	@ProjectId INT,
	@ReleaseId INT
BEGIN
	SET NOCOUNT ON;
	
	--First loop through all the projects
	DECLARE ProjectCursor CURSOR LOCAL FOR
		SELECT PROJECT_ID
		FROM TST_PROJECT
		ORDER BY PROJECT_ID
		
	--Loop
	OPEN ProjectCursor   
	FETCH NEXT FROM ProjectCursor INTO @ProjectId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		--Now loop through all the releases in the project
		DECLARE ReleaseCursor CURSOR LOCAL FOR
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			ORDER BY RELEASE_ID
			
		--Loop
		OPEN ReleaseCursor   
		FETCH NEXT FROM ReleaseCursor INTO @ReleaseId
		WHILE @@FETCH_STATUS = 0   
		BEGIN
			EXEC RELEASE_REFRESH_REQUIREMENT_COMPLETION @ProjectId, @ReleaseId
			FETCH NEXT FROM ReleaseCursor INTO @ReleaseId
		END   
		
		CLOSE ReleaseCursor
		DEALLOCATE ReleaseCursor	
	
		EXEC PROJECT_REFRESH_REQUIREMENT_COMPLETION @ProjectId
		FETCH NEXT FROM ProjectCursor INTO @ProjectId
	END   

	--Clean up
	CLOSE ProjectCursor   
	DEALLOCATE ProjectCursor
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Sets
-- =============================================
IF OBJECT_ID ( 'MIGRATION_REFRESH_PROJECT_TEST_CACHE', 'P' ) IS NOT NULL 
    DROP PROCEDURE MIGRATION_REFRESH_PROJECT_TEST_CACHE;
GO
CREATE PROCEDURE MIGRATION_REFRESH_PROJECT_TEST_CACHE
	@ProjectId INT
AS
DECLARE
	@TestCaseId INT,
	@TestCaseFolderId INT,
	@TestSetId INT,
	@TestSetFolderId INT
BEGIN
	SET NOCOUNT ON;
	
	--First refresh the test cases
	DECLARE TestCaseCursor CURSOR LOCAL FOR
		SELECT TEST_CASE_ID
		FROM TST_TEST_CASE
		WHERE PROJECT_ID = @ProjectId
		ORDER BY TEST_CASE_ID
		
	--Loop
	OPEN TestCaseCursor   
	FETCH NEXT FROM TestCaseCursor INTO @TestCaseId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3 @ProjectId, @TestCaseId
		FETCH NEXT FROM TestCaseCursor INTO @TestCaseId
	END   

	--Clean up
	CLOSE TestCaseCursor   
	DEALLOCATE TestCaseCursor
	
	--Next refresh the test case folders in reverse hierarchy order
	CREATE TABLE #tblTestCaseFolders
	(
		TEST_CASE_FOLDER_ID INT,
		PROJECT_ID INT,
		NAME NVARCHAR(255),
		PARENT_TEST_CASE_FOLDER_ID INT,
		HIERARCHY_LEVEL INT,
		INDENT_LEVEL NVARCHAR(100)
	)
	INSERT INTO #tblTestCaseFolders(TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	EXEC TESTCASE_RETRIEVE_FOLDER_HIERARCHY @ProjectId
	
	DECLARE TestCaseFolderCursor CURSOR LOCAL FOR
		SELECT TEST_CASE_FOLDER_ID
		FROM #tblTestCaseFolders
		ORDER BY INDENT_LEVEL DESC, TEST_CASE_FOLDER_ID ASC
		
	--Loop
	OPEN TestCaseFolderCursor   
	FETCH NEXT FROM TestCaseFolderCursor INTO @TestCaseFolderId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTCASE_REFRESH_FOLDER_EXECUTION_STATUS @ProjectId, @TestCaseFolderId
		FETCH NEXT FROM TestCaseFolderCursor INTO @TestCaseFolderId
	END   

	--Clean up
	CLOSE TestCaseFolderCursor   
	DEALLOCATE TestCaseFolderCursor
	
	--First refresh the test sets	
	DECLARE TestSetCursor CURSOR LOCAL FOR
		SELECT TEST_SET_ID
		FROM TST_TEST_SET
		WHERE PROJECT_ID = @ProjectId
		ORDER BY TEST_SET_ID
		
	--Loop
	OPEN TestSetCursor   
	FETCH NEXT FROM TestSetCursor INTO @TestSetId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTSET_REFRESH_EXECUTION_DATA @ProjectId, @TestSetId
		FETCH NEXT FROM TestSetCursor INTO @TestSetId
	END   

	--Clean up
	CLOSE TestSetCursor   
	DEALLOCATE TestSetCursor
	
	--Next refresh the test set folders in reverse hierarchy order
	CREATE TABLE #tblTestSetFolders
	(		
		TEST_SET_FOLDER_ID INT,
		PROJECT_ID INT,
		NAME NVARCHAR(255),
		PARENT_TEST_SET_FOLDER_ID INT,
		HIERARCHY_LEVEL INT,
		INDENT_LEVEL NVARCHAR(100)
	)
	INSERT INTO #tblTestSetFolders(TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	EXEC TESTSET_RETRIEVE_FOLDER_HIERARCHY @ProjectId

	DECLARE TestSetFolderCursor CURSOR LOCAL FOR
		SELECT TEST_SET_FOLDER_ID
		FROM #tblTestSetFolders
		ORDER BY INDENT_LEVEL DESC, TEST_SET_FOLDER_ID ASC
		
	--Loop
	OPEN TestSetFolderCursor   
	FETCH NEXT FROM TestSetFolderCursor INTO @TestSetFolderId
	WHILE @@FETCH_STATUS = 0   
	BEGIN
		EXEC TESTSET_REFRESH_FOLDER_EXECUTION_STATUS @ProjectId, @TestSetFolderId
		FETCH NEXT FROM TestSetFolderCursor INTO @TestSetFolderId
	END   

	--Clean up
	CLOSE TestSetFolderCursor   
	DEALLOCATE TestSetFolderCursor
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Notification
-- Description:		Deletes all the notification events and templates for a specific project template
-- =============================================
IF OBJECT_ID ( 'NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE', 'P' ) IS NOT NULL 
    DROP PROCEDURE NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE;
GO
CREATE PROCEDURE NOTIFICATION_DELETE_ALL_FOR_PROJECT_TEMPLATE
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete all the notification events
	DELETE FROM TST_NOTIFICATION_EVENT WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId

    --Delete all the notification templates
    DELETE FROM TST_NOTIFICATION_ARTIFACT_TEMPLATE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
END
GO
-- ===================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Portfolio
-- Description:		Refreshes the requirement count, overall percent complete for the portfolio
-- ===================================================================================================
IF OBJECT_ID ( 'PORTFOLIO_REFRESH_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE PORTFOLIO_REFRESH_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE PORTFOLIO_REFRESH_REQUIREMENT_COMPLETION
	@PortfolioId INT
AS
BEGIN							
	--Next we update the portfolio (if exists) with the percentage complete and requirement counts
	MERGE TST_PORTFOLIO AS TARGET	
	USING (
		SELECT
			PRG.PORTFOLIO_ID,
			SUM(PRG.REQUIREMENT_COUNT) AS REQUIREMENT_COUNT,
			SUM(PRG.PERCENT_COMPLETE * PRG.REQUIREMENT_COUNT) / SUM(PRG.REQUIREMENT_COUNT) AS PERCENT_COMPLETE
		FROM
			TST_PROJECT_GROUP PRG
		WHERE
			PRG.PORTFOLIO_ID = @PortfolioId AND
			PRG.IS_ACTIVE = 1 AND
			PRG.REQUIREMENT_COUNT > 0
		GROUP BY PRG.PORTFOLIO_ID
	) AS SOURCE
	ON
		TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
				TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
			UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = 0,
				TARGET.PERCENT_COMPLETE = 0;
				
	--Next we update the portfolio with the min/max start/end dates
	MERGE TST_PORTFOLIO AS TARGET	
	USING (
		SELECT
			PRG.PORTFOLIO_ID,
			MIN(PRG.START_DATE) AS MIN_START_DATE,
			MAX(PRG.END_DATE) AS MAX_END_DATE
		FROM
			TST_PROJECT_GROUP PRG
		WHERE
			PRG.PORTFOLIO_ID = @PortfolioId AND
			PRG.IS_ACTIVE = 1 AND
			PRG.START_DATE IS NOT NULL AND
			PRG.END_DATE IS NOT NULL
		GROUP BY PRG.PORTFOLIO_ID
	) AS SOURCE
	ON
		TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.START_DATE = SOURCE.MIN_START_DATE,
				TARGET.END_DATE = SOURCE.MAX_END_DATE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
			UPDATE
			SET	
				TARGET.START_DATE = NULL,
				TARGET.END_DATE = NULL;
END
GO
-- ===================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: ProjectGroup
-- Description:		Refreshes the requirement count, overall percent complete for the project group
--					and also rolls up to the parent portfolio (if any)
-- ===================================================================================================
IF OBJECT_ID ( 'PROJECTGROUP_REFRESH_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECTGROUP_REFRESH_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE PROJECTGROUP_REFRESH_REQUIREMENT_COMPLETION
	@ProjectGroupId INT
AS
DECLARE
	@PortfolioId INT
BEGIN				
	--First we update the program with the percentage complete and requirement counts
	MERGE TST_PROJECT_GROUP AS TARGET	
	USING (
		SELECT
			PRJ.PROJECT_GROUP_ID,
			SUM(PRJ.REQUIREMENT_COUNT) AS REQUIREMENT_COUNT,
			SUM(PRJ.PERCENT_COMPLETE * PRJ.REQUIREMENT_COUNT) / SUM(PRJ.REQUIREMENT_COUNT) AS PERCENT_COMPLETE
		FROM
			TST_PROJECT PRJ
		WHERE
			PRJ.PROJECT_GROUP_ID = @ProjectGroupId AND
			PRJ.IS_ACTIVE = 1 AND
			PRJ.REQUIREMENT_COUNT > 0
		GROUP BY PRJ.PROJECT_GROUP_ID
	) AS SOURCE
	ON
		TARGET.PROJECT_GROUP_ID = SOURCE.PROJECT_GROUP_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
				TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_GROUP_ID = @ProjectGroupId THEN
			UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = 0,
				TARGET.PERCENT_COMPLETE = 0;
				
	--Next we update the program with the min/max start/end dates
	MERGE TST_PROJECT_GROUP AS TARGET	
	USING (
		SELECT
			PRJ.PROJECT_GROUP_ID,
			MIN(PRJ.START_DATE) AS MIN_START_DATE,
			MAX(PRJ.END_DATE) AS MAX_END_DATE
		FROM
			TST_PROJECT PRJ
		WHERE
			PRJ.PROJECT_GROUP_ID = @ProjectGroupId AND
			PRJ.IS_ACTIVE = 1 AND
			PRJ.START_DATE IS NOT NULL AND
			PRJ.END_DATE IS NOT NULL
		GROUP BY PRJ.PROJECT_GROUP_ID
	) AS SOURCE
	ON
		TARGET.PROJECT_GROUP_ID = SOURCE.PROJECT_GROUP_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.START_DATE = SOURCE.MIN_START_DATE,
				TARGET.END_DATE = SOURCE.MAX_END_DATE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_GROUP_ID = @ProjectGroupId THEN
			UPDATE
			SET	
				TARGET.START_DATE = NULL,
				TARGET.END_DATE = NULL;
				
	--Next we update the portfolio (if exists) with the percentage complete and requirement counts
	SELECT @PortfolioId = PORTFOLIO_ID FROM TST_PROJECT_GROUP WHERE PROJECT_GROUP_ID = @ProjectGroupId;
	IF @PortfolioId IS NOT NULL
	BEGIN
		MERGE TST_PORTFOLIO AS TARGET	
		USING (
			SELECT
				PRG.PORTFOLIO_ID,
				SUM(PRG.REQUIREMENT_COUNT) AS REQUIREMENT_COUNT,
				SUM(PRG.PERCENT_COMPLETE * PRG.REQUIREMENT_COUNT) / SUM(PRG.REQUIREMENT_COUNT) AS PERCENT_COMPLETE
			FROM
				TST_PROJECT_GROUP PRG
			WHERE
				PRG.PORTFOLIO_ID = @PortfolioId AND
				PRG.IS_ACTIVE = 1 AND
				PRG.REQUIREMENT_COUNT > 0
			GROUP BY PRG.PORTFOLIO_ID
		) AS SOURCE
		ON
			TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
		WHEN MATCHED THEN
			UPDATE
				SET	
					TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
					TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
		WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
				UPDATE
				SET	
					TARGET.REQUIREMENT_COUNT = 0,
					TARGET.PERCENT_COMPLETE = 0;
					
		--Next we update the portfolio with the min/max start/end dates
		MERGE TST_PORTFOLIO AS TARGET	
		USING (
			SELECT
				PRG.PORTFOLIO_ID,
				MIN(PRG.START_DATE) AS MIN_START_DATE,
				MAX(PRG.END_DATE) AS MAX_END_DATE
			FROM
				TST_PROJECT_GROUP PRG
			WHERE
				PRG.PORTFOLIO_ID = @PortfolioId AND
				PRG.IS_ACTIVE = 1 AND
				PRG.START_DATE IS NOT NULL AND
				PRG.END_DATE IS NOT NULL
			GROUP BY PRG.PORTFOLIO_ID
		) AS SOURCE
		ON
			TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
		WHEN MATCHED THEN
			UPDATE
				SET	
					TARGET.START_DATE = SOURCE.MIN_START_DATE,
					TARGET.END_DATE = SOURCE.MAX_END_DATE
		WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
				UPDATE
				SET	
					TARGET.START_DATE = NULL,
					TARGET.END_DATE = NULL;
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes a Project
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE;
GO
CREATE PROCEDURE PROJECT_DELETE
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --Unset any user's 'last opened project' if set
	UPDATE TST_USER_PROFILE SET LAST_OPENED_PROJECT_ID = NULL WHERE LAST_OPENED_PROJECT_ID = @ProjectId

	--Delete any project settings
	DELETE FROM TST_PROJECT_SETTING_VALUE WHERE PROJECT_ID = @ProjectId

    --Finally delete the project itself
    DELETE FROM TST_PROJECT WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Associations
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_ASSOCIATIONS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_ASSOCIATIONS;
GO
CREATE PROCEDURE PROJECT_DELETE_ASSOCIATIONS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Requirements
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = 1 AND SOURCE_ARTIFACT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = 1 AND DEST_ARTIFACT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT WHERE PROJECT_ID = @ProjectId)
	--Incidents
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = 3 AND SOURCE_ARTIFACT_ID IN (SELECT INCIDENT_ID FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = 3 AND DEST_ARTIFACT_ID IN (SELECT INCIDENT_ID FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId)
	--Tasks
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = 6 AND SOURCE_ARTIFACT_ID IN (SELECT TASK_ID FROM TST_TASK WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = 6 AND DEST_ARTIFACT_ID IN (SELECT TASK_ID FROM TST_TASK WHERE PROJECT_ID = @ProjectId)
	--Test Runs
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = 5 AND SOURCE_ARTIFACT_ID IN (SELECT TR.TEST_RUN_ID FROM TST_TEST_RUN TR INNER JOIN TST_TEST_CASE TC ON TR.TEST_CASE_ID = TC.TEST_CASE_ID WHERE TC.PROJECT_ID = @ProjectId)
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = 5 AND DEST_ARTIFACT_ID IN (SELECT TR.TEST_RUN_ID FROM TST_TEST_RUN TR INNER JOIN TST_TEST_CASE TC ON TR.TEST_CASE_ID = TC.TEST_CASE_ID WHERE TC.PROJECT_ID = @ProjectId)
	--Test Steps
    DELETE FROM TST_ARTIFACT_LINK WHERE SOURCE_ARTIFACT_TYPE_ID = 7 AND SOURCE_ARTIFACT_ID IN (SELECT TS.TEST_STEP_ID FROM TST_TEST_STEP TS INNER JOIN TST_TEST_CASE TC ON TS.TEST_CASE_ID = TC.TEST_CASE_ID WHERE TC.PROJECT_ID = @ProjectId)
    DELETE FROM TST_ARTIFACT_LINK WHERE DEST_ARTIFACT_TYPE_ID = 7 AND DEST_ARTIFACT_ID IN (SELECT TS.TEST_STEP_ID FROM TST_TEST_STEP TS INNER JOIN TST_TEST_CASE TC ON TS.TEST_CASE_ID = TC.TEST_CASE_ID WHERE TC.PROJECT_ID = @ProjectId)
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Custom Properties
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_CUSTOM_PROPERTIES', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_CUSTOM_PROPERTIES;
GO
CREATE PROCEDURE PROJECT_DELETE_CUSTOM_PROPERTIES
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	
    --Now we need to delete all the custom properties and then custom lists. The dependent entities should then cascade
    DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId

    --Now we need to delete all the user membership and user project settings
    DELETE FROM TST_USER_ARTIFACT_FIELD WHERE PROJECT_ID = @ProjectId
    
    --Delete all of the artifact custom property records
    DELETE FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Mappings
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_DATA_MAPPINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_DATA_MAPPINGS;
GO
CREATE PROCEDURE PROJECT_DELETE_DATA_MAPPINGS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Incidents
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_INCIDENTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_INCIDENTS;
GO
CREATE PROCEDURE PROJECT_DELETE_INCIDENTS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_INCIDENT_RESOLUTION WHERE INCIDENT_ID IN (SELECT INCIDENT_ID FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_INCIDENT WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_PLACEHOLDER WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes a Project Role
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_PROJECT_ROLE', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_PROJECT_ROLE;
GO
CREATE PROCEDURE PROJECT_DELETE_PROJECT_ROLE
	@ProjectRoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the dependent data first
    DELETE FROM TST_WORKFLOW_TRANSITION_ROLE WHERE PROJECT_ROLE_ID = @ProjectRoleId;
    DELETE FROM TST_PROJECT_USER WHERE PROJECT_ROLE_ID = @ProjectRoleId;
    DELETE FROM TST_PROJECT_ROLE_PERMISSION WHERE PROJECT_ROLE_ID = @ProjectRoleId;

	--Now delete the project role itself
    DELETE FROM TST_PROJECT_ROLE WHERE PROJECT_ROLE_ID = @ProjectRoleId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Releases & Automation Hosts
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_RELEASES_AUTOMATION', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_RELEASES_AUTOMATION;
GO
CREATE PROCEDURE PROJECT_DELETE_RELEASES_AUTOMATION
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM TST_RELEASE WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_AUTOMATION_HOST WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Reports
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_REPORTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_REPORTS;
GO
CREATE PROCEDURE PROJECT_DELETE_REPORTS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Everything else cascades
	DELETE FROM TST_REPORT_SAVED WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Requirements
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_REQUIREMENTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_REQUIREMENTS;
GO
CREATE PROCEDURE PROJECT_DELETE_REQUIREMENTS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TST_REQUIREMENT_TEST_CASE WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT WHERE PROJECT_ID = @ProjectId)
	DELETE FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT WHERE PROJECT_ID = @ProjectId)
	DELETE FROM TST_REQUIREMENT WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Risks
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_RISKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_RISKS;
GO
CREATE PROCEDURE PROJECT_DELETE_RISKS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE TST_TASK SET RISK_ID = NULL WHERE RISK_ID IN (SELECT RISK_ID FROM TST_RISK WHERE PROJECT_ID = @ProjectId)
	DELETE FROM TST_RISK WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Cases & Tasks
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_TEST_CASES_TASKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_TEST_CASES_TASKS;
GO
CREATE PROCEDURE PROJECT_DELETE_TEST_CASES_TASKS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --Next we need to delete all the test cases associated with the project
    --including test steps, parameters, release/req mappings and all user navigation
    --The association between test steps and incidents will already have been handled by the
    --incident delete section
    DELETE FROM TST_TEST_STEP_PARAMETER WHERE TEST_STEP_ID IN (SELECT TEST_STEP_ID FROM TST_TEST_STEP WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId))
    DELETE FROM TST_REQUIREMENT_TEST_STEP WHERE TEST_STEP_ID IN (SELECT TEST_STEP_ID FROM TST_TEST_STEP WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId))
    DELETE FROM TST_TEST_STEP WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_REQUIREMENT_TEST_CASE WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_RELEASE_TEST_CASE WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId
    UPDATE TST_TEST_CASE_FOLDER SET PARENT_TEST_CASE_FOLDER_ID = NULL WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_TEST_CASE_FOLDER WHERE PROJECT_ID = @ProjectId

    --Now we need to delete all the project task folders (tasks are cascaded automatically)
    UPDATE TST_TASK_FOLDER SET PARENT_TASK_FOLDER_ID = NULL WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_TASK_FOLDER WHERE PROJECT_ID = @ProjectId
	DELETE FROM TST_VERSION_CONTROL_PULL_REQUEST WHERE TASK_ID IN (SELECT TASK_ID FROM TST_TASK WHERE PROJECT_ID = @ProjectId)
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Runs
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_TEST_RUNS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_TEST_RUNS;
GO
CREATE PROCEDURE PROJECT_DELETE_TEST_RUNS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --We need to delete all the test runs associated with the project
    --including all test run steps and user navigation data as well as any pending entries
    DELETE FROM TST_TEST_RUN_STEP WHERE TEST_RUN_ID IN (SELECT TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId))
    DELETE FROM TST_TEST_RUN WHERE TEST_CASE_ID IN (SELECT TEST_CASE_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_RUNS_PENDING WHERE PROJECT_ID = @ProjectId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Deletes Project Test Sets
-- =============================================
IF OBJECT_ID ( 'PROJECT_DELETE_TEST_SETS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_DELETE_TEST_SETS;
GO
CREATE PROCEDURE PROJECT_DELETE_TEST_SETS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
    --We need to delete all the test sets associated with the project
    --including all mapped test cases and user navigation data
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER WHERE TEST_SET_TEST_CASE_ID IN (SELECT TEST_SET_TEST_CASE_ID FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_ID IN (SELECT TEST_SET_ID FROM TST_TEST_SET WHERE PROJECT_ID = @ProjectId))
    DELETE FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_ID IN (SELECT TEST_SET_ID FROM TST_TEST_SET WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_SET_PARAMETER WHERE TEST_SET_ID IN (SELECT TEST_SET_ID FROM TST_TEST_SET WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_SET_FOLDER WHERE PROJECT_ID = @ProjectId
    DELETE FROM TST_TEST_SET WHERE PROJECT_ID = @ProjectId
    
    --Now we can safely delete all of the test configuration sets
    DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE WHERE TEST_CONFIGURATION_SET_ID IN (SELECT TEST_CONFIGURATION_SET_ID FROM TST_TEST_CONFIGURATION_SET WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_CONFIGURATION WHERE TEST_CONFIGURATION_SET_ID IN (SELECT TEST_CONFIGURATION_SET_ID FROM TST_TEST_CONFIGURATION_SET WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER WHERE TEST_CONFIGURATION_SET_ID IN (SELECT TEST_CONFIGURATION_SET_ID FROM TST_TEST_CONFIGURATION_SET WHERE PROJECT_ID = @ProjectId)
    DELETE FROM TST_TEST_CONFIGURATION_SET WHERE PROJECT_ID = @ProjectId
END
GO
-- ===================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Refreshes the requirement count, overall percent complete for the project and also
--					rolls up to the parent program and portfolio (if any)
-- ===================================================================================================
IF OBJECT_ID ( 'PROJECT_REFRESH_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_REFRESH_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE PROJECT_REFRESH_REQUIREMENT_COMPLETION
	@ProjectId INT
AS
DECLARE
	@ProjectGroupId INT,
	@PortfolioId INT
BEGIN
	--First we update the requirement count and % complete from only the active releases
	--We also have to account for the major releases that contain minor releases/sprints/phases
	--and avoid double-counting
	MERGE TST_PROJECT AS TARGET	
	USING (
		SELECT
			@ProjectId AS PROJECT_ID,
			REQ.IS_DELETED,
			SUM(ISNULL(REQ.ESTIMATE_POINTS,0)) AS REQUIREMENT_POINTS,
			COUNT(REQ.REQUIREMENT_ID) AS REQUIREMENT_COUNT,
			SUM((CASE WHEN REQ.REQUIREMENT_STATUS_ID IN
			(
				9, /*Tested*/
				10, /*Completed*/
				13 /*Released*/
			)
			THEN 1 ELSE 0 END)* 100) / COUNT(REQ.REQUIREMENT_ID) AS PERCENT_COMPLETE
		FROM
			TST_REQUIREMENT REQ
			INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE
			REQ.PROJECT_ID = @ProjectId AND
			REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) AND
			REQ.REQUIREMENT_STATUS_ID NOT IN (6 /* Rejected */, 8 /* Obsolete*/) AND
			REQ.IS_DELETED = 0
		GROUP BY REQ.IS_DELETED
	) AS SOURCE
	ON
		TARGET.PROJECT_ID = SOURCE.PROJECT_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
				TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_ID = @ProjectId THEN
			UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = 0,
				TARGET.PERCENT_COMPLETE = 0;
				
	--Next we update the min start-date and max end-date from all the "active" releases
	MERGE TST_PROJECT AS TARGET	
	USING (
		SELECT
			REL.PROJECT_ID,
			MIN(REL.START_DATE) AS MIN_START_DATE,
			MAX(REL.END_DATE) AS MAX_END_DATE
		FROM
			TST_RELEASE REL
		WHERE
			REL.PROJECT_ID = @ProjectId AND
			REL.IS_DELETED = 0 AND
			REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/)
		GROUP BY REL.PROJECT_ID
	) AS SOURCE
	ON
		TARGET.PROJECT_ID = SOURCE.PROJECT_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.START_DATE = SOURCE.MIN_START_DATE,
				TARGET.END_DATE = SOURCE.MAX_END_DATE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_ID = @ProjectId THEN
			UPDATE
			SET	
				TARGET.START_DATE = NULL,
				TARGET.END_DATE = NULL;
				
	--Next we update the program with the percentage complete and requirement counts
	SELECT @ProjectGroupId = PROJECT_GROUP_ID FROM TST_PROJECT WHERE PROJECT_ID = @ProjectId;
	MERGE TST_PROJECT_GROUP AS TARGET	
	USING (
		SELECT
			PRJ.PROJECT_GROUP_ID,
			SUM(PRJ.REQUIREMENT_COUNT) AS REQUIREMENT_COUNT,
			FLOOR(SUM(CAST(PRJ.PERCENT_COMPLETE AS DECIMAL(11,1)) * CAST(PRJ.REQUIREMENT_COUNT AS DECIMAL(11,1))) / SUM(CAST(PRJ.REQUIREMENT_COUNT AS DECIMAL(11,1)))) AS PERCENT_COMPLETE
		FROM
			TST_PROJECT PRJ
		WHERE
			PRJ.PROJECT_GROUP_ID = @ProjectGroupId AND
			PRJ.IS_ACTIVE = 1 AND
			PRJ.REQUIREMENT_COUNT > 0
		GROUP BY PRJ.PROJECT_GROUP_ID
	) AS SOURCE
	ON
		TARGET.PROJECT_GROUP_ID = SOURCE.PROJECT_GROUP_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
				TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_GROUP_ID = @ProjectGroupId THEN
			UPDATE
			SET	
				TARGET.REQUIREMENT_COUNT = 0,
				TARGET.PERCENT_COMPLETE = 0;
				
	--Next we update the program with the min/max start/end dates
	MERGE TST_PROJECT_GROUP AS TARGET	
	USING (
		SELECT
			PRJ.PROJECT_GROUP_ID,
			MIN(PRJ.START_DATE) AS MIN_START_DATE,
			MAX(PRJ.END_DATE) AS MAX_END_DATE
		FROM
			TST_PROJECT PRJ
		WHERE
			PRJ.PROJECT_GROUP_ID = @ProjectGroupId AND
			PRJ.IS_ACTIVE = 1 AND
			PRJ.START_DATE IS NOT NULL AND
			PRJ.END_DATE IS NOT NULL
		GROUP BY PRJ.PROJECT_GROUP_ID
	) AS SOURCE
	ON
		TARGET.PROJECT_GROUP_ID = SOURCE.PROJECT_GROUP_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.START_DATE = SOURCE.MIN_START_DATE,
				TARGET.END_DATE = SOURCE.MAX_END_DATE
	WHEN NOT MATCHED BY SOURCE AND TARGET.PROJECT_GROUP_ID = @ProjectGroupId THEN
			UPDATE
			SET	
				TARGET.START_DATE = NULL,
				TARGET.END_DATE = NULL;
				
	--Next we update the portfolio (if exists) with the percentage complete and requirement counts
	--We do it from the product counts (not program) to reduce accumulated rounding errors
	SELECT @PortfolioId = PORTFOLIO_ID FROM TST_PROJECT_GROUP WHERE PROJECT_GROUP_ID = @ProjectGroupId;
	IF @PortfolioId IS NOT NULL
	BEGIN
		MERGE TST_PORTFOLIO AS TARGET	
		USING (
			SELECT
				PRG.PORTFOLIO_ID,
				SUM(PRJ.REQUIREMENT_COUNT) AS REQUIREMENT_COUNT,
				FLOOR(SUM(CAST(PRJ.PERCENT_COMPLETE AS DECIMAL(11,1)) * CAST(PRJ.REQUIREMENT_COUNT AS DECIMAL(11,1))) / SUM(CAST(PRJ.REQUIREMENT_COUNT AS DECIMAL(11,1)))) AS PERCENT_COMPLETE
			FROM
				TST_PROJECT PRJ
				INNER JOIN TST_PROJECT_GROUP PRG ON PRJ.PROJECT_GROUP_ID = PRG.PROJECT_GROUP_ID
			WHERE
				PRG.PORTFOLIO_ID = @PortfolioId AND
				PRG.IS_ACTIVE = 1 AND
				PRJ.IS_ACTIVE = 1 AND
				PRJ.REQUIREMENT_COUNT > 0				
			GROUP BY PRG.PORTFOLIO_ID
		) AS SOURCE
		ON
			TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
		WHEN MATCHED THEN
			UPDATE
				SET	
					TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
					TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
		WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
				UPDATE
				SET	
					TARGET.REQUIREMENT_COUNT = 0,
					TARGET.PERCENT_COMPLETE = 0;
					
		--Next we update the portfolio with the min/max start/end dates
		--We can do this directly from the programs, since no worry about rounding
		MERGE TST_PORTFOLIO AS TARGET	
		USING (
			SELECT
				PRG.PORTFOLIO_ID,
				MIN(PRG.START_DATE) AS MIN_START_DATE,
				MAX(PRG.END_DATE) AS MAX_END_DATE
			FROM
				TST_PROJECT_GROUP PRG
			WHERE
				PRG.PORTFOLIO_ID = @PortfolioId AND
				PRG.IS_ACTIVE = 1 AND
				PRG.START_DATE IS NOT NULL AND
				PRG.END_DATE IS NOT NULL
			GROUP BY PRG.PORTFOLIO_ID
		) AS SOURCE
		ON
			TARGET.PORTFOLIO_ID = SOURCE.PORTFOLIO_ID
		WHEN MATCHED THEN
			UPDATE
				SET	
					TARGET.START_DATE = SOURCE.MIN_START_DATE,
					TARGET.END_DATE = SOURCE.MAX_END_DATE
		WHEN NOT MATCHED BY SOURCE AND TARGET.PORTFOLIO_ID = @PortfolioId THEN
				UPDATE
				SET	
					TARGET.START_DATE = NULL,
					TARGET.END_DATE = NULL;
	END
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Retrieves the project resources for a specific release/iteration
-- ================================================================
IF OBJECT_ID ( 'PROJECT_RETRIEVE_RESOURCES_BY_RELEASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_RETRIEVE_RESOURCES_BY_RELEASE;
GO
CREATE PROCEDURE PROJECT_RETRIEVE_RESOURCES_BY_RELEASE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)
	DECLARE @ReturnValue INT
	
	--Populate list of child iterations
	INSERT @ReleaseList (RELEASE_ID)
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	
	SELECT	PRU.PROJECT_ID, PRU.USER_ID, MIN(USP.FIRST_NAME + ' ' + USP.LAST_NAME) AS FULL_NAME,
			MIN(PRU.PROJECT_ROLE_ID) AS PROJECT_ROLE_ID, MIN(PRR.NAME) AS PROJECT_ROLE_NAME,
			SUM(ISNULL(RES.INCIDENT_EFFORT,0)) AS INCIDENT_EFFORT, SUM(ISNULL(RES.REQ_TASK_EFFORT,0)) AS REQ_TASK_EFFORT,
			SUM(ISNULL(RES.INCIDENT_EFFORT_OPEN,0)) AS INCIDENT_EFFORT_OPEN, SUM(ISNULL(RES.REQ_TASK_EFFORT_OPEN,0)) AS REQ_TASK_EFFORT_OPEN
	FROM (
		--We want the totals for open and all incident
		SELECT  INC.PROJECT_ID, INC.OWNER_ID AS USER_ID, 
				INC.PROJECTED_EFFORT AS INCIDENT_EFFORT, 0 AS REQ_TASK_EFFORT,
				(CASE WHEN IST.IS_OPEN_STATUS = 1 THEN INC.PROJECTED_EFFORT ELSE 0 END) AS INCIDENT_EFFORT_OPEN,
				0 AS REQ_TASK_EFFORT_OPEN
		FROM	TST_INCIDENT INC INNER JOIN TST_INCIDENT_STATUS IST
		ON		INC.INCIDENT_STATUS_ID = IST.INCIDENT_STATUS_ID
		WHERE	INC.IS_DELETED = 0 AND INC.RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
		UNION ALL
		--We want the totals for incomplete and all tasks (ignore deferred)
		SELECT  TSK.PROJECT_ID, TSK.OWNER_ID AS USER_ID,
				0 AS INCIDENT_EFFORT, TSK.PROJECTED_EFFORT AS REQ_TASK_EFFORT,
				0 AS INCIDENT_EFFORT_OPEN,
				(CASE WHEN TSK.TASK_STATUS_ID <> 3 THEN TSK.PROJECTED_EFFORT ELSE 0 END) AS REQ_TASK_EFFORT_OPEN
		FROM	TST_TASK TSK
		WHERE	TSK.IS_DELETED = 0 AND TSK.TASK_STATUS_ID <> 5 AND
				TSK.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
		UNION ALL
		--We need to get the totals for in-progress and completed requirements (ignore rejected,under review, requested)
		--Ignore any requirements that do have a task effort set
		SELECT  REQ.PROJECT_ID, REQ.OWNER_ID AS USER_ID,
				0 AS INCIDENT_EFFORT, REQ.ESTIMATED_EFFORT AS REQ_TASK_EFFORT,
				0 AS INCIDENT_EFFORT_OPEN,
				(CASE WHEN REQ.REQUIREMENT_STATUS_ID IN (2,3,5) THEN REQ.ESTIMATED_EFFORT ELSE 0 END) AS REQ_TASK_EFFORT_OPEN
		FROM	TST_REQUIREMENT REQ
		WHERE	REQ.IS_DELETED = 0 AND REQ.REQUIREMENT_STATUS_ID IN (2,3,4,5,8,9,10) AND
				(REQ.TASK_PROJECTED_EFFORT IS NULL OR REQ.TASK_PROJECTED_EFFORT = 0) AND
				REQ.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
		) AS RES RIGHT JOIN TST_PROJECT_USER PRU
	ON	RES.PROJECT_ID = PRU.PROJECT_ID AND RES.USER_ID = PRU.USER_ID INNER JOIN TST_USER USR
	ON	PRU.USER_ID = USR.USER_ID INNER JOIN TST_USER_PROFILE USP
	ON	USR.USER_ID = USP.USER_ID INNER JOIN TST_PROJECT_ROLE PRR
	ON	PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
	WHERE USR.IS_ACTIVE = 1 AND PRU.PROJECT_ID = @ProjectId
	GROUP BY PRU.USER_ID, PRU.PROJECT_ID
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Project
-- Description:		Retrieves the view permissions for a specific user
-- ================================================================
IF OBJECT_ID ( 'PROJECT_RETRIEVE_VIEW_PERMISSIONS', 'P' ) IS NOT NULL 
    DROP PROCEDURE PROJECT_RETRIEVE_VIEW_PERMISSIONS;
GO
CREATE PROCEDURE PROJECT_RETRIEVE_VIEW_PERMISSIONS
	@UserId INT
AS
BEGIN
	SELECT PRU.PROJECT_ID, PRP.ARTIFACT_TYPE_ID
	FROM VW_PROJECT_USER PRU INNER JOIN TST_PROJECT_ROLE_PERMISSION PRP
	ON	PRU.PROJECT_ROLE_ID = PRP.PROJECT_ROLE_ID
	WHERE	PRU.USER_ID = @UserId
	AND     PRP.PERMISSION_ID = 4 --View
	ORDER BY PRU.PROJECT_ID, PRP.ARTIFACT_TYPE_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Collapses all the levels of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'RELEASE_COLLAPSE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_COLLAPSE;
GO
CREATE PROCEDURE RELEASE_COLLAPSE
	@UserId INT,
	@ProjectId INT,
	@SummaryReleaseId INT
AS
BEGIN
	DECLARE @Length INT,
	@NewIsVisible BIT,
	@NewIsExpanded BIT,
	@ReleaseId INT,
	@IsSummary BIT,
	@IsVisible BIT,
	@IsExpanded BIT,
	@ReleaseCount INT,
	@IndentLevel NVARCHAR(100),
	@IsParentSummary BIT,
	@IsParentExpanded BIT,
	@IsParentVisible BIT
				
	--First we need to retrieve the release to make sure it is an expanded summary one
	SET @IsParentSummary = (SELECT IS_SUMMARY FROM TST_RELEASE WHERE RELEASE_ID = @SummaryReleaseId)
	SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
	IF @ReleaseCount = 0
	BEGIN
		SET @IsParentExpanded = 1
		SET @IsParentVisible = 1
	END
	ELSE
	BEGIN
		SET @IsParentExpanded = (SELECT IS_EXPANDED FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
		SET @IsParentVisible = (SELECT IS_VISIBLE FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
	END
	SET @IndentLevel = (SELECT INDENT_LEVEL FROM TST_RELEASE WHERE RELEASE_ID = @SummaryReleaseId)

	IF @IsParentSummary = 1 AND @IsParentExpanded = 1
	BEGIN	
		SET @Length = LEN(@IndentLevel)

		--Collapse the parent folder to start with
		SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId)
		IF @ReleaseCount = 0
		BEGIN
			INSERT INTO TST_RELEASE_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID) VALUES (@UserId, 0, @IsParentVisible, @SummaryReleaseId)
		END
		ELSE
		BEGIN
			UPDATE TST_RELEASE_USER SET IS_EXPANDED = 0 WHERE (RELEASE_ID = @SummaryReleaseId AND USER_ID = @UserId);
		END
			
		--Get all its child items and make them non-visible, collapsing any folders as well

		--Update settings
		UPDATE TST_RELEASE_USER
			SET IS_VISIBLE = 0, IS_EXPANDED = 0
			WHERE USER_ID = @UserId
			AND RELEASE_ID IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND IS_DELETED = 0
				)
		--Insert settings
		INSERT INTO TST_RELEASE_USER
			(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
		SELECT @UserId, 0, 0, RELEASE_ID
		FROM TST_RELEASE
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND RELEASE_ID NOT IN (
					SELECT RELEASE_ID
					FROM TST_RELEASE_USER
					WHERE USER_ID = @UserId)
				AND IS_DELETED = 0
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the count of releases for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_COUNT;
GO
CREATE PROCEDURE RELEASE_COUNT
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@IncludeDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @DeletedClause NVARCHAR(MAX)
BEGIN
	SET @DeletedClause = ''
	IF @IncludeDeleted = 0
	BEGIN
		SET @DeletedClause = 'AND REL.IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	IF @UserId IS NULL OR @UserId < 1
	BEGIN
		SET @SQL = '
SELECT	COUNT(REL.RELEASE_ID) AS ARTIFACT_COUNT
FROM	VW_RELEASE_LIST REL 
WHERE	REL.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
' + ISNULL(@Filters,'') + ' ' + @DeletedClause
	END
	ELSE
	BEGIN
		SET @SQL = '
SELECT	COUNT(REL.RELEASE_ID) AS ARTIFACT_COUNT
FROM	VW_RELEASE_LIST REL LEFT JOIN (SELECT * FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST(@UserId AS NVARCHAR) + ') AS RLU
ON		REL.RELEASE_ID = RLU.RELEASE_ID
WHERE	(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL)
AND    REL.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
' + ISNULL(@Filters,'') + ' ' + @DeletedClause
	END
	EXEC (@SQL)
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes a Release
-- =============================================
IF OBJECT_ID ( 'RELEASE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_DELETE;
GO
CREATE PROCEDURE RELEASE_DELETE
	@ReleaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete any data-mapping entries
	DELETE FROM TST_DATA_SYNC_ARTIFACT_MAPPING
	WHERE ARTIFACT_TYPE_ID = 4 /* Release */
	AND ARTIFACT_ID = @ReleaseId
	
	--Have to set the 3 Incident release fields to null
	--Cannot use cascades because of the multiple field issue
	UPDATE TST_INCIDENT SET DETECTED_RELEASE_ID = NULL WHERE DETECTED_RELEASE_ID = @ReleaseId;
	UPDATE TST_INCIDENT SET RESOLVED_RELEASE_ID = NULL WHERE RESOLVED_RELEASE_ID = @ReleaseId;
	UPDATE TST_INCIDENT SET VERIFIED_RELEASE_ID = NULL WHERE VERIFIED_RELEASE_ID = @ReleaseId;
	
	--Have to unset any builds linked to the release, cannot use cascades due to
	--SQL Server limitations
	UPDATE TST_INCIDENT SET RESOLVED_BUILD_ID = NULL WHERE RESOLVED_BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	UPDATE TST_INCIDENT SET DETECTED_BUILD_ID = NULL WHERE DETECTED_BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	UPDATE TST_TEST_RUN SET BUILD_ID = NULL WHERE BUILD_ID IN
		(SELECT BUILD_ID FROM TST_BUILD WHERE RELEASE_ID = @ReleaseId)
	
	--Now delete the release itself
    DELETE FROM TST_RELEASE WHERE RELEASE_ID = @ReleaseId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes the user navigation data for all releases for a given user
-- =============================================
IF OBJECT_ID ( 'RELEASE_DELETE_NAVIGATION_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_DELETE_NAVIGATION_DATA;
GO
CREATE PROCEDURE RELEASE_DELETE_NAVIGATION_DATA
	@UserId INT
AS
BEGIN
	--Now delete the navigation data
    DELETE FROM TST_RELEASE_USER WHERE USER_ID = @UserId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Expands one level of children of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'RELEASE_EXPAND', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_EXPAND;
GO
CREATE PROCEDURE RELEASE_EXPAND
	@UserId INT,
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	DECLARE @Length INT
	
	SET @Length = LEN(@IndentLevel)

	--We need to expand the immediate child items and make visible
	--Update settings
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND IS_DELETED = 0
			)
	--Insert settings
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Expands the hierarchy to a specific level
-- =============================================
IF OBJECT_ID ( 'RELEASE_EXPAND_TO_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_EXPAND_TO_LEVEL;
GO
CREATE PROCEDURE RELEASE_EXPAND_TO_LEVEL
	@UserId INT,
	@ProjectId INT,
	@Level INT
AS
BEGIN
	--Make all items that are the requested level or less visible, the others hidden
	
	--Show
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) <= (@Level * 3) 
			AND IS_DELETED = 0
			)
			
	--Hide
	UPDATE TST_RELEASE_USER
		SET IS_VISIBLE = 0
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3) 
			AND IS_DELETED = 0
			)
	
	--Those folder items that are less than the requested level only, expand
	
	--Expand
	UPDATE TST_RELEASE_USER
		SET IS_EXPANDED = 1
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3) 
			AND IS_DELETED = 0
			AND IS_SUMMARY = 1
			)
			
	--Collapse
	UPDATE TST_RELEASE_USER
		SET IS_EXPANDED = 0
		WHERE USER_ID = @UserId
		AND RELEASE_ID IN (
			SELECT RELEASE_ID
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND (LEN(INDENT_LEVEL) >= (@Level * 3) OR IS_SUMMARY = 0)
			AND IS_DELETED = 0
			)

	--Now do the inserts for the case where user has no existing settings
	--Visible and Expanded
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 1, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_SUMMARY = 1
			AND IS_DELETED = 0
			
	--Visible but not Expanded (2-cases)
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_SUMMARY = 0
			AND IS_DELETED = 0
			
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 1, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) = (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
			
	--Hidden and Collapsed
	INSERT INTO TST_RELEASE_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
	SELECT @UserId, 0, 0, RELEASE_ID
	FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3)
			AND RELEASE_ID NOT IN (
				SELECT RELEASE_ID
				FROM TST_RELEASE_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Returns the next available indent level (for new inserts)
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL;
GO
CREATE PROCEDURE RELEASE_GET_NEXT_AVAILABLE_INDENT_LEVEL
	@ProjectId INT,
	@UserId INT,
	@IgnoreLastInserted BIT
AS
BEGIN
	--See if they want to insert at the root level or directly under the last inserted item
	IF @IgnoreLastInserted = 1
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_RELEASE REL
			LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = @UserId) AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) AND
			LEN(INDENT_LEVEL) = 3
		ORDER BY REL.INDENT_LEVEL DESC, REL.RELEASE_ID
	END
	ELSE
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_RELEASE REL
			LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = @UserId) AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL)
		ORDER BY REL.INDENT_LEVEL DESC, REL.RELEASE_ID
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the parent release of the current release/iteration (if there is one)
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_PARENT_RELEASE_ID', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_PARENT_RELEASE_ID;
GO
CREATE PROCEDURE RELEASE_GET_PARENT_RELEASE_ID
	@ReleaseOrIterationId INT
AS
DECLARE
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
BEGIN
	--Get the indent level of the passed in release/iteration
	SELECT
		@IndentLevel = INDENT_LEVEL,
		@ProjectId = PROJECT_ID
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseOrIterationId
	
	--Get the parent of this
	SELECT RELEASE_ID
	FROM TST_RELEASE
	WHERE
		INDENT_LEVEL = SUBSTRING(@IndentLevel, 1, LEN(@IndentLevel)-3) AND
		PROJECT_ID = @ProjectId
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Returns the previous peer for the given indent level
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_GET_PREVIOUS_PEER', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_PREVIOUS_PEER;
GO
CREATE PROCEDURE RELEASE_GET_PREVIOUS_PEER
	@ProjectId INT,
	@IndentLevel NVARCHAR(100),
	@IncludeDeleted BIT
AS
BEGIN
	--Find the first release that is just before the current one
	SELECT TOP 1 INDENT_LEVEL
	FROM TST_RELEASE
	WHERE LEN(INDENT_LEVEL) = LEN(@IndentLevel)
		AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
		AND PROJECT_ID = @ProjectId
		AND INDENT_LEVEL < @IndentLevel
	ORDER BY INDENT_LEVEL DESC
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of items under a release
--					as well as the release itself
-- ================================================================
IF OBJECT_ID ( 'RELEASE_GET_SELF_AND_CHILDREN', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_SELF_AND_CHILDREN;
GO
CREATE PROCEDURE RELEASE_GET_SELF_AND_CHILDREN
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeMajorBranches BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT
	DECLARE @ChildMajorReleases TABLE
	(
		INDENT_LEVEL NVARCHAR(100)
	)

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId
    	IF (@IndentLevel IS NULL)
    	BEGIN
			RETURN
		END

	SET @IndentLevelLength = LEN(@IndentLevel)
	IF @IncludeMajorBranches = 1
	BEGIN
		--Now get the list of all children and the release itself
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
		ORDER BY INDENT_LEVEL
	END
	ELSE
	BEGIN
		--Get a list of all the child major releases (that we need to ignore)
		INSERT INTO @ChildMajorReleases
			SELECT INDENT_LEVEL
			FROM TST_RELEASE
			WHERE PROJECT_ID = @ProjectId
			AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
			AND LEN(INDENT_LEVEL) > @IndentLevelLength
			AND RELEASE_TYPE_ID = 1 /* Major Release */
				
		--Now get the list of all children and the release itself
		--ignoring those that are parents of the child major releases
		SELECT RELEASE_ID
		FROM TST_RELEASE
		WHERE PROJECT_ID = @ProjectId
		AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
		AND (RELEASE_ID = @ReleaseId OR (LEN(INDENT_LEVEL) > @IndentLevelLength))
		AND RELEASE_ID NOT IN (
			SELECT REL.RELEASE_ID FROM TST_RELEASE REL INNER JOIN @ChildMajorReleases MAJ
			ON dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(REL.INDENT_LEVEL, MAJ.INDENT_LEVEL, 100) = 1
			WHERE REL.PROJECT_ID = @ProjectId AND REL.RELEASE_ID <> @ReleaseId)
		ORDER BY INDENT_LEVEL	
	END
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of iterations under a release
--					as well as the release itself
-- ================================================================
IF OBJECT_ID ( 'RELEASE_GET_SELF_AND_ITERATIONS', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_GET_SELF_AND_ITERATIONS;
GO
CREATE PROCEDURE RELEASE_GET_SELF_AND_ITERATIONS
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeDeleted BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_RELEASE
	WHERE RELEASE_ID = @ReleaseId

	IF (@IndentLevel IS NULL)
	BEGIN
		--Need to have the column returns, so just use a fake query
		SELECT RELEASE_ID FROM TST_RELEASE WHERE RELEASE_ID = -1
		RETURN
	END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of iterations and the parent release itself
	SELECT RELEASE_ID
	FROM TST_RELEASE
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
	AND (RELEASE_ID = @ReleaseId OR (RELEASE_TYPE_ID = 3 /* Iteration */ AND LEN(INDENT_LEVEL) = (@IndentLevelLength + 3)))
	AND (IS_DELETED = 0 OR @IncludeDeleted = 1)
	ORDER BY INDENT_LEVEL
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Inserts a new 'filler' release when we're fixing any hierarchy errors
-- =============================================
IF OBJECT_ID ( 'RELEASE_INSERT_FILLER', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_INSERT_FILLER;
GO
CREATE PROCEDURE RELEASE_INSERT_FILLER
	@ProjectId INT,
	@Name NVARCHAR(255),
	@IndentLevel NVARCHAR(100),
	@VersionNumber NVARCHAR(20)
AS
BEGIN
	--Now insert the filler release
    INSERT INTO TST_RELEASE (CREATOR_ID, PROJECT_ID, RELEASE_STATUS_ID, RELEASE_TYPE_ID, NAME, VERSION_NUMBER, CREATION_DATE, INDENT_LEVEL, LAST_UPDATE_DATE, CONCURRENCY_DATE, START_DATE, END_DATE, RESOURCE_COUNT, IS_SUMMARY, IS_ATTACHMENTS, IS_DELETED, DAYS_NON_WORKING, PLANNED_EFFORT, AVAILABLE_EFFORT, COUNT_BLOCKED, COUNT_CAUTION, COUNT_FAILED, COUNT_NOT_APPLICABLE, COUNT_NOT_RUN, COUNT_PASSED, TASK_COUNT, TASK_PERCENT_ON_TIME, TASK_PERCENT_LATE_FINISH, TASK_PERCENT_NOT_START, TASK_PERCENT_LATE_START, PERCENT_COMPLETE)
	VALUES (1, @ProjectId, 1, 1, @Name, @VersionNumber, GETUTCDATE(), @IndentLevel, GETUTCDATE(), GETUTCDATE(), GETUTCDATE(), GETUTCDATE(), 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
END
GO
-- =============================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the summary task progress and task estimated/actual effort for a particular
--					release/iteration
-- =============================================================================================================
IF OBJECT_ID ( 'RELEASE_REFRESH_PROGRESS_AND_EFFORT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REFRESH_PROGRESS_AND_EFFORT;
GO
CREATE PROCEDURE RELEASE_REFRESH_PROGRESS_AND_EFFORT
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeTaskEffort BIT,
	@IncludeIncidentEffort BIT,
	@IncludeTestCaseEffort BIT
AS
BEGIN
	DECLARE
		@Task_Count INT,
		@Task_PercentOnTime INT,
		@Task_PercentLateFinish INT,
		@Task_PercentNotStart1 INT,
		@Task_PercentNotStart2 INT,
		@Task_PercentLateStart INT,
		@Task_ProjectedEffort INT
		
	--This temp table is being used later to cache list of child releases	
	DECLARE @tmpChildReleases AS TABLE 
	(
		RELEASE_ID INT
	);
	
	--Get the list of child requirements from function
	DELETE FROM @tmpChildReleases;
	INSERT INTO @tmpChildReleases
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_CHILDREN(@ProjectId, @ReleaseId, 0);
		
	--First we need to update the effort values
	MERGE TST_RELEASE AS TARGET	
	USING (
		--We add on the requirements, tasks, incidents and test case efforts as appropriate
		
		SELECT	@ReleaseId AS RELEASE_ID,
				SUM(ISNULL(TASK_ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TASK_PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(TASK_REMAINING_EFFORT,ISNULL(TASK_ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(TASK_ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM
		(
		--Requirements
		--We exclude the summary items to avoid duplicated effort
		SELECT	SUM(ISNULL(REQ.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(REQ.ESTIMATED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(CASE WHEN REQ.REQUIREMENT_STATUS_ID IN
				(
					9, /*Tested*/
					10, /*Completed*/
					13 /*Released*/
				)
				THEN 0 ELSE ISNULL(REQ.ESTIMATED_EFFORT,0) END) AS TASK_REMAINING_EFFORT,
				0 AS TASK_ACTUAL_EFFORT
		FROM TST_REQUIREMENT REQ
		WHERE REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
		AND REQ.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND REQ.REQUIREMENT_STATUS_ID NOT IN (6 /* Rejected */, 8 /* Obsolete*/)
		AND REQ.PROJECT_ID = @ProjectId
		AND REQ.TASK_COUNT = 0
		UNION ALL
		--Tasks
		SELECT	SUM(ISNULL(TSK.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TSK.PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(TSK.REMAINING_EFFORT,ISNULL(TSK.ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(TSK.ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM TST_TASK TSK
		WHERE TSK.IS_DELETED = 0
		AND TSK.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND TSK.TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
		AND TSK.PROJECT_ID = @ProjectId
		AND @IncludeTaskEffort = 1
		UNION ALL
		--Incidents
		SELECT	SUM(ISNULL(INC.ESTIMATED_EFFORT,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(INC.PROJECTED_EFFORT,0)) AS TASK_PROJECTED_EFFORT,
				SUM(ISNULL(INC.REMAINING_EFFORT,ISNULL(INC.ESTIMATED_EFFORT,0))) AS TASK_REMAINING_EFFORT,
				SUM(ISNULL(INC.ACTUAL_EFFORT,0)) AS TASK_ACTUAL_EFFORT
		FROM TST_INCIDENT INC
		WHERE INC.IS_DELETED = 0
		AND INC.RESOLVED_RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND INC.PROJECT_ID = @ProjectId
		AND @IncludeIncidentEffort = 1	
		UNION ALL
		--Test Cases
		SELECT	SUM(ISNULL(TST.ESTIMATED_DURATION,0)) AS TASK_ESTIMATED_EFFORT,
				SUM(ISNULL(TST.ESTIMATED_DURATION,0)) AS TASK_PROJECTED_EFFORT,
				SUM(CASE WHEN RTC.EXECUTION_STATUS_ID IN
				(
					2, /*Passed*/
					4 /*N/A*/
				)
				THEN 0 ELSE ISNULL(TST.ESTIMATED_DURATION,0) END) AS TASK_REMAINING_EFFORT,
				0 AS TASK_ACTUAL_EFFORT
		FROM TST_TEST_CASE TST
		INNER JOIN TST_RELEASE_TEST_CASE RTC ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE TST.IS_DELETED = 0
		AND RTC.RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
		AND TST.TEST_CASE_STATUS_ID NOT IN (3 /* Rejected */, 6 /* Obsolete*/)
		AND TST.PROJECT_ID = @ProjectId
		AND @IncludeTestCaseEffort = 1
		) AS TOTAL_EFFORT
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.TASK_ESTIMATED_EFFORT = SOURCE.TASK_ESTIMATED_EFFORT,
				TARGET.TASK_PROJECTED_EFFORT = SOURCE.TASK_PROJECTED_EFFORT,
				TARGET.TASK_REMAINING_EFFORT = SOURCE.TASK_REMAINING_EFFORT,
				TARGET.TASK_ACTUAL_EFFORT = SOURCE.TASK_ACTUAL_EFFORT,
				TARGET.AVAILABLE_EFFORT = TARGET.PLANNED_EFFORT - SOURCE.TASK_PROJECTED_EFFORT			
	WHEN NOT MATCHED BY SOURCE AND TARGET.RELEASE_ID = @ReleaseId THEN
			UPDATE
			SET	
				TARGET.TASK_ESTIMATED_EFFORT = NULL,
				TARGET.TASK_PROJECTED_EFFORT = NULL,
				TARGET.TASK_REMAINING_EFFORT = NULL,
				TARGET.TASK_ACTUAL_EFFORT = NULL,
				TARGET.AVAILABLE_EFFORT = TARGET.PLANNED_EFFORT;
				
		--Now we need to get a count of the tasks for the release and its children
		SELECT @Task_Count = COUNT(TASK_ID) FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
		SELECT @Task_ProjectedEffort = TASK_PROJECTED_EFFORT
		FROM TST_RELEASE
		WHERE RELEASE_ID = @ReleaseId;
		
		--If we have at least one task, need to update the Progress indicator percentages
		IF @Task_Count > 0
		BEGIN
			SELECT @Task_PercentLateFinish = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND TASK_STATUS_ID <> 5 /*Deferred*/
								AND COMPLETION_PERCENT < 100 AND (COMPLETION_PERCENT > 0 OR TASK_STATUS_ID = 2) AND END_DATE < GETUTCDATE()
			SELECT @Task_PercentNotStart1 = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT = 0 AND (START_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 5 /*Deferred*/) AND TASK_STATUS_ID IN (1,4,5)
			SELECT @Task_PercentLateStart = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT = 0 AND START_DATE < GETUTCDATE() AND TASK_STATUS_ID IN (1,4)		
			SELECT @Task_PercentOnTime = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE RELEASE_ID IN (SELECT RELEASE_ID FROM @tmpChildReleases)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID NOT IN (6 /* Rejected */, 9 /* Obsolete*/, 7 /* Duplicate */)
								AND COMPLETION_PERCENT > 0 AND (END_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 3 /*Completed*/)		
			SET @Task_PercentNotStart2 = 100 - (ISNULL(@Task_PercentLateFinish,0) + ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentOnTime,0) + ISNULL(@Task_PercentLateStart,0))
		END
		ELSE
		BEGIN
			SET @Task_PercentOnTime = 0
			SET @Task_PercentLateFinish = 0
			SET @Task_PercentNotStart1 = 0
			SET @Task_PercentNotStart2 = 0
			SET @Task_PercentLateStart = 0
		END
		
		--Update the release
		UPDATE TST_RELEASE
			SET			
				TASK_COUNT = @Task_Count,
				TASK_PERCENT_ON_TIME = ISNULL(@Task_PercentOnTime,0),
				TASK_PERCENT_LATE_FINISH = ISNULL(@Task_PercentLateFinish,0),
				TASK_PERCENT_NOT_START = ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentNotStart2,0),
				TASK_PERCENT_LATE_START = ISNULL(@Task_PercentLateStart,0)			
		WHERE RELEASE_ID = @ReleaseId
END
GO
-- =============================================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the requirement count, overall percent complete, and the total requirement points
-- =============================================================================================================
IF OBJECT_ID ( 'RELEASE_REFRESH_REQUIREMENT_COMPLETION', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REFRESH_REQUIREMENT_COMPLETION;
GO
CREATE PROCEDURE RELEASE_REFRESH_REQUIREMENT_COMPLETION
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	MERGE TST_RELEASE AS TARGET	
	USING (
		SELECT
			@ReleaseId AS RELEASE_ID,
			IS_DELETED,
			SUM(ISNULL(REQ.ESTIMATE_POINTS,0)) AS REQUIREMENT_POINTS,
			COUNT(REQ.REQUIREMENT_ID) AS REQUIREMENT_COUNT,
			SUM((CASE WHEN REQ.REQUIREMENT_STATUS_ID IN
			(
				9, /*Tested*/
				10, /*Completed*/
				13 /*Released*/
			)
			THEN 1 ELSE 0 END)* 100) / COUNT(REQ.REQUIREMENT_ID) AS PERCENT_COMPLETE
		FROM
			TST_REQUIREMENT REQ
		WHERE
			REQ.PROJECT_ID = @ProjectId AND
			REQ.RELEASE_ID IN (SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_CHILDREN(@ProjectId, @ReleaseId, 0)) AND
			REQ.REQUIREMENT_STATUS_ID NOT IN (6 /* Rejected */, 8 /* Obsolete*/) AND
			REQ.IS_DELETED = 0
		GROUP BY IS_DELETED
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.REQUIREMENT_POINTS = SOURCE.REQUIREMENT_POINTS,
				TARGET.REQUIREMENT_COUNT = SOURCE.REQUIREMENT_COUNT,
				TARGET.PERCENT_COMPLETE = SOURCE.PERCENT_COMPLETE
	WHEN NOT MATCHED BY SOURCE AND TARGET.RELEASE_ID = @ReleaseId THEN
			UPDATE
			SET	
				TARGET.REQUIREMENT_POINTS = NULL,
				TARGET.REQUIREMENT_COUNT = 0,
				TARGET.PERCENT_COMPLETE = 0;
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the release-test-case counts
-- =======================================================
IF OBJECT_ID ( 'RELEASE_REFRESH_TESTCASE_COUNTS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [RELEASE_REFRESH_TESTCASE_COUNTS];
GO
CREATE PROCEDURE [RELEASE_REFRESH_TESTCASE_COUNTS]
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN		
	SET NOCOUNT ON;

	--Now we need to update the execution info for the release/test case information	
	--Create the CTE to get the most recent test run per release
	--Use the CROSS APPLY to handle rollup release considerations
    UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE RELEASE_ID = @ReleaseId;
	
	WITH CTE AS
	(
		SELECT T1.TEST_RUN_ID, T2.RELEASE_ID, T1.END_DATE, T1.EXECUTION_STATUS_ID, T1.ACTUAL_DURATION, ROW_NUMBER() OVER
		(
			PARTITION BY T2.RELEASE_ID
			ORDER BY END_DATE DESC
		) AS TRN
		FROM TST_TEST_RUN T1
		CROSS APPLY dbo.FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS(@ProjectId, T1.RELEASE_ID) AS T2
		WHERE T1.RELEASE_ID = @ReleaseId AND T1.EXECUTION_STATUS_ID <> 3 /* Not Run */	
	)	
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN3.END_DATE,
		EXECUTION_STATUS_ID = TRN3.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN3.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		(SELECT TRN1.TEST_CASE_ID, TRN2.RELEASE_ID, TRN1.EXECUTION_STATUS_ID, TRN1.END_DATE, TRN1.ACTUAL_DURATION
		FROM TST_TEST_RUN TRN1
		INNER JOIN
			(		
				SELECT TEST_RUN_ID, RELEASE_ID, END_DATE, EXECUTION_STATUS_ID, ACTUAL_DURATION FROM CTE
				WHERE TRN = 1
			) TRN2
		ON TRN1.TEST_RUN_ID = TRN2.TEST_RUN_ID
		) TRN3
	ON TST.TEST_CASE_ID = TRN3.TEST_CASE_ID 
	AND TST.RELEASE_ID = TRN3.RELEASE_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Refreshes the test status of a particular release
-- =============================================
IF OBJECT_ID ( 'RELEASE_REFRESH_TEST_STATUS', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_REFRESH_TEST_STATUS;
GO
CREATE PROCEDURE RELEASE_REFRESH_TEST_STATUS
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	MERGE TST_RELEASE AS TARGET	
	USING (
		SELECT
			RTC.RELEASE_ID,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 1 THEN 1 ELSE 0 END) AS COUNT_FAILED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 2 THEN 1 ELSE 0 END) AS COUNT_PASSED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 3 THEN 1 ELSE 0 END) AS COUNT_NOT_RUN,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 4 THEN 1 ELSE 0 END) AS COUNT_NOT_APPLICABLE,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 5 THEN 1 ELSE 0 END) AS COUNT_BLOCKED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 6 THEN 1 ELSE 0 END) AS COUNT_CAUTION
		FROM
			TST_RELEASE_TEST_CASE RTC INNER JOIN TST_TEST_CASE TST
			ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE
			RTC.RELEASE_ID = @ReleaseId AND
			TST.IS_DELETED = 0
		GROUP BY RELEASE_ID
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID
	WHEN MATCHED THEN
		UPDATE
			SET	
				TARGET.COUNT_PASSED = SOURCE.COUNT_PASSED,
				TARGET.COUNT_FAILED = SOURCE.COUNT_FAILED,
				TARGET.COUNT_CAUTION = SOURCE.COUNT_CAUTION,
				TARGET.COUNT_BLOCKED = SOURCE.COUNT_BLOCKED,
				TARGET.COUNT_NOT_RUN = SOURCE.COUNT_NOT_RUN,
				TARGET.COUNT_NOT_APPLICABLE = SOURCE.COUNT_NOT_APPLICABLE
	WHEN NOT MATCHED BY SOURCE AND TARGET.RELEASE_ID = @ReleaseId THEN
			UPDATE
			SET	
				TARGET.COUNT_PASSED = 0,
				TARGET.COUNT_FAILED = 0,
				TARGET.COUNT_CAUTION = 0,
				TARGET.COUNT_BLOCKED = 0,
				TARGET.COUNT_NOT_RUN = 0,
				TARGET.COUNT_NOT_APPLICABLE = 0;
END
GO
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
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of filtered releases with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_RETRIEVE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_RETRIEVE;
GO
CREATE PROCEDURE RELEASE_RETRIEVE
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@IncDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_RELEASE_LIST_INTERNAL REL LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID, IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE (RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) ' +
					'AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' + ISNULL(@Filters,'')
	IF @IncDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
DECLARE @FOLDER CHAR(1)
DECLARE @EXPANDED CHAR(1)
DECLARE @INDENT NVARCHAR(100)
DECLARE @PREVINDENT  NVARCHAR(100)
SET @PageSize = ' + CAST(@NumRows AS NVARCHAR) + '

DECLARE @PK INT
DECLARE @tblNormalized TABLE
(
	PK INT NOT NULL PRIMARY KEY,
	INDENT NVARCHAR(100) NOT NULL
)
DECLARE @tblPK TABLE
(
	PK INT NOT NULL PRIMARY KEY
)
DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT RELEASE_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL

OPEN PagingCursor
FETCH RELATIVE ' + CAST(@StartRow AS NVARCHAR) + ' FROM PagingCursor INTO @PK

SET NOCOUNT ON
WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
	INSERT @tblPK (PK)  VALUES (@PK)
	FETCH NEXT FROM PagingCursor INTO @PK
	SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT REL.*,ISNULL(RLU.IS_EXPANDED, CASE REL.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RLU.IS_VISIBLE, 1) AS IS_VISIBLE FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REL.RELEASE_ID = tblPK.PK ' + @WHERE + '
ORDER BY REL.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Retrieves the list of releases for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'RELEASE_RETRIEVE_CUSTOM', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_RETRIEVE_CUSTOM;
GO
CREATE PROCEDURE RELEASE_RETRIEVE_CUSTOM
	@UserId INT,
	@ProjectId INT,
	@FilterSort NVARCHAR(MAX),
	@NumRows INT,
	@IncludeDeleted BIT,
	@OnlyShowVisible BIT
AS
	DECLARE @TOP NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE_ORDER_BY NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_RELEASE_LIST_INTERNAL REL LEFT JOIN (SELECT RELEASE_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_RELEASE_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RLU ON REL.RELEASE_ID = RLU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	IF @OnlyShowVisible = 1
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE (RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) '
	END
	ELSE
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE 1=1 '
	END

	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND REL.IS_DELETED = 0 '
	END
	
	IF @ProjectId IS NOT NULL
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' '
	END
	
	IF @FilterSort IS NOT NULL AND LEN(@FilterSort) > 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND ' + @FilterSort + ' '
	END
	
	--See if we have to limit the number of rows retrieved
	SET @TOP = ''
	IF @NumRows IS NOT NULL
	BEGIN
		SET @TOP = 'TOP ' + CAST(@NumRows AS NVARCHAR)
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
SELECT ' + @TOP + ' ISNULL(RLU.IS_EXPANDED, CASE REL.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RLU.IS_VISIBLE, 1) AS IS_VISIBLE, REL.*
FROM ' + @TABLES + ' ' + @WHERE_ORDER_BY
	EXEC (@SQL)
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Deletes test coverage
-- =============================================
IF OBJECT_ID ( 'RELEASE_SAVE_TEST_COVERAGE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_SAVE_TEST_COVERAGE_DELETE;
GO
CREATE PROCEDURE RELEASE_SAVE_TEST_COVERAGE_DELETE
	@ReleaseId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TST_RELEASE_TEST_CASE
	WHERE TEST_CASE_ID = @TestCaseId AND RELEASE_ID = @ReleaseId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Inserts test coverage
-- =============================================
IF OBJECT_ID ( 'RELEASE_SAVE_TEST_COVERAGE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_SAVE_TEST_COVERAGE_INSERT;
GO
CREATE PROCEDURE RELEASE_SAVE_TEST_COVERAGE_INSERT
	@ReleaseId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO TST_RELEASE_TEST_CASE (RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID)
	VALUES (@ReleaseId, @TestCaseId, 3 /*Not Run*/ );
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Release
-- Description:		Updates the positional data for the release
-- =============================================
IF OBJECT_ID ( 'RELEASE_UPDATE_POSITIONAL', 'P' ) IS NOT NULL 
    DROP PROCEDURE RELEASE_UPDATE_POSITIONAL;
GO
CREATE PROCEDURE RELEASE_UPDATE_POSITIONAL
	@ReleaseId INT,
	@UserId INT,
	@IsExpanded BIT,
	@IsVisible BIT,
	@IsSummary BIT,
	@IndentLevel NVARCHAR(100)	
AS
	DECLARE @ReleaseCount INT
BEGIN
	--First update the release table itself
	UPDATE TST_RELEASE
	SET	INDENT_LEVEL = @IndentLevel,
		IS_SUMMARY = @IsSummary
	WHERE RELEASE_ID = @ReleaseId

	--Now insert/update the release user navigation metadata
    SET @ReleaseCount = (SELECT COUNT(*) FROM TST_RELEASE_USER WHERE RELEASE_ID = @ReleaseId AND USER_ID = @UserId);
    IF @ReleaseCount = 0 AND @UserId IS NOT NULL
    BEGIN
		INSERT INTO TST_RELEASE_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, RELEASE_ID)
		VALUES (@UserId, @IsExpanded, @IsVisible, @ReleaseId)
	END
    ELSE
    BEGIN
		UPDATE TST_RELEASE_USER
		SET	IS_EXPANDED = @IsExpanded,
			IS_VISIBLE = @IsVisible
		WHERE RELEASE_ID = @ReleaseId
		AND USER_ID = @UserId
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Collapses all the levels of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_COLLAPSE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COLLAPSE;
GO
CREATE PROCEDURE REQUIREMENT_COLLAPSE
	@UserId INT,
	@ProjectId INT,
	@SummaryRequirementId INT
AS
BEGIN
	DECLARE @Length INT,
	@NewIsVisible BIT,
	@NewIsExpanded BIT,
	@RequirementId INT,
	@IsSummary BIT,
	@IsVisible BIT,
	@IsExpanded BIT,
	@RequirementCount INT,
	@IndentLevel NVARCHAR(100),
	@ParentIsSummary BIT,
	@ParentIsExpanded BIT,
	@ParentIsVisible BIT
				
	--First we need to retrieve the test-case to make sure it is an expanded summary one
	SET @ParentIsSummary = (SELECT IS_SUMMARY FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @SummaryRequirementId)
	SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
	IF @RequirementCount = 0
	BEGIN
		SET @ParentIsExpanded = 1
		SET @ParentIsVisible = 1
	END
	ELSE
	BEGIN
		SET @ParentIsExpanded = (SELECT IS_EXPANDED FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
		SET @ParentIsVisible = (SELECT IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
	END
	SET @IndentLevel = (SELECT INDENT_LEVEL FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @SummaryRequirementId)

	IF @ParentIsSummary = 1 AND @ParentIsExpanded = 1
	BEGIN	
		SET @Length = LEN(@IndentLevel)

		--Collapse the parent folder to start with
		SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId)
		IF @RequirementCount = 0
		BEGIN
			INSERT INTO TST_REQUIREMENT_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID) VALUES (@UserId, 0, @ParentIsVisible, @SummaryRequirementId)
		END
		ELSE
		BEGIN
			UPDATE TST_REQUIREMENT_USER SET IS_EXPANDED = 0 WHERE (REQUIREMENT_ID = @SummaryRequirementId AND USER_ID = @UserId);
		END
			
		--Get all its child items and make them non-visible, collapsing any folders as well

		--Update settings
		UPDATE TST_REQUIREMENT_USER
			SET IS_VISIBLE = 0, IS_EXPANDED = 0
			WHERE USER_ID = @UserId
			AND REQUIREMENT_ID IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND IS_DELETED = 0
				)
		--Insert settings
		INSERT INTO TST_REQUIREMENT_USER
			(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
		SELECT @UserId, 0, 0, REQUIREMENT_ID
		FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId
				AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
				AND LEN(INDENT_LEVEL) >= (@Length + 3) 
				AND REQUIREMENT_ID NOT IN (
					SELECT REQUIREMENT_ID
					FROM TST_REQUIREMENT_USER
					WHERE USER_ID = @UserId)
				AND IS_DELETED = 0
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the count of requirements for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_COUNT
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@IncludeDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @DeletedClause NVARCHAR(MAX)
BEGIN
	SET @DeletedClause = ''
	IF @IncludeDeleted = 0
	BEGIN
		SET @DeletedClause = 'AND REQ.IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	IF @UserId IS NULL OR @UserId < 1
	BEGIN
		SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ 
WHERE	REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND    (REQ.IS_SUMMARY = 1 OR (1=1 ' + @Filters + ')) ' + @DeletedClause
	END
	ELSE
	BEGIN
		SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ LEFT JOIN (SELECT * FROM TST_REQUIREMENT_USER WHERE USER_ID = ' + CAST(@UserId AS NVARCHAR) + ') AS RQU
ON		REQ.REQUIREMENT_ID = RQU.REQUIREMENT_ID
WHERE	(RQU.IS_VISIBLE = 1 OR RQU.IS_VISIBLE IS NULL)
AND    REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND    (REQ.IS_SUMMARY = 1 OR (1=1 ' + @Filters + ')) ' + @DeletedClause
	END
	EXEC (@SQL)
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the count of non-summary requirements for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_COUNT_NON_SUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_COUNT_NON_SUMMARY;
GO
CREATE PROCEDURE REQUIREMENT_COUNT_NON_SUMMARY
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@IncludeDeleted BIT
AS
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @DeletedClause NVARCHAR(MAX)
BEGIN
	SET @DeletedClause = ''
	IF @IncludeDeleted = 0
	BEGIN
		SET @DeletedClause = 'AND REQ.IS_DELETED = 0'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
SELECT	COUNT(REQ.REQUIREMENT_ID) AS ARTIFACT_COUNT
FROM	VW_REQUIREMENT_LIST REQ 
WHERE	REQ.PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + '
AND		REQ.IS_SUMMARY = 0 ' + @Filters + ' ' + @DeletedClause

	EXEC (@SQL)
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes a Requirement
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_DELETE;
GO
CREATE PROCEDURE REQUIREMENT_DELETE
	@RequirementId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete user notification subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 1 AND ARTIFACT_ID = @RequirementId);
	--Delete any linked test steps
	DELETE FROM TST_REQUIREMENT_TEST_STEP WHERE REQUIREMENT_ID = @RequirementId;
	DELETE FROM TST_REQUIREMENT_SIGNATURE WHERE REQUIREMENT_ID = @RequirementId;
	--Now delete the requirement itself
    DELETE FROM TST_REQUIREMENT WHERE REQUIREMENT_ID = @RequirementId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes the user navigation data for all requirements for a given user
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_DELETE_NAVIGATION_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_DELETE_NAVIGATION_DATA;
GO
CREATE PROCEDURE REQUIREMENT_DELETE_NAVIGATION_DATA
	@UserId INT
AS
BEGIN
	--Now delete the navigation data
    DELETE FROM TST_REQUIREMENT_USER WHERE USER_ID = @UserId
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Expands one level of children of a specific node in the hierarchy
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_EXPAND', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_EXPAND;
GO
CREATE PROCEDURE REQUIREMENT_EXPAND
	@UserId INT,
	@ProjectId INT,
	@IndentLevel NVARCHAR(100)
AS
BEGIN
	DECLARE @Length INT
	
	SET @Length = LEN(@IndentLevel)

	--We need to expand the immediate child items and make visible
	--Update settings
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND IS_DELETED = 0
			)
	--Insert settings
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND SUBSTRING(INDENT_LEVEL, 1, @Length) = @IndentLevel
			AND LEN(INDENT_LEVEL) = (@Length + 3) 
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Expands the hierarchy to a specific level
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_EXPAND_TO_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_EXPAND_TO_LEVEL;
GO
CREATE PROCEDURE REQUIREMENT_EXPAND_TO_LEVEL
	@UserId INT,
	@ProjectId INT,
	@Level INT
AS
BEGIN
	--Make all items that are the requested level or less visible, the others hidden
	
	--Show
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) <= (@Level * 3) 
			AND IS_DELETED = 0
			)
			
	--Hide
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3) 
			AND IS_DELETED = 0
			)
	
	--Those folder items that are less than the requested level only, expand
	
	--Expand
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3) 
			AND IS_DELETED = 0
			)
			
	--Collapse
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) >= (@Level * 3) 
			AND IS_DELETED = 0
			)

	--Now do the inserts for the case where user has no existing settings
	--Visible and Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 1, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) < (@Level * 3)
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Visible but not Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) = (@Level * 3)
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Hidden and Collapsed
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 0, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND LEN(INDENT_LEVEL) > (@Level * 3)
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Focuses the hierarchy on a specific branch
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_FOCUS_ON', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_FOCUS_ON;
GO
CREATE PROCEDURE REQUIREMENT_FOCUS_ON
	@UserId INT,
	@RequirementId INT
AS
BEGIN
	DECLARE
		@IndentLevel NVARCHAR(100),
		@IsSummary BIT,
		@ProjectId INT
	
	--First we need to get the indent-level, project id and summary flag of the passed-in requirement
	SELECT @IndentLevel = INDENT_LEVEL, @IsSummary = IS_SUMMARY, @ProjectId = PROJECT_ID
	FROM TST_REQUIREMENT
	WHERE REQUIREMENT_ID = @RequirementId

	--Update the visible flags
	--Show
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND (dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1)
			AND IS_DELETED = 0
			)
			
	--Hide
	UPDATE TST_REQUIREMENT_USER
		SET IS_VISIBLE = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 0
			AND IS_DELETED = 0
			)
	
	--Update the expand/collapse flag
	
	--Expand
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 1
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 1
			AND IS_DELETED = 0
			)
			
	--Collapse
	UPDATE TST_REQUIREMENT_USER
		SET IS_EXPANDED = 0
		WHERE USER_ID = @UserId
		AND REQUIREMENT_ID IN (
			SELECT REQUIREMENT_ID
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 0
			AND IS_DELETED = 0
			)

	--Now do the inserts for the case where user has no existing settings
	--Visible and Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 1, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 1
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Visible but not Expanded
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 1, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 1
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)) = 0
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
						
	--Hidden and Collapsed
	INSERT INTO TST_REQUIREMENT_USER
		(USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
	SELECT @UserId, 0, 0, REQUIREMENT_ID
	FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId
			AND dbo.FN_GLOBAL_INDENT_LEVEL_COMPARE(INDENT_LEVEL, @IndentLevel, LEN(INDENT_LEVEL)-3) = 0
			AND REQUIREMENT_ID NOT IN (
				SELECT REQUIREMENT_ID
				FROM TST_REQUIREMENT_USER
				WHERE USER_ID = @UserId)
			AND IS_DELETED = 0
	END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Returns the next available indent level (for new inserts)
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_NEXT_AVAILABLE_INDENT_LEVEL', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_NEXT_AVAILABLE_INDENT_LEVEL;
GO
CREATE PROCEDURE REQUIREMENT_GET_NEXT_AVAILABLE_INDENT_LEVEL
	@ProjectId INT,
	@UserId INT,
	@IgnoreLastInserted BIT
AS
BEGIN
	--See if they want to insert at the root level or directly under the last inserted item
	IF @IgnoreLastInserted = 1
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_REQUIREMENT REL
			LEFT JOIN (SELECT REQUIREMENT_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE USER_ID = @UserId) AS RLU ON REL.REQUIREMENT_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL) AND
			LEN(INDENT_LEVEL) = 3
		ORDER BY REL.INDENT_LEVEL DESC, REL.REQUIREMENT_ID
	END
	ELSE
	BEGIN
		SELECT TOP 1 REL.INDENT_LEVEL
		FROM TST_REQUIREMENT REL
			LEFT JOIN (SELECT REQUIREMENT_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE USER_ID = @UserId) AS RLU ON REL.REQUIREMENT_ID = RLU.USER_PK_ID
		WHERE
			REL.IS_DELETED = 0 AND
			REL.PROJECT_ID = @ProjectId AND
			(RLU.IS_VISIBLE = 1 OR RLU.IS_VISIBLE IS NULL)
		ORDER BY REL.INDENT_LEVEL DESC, REL.REQUIREMENT_ID
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Returns the previous peer for the given indent level
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_PREVIOUS_PEER', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_PREVIOUS_PEER;
GO
CREATE PROCEDURE REQUIREMENT_GET_PREVIOUS_PEER
	@ProjectId INT,
	@IndentLevel NVARCHAR(100),
	@IncludeDeleted BIT
AS
BEGIN
	--Find the first requirement that is just before the current one
	SELECT TOP 1 INDENT_LEVEL
	FROM TST_REQUIREMENT
	WHERE LEN(INDENT_LEVEL) = LEN(@IndentLevel)
		AND (@IncludeDeleted = 1 OR IS_DELETED = 0)
		AND PROJECT_ID = @ProjectId
		AND INDENT_LEVEL < @IndentLevel
	ORDER BY INDENT_LEVEL DESC
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the total estimated effort for any requirement
--					that does not have any tasks but is part of the release
-- ================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_RELEASE_ESTIMATE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_RELEASE_ESTIMATE;
GO
CREATE PROCEDURE REQUIREMENT_GET_RELEASE_ESTIMATE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--We exclude the summary items to avoid duplicated effort
	SELECT SUM(REQ.ESTIMATED_EFFORT) AS ESTIMATED_EFFORT
	FROM TST_REQUIREMENT REQ
	WHERE REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
	AND REQ.RELEASE_ID = @ReleaseId
	AND REQ.TASK_COUNT = 0
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of requirements under a summary
--					requirement as well as the item itself
-- ================================================================
IF OBJECT_ID ( 'REQUIREMENT_GET_SELF_AND_CHILDREN', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_GET_SELF_AND_CHILDREN;
GO
CREATE PROCEDURE REQUIREMENT_GET_SELF_AND_CHILDREN
	@ProjectId INT,
	@RequirementId INT,
	@IncludeDeleted BIT
AS
BEGIN
	DECLARE @IndentLevel NVARCHAR(100)
	DECLARE @IndentLevelLength INT

	--Initialize
	SET @IndentLevel = NULL

	--First get the indent-level of the passed-in item
	SELECT @IndentLevel = INDENT_LEVEL
	FROM TST_REQUIREMENT
	WHERE REQUIREMENT_ID = @RequirementId

	IF (@IndentLevel IS NULL)
	BEGIN
		SELECT NULL AS REQUIREMENT_ID
		RETURN
	END

	SET @IndentLevelLength = LEN(@IndentLevel)
	--Now get the list of test cases and the folder itself
	SELECT REQUIREMENT_ID
	FROM TST_REQUIREMENT
	WHERE PROJECT_ID = @ProjectId
	AND LEFT(INDENT_LEVEL, @IndentLevelLength) = @IndentLevel
	AND (REQUIREMENT_ID = @RequirementId OR LEN(INDENT_LEVEL) >= (@IndentLevelLength + 3))
	AND (IS_DELETED = 0 OR @IncludeDeleted = 1)
	ORDER BY INDENT_LEVEL
END
GO
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
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Refreshes the summary task progress and test coverage for a particular
--					requirement in the requirements tree as well as its status. Also rolls up
--					the data to all the parents of the requirement as well.
--                  Updated and improved 1/3/2020 By SWB
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_REFRESH_TASK_TEST_INFO', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_REFRESH_TASK_TEST_INFO;
GO
CREATE PROCEDURE REQUIREMENT_REFRESH_TASK_TEST_INFO
	@ProjectId INT,
	@RequirementId INT /* Not Actually Used */,
	@IndentLevel NVARCHAR(100),
	@ChangeStatusFromTasks BIT,
	@ChangeStatusFromTestCases BIT
AS
BEGIN
	DECLARE
		@Length INT,
		@cRequirementId INT,
		@cIsSummary BIT,
		@cIndentLevel NVARCHAR(100),
		@cExistingStatusId INT,
		@ReqPointEffort INT,
		
		@TestCaseCount_Total INT,
		@TestCaseCount_Passed INT,
		@TestCaseCount_Failed INT,
		@TestCaseCount_Blocked INT,
		@TestCaseCount_Caution INT,
		
		@Task_Count INT,
		@Task_EstimatedEffort INT,
		@Task_ActualEffort INT,
		@Task_RemainingEffort INT,
		@Task_ProjectedEffort INT,
		
		@Task_PercentOnTime INT,
		@Task_PercentLateFinish INT,
		@Task_PercentNotStart1 INT,
		@Task_PercentNotStart2 INT,
		@Task_PercentLateStart INT,
		
		@Requirement_StatusId INT,
		@Requirement_EstimatedEffort INT,
		@Requirement_EstimatePoints DECIMAL(9,1),
		@Requirement_ChildCount INT,
		@ReqCount INT,
		@TaskCount INT
	
	--This temp table is being used later to cache list of child requirements	
	DECLARE @tmpChildRequirements AS TABLE 
	(
		REQUIREMENT_ID INT
	);
		
	--Store the length of the requirement's indent level
	SET @Length = LEN(@IndentLevel)
	
	--Get the current story/effort metric for this project
	SELECT @ReqPointEffort = REQ_POINT_EFFORT FROM TST_PROJECT WHERE PROJECT_ID = @ProjectId

	/*
	--Declare a temp table with an iterator for the list of requirements that are parents of the passed-in one or are the passed-in one
	--itself. We need to then update the test case and task status for each of them in turn
	--Loop through each row of the temp table using the iterator count 
	*/

	DECLARE @MaxIterator INT
    DECLARE @Iterator INT 

	CREATE TABLE #MY_REQUIREMENTS (Iterator INT IDENTITY(1, 1), REQUIREMENT_ID INT, IS_SUMMARY BIT, INDENT_LEVEL NVARCHAR(100), REQUIREMENT_STATUS_ID INT);

	INSERT INTO #MY_REQUIREMENTS
	SELECT  REQUIREMENT_ID, IS_SUMMARY, INDENT_LEVEL, REQUIREMENT_STATUS_ID	    
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
				AND ((LEN(INDENT_LEVEL) < LEN(@IndentLevel) AND IS_SUMMARY = 1) OR INDENT_LEVEL = @IndentLevel)
		ORDER BY INDENT_LEVEL DESC

SELECT @MaxIterator = MAX(Iterator), @Iterator = 1
FROM   #MY_REQUIREMENTS;

   WHILE @Iterator <= @MaxIterator 
   BEGIN 

		--Logging for timing
		--SELECT 'Start Iteration #' + CAST (@Iterator AS NVARCHAR(MAX)) +', DateTime=' +  CONVERT(NVARCHAR, GETDATE(), 114) AS 'DEBUG';
 
      --Need to set these @cRequirementId, @cIsSummary, @cIndentLevel, @cExistingStatusId

      SELECT @cRequirementId = REQUIREMENT_ID FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cIsSummary = IS_SUMMARY FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cIndentLevel = INDENT_LEVEL FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

	  SELECT @cExistingStatusId = REQUIREMENT_STATUS_ID FROM #MY_REQUIREMENTS 
	  WHERE  Iterator = @Iterator;

		--Loop through all the requirements
		
		--Get the list of child requirements from function
		DELETE FROM @tmpChildRequirements;
		INSERT INTO @tmpChildRequirements
		SELECT REQUIREMENT_ID FROM FN_REQUIREMENT_GET_SELF_AND_CHILDREN(@ProjectId,@cRequirementId);
		
		--Get the count of test cases that are linked to this requirement or its children
		SELECT @TestCaseCount_Total = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
										INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
										WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
										AND TST.IS_DELETED = 0
		IF @TestCaseCount_Total > 0
		BEGIN
			SELECT @TestCaseCount_Passed = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 2
			SELECT @TestCaseCount_Failed = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 1
			SELECT @TestCaseCount_Caution = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 6
			SELECT @TestCaseCount_Blocked = COUNT(RTC.TEST_CASE_ID) FROM TST_REQUIREMENT_TEST_CASE RTC
											INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND TST.IS_DELETED = 0 AND TST.EXECUTION_STATUS_ID = 5
		END
		ELSE
		BEGIN
			SET @TestCaseCount_Passed = 0;
			SET @TestCaseCount_Failed = 0;
			SET @TestCaseCount_Caution = 0;
			SET @TestCaseCount_Blocked = 0;
		END;
			
		--Now we need to get a count of the tasks and total of the task effort for
		--the requirement and its children
		SELECT @Task_Count = COUNT(TASK_ID) FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
		--If we have at least one task, need to update the Progress indicator percentages and effort values
		IF @Task_Count > 0
		BEGIN
			SELECT @Task_EstimatedEffort = SUM(ISNULL(ESTIMATED_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_ActualEffort = SUM(ISNULL(ACTUAL_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_RemainingEffort = SUM(ISNULL(REMAINING_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
			SELECT @Task_ProjectedEffort = SUM(ISNULL(PROJECTED_EFFORT, 0)) FROM TST_TASK
											WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
											AND IS_DELETED = 0
										
			SELECT @Task_PercentLateFinish = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND TASK_STATUS_ID <> 5 /*Deferred*/
								AND COMPLETION_PERCENT < 100 AND (COMPLETION_PERCENT > 0 OR TASK_STATUS_ID = 2) AND END_DATE < GETUTCDATE()
			SELECT @Task_PercentNotStart1 = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT = 0 AND (START_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 5 /*Deferred*/) AND TASK_STATUS_ID IN (1,4,5)
			SELECT @Task_PercentLateStart = (COUNT(TASK_ID)*100)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT = 0 AND START_DATE < GETUTCDATE() AND TASK_STATUS_ID IN (1,4)		
			SELECT @Task_PercentOnTime = SUM(COMPLETION_PERCENT)/@Task_Count FROM TST_TASK
								WHERE REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
								AND IS_DELETED = 0
								AND COMPLETION_PERCENT > 0 AND (END_DATE >= GETUTCDATE() OR TASK_STATUS_ID = 3 /*Completed*/)		
			SET @Task_PercentNotStart2 = 100 - (ISNULL(@Task_PercentLateFinish,0) + ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentOnTime,0) + ISNULL(@Task_PercentLateStart,0))
		END
		ELSE
		BEGIN
			SET @Task_EstimatedEffort = NULL;
			SET @Task_ActualEffort = NULL;
			SET @Task_RemainingEffort = NULL;
			SET @Task_ProjectedEffort = NULL;

			SET @Task_PercentOnTime = 0
			SET @Task_PercentLateFinish = 0
			SET @Task_PercentNotStart1 = 0
			SET @Task_PercentNotStart2 = 0
			SET @Task_PercentLateStart = 0
		END
		
		--Update the requirement
		UPDATE TST_REQUIREMENT
			SET COVERAGE_COUNT_TOTAL = @TestCaseCount_Total,
			COVERAGE_COUNT_PASSED = @TestCaseCount_Passed,
			COVERAGE_COUNT_FAILED = @TestCaseCount_Failed,
			COVERAGE_COUNT_CAUTION = @TestCaseCount_Caution,
			COVERAGE_COUNT_BLOCKED = @TestCaseCount_Blocked,
			
			TASK_COUNT = @Task_Count,
			TASK_ESTIMATED_EFFORT = @Task_EstimatedEffort,
			TASK_REMAINING_EFFORT = @Task_RemainingEffort,
			TASK_ACTUAL_EFFORT = @Task_ActualEffort,
			TASK_PROJECTED_EFFORT = @Task_ProjectedEffort,
			
			TASK_PERCENT_ON_TIME = ISNULL(@Task_PercentOnTime,0),
			TASK_PERCENT_LATE_FINISH = ISNULL(@Task_PercentLateFinish,0),
			TASK_PERCENT_NOT_START = ISNULL(@Task_PercentNotStart1,0) + ISNULL(@Task_PercentNotStart2,0),
			TASK_PERCENT_LATE_START = ISNULL(@Task_PercentLateStart,0),
			
			ESTIMATED_EFFORT = (ESTIMATE_POINTS * @ReqPointEffort)
		WHERE REQUIREMENT_ID = @cRequirementId
		
		--If we have a summary, need to also update the Estimated Effort/Points from the child requirements
		IF @cIsSummary = 1
		BEGIN
			SELECT
				@Requirement_EstimatedEffort = SUM(ISNULL(ESTIMATED_EFFORT, 0)),
				@Requirement_EstimatePoints = SUM(ISNULL(ESTIMATE_POINTS, 0))
			FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
				AND IS_SUMMARY = 0
				AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel
				AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
			--Length check the estimate
			IF @Requirement_EstimatePoints > 99999999.9
			BEGIN
				SET @Requirement_EstimatePoints = 99999999.9
			END
			--Update the requirement
			UPDATE TST_REQUIREMENT
				SET ESTIMATED_EFFORT = @Requirement_EstimatedEffort,
				ESTIMATE_POINTS = @Requirement_EstimatePoints
			WHERE REQUIREMENT_ID = @cRequirementId
		END
		
		--Get the total count of child requirements
		SELECT @Requirement_ChildCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
			WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
				AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)

		--If we have any tasks or are a summary, need to update the status id
		--Now lets iterate through all the peers and calculate the rollup scope level
		--Generally the most advanced status (i.e. furthest in the workflow) will be reflected
		--in the rollup. However the one exception is that if we have one requirement in completed
		--status and the others in any other status then we always rollup to 'In Progress'
		--Also rejected and obsolete statuses don't rollup, so the parent will just show completed
		--unless all items are rejected/obsolete
		IF ((@cIsSummary = 1 AND @Requirement_ChildCount > 0) OR (@Task_Count > 0 AND @ChangeStatusFromTasks = 1) OR (@TestCaseCount_Total > 0 AND @ChangeStatusFromTestCases = 1))
		BEGIN
			SET @Requirement_StatusId = @cExistingStatusId
						
			--First we roll-up the status of summary requirements only
			IF @cIsSummary = 1 AND @Requirement_ChildCount > 0
			BEGIN
				SET @Requirement_StatusId = 1 /*Requested*/				
			
				--SELECT 'BEGIN:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
				--See if any child requirements = Evaluated
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 7
				IF @Requirement_StatusId IN (1) AND @ReqCount > 0 SET @Requirement_StatusId = 7

				--See if any child requirements = Accepted
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 5
				IF @Requirement_StatusId IN (1,7) AND @ReqCount > 0 SET @Requirement_StatusId = 5

				--See if any child requirements = Planned
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 2
				IF @Requirement_StatusId IN (1,7,5) AND @ReqCount > 0 SET @Requirement_StatusId = 2
				
				--See if any child requirements = In Progress
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
					AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
					AND REQUIREMENT_STATUS_ID = 3
				IF @Requirement_StatusId IN (1,7,5,2) AND @ReqCount > 0 SET @Requirement_StatusId = 3
				
				--See if any child requirements are Developed,Tested,Completed and we're still listed as requested/planned
				--If so, switch to in-progress
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (4,9,10)
				IF @Requirement_StatusId IN (1,2) AND @ReqCount > 0 SET @Requirement_StatusId = 3
				
				--If all child requirements are Developed/Tested/Completed (we can ignore obsolete/rejected) set to Developed
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (4,6,8,9,10)
				IF @ReqCount = @Requirement_ChildCount SET @Requirement_StatusId = 4
				
				--If all child requirements are Tested/Completed (we can ignore obsolete/rejected) set to Tested
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
				WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
					AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
					AND REQUIREMENT_STATUS_ID IN (9,10,6,8)
				IF (@ReqCount = @Requirement_ChildCount AND @TaskCount = @Task_Count) SET @Requirement_StatusId = 9

				--If all child requirements are Completed (we can ignore obsolete/rejected) set to Completed
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID IN (10,6,8)	
				IF @ReqCount = @Requirement_ChildCount  SET @Requirement_StatusId = 10
				
				--If all child requirements are Rejected set to Rejected
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 6
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0) SET @Requirement_StatusId = 6

				--If all child requirements are Obsolete set to Obsolete
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 8
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 8

				--If all child requirements are 'Ready for Review' set to 'Ready for Review'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 11
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 11

				--If all child requirements are 'Ready for Test' set to 'Ready for Test'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 12
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 12

				--If all child requirements are 'Released' set to 'Released'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 13
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 13

				--If all child requirements are 'Design in Process' set to 'Design in Process'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 14
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 14

				--If all child requirements are 'Design Approval' set to 'Design Approval'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 15
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 15

				--If all child requirements are 'Documented' set to 'Documented'
				SELECT @ReqCount = COUNT(REQUIREMENT_ID) FROM TST_REQUIREMENT
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0
						AND SUBSTRING(INDENT_LEVEL, 1, LEN(@cIndentLevel)) = @cIndentLevel AND LEN(INDENT_LEVEL) > LEN(@cIndentLevel)
						AND REQUIREMENT_STATUS_ID = 16
				IF (@ReqCount = @Requirement_ChildCount AND @ReqCount > 0)SET @Requirement_StatusId = 16
				
				--SELECT 'END:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
			END
			
			--Next we consider the status of tasks
			IF @Task_Count > 0 AND @ChangeStatusFromTasks = 1
			BEGIN
				--If any tasks exist in the Not-Started/Blocked statuses, set requirement status to Planned
				--unless we're in the Developed/Tested/Completed status, in which case, switch back to 'In Progress'
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (1,4)			
				IF @Requirement_StatusId IN (1,7,5) AND @TaskCount > 0 SET @Requirement_StatusId = 2
				IF @Requirement_StatusId IN (4,9,10) AND @TaskCount > 0 SET @Requirement_StatusId = 3

				--If any tasks exist in the In-Progress status, set the requirement status to In-Progress
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (2)		
				IF @Requirement_StatusId IN (1,7,5,2,4,9,10) AND @TaskCount > 0 SET @Requirement_StatusId = 3
				
				--See if any tasks exist in the Completed status, yet the requirement status was previously 'Planned'
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID = 3	
				IF @Requirement_StatusId = 2 AND @TaskCount > 0 SET @Requirement_StatusId = 3
						
				--If all tasks exist in a Completed status (or deferred since that's ignored), set Requirement to Developed
				--unless already in Tested/Completed/Obsolete status. We don't do this for summary tasks because that might override
				--the situation where we have child requirements with no tasks that are in an 'earlier' status
				SELECT @TaskCount = COUNT(TASK_ID) FROM TST_TASK
					WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @tmpChildRequirements)
						AND TASK_STATUS_ID IN (3,5)
				IF @Requirement_StatusId NOT IN (8,9,10) AND @TaskCount = @Task_Count AND @cIsSummary = 0 SET @Requirement_StatusId = 4				
			END
			--SELECT 'AFTER-TASKS:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)

			--Finally we consider the status of test cases
			IF @TestCaseCount_Total > 0 AND @ChangeStatusFromTestCases = 1
			BEGIN
				--If the current status is 'Developed' and all tests are passed, switch to 'Tested'
				IF @Requirement_StatusId = 4 AND @TestCaseCount_Total = @TestCaseCount_Passed AND @TestCaseCount_Total > 0
					SET @Requirement_StatusId = 9
			END
			--SELECT 'AFTER-TESTS:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
								
			--Update the requirement
			--SELECT 'UPDATE:' + CAST (@cRequirementId AS NVARCHAR) + '=' + CAST (@Requirement_StatusId AS NVARCHAR)
			UPDATE TST_REQUIREMENT
				SET REQUIREMENT_STATUS_ID = @Requirement_StatusId
			WHERE REQUIREMENT_ID = @cRequirementId

		END ----If we have any tasks or are a summary, need to update the status id
		      --Bump up to get next row from temp table to fill in @cRequirementId, @cIsSummary, @cIndentLevel, @cExistingStatusId

			--Logging for timing
			--SELECT 'End Iteration #' + CAST (@Iterator AS NVARCHAR(MAX)) +', DateTime=' +  CONVERT(NVARCHAR, GETDATE(), 114) AS 'DEBUG';
			SET @Iterator = @Iterator +1;
   END --While
		
END  
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		This function reorders a section of the release tree after a delete
--					operation It syncs up the 'indent' level string with the actual
--					normalized data
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_REORDER_REQUIREMENTS_AFTER_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_REORDER_REQUIREMENTS_AFTER_DELETE;
GO
CREATE PROCEDURE REQUIREMENT_REORDER_REQUIREMENTS_AFTER_DELETE
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
	UPDATE TST_REQUIREMENT
		SET INDENT_LEVEL =	SUBSTRING(INDENT_LEVEL, 1, @Length-3) +
							dbo.FN_GLOBAL_DECREMENT_INDENT_LEVEL(SUBSTRING(INDENT_LEVEL, (@Length-3) + 1, 3)) + 
							SUBSTRING(INDENT_LEVEL, @Length + 1, LEN(INDENT_LEVEL) - @Length)
	WHERE
		PROJECT_ID = @ProjectId AND
		SUBSTRING(INDENT_LEVEL, 1, @Length-3) = @ParentIndentLevel AND
		SUBSTRING(INDENT_LEVEL, (@Length - 3) + 1, 3) > @ItemIndentElement

END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		This function reorders a section of the release tree before an
--					insert operation so that there is space in the releases indent-level
--					scheme for the new item
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_REORDER_REQUIREMENTS_BEFORE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_REORDER_REQUIREMENTS_BEFORE_INSERT;
GO
CREATE PROCEDURE REQUIREMENT_REORDER_REQUIREMENTS_BEFORE_INSERT
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
	UPDATE TST_REQUIREMENT
		SET INDENT_LEVEL =	SUBSTRING(INDENT_LEVEL, 1, @Length-3) +
							dbo.FN_GLOBAL_INCREMENT_INDENT_LEVEL(SUBSTRING(INDENT_LEVEL, (@Length-3) + 1, 3)) + 
							SUBSTRING(INDENT_LEVEL, @Length + 1, LEN(INDENT_LEVEL) - @Length)
	WHERE
		PROJECT_ID = @ProjectId AND
		SUBSTRING(INDENT_LEVEL, 1, @Length-3) = @ParentIndentLevel AND
		SUBSTRING(INDENT_LEVEL, (@Length - 3) + 1, 3) >= @ItemIndentElement

END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of filtered requirements with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@OnlyExpanded BIT,
	@IncludeDeleted BIT
AS
	DECLARE @ExpandedClause NVARCHAR(MAX)
	DECLARE @NormalizingClause NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_REQUIREMENT_LIST_INTERNAL REQ LEFT JOIN (SELECT REQUIREMENT_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RQU ON REQ.REQUIREMENT_ID = RQU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE (RQU.IS_VISIBLE = 1 OR RQU.IS_VISIBLE IS NULL) ' +
					'AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' +
					'AND (REQ.IS_SUMMARY = 1 OR (1=1 ' + ISNULL(@Filters,'') + '))'
	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND REQ.IS_DELETED = 0'
	END

	--Create the appropriate clause for removing unnecessary folders depending on whether
	--we need to remove all non-matching folders or just ones that are expanded
	IF @OnlyExpanded = 1
	BEGIN
		SET @ExpandedClause = 'AND @PREVINDENT <> '''' AND (LEFT(@PREVINDENT,LEN(@INDENT)) = @INDENT OR @EXPANDED = 0)'
	END
	IF @OnlyExpanded = 0
	BEGIN
		SET @ExpandedClause = 'AND @PREVINDENT <> '''' AND LEFT(@PREVINDENT,LEN(@INDENT)) = @INDENT'
	END

	--Only remove filters if we have a filter set (since expensive operation)
	IF @Filters = '' OR @Filters IS NULL
	BEGIN
		SET @NormalizingClause =
'DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL'
	END
	ELSE
	BEGIN
		SET @NormalizingClause =
'DECLARE NormalizingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID, IS_SUMMARY, INDENT_LEVEL, RQU.IS_EXPANDED
FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL

OPEN NormalizingCursor
FETCH LAST FROM NormalizingCursor INTO @PK, @SUMMARY, @INDENT, @EXPANDED

SET @PREVINDENT = ''''
SET NOCOUNT ON
WHILE @@FETCH_STATUS = 0
BEGIN
	IF @SUMMARY = 0
	BEGIN
		INSERT @tblNormalized (PK,INDENT) VALUES (@PK,@INDENT)
		SET @PREVINDENT = @INDENT
	END
	IF @SUMMARY = 1 ' + @ExpandedClause + '
	BEGIN
		INSERT @tblNormalized (PK,INDENT)  VALUES (@PK,@INDENT)
	END
	FETCH PRIOR FROM NormalizingCursor INTO @PK, @SUMMARY, @INDENT, @EXPANDED
END

CLOSE       NormalizingCursor
DEALLOCATE  NormalizingCursor

DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT PK FROM @tblNormalized ORDER BY INDENT'
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
DECLARE @SUMMARY BIT
DECLARE @EXPANDED BIT
DECLARE @INDENT NVARCHAR(100)
DECLARE @PREVINDENT  NVARCHAR(100)
SET @PageSize = ' + CAST(@NumRows AS NVARCHAR) + '

DECLARE @PK INT
DECLARE @tblNormalized TABLE
(
	PK INT NOT NULL PRIMARY KEY,
	INDENT NVARCHAR(100) NOT NULL
)
DECLARE @tblPK TABLE
(
	PK INT NOT NULL PRIMARY KEY
) ' + @NormalizingClause + ' ' +

'OPEN PagingCursor
FETCH RELATIVE ' + CAST(@StartRow AS NVARCHAR) + ' FROM PagingCursor INTO @PK

SET NOCOUNT ON
WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
	INSERT @tblPK (PK)  VALUES (@PK)
	FETCH NEXT FROM PagingCursor INTO @PK
	SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT ISNULL(RQU.IS_EXPANDED, CASE REQ.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RQU.IS_VISIBLE, 1) AS IS_VISIBLE, REQ.* FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REQ.REQUIREMENT_ID = tblPK.PK ' + @WHERE + '
ORDER BY REQ.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a count of requirements by scope level for a particular importance level
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE', 'P' ) IS NOT NULL 
    DROP PROCEDURE [REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE];
GO
CREATE PROCEDURE [REQUIREMENT_RETRIEVE_COUNT_BY_IMPORTANCE]
	@ProjectId INT,
	@ImportanceId INT,
	@ReleaseId INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified
	IF @ReleaseId IS NOT NULL
	BEGIN
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	END

	--See if we have an importance specified	
	IF @ImportanceId IS NULL
	BEGIN
		--Get count for requirements that have no importance set
		SELECT	REQUIREMENT_STATUS_ID AS RequirementStatusId, COUNT(REQUIREMENT_ID) AS RequirementCount
		FROM TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND IMPORTANCE_ID IS NULL
		AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
		AND IS_DELETED = 0
		GROUP BY REQUIREMENT_STATUS_ID
		ORDER BY REQUIREMENT_STATUS_ID
	END
	ELSE
	BEGIN				
		--Get count for requirements that do have the importance set
		SELECT	REQUIREMENT_STATUS_ID AS RequirementStatusId, COUNT(REQUIREMENT_ID) AS RequirementCount
		FROM	TST_REQUIREMENT
		WHERE PROJECT_ID = @ProjectId
		AND	IMPORTANCE_ID = @ImportanceId
		AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
		AND IS_DELETED = 0
		GROUP BY REQUIREMENT_STATUS_ID
		ORDER BY REQUIREMENT_STATUS_ID
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a dataset of total requirement coverage for a project/release
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_COVERAGE_SUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_COVERAGE_SUMMARY;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_COVERAGE_SUMMARY
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified,
	--if we have @ReleaseId = -2 it means only load in active releases
	IF @ReleaseId IS NOT NULL
	BEGIN
		IF @ReleaseId = -2
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM TST_RELEASE WHERE PROJECT_ID = @ProjectId AND RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) AND IS_DELETED = 0
		END
		ELSE
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
		END
	END
	
	--Create select command for retrieving the total number of requirements per coverage status
	--(i.e. sum of coverage per requirement normalized by the count for that requirement
	SELECT	1 AS CoverageStatusOrder, 'Passed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_PASSED AS FLOAT(53)) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
	UNION
		SELECT	2 AS CoverageStatusOrder, 'Failed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_FAILED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
	UNION
		SELECT	3 AS CoverageStatusOrder, 'Blocked' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_BLOCKED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
	UNION
		SELECT	4 AS CoverageStatusOrder, 'Caution' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_CAUTION AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
	UNION
		SELECT	5 AS CoverageStatusOrder, 'Not Run' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE (CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) - CAST (COVERAGE_COUNT_PASSED AS FLOAT(53)) - CAST (COVERAGE_COUNT_CAUTION AS FLOAT(53)) - CAST (COVERAGE_COUNT_BLOCKED AS FLOAT(53)) - CAST (COVERAGE_COUNT_FAILED AS FLOAT(53))) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
	UNION
		SELECT	6 AS CoverageStatusOrder, 'Not Covered' AS CoverageStatus, CAST (COUNT(REQUIREMENT_ID) - SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE 1 END) AS FLOAT(53)) AS CoverageCount
		FROM	TST_REQUIREMENT
		WHERE	PROJECT_ID = @ProjectId AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID NOT IN (6,8) AND (@ReleaseId IS NULL OR RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList))
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of requirements for a specific user with custom filter/sort
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_CUSTOM', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_CUSTOM;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_CUSTOM
	@UserId INT,
	@ProjectId INT,
	@FilterSort NVARCHAR(MAX),
	@NumRows INT,
	@IncludeDeleted BIT,
	@OnlyShowVisible BIT
AS
	DECLARE @TOP NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE_ORDER_BY NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_REQUIREMENT_LIST_INTERNAL REQ LEFT JOIN (SELECT REQUIREMENT_ID AS USER_PK_ID,USER_ID,IS_EXPANDED,IS_VISIBLE FROM TST_REQUIREMENT_USER WHERE USER_ID = ' + CAST (@UserId AS NVARCHAR)+ ') AS RQU ON REQ.REQUIREMENT_ID = RQU.USER_PK_ID'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	IF @OnlyShowVisible = 1
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE (RQU.IS_VISIBLE = 1 OR RQU.IS_VISIBLE IS NULL) '
	END
	ELSE
	BEGIN
		SET @WHERE_ORDER_BY =	'WHERE 1=1 '
	END

	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND REQ.IS_DELETED = 0 '
	END
	
	IF @ProjectId IS NOT NULL
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' '
	END
	
	IF @FilterSort IS NOT NULL AND LEN(@FilterSort) > 0
	BEGIN
		SET @WHERE_ORDER_BY = @WHERE_ORDER_BY + ' AND ' + @FilterSort + ' '
	END
	
	--See if we have to limit the number of rows retrieved
	SET @TOP = ''
	IF @NumRows IS NOT NULL
	BEGIN
		SET @TOP = 'TOP ' + CAST(@NumRows AS NVARCHAR)
	END

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
SELECT ' + @TOP + ' ISNULL(RQU.IS_EXPANDED, CASE REQ.IS_SUMMARY WHEN 1 THEN 1 ELSE 0 END) AS IS_EXPANDED, ISNULL(RQU.IS_VISIBLE, 1) AS IS_VISIBLE, REQ.*
FROM ' + @TABLES + ' ' + @WHERE_ORDER_BY
	EXEC (@SQL)
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of total requirement coverage for a project group
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_GROUP_COVERAGE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_GROUP_COVERAGE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_GROUP_COVERAGE
	@ProjectGroupId INT,
	@RequirementStatusesToIgnore NVARCHAR(MAX),
	@ActiveReleasesOnly BIT,
	@IncludeDeleted BIT
AS
	DECLARE @RequirementStatusesToIgnoreTable TABLE (STATUS_ID INT)
BEGIN

	--The requirements cannot be in one of the following statuses
	INSERT INTO @RequirementStatusesToIgnoreTable (STATUS_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementStatusesToIgnore, ',')

	--Create select command for retrieving the total number of requirements per coverage status
	--(i.e. sum of coverage per requirement normalized by the count for that requirement
	SELECT	1 AS CoverageStatusOrder, 'Passed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_PASSED AS FLOAT(53)) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	2 AS CoverageStatusOrder, 'Failed' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_FAILED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	3 AS CoverageStatusOrder, 'Blocked' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_BLOCKED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	4 AS CoverageStatusOrder, 'Caution' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_CAUTION AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	5 AS CoverageStatusOrder, 'Not Run' AS CoverageStatus, ROUND(SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE (CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) - CAST (COVERAGE_COUNT_PASSED AS FLOAT(53)) - CAST (COVERAGE_COUNT_CAUTION AS FLOAT(53)) - CAST (COVERAGE_COUNT_BLOCKED AS FLOAT(53)) - CAST (COVERAGE_COUNT_FAILED AS FLOAT(53))) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END), 1) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
	UNION
		SELECT	6 AS CoverageStatusOrder, 'Not Covered' AS CoverageStatus, CAST (COUNT(REQUIREMENT_ID) - SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE 1 END) AS FLOAT(53)) AS CoverageCount
		FROM	TST_REQUIREMENT REQ
		INNER JOIN TST_PROJECT PRJ ON REQ.PROJECT_ID = PRJ.PROJECT_ID
		LEFT JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
		WHERE	PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID =  @ProjectGroupId  AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable) 
		AND (@IncludeDeleted = 1 OR REQ.IS_DELETED = 0)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0);
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of incidents mapped against requirement
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_INCIDENT_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_INCIDENT_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_INCIDENT_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@NumberOfRows INT,
	@OnlyIncludeWithOpenIncidents BIT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified
	IF @ReleaseId IS NOT NULL
	BEGIN
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	END
	
	--Create select command for retrieving the the list of open incidents and total incidents for requirements
	--This needs to be a union between the incidents linked via test runs and those statically linked
	SELECT TOP(@NumberOfRows) REQ.REQUIREMENT_ID AS RequirementId, REQ.NAME AS Name, REQ.INDENT_LEVEL AS IndentLevel,
		REQ.IS_SUMMARY AS IsSummary,
		RIN1.INCIDENT_COUNT AS IncidentTotalCount, RIN2.INCIDENT_COUNT AS IncidentOpenCount
	FROM TST_REQUIREMENT REQ LEFT JOIN (
		SELECT REQUIREMENT_ID, COUNT(INCIDENT_ID) AS INCIDENT_COUNT
		FROM VW_REQUIREMENT_INCIDENTS
		WHERE (@ReleaseId IS NULL OR DETECTED_RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
		GROUP BY REQUIREMENT_ID) RIN1
	ON REQ.REQUIREMENT_ID = RIN1.REQUIREMENT_ID LEFT JOIN (
		SELECT REQUIREMENT_ID, COUNT(INCIDENT_ID) AS INCIDENT_COUNT
		FROM VW_REQUIREMENT_INCIDENTS
		WHERE IS_OPEN_STATUS = 1 AND (@ReleaseId IS NULL OR DETECTED_RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
		GROUP BY REQUIREMENT_ID) RIN2
	ON REQ.REQUIREMENT_ID = RIN2.REQUIREMENT_ID
	--Create the appropriate clause for determining if we show all incident or just open incident rows
	WHERE ((@OnlyIncludeWithOpenIncidents = 1 AND RIN2.INCIDENT_COUNT IS NOT NULL) OR (@OnlyIncludeWithOpenIncidents = 0 AND RIN1.INCIDENT_COUNT IS NOT NULL))
	AND REQ.PROJECT_ID = @ProjectId AND REQ.IS_DELETED = 0
	ORDER BY IncidentOpenCount DESC, IncidentTotalCount DESC, RequirementId ASC
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves the list of filtered non-summary requirements with database pagination
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_NONSUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_NONSUMMARY;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_NONSUMMARY
	@UserId INT,
	@ProjectId INT,
	@Filters NVARCHAR(MAX),
	@StartRow INT,
	@NumRows INT,
	@IncludeDeleted BIT
AS
	DECLARE @ExpandedClause NVARCHAR(MAX)
	DECLARE @NormalizingClause NVARCHAR(MAX)
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @TABLES NVARCHAR(MAX)
	DECLARE @WHERE NVARCHAR(MAX)
BEGIN
	--Create the list of tables to be joined into the query
	SET @TABLES = 'VW_REQUIREMENT_LIST REQ'

	--Create the complete WHERE clause that contains the standard items plus any passed-in filters
	SET @WHERE =	'WHERE PROJECT_ID = ' + CAST(@ProjectId AS NVARCHAR) + ' ' +
					'AND IS_SUMMARY = 0' + @Filters
	IF @IncludeDeleted = 0
	BEGIN
		SET @WHERE = @WHERE + ' AND REQ.IS_DELETED = 0'
	END

	SET @NormalizingClause =
'DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT REQUIREMENT_ID FROM ' + @TABLES + ' ' + @WHERE + ' ORDER BY INDENT_LEVEL'

	--Create the complete dynamic SQL statement to be executed
	SET @SQL = '
DECLARE @PageSize INT
DECLARE @INDENT NVARCHAR(100)
DECLARE @PREVINDENT  NVARCHAR(100)
SET @PageSize = ' + CAST(@NumRows AS NVARCHAR) + '

DECLARE @PK INT
DECLARE @tblNormalized TABLE
(
	PK INT NOT NULL PRIMARY KEY,
	INDENT NVARCHAR(100) NOT NULL
)
DECLARE @tblPK TABLE
(
	PK INT NOT NULL PRIMARY KEY
) ' + @NormalizingClause + ' ' +

'OPEN PagingCursor
FETCH RELATIVE ' + CAST(@StartRow AS NVARCHAR) + ' FROM PagingCursor INTO @PK

SET NOCOUNT ON
WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
	INSERT @tblPK (PK)  VALUES (@PK)
	FETCH NEXT FROM PagingCursor INTO @PK
	SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT CAST(0 AS BIT) AS IS_EXPANDED, CAST(1 AS BIT) AS IS_VISIBLE, REQ.* FROM ' + @TABLES + ' INNER JOIN @tblPK tblPK ON REQ.REQUIREMENT_ID = tblPK.PK ' + @WHERE + '
ORDER BY REQ.INDENT_LEVEL'
	EXEC (@SQL)
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Get the list of requirements not mapped to test cases that are part of the release
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_NOT_COVERED_COUNT
	@ProjectId INT,
	@ReleaseId INT,
	@RequirementStatusesToIgnore NVARCHAR(MAX)
AS
	DECLARE @RequirementStatusesToIgnoreTable TABLE (STATUS_ID INT)
BEGIN
	--The requirements cannot be in one of the following statuses
	INSERT INTO @RequirementStatusesToIgnoreTable (STATUS_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementStatusesToIgnore, ',')
		
	--Create select command for retrieving all the requirements in the project together with their mapped test cases
	SELECT CAST (COUNT(REQUIREMENT_ID) AS FLOAT(53)) AS REQUIREMENT_COUNT
	FROM TST_REQUIREMENT
	WHERE PROJECT_ID = @ProjectId
	AND REQUIREMENT_ID NOT IN
		(SELECT REQUIREMENT_ID FROM TST_REQUIREMENT_TEST_CASE AS RQT INNER JOIN TST_RELEASE_TEST_CASE AS RLT
		ON RQT.TEST_CASE_ID = RLT.TEST_CASE_ID WHERE RLT.RELEASE_ID = @ReleaseId)
		AND REQUIREMENT_STATUS_ID NOT IN (SELECT STATUS_ID FROM @RequirementStatusesToIgnoreTable)
		AND IS_DELETED = 0
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Retrieves a list of requirements and mapped test cases for a specific release
-- =====================================================================================
IF OBJECT_ID ( 'REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE;
GO
CREATE PROCEDURE REQUIREMENT_RETRIEVE_TEST_CASES_BY_RELEASE
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN

	--Create select command for retrieving all the requirements in the project together with their mapped test cases
	SELECT RQT.REQUIREMENT_ID, RQT.TEST_CASE_ID
	FROM TST_REQUIREMENT_TEST_CASE AS RQT
		INNER JOIN TST_RELEASE_TEST_CASE AS RLT ON RQT.TEST_CASE_ID = RLT.TEST_CASE_ID
		INNER JOIN TST_TEST_CASE AS TSC ON RQT.TEST_CASE_ID = TSC.TEST_CASE_ID
		INNER JOIN TST_REQUIREMENT AS REQ ON RQT.REQUIREMENT_ID = REQ.REQUIREMENT_ID
	WHERE RLT.RELEASE_ID = @ReleaseId
		AND TSC.IS_DELETED = 0
		AND REQ.IS_DELETED = 0
		AND REQ.PROJECT_ID = @ProjectId
	ORDER BY RQT.REQUIREMENT_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Deletes test coverage
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_SAVE_COVERAGE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_SAVE_COVERAGE_DELETE;
GO
CREATE PROCEDURE REQUIREMENT_SAVE_COVERAGE_DELETE
	@RequirementId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TST_REQUIREMENT_TEST_CASE
	WHERE TEST_CASE_ID = @TestCaseId AND REQUIREMENT_ID = @RequirementId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Inserts test coverage
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_SAVE_COVERAGE_INSERT', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_SAVE_COVERAGE_INSERT;
GO
CREATE PROCEDURE REQUIREMENT_SAVE_COVERAGE_INSERT
	@RequirementId INT,
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO TST_REQUIREMENT_TEST_CASE (REQUIREMENT_ID, TEST_CASE_ID)
	VALUES (@RequirementId, @TestCaseId);
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Updates the positional data for the requirement
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_UPDATE_POSITIONAL', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_UPDATE_POSITIONAL;
GO
CREATE PROCEDURE REQUIREMENT_UPDATE_POSITIONAL
	@RequirementId INT,
	@UserId INT,
	@IsExpanded BIT,
	@IsVisible BIT,
	@IsSummary BIT,
	@IndentLevel NVARCHAR(100)	
AS
	DECLARE @RequirementCount INT
BEGIN
	--First update the requirement table itself
	UPDATE TST_REQUIREMENT
	SET	INDENT_LEVEL = @IndentLevel,
		IS_SUMMARY = @IsSummary
	WHERE REQUIREMENT_ID = @RequirementId

	--Now insert/update the requirement user navigation metadata
    SET @RequirementCount = (SELECT COUNT(*) FROM TST_REQUIREMENT_USER WHERE REQUIREMENT_ID = @RequirementId AND USER_ID = @UserId);
    IF @RequirementCount = 0 AND @UserId IS NOT NULL
    BEGIN
		INSERT INTO TST_REQUIREMENT_USER (USER_ID, IS_EXPANDED, IS_VISIBLE, REQUIREMENT_ID)
		VALUES (@UserId, @IsExpanded, @IsVisible, @RequirementId)
	END
    ELSE
    BEGIN
		UPDATE TST_REQUIREMENT_USER
		SET	IS_EXPANDED = @IsExpanded,
			IS_VISIBLE = @IsVisible
		WHERE REQUIREMENT_ID = @RequirementId
		AND USER_ID = @UserId
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Requirement
-- Description:		Updates the rank for passed in requirement ids
-- =============================================
IF OBJECT_ID ( 'REQUIREMENT_UPDATE_RANKS', 'P' ) IS NOT NULL 
    DROP PROCEDURE REQUIREMENT_UPDATE_RANKS;
GO
CREATE PROCEDURE REQUIREMENT_UPDATE_RANKS
	@ProjectId INT,
	@RequirementIds NVARCHAR(MAX),
	@ExistingRank INT
AS
	DECLARE @RequirementIdsTable TABLE (REQUIREMENT_ID INT)
BEGIN
	--Get the list of requirement IDs
	INSERT INTO @RequirementIdsTable (REQUIREMENT_ID)
		SELECT ITEM FROM FN_GLOBAL_CONVERT_LIST_TO_TABLE(@RequirementIds, ',')

	IF @ExistingRank IS NOT NULL
	BEGIN
		--Increment the requirements that have a higher rank
		UPDATE TST_REQUIREMENT
			SET RANK = RANK + 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK > @ExistingRank
				AND REQUIREMENT_ID NOT IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
				
		--Decrement the requirements that have a lower or equal rank
		UPDATE TST_REQUIREMENT
			SET RANK = RANK - 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND RANK <= @ExistingRank
				AND REQUIREMENT_ID NOT IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
				
		-- Set the selected requirements to this rank
		UPDATE TST_REQUIREMENT
			SET RANK = @ExistingRank
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
	END
	ELSE
	BEGIN
		-- Set the selected requirements to the lowest non-null rank
		UPDATE TST_REQUIREMENT
			SET RANK = 1
			WHERE PROJECT_ID = @ProjectId
				AND IS_DELETED = 0
				AND REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM @RequirementIdsTable)
	END	
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: SourceCode
-- Description:		Deletes the database part of the source code cache
-- =============================================
IF OBJECT_ID ( 'SOURCE_CODE_DELETE_PROJECT_CACHE', 'P' ) IS NOT NULL 
    DROP PROCEDURE SOURCE_CODE_DELETE_PROJECT_CACHE;
GO
CREATE PROCEDURE SOURCE_CODE_DELETE_PROJECT_CACHE
	@VersionControlSystemId INT,
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete pull requests (both source and dest branches)
	DELETE FROM TST_VERSION_CONTROL_PULL_REQUEST
	WHERE SOURCE_BRANCH_ID IN
		(SELECT BRANCH_ID FROM TST_SOURCE_CODE_COMMIT_BRANCH
		WHERE VERSION_CONTROL_SYSTEM_ID = @VersionControlSystemId AND PROJECT_ID = @ProjectId);
	DELETE FROM TST_VERSION_CONTROL_PULL_REQUEST
	WHERE DEST_BRANCH_ID IN
		(SELECT BRANCH_ID FROM TST_SOURCE_CODE_COMMIT_BRANCH
		WHERE VERSION_CONTROL_SYSTEM_ID = @VersionControlSystemId AND PROJECT_ID = @ProjectId);			
	
	--Delete commits
	DELETE FROM TST_SOURCE_CODE_COMMIT_BRANCH WHERE VERSION_CONTROL_SYSTEM_ID = @VersionControlSystemId AND PROJECT_ID = @ProjectId;
	DELETE FROM TST_SOURCE_CODE_COMMIT WHERE VERSION_CONTROL_SYSTEM_ID = @VersionControlSystemId AND PROJECT_ID = @ProjectId;
	--Delete branches
    DELETE FROM TST_VERSION_CONTROL_BRANCH WHERE VERSION_CONTROL_SYSTEM_ID = @VersionControlSystemId AND PROJECT_ID = @ProjectId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: SystemManager
-- Description:		Checks to see if SQL Full Text Indexing is Installed
-- =============================================
IF OBJECT_ID ( 'SYSTEM_CHECK_FULLTEXT_INDEXING', 'P' ) IS NOT NULL 
    DROP PROCEDURE [SYSTEM_CHECK_FULLTEXT_INDEXING];
GO
CREATE PROCEDURE [SYSTEM_CHECK_FULLTEXT_INDEXING]
AS
BEGIN
	SELECT CAST (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS BIT) AS IS_FULL_TEXT_INSTALLED
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: SystemManager
-- Description:		Refreshes the database indexes
-- =============================================
IF OBJECT_ID ( 'SYSTEM_REFRESH_INDEXES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [SYSTEM_REFRESH_INDEXES];
GO
CREATE PROCEDURE [SYSTEM_REFRESH_INDEXES]
AS
BEGIN
	--Simply calls the built in stored procedure to do this
	EXEC sp_MSforeachtable 'alter index all on ? rebuild'
END
GO

/****** Object:  StoredProcedure [dbo].[SYSTEM_USAGE_REPORT]    Script Date: 11/30/2023 11:28:07 PM ******/
/*


 --NumberOfActiveAccount
 --Getting active users count based on last_login activity should be exists with in 120 days with active status.

 --ActiveUserPercentage
 -- (No.of Active users / Total No.of users)*100

 --Per Day
 --Average(How many times of user logged in a day)

  --Per Week
 --Average(How many times of user logged in a week)

  --Per Month
 --Average(How many times of user logged in a Month)

 -- Time Per Day
 --Average(How many seconds user logged in a day)
 
 -- Time Per Week
 --Average(How many seconds user logged in a Week)

  -- Time Per Month
 --Average(How many seconds user logged in a Month)
 
 
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[SYSTEM_USAGE_REPORT]
	@year varchar(50)
AS
BEGIN



declare @Last120DaysActiveUsers as Table
(
	User_ID int
)

declare @TotalActiveUsers as Table
(
	User_ID int
)

Declare @TmpMonthwiseActiveUsers as Table
(
	User_ID int,
	MonthT int,
	YearT int
)

Declare @TmpActiveUsersCF as Table
(
	MonthT int,
	YearT int,
	ActiveCount int 
)

Insert into @TmpMonthwiseActiveUsers 
Select distinct U.User_ID, Month(Last_Login_Date) MonthT , Year(Last_Login_Date) YearT  
From
TST_User U 
where  U.IS_ACTIVE = 1
and (DATEDIFF(day, Last_Login_date, GETDATE())) <= 120 

Declare @TotalCount int, @TotalActiveUsersCount int  

Select  @TotalCount = Count(*) from @TmpMonthwiseActiveUsers

Insert into @TmpActiveUsersCF
Select MonthT, YearT, Count(*) ActiveCount from @TmpMonthwiseActiveUsers group by MonthT, YearT

Update @TmpActiveUsersCF Set ActiveCount = @TotalCount 

Declare @MonthT int, @YearT int, @ActiveCount int, @Total int, @CreatedCount int  

Set @Total = 0

DECLARE CarryForwardCount CURSOR FOR 
Select MonthT, YearT,  ActiveCount from @TmpActiveUsersCF Order By YearT Desc, MonthT Desc 

OPEN CarryForwardCount  
FETCH NEXT FROM CarryForwardCount INTO @MonthT, @YearT, @ActiveCount   

WHILE @@FETCH_STATUS = 0  
BEGIN  

	  Update @TmpActiveUsersCF Set ActiveCount = ActiveCount - @Total where YearT =  @YearT  and MonthT = @MonthT

	  Select @CreatedCount = Count(*) 
		From
		TST_User U 
		where  U.IS_ACTIVE = 1
		and (DATEDIFF(day, Last_Login_date, GETDATE())) <= 120 and Month(CREATION_DATE) = @MonthT and Year(CREATION_DATE) = @YearT

	  Set @Total = @Total + IsNull(@CreatedCount,0) 

      FETCH NEXT FROM CarryForwardCount INTO @MonthT, @YearT, @ActiveCount    
END 

CLOSE CarryForwardCount  
DEALLOCATE CarryForwardCount

Insert into @TotalActiveUsers 
Select distinct U.User_ID
From
TST_User U 
where  U.IS_ACTIVE = 1


Select @TotalActiveUsersCount = Count(*) from @TotalActiveUsers


Insert into @Last120DaysActiveUsers
Select distinct U.User_ID
From
TST_User U 
where  U.IS_ACTIVE = 1
and (DATEDIFF(day, Last_Login_date, GETDATE())) <= 120 --Last_Login_date is not null --

;with
cte as (
    select 0 n
    union all select n + 1 from cte where n < 11
),
eachmonth as (
select DATENAME(month,(dateadd(month, n, convert(date, @year+'-01-01')))) Month,(dateadd(month, n, convert(date, @year+'-01-01'))) dt,DAY(EOMONTH(dateadd(month, n, convert(date, @year+'-01-01')))) daycount from cte 
),
GetAllUserTill as(
Select USER_ID, 
case
when (Select count(*) from @Last120DaysActiveUsers where User_ID = TST_USer.User_ID) > 0 then 1
else 0
end
IS_ACTIVE,
case
when (Select count(*) from @Last120DaysActiveUsers where User_ID = TST_USer.User_ID) > 0 then cast(concat(Month(getdate()),'/1/',@year) as date)
else cast(concat(Month(CREATION_DATE),'/1/',@year) as date)
end
 CREATION_DATE
From TST_USer
),
 activeaccount as (

SELECT  DATENAME(month,CREATION_DATE) AS [MonthName],
       COUNT(USER_ID) AS [ActiveCountUsers] ,convert (decimal (18,2), COUNT(USER_ID)) As Count2
FROM GetAllUserTill --[TST_USER]
WHERE IS_ACTIVE = 1 --AND YEAR(CREATION_DATE) = @year
GROUP BY DATENAME(month,CREATION_DATE)
),
totaluser as (select datename(month,CREATION_DATE)Month,convert (decimal (18,2), COUNT(USER_ID)) As Count2 from GetAllUserTill --[TST_USER] 
--where YEAR(CREATION_DATE) = @year
group by YEAR(CREATION_DATE),datename(month,CREATION_DATE)
),
perday as (
--PerDay - Per Month
Select year(data.Date) As YEAR,DATENAME(month,(data.Date)) As MONTH, sum(data.DayCount) AS PerDay from (SELECT  COUNT(*) AS [DayCount]
      , CAST(LOGIN_DATE AS DATE)    AS [Date]
FROM [ValidationMasterAudit].dbo.TST_USER_ACTIVITY_LOG_AUDIT
WHERE LOGOUT_DATE IS NULL and year(LOGIN_DATE) = @year
GROUP BY CAST(LOGIN_DATE AS DATE) ) AS data
 group by year(data.Date),DATENAME(month,(data.Date))),

timepermonthandday as (Select DATENAME(month,(data.Date)) As MONTH, sum(DateDiff) TimePerDay from (SELECT  COUNT(*) AS [DayCount]
      , CAST(LOGOUT_DATE AS DATE)    AS [Date],sum(DATEDIFF(SECOND, LOGIN_DATE, LOGOUT_DATE)) AS DateDiff
FROM [ValidationMasterAudit].dbo.TST_USER_ACTIVITY_LOG_AUDIT
WHERE LOGOUT_DATE IS not NULL and year(LOGOUT_DATE) = @year
GROUP BY CAST(LOGOUT_DATE AS DATE) ) AS data
 group by year(data.Date),DATENAME(month,(data.Date)))

 --Select * from eachmonth

		   select e.Month as MonthName, isnull(sum(cf.ActiveCount),0) NumberOfActiveAccount, isnull(CAST((sum(cf.ActiveCount)*100) AS DECIMAL(18,2))/@TotalActiveUsersCount,0) as ActiveUserPercentage,
		  isnull((sum(p.PerDay)) / ((e.daycount)),0) as PerDay,
		  cast(sum(p.PerDay) as float)/cast(4 as float)   PerWeek,p.PerDay PerMonth ,
		   --prday
		   RIGHT('0' + CAST((((tm.TimePerDay )/((e.daycount) ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )/((e.daycount) ))) / 60) % 60 AS VARCHAR),2) as TimePerDay,

--perweek
		   RIGHT('0' + CAST((((tm.TimePerDay )/((4) ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )/((4) ))) / 60) % 60 AS VARCHAR),2)  as TimePerWeek,
--permonth,
		   RIGHT('0' + CAST((((tm.TimePerDay ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )) / 60)) % 60 AS VARCHAR),2) as TimePerMonth



		  from eachmonth e 
		   
		   left join PerDay p on p.MONTH = e.Month
		  left join  activeaccount a on a.MonthName = e.MONTH
		  left join timepermonthandday tm on tm.MONTH = e.Month
		  left join totaluser tu on tu.Month = e.Month 
		  left join @TmpActiveUsersCF cf On Month(e.dt) = cf.MonthT and Year(e.dt) = cf.YearT 
		  group by e.MONTH,p.YEAR,p.PerDay,e.dt,tm.TimePerDay,e.daycount,tu.Month  order by e.dt
		 
END
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: ManagerBase
-- Description:		Gets the database server properties for the admin screen
-- =====================================================================
IF OBJECT_ID ( 'SYSINFO', 'P' ) IS NOT NULL 
    DROP PROCEDURE SYSINFO;
GO
CREATE PROCEDURE SYSINFO
AS
BEGIN
	SELECT
		CAST(SERVERPROPERTY('BuildClrVersion') AS NVARCHAR) AS BuildClrVersion,
		CAST(SERVERPROPERTY('Collation') AS NVARCHAR) AS Collation,
		CAST(SERVERPROPERTY('CollationId') AS BIGINT) AS CollationId,
		CAST(SERVERPROPERTY('ComparisonStyle') AS BIGINT) AS ComparisonStyle,
		CAST(SERVERPROPERTY('ComputerNamePhysicalNetBIOS') AS NVARCHAR) AS ComputerNamePhysicalNetBIOS,
		CAST(SERVERPROPERTY('Edition') AS NVARCHAR) AS Edition,
		CAST(SERVERPROPERTY('EditionId') AS BIGINT) AS EditionId,
		CAST(SERVERPROPERTY('EngineEdition') AS INT) AS EngineEdition,
		CAST(SERVERPROPERTY('HadrManagerStatus') AS SMALLINT) AS HadrManagerStatus,
		CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR) AS InstanceDefaultDataPath,
		CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS NVARCHAR) AS InstanceDefaultLogPath,
		CAST(SERVERPROPERTY('InstanceName') AS NVARCHAR) AS InstanceName,
		CAST(SERVERPROPERTY('IsAdvancedAnalyticsInstalled') AS BIT) AS IsAdvancedAnalyticsInstalled,
		CAST(SERVERPROPERTY('IsClustered') AS BIT) AS IsClustered,
		CAST(SERVERPROPERTY('IsFullTextInstalled') AS BIT) AS IsFullTextInstalled,
		CAST(SERVERPROPERTY('IsHadrEnabled') AS BIT) AS IsHadrEnabled,
		CAST(SERVERPROPERTY('IsIntegratedSecurityOnly') AS BIT) AS IsIntegratedSecurityOnly,
		CAST(SERVERPROPERTY('IsLocalDB') AS BIT) AS IsLocalDB,
		CAST(SERVERPROPERTY('IsPolybaseInstalled') AS BIT) AS IsPolybaseInstalled,
		CAST(SERVERPROPERTY('IsSingleUser') AS BIT) AS IsSingleUser,
		CAST(SERVERPROPERTY('IsXTPSupported') AS BIT) AS IsXTPSupported,
		CAST(SERVERPROPERTY('LCID') AS INT) AS LCID,
		CAST(SERVERPROPERTY('LicenseType') AS NVARCHAR) AS LicenseType,
		CAST(SERVERPROPERTY('MachineName') AS NVARCHAR) AS MachineName,
		CAST(SERVERPROPERTY('NumLicenses') AS INT) AS NumLicenses,
		CAST(SERVERPROPERTY('ProcessID') AS BIGINT) AS ProcessID,
		CAST(SERVERPROPERTY('ProductBuild') AS BIGINT) AS ProductBuild,
		CAST(SERVERPROPERTY('ProductBuildType') AS NVARCHAR) AS ProductBuildType,
		CAST(SERVERPROPERTY('ProductLevel') AS NVARCHAR) AS ProductLevel,
		CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) AS ProductMajorVersion,
		CAST(SERVERPROPERTY('ProductMinorVersion') AS INT) AS ProductMinorVersion,
		CAST(SERVERPROPERTY('ProductUpdateLevel') AS NVARCHAR) AS ProductUpdateLevel,
		CAST(SERVERPROPERTY('ProductUpdateReference') AS NVARCHAR) AS ProductUpdateReference,
		CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR) AS ProductVersion,
		CAST(SERVERPROPERTY('ResourceLastUpdateDateTime') AS DATETIME) AS ResourceLastUpdateDateTime,
		CAST(SERVERPROPERTY('ResourceVersion') AS NVARCHAR) AS ResourceVersion,
		CAST(SERVERPROPERTY('ServerName') AS NVARCHAR) AS ServerName,
		CAST(SERVERPROPERTY('SqlCharSet') AS SMALLINT) AS SqlCharSet,
		CAST(SERVERPROPERTY('SqlCharSetName') AS NVARCHAR) AS SqlCharSetName,
		CAST(SERVERPROPERTY('SqlSortOrder') AS SMALLINT) AS SqlSortOrder,
		CAST(SERVERPROPERTY('SqlSortOrderName') AS NVARCHAR) AS SqlSortOrderName,
		CAST(SERVERPROPERTY('FilestreamShareName') AS NVARCHAR) AS FilestreamShareName,
		CAST(SERVERPROPERTY('FilestreamConfiguredLevel') AS INT) AS FilestreamConfiguredLevel,
		CAST(SERVERPROPERTY('FilestreamEffectiveLevel') AS INT) AS FilestreamEffectiveLevel,
		@@SERVERNAME as SysServerName,
		@@VERSION as SysFullVersion
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Incident
-- Description:		Deletes an Incident
-- =============================================
IF OBJECT_ID ( 'TASK_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TASK_DELETE;
GO
CREATE PROCEDURE TASK_DELETE
	@TaskId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete pull request info
	DELETE FROM TST_VERSION_CONTROL_PULL_REQUEST WHERE TASK_ID = @TaskId;
	--Delete user subscription.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 6 AND ARTIFACT_ID = @TaskId);
	--Now delete the incident itself
    DELETE FROM TST_TASK WHERE TASK_ID = @TaskId;
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Refreshes the folder hierarchy for the specified project
-- =====================================================================
IF OBJECT_ID ( 'TASK_REFRESH_FOLDER_HIERARCHY', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TASK_REFRESH_FOLDER_HIERARCHY];
GO
CREATE PROCEDURE [TASK_REFRESH_FOLDER_HIERARCHY]
	@ProjectId INT
AS
BEGIN
	--First delete the existing folders
	DELETE FROM TST_TASK_FOLDER_HIERARCHY
	WHERE PROJECT_ID = @ProjectId;

	WITH TASK_FOLDER_HIERARCHY (TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	AS
	(
		SELECT	TKF.TASK_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TASK_FOLDER_ID, 1 AS HIERARCHY_LEVEL,
				CAST(dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS NVARCHAR(MAX)) AS INDENT_LEVEL
		FROM TST_TASK_FOLDER TKF
		WHERE TKF.PARENT_TASK_FOLDER_ID IS NULL AND TKF.PROJECT_ID = @ProjectId
		UNION ALL
		SELECT	TKF.TASK_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TASK_FOLDER_ID, (CTE.HIERARCHY_LEVEL + 1) AS HIERARCHY_LEVEL,
				CTE.INDENT_LEVEL + dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS INDENT_LEVEL
		FROM TST_TASK_FOLDER TKF
		INNER JOIN TASK_FOLDER_HIERARCHY CTE ON TKF.PARENT_TASK_FOLDER_ID = CTE.TASK_FOLDER_ID
		WHERE TKF.PARENT_TASK_FOLDER_ID IS NOT NULL AND TKF.PROJECT_ID = @ProjectId
	)
	INSERT INTO TST_TASK_FOLDER_HIERARCHY (TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	SELECT ISNULL(TASK_FOLDER_ID, 0) AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TASK_FOLDER_HIERARCHY
	ORDER BY PROJECT_ID, INDENT_LEVEL COLLATE Latin1_General_BIN
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Gets the list of all children of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TASK_RETRIEVE_CHILD_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TASK_RETRIEVE_CHILD_FOLDERS];
GO
CREATE PROCEDURE [TASK_RETRIEVE_CHILD_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TASK_FOLDER_HIERARCHY WHERE TASK_FOLDER_ID = @FolderId;

	--Now get the child folders
	SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TASK_FOLDER_HIERARCHY
	WHERE SUBSTRING(INDENT_LEVEL, 1, LEN(@IndentLevel)) = @IndentLevel
	AND (LEN(INDENT_LEVEL) > LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL, TASK_FOLDER_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Retrieves the project group summary status for tasks
-- =============================================
IF OBJECT_ID ( 'TASK_RETRIEVE_GROUP_SUMMARY', 'P' ) IS NOT NULL 
    DROP PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY;
GO
CREATE PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY
	@ProjectGroupId INT,
	@ActiveReleasesOnly BIT
AS
BEGIN
	SELECT	1 AS ProgressOrderId, 'On Schedule' AS ProgressCaption, SUM((REL.TASK_PERCENT_ON_TIME * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
	WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	2 AS ProgressOrderId, 'Late Finish' AS ProgressCaption, SUM((REL.TASK_PERCENT_LATE_FINISH * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	3 AS ProgressOrderId, 'Late Start' AS ProgressCaption, SUM((REL.TASK_PERCENT_LATE_START * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
	UNION
	SELECT	4 AS ProgressOrderId, 'Not Started' AS ProgressCaption, SUM((REL.TASK_PERCENT_NOT_START * REL.TASK_COUNT) / 100) AS TaskCount
	FROM	TST_RELEASE REL INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
    WHERE	PRJ.PROJECT_GROUP_ID = @ProjectGroupId
		AND REL.RELEASE_TYPE_ID IN (1,2)
		AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		AND REL.IS_DELETED = 0
		AND PRJ.IS_ACTIVE = 1
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Retrieves the project group summary status for tasks by project
-- =============================================
IF OBJECT_ID ( 'TASK_RETRIEVE_GROUP_SUMMARY_BY_PROJECT', 'P' ) IS NOT NULL 
    DROP PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY_BY_PROJECT;
GO
CREATE PROCEDURE TASK_RETRIEVE_GROUP_SUMMARY_BY_PROJECT
	@ProjectGroupId INT,
	@ActiveReleasesOnly BIT
AS
BEGIN
	SELECT
		PRJ2.PROJECT_GROUP_ID,
		PRJ2.PROJECT_ID,
		PRJ2.NAME AS PROJECT_NAME,
		PRJ2.DESCRIPTION AS PROJECT_DESCRIPTION,
		ISNULL(PTP.TASK_COUNT,0) AS TASK_COUNT,
		PTP.TASK_PERCENT_ON_TIME,
		PTP.TASK_PERCENT_LATE_FINISH,
		PTP.TASK_PERCENT_NOT_START,
		PTP.TASK_PERCENT_LATE_START,
		PTP.TASK_ESTIMATED_EFFORT,
		PTP.TASK_ACTUAL_EFFORT,
		PTP.TASK_REMAINING_EFFORT,
		PTP.TASK_PROJECTED_EFFORT

	FROM
		(SELECT
			PRJ.PROJECT_ID,
			SUM(REL.TASK_COUNT) AS TASK_COUNT,
			ISNULL(AVG(REL.TASK_PERCENT_ON_TIME),0) AS TASK_PERCENT_ON_TIME,
			ISNULL(AVG(REL.TASK_PERCENT_LATE_FINISH),0) AS TASK_PERCENT_LATE_FINISH,
			ISNULL(AVG(REL.TASK_PERCENT_NOT_START),0) AS TASK_PERCENT_NOT_START,
			ISNULL(AVG(REL.TASK_PERCENT_LATE_START),0) AS TASK_PERCENT_LATE_START,
			ISNULL(SUM(REL.TASK_ESTIMATED_EFFORT),0) AS TASK_ESTIMATED_EFFORT,
			ISNULL(SUM(REL.TASK_ACTUAL_EFFORT),0) AS TASK_ACTUAL_EFFORT,
			ISNULL(SUM(REL.TASK_REMAINING_EFFORT),0) AS TASK_REMAINING_EFFORT,
			ISNULL(SUM(REL.TASK_PROJECTED_EFFORT),0) AS TASK_PROJECTED_EFFORT
		FROM TST_RELEASE REL
			INNER JOIN TST_PROJECT PRJ ON REL.PROJECT_ID = PRJ.PROJECT_ID
		WHERE (REL.RELEASE_TYPE_ID = 1 OR LEN(REL.INDENT_LEVEL) = 3)
			AND (REL.TASK_COUNT > 0)
			AND REL.IS_DELETED = 0
			AND PRJ.IS_ACTIVE = 1
			AND (REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) OR @ActiveReleasesOnly = 0)
		GROUP BY PRJ.PROJECT_ID
		) AS PTP
			RIGHT JOIN TST_PROJECT PRJ2 ON PRJ2.PROJECT_ID = PTP.PROJECT_ID
			WHERE PRJ2.IS_ACTIVE = 1 AND PRJ2.PROJECT_GROUP_ID = @ProjectGroupId
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
--                  Updated and improved 2/7/2020 By SWB
-- =====================================================================
IF OBJECT_ID ( 'TASK_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TASK_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TASK_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TASK_FOLDER_HIERARCHY WHERE TASK_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TASK_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Deletes Project Template Configurable Types
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_DELETE_CONFIGURABLE_TYPES', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_DELETE_CONFIGURABLE_TYPES;
GO
CREATE PROCEDURE TEMPLATE_DELETE_CONFIGURABLE_TYPES
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Incident Types
	DELETE FROM TST_INCIDENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Task Types
	DELETE FROM TST_TASK_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Test Case Types
	DELETE FROM TST_TEST_CASE_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Requirement Types
	DELETE FROM TST_REQUIREMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Document Types
	DELETE FROM TST_DOCUMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
	--Risk Types
	DELETE FROM TST_RISK_TYPE WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId	
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Deletes Project Template Custom Properties
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_DELETE_CUSTOM_PROPERTIES', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_DELETE_CUSTOM_PROPERTIES;
GO
CREATE PROCEDURE TEMPLATE_DELETE_CUSTOM_PROPERTIES
	@ProjectTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;	
    --Now we need to delete all the custom properties and then custom lists. The dependent entities should then cascade
    DELETE FROM TST_CUSTOM_PROPERTY WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
    DELETE FROM TST_CUSTOM_PROPERTY_LIST WHERE PROJECT_TEMPLATE_ID = @ProjectTemplateId
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TemplateManager
-- Description:		Is the user authorized to edit artifacts that are part of the template
-- ================================================================
IF OBJECT_ID ( 'TEMPLATE_IS_AUTHORIZED_TO_EDIT', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_IS_AUTHORIZED_TO_EDIT;
GO
CREATE PROCEDURE TEMPLATE_IS_AUTHORIZED_TO_EDIT
	@UserId INT,
	@ProjectTemplateId INT
AS
BEGIN
	IF EXISTS(
		SELECT TMP.PROJECT_TEMPLATE_ID
		FROM TST_PROJECT_TEMPLATE TMP
			INNER JOIN TST_PROJECT PRJ ON TMP.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
			INNER JOIN TST_PROJECT_USER PRU ON PRJ.PROJECT_ID = PRU.PROJECT_ID
			INNER JOIN TST_PROJECT_ROLE PRR ON PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
		WHERE
			PRR.IS_TEMPLATE_ADMIN = 1 AND
			PRU.USER_ID = @UserId AND
			TMP.PROJECT_TEMPLATE_ID = @ProjectTemplateId
			)
	BEGIN
		SELECT CAST (1 AS BIT) AS IS_AUTHORIZED
	END
	ELSE
	BEGIN
		SELECT CAST (0 AS BIT) AS IS_AUTHORIZED
	END
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TemplateManager
-- Description:		Is the user authorized to view artifacts that are part of the template
-- ================================================================
IF OBJECT_ID ( 'TEMPLATE_IS_AUTHORIZED_TO_VIEW', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_IS_AUTHORIZED_TO_VIEW;
GO
CREATE PROCEDURE TEMPLATE_IS_AUTHORIZED_TO_VIEW
	@UserId INT,
	@ProjectTemplateId INT
AS
BEGIN
	IF EXISTS(
		SELECT TMP.PROJECT_TEMPLATE_ID
		FROM TST_PROJECT_TEMPLATE TMP
			INNER JOIN TST_PROJECT PRJ ON TMP.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
			INNER JOIN TST_PROJECT_USER PRU ON PRJ.PROJECT_ID = PRU.PROJECT_ID
			INNER JOIN TST_PROJECT_ROLE PRR ON PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
		WHERE
			PRU.USER_ID = @UserId AND
			TMP.PROJECT_TEMPLATE_ID = @ProjectTemplateId AND
			PRR.IS_ACTIVE = 1 AND
			TMP.IS_ACTIVE = 1
			)
	BEGIN
		SELECT CAST (1 AS BIT) AS IS_AUTHORIZED
	END
	ELSE
	BEGIN
		SELECT CAST (0 AS BIT) AS IS_AUTHORIZED
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Clears the user settings that would be affected by a change in project template
-- Remarks:			
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_CLEAR_USER_SETTINGS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_CLEAR_USER_SETTINGS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_CLEAR_USER_SETTINGS
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Collections
	DELETE FROM TST_PROJECT_COLLECTION_ENTRY WHERE PROJECT_ID = @ProjectId
	
	--Columns
	DELETE FROM TST_USER_ARTIFACT_FIELD WHERE PROJECT_ID = @ProjectId
	DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE PROJECT_ID = @ProjectId
	
	--Saved Filters and Reports
	DELETE FROM TST_SAVED_FILTER WHERE PROJECT_ID = @ProjectId
	DELETE FROM TST_REPORT_SAVED WHERE PROJECT_ID = @ProjectId

END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the document fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_DOCUMENT_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_DOCUMENT_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_DOCUMENT_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Document Types	
UPDATE INC
	SET INC.DOCUMENT_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_PROJECT_ATTACHMENT INC INNER JOIN (
	SELECT INC.ATTACHMENT_ID, T1.DOCUMENT_TYPE_ID AS OLD_TYPE_ID, T2.DOCUMENT_TYPE_ID AS NEW_TYPE_ID
	FROM TST_PROJECT_ATTACHMENT INC
		INNER JOIN TST_DOCUMENT_TYPE T1 ON INC.DOCUMENT_TYPE_ID = T1.DOCUMENT_TYPE_ID
		INNER JOIN TST_DOCUMENT_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.ATTACHMENT_ID, T1.DOCUMENT_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 DOCUMENT_TYPE_ID FROM TST_DOCUMENT_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_PROJECT_ATTACHMENT INC
		INNER JOIN TST_DOCUMENT_TYPE T1 ON INC.DOCUMENT_TYPE_ID = T1.DOCUMENT_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_DOCUMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.ATTACHMENT_ID = MAP.ATTACHMENT_ID
WHERE 
	MAP.ATTACHMENT_ID = INC.ATTACHMENT_ID

-- Document Status
UPDATE INC
	SET INC.DOCUMENT_STATUS_ID = MAP.NEW_STATUS_ID
FROM 
	TST_ATTACHMENT INC INNER JOIN (
	SELECT INC.ATTACHMENT_ID, T1.DOCUMENT_STATUS_ID AS OLD_STATUS_ID, T2.DOCUMENT_STATUS_ID AS NEW_STATUS_ID
	FROM TST_ATTACHMENT INC
		INNER JOIN TST_DOCUMENT_STATUS T1 ON INC.DOCUMENT_STATUS_ID = T1.DOCUMENT_STATUS_ID
		INNER JOIN TST_DOCUMENT_STATUS T2 ON T1.NAME = T2.NAME		
		INNER JOIN TST_PROJECT_ATTACHMENT PAT ON INC.ATTACHMENT_ID = PAT.ATTACHMENT_ID
	WHERE PAT.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.ATTACHMENT_ID, T1.DOCUMENT_STATUS_ID AS OLD_STATUS_ID, (SELECT TOP 1 DOCUMENT_STATUS_ID FROM TST_DOCUMENT_STATUS WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_STATUS_ID
	FROM TST_ATTACHMENT INC
		INNER JOIN TST_DOCUMENT_STATUS T1 ON INC.DOCUMENT_STATUS_ID = T1.DOCUMENT_STATUS_ID
		INNER JOIN TST_PROJECT_ATTACHMENT PAT ON INC.ATTACHMENT_ID = PAT.ATTACHMENT_ID
	WHERE PAT.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_DOCUMENT_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.ATTACHMENT_ID = MAP.ATTACHMENT_ID
WHERE 
	MAP.ATTACHMENT_ID = INC.ATTACHMENT_ID	
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the incident fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_INCIDENT_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_INCIDENT_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_INCIDENT_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Incident Types	
UPDATE INC
	SET INC.INCIDENT_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_INCIDENT INC INNER JOIN (
	SELECT INC.INCIDENT_ID, T1.INCIDENT_TYPE_ID AS OLD_TYPE_ID, T2.INCIDENT_TYPE_ID AS NEW_TYPE_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_TYPE T1 ON INC.INCIDENT_TYPE_ID = T1.INCIDENT_TYPE_ID
		INNER JOIN TST_INCIDENT_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.INCIDENT_ID, T1.INCIDENT_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 INCIDENT_TYPE_ID FROM TST_INCIDENT_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_TYPE T1 ON INC.INCIDENT_TYPE_ID = T1.INCIDENT_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.INCIDENT_ID = MAP.INCIDENT_ID
WHERE 
	MAP.INCIDENT_ID = INC.INCIDENT_ID

-- Incident Status
UPDATE INC
	SET INC.INCIDENT_STATUS_ID = MAP.NEW_STATUS_ID
FROM 
	TST_INCIDENT INC INNER JOIN (
	SELECT INC.INCIDENT_ID, T1.INCIDENT_STATUS_ID AS OLD_STATUS_ID, T2.INCIDENT_STATUS_ID AS NEW_STATUS_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_STATUS T1 ON INC.INCIDENT_STATUS_ID = T1.INCIDENT_STATUS_ID
		INNER JOIN TST_INCIDENT_STATUS T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.INCIDENT_ID, T1.INCIDENT_STATUS_ID AS OLD_STATUS_ID, (SELECT TOP 1 INCIDENT_STATUS_ID FROM TST_INCIDENT_STATUS WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_STATUS_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_STATUS T1 ON INC.INCIDENT_STATUS_ID = T1.INCIDENT_STATUS_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.INCIDENT_ID = MAP.INCIDENT_ID
WHERE 
	MAP.INCIDENT_ID = INC.INCIDENT_ID
	
--Incident Priority
UPDATE INC
	SET INC.PRIORITY_ID = MAP.NEW_PRIORITY_ID
FROM 
	TST_INCIDENT INC INNER JOIN (
	SELECT INC.INCIDENT_ID, T1.PRIORITY_ID AS OLD_PRIORITY_ID, T2.PRIORITY_ID AS NEW_PRIORITY_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_PRIORITY T1 ON INC.PRIORITY_ID = T1.PRIORITY_ID
		INNER JOIN TST_INCIDENT_PRIORITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.INCIDENT_ID, T1.PRIORITY_ID AS OLD_PRIORITY_ID, NULL AS NEW_PRIORITY_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_PRIORITY T1 ON INC.PRIORITY_ID = T1.PRIORITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.INCIDENT_ID = MAP.INCIDENT_ID
WHERE 
	MAP.INCIDENT_ID = INC.INCIDENT_ID
		
--Incident Severity
UPDATE INC
	SET INC.SEVERITY_ID = MAP.NEW_SEVERITY_ID
FROM 
	TST_INCIDENT INC INNER JOIN (
	SELECT INC.INCIDENT_ID, T1.SEVERITY_ID AS OLD_SEVERITY_ID, T2.SEVERITY_ID AS NEW_SEVERITY_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_SEVERITY T1 ON INC.SEVERITY_ID = T1.SEVERITY_ID
		INNER JOIN TST_INCIDENT_SEVERITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.INCIDENT_ID, T1.SEVERITY_ID AS OLD_SEVERITY_ID, NULL AS NEW_SEVERITY_ID
	FROM TST_INCIDENT INC
		INNER JOIN TST_INCIDENT_SEVERITY T1 ON INC.SEVERITY_ID = T1.SEVERITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_SEVERITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.INCIDENT_ID = MAP.INCIDENT_ID
WHERE 
	MAP.INCIDENT_ID = INC.INCIDENT_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the requirement fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_REQUIREMENT_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_REQUIREMENT_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_REQUIREMENT_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Requirement Types	
UPDATE INC
	SET INC.REQUIREMENT_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_REQUIREMENT INC INNER JOIN (
	SELECT INC.REQUIREMENT_ID, T1.REQUIREMENT_TYPE_ID AS OLD_TYPE_ID, T2.REQUIREMENT_TYPE_ID AS NEW_TYPE_ID
	FROM TST_REQUIREMENT INC
		INNER JOIN TST_REQUIREMENT_TYPE T1 ON INC.REQUIREMENT_TYPE_ID = T1.REQUIREMENT_TYPE_ID
		INNER JOIN TST_REQUIREMENT_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
		AND INC.REQUIREMENT_TYPE_ID > 0 /* Exclude Package */
	UNION	
	SELECT INC.REQUIREMENT_ID, T1.REQUIREMENT_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 REQUIREMENT_TYPE_ID FROM TST_REQUIREMENT_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_REQUIREMENT INC
		INNER JOIN TST_REQUIREMENT_TYPE T1 ON INC.REQUIREMENT_TYPE_ID = T1.REQUIREMENT_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_REQUIREMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.REQUIREMENT_ID = MAP.REQUIREMENT_ID
WHERE 
	MAP.REQUIREMENT_ID = INC.REQUIREMENT_ID

--Requirement Importance
UPDATE INC
	SET INC.IMPORTANCE_ID = MAP.NEW_IMPORTANCE_ID
FROM 
	TST_REQUIREMENT INC INNER JOIN (
	SELECT INC.REQUIREMENT_ID, T1.IMPORTANCE_ID AS OLD_IMPORTANCE_ID, T2.IMPORTANCE_ID AS NEW_IMPORTANCE_ID
	FROM TST_REQUIREMENT INC
		INNER JOIN TST_IMPORTANCE T1 ON INC.IMPORTANCE_ID = T1.IMPORTANCE_ID
		INNER JOIN TST_IMPORTANCE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.REQUIREMENT_ID, T1.IMPORTANCE_ID AS OLD_IMPORTANCE_ID, NULL AS NEW_IMPORTANCE_ID
	FROM TST_REQUIREMENT INC
		INNER JOIN TST_IMPORTANCE T1 ON INC.IMPORTANCE_ID = T1.IMPORTANCE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_IMPORTANCE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP		
	ON INC.REQUIREMENT_ID = MAP.REQUIREMENT_ID
WHERE 
	MAP.REQUIREMENT_ID = INC.REQUIREMENT_ID
		
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the risk fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_RISK_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_RISK_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_RISK_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Risk Types	
UPDATE INC
	SET INC.RISK_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_RISK INC INNER JOIN (
	SELECT INC.RISK_ID, T1.RISK_TYPE_ID AS OLD_TYPE_ID, T2.RISK_TYPE_ID AS NEW_TYPE_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_TYPE T1 ON INC.RISK_TYPE_ID = T1.RISK_TYPE_ID
		INNER JOIN TST_RISK_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.RISK_ID, T1.RISK_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 RISK_TYPE_ID FROM TST_RISK_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_TYPE T1 ON INC.RISK_TYPE_ID = T1.RISK_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.RISK_ID = MAP.RISK_ID
WHERE 
	MAP.RISK_ID = INC.RISK_ID

-- Risk Status
UPDATE INC
	SET INC.RISK_STATUS_ID = MAP.NEW_STATUS_ID
FROM 
	TST_RISK INC INNER JOIN (
	SELECT INC.RISK_ID, T1.RISK_STATUS_ID AS OLD_STATUS_ID, T2.RISK_STATUS_ID AS NEW_STATUS_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_STATUS T1 ON INC.RISK_STATUS_ID = T1.RISK_STATUS_ID
		INNER JOIN TST_RISK_STATUS T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.RISK_ID, T1.RISK_STATUS_ID AS OLD_STATUS_ID, (SELECT TOP 1 RISK_STATUS_ID FROM TST_RISK_STATUS WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_STATUS_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_STATUS T1 ON INC.RISK_STATUS_ID = T1.RISK_STATUS_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.RISK_ID = MAP.RISK_ID
WHERE 
	MAP.RISK_ID = INC.RISK_ID
	
--Risk Priority
UPDATE INC
	SET INC.RISK_PROBABILITY_ID = MAP.NEW_PROBABILITY_ID
FROM 
	TST_RISK INC INNER JOIN (
	SELECT INC.RISK_ID, T1.RISK_PROBABILITY_ID AS OLD_PROBABILITY_ID, T2.RISK_PROBABILITY_ID AS NEW_PROBABILITY_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_PROBABILITY T1 ON INC.RISK_PROBABILITY_ID = T1.RISK_PROBABILITY_ID
		INNER JOIN TST_RISK_PROBABILITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.RISK_ID, T1.RISK_PROBABILITY_ID AS OLD_PROBABILITY_ID, NULL AS NEW_PROBABILITY_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_PROBABILITY T1 ON INC.RISK_PROBABILITY_ID = T1.RISK_PROBABILITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_PROBABILITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.RISK_ID = MAP.RISK_ID
WHERE 
	MAP.RISK_ID = INC.RISK_ID
		
--Risk Severity
UPDATE INC
	SET INC.RISK_IMPACT_ID = MAP.NEW_IMPACT_ID
FROM 
	TST_RISK INC INNER JOIN (
	SELECT INC.RISK_ID, T1.RISK_IMPACT_ID AS OLD_IMPACT_ID, T2.RISK_IMPACT_ID AS NEW_IMPACT_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_IMPACT T1 ON INC.RISK_IMPACT_ID = T1.RISK_IMPACT_ID
		INNER JOIN TST_RISK_IMPACT T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.RISK_ID, T1.RISK_IMPACT_ID AS OLD_IMPACT_ID, NULL AS NEW_IMPACT_ID
	FROM TST_RISK INC
		INNER JOIN TST_RISK_IMPACT T1 ON INC.RISK_IMPACT_ID = T1.RISK_IMPACT_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_IMPACT WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.RISK_ID = MAP.RISK_ID
WHERE 
	MAP.RISK_ID = INC.RISK_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Displays the number of standard fields that will be defaulted/nulled if a template change occurs
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_STANDARD_FIELDS_INFORMATION', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_STANDARD_FIELDS_INFORMATION;
GO
CREATE PROCEDURE TEMPLATE_REMAP_STANDARD_FIELDS_INFORMATION
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Requirement Type
SELECT 'Requirement' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.REQUIREMENT_ID) AS AFFECTED_ITEMS
FROM TST_REQUIREMENT INC
	INNER JOIN TST_REQUIREMENT_TYPE T1 ON INC.REQUIREMENT_TYPE_ID = T1.REQUIREMENT_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_REQUIREMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

--Requirement Importance
SELECT 'Requirement' AS ARTIFACT_TYPE, 'Importance' AS ARTIFACT_FIELD, COUNT(INC.REQUIREMENT_ID) AS AFFECTED_ITEMS
FROM TST_REQUIREMENT INC
	INNER JOIN TST_IMPORTANCE T1 ON INC.IMPORTANCE_ID = T1.IMPORTANCE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_IMPORTANCE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Test Case Types	
SELECT 'Test Case' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.TEST_CASE_ID) AS AFFECTED_ITEMS
FROM TST_TEST_CASE INC
	INNER JOIN TST_TEST_CASE_TYPE T1 ON INC.TEST_CASE_TYPE_ID = T1.TEST_CASE_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_TEST_CASE_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

--Test Case Priority
SELECT 'Test Case' AS ARTIFACT_TYPE, 'Priority' AS ARTIFACT_FIELD, COUNT(INC.TEST_CASE_ID) AS AFFECTED_ITEMS
FROM TST_TEST_CASE INC
	INNER JOIN TST_TEST_CASE_PRIORITY T1 ON INC.TEST_CASE_PRIORITY_ID = T1.TEST_CASE_PRIORITY_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_TEST_CASE_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Task Types	
SELECT 'Task' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.TASK_ID) AS AFFECTED_ITEMS
FROM TST_TASK INC
	INNER JOIN TST_TASK_TYPE T1 ON INC.TASK_TYPE_ID = T1.TASK_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

--Task Priority
SELECT 'Task' AS ARTIFACT_TYPE, 'Priority' AS ARTIFACT_FIELD, COUNT(INC.TASK_ID) AS AFFECTED_ITEMS
FROM TST_TASK INC
	INNER JOIN TST_TASK_PRIORITY T1 ON INC.TASK_PRIORITY_ID = T1.TASK_PRIORITY_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Incident Types	
SELECT 'Incident' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.INCIDENT_ID) AS AFFECTED_ITEMS
FROM TST_INCIDENT INC
	INNER JOIN TST_INCIDENT_TYPE T1 ON INC.INCIDENT_TYPE_ID = T1.INCIDENT_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Incident Status
SELECT 'Incident' AS ARTIFACT_TYPE, 'Status' AS ARTIFACT_FIELD, COUNT(INC.INCIDENT_ID) AS AFFECTED_ITEMS
FROM TST_INCIDENT INC
	INNER JOIN TST_INCIDENT_STATUS T1 ON INC.INCIDENT_STATUS_ID = T1.INCIDENT_STATUS_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION
	
--Incident Priority
SELECT 'Incident' AS ARTIFACT_TYPE, 'Priority' AS ARTIFACT_FIELD, COUNT(INC.INCIDENT_ID) AS AFFECTED_ITEMS
FROM TST_INCIDENT INC
	INNER JOIN TST_INCIDENT_PRIORITY T1 ON INC.PRIORITY_ID = T1.PRIORITY_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION
		
--Incident Severity
SELECT 'Incident' AS ARTIFACT_TYPE, 'Severity' AS ARTIFACT_FIELD, COUNT(INC.INCIDENT_ID) AS AFFECTED_ITEMS
FROM TST_INCIDENT INC
	INNER JOIN TST_INCIDENT_SEVERITY T1 ON INC.SEVERITY_ID = T1.SEVERITY_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_INCIDENT_SEVERITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Risk Types	
SELECT 'Risk' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.RISK_ID) AS AFFECTED_ITEMS
FROM TST_RISK INC
	INNER JOIN TST_RISK_TYPE T1 ON INC.RISK_TYPE_ID = T1.RISK_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Risk Status
SELECT 'Risk' AS ARTIFACT_TYPE, 'Status' AS ARTIFACT_FIELD, COUNT(INC.RISK_ID) AS AFFECTED_ITEMS
FROM TST_RISK INC
	INNER JOIN TST_RISK_STATUS T1 ON INC.RISK_STATUS_ID = T1.RISK_STATUS_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

--Risk Probability
SELECT 'Risk' AS ARTIFACT_TYPE, 'Probability' AS ARTIFACT_FIELD, COUNT(INC.RISK_ID) AS AFFECTED_ITEMS
FROM TST_RISK INC
	INNER JOIN TST_RISK_PROBABILITY T1 ON INC.RISK_PROBABILITY_ID = T1.RISK_PROBABILITY_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_PROBABILITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
		
UNION
		
--Risk Impact
SELECT 'Risk' AS ARTIFACT_TYPE, 'Impact' AS ARTIFACT_FIELD, COUNT(INC.RISK_ID) AS AFFECTED_ITEMS
FROM TST_RISK INC
	INNER JOIN TST_RISK_IMPACT T1 ON INC.RISK_IMPACT_ID = T1.RISK_IMPACT_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_RISK_IMPACT WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Document Types	
SELECT 'Document' AS ARTIFACT_TYPE, 'Type' AS ARTIFACT_FIELD, COUNT(INC.ATTACHMENT_ID) AS AFFECTED_ITEMS
FROM TST_PROJECT_ATTACHMENT INC
	INNER JOIN TST_DOCUMENT_TYPE T1 ON INC.DOCUMENT_TYPE_ID = T1.DOCUMENT_TYPE_ID
WHERE INC.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_DOCUMENT_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

UNION

-- Document Status
SELECT 'Document' AS ARTIFACT_TYPE, 'Status' AS ARTIFACT_FIELD, COUNT(INC.ATTACHMENT_ID) AS AFFECTED_ITEMS
FROM TST_ATTACHMENT INC
	INNER JOIN TST_DOCUMENT_STATUS T1 ON INC.DOCUMENT_STATUS_ID = T1.DOCUMENT_STATUS_ID
	INNER JOIN TST_PROJECT_ATTACHMENT PAT ON INC.ATTACHMENT_ID = PAT.ATTACHMENT_ID
WHERE PAT.PROJECT_ID = @ProjectId
	AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
	AND T1.NAME NOT IN (SELECT NAME FROM TST_DOCUMENT_STATUS WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)

--Order Clause
ORDER BY ARTIFACT_TYPE, ARTIFACT_FIELD

END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the task fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_TASK_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_TASK_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_TASK_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Task Types	
UPDATE INC
	SET INC.TASK_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_TASK INC INNER JOIN (
	SELECT INC.TASK_ID, T1.TASK_TYPE_ID AS OLD_TYPE_ID, T2.TASK_TYPE_ID AS NEW_TYPE_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_TYPE T1 ON INC.TASK_TYPE_ID = T1.TASK_TYPE_ID
		INNER JOIN TST_TASK_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TASK_ID, T1.TASK_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 TASK_TYPE_ID FROM TST_TASK_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_TYPE T1 ON INC.TASK_TYPE_ID = T1.TASK_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TASK_ID = MAP.TASK_ID
WHERE 
	MAP.TASK_ID = INC.TASK_ID

--Task Priority
UPDATE INC
	SET INC.TASK_PRIORITY_ID = MAP.NEW_TASK_PRIORITY_ID
FROM 
	TST_TASK INC INNER JOIN (
	SELECT INC.TASK_ID, T1.TASK_PRIORITY_ID AS OLD_TASK_PRIORITY_ID, T2.TASK_PRIORITY_ID AS NEW_TASK_PRIORITY_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_PRIORITY T1 ON INC.TASK_PRIORITY_ID = T1.TASK_PRIORITY_ID
		INNER JOIN TST_TASK_PRIORITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TASK_ID, T1.TASK_PRIORITY_ID AS OLD_TASK_PRIORITY_ID, NULL AS NEW_TASK_PRIORITY_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_PRIORITY T1 ON INC.TASK_PRIORITY_ID = T1.TASK_PRIORITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TASK_ID = MAP.TASK_ID
WHERE 
	MAP.TASK_ID = INC.TASK_ID		
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the test case fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_TEST_CASE_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_TEST_CASE_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_TEST_CASE_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Test Case Types	
UPDATE INC
	SET INC.TEST_CASE_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_TEST_CASE INC INNER JOIN (
	SELECT INC.TEST_CASE_ID, T1.TEST_CASE_TYPE_ID AS OLD_TYPE_ID, T2.TEST_CASE_TYPE_ID AS NEW_TYPE_ID
	FROM TST_TEST_CASE INC
		INNER JOIN TST_TEST_CASE_TYPE T1 ON INC.TEST_CASE_TYPE_ID = T1.TEST_CASE_TYPE_ID
		INNER JOIN TST_TEST_CASE_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TEST_CASE_ID, T1.TEST_CASE_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 TEST_CASE_TYPE_ID FROM TST_TEST_CASE_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_TEST_CASE INC
		INNER JOIN TST_TEST_CASE_TYPE T1 ON INC.TEST_CASE_TYPE_ID = T1.TEST_CASE_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TEST_CASE_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TEST_CASE_ID = MAP.TEST_CASE_ID
WHERE 
	MAP.TEST_CASE_ID = INC.TEST_CASE_ID

--Test Case Priority
UPDATE INC
	SET INC.TEST_CASE_PRIORITY_ID = MAP.NEW_TEST_CASE_PRIORITY_ID
FROM 
	TST_TEST_CASE INC INNER JOIN (
	SELECT INC.TEST_CASE_ID, T1.TEST_CASE_PRIORITY_ID AS OLD_TEST_CASE_PRIORITY_ID, T2.TEST_CASE_PRIORITY_ID AS NEW_TEST_CASE_PRIORITY_ID
	FROM TST_TEST_CASE INC
		INNER JOIN TST_TEST_CASE_PRIORITY T1 ON INC.TEST_CASE_PRIORITY_ID = T1.TEST_CASE_PRIORITY_ID
		INNER JOIN TST_TEST_CASE_PRIORITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TEST_CASE_ID, T1.TEST_CASE_PRIORITY_ID AS OLD_TEST_CASE_PRIORITY_ID, NULL AS NEW_TEST_CASE_PRIORITY_ID
	FROM TST_TEST_CASE INC
		INNER JOIN TST_TEST_CASE_PRIORITY T1 ON INC.TEST_CASE_PRIORITY_ID = T1.TEST_CASE_PRIORITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TEST_CASE_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TEST_CASE_ID = MAP.TEST_CASE_ID
WHERE 
	MAP.TEST_CASE_ID = INC.TEST_CASE_ID	
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TemplateManager
-- Description:		Retrieves the list of templates that the
--					specified user is the owner/admin for
-- ================================================================
IF OBJECT_ID ( 'TEMPLATE_RETRIEVE_BY_OWNER', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_RETRIEVE_BY_OWNER;
GO
CREATE PROCEDURE TEMPLATE_RETRIEVE_BY_OWNER
	@UserId INT
AS
BEGIN
	SELECT DISTINCT TMP.*
	FROM TST_PROJECT_TEMPLATE TMP
		INNER JOIN TST_PROJECT PRJ ON TMP.PROJECT_TEMPLATE_ID = PRJ.PROJECT_TEMPLATE_ID
		INNER JOIN TST_PROJECT_USER PRU ON PRJ.PROJECT_ID = PRU.PROJECT_ID
		INNER JOIN TST_PROJECT_ROLE PRR ON PRU.PROJECT_ROLE_ID = PRR.PROJECT_ROLE_ID
	WHERE
		PRR.IS_TEMPLATE_ADMIN = 1 AND
		PRU.USER_ID = @UserId
	ORDER BY TMP.NAME, TMP.PROJECT_TEMPLATE_ID
END
GO
-- ================================================================ 
-- Author:			Inflectra Corporation 
-- Business Object: TestCase 
-- Description:		Adds any test cases attached to the requirement to the release 
-- Updated 11/3/2020 - [TK:2479] For Baseline History recording. 
-- Updated 2/20/2021 - Don't add the test case if not in the same project
-- ================================================================ 
IF OBJECT_ID ( 'TESTCASE_ADD_TO_REQUIREMENT_RELEASE', 'P' ) IS NOT NULL  
    DROP PROCEDURE TESTCASE_ADD_TO_REQUIREMENT_RELEASE
GO
CREATE PROCEDURE TESTCASE_ADD_TO_REQUIREMENT_RELEASE 
	@ProjectId INT, 
	@RequirementId INT, 
	@ReleaseId INT, 
	@RecordHistory BIT, 
	@UserId INT 
AS 
BEGIN 
	DECLARE 
		@IsIterationOrPhase BIT, /* Check if we may have a parent. */ 
		@IndentLevel NVARCHAR(MAX), /* The Indent level of the Release we're adding to. */ 
		@ParentReleaseId INT, /* The parent release ID (if any). */ 
		@ChangeSetDate DATETIME2, /* For re-selecting the Changeset ID. */ 
		@ChangeSetId BIGINT, /* The Changeset ID to insert the child records. */ 
		@ReleaseName NVARCHAR(MAX) /* The name of the Release, used for History. */ 
	 
	--See if we have an iteration 
	SELECT @IsIterationOrPhase = (CASE RELEASE_TYPE_ID 
									WHEN 3 THEN 1 
									WHEN 4 THEN 1 
									ELSE 0 END), 
		@IndentLevel = INDENT_LEVEL 
	FROM TST_RELEASE 
	WHERE RELEASE_ID = @ReleaseId; 
 
 	-- Add the Changeset for this item. 
	SELECT @ChangeSetId = 0;
	IF (@RecordHistory = 1)
	BEGIN 
		IF EXISTS (
			SELECT *
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND TST.PROJECT_ID = @ProjectId
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ReleaseId)
			)
		BEGIN
			-- Get the parent's Release name. 
			SELECT @ReleaseName = NAME 
				FROM TST_RELEASE 
				WHERE RELEASE_ID = @ReleaseId;
			-- Get the date. -- 
			SELECT @ChangeSetDate = GETUTCDATE(); 
			-- Insert our master Changeset. 
			INSERT INTO TST_HISTORY_CHANGESET (USER_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, CHANGE_DATE, CHANGETYPE_ID, PROJECT_ID, ARTIFACT_DESC) 
			VALUES (@UserId, 4 /* Release */, @ReleaseId, @ChangeSetDate, 13 /* Association Add */, @ProjectId, @ReleaseName);
			-- Get the Changeset ID we just wrote
			SET @ChangeSetId = @@IDENTITY;
			-- Now Write out the Association changes. 
			INSERT INTO TST_HISTORY_ASSOCIATION ( 
				CHANGESET_ID,  
				SOURCE_ARTIFACT_TYPE_ID,  
				SOURCE_ARTIFACT_ID,  
				DEST_ARTIFACT_TYPE_ID, 
				DEST_ARTIFACT_ID) 
				SELECT 
					@ChangeSetId, 
					4, /* Artifact Type: Release */ 
					@ReleaseId, 
					2, /* Artifact Type: Test Case */ 
					RTC.TEST_CASE_ID 
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ReleaseId);
		END
	END 

	--Insert into the TestCase<->Release Table. 
	INSERT INTO TST_RELEASE_TEST_CASE 
		(RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID) 
	 
		--Get the list of test cases that belong to this requirement (that are not deleted) 
		--they need to be in the same project
		SELECT @ReleaseId, TST.TEST_CASE_ID, 3 /*Not Run*/ AS EXECUTION_STATUS_ID 
		FROM TST_REQUIREMENT_TEST_CASE RTC 
		INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
		WHERE RTC.REQUIREMENT_ID = @RequirementId 
			AND TST.IS_DELETED = 0 
			AND TST.PROJECT_ID = @ProjectId
			AND RTC.TEST_CASE_ID NOT IN ( 
				SELECT TEST_CASE_ID 
				FROM TST_RELEASE_TEST_CASE 
				WHERE RELEASE_ID = @ReleaseId);
	 
	--Do the same for parent release if an iteration/phase 
	IF @IsIterationOrPhase = 1 
	BEGIN 
		SELECT @ParentReleaseId = RELEASE_ID 
		FROM TST_RELEASE 
		WHERE PROJECT_ID = @ProjectId 
		AND LEN(INDENT_LEVEL) = LEN(@IndentLevel) - 3 
		AND SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL;
		 
		IF @ParentReleaseId IS NOT NULL 
		BEGIN 
			-- Need to insert into the HISTORY table. 
			IF (@RecordHistory = 1)
			BEGIN 
				IF EXISTS (
					SELECT *
					FROM TST_REQUIREMENT_TEST_CASE RTC 
					INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
					WHERE RTC.REQUIREMENT_ID = @RequirementId 
						AND TST.IS_DELETED = 0 
						AND TST.PROJECT_ID = @ProjectId
						AND RTC.TEST_CASE_ID NOT IN ( 
							SELECT TEST_CASE_ID 
							FROM TST_RELEASE_TEST_CASE 
							WHERE RELEASE_ID = @ParentReleaseId)
					)
				BEGIN
					-- Get the parent's Release name. 
					SELECT @ReleaseName = NAME 
						FROM TST_RELEASE 
						WHERE RELEASE_ID = @ParentReleaseId;
					-- Get the date. -- 
					SET @ChangeSetDate = GETUTCDATE();
					-- Insert our master Changeset. 
					INSERT INTO TST_HISTORY_CHANGESET (USER_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, CHANGE_DATE, CHANGETYPE_ID, PROJECT_ID, ARTIFACT_DESC) 
					VALUES (@UserId, 4 /* Release */, @ParentReleaseId, @ChangeSetDate, 13 /* Association Add */, @ProjectId, @ReleaseName);
					-- Get the Changeset ID we just wrote.
					SET @ChangeSetId = @@IDENTITY;
					-- Now Write out the Association changes. 
					INSERT INTO TST_HISTORY_ASSOCIATION ( 
						CHANGESET_ID,  
						SOURCE_ARTIFACT_TYPE_ID,  
						SOURCE_ARTIFACT_ID,  
						DEST_ARTIFACT_TYPE_ID, 
						DEST_ARTIFACT_ID) 
						SELECT 
							@ChangeSetId, 
							4, /* Artifact Type: Release */ 
							@ParentReleaseId, 
							2, /* Artifact Type: Test Case */ 
							RTC.TEST_CASE_ID 
						FROM TST_REQUIREMENT_TEST_CASE RTC 
						INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
						WHERE RTC.REQUIREMENT_ID = @RequirementId 
							AND TST.IS_DELETED = 0 
							AND RTC.TEST_CASE_ID NOT IN ( 
								SELECT TEST_CASE_ID 
								FROM TST_RELEASE_TEST_CASE 
								WHERE RELEASE_ID = @ParentReleaseId);
				END
			END 

			--Insert into the TestCase<->Release table. 
			INSERT INTO TST_RELEASE_TEST_CASE 
				(RELEASE_ID, TEST_CASE_ID, EXECUTION_STATUS_ID)
				--Get the list of test cases that belong to this requirement (that are not deleted) 
				SELECT @ParentReleaseId, TST.TEST_CASE_ID, 3 /*Not Run*/ AS EXECUTION_STATUS_ID 
				FROM TST_REQUIREMENT_TEST_CASE RTC 
				INNER JOIN TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID 
				WHERE RTC.REQUIREMENT_ID = @RequirementId 
					AND TST.IS_DELETED = 0 
					AND TST.PROJECT_ID = @ProjectId
					AND RTC.TEST_CASE_ID NOT IN ( 
						SELECT TEST_CASE_ID 
						FROM TST_RELEASE_TEST_CASE 
						WHERE RELEASE_ID = @ParentReleaseId);	 
		END 
	END 
END 
GO 
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes a Test Case
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE;
GO
CREATE PROCEDURE TESTCASE_DELETE
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete user subscriptions.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 2 AND ARTIFACT_ID = @TestCaseId);
	--Now delete the test case itself
	DELETE FROM TST_TEST_CASE_SIGNATURE WHERE TEST_CASE_ID = @TestCaseId;
    DELETE FROM TST_TEST_CASE WHERE TEST_CASE_ID = @TestCaseId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes a Test Case Parameter
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE_PARAMETER', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE_PARAMETER;
GO
CREATE PROCEDURE TESTCASE_DELETE_PARAMETER
	@TestCaseParameterId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the varies entries
	DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE WHERE TEST_CASE_PARAMETER_ID = @TestCaseParameterId;
	DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER WHERE TEST_CASE_PARAMETER_ID = @TestCaseParameterId;
    DELETE FROM TST_TEST_STEP_PARAMETER WHERE TEST_CASE_PARAMETER_ID = @TestCaseParameterId;
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER WHERE TEST_CASE_PARAMETER_ID = @TestCaseParameterId;
    DELETE FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_PARAMETER_ID = @TestCaseParameterId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes all the Test Case Parameters
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE_PARAMETER_BY_TEST', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE_PARAMETER_BY_TEST;
GO
CREATE PROCEDURE TESTCASE_DELETE_PARAMETER_BY_TEST
	@TestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the test configuration parameter values
	DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);
		
	--Delete the test set configuration parameters
	DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);
	
	--Delete the test step parameters
    DELETE FROM TST_TEST_STEP_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);

	--Delete the test set test case parameters
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER
	WHERE TEST_CASE_PARAMETER_ID IN
		(SELECT TEST_CASE_PARAMETER_ID FROM TST_TEST_CASE_PARAMETER WHERE TEST_CASE_ID = @TestCaseId);

	--Delete the test case parameters
    DELETE FROM TST_TEST_CASE_PARAMETER
	WHERE TEST_CASE_ID = @TestCaseId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Deletes a Test Step
-- =============================================
IF OBJECT_ID ( 'TESTCASE_DELETE_STEP', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_DELETE_STEP;
GO
CREATE PROCEDURE TESTCASE_DELETE_STEP
	@TestStepId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Unlink or delete the varies entries
	DELETE FROM TST_REQUIREMENT_TEST_STEP WHERE TEST_STEP_ID = @TestStepId;
    DELETE FROM TST_TEST_STEP WHERE TEST_STEP_ID = @TestStepId;
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves the total estimated testing duration for
--					a release/iteration. Does not include child iterations
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_GET_TOTAL_RELEASE_DURATION', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_GET_TOTAL_RELEASE_DURATION;
GO
CREATE PROCEDURE TESTCASE_GET_TOTAL_RELEASE_DURATION
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	SELECT SUM(TST.ESTIMATED_DURATION) AS ESTIMATED_DURATION
	FROM TST_TEST_CASE TST
	INNER JOIN TST_RELEASE_TEST_CASE RTC ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE TST.IS_DELETED = 0
	AND RTC.RELEASE_ID = @ReleaseId
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Refreshes the execution status and last-run date of a folder based on its child folders and child test cases
-- Remarks:			Does not update parents itself, would need to be called for each parent folder
-- =======================================================
IF OBJECT_ID ( 'TESTCASE_REFRESH_FOLDER_EXECUTION_STATUS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_REFRESH_FOLDER_EXECUTION_STATUS];
GO
CREATE PROCEDURE [TESTCASE_REFRESH_FOLDER_EXECUTION_STATUS]
	@ProjectId INT,
	@TestCaseFolderId INT
AS
BEGIN
	SET ANSI_WARNINGS OFF	--We want to SUM/AVG values that have NULLs
	SET NOCOUNT ON
	DECLARE
		@CountNotApplicable INT,
		@CountNotRun INT,
		@CountPassed INT,
		@CountFailed INT,
		@CountCaution INT,
		@CountBlocked INT,
		@EstimatedDuration1 INT,
		@EstimatedDuration2 INT,
		@ActualDuration1 INT,
		@ActualDuration2 INT,
		@ExecutionDate DATETIME

	--Default counts
	SET @CountNotApplicable = 0
	SET @CountNotRun = 0
	SET @CountPassed = 0
	SET @CountFailed = 0
	SET @CountCaution = 0
	SET @CountBlocked = 0

	--Sum child folders and test cases
	--N/A
	SELECT	@CountNotApplicable = @CountNotApplicable + ISNULL(SUM(COUNT_NOT_APPLICABLE), 0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountNotApplicable = @CountNotApplicable + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 4 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0
	
	--Not Run
	SELECT	@CountNotRun = @CountNotRun + ISNULL(SUM(COUNT_NOT_RUN),0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountNotRun = @CountNotRun + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 3 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Passed
	SELECT	@CountPassed = @CountPassed + ISNULL(SUM(COUNT_PASSED),0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountPassed = @CountPassed + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 2 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Failed
	SELECT	@CountFailed = @CountFailed + ISNULL(SUM(COUNT_FAILED),0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountFailed = @CountFailed + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 1 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Caution
	SELECT	@CountCaution = @CountCaution + ISNULL(SUM(COUNT_CAUTION),0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountCaution = @CountCaution + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 6 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Blocked
	SELECT	@CountBlocked = @CountBlocked + ISNULL(SUM(COUNT_BLOCKED),0)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@CountBlocked = @CountBlocked + COUNT(TEST_CASE_ID)
	FROM TST_TEST_CASE WHERE EXECUTION_STATUS_ID = 5 AND TEST_CASE_FOLDER_ID = @TestCaseFolderId
		AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Estimated Duration
	SELECT	@EstimatedDuration1 = SUM(ESTIMATED_DURATION)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@EstimatedDuration2 = SUM(ESTIMATED_DURATION)
	FROM TST_TEST_CASE WHERE TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId AND IS_DELETED = 0
		
	--Actual Duration
	SELECT	@ActualDuration1 = SUM(ACTUAL_DURATION)
	FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
	SELECT	@ActualDuration2 = SUM(ACTUAL_DURATION)
	FROM TST_TEST_CASE WHERE TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId AND IS_DELETED = 0

	--Execution Date
	SELECT @ExecutionDate = MIN(EXECUTION_DATE)
	FROM
		(
		SELECT MIN(EXECUTION_DATE) AS EXECUTION_DATE
		FROM TST_TEST_CASE_FOLDER WHERE PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId
		UNION
		SELECT MIN(EXECUTION_DATE) AS EXECUTION_DATE
		FROM TST_TEST_CASE WHERE TEST_CASE_FOLDER_ID = @TestCaseFolderId AND PROJECT_ID = @ProjectId AND IS_DELETED = 0
		) VW

	--Now actually update the folder
	--Have to use COALESCE when adding values to avoid NULL issues
    UPDATE TST_TEST_CASE_FOLDER
		SET COUNT_NOT_APPLICABLE = @CountNotApplicable,
		COUNT_NOT_RUN = @CountNotRun,
		COUNT_PASSED = @CountPassed,
		COUNT_FAILED = @CountFailed,
		COUNT_CAUTION = @CountCaution,
		COUNT_BLOCKED = @CountBlocked,
		ESTIMATED_DURATION = COALESCE(@EstimatedDuration1 + @EstimatedDuration2, @EstimatedDuration1, @EstimatedDuration2),
		ACTUAL_DURATION = COALESCE(@ActualDuration1 + @ActualDuration2, @ActualDuration1, @ActualDuration2),
		EXECUTION_DATE = @ExecutionDate   
    WHERE TEST_CASE_FOLDER_ID = @TestCaseFolderId

	--Now we need to do the same thing for the release folder execution data as well
	MERGE TST_RELEASE_TEST_CASE_FOLDER AS TARGET
	USING (
		SELECT
			RELEASE_ID,
			TEST_CASE_FOLDER_ID,
			SUM(COUNT_FAILED) AS COUNT_FAILED,
			SUM(COUNT_PASSED) AS COUNT_PASSED,
			SUM(COUNT_NOT_RUN) AS COUNT_NOT_RUN,
			SUM(COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
			SUM(COUNT_BLOCKED) AS COUNT_BLOCKED,
			SUM(COUNT_CAUTION) AS COUNT_CAUTION,
			MIN(EXECUTION_DATE) AS EXECUTION_DATE,
			SUM(ACTUAL_DURATION) AS ACTUAL_DURATION
		FROM
		(
			SELECT
				RTC.RELEASE_ID,
				@TestCaseFolderId AS TEST_CASE_FOLDER_ID,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 1 THEN 1 ELSE 0 END) AS COUNT_FAILED,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 2 THEN 1 ELSE 0 END) AS COUNT_PASSED,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 3 THEN 1 ELSE 0 END) AS COUNT_NOT_RUN,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 4 THEN 1 ELSE 0 END) AS COUNT_NOT_APPLICABLE,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 5 THEN 1 ELSE 0 END) AS COUNT_BLOCKED,
				SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 6 THEN 1 ELSE 0 END) AS COUNT_CAUTION,
				MIN(RTC.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(RTC.ACTUAL_DURATION) AS ACTUAL_DURATION
			FROM
				TST_RELEASE_TEST_CASE RTC
			INNER JOIN
				TST_TEST_CASE TST ON RTC.TEST_CASE_ID = TST.TEST_CASE_ID
			WHERE
				TST.TEST_CASE_FOLDER_ID = @TestCaseFolderId AND
				TST.IS_DELETED = 0
			GROUP BY RELEASE_ID
			UNION
			SELECT
				RTF.RELEASE_ID,
				@TestCaseFolderId AS TEST_CASE_FOLDER_ID,
				SUM(RTF.COUNT_FAILED) AS COUNT_FAILED,
				SUM(RTF.COUNT_PASSED) AS COUNT_PASSED,
				SUM(RTF.COUNT_NOT_RUN) AS COUNT_NOT_RUN,
				SUM(RTF.COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
				SUM(RTF.COUNT_BLOCKED) AS COUNT_BLOCKED,
				SUM(RTF.COUNT_CAUTION) AS COUNT_CAUTION,				
				MIN(RTF.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(RTF.ACTUAL_DURATION) AS ACTUAL_DURATION
			FROM
				TST_RELEASE_TEST_CASE_FOLDER RTF
			INNER JOIN
				TST_TEST_CASE_FOLDER TSF ON RTF.TEST_CASE_FOLDER_ID = TSF.TEST_CASE_FOLDER_ID
			WHERE
				TSF.PARENT_TEST_CASE_FOLDER_ID = @TestCaseFolderId
			GROUP BY RELEASE_ID
		) AS GRP
		GROUP BY RELEASE_ID, TEST_CASE_FOLDER_ID
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID AND
		TARGET.TEST_CASE_FOLDER_ID = SOURCE.TEST_CASE_FOLDER_ID
	WHEN MATCHED THEN
		UPDATE
			SET
				TARGET.COUNT_PASSED = SOURCE.COUNT_PASSED,
				TARGET.COUNT_FAILED = SOURCE.COUNT_FAILED,
				TARGET.COUNT_CAUTION = SOURCE.COUNT_CAUTION,
				TARGET.COUNT_BLOCKED = SOURCE.COUNT_BLOCKED,
				TARGET.COUNT_NOT_RUN = SOURCE.COUNT_NOT_RUN,
				TARGET.COUNT_NOT_APPLICABLE = SOURCE.COUNT_NOT_APPLICABLE,
				TARGET.ACTUAL_DURATION = SOURCE.ACTUAL_DURATION,
				TARGET.EXECUTION_DATE = SOURCE.EXECUTION_DATE
	WHEN NOT MATCHED BY TARGET THEN 
		INSERT (RELEASE_ID, TEST_CASE_FOLDER_ID,
			COUNT_PASSED, COUNT_FAILED, COUNT_CAUTION, COUNT_BLOCKED, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE,
			ACTUAL_DURATION, EXECUTION_DATE) 
		VALUES (SOURCE.RELEASE_ID, SOURCE.TEST_CASE_FOLDER_ID,
			SOURCE.COUNT_PASSED, SOURCE.COUNT_FAILED, SOURCE.COUNT_CAUTION, SOURCE.COUNT_BLOCKED, SOURCE.COUNT_NOT_RUN,
			SOURCE.COUNT_NOT_APPLICABLE, SOURCE.ACTUAL_DURATION, SOURCE.EXECUTION_DATE)
	WHEN NOT MATCHED BY SOURCE AND TARGET.TEST_CASE_FOLDER_ID = @TestCaseFolderId THEN 
		DELETE;
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Refreshes the folder hierarchy for the specified project
-- =====================================================================
IF OBJECT_ID ( 'TESTCASE_REFRESH_FOLDER_HIERARCHY', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_REFRESH_FOLDER_HIERARCHY];
GO
CREATE PROCEDURE [TESTCASE_REFRESH_FOLDER_HIERARCHY]
	@ProjectId INT
AS
BEGIN
	--First delete the existing folders
	DELETE FROM TST_TEST_CASE_FOLDER_HIERARCHY
	WHERE PROJECT_ID = @ProjectId;

	WITH TEST_CASE_FOLDER_HIERARCHY (TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	AS
	(
		SELECT	TKF.TEST_CASE_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TEST_CASE_FOLDER_ID, 1 AS HIERARCHY_LEVEL,
				CAST(dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS NVARCHAR(MAX)) AS INDENT_LEVEL
		FROM TST_TEST_CASE_FOLDER TKF
		WHERE TKF.PARENT_TEST_CASE_FOLDER_ID IS NULL AND TKF.PROJECT_ID = @ProjectId
		UNION ALL
		SELECT	TKF.TEST_CASE_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TEST_CASE_FOLDER_ID, (CTE.HIERARCHY_LEVEL + 1) AS HIERARCHY_LEVEL,
				CTE.INDENT_LEVEL + dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS INDENT_LEVEL
		FROM TST_TEST_CASE_FOLDER TKF
		INNER JOIN TEST_CASE_FOLDER_HIERARCHY CTE ON TKF.PARENT_TEST_CASE_FOLDER_ID = CTE.TEST_CASE_FOLDER_ID
		WHERE TKF.PARENT_TEST_CASE_FOLDER_ID IS NOT NULL AND TKF.PROJECT_ID = @ProjectId
	)
	INSERT INTO TST_TEST_CASE_FOLDER_HIERARCHY (TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	SELECT ISNULL(TEST_CASE_FOLDER_ID, 0) AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TEST_CASE_FOLDER_HIERARCHY
	ORDER BY PROJECT_ID, INDENT_LEVEL COLLATE Latin1_General_BIN
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Refreshes the hierarchy of test cases in a project
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_REFRESH_PARAMETER_HIERARCHY', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_REFRESH_PARAMETER_HIERARCHY;
GO
CREATE PROCEDURE TESTCASE_REFRESH_PARAMETER_HIERARCHY
	@ProjectId INT
AS
BEGIN
	--First drop the existing rows for this project
	DELETE FROM TST_TEST_CASE_PARAMETER_HIERARCHY WHERE PROJECT_ID = @ProjectId;
	DELETE FROM TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET WHERE PROJECT_ID = @ProjectId;	

	--First for when we want to include the values set already at lower-levels
	WITH TEST_CASE_PARAMETER_HIERARCHY (TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	AS
	(
		SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
		FROM TST_TEST_CASE_PARAMETER
		UNION ALL
		SELECT TCPH.TEST_CASE_PARAMETER_ID, STP.TEST_CASE_ID AS TEST_CASE_ID, TCPH.NAME, TCPH.DEFAULT_VALUE
		FROM TEST_CASE_PARAMETER_HIERARCHY TCPH INNER JOIN TST_TEST_STEP STP
		ON STP.LINKED_TEST_CASE_ID = TCPH.TEST_CASE_ID INNER JOIN TST_TEST_CASE TST
		ON STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE STP.LINKED_TEST_CASE_ID IS NOT NULL AND PROJECT_ID = @ProjectId
		AND STP.IS_DELETED = 0 AND TST.IS_DELETED = 0
	)
	INSERT INTO TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET (PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	SELECT @ProjectId AS PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, MAX(DEFAULT_VALUE) AS DEFAULT_VALUE
	FROM TEST_CASE_PARAMETER_HIERARCHY
	GROUP BY TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME
	ORDER BY TEST_CASE_ID, TEST_CASE_PARAMETER_ID;

	--Next for when we don't want to include the values set already at lower-levels
	WITH TEST_CASE_PARAMETER_HIERARCHY (TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	AS
	(
		SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
		FROM TST_TEST_CASE_PARAMETER
		UNION ALL
		SELECT TCPH.TEST_CASE_PARAMETER_ID, STP.TEST_CASE_ID AS TEST_CASE_ID, TCPH.NAME, TCPH.DEFAULT_VALUE
		FROM TEST_CASE_PARAMETER_HIERARCHY TCPH INNER JOIN TST_TEST_STEP STP
		ON STP.LINKED_TEST_CASE_ID = TCPH.TEST_CASE_ID INNER JOIN TST_TEST_CASE TST
		ON STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE STP.LINKED_TEST_CASE_ID IS NOT NULL AND PROJECT_ID = @ProjectId
		AND NOT TCPH.TEST_CASE_PARAMETER_ID IN
			(SELECT TSP.TEST_CASE_PARAMETER_ID
				FROM TST_TEST_STEP_PARAMETER TSP
				WHERE TSP.TEST_STEP_ID = STP.TEST_STEP_ID)
		AND STP.IS_DELETED = 0 AND TST.IS_DELETED = 0
	)
	INSERT INTO TST_TEST_CASE_PARAMETER_HIERARCHY (PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	SELECT @ProjectId AS PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, MAX(DEFAULT_VALUE) AS DEFAULT_VALUE
	FROM	TEST_CASE_PARAMETER_HIERARCHY
	GROUP BY TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME
	ORDER BY TEST_CASE_ID, TEST_CASE_PARAMETER_ID;		
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Refreshes the hierarchy of parameters for a test case
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_REFRESH_PARAMETER_HIERARCHY_FOR_TEST_CASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_REFRESH_PARAMETER_HIERARCHY_FOR_TEST_CASE;
GO
CREATE PROCEDURE TESTCASE_REFRESH_PARAMETER_HIERARCHY_FOR_TEST_CASE
	@ProjectId INT,
	@TestCaseId INT
AS
BEGIN
	--First drop the existing rows for this project
	DELETE
		FROM TST_TEST_CASE_PARAMETER_HIERARCHY
		WHERE
			PROJECT_ID = @ProjectId
			AND TEST_CASE_ID = @TestCaseId;
	DELETE
		FROM TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET
		WHERE
			PROJECT_ID = @ProjectId
			AND TEST_CASE_ID = @TestCaseId;

	--First for when we want to include the values set already at lower-levels
	WITH TEST_CASE_PARAMETER_HIERARCHY (TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	AS
	(
		SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
		FROM TST_TEST_CASE_PARAMETER
		UNION ALL
		SELECT TCPH.TEST_CASE_PARAMETER_ID, STP.TEST_CASE_ID AS TEST_CASE_ID, TCPH.NAME, TCPH.DEFAULT_VALUE
		FROM TEST_CASE_PARAMETER_HIERARCHY TCPH INNER JOIN TST_TEST_STEP STP
		ON STP.LINKED_TEST_CASE_ID = TCPH.TEST_CASE_ID INNER JOIN TST_TEST_CASE TST
		ON STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE STP.LINKED_TEST_CASE_ID IS NOT NULL AND PROJECT_ID = @ProjectId
		AND STP.IS_DELETED = 0 AND TST.IS_DELETED = 0
	)
	INSERT INTO TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET (PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	SELECT @ProjectId AS PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, MAX(DEFAULT_VALUE) AS DEFAULT_VALUE
	FROM TEST_CASE_PARAMETER_HIERARCHY
	WHERE TEST_CASE_ID = @TestCaseId
	GROUP BY TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME
	ORDER BY TEST_CASE_ID, TEST_CASE_PARAMETER_ID;

	--Next for when we don't want to include the values set already at lower-levels
	WITH TEST_CASE_PARAMETER_HIERARCHY (TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	AS
	(
		SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
		FROM TST_TEST_CASE_PARAMETER
		UNION ALL
		SELECT TCPH.TEST_CASE_PARAMETER_ID, STP.TEST_CASE_ID AS TEST_CASE_ID, TCPH.NAME, TCPH.DEFAULT_VALUE
		FROM TEST_CASE_PARAMETER_HIERARCHY TCPH INNER JOIN TST_TEST_STEP STP
		ON STP.LINKED_TEST_CASE_ID = TCPH.TEST_CASE_ID INNER JOIN TST_TEST_CASE TST
		ON STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE STP.LINKED_TEST_CASE_ID IS NOT NULL AND PROJECT_ID = @ProjectId
		AND NOT TCPH.TEST_CASE_PARAMETER_ID IN
			(SELECT TSP.TEST_CASE_PARAMETER_ID
				FROM TST_TEST_STEP_PARAMETER TSP
				WHERE TSP.TEST_STEP_ID = STP.TEST_STEP_ID)
		AND STP.IS_DELETED = 0 AND TST.IS_DELETED = 0
	)
	INSERT INTO TST_TEST_CASE_PARAMETER_HIERARCHY (PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE)
	SELECT @ProjectId AS PROJECT_ID, TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, MAX(DEFAULT_VALUE) AS DEFAULT_VALUE
	FROM	TEST_CASE_PARAMETER_HIERARCHY
	WHERE TEST_CASE_ID = @TestCaseId
	GROUP BY TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME
	ORDER BY TEST_CASE_ID, TEST_CASE_PARAMETER_ID;		
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Gets the list of all children of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_CHILD_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_CHILD_FOLDERS];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_CHILD_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(255)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_CASE_FOLDER_HIERARCHY WHERE TEST_CASE_FOLDER_ID = @FolderId;

	--Now get the child folders
	SELECT TEST_CASE_FOLDER_ID AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_CASE_FOLDER_HIERARCHY
	WHERE SUBSTRING(INDENT_LEVEL, 1, LEN(@IndentLevel)) = @IndentLevel
	AND (LEN(INDENT_LEVEL) > LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL, TEST_CASE_FOLDER_ID
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves a list of test-case execution status summary for an entire project group
-- =====================================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_GROUP', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_GROUP];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_GROUP]
	@ProjectGroupId INT,
	@ActiveReleasesOnly BIT
AS
BEGIN
	IF @ActiveReleasesOnly = 1
	BEGIN
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID,
				MIN(EXE.NAME) AS EXECUTION_STATUS_NAME,
				COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE
		LEFT JOIN (SELECT RTC.TEST_CASE_ID, RTC.EXECUTION_STATUS_ID
					FROM TST_RELEASE_TEST_CASE RTC
					INNER JOIN TST_TEST_CASE TST ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
					INNER JOIN TST_PROJECT PRJ ON TST.PROJECT_ID = PRJ.PROJECT_ID
					INNER JOIN TST_RELEASE REL ON RTC.RELEASE_ID = REL.RELEASE_ID
					WHERE PRJ.IS_ACTIVE = 1
						AND REL.RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/)
						AND REL.IS_DELETED = 0
						AND PRJ.PROJECT_GROUP_ID = @ProjectGroupId
						AND TST.IS_DELETED = 0) AS TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		WHERE	EXE.EXECUTION_STATUS_ID <> 4 /* N/A */
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXECUTION_STATUS_ID
	END
	ELSE
	BEGIN
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID, MIN(EXE.NAME) AS EXECUTION_STATUS_NAME, COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE LEFT JOIN
			(SELECT TEST_CASE_ID, EXECUTION_STATUS_ID
				FROM TST_TEST_CASE TST2 INNER JOIN TST_PROJECT PRJ
				ON TST2.PROJECT_ID = PRJ.PROJECT_ID
				WHERE PRJ.IS_ACTIVE = 1 AND PRJ.PROJECT_GROUP_ID = @ProjectGroupId
				AND TST2.IS_DELETED = 0) TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		WHERE	EXE.EXECUTION_STATUS_ID <> 4 /* N/A */
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXE.EXECUTION_STATUS_ID
	END
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves a list of test-case execution status summary for a project / release
-- =====================================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_EXECUTION_STATUS_SUMMARY_PROJECT]
	@ProjectId INT,
	@ReleaseId INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	IF @ReleaseId IS NULL
	BEGIN
		--Create select command for retrieving the summary data - use outer join to ensure that we always
		--Return all execution status codes, so that the graph colors don't get mixed up!!
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID, MIN(EXE.NAME) AS EXECUTION_STATUS_NAME, COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE LEFT JOIN (SELECT TEST_CASE_ID, EXECUTION_STATUS_ID FROM TST_TEST_CASE WHERE PROJECT_ID = @ProjectId AND IS_DELETED = 0) AS TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXECUTION_STATUS_ID

	END
	ELSE
	BEGIN
		--Populate list of child iterations if we have a release specified
		--if we have @ReleaseId = -2 it means only load in active releases
		IF @ReleaseId = -2
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM TST_RELEASE WHERE PROJECT_ID = @ProjectId AND RELEASE_STATUS_ID NOT IN (4 /*Closed*/, 5 /*Deferred*/, 6 /*Cancelled*/) AND IS_DELETED = 0
		END
		ELSE
		BEGIN
			INSERT @ReleaseList (RELEASE_ID)
			SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
		END
		
		--Create select command for retrieving the summary data - use outer join to ensure that we always
        --Return all execution status codes, so that the graph colors don't get mixed up!!
		SELECT	EXE.EXECUTION_STATUS_ID AS EXECUTION_STATUS_ID,
				MIN(EXE.NAME) AS EXECUTION_STATUS_NAME,
				COUNT(TST.TEST_CASE_ID) AS STATUS_COUNT
		FROM	TST_EXECUTION_STATUS EXE
		LEFT JOIN (SELECT RTC.TEST_CASE_ID, RTC.EXECUTION_STATUS_ID
					FROM TST_RELEASE_TEST_CASE RTC
					INNER JOIN TST_TEST_CASE TST ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
					AND RTC.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
					WHERE TST.PROJECT_ID = @ProjectId
					AND TST.IS_DELETED = 0) AS TST
		ON		EXE.EXECUTION_STATUS_ID = TST.EXECUTION_STATUS_ID
		WHERE	EXE.EXECUTION_STATUS_ID <> 4 /* N/A */
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY EXECUTION_STATUS_ID
	END
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Retrieves the list of parameters for a test case
-- ================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_PARAMETERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_RETRIEVE_PARAMETERS;
GO
CREATE PROCEDURE TESTCASE_RETRIEVE_PARAMETERS
	@TestCaseId INT,
	@IncludeInherited BIT,
	@IncludeAlreadySet BIT
AS
DECLARE
	@ProjectId INT
BEGIN
	--First get the project ID of the test case
	SELECT @ProjectId = PROJECT_ID FROM TST_TEST_CASE WHERE TEST_CASE_ID = @TestCaseId;

	--Do we want to query multiple-levels of linking
	IF @IncludeInherited = 1
	BEGIN
		--Do we want to include the values set already at lower-levels
		IF @IncludeAlreadySet = 1
		BEGIN
			SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
			FROM TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET
			WHERE	TEST_CASE_ID = @TestCaseId AND PROJECT_ID = @ProjectId
			ORDER BY NAME, TEST_CASE_PARAMETER_ID	
		END
		ELSE
		BEGIN
			SELECT TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
			FROM TST_TEST_CASE_PARAMETER_HIERARCHY
			WHERE	TEST_CASE_ID = @TestCaseId AND PROJECT_ID = @ProjectId
			ORDER BY NAME, TEST_CASE_PARAMETER_ID			
		END
	END
	ELSE
	BEGIN
        SELECT	TEST_CASE_PARAMETER_ID, TEST_CASE_ID, NAME, DEFAULT_VALUE
        FROM	TST_TEST_CASE_PARAMETER
        WHERE	TEST_CASE_ID = @TestCaseId
        ORDER BY NAME, TEST_CASE_PARAMETER_ID
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TESTCASE_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCASE_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TESTCASE_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_CASE_FOLDER_HIERARCHY WHERE TEST_CASE_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TEST_CASE_FOLDER_ID AS TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_CASE_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestCase
-- Description:		Changes the steps flag of all the test cases that link to a test case
-- =============================================
IF OBJECT_ID ( 'TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG;
GO
CREATE PROCEDURE TESTCASE_UPDATE_PARENT_TESTSTEPS_FLAG
	@ProjectId INT,
	@LinkedTestCaseId INT,	/* The test case that is linked to */
	@LinkedTestCaseDeleted BIT	/* Has this test case been deleted or not */
AS
BEGIN
	UPDATE TC
		SET TC.IS_TEST_STEPS = ~VW.IS_TEST_STEPS	
	FROM TST_TEST_CASE TC INNER JOIN 
	(
		SELECT
			STP.TEST_CASE_ID,
			SUM(CASE WHEN (STP.LINKED_TEST_CASE_ID <> @LinkedTestCaseId OR @LinkedTestCaseDeleted = 0 OR STP.LINKED_TEST_CASE_ID IS NULL) THEN 1 ELSE 0 END) AS STEP_COUNT,
			TST.IS_TEST_STEPS
		FROM TST_TEST_STEP STP INNER JOIN TST_TEST_CASE TST
		ON	STP.TEST_CASE_ID = TST.TEST_CASE_ID
		WHERE
			STP.IS_DELETED = 0 AND
			STP.LINKED_TEST_CASE_ID = @LinkedTestCaseId AND
			TST.PROJECT_ID = @ProjectId
		GROUP BY STP.TEST_CASE_ID, TST.IS_TEST_STEPS) VW
	ON VW.TEST_CASE_ID = TC.TEST_CASE_ID
	WHERE ((VW.IS_TEST_STEPS = 1 AND VW.STEP_COUNT = 0)
	OR	  (VW.IS_TEST_STEPS = 0 AND VW.STEP_COUNT > 0))
	AND	TC.PROJECT_ID = @ProjectId
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestConfiguration
-- Description:		Populates the test configuration properties from the provided parameters
-- =====================================================================
IF OBJECT_ID ( 'TESTCONFIGURATION_DELETE_CONFIG_VALUES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCONFIGURATION_DELETE_CONFIG_VALUES];
GO
CREATE PROCEDURE [TESTCONFIGURATION_DELETE_CONFIG_VALUES]
	@TestConfigurationSetId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Delete the existing values
	DELETE FROM TST_TEST_CONFIGURATION_PARAMETER_VALUE WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION_SET_PARAMETER WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
	DELETE FROM TST_TEST_CONFIGURATION_SET WHERE TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId;
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestConfiguration
-- Description:		Retrieves a crosstab of all the configuration values in a set
-- =====================================================================
IF OBJECT_ID ( 'TESTCONFIGURATION_RETRIEVE_CONFIG_VALUES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTCONFIGURATION_RETRIEVE_CONFIG_VALUES];
GO
CREATE PROCEDURE [TESTCONFIGURATION_RETRIEVE_CONFIG_VALUES]
	@TestConfigurationSetId INT
AS
BEGIN
	SELECT	TCG.TEST_CONFIGURATION_ID, TCG.POSITION, TPV.TEST_CASE_PARAMETER_ID, TPV.CUSTOM_PROPERTY_VALUE_ID,
			TCP.NAME AS PARAMETER_NAME, CPV.NAME AS PARAMETER_VALUE
	FROM TST_TEST_CONFIGURATION TCG
	INNER JOIN TST_TEST_CONFIGURATION_PARAMETER_VALUE TPV ON TCG.TEST_CONFIGURATION_ID = TPV.TEST_CONFIGURATION_ID
	INNER JOIN TST_CUSTOM_PROPERTY_VALUE CPV ON TPV.CUSTOM_PROPERTY_VALUE_ID = CPV.CUSTOM_PROPERTY_VALUE_ID
	INNER JOIN TST_TEST_CASE_PARAMETER TCP ON TPV.TEST_CASE_PARAMETER_ID = TCP.TEST_CASE_PARAMETER_ID
	WHERE TCG.TEST_CONFIGURATION_SET_ID = @TestConfigurationSetId
ORDER BY TCG.POSITION ASC, TEST_CASE_PARAMETER_ID ASC, TCG.TEST_CONFIGURATION_ID
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Deletes by ID
-- =============================================
IF OBJECT_ID ( 'TESTRUN_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_DELETE];
GO
CREATE PROCEDURE [TESTRUN_DELETE]
	@TestRunId INT
AS
BEGIN
	SET NOCOUNT ON;
	-- Cascades delete the link from test run steps to incidents
		
	--Delete the test run and its steps
	DELETE FROM TST_TEST_RUN_STEP WHERE TEST_RUN_ID = @TestRunId;
	DELETE FROM TST_TEST_RUN WHERE TEST_RUN_ID = @TestRunId;
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Deletes by Test Case ID
-- =============================================
IF OBJECT_ID ( 'TESTRUN_DELETE_BY_TESTCASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTRUN_DELETE_BY_TESTCASE;
GO
CREATE PROCEDURE TESTRUN_DELETE_BY_TESTCASE
	@TestCaseId INT,
	@ArtifactTypeId INT
AS
BEGIN
	SET NOCOUNT ON;
	--Delete the dependent records
 	DELETE FROM TST_TEST_RUN_STEP_INCIDENT
	WHERE TEST_RUN_STEP_ID IN
		(SELECT TRS.TEST_RUN_STEP_ID
		FROM TST_TEST_RUN_STEP TRS INNER JOIN TST_TEST_RUN TRN
		ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
		WHERE TRN.TEST_CASE_ID = @TestCaseId);

	DELETE FROM TST_TEST_RUN_STEP WHERE TEST_RUN_ID IN
		(SELECT TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId);
	DELETE FROM TST_ARTIFACT_CUSTOM_PROPERTY
	WHERE ARTIFACT_ID IN
		(SELECT TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId)
	AND ARTIFACT_TYPE_ID = @ArtifactTypeId;

	--Now delete the test run itself
	DELETE FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId;
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Refreshes the execution status and last-run date of the test cases linked to
--					the test runs being updated
-- Remarks:			This version is used when we have a test runs pending id
-- =======================================================
IF OBJECT_ID ( 'TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS];
GO
CREATE PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS]
	@ProjectId INT,
	@TestRunsPendingId INT
AS
BEGIN
	DECLARE @ReleaseId INT
	DECLARE  @ParentsAndSelf TABLE
	(
		RELEASE_ID INT
	)
		
	SET NOCOUNT ON;
	SET ANSI_WARNINGS OFF;

	--First get the release id of the release/iteration that we were executed against
	SELECT TOP (1) @ReleaseId = RELEASE_ID
	FROM TST_TEST_RUN
	WHERE TEST_RUNS_PENDING_ID = @TestRunsPendingId AND RELEASE_ID IS NOT NULL

	--Populate list of self and parent releases that we need to consider
	INSERT @ParentsAndSelf (RELEASE_ID)
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS (@ProjectId, @ReleaseId)

	--Next update the test step status from the test run steps	
	--We need to do before we do the test case itself because otherwise the execution date will have changed
	UPDATE TST_TEST_STEP
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID
	FROM
		TST_TEST_STEP TSP
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TSP.TEST_STEP_ID = TRS.TEST_STEP_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	INNER JOIN
		TST_TEST_CASE TST ON TRN.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId

    --Now we need to select the list of test runs and update the 'last run' information
    --for the underlying test cases (provided we have a more recent end-date that the last-execution-date).
    --For test runs that are marked as not-run, we don't modify the test case status
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID		
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId
	
    --Next need to take into account any linked test cases, since they will need to update the linked test case instead.
	--We no longer updated teh status of the linked test cases because they don't have test runs
	--and there are difficulties trying to update the execution status using an aggregation function
	--where the linked test cases have multiple steps with different execution statuses
	/*UPDATE TST_TEST_CASE
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID,
		EXECUTION_DATE = TRS.END_DATE,
		ACTUAL_DURATION = TRS.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TRS.TEST_CASE_ID = TST.TEST_CASE_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.END_DATE IS NOT NULL AND
		(TRS.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId*/

	--Now we need to update the execution info for the release/test case information		
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON
			TST.TEST_CASE_ID = TRN.TEST_CASE_ID	AND
			TST.RELEASE_ID IN (SELECT RELEASE_ID FROM @ParentsAndSelf)
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUNS_PENDING_ID = @TestRunsPendingId

END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Refreshes the execution status and last-run date of the specified test cases
-- Remarks:			This version is used when we have a test run id
-- =======================================================
IF OBJECT_ID ( 'TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS2', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS2];
GO
CREATE PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS2]
	@ProjectId INT,
	@TestRunId INT
AS
BEGIN
	DECLARE @ReleaseId INT
	DECLARE  @ParentsAndSelf TABLE
	(
		RELEASE_ID INT
	)
		
	SET NOCOUNT ON;
	SET ANSI_WARNINGS OFF;

	--First get the release id of the release/iteration that we were executed against
	SELECT @ReleaseId = RELEASE_ID
	FROM TST_TEST_RUN
	WHERE TEST_RUN_ID = @TestRunId

	--Populate list of self and parent releases that we need to consider
	INSERT @ParentsAndSelf (RELEASE_ID)
	SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS (@ProjectId, @ReleaseId)

	--Next update the test step status from the test run steps	
	--We need to do before we do the test case itself because otherwise the execution date will have changed
	UPDATE TST_TEST_STEP
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID
	FROM
		TST_TEST_STEP TSP
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TSP.TEST_STEP_ID = TRS.TEST_STEP_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	INNER JOIN
		TST_TEST_CASE TST ON TRN.TEST_CASE_ID = TST.TEST_CASE_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUN_ID = @TestRunId

    --Now we need to select the list of test runs and update the 'last run' information
    --for the underlying test cases (provided we have a more recent end-date that the last-execution-date).
    --For test runs that are marked as not-run, we don't modify the test case status
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID		
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUN_ID = @TestRunId
	
    --Next need to take into account any linked test cases, since they will need to update the linked test case instead.
	--We no longer updated teh status of the linked test cases because they don't have test runs
	--and there are difficulties trying to update the execution status using an aggregation function
	--where the linked test cases have multiple steps with different execution statuses
	/*
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID,
		EXECUTION_DATE = TRS.END_DATE,
		ACTUAL_DURATION = TRS.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN_STEP TRS ON TRS.TEST_CASE_ID = TST.TEST_CASE_ID
	INNER JOIN
		TST_TEST_RUN TRN ON TRS.TEST_RUN_ID = TRN.TEST_RUN_ID
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRS.END_DATE IS NOT NULL AND
		(TRS.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUN_ID = @TestRunId*/

	--Now we need to update the execution info for the release/test case information		
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		TST_TEST_RUN TRN ON
			TST.TEST_CASE_ID = TRN.TEST_CASE_ID	AND
			TST.RELEASE_ID IN (SELECT RELEASE_ID FROM @ParentsAndSelf)
	WHERE
		TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND
		TRN.END_DATE IS NOT NULL AND
		(TRN.END_DATE > TST.EXECUTION_DATE OR TST.EXECUTION_DATE IS NULL) AND
		TRN.TEST_RUN_ID = @TestRunId

END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Refreshes the execution status and last-run date of the specified test cases
-- Remarks:			This version is used when we have a test case id
-- =======================================================
IF OBJECT_ID ( 'TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3];
GO
CREATE PROCEDURE [TESTRUN_REFRESH_TESTCASE_EXECUTION_STATUS3]
	@ProjectId INT,
	@TestCaseId INT
AS
BEGIN		
	SET NOCOUNT ON;

    --First update the test case execution data from the most recent run
    UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE TEST_CASE_ID = @TestCaseId;
	
	UPDATE TST_TEST_CASE
	SET
		EXECUTION_DATE = TRN.END_DATE,
		EXECUTION_STATUS_ID = TRN.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN.ACTUAL_DURATION
	FROM
		TST_TEST_CASE TST
	INNER JOIN
		(SELECT TRN1.TEST_CASE_ID, TRN1.EXECUTION_STATUS_ID, TRN1.END_DATE, TRN1.ACTUAL_DURATION
		FROM TST_TEST_RUN TRN1
		INNER JOIN
			(SELECT TOP 1 TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId AND EXECUTION_STATUS_ID <> 3 /* Not Run */ ORDER BY END_DATE DESC, TEST_RUN_ID DESC) TRN2
		ON TRN1.TEST_RUN_ID = TRN2.TEST_RUN_ID) TRN
	ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID;
	
	--Next update the test step status from the most recent test run
	UPDATE TST_TEST_STEP
	SET
		EXECUTION_STATUS_ID = TRS.EXECUTION_STATUS_ID
	FROM
		TST_TEST_STEP TSP
	INNER JOIN
		(SELECT TRS.TEST_STEP_ID, TRS.EXECUTION_STATUS_ID
		FROM TST_TEST_RUN_STEP TRS
		INNER JOIN
			(SELECT TOP 1 TEST_RUN_ID FROM TST_TEST_RUN WHERE TEST_CASE_ID = @TestCaseId AND EXECUTION_STATUS_ID <> 3 /* Not Run */ ORDER BY END_DATE DESC, TEST_RUN_ID DESC) TRN
		ON TRN.TEST_RUN_ID = TRS.TEST_RUN_ID) TRS
	ON TSP.TEST_STEP_ID	= TRS.TEST_STEP_ID;
	
	--Now we need to update the execution info for the release/test case information	
	--Create the CTE to get the most recent test run per release
	--Use the CROSS APPLY to handle rollup release considerations
    UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = NULL,
		EXECUTION_STATUS_ID = 3 /* Not Run */,
		ACTUAL_DURATION = NULL
	WHERE TEST_CASE_ID = @TestCaseId;
	
	WITH CTE AS
	(
		SELECT T1.TEST_RUN_ID, T2.RELEASE_ID, T1.END_DATE, T1.EXECUTION_STATUS_ID, T1.ACTUAL_DURATION, ROW_NUMBER() OVER
		(
			PARTITION BY T2.RELEASE_ID
			ORDER BY END_DATE DESC, TEST_RUN_ID DESC
		) AS TRN
		FROM TST_TEST_RUN T1
		CROSS APPLY dbo.FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS(@ProjectId, T1.RELEASE_ID) AS T2
		WHERE T1.TEST_CASE_ID = @TestCaseId AND T1.EXECUTION_STATUS_ID <> 3 /* Not Run */	
	)	
	UPDATE TST_RELEASE_TEST_CASE
	SET
		EXECUTION_DATE = TRN3.END_DATE,
		EXECUTION_STATUS_ID = TRN3.EXECUTION_STATUS_ID,
		ACTUAL_DURATION = TRN3.ACTUAL_DURATION
	FROM
		TST_RELEASE_TEST_CASE TST
	INNER JOIN
		(SELECT TRN1.TEST_CASE_ID, TRN2.RELEASE_ID, TRN1.EXECUTION_STATUS_ID, TRN1.END_DATE, TRN1.ACTUAL_DURATION
		FROM TST_TEST_RUN TRN1
		INNER JOIN
			(		
				SELECT TEST_RUN_ID, RELEASE_ID, END_DATE, EXECUTION_STATUS_ID, ACTUAL_DURATION FROM CTE
				WHERE TRN = 1
			) TRN2
		ON TRN1.TEST_RUN_ID = TRN2.TEST_RUN_ID
		) TRN3
	ON TST.TEST_CASE_ID = TRN3.TEST_CASE_ID 
	AND TST.RELEASE_ID = TRN3.RELEASE_ID
END
GO
-- ====================================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestRun
-- Description:		Retrieves a list of the daily count of test-runs for a day in the specified timezone offset
-- =====================================================================================
IF OBJECT_ID ( 'TESTRUN_RETRIEVE_DAILY_COUNT', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTRUN_RETRIEVE_DAILY_COUNT];
GO
CREATE PROCEDURE [TESTRUN_RETRIEVE_DAILY_COUNT]
	@ProjectId INT,
	@ReleaseId INT,
	@UtcOffsetHours INT,
	@UtcOffsetMinutes INT
AS
BEGIN
	--Declare results set
	DECLARE  @ReleaseList TABLE
	(
		RELEASE_ID INT
	)

	--Populate list of child iterations if we have a release specified
	IF @ReleaseId IS NOT NULL
	BEGIN
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)
	END
	
    --Create select command for retrieving the number of test runs per day
    --We need to reconstruct the dates to exclude the time component
    SELECT	TOP 5 TRN.EXECUTION_DATE, COUNT(TRN.TEST_RUN_ID) AS EXECUTION_COUNT
    FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute,@UtcOffsetMinutes,DATEADD(hour,@UtcOffsetHours,END_DATE)) AS FLOAT))AS DATETIME) AS EXECUTION_DATE,
			TEST_RUN_ID, TEST_CASE_ID, RELEASE_ID
			FROM TST_TEST_RUN
			WHERE EXECUTION_STATUS_ID <> 3 /* NotRun */) AS TRN INNER JOIN TST_TEST_CASE TST
    ON		TRN.TEST_CASE_ID = TST.TEST_CASE_ID
    WHERE
		TST.PROJECT_ID = @ProjectId AND
		TST.IS_DELETED = 0 AND
		(@ReleaseId IS NULL OR RELEASE_ID IN (
			SELECT RELEASE_ID FROM @ReleaseList))
    GROUP BY TRN.EXECUTION_DATE
    ORDER BY TRN.EXECUTION_DATE DESC
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Deletes a Test Set and associated data
-- =======================================================
IF OBJECT_ID ( 'TESTSET_DELETE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTSET_DELETE;
GO
CREATE PROCEDURE TESTSET_DELETE
	@TestSetId INT
AS
BEGIN
	SET NOCOUNT ON;

	--Delete from user subscriptions.
	DELETE FROM TST_NOTIFICATION_USER_SUBSCRIPTION WHERE (ARTIFACT_TYPE_ID = 8 AND ARTIFACT_ID = @TestSetId);

	--Delete the test cases in the set, not-cascadable
	DELETE FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_ID = @TestSetId;

	--Delete the test set. Other needed deletes are set to cascade.
	DELETE FROM TST_TEST_SET WHERE TEST_SET_ID = @TestSetId;
END
GO
-- =================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Deletes a Test Case in a Test Set
-- ==================================================
IF OBJECT_ID ( 'TESTSET_DELETE_TESTCASE', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTSET_DELETE_TESTCASE;
GO
CREATE PROCEDURE TESTSET_DELETE_TESTCASE
	@TestSetTestCaseId INT
AS
BEGIN
	SET NOCOUNT ON;

	--Remove the link to the test set from related entities
	UPDATE TST_TEST_RUN SET TEST_SET_TEST_CASE_ID = NULL WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;

	--Delete the varies entries
    DELETE FROM TST_TEST_SET_TEST_CASE_PARAMETER WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;
    DELETE FROM TST_TEST_SET_TEST_CASE WHERE TEST_SET_TEST_CASE_ID = @TestSetTestCaseId;
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Refreshes the execution data associated with a test set
-- Remarks:			If @TestSetId = NULL, it refreshes all test sets in the project
-- =======================================================
IF OBJECT_ID ( 'TESTSET_REFRESH_EXECUTION_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_REFRESH_EXECUTION_DATA];
GO
CREATE PROCEDURE [TESTSET_REFRESH_EXECUTION_DATA]
	@ProjectId INT,
	@TestSetId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET ANSI_WARNINGS OFF;

    --First reset the execution data to starting values
    UPDATE TST_TEST_SET
	SET
		ESTIMATED_DURATION = NULL,
		ACTUAL_DURATION = NULL,
		COUNT_PASSED = 0,
		COUNT_FAILED = 0,
		COUNT_CAUTION = 0,
		COUNT_BLOCKED = 0,
		COUNT_NOT_RUN = 0,
		COUNT_NOT_APPLICABLE = 0,
		EXECUTION_DATE = NULL
	WHERE TEST_SET_ID = @TestSetId;

	--Next we need to update the test set execution data
	MERGE TST_TEST_SET AS TSX
	USING (SELECT
			TEST_SET_ID, MIN(EXECUTION_DATE) AS EXECUTION_DATE, SUM(ESTIMATED_DURATION) AS ESTIMATED_DURATION, SUM(ACTUAL_DURATION) AS ACTUAL_DURATION,
			SUM(IS_FAILED) AS COUNT_FAILED, SUM(IS_PASSED) AS COUNT_PASSED, SUM(IS_NOT_RUN) AS COUNT_NOT_RUN, SUM(IS_BLOCKED) AS COUNT_BLOCKED,
			SUM(IS_CAUTION) AS COUNT_CAUTION, SUM(IS_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE
		FROM
			(SELECT TSE.TEST_SET_ID, TSC.TEST_CASE_ID, TSC.TEST_SET_TEST_CASE_ID, TRN.EXECUTION_DATE, TST.ESTIMATED_DURATION, TRN.ACTUAL_DURATION,
					(CASE TRN.EXECUTION_STATUS_ID WHEN 1 THEN 1 ELSE 0 END) AS IS_FAILED,
					(CASE TRN.EXECUTION_STATUS_ID WHEN 2 THEN 1 ELSE 0 END) AS IS_PASSED,
					(CASE ISNULL(TRN.EXECUTION_STATUS_ID,3) WHEN 3 THEN 1 ELSE 0 END) AS IS_NOT_RUN,
					(CASE TRN.EXECUTION_STATUS_ID WHEN 4 THEN 1 ELSE 0 END) AS IS_NOT_APPLICABLE,
					(CASE TRN.EXECUTION_STATUS_ID WHEN 5 THEN 1 ELSE 0 END) AS IS_BLOCKED,
					(CASE TRN.EXECUTION_STATUS_ID WHEN 6 THEN 1 ELSE 0 END) AS IS_CAUTION
			FROM TST_TEST_SET TSE LEFT JOIN TST_TEST_SET_TEST_CASE TSC
			ON TSE.TEST_SET_ID = TSC.TEST_SET_ID LEFT JOIN 
						(SELECT RUN1.TEST_SET_TEST_CASE_ID, MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID, MIN(RUN1.END_DATE) AS EXECUTION_DATE, MIN(RUN1.ESTIMATED_DURATION) AS ESTIMATED_DURATION, MIN(RUN1.ACTUAL_DURATION) AS ACTUAL_DURATION
						FROM TST_TEST_RUN RUN1 INNER JOIN
							(SELECT TEST_SET_TEST_CASE_ID, MAX(END_DATE) AS END_DATE
							FROM TST_TEST_RUN
							GROUP BY TEST_SET_TEST_CASE_ID) RUN2
						ON RUN1.TEST_SET_TEST_CASE_ID = RUN2.TEST_SET_TEST_CASE_ID AND RUN1.END_DATE = RUN2.END_DATE
						GROUP BY RUN1.TEST_SET_TEST_CASE_ID) TRN
			ON		TSC.TEST_SET_TEST_CASE_ID = TRN.TEST_SET_TEST_CASE_ID INNER JOIN TST_TEST_CASE TST
			ON     TST.TEST_CASE_ID = TSC.TEST_CASE_ID
			WHERE TSE.IS_DELETED = 0
				AND TST.IS_DELETED = 0
				AND (@TestSetId IS NULL OR TSE.TEST_SET_ID = @TestSetId)
				AND TSE.PROJECT_ID = @ProjectId
				AND TST.PROJECT_ID = @ProjectId) TSE2
		GROUP BY TEST_SET_ID) AS TSE3
		ON TSX.TEST_SET_ID = TSE3.TEST_SET_ID
	WHEN MATCHED THEN
		UPDATE
			SET
				TSX.ESTIMATED_DURATION = TSE3.ESTIMATED_DURATION,
				TSX.ACTUAL_DURATION = TSE3.ACTUAL_DURATION,
				TSX.COUNT_PASSED = TSE3.COUNT_PASSED,
				TSX.COUNT_FAILED = TSE3.COUNT_FAILED,
				TSX.COUNT_CAUTION = TSE3.COUNT_CAUTION,
				TSX.COUNT_BLOCKED = TSE3.COUNT_BLOCKED,
				TSX.COUNT_NOT_RUN = TSE3.COUNT_NOT_RUN,
				TSX.COUNT_NOT_APPLICABLE = TSE3.COUNT_NOT_APPLICABLE,
				TSX.EXECUTION_DATE = TSE3.EXECUTION_DATE;
				
	--Next we need to update the release data (for all releases)
	WITH CTE AS
	(
		SELECT T1.TEST_SET_ID, T1.TEST_RUN_ID, T2.RELEASE_ID AS RELEASE_ID, T1.TEST_SET_TEST_CASE_ID, T1.END_DATE, T1.EXECUTION_STATUS_ID, T1.ACTUAL_DURATION, ROW_NUMBER() OVER
		(
			PARTITION BY T2.RELEASE_ID, T1.TEST_SET_TEST_CASE_ID
			ORDER BY END_DATE DESC
		) AS TRN
		FROM TST_TEST_RUN T1
		CROSS APPLY dbo.FN_RELEASE_GET_SELF_AND_ROLLUP_PARENTS(@ProjectId, T1.RELEASE_ID) AS T2
		WHERE (T1.TEST_SET_ID = @TestSetId OR (@TestSetId IS NULL AND T1.TEST_SET_ID IS NOT NULL))AND T1.EXECUTION_STATUS_ID <> 3 /* Not Run */	
	)
	
	MERGE TST_RELEASE_TEST_SET AS TARGET
	USING (
		SELECT 
			RTC.TEST_SET_ID,
			RTC.RELEASE_ID,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 1 THEN 1 ELSE 0 END) AS COUNT_FAILED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 2 THEN 1 ELSE 0 END) AS COUNT_PASSED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 3 THEN 1 ELSE 0 END) AS COUNT_NOT_RUN,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 4 THEN 1 ELSE 0 END) AS COUNT_NOT_APPLICABLE,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 5 THEN 1 ELSE 0 END) AS COUNT_BLOCKED,
			SUM(CASE RTC.EXECUTION_STATUS_ID WHEN 6 THEN 1 ELSE 0 END) AS COUNT_CAUTION,
			MIN(RTC.END_DATE) AS EXECUTION_DATE,
			SUM(RTC.ACTUAL_DURATION) AS ACTUAL_DURATION
		FROM
		(
			SELECT TEST_SET_ID, TEST_RUN_ID, RELEASE_ID, TEST_SET_TEST_CASE_ID, END_DATE, EXECUTION_STATUS_ID, ACTUAL_DURATION
			FROM CTE
			WHERE TRN = 1
		) RTC
		GROUP BY RTC.TEST_SET_ID, RTC.RELEASE_ID
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID AND
		TARGET.TEST_SET_ID = SOURCE.TEST_SET_ID
	WHEN MATCHED THEN
		UPDATE
			SET
				TARGET.COUNT_PASSED = SOURCE.COUNT_PASSED,
				TARGET.COUNT_FAILED = SOURCE.COUNT_FAILED,
				TARGET.COUNT_CAUTION = SOURCE.COUNT_CAUTION,
				TARGET.COUNT_BLOCKED = SOURCE.COUNT_BLOCKED,
				TARGET.COUNT_NOT_RUN = SOURCE.COUNT_NOT_RUN,
				TARGET.COUNT_NOT_APPLICABLE = SOURCE.COUNT_NOT_APPLICABLE,
				TARGET.ACTUAL_DURATION = SOURCE.ACTUAL_DURATION,
				TARGET.EXECUTION_DATE = SOURCE.EXECUTION_DATE
	WHEN NOT MATCHED BY TARGET THEN 
		INSERT (RELEASE_ID, TEST_SET_ID,
			COUNT_PASSED, COUNT_FAILED, COUNT_CAUTION, COUNT_BLOCKED, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE,
			ACTUAL_DURATION, EXECUTION_DATE) 
		VALUES (SOURCE.RELEASE_ID, SOURCE.TEST_SET_ID,
			SOURCE.COUNT_PASSED, SOURCE.COUNT_FAILED, SOURCE.COUNT_CAUTION, SOURCE.COUNT_BLOCKED, SOURCE.COUNT_NOT_RUN,
			SOURCE.COUNT_NOT_APPLICABLE, SOURCE.ACTUAL_DURATION, SOURCE.EXECUTION_DATE);	
END
GO
-- =======================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Refreshes the execution status and last-run date of a folder based on its child folders and child test sets
-- Remarks:			Does not update parents itself, would need to be called for each parent folder
-- =======================================================
IF OBJECT_ID ( 'TESTSET_REFRESH_FOLDER_EXECUTION_STATUS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_REFRESH_FOLDER_EXECUTION_STATUS];
GO
CREATE PROCEDURE [TESTSET_REFRESH_FOLDER_EXECUTION_STATUS]
	@ProjectId INT,
	@TestSetFolderId INT
AS
BEGIN
	SET ANSI_WARNINGS OFF	--We want to SUM/AVG values that have NULLs
	SET NOCOUNT ON

	--First update the test set folders for all releases
	MERGE TST_TEST_SET_FOLDER AS TARGET
	USING (
		SELECT
			TEST_SET_FOLDER_ID,
			ISNULL(SUM(COUNT_FAILED),0) AS COUNT_FAILED,
			ISNULL(SUM(COUNT_PASSED),0) AS COUNT_PASSED,
			ISNULL(SUM(COUNT_NOT_RUN),0) AS COUNT_NOT_RUN,
			ISNULL(SUM(COUNT_NOT_APPLICABLE),0) AS COUNT_NOT_APPLICABLE,
			ISNULL(SUM(COUNT_BLOCKED),0) AS COUNT_BLOCKED,
			ISNULL(SUM(COUNT_CAUTION),0) AS COUNT_CAUTION,
			MIN(EXECUTION_DATE) AS EXECUTION_DATE,
			SUM(ACTUAL_DURATION) AS ACTUAL_DURATION,
			SUM(ESTIMATED_DURATION) AS ESTIMATED_DURATION
		FROM
		(
			SELECT
				@TestSetFolderId AS TEST_SET_FOLDER_ID,
				SUM(TSE.COUNT_FAILED) AS COUNT_FAILED,
				SUM(TSE.COUNT_PASSED) AS COUNT_PASSED,
				SUM(TSE.COUNT_NOT_RUN) AS COUNT_NOT_RUN,
				SUM(TSE.COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
				SUM(TSE.COUNT_BLOCKED) AS COUNT_BLOCKED,
				SUM(TSE.COUNT_CAUTION) AS COUNT_CAUTION,				
				MIN(TSE.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(TSE.ACTUAL_DURATION) AS ACTUAL_DURATION,
				SUM(TSE.ESTIMATED_DURATION) AS ESTIMATED_DURATION
			FROM
				TST_TEST_SET TSE
			WHERE
				TSE.TEST_SET_FOLDER_ID = @TestSetFolderId
				AND TSE.IS_DELETED = 0
			UNION
			SELECT
				@TestSetFolderId AS TEST_SET_FOLDER_ID,
				SUM(TSF.COUNT_FAILED) AS COUNT_FAILED,
				SUM(TSF.COUNT_PASSED) AS COUNT_PASSED,
				SUM(TSF.COUNT_NOT_RUN) AS COUNT_NOT_RUN,
				SUM(TSF.COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
				SUM(TSF.COUNT_BLOCKED) AS COUNT_BLOCKED,
				SUM(TSF.COUNT_CAUTION) AS COUNT_CAUTION,				
				MIN(TSF.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(TSF.ACTUAL_DURATION) AS ACTUAL_DURATION,
				SUM(TSF.ESTIMATED_DURATION) AS ESTIMATED_DURATION
			FROM
				TST_TEST_SET_FOLDER TSF
			WHERE
				TSF.PARENT_TEST_SET_FOLDER_ID = @TestSetFolderId
		) AS GRP
		GROUP BY TEST_SET_FOLDER_ID
	) AS SOURCE
	ON
		TARGET.TEST_SET_FOLDER_ID = SOURCE.TEST_SET_FOLDER_ID
	WHEN MATCHED THEN
		UPDATE
			SET
				TARGET.COUNT_PASSED = SOURCE.COUNT_PASSED,
				TARGET.COUNT_FAILED = SOURCE.COUNT_FAILED,
				TARGET.COUNT_CAUTION = SOURCE.COUNT_CAUTION,
				TARGET.COUNT_BLOCKED = SOURCE.COUNT_BLOCKED,
				TARGET.COUNT_NOT_RUN = SOURCE.COUNT_NOT_RUN,
				TARGET.COUNT_NOT_APPLICABLE = SOURCE.COUNT_NOT_APPLICABLE,
				TARGET.ESTIMATED_DURATION = SOURCE.ESTIMATED_DURATION,
				TARGET.ACTUAL_DURATION = SOURCE.ACTUAL_DURATION,
				TARGET.EXECUTION_DATE = SOURCE.EXECUTION_DATE;

	--Now we need to do the same thing for the release folder execution data as well
	MERGE TST_RELEASE_TEST_SET_FOLDER AS TARGET
	USING (
		SELECT
			RELEASE_ID,
			TEST_SET_FOLDER_ID,
			SUM(COUNT_FAILED) AS COUNT_FAILED,
			SUM(COUNT_PASSED) AS COUNT_PASSED,
			SUM(COUNT_NOT_RUN) AS COUNT_NOT_RUN,
			SUM(COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
			SUM(COUNT_BLOCKED) AS COUNT_BLOCKED,
			SUM(COUNT_CAUTION) AS COUNT_CAUTION,
			MIN(EXECUTION_DATE) AS EXECUTION_DATE,
			SUM(ACTUAL_DURATION) AS ACTUAL_DURATION
		FROM
		(
			SELECT
				RTC.RELEASE_ID,
				@TestSetFolderId AS TEST_SET_FOLDER_ID,
				SUM(RTC.COUNT_FAILED) AS COUNT_FAILED,
				SUM(RTC.COUNT_PASSED) AS COUNT_PASSED,
				SUM(RTC.COUNT_NOT_RUN) AS COUNT_NOT_RUN,
				SUM(RTC.COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
				SUM(RTC.COUNT_BLOCKED) AS COUNT_BLOCKED,
				SUM(RTC.COUNT_CAUTION) AS COUNT_CAUTION,				
				MIN(RTC.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(RTC.ACTUAL_DURATION) AS ACTUAL_DURATION
			FROM
				TST_RELEASE_TEST_SET RTC
			INNER JOIN
				TST_TEST_SET TSE ON RTC.TEST_SET_ID = TSE.TEST_SET_ID
			WHERE
				TSE.TEST_SET_FOLDER_ID = @TestSetFolderId
				AND TSE.IS_DELETED = 0
			GROUP BY RTC.RELEASE_ID
			UNION
			SELECT
				RTF.RELEASE_ID,
				@TestSetFolderId AS TEST_SET_FOLDER_ID,
				SUM(RTF.COUNT_FAILED) AS COUNT_FAILED,
				SUM(RTF.COUNT_PASSED) AS COUNT_PASSED,
				SUM(RTF.COUNT_NOT_RUN) AS COUNT_NOT_RUN,
				SUM(RTF.COUNT_NOT_APPLICABLE) AS COUNT_NOT_APPLICABLE,
				SUM(RTF.COUNT_BLOCKED) AS COUNT_BLOCKED,
				SUM(RTF.COUNT_CAUTION) AS COUNT_CAUTION,				
				MIN(RTF.EXECUTION_DATE) AS EXECUTION_DATE,
				SUM(RTF.ACTUAL_DURATION) AS ACTUAL_DURATION
			FROM
				TST_RELEASE_TEST_SET_FOLDER RTF
			INNER JOIN
				TST_TEST_SET_FOLDER TSF ON RTF.TEST_SET_FOLDER_ID = TSF.TEST_SET_FOLDER_ID
			WHERE
				TSF.PARENT_TEST_SET_FOLDER_ID = @TestSetFolderId
			GROUP BY RELEASE_ID
		) AS GRP
		GROUP BY RELEASE_ID, TEST_SET_FOLDER_ID
	) AS SOURCE
	ON
		TARGET.RELEASE_ID = SOURCE.RELEASE_ID AND
		TARGET.TEST_SET_FOLDER_ID = SOURCE.TEST_SET_FOLDER_ID
	WHEN MATCHED THEN
		UPDATE
			SET
				TARGET.COUNT_PASSED = SOURCE.COUNT_PASSED,
				TARGET.COUNT_FAILED = SOURCE.COUNT_FAILED,
				TARGET.COUNT_CAUTION = SOURCE.COUNT_CAUTION,
				TARGET.COUNT_BLOCKED = SOURCE.COUNT_BLOCKED,
				TARGET.COUNT_NOT_RUN = SOURCE.COUNT_NOT_RUN,
				TARGET.COUNT_NOT_APPLICABLE = SOURCE.COUNT_NOT_APPLICABLE,
				TARGET.ACTUAL_DURATION = SOURCE.ACTUAL_DURATION,
				TARGET.EXECUTION_DATE = SOURCE.EXECUTION_DATE
	WHEN NOT MATCHED BY TARGET THEN 
		INSERT (RELEASE_ID, TEST_SET_FOLDER_ID,
			COUNT_PASSED, COUNT_FAILED, COUNT_CAUTION, COUNT_BLOCKED, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE,
			ACTUAL_DURATION, EXECUTION_DATE) 
		VALUES (SOURCE.RELEASE_ID, SOURCE.TEST_SET_FOLDER_ID,
			SOURCE.COUNT_PASSED, SOURCE.COUNT_FAILED, SOURCE.COUNT_CAUTION, SOURCE.COUNT_BLOCKED, SOURCE.COUNT_NOT_RUN,
			SOURCE.COUNT_NOT_APPLICABLE, SOURCE.ACTUAL_DURATION, SOURCE.EXECUTION_DATE);
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Refreshes the folder hierarchy for the specified project
-- =====================================================================
IF OBJECT_ID ( 'TESTSET_REFRESH_FOLDER_HIERARCHY', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_REFRESH_FOLDER_HIERARCHY];
GO
CREATE PROCEDURE [TESTSET_REFRESH_FOLDER_HIERARCHY]
	@ProjectId INT
AS
BEGIN
	--First delete the existing folders
	DELETE FROM TST_TEST_SET_FOLDER_HIERARCHY
	WHERE PROJECT_ID = @ProjectId;
	
	WITH TEST_SET_FOLDER_HIERARCHY (TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	AS
	(
		SELECT	TKF.TEST_SET_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TEST_SET_FOLDER_ID, 1 AS HIERARCHY_LEVEL,
				CAST(dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS NVARCHAR(MAX)) AS INDENT_LEVEL
		FROM TST_TEST_SET_FOLDER TKF
		WHERE TKF.PARENT_TEST_SET_FOLDER_ID IS NULL AND TKF.PROJECT_ID = @ProjectId
		UNION ALL
		SELECT	TKF.TEST_SET_FOLDER_ID, TKF.PROJECT_ID, TKF.NAME, TKF.PARENT_TEST_SET_FOLDER_ID, (CTE.HIERARCHY_LEVEL + 1) AS HIERARCHY_LEVEL,
				CTE.INDENT_LEVEL + dbo.FN_CREATE_INDENT_LEVEL(ROW_NUMBER() OVER(ORDER BY TKF.NAME)) AS INDENT_LEVEL
		FROM TST_TEST_SET_FOLDER TKF
		INNER JOIN TEST_SET_FOLDER_HIERARCHY CTE ON TKF.PARENT_TEST_SET_FOLDER_ID = CTE.TEST_SET_FOLDER_ID
		WHERE TKF.PARENT_TEST_SET_FOLDER_ID IS NOT NULL AND TKF.PROJECT_ID = @ProjectId
	)
	INSERT INTO TST_TEST_SET_FOLDER_HIERARCHY (TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL)
	SELECT ISNULL(TEST_SET_FOLDER_ID, 0) AS TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TEST_SET_FOLDER_HIERARCHY
	ORDER BY PROJECT_ID, INDENT_LEVEL COLLATE Latin1_General_BIN
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Gets the list of all children of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TESTSET_RETRIEVE_CHILD_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_RETRIEVE_CHILD_FOLDERS];
GO
CREATE PROCEDURE [TESTSET_RETRIEVE_CHILD_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_SET_FOLDER_HIERARCHY WHERE TEST_SET_FOLDER_ID = @FolderId;

	--Now get the child folders
	SELECT TEST_SET_FOLDER_ID AS TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_SET_FOLDER_HIERARCHY
	WHERE SUBSTRING(INDENT_LEVEL, 1, LEN(@IndentLevel)) = @IndentLevel
	AND (LEN(INDENT_LEVEL) > LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL, TEST_SET_FOLDER_ID
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Gets the list of all parents of the specified folder in hierarchy order
--                  Updated and improved 2/7/2020 By SWB
-- =====================================================================
IF OBJECT_ID ( 'TESTSET_RETRIEVE_PARENT_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_RETRIEVE_PARENT_FOLDERS];
GO
CREATE PROCEDURE [TESTSET_RETRIEVE_PARENT_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TEST_SET_FOLDER_HIERARCHY WHERE TEST_SET_FOLDER_ID = @FolderId;
	
	--Now get the parent folders
	SELECT TEST_SET_FOLDER_ID AS TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TEST_SET_FOLDER_HIERARCHY
	WHERE SUBSTRING(@IndentLevel, 1, LEN(INDENT_LEVEL)) = INDENT_LEVEL
	AND (LEN(INDENT_LEVEL) < LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL
END
GO
-- ================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Retrieves the summary info for the project home
-- ================================================================
IF OBJECT_ID ( 'TESTSET_RETRIEVE_SUMMARY_DATA', 'P' ) IS NOT NULL 
    DROP PROCEDURE TESTSET_RETRIEVE_SUMMARY_DATA;
GO
CREATE PROCEDURE TESTSET_RETRIEVE_SUMMARY_DATA
	@ProjectId INT,
	@ReleaseId INT,
	@IncludeDeleted BIT
AS
BEGIN
	--Handle the case where no release is specified separately
	IF @ReleaseId IS NULL
	BEGIN
		SELECT
			EXE.EXECUTION_STATUS_ID AS ExecutionStatusId,
			MIN(EXE.NAME) AS ExecutionStatusName,
			COUNT(TCL2.TEST_CASE_ID) AS StatusCount
		FROM VW_EXECUTION_STATUS_ACTIVE AS EXE
			LEFT JOIN (
				SELECT TCL.TEST_SET_ID,
					TCL.TEST_CASE_ID,
					ISNULL(TRN.EXECUTION_STATUS_ID,3) AS EXECUTION_STATUS_ID 
				FROM VW_TESTSET_TESTCASE_LIST AS TCL
					LEFT JOIN (
						SELECT RUN1.TEST_SET_ID,
							RUN1.TEST_SET_TEST_CASE_ID,
							RUN1.TEST_CASE_ID,
							MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID
						FROM TST_TEST_RUN AS RUN1
							INNER JOIN (
								SELECT TEST_CASE_ID,
									TEST_SET_ID,
									TEST_SET_TEST_CASE_ID,
									MAX(END_DATE) AS END_DATE 
								FROM TST_TEST_RUN 
								WHERE TEST_SET_ID IS NOT NULL
									AND EXECUTION_STATUS_ID <> 3
									AND EXECUTION_STATUS_ID <> 4 
								GROUP BY TEST_CASE_ID, TEST_SET_ID, TEST_SET_TEST_CASE_ID) AS RUN2 ON RUN1.TEST_CASE_ID = RUN2.TEST_CASE_ID
									AND RUN1.END_DATE = RUN2.END_DATE
						WHERE RUN1.TEST_SET_ID IS NOT NULL AND RUN1.EXECUTION_STATUS_ID <> 3 AND RUN1.EXECUTION_STATUS_ID <> 4
						GROUP BY RUN1.TEST_CASE_ID, RUN1.TEST_SET_ID, RUN1.TEST_SET_TEST_CASE_ID) AS TRN ON TCL.TEST_SET_ID = TRN.TEST_SET_ID AND TCL.TEST_CASE_ID = TRN.TEST_CASE_ID AND TRN.TEST_SET_TEST_CASE_ID = TCL.TEST_SET_TEST_CASE_ID				
				WHERE PROJECT_ID = @ProjectId AND (TCL.IS_TEST_CASE_DELETED = 0 OR @IncludeDeleted = 1) AND (TCL.IS_TEST_SET_DELETED = 0 OR @IncludeDeleted = 1)) AS TCL2 ON EXE.EXECUTION_STATUS_ID = TCL2.EXECUTION_STATUS_ID
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY ExecutionStatusId
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)

		--Populate list of child iterations
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_ITERATIONS (@ProjectId, @ReleaseId)

		SELECT	EXE.EXECUTION_STATUS_ID AS ExecutionStatusId, MIN(EXE.NAME) AS ExecutionStatusName, COUNT(TCL2.TEST_CASE_ID) AS StatusCount
		FROM VW_EXECUTION_STATUS_ACTIVE EXE LEFT JOIN
			(SELECT TCL.TEST_SET_ID, TCL.TEST_CASE_ID, TCL.TEST_SET_TEST_CASE_ID, ISNULL(TRN.EXECUTION_STATUS_ID,3) AS EXECUTION_STATUS_ID
			FROM VW_TESTSET_TESTCASE_LIST TCL LEFT JOIN 
				(SELECT RUN1.TEST_SET_ID, RUN1.TEST_CASE_ID, RUN1.TEST_SET_TEST_CASE_ID, MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID
				FROM TST_TEST_RUN RUN1 INNER JOIN
					(SELECT TEST_CASE_ID, TEST_SET_ID, TEST_SET_TEST_CASE_ID, MAX(END_DATE) AS END_DATE
					FROM TST_TEST_RUN
					WHERE TEST_SET_ID IS NOT NULL AND EXECUTION_STATUS_ID <> 3 AND EXECUTION_STATUS_ID <> 4
					AND RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
					GROUP BY TEST_CASE_ID, TEST_SET_ID, TEST_SET_TEST_CASE_ID) RUN2
				ON RUN1.TEST_CASE_ID = RUN2.TEST_CASE_ID AND RUN1.END_DATE = RUN2.END_DATE
				WHERE RUN1.TEST_SET_ID IS NOT NULL AND RUN1.EXECUTION_STATUS_ID <> 3 AND RUN1.EXECUTION_STATUS_ID <> 4
				AND RUN1.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList)
				GROUP BY RUN1.TEST_CASE_ID, RUN1.TEST_SET_ID, RUN1.TEST_SET_TEST_CASE_ID) TRN
				ON TCL.TEST_SET_ID = TRN.TEST_SET_ID AND TCL.TEST_CASE_ID = TRN.TEST_CASE_ID
				AND TCL.TEST_SET_TEST_CASE_ID = TRN.TEST_SET_TEST_CASE_ID
			WHERE PROJECT_ID = @ProjectId AND TCL.RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList) AND (TCL.IS_TEST_CASE_DELETED = 0 OR @IncludeDeleted = 1) AND (TCL.IS_TEST_SET_DELETED = 0 OR @IncludeDeleted = 1)) TCL2
		ON EXE.EXECUTION_STATUS_ID = TCL2.EXECUTION_STATUS_ID
		GROUP BY EXE.EXECUTION_STATUS_ID
		ORDER BY ExecutionStatusId
	END
END
GO
-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: TestSet
-- Description:		Retrieves the list of test cases with the execution information
-- =====================================================================
IF OBJECT_ID ( 'TESTSET_RETRIEVE_TESTCASES', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TESTSET_RETRIEVE_TESTCASES];
GO
CREATE PROCEDURE [TESTSET_RETRIEVE_TESTCASES]
	@ProjectId INT,
	@TestSetId INT,
	@ReleaseId INT,
	@IncludeDeleted BIT
AS
BEGIN
	IF @ReleaseId IS NULL
	BEGIN
		SELECT TSC.*,
		ISNULL(TRN.EXECUTION_STATUS_ID, 3 /* Not Run */) AS EXECUTION_STATUS_ID,
		ISNULL(EXE.NAME, 'Not Run') AS EXECUTION_STATUS_NAME,
		TRN.EXECUTION_DATE,
		TRN.ACTUAL_DURATION
		FROM [VW_TESTSET_TESTCASE_LIST_INTERNAL] TSC
		LEFT JOIN
			(SELECT RUN1.TEST_SET_TEST_CASE_ID, MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID, MIN(RUN1.END_DATE) AS EXECUTION_DATE, MIN(RUN1.ACTUAL_DURATION) AS ACTUAL_DURATION FROM TST_TEST_RUN RUN1
				INNER JOIN (SELECT TEST_SET_TEST_CASE_ID, MAX(END_DATE) AS END_DATE FROM TST_TEST_RUN WHERE TEST_SET_ID = @TestSetId GROUP BY TEST_SET_TEST_CASE_ID) RUN2
				ON RUN1.TEST_SET_TEST_CASE_ID = RUN2.TEST_SET_TEST_CASE_ID AND RUN1.END_DATE = RUN2.END_DATE
				WHERE RUN1.TEST_SET_ID = @TestSetId GROUP BY RUN1.TEST_SET_TEST_CASE_ID) TRN
			ON TSC.TEST_SET_TEST_CASE_ID = TRN.TEST_SET_TEST_CASE_ID
		LEFT JOIN TST_EXECUTION_STATUS EXE ON TRN.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		WHERE	TSC.TEST_SET_ID = @TestSetId
		AND		TSC.PROJECT_ID = @ProjectId
		AND		(TSC.IS_TEST_CASE_DELETED = 0 OR @IncludeDeleted = 1)
		ORDER BY TSC.POSITION, TSC.TEST_SET_TEST_CASE_ID
	END
	ELSE
	BEGIN
		--Declare results set
		DECLARE  @ReleaseList TABLE
		(
			RELEASE_ID INT
		)
		DECLARE @ReturnValue INT
		
		--Populate list of child items that we need to consider
		INSERT @ReleaseList (RELEASE_ID)
		SELECT RELEASE_ID FROM FN_RELEASE_GET_SELF_AND_CHILDREN (@ProjectId, @ReleaseId, 0)
		
		SELECT TSC.*,
		ISNULL(TRN.EXECUTION_STATUS_ID, 3 /* Not Run */) AS EXECUTION_STATUS_ID,
		ISNULL(EXE.NAME, 'Not Run') AS EXECUTION_STATUS_NAME,
		TRN.EXECUTION_DATE,
		TRN.ACTUAL_DURATION
		FROM [VW_TESTSET_TESTCASE_LIST_INTERNAL] TSC
		LEFT JOIN
			(SELECT RUN1.TEST_SET_TEST_CASE_ID, MIN(RUN1.EXECUTION_STATUS_ID) AS EXECUTION_STATUS_ID, MIN(RUN1.END_DATE) AS EXECUTION_DATE, MIN(RUN1.ACTUAL_DURATION) AS ACTUAL_DURATION FROM TST_TEST_RUN RUN1
				INNER JOIN (SELECT TEST_SET_TEST_CASE_ID, MAX(END_DATE) AS END_DATE FROM TST_TEST_RUN WHERE TEST_SET_ID = @TestSetId AND RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList) GROUP BY TEST_SET_TEST_CASE_ID) RUN2
				ON RUN1.TEST_SET_TEST_CASE_ID = RUN2.TEST_SET_TEST_CASE_ID AND RUN1.END_DATE = RUN2.END_DATE
				WHERE RUN1.TEST_SET_ID = @TestSetId AND RELEASE_ID IN (SELECT RELEASE_ID FROM @ReleaseList) GROUP BY RUN1.TEST_SET_TEST_CASE_ID) TRN
			ON TSC.TEST_SET_TEST_CASE_ID = TRN.TEST_SET_TEST_CASE_ID
		LEFT JOIN TST_EXECUTION_STATUS EXE ON TRN.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID
		WHERE	TSC.TEST_SET_ID = @TestSetId
		AND		TSC.PROJECT_ID = @ProjectId
		AND		(TSC.IS_TEST_CASE_DELETED = 0 OR @IncludeDeleted = 1)
		ORDER BY TSC.POSITION, TSC.TEST_SET_TEST_CASE_ID
	END
END
GO
-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: UserManager
-- Description:		Deletes/unassociates all the related information to a demo user
-- =============================================
IF OBJECT_ID ( 'USER_DELETE_RELATED', 'P' ) IS NOT NULL 
    DROP PROCEDURE USER_DELETE_RELATED;
GO
CREATE PROCEDURE USER_DELETE_RELATED
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
    
    --Delete the contacts and instant messages
    DELETE TST_MESSAGE WHERE SENDER_USER_ID = @UserId
    DELETE TST_MESSAGE WHERE RECIPIENT_USER_ID = @UserId
    DELETE TST_USER_CONTACT WHERE CREATOR_USER_ID = @UserId
    DELETE TST_USER_CONTACT WHERE CONTACT_USER_ID = @UserId
    
    --Move any references to the user to the system administrator user
    UPDATE TST_TASK SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TASK SET OWNER_ID = NULL WHERE OWNER_ID = @UserId
    UPDATE TST_INCIDENT SET OPENER_ID = 1 WHERE OPENER_ID = @UserId
    UPDATE TST_INCIDENT SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_INCIDENT_RESOLUTION SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TEST_CASE SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_TEST_CASE SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_TEST_SET SET OWNER_ID = 1 WHERE OWNER_ID = @UserId
    UPDATE TST_TEST_SET SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_REQUIREMENT SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_RELEASE SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId
    UPDATE TST_TEST_RUN SET TESTER_ID = 1 WHERE TESTER_ID = @UserId
    UPDATE TST_TEST_RUN SET TESTER_ID = 1 WHERE TESTER_ID = @UserId
    UPDATE TST_ATTACHMENT SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_ATTACHMENT SET EDITOR_ID = 1 WHERE EDITOR_ID = @UserId
    UPDATE TST_ATTACHMENT_VERSION SET AUTHOR_ID = 1 WHERE AUTHOR_ID = @UserId
    UPDATE TST_HISTORY_CHANGESET SET USER_ID = 1 WHERE USER_ID = @UserId
    UPDATE TST_ARTIFACT_LINK SET CREATOR_ID = 1 WHERE CREATOR_ID = @UserId

    --Delete any user project settings
    DELETE FROM TST_TEST_RUNS_PENDING WHERE TESTER_ID = @UserId
    DELETE FROM TST_PROJECT_COLLECTION_ENTRY WHERE USER_ID = @UserId
    DELETE FROM TST_USER_COLLECTION_ENTRY WHERE USER_ID = @UserId
    DELETE FROM TST_USER_ARTIFACT_FIELD WHERE USER_ID = @UserId
    DELETE FROM TST_USER_CUSTOM_PROPERTY WHERE USER_ID = @UserId
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[VM_Report_ActualResults]', 'P') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_Report_ActualResults]
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2021.03.31
-- Description:	Build a dataset that replaces the Attachment_ID with the Attachment_Version_ID in both the EXPECTED_RESULT and ACTUAL_RESULT
-- =============================================
CREATE PROCEDURE VM_Report_ActualResults
	@TestRunId INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ResutsTable TABLE(
		TEST_RUN_ID INT, 
		TEST_STEP_ID INT, 
		POSITION INT, 
		DESCRIPTION NVARCHAR(MAX), 

		/*** EXPECTED RESULTS ***/
		ER_ATTACHMENT_VERSION_ID NVARCHAR(50), 
		ER_ATTACHMENT_ID NVARCHAR(50),
		EXPECTED_RESULT_Original NVARCHAR(MAX),
		EXPECTED_RESULT NVARCHAR(MAX), 
		/*** ACTUAL RESULTS ***/
		AR_ATTACHMENT_VERSION_ID NVARCHAR(50), 
		AR_ATTACHMENT_ID NVARCHAR(50),
		ACTUAL_RESULT_Original NVARCHAR(MAX),
		ACTUAL_RESULT NVARCHAR(MAX),

		EXECUTION_STATUS_NAME NVARCHAR(50),
		END_DATE DATETIME,
		CUST_14 NVARCHAR(50),
		CUST_02 NVARCHAR(50)
	)
 
	INSERT INTO @ResutsTable 
		SELECT 
			VW_TESTRUNSTEP_LIST.TEST_RUN_ID, 
			VW_TESTRUNSTEP_LIST.TEST_STEP_ID, 
			VW_TESTRUNSTEP_LIST.POSITION, 
			VW_TESTRUNSTEP_LIST.DESCRIPTION, 
			0,0,
			VW_TESTRUNSTEP_LIST.EXPECTED_RESULT, 
			VW_TESTRUNSTEP_LIST.EXPECTED_RESULT, 
			0,0,
			VW_TESTRUNSTEP_LIST.ACTUAL_RESULT, 
			VW_TESTRUNSTEP_LIST.ACTUAL_RESULT, 
			VW_TESTRUNSTEP_LIST.EXECUTION_STATUS_NAME, 
			VW_TESTRUNSTEP_LIST.END_DATE, 
			VW_TESTCASE_LIST.CUST_14, 
			VW_TESTRUN_LIST.CUST_02 
		FROM (dbo.VW_TESTRUNSTEP_LIST 
			INNER JOIN dbo.VW_TESTCASE_LIST 
				ON dbo.VW_TESTRUNSTEP_LIST.TEST_CASE_ID = dbo.VW_TESTCASE_LIST.TEST_CASE_ID) 
			INNER JOIN dbo.VW_TESTRUN_LIST 
				ON dbo.VW_TESTRUNSTEP_LIST.TEST_RUN_ID = dbo.VW_TESTRUN_LIST.TEST_RUN_ID 
		WHERE (VW_TESTRUNSTEP_LIST.TEST_RUN_ID = @TestRunId) 
		ORDER BY VW_TESTRUNSTEP_LIST.TEST_RUN_ID, VW_TESTRUNSTEP_LIST.TEST_STEP_ID

-- EXPECTED RESULTS
	UPDATE @ResutsTable
	SET ER_ATTACHMENT_ID = SUBSTRING(EXPECTED_RESULT, PATINDEX('%Attachment/%', EXPECTED_RESULT)+11, (PATINDEX('%.aspx%', EXPECTED_RESULT)) - (PATINDEX('%Attachment/%', EXPECTED_RESULT)+11))
	WHERE EXPECTED_RESULT LIKE '%img src%'

	UPDATE @ResutsTable
	SET ER_ATTACHMENT_VERSION_ID = moo.ATTACHMENT_VERSION_ID
	FROM @ResutsTable ert INNER JOIN TST_ATTACHMENT_VERSION moo ON ert.ER_ATTACHMENT_ID = moo.ATTACHMENT_ID AND IS_CURRENT = 1

	UPDATE @ResutsTable
	SET EXPECTED_RESULT = REPLACE(EXPECTED_RESULT,ER_ATTACHMENT_ID, ER_ATTACHMENT_VERSION_ID)
	WHERE ER_ATTACHMENT_ID > 0



-- ACTUAL RESULTS
	UPDATE @ResutsTable
	SET AR_ATTACHMENT_ID = SUBSTRING(ACTUAL_RESULT, PATINDEX('%Attachment/%', ACTUAL_RESULT)+11, (PATINDEX('%.aspx%', ACTUAL_RESULT)) - (PATINDEX('%Attachment/%', ACTUAL_RESULT)+11))
	WHERE ACTUAL_RESULT LIKE '%img src%'

	UPDATE @ResutsTable
	SET AR_ATTACHMENT_VERSION_ID = moo.ATTACHMENT_VERSION_ID
	FROM @ResutsTable ert INNER JOIN TST_ATTACHMENT_VERSION moo ON ert.AR_ATTACHMENT_ID = moo.ATTACHMENT_ID AND IS_CURRENT = 1

	UPDATE @ResutsTable
	SET ACTUAL_RESULT = REPLACE(ACTUAL_RESULT,AR_ATTACHMENT_ID, AR_ATTACHMENT_VERSION_ID)
	WHERE AR_ATTACHMENT_ID > 0


	--FINAL RESULTS
	SELECT * FROM @ResutsTable
	ORDER BY POSITION

END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[VM_Report_DeleteTemplate]', 'P') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_Report_DeleteTemplate]
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2022.08.19
-- Description:	Delete all template records
-- =============================================
CREATE PROCEDURE VM_Report_DeleteTemplate
(
@TemplateId int
)
AS
BEGIN
	SET NOCOUNT ON;
	
	DELETE TST_TEMPLATE_DATASOURCE WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE_PARAMETER WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE_OUTTYPE WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE WHERE TemplateId = @TemplateId

END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[VM_Report_ExpectedResults]', 'P') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_Report_ExpectedResults]
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2021.03.30
-- Description:	Build a dataset that replaces the Attachment_ID with the Attachment_Version_ID in the EXPECTED_RESULT
-- =============================================
CREATE PROCEDURE VM_Report_ExpectedResults
	@TestCaseId INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExptectResutsTable TABLE(
		TST_TEST_CASE_TEST_CASE_ID INT, 
		TST_TEST_STEP_TEST_CASE_ID INT, 
		POSITION INT, 
		TEST_STEP_ID INT, 
		DESCRIPTION NVARCHAR(MAX), 
		ATTACHMENT_VERSION_ID NVARCHAR(50), 
		ATTACHMENT_ID NVARCHAR(50),
		EXPECTED_RESULT_Original NVARCHAR(MAX),
		EXPECTED_RESULT NVARCHAR(MAX))
 
	INSERT INTO @ExptectResutsTable 
		SELECT DISTINCT dbo.TST_TEST_CASE.TEST_CASE_ID as TST_TEST_CASE_TEST_CASE_ID, 
		   dbo.TST_TEST_STEP.TEST_CASE_ID as TST_TEST_STEP_TEST_CASE_ID, 
		   dbo.TST_TEST_STEP.POSITION as POSITION, 
		   dbo.TST_TEST_STEP.TEST_STEP_ID, 
		   dbo.TST_TEST_STEP.DESCRIPTION, 
		   0,0,
		   EXPECTED_RESULT AS EXPECTED_RESULT_Original,
		   EXPECTED_RESULT
		FROM dbo.TST_TEST_CASE 
		INNER JOIN dbo.TST_TEST_STEP ON dbo.TST_TEST_CASE.TEST_CASE_ID = dbo.TST_TEST_STEP.TEST_CASE_ID 
		WHERE(dbo.TST_TEST_STEP.TEST_CASE_ID = @TestCaseId)


-- EXPECTED RESULTS
	UPDATE @ExptectResutsTable
	SET ATTACHMENT_ID = SUBSTRING(EXPECTED_RESULT, PATINDEX('%Attachment/%', EXPECTED_RESULT)+11, (PATINDEX('%.aspx%', EXPECTED_RESULT)) - (PATINDEX('%Attachment/%', EXPECTED_RESULT)+11))
	WHERE EXPECTED_RESULT LIKE '%img src%'

	UPDATE @ExptectResutsTable
	SET ATTACHMENT_VERSION_ID = moo.ATTACHMENT_VERSION_ID
	FROM @ExptectResutsTable ert INNER JOIN TST_ATTACHMENT_VERSION moo ON ert.ATTACHMENT_ID = moo.ATTACHMENT_ID AND IS_CURRENT = 1

	UPDATE @ExptectResutsTable
	SET EXPECTED_RESULT = REPLACE(EXPECTED_RESULT,ATTACHMENT_ID, ATTACHMENT_VERSION_ID)
	WHERE ATTACHMENT_ID > 0

	SELECT * FROM @ExptectResutsTable
	ORDER BY POSITION
END
GO
IF OBJECT_ID('VM_TEST_CASE_PREPARATION_STATUS_PIVOT') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_TEST_CASE_PREPARATION_STATUS_PIVOT]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2023.03.04
-- Description:	Build a dataset that dynamically collects data
-- Execution Example: EXEC VM_TEST_CASE_PREPARATION_STATUS_PIVOT 1
-- =============================================
CREATE PROCEDURE [dbo].[VM_TEST_CASE_PREPARATION_STATUS_PIVOT]
	@ProjectId INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	--build table variable to hold the names
	DECLARE @Names TABLE (Name VARCHAR(200))

	--Get unique instances of each TestCaseType
	INSERT INTO @Names
	SELECT DISTINCT NAME 
	FROM TST_TEST_CASE_TYPE 
	WHERE IS_ACTIVE = 1 AND PROJECT_TEMPLATE_ID = @ProjectId
	ORDER BY 1
	   
	--FORMAT THE COLUMNS FOR USE IN THE PIVOT TABLE
	DECLARE @cols varchar(200)
	SET @cols = NULL
	SELECT  @cols = COALESCE(@cols + '],[','') + [NAME] 
	FROM @Names
	SET @cols = '[' + @cols + ']'


	--BUILD THE PIVOT TABLE SQL
	DECLARE @sql VARCHAR(MAX)
	SET @sql = 'SELECT  [TestPreparationStatusKey], ' + @cols + ' FROM 
	(
	select tcps.[NAME] AS  [TestPreparationStatusKey], tc.TEST_CASE_PREPARATION_STATUS_ID, tct.[NAME] AS [TestCaseTypeName]
	from TST_TEST_CASE tc
	inner join TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = ' + CONVERT(VARCHAR(16),@ProjectId) + '
	inner join TST_TEST_CASE_TYPE tct ON tc.TEST_CASE_TYPE_ID = tct.TEST_CASE_TYPE_ID AND tct.IS_ACTIVE = 1
	) x
	pivot 
	(
	COUNT(TEST_CASE_PREPARATION_STATUS_ID)
	for [TestCaseTypeName] in (' + @cols + ')
	) p'

	EXEC (@sql)

END
GO


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

GRANT EXECUTE TO [IIS APPPOOL\DefaultAppPool]
GO
--GRANT EXECUTE TO [Master]
--GO
