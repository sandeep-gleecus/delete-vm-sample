UPDATE tst_user SET email_address = 'webmaster@inflectra.com', password = '7809D37D14A276DBAA61C910DD9AC068' WHERE user_id = 1
GO
UPDATE tst_user SET active_yn = 'N' WHERE user_id = 2
GO
UPDATE tst_user SET active_yn = 'N' WHERE user_id = 3
GO
UPDATE tst_user SET active_yn = 'N' WHERE user_id = 4
GO
DELETE FROM TST_PROJECT_ROLE_PERMISSION WHERE PERMISSION_ID = 3 AND PROJECT_ROLE_ID = 2
GO
UPDATE TST_GLOBAL_SETTING SET VALUE = 'D:\Sites\Attachments\SpiraDemo1' WHERE NAME = 'General_AttachmentFolder'
GO
UPDATE TST_GLOBAL_SETTING SET VALUE = 'False' WHERE NAME = 'EmailSettings_Enabled'
GO
INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('EmailSettings_MailServer', 'mail.inflectra.com')
GO
INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('EmailSettings_MailServerPort', '25')
GO
INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('EmailSettings_SmtpUser', 'webmaster@inflectra.com')
GO
INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('EmailSettings_SmtpPassword', 'Mox235')
GO
INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('General_WebServerUrl', 'http://www.inflectra.com/SpiraDemo')
GO
UPDATE TST_GLOBAL_SETTING SET VALUE = 'webmaster@inflectra.com' WHERE NAME = 'EmailSettings_EMailFrom'
GO
UPDATE TST_GLOBAL_SETTING SET VALUE = 'False' WHERE NAME = 'EmailSettings_AllowUserControl'
GO