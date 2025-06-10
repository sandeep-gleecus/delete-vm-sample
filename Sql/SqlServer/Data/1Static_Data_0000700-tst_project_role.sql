/***************************************************************
**	Insert script for table TST_PROJECT_ROLE
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_ROLE ON; 

INSERT INTO TST_PROJECT_ROLE
(
PROJECT_ROLE_ID, NAME, DESCRIPTION, IS_ADMIN, IS_ACTIVE, IS_DISCUSSIONS_ADD, IS_SOURCE_CODE_VIEW, IS_SOURCE_CODE_EDIT, IS_DOCUMENT_FOLDERS_EDIT, IS_LIMITED_VIEW, IS_TEMPLATE_ADMIN
)
VALUES
(
1, 'Product Owner', 'Can see all product artifacts. Can create/modify all artifacts. Can access the product/template administration tools', 1, 1, 1, 1, 1, 1, 0, 1
),
(
2, 'Manager', 'Can see all product artifacts. Can create/modify all artifacts', 0, 1, 1, 1, 1, 1, 0, 0
),
(
3, 'Developer', 'Can see all product artifacts (except automation hosts). Can create new and modify your own risks and incidents. Can create and modify all / bulk edit documents, tasks, and tests', 0, 1, 1, 1, 1, 1, 0, 0
),
(
4, 'Tester', 'Can see all product artifacts (except tasks). Can create new and modify your own risks and tests. Can create and modify all / bulk edit documents, incidents, and automation hosts', 0, 1, 1, 1, 1, 1, 0, 0
),
(
5, 'Observer', 'Can see all product artifacts, but cannot perform any write operations (create / modify / delete)', 0, 1, 0, 1, 0, 0, 0, 0
),
(
6, 'Incident User', 'Can only create/modify/view incidents and their attachments that you are the owner of. Cannot see anything else (cannot see requirements, tests, risks, or releases)', 0, 1, 1, 1, 0, 0, 1, 0
)
GO

SET IDENTITY_INSERT TST_PROJECT_ROLE OFF; 

