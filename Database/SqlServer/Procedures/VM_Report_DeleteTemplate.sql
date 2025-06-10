SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[VM_Report_DeleteTemplate]', 'P') IS NOT NULL 
	DROP PROCEDURE [dbo].[VM_Report_DeleteTemplate]
GO

-- =============================================
-- Author:		Gerald Green
-- Create date: 2022.08.19
-- Description:	Delete all template records
-- =============================================
CREATE PROCEDURE VM_Report_DeleteTemplate
(
@TemplateId int
)
AS
BEGIN
	SET NOCOUNT ON;
	
	DELETE TST_TEMPLATE_DATASOURCE WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE_PARAMETER WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE_OUTTYPE WHERE TemplateId = @TemplateId
	DELETE TST_TEMPLATE WHERE TemplateId = @TemplateId

END
GO
