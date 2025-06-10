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
