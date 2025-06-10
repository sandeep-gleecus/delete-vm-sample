/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [PRODUCT_TYPE_ID]
      ,[NAME]
      ,[PRODUCT_LICENSE_NUMBER]
      ,[ACTIVE_YN]
  FROM [dbo].[TST_PRODUCT_TYPE]