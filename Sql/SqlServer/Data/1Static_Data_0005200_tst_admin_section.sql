/***************************************************************
**	Insert script for table TST_ADMIN_SECTION
***************************************************************/

SET IDENTITY_INSERT TST_ADMIN_SECTION ON 

INSERT TST_ADMIN_SECTION (ADMIN_SECTION_ID, NAME, DESCRIPTION, PARENT_ID, IS_ACTIVE) 
VALUES (1, N'Workspaces', N'Workspaces', NULL, 1)
,(2, N'View / Edit Projects', N'View / Edit Projects', 1, 1)
,(3, N'View / Edit Programs', N'View / Edit Programs', 1, 1)
,(4, N'View / Edit Portfolios', N'View / Edit Portfolios', 1, 1)
,(5, N'View / Edit Templates', N'View / Edit Templates', 1, 1)
,(6, N'Users', N'Users', NULL, 1)
,(7, N'View / Edit Users', N'View / Edit Users', 6, 1)
,(8, N'View / Edit Project Roles', N'View / Edit Project Roles', 6, 1)
,(9, N'Active Sessions', N'Active Sessions', 6, 1)
,(10, N'Pending Requests', N'Pending Requests', 6, 1)
,(11, N'LDAP Configuration', N'LDAP Configuration', 6, 1)
,(12, N'Manage Login Providers', N'Manage Login Providers', 6, 1)
,(14, N'System', N'System', NULL, 1)
,(15, N'General Settings', N'General Settings', 14, 1)
,(16, N'Event Log', N'Event Log', 14, 1)
,(18, N'Security Settings', N'Security Settings', 14, 1)
,(19, N'Email Configuration', N'Email Configuration', 14, 1)
,(20, N'File Type Icons', N'File Type Icons', 14, 1)
,(21, N'License Details', N'License Details', 14, 1)
,(22, N'System Information', N'System Information', 14, 1)
,(23, N'Integration', N'Integration', NULL, 1)
,(24, N'Source Code', N'Source Code', 23, 1)
,(25, N'Data Synchronization', N'Data Synchronization', 23, 1)
,(26, N'Test Automation', N'Test Automation', 23, 1)
,(27, N'Web Services', N'Web Services', 23, 1)
,(28, N'Reporting', N'Reporting', NULL, 1)
,(29, N'Edit Reports', N'Edit Reports', 28, 1)
,(30, N'Edit Graphs', N'Edit Graphs', 28, 1)
,(31, N'Audit Trail', N'Audit Trail', NULL, 1)
,(32, N'AuditTrail', N'AuditTrail', 31, 1)
,(33, N'AuditTrail Settings', N'AuditTrail Settings', 31, 0)
SET IDENTITY_INSERT TST_ADMIN_SECTION OFF