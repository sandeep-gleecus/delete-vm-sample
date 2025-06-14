-- =============================================
-- Author:			Inflectra Corporation
-- Business Object: Template
-- Description:		Changes the task fields in a project from one template to another
-- Remarks:			It maps field by name where possible, otherwise just uses the defaults
-- =============================================
IF OBJECT_ID ( 'TEMPLATE_REMAP_TASK_FIELDS', 'P' ) IS NOT NULL 
    DROP PROCEDURE TEMPLATE_REMAP_TASK_FIELDS;
GO
CREATE PROCEDURE TEMPLATE_REMAP_TASK_FIELDS
	@ProjectId INT,
	@OldTemplateId INT,
	@NewTemplateId INT
AS
BEGIN
	SET NOCOUNT ON;

-- Task Types	
UPDATE INC
	SET INC.TASK_TYPE_ID = MAP.NEW_TYPE_ID
FROM 
	TST_TASK INC INNER JOIN (
	SELECT INC.TASK_ID, T1.TASK_TYPE_ID AS OLD_TYPE_ID, T2.TASK_TYPE_ID AS NEW_TYPE_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_TYPE T1 ON INC.TASK_TYPE_ID = T1.TASK_TYPE_ID
		INNER JOIN TST_TASK_TYPE T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TASK_ID, T1.TASK_TYPE_ID AS OLD_TYPE_ID, (SELECT TOP 1 TASK_TYPE_ID FROM TST_TASK_TYPE WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = @NewTemplateId) AS NEW_TYPE_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_TYPE T1 ON INC.TASK_TYPE_ID = T1.TASK_TYPE_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_TYPE WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TASK_ID = MAP.TASK_ID
WHERE 
	MAP.TASK_ID = INC.TASK_ID

--Task Priority
UPDATE INC
	SET INC.TASK_PRIORITY_ID = MAP.NEW_TASK_PRIORITY_ID
FROM 
	TST_TASK INC INNER JOIN (
	SELECT INC.TASK_ID, T1.TASK_PRIORITY_ID AS OLD_TASK_PRIORITY_ID, T2.TASK_PRIORITY_ID AS NEW_TASK_PRIORITY_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_PRIORITY T1 ON INC.TASK_PRIORITY_ID = T1.TASK_PRIORITY_ID
		INNER JOIN TST_TASK_PRIORITY T2 ON T1.NAME = T2.NAME		
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T2.PROJECT_TEMPLATE_ID = @NewTemplateId
	UNION	
	SELECT INC.TASK_ID, T1.TASK_PRIORITY_ID AS OLD_TASK_PRIORITY_ID, NULL AS NEW_TASK_PRIORITY_ID
	FROM TST_TASK INC
		INNER JOIN TST_TASK_PRIORITY T1 ON INC.TASK_PRIORITY_ID = T1.TASK_PRIORITY_ID
	WHERE INC.PROJECT_ID = @ProjectId
		AND T1.PROJECT_TEMPLATE_ID = @OldTemplateId
		AND T1.NAME NOT IN (SELECT NAME FROM TST_TASK_PRIORITY WHERE PROJECT_TEMPLATE_ID = @NewTemplateId)
	) MAP
	ON INC.TASK_ID = MAP.TASK_ID
WHERE 
	MAP.TASK_ID = INC.TASK_ID		
END
GO
