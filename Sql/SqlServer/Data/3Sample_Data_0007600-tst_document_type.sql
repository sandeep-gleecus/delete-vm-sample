/***************************************************************
**	Insert script for table TST_DOCUMENT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_DOCUMENT_TYPE ON; 

INSERT INTO TST_DOCUMENT_TYPE
(
DOCUMENT_TYPE_ID, PROJECT_TEMPLATE_ID, DOCUMENT_WORKFLOW_ID, NAME, DESCRIPTION, IS_ACTIVE, IS_DEFAULT
)
VALUES
(
1, 1, 1, 'Default', 'The default document type for a project', 1, 1
),
(
2, 1, 1, 'Functional Specification', 'Functional specification for the system. Can be performance or feature related', 1, 0
),
(
3, 1, 1, 'Screen Shot', NULL, 1, 0
),
(
4, 1, 1, 'Stack Trace', NULL, 1, 0
),
(
5, 1, 1, 'UML Diagram', 'UML documents such as sequence diagram, state diagram, use-case diagram, class diagram, etc.', 1, 0
),
(
6, 1, 1, 'Screen Layout', NULL, 1, 0
),
(
7, 2, 2, 'Default', '', 1, 1
),
(
8, 3, 3, 'Default', NULL, 1, 1
),
(
9, 4, 4, 'Default', NULL, 1, 1
)
GO

SET IDENTITY_INSERT TST_DOCUMENT_TYPE OFF; 

