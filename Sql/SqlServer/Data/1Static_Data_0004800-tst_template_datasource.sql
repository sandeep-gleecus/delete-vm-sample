/***************************************************************
**	Insert script for table TST_TEMPLATE_DATASOURCE
***************************************************************/
SET IDENTITY_INSERT TST_TEMPLATE_DATASOURCE ON; 

INSERT INTO TST_TEMPLATE_DATASOURCE
(
TEMPLATEDATASOURCEID, TEMPLATEID, NAME, TYPE, PROVIDERCLASS, CONNECTIONSTRING
)
VALUES
(
3090, 3098, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
),
(
3091, 3099, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
),
(
3092, 3100, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
),
(
3093, 3101, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
),
(
3094, 3102, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
),
(
3095, 3103, 'VMASTER', NULL, 'System.Data.SqlClient', 'Data Source=VMASTER;Initial Catalog=ValidationMaster;Integrated Security=True'
)
GO

SET IDENTITY_INSERT TST_TEMPLATE_DATASOURCE OFF; 

