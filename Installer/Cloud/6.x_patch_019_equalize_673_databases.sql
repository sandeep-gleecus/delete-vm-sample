-- Fix the Notification Subject columns. This was due to a field wrong in the DEZ, and when
--  installer work was done, I not realizing this and referring to some columns over the other
--  so getting confused.
IF (COL_LENGTH('TST_DOCUMENT_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO
IF (COL_LENGTH('TST_RELEASE_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO
IF (COL_LENGTH('TST_REQUIREMENT_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO
IF (COL_LENGTH('TST_RISK_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO
IF (COL_LENGTH('TST_TASK_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO
IF (COL_LENGTH('TST_TEST_CASE_WORKFLOW_TRANSITION','NOTIFY_SUBJECT') < 128)
	ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION]
		ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL
GO

--Foreign Key indexes added through the DEZ that were auto-created and not logged into the
-- Version Tracker sheet.
IF (INDEXPROPERTY(Object_Id('TST_RELEASE'), 'IDX_TST_RELEASE_9_FK', 'IndexID') IS NULL)
	CREATE NONCLUSTERED INDEX [IDX_TST_RELEASE_9_FK]
		ON [dbo].[TST_RELEASE]([BRANCH_ID] ASC);
GO
IF (INDEXPROPERTY(Object_Id('TST_REPORT_CATEGORY'), 'IDX_TST_REPORT_CATEGORY_2_FK', 'IndexID') IS NULL)
	CREATE NONCLUSTERED INDEX [IDX_TST_REPORT_CATEGORY_2_FK]
		ON [dbo].[TST_REPORT_CATEGORY]([WORKSPACE_TYPE_ID] ASC);
GO
IF (INDEXPROPERTY(Object_Id('TST_RISK'), 'IDX_TST_RISK_12_FK', 'IndexID') IS NULL)
	CREATE NONCLUSTERED INDEX [IDX_TST_RISK_12_FK]
		ON [dbo].[TST_RISK]([RISK_DETECTABILITY_ID] ASC);
GO
IF (INDEXPROPERTY(Object_Id('TST_HISTORY_POSITION'), 'IDX_TST_HISTORY_POSITION_2_FK', 'IndexID') IS NULL)
	CREATE NONCLUSTERED INDEX [IDX_TST_HISTORY_POSITION_2_FK]
		ON [dbo].[TST_HISTORY_POSITION]([CHILD_ARTIFACT_TYPE_ID] ASC);
GO

--Changes TIMECARD fields that was accidently marked NOT NULL, and drops uneeded constraint.
ALTER TABLE [TST_TIMECARD]
	ALTER COLUMN [APPROVER_COMMENTS] NVARCHAR(MAX) NULL
GO
ALTER TABLE [TST_TIMECARD_ENTRY_TYPE]
	ALTER COLUMN [DESCRIPTION] NVARCHAR(MAX) NULL
GO
ALTER TABLE TST_TIMECARD_ENTRY
	ALTER COLUMN [DESCRIPTION] NVARCHAR(MAX) NULL
GO
IF (OBJECT_ID('DEF_TST_TIMECARD_APPROVER_COMMENTS', 'D') IS NOT NULL)
	ALTER TABLE [TST_TIMECARD]
		DROP CONSTRAINT [DEF_TST_TIMECARD_APPROVER_COMMENTS]
GO
IF (OBJECT_ID('DEF_TST_TIMECARD_ENTRY_TYPE_DESCRIPTION', 'D') IS NOT NULL)
	ALTER TABLE [TST_TIMECARD_ENTRY_TYPE]
		DROP CONSTRAINT [DEF_TST_TIMECARD_ENTRY_TYPE_DESCRIPTION]
GO
IF (OBJECT_ID('DEF_TST_TIMECARD_ENTRY_DESCRIPTION', 'D') IS NOT NULL)
	ALTER TABLE [TST_TIMECARD_ENTRY]
		DROP CONSTRAINT [DEF_TST_TIMECARD_ENTRY_DESCRIPTION]
GO

--Drop UNNAMED default constraints. Most of these are added in next step.
declare @p019_schema_name nvarchar(256)
declare @p019_table_name nvarchar(256)
declare @p019_col_name nvarchar(256)
declare @p019_command  nvarchar(1000)
set @p019_schema_name = N'dbo'
set @p019_col_name = N'REQUIREMENT_COUNT'
-- TST_PROJECT.REQUIREMENT_COUNT
set @p019_table_name = N'TST_PROJECT'
select @p019_command = 'ALTER TABLE ' + @p019_schema_name + '.[' + @p019_table_name + '] DROP CONSTRAINT ' + d.name
 from sys.tables t
  join sys.default_constraints d on d.parent_object_id = t.object_id
  join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
 where t.name = @p019_table_name
  and t.schema_id = schema_id(@p019_schema_name)
  and c.name = @p019_col_name
if (LEN(@p019_command) > 10)
	execute (@p019_command);
ELSE
	print N'No constraint found for [' + @p019_table_name + '].[' + @p019_col_name + ']';

-- TST_RELEASE.REQUIREMENT_COUNT
set @p019_table_name = N'TST_RELEASE'
select @p019_command = 'ALTER TABLE ' + @p019_schema_name + '.[' + @p019_table_name + '] DROP CONSTRAINT ' + d.name
 from sys.tables t
  join sys.default_constraints d on d.parent_object_id = t.object_id
  join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
 where t.name = @p019_table_name
  and t.schema_id = schema_id(@p019_schema_name)
  and c.name = @p019_col_name
if (LEN(@p019_command) > 10)
	execute (@p019_command);
ELSE
	print N'No constraint found for [' + @p019_table_name + '].[' + @p019_col_name + ']';

-- TST_PROJECT_GROUP.REQUIREMENT_COUNT
set @p019_table_name = N'TST_PROJECT_GROUP'
select @p019_command = 'ALTER TABLE ' + @p019_schema_name + '.[' + @p019_table_name + '] DROP CONSTRAINT ' + d.name
 from sys.tables t
  join sys.default_constraints d on d.parent_object_id = t.object_id
  join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
 where t.name = @p019_table_name
  and t.schema_id = schema_id(@p019_schema_name)
  and c.name = @p019_col_name
if (LEN(@p019_command) > 10)
	execute (@p019_command);
ELSE
	print N'No constraint found for [' + @p019_table_name + '].[' + @p019_col_name + ']';

-- TST_PORTFOLIO.REQUIREMENT_COUNT
set @p019_table_name = N'TST_PORTFOLIO'
select @p019_command = 'ALTER TABLE ' + @p019_schema_name + '.[' + @p019_table_name + '] DROP CONSTRAINT ' + d.name
 from sys.tables t
  join sys.default_constraints d on d.parent_object_id = t.object_id
  join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
 where t.name = @p019_table_name
  and t.schema_id = schema_id(@p019_schema_name)
  and c.name = @p019_col_name
if (LEN(@p019_command) > 10)
	execute (@p019_command);
ELSE
	print N'No constraint found for [' + @p019_table_name + '].[' + @p019_col_name + ']';

--Add new named Default Constraints
IF (OBJECT_ID('DEF_TST_PORTFOLIO_REQUIREMENT_COUNT', 'D') IS NULL)
	ALTER TABLE [TST_PORTFOLIO]
		ADD CONSTRAINT [DEF_TST_PORTFOLIO_REQUIREMENT_COUNT] DEFAULT ((0)) FOR [REQUIREMENT_COUNT]
GO
IF (OBJECT_ID('DEF_TST_PROJECT_GROUP_REQUIREMENT_COUNT', 'D') IS NULL)
	ALTER TABLE [dbo].[TST_PROJECT_GROUP]
		ADD CONSTRAINT [DEF_TST_PROJECT_GROUP_REQUIREMENT_COUNT] DEFAULT ((0)) FOR [REQUIREMENT_COUNT]
GO
IF (OBJECT_ID('DEF_TST_RELEASE_REQUIREMENT_COUNT', 'D') IS NULL)
	ALTER TABLE [dbo].[TST_RELEASE]
		ADD CONSTRAINT [DEF_TST_RELEASE_REQUIREMENT_COUNT] DEFAULT ((0)) FOR [REQUIREMENT_COUNT];
GO
