-- UpdateFileTypes()
IF ((SELECT COUNT(*) FROM [TST_GLOBAL_FILETYPES] WHERE [FILE_EXTENSION] = 'diagram') = 0)
BEGIN
	INSERT INTO [TST_GLOBAL_FILETYPES] VALUES
		('application/x-diagram','Diagram.svg','diagram','Spira Diagram'),
		('application/x-orgchart','Orgchart.svg','orgchart','Spira Org Chart'),
		('application/x-mindmap','Mindmap.svg','mindmap','Spira Mindmap');
END
GO
