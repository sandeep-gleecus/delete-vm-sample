select * from TST_GLOBAL_SETTING
where name like 'lice%'

select * from TST_PRODUCT_TYPE

/*
UPDATE TST_PRODUCT_TYPE
SET ACTIVE_YN = 'N'
WHERE PRODUCT_TYPE_ID < 4
*/

SELECT * FROM TST_TEMPLATE WHERE TemplateId = 3105
SELECT * FROM TST_TEMPLATE_DATASOURCE WHERE TemplateId = 3105
SELECT * FROM TST_TEMPLATE_OUTTYPE WHERE TemplateId = 3105
SELECT * FROM TST_TEMPLATE_PARAMETER WHERE TemplateId = 3105

UPDATE TST_TEMPLATE_PARAMETER
SET ParameterLabel = 'TestCaseId'
WHERE ParameterId = 2058

UPDATE TST_TEMPLATE_PARAMETER
SET ParameterLabel = 'projectId'
WHERE ParameterId = 2056

SELECT * FROM TST_SCHEDULES

SELECT t.TemplateName, t.templateid,  tp.ParameterLabel,  * FROM TST_TEMPLATE t INNER JOIN TST_TEMPLATE_PARAMETER tp ON t.TemplateId = tp.TemplateId

http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3103/projectID=1/Word

3103/projectID=1/Word


http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3098/TestCaseID=8/Word -- ERROR evaluateBoolean("=CONTAINS(${varName2.EXPECTED_RESULT},'<img src')")
http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3099/TestCaseID=8/Word -- ERROR evaluateBoolean("=CONTAINS(${varName2.EXPECTED_RESULT},'<img src')")

http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3100/TestRunID=8/Word --WORKED
http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3101/TestRunID=8/Word --WORKED

http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3102/ProjectID=1/Word --WORKED
http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3103/ProjectID=1/Word --FAIL ... MUST be projectId
http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3104/ProjectID=1/Word --WORKED

http://dev.validationmastercloud.com/ValidationMaster/api/ReportApi/GenerateReport/3105/TestCaseID=8/Word
