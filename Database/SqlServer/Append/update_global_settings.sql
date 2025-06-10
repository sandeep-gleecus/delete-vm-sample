UPDATE TST_GLOBAL_SETTING
SET VALUE = 'RRt+1579wQ1MUpsqa7ZPMd6wdjUHs/WCN+jk9exl3gLW5QhtlW0sm8U7eTKXE/zA9DHwXrncUAblI5ohVlcxDg=='
WHERE NAME = 'License_LicenseKey'
GO
UPDATE TST_GLOBAL_SETTING
SET VALUE = 'SpiraPlan'
WHERE NAME = 'License_ProductType'
GO
UPDATE TST_GLOBAL_SETTING
SET VALUE = 'C:\Git\SpiraTeam\SpiraTest\Attachments'
WHERE NAME = 'General_AttachmentFolder'
GO
IF EXISTS (SELECT * FROM TST_GLOBAL_SETTING WHERE NAME='Cache_Folder')
	UPDATE TST_GLOBAL_SETTING
	SET VALUE = 'C:\Git\SpiraTeam\SpiraTest\SpiraCache'
	WHERE NAME = 'Cache_Folder'
ELSE
	INSERT INTO TST_GLOBAL_SETTING
	(
	NAME, VALUE
	)
	VALUES
	(
	'Cache_Folder', 'C:\Git\SpiraTeam\SpiraTest\SpiraCache'
	)
GO
UPDATE TST_PRODUCT_TYPE
SET ACTIVE_YN = 'Y'
WHERE NAME = 'SpiraTest'
GO
UPDATE TST_PRODUCT_TYPE
SET ACTIVE_YN = 'Y'
WHERE NAME = 'SpiraPlan'
GO
UPDATE TST_PRODUCT_TYPE
SET ACTIVE_YN = 'Y'
WHERE NAME = 'SpiraTeam'
GO
INSERT INTO TST_GLOBAL_SETTING
(
NAME, VALUE
)
VALUES
(
'EmailSettings_MailServer', 'gallifrey.corp.inflectra.com'
)
GO
INSERT INTO TST_GLOBAL_SETTING
(
NAME, VALUE
)
VALUES
(
'Api_AllowedCorsOrigins', '*'
)
GO
--Set an RSS Token for the Administrator user so that we can do REST API Testing
UPDATE TST_USER
SET RSS_TOKEN = '{B9050F75-C5E6-4244-8712-FBF20061A976}'
WHERE USER_ID = 1
GO

--Update the mail from address
UPDATE TST_GLOBAL_SETTING
SET VALUE = 'spira@mycompany.com'
WHERE NAME = 'EmailSettings_EMailFrom'
GO


