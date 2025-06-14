IF OBJECT_ID ( 'RPT_CUSTOM_LIST_VALUES', 'V' ) IS NOT NULL 
    DROP VIEW [RPT_CUSTOM_LIST_VALUES];
GO
CREATE VIEW [RPT_CUSTOM_LIST_VALUES]
AS
	SELECT	CPV.*, PRJ.PROJECT_ID, CPL.NAME AS CUSTOM_PROPERTY_LIST_NAME, CPL.IS_ACTIVE AS CUSTOM_PROPERTY_LIST_IS_ACTIVE, PRJ.NAME AS PROJECT_NAME, PRJ.IS_ACTIVE AS PROJECT_IS_ACTIVE
	FROM TST_CUSTOM_PROPERTY_VALUE AS CPV 
		INNER JOIN TST_CUSTOM_PROPERTY_LIST AS CPL ON CPV.CUSTOM_PROPERTY_LIST_ID = CPL.CUSTOM_PROPERTY_LIST_ID
		INNER JOIN TST_PROJECT PRJ ON PRJ.PROJECT_TEMPLATE_ID = CPL.PROJECT_TEMPLATE_ID
GO
