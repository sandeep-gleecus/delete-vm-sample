/***************************************************************
**	Insert script for table TST_GLOBAL_SETTING
***************************************************************/
INSERT INTO TST_GLOBAL_SETTING
(
NAME, VALUE
)
VALUES
(
'Ldap_Host', 'myldapserver'
),
(
'Ldap_BaseDn', 'CN=Users,OU=Headquarters,DC=MyCompany,DC=Com'
),
(
'Ldap_BindDn', 'CN=sysadmin,CN=Users,OU=Headquarters,DC=MyCompany,DC=Com'
),
(
'Ldap_BindPassword', 'sysadmin'
),
(
'Ldap_Login', 'uid'
),
(
'Ldap_FirstName', 'givenName'
),
(
'Ldap_LastName', 'sn'
),
(
'Ldap_MiddleInitial', 'initials'
),
(
'Ldap_EmailAddress', 'mail'
),
(
'Database_SampleDataCanBeDeleted', 'True'
)
GO

