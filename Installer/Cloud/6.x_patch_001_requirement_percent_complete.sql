-- =====================================================================
-- Author:			Inflectra Corporation
-- Description:		Alters the [TST_REQUIREMENT] table to have
--					make sure the PERCENT_COMPLETE field is nullable
-- =====================================================================
ALTER TABLE [TST_REQUIREMENT]
ALTER COLUMN [PERCENT_COMPLETE] INTEGER;
GO
