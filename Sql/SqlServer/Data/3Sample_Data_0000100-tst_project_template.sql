/***************************************************************
**	Insert script for table TST_PROJECT_TEMPLATE
***************************************************************/
SET IDENTITY_INSERT TST_PROJECT_TEMPLATE ON; 

INSERT INTO TST_PROJECT_TEMPLATE
(
PROJECT_TEMPLATE_ID, NAME, DESCRIPTION, IS_ACTIVE
)
VALUES
(
1, 'Library Information System (Sample)', '<p>This template is designed to work with the&nbsp;sample product Library Information System. The template showcases a number of different parts of the system, through that product.&nbsp;</p><p>It is not designed to be used for real life products.</p>', 1
),
(
2, 'Default', '<p>This basic default template matches the one the system automatically generates when you create a completely new template. It is a good basis for customizing your template if no other template fit your needs</p>', 1
),
(
3, 'Regulated Industries', '<p>This template is designed specifically for products that are developed in a regulated environment. For example life sciences. The workflows have been configured to help you meet requirements in your work, such as those arising from FDA 21 CFR Part 11. Workflows include the use of electronic signatures for key stages of sign-off; limit who can transition an artifact between statuses, and manages which fields are disabled or required at each workflow step.</p>', 1
),
(
4, 'Flexible', '<p>This template is designed to alllow users to be as unconstrained from workflow requirements as possible. All relevant fields are available and editable (and not required) at all times. Active statuses are streamlined. This template should be used only for times when process controls are not required or are very lightweight.</p>', 1
)
GO

SET IDENTITY_INSERT TST_PROJECT_TEMPLATE OFF; 

