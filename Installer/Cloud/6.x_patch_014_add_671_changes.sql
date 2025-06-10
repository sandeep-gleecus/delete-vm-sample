-- CorrectHistoryTable()
IF (OBJECT_ID('FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION', 'F') IS NOT NULL)
BEGIN 
	ALTER TABLE [TST_HISTORY_POSITION] 
		DROP CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION] 
END
GO

ALTER TABLE [TST_HISTORY_POSITION] 
	ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION]
	FOREIGN KEY([CHANGESET_ID]) REFERENCES[TST_HISTORY_CHANGESET]([CHANGESET_ID]) ON DELETE CASCADE
GO

IF (EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_TST_HISTORY_POSITION_3_FK'))
BEGIN
	DROP INDEX [IDX_TST_HISTORY_POSITION_3_FK] ON [TST_HISTORY_POSITION]
END
GO


-- UpdateFileTypes()
IF ((SELECT COUNT(*) FROM [TST_GLOBAL_FILETYPES] WHERE [FILE_EXTENSION] = 'ps1xml') = 0)
BEGIN
	INSERT INTO [TST_GLOBAL_FILETYPES] VALUES
		('text/x-ignore','Text.svg','ignore','Ignore File'),
		('text/x-ignore','Text.svg','gitignore','GitIgnore File'),
		('text/x-sql','Text.svg','sql','SQL file'),
		('text/x-csv','Text.svg','csv','CSV file'),
		('text/x-tsv','Text.svg','tsv','TSV file'),
		('text/xml','ASP-Net.svg','ascx','ASCX file'),
		('text/xml','ASP-Net.svg','ashx','ASHX file'),
		('text/xml','ASP-Net.svg','asax','ASAX file'),
		('text/x-editorconfig','Text.svg','editorconfig','Editorconfig file'),
		('text/x-gitattributes','Text.svg','gitattributes','Gitattributes file'),
		('text/xml','Text.svg','build','Build file'),
		('text/x-sln','Text.svg','sln','Solution file'),
		('text/xml','Text.svg','config','Config file'),
		('text/xml','Text.svg','csproj','CSProj file'),
		('text/xml','Text.svg','settings','Settings file'),
		('application/json','JavaScript.svg','map','JS Map file'),
		('text/plain','Cascading-Style-Sheet.svg','scss','SCSS Stylesheet'),
		('text/plain','Cascading-Style-Sheet.svg','sass','SASS Stylesheet'),
		('image/webp','TIFF-Image.svg','webp','WebP Image'),
		('image/ico','TIFF-Image.svg','ico','ICO Image'),
		('text/xml','Text.svg','resx','Resx File'),
		('text/plain','Text.svg','log','Log file'),
		('text/plain','Text.svg','swift','Swift file'),
		('text/x-powershell','Text.svg','ps1','Powershell Script'),
		('text/x-powershell','Text.svg','psm1','Powershell Module'),
		('text/x-powershell','Text.svg','psd1','Powershell Data File'),
		('text/x-powershell','Text.svg','pssc','Powershell Session Configuration'),
		('text/x-powershell','Text.svg','psrc','Powershell Role Compatibility'),
		('text/x-powershell','XML.svg','ps1xml','Powershell XML File'),
		('text/x-powershell','XML.svg','cdxml','Powershell XML File');
END
GO

UPDATE [TST_GLOBAL_FILETYPES]
	SET [FILETYPE_MIME] = 'application/photoshop' 
	WHERE [FILETYPE_MIME] = 'image/photoshop';
GO


-- UpdateProjCollection()
IF ((SELECT COUNT(*) FROM [TST_PROJECT_COLLECTION] WHERE [NAME] = 'PullRequest.Commits.General') = 0)
BEGIN
	INSERT INTO [TST_PROJECT_COLLECTION] VALUES
		('PullRequests.Filters','Y'),
		('PullRequests.General','Y'),
		('BuildDetails.Commits.Filters','Y'),
		('BuildDetails.Commits.General','Y'),
		('SourceCodeFileDetails.Commits.Filters','Y'),
		('SourceCodeFileDetails.Commits.General','Y'),
		('PullRequest.Commits.Filters','Y'),
		('PullRequest.Commits.General','Y');
END
GO


-- UpdateReqTable()
IF EXISTS (
	SELECT * 
		FROM sys.indexes
		WHERE name = 'AK_TST_REQUIREMENT_STEP_POSITION')
BEGIN
    DROP INDEX [TST_REQUIREMENT_STEP].[AK_TST_REQUIREMENT_STEP_POSITION];
END
GO

CREATE NONCLUSTERED INDEX [AK_TST_REQUIREMENT_STEP_POSITION] ON [TST_REQUIREMENT_STEP] ([REQUIREMENT_ID], [POSITION])
GO

-- AdjustVersionControl()
IF (
	(SELECT [CHARACTER_MAXIMUM_LENGTH] 
	  FROM information_schema.columns 
	  WHERE 
		[TABLE_NAME] = 'TST_VERSION_CONTROL_SYSTEM' AND 
		[COLUMN_NAME] = 'LOGIN') 
	< 255)
BEGIN
	ALTER TABLE [TST_VERSION_CONTROL_SYSTEM] ALTER COLUMN [LOGIN] nvarchar(255) NOT NULL
END
GO

IF (
	(SELECT [CHARACTER_MAXIMUM_LENGTH] 
	  FROM information_schema.columns 
	  WHERE 
		[TABLE_NAME] = 'TST_VERSION_CONTROL_SYSTEM' AND 
		[COLUMN_NAME] = 'PASSWORD') 
	< 255)
BEGIN
	ALTER TABLE [TST_VERSION_CONTROL_SYSTEM] ALTER COLUMN [PASSWORD] nvarchar(255) NOT NULL
END
GO

IF (
	(SELECT [CHARACTER_MAXIMUM_LENGTH] 
	  FROM information_schema.columns 
	  WHERE 
		[TABLE_NAME] = 'TST_VERSION_CONTROL_PROJECT' AND 
		[COLUMN_NAME] = 'LOGIN') 
	< 255)
BEGIN
	ALTER TABLE [TST_VERSION_CONTROL_PROJECT] ALTER COLUMN [LOGIN] nvarchar(255) NULL
END
GO

IF (
	(SELECT [CHARACTER_MAXIMUM_LENGTH] 
	  FROM information_schema.columns 
	  WHERE 
		[TABLE_NAME] = 'TST_VERSION_CONTROL_PROJECT' AND 
		[COLUMN_NAME] = 'PASSWORD') 
	< 255)
BEGIN
	ALTER TABLE [TST_VERSION_CONTROL_PROJECT] ALTER COLUMN [PASSWORD] nvarchar(255) NULL
END
GO
