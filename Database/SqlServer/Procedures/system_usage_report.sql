
/****** Object:  StoredProcedure [dbo].[SYSTEM_USAGE_REPORT]    Script Date: 11/30/2023 11:28:07 PM ******/
/*
 --NumberOfActiveAccount
 --Getting active users count based on last_login activity should be exists with in 120 days with active status.

 --ActiveUserPercentage
 -- (No.of Active users / Total No.of users)*100

 --Per Day
 --Average(How many times of user logged in a day)

  --Per Week
 --Average(How many times of user logged in a week)

  --Per Month
 --Average(How many times of user logged in a Month)

 -- Time Per Day
 --Average(How many seconds user logged in a day)
 
 -- Time Per Week
 --Average(How many seconds user logged in a Week)

  -- Time Per Month
 --Average(How many seconds user logged in a Month)
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[SYSTEM_USAGE_REPORT]
	@year varchar(50)
AS
BEGIN



declare @Last120DaysActiveUsers as Table
(
	User_ID int
)


Insert into @Last120DaysActiveUsers
Select distinct U.User_ID
From
TST_User U 
where  U.IS_ACTIVE = 1
and (DATEDIFF(day, Last_Login_date, GETDATE())) <= 120 --Last_Login_date is not null --

;with
cte as (
    select 0 n
    union all select n + 1 from cte where n < 11
),
eachmonth as (
select DATENAME(month,(dateadd(month, n, convert(date, @year+'-01-01')))) Month,(dateadd(month, n, convert(date, @year+'-01-01'))) dt,DAY(EOMONTH(dateadd(month, n, convert(date, @year+'-01-01')))) daycount from cte 
),
GetAllUserTill as(
Select USER_ID, 
case
when (Select count(*) from @Last120DaysActiveUsers where User_ID = TST_USer.User_ID) > 0 then 1
else 0
end
IS_ACTIVE,
case
when (Select count(*) from @Last120DaysActiveUsers where User_ID = TST_USer.User_ID) > 0 then cast(concat(Month(getdate()),'/1/',@year) as date)
else cast(concat(Month(CREATION_DATE),'/1/',@year) as date)
end
 CREATION_DATE
From TST_USer
),
 activeaccount as (

SELECT  DATENAME(month,CREATION_DATE) AS [MonthName],
       COUNT(USER_ID) AS [ActiveCountUsers] ,convert (decimal (18,2), COUNT(USER_ID)) As Count2
FROM GetAllUserTill --[TST_USER]
WHERE IS_ACTIVE = 1 --AND YEAR(CREATION_DATE) = @year
GROUP BY DATENAME(month,CREATION_DATE)
),
totaluser as (select datename(month,CREATION_DATE)Month,convert (decimal (18,2), COUNT(USER_ID)) As Count2 from GetAllUserTill --[TST_USER] 
--where YEAR(CREATION_DATE) = @year
group by YEAR(CREATION_DATE),datename(month,CREATION_DATE)
),
perday as (
--PerDay - Per Month
Select year(data.Date) As YEAR,DATENAME(month,(data.Date)) As MONTH, sum(data.DayCount) AS PerDay from (SELECT  COUNT(*) AS [DayCount]
      , CAST(LOGIN_DATE AS DATE)    AS [Date]
FROM [ValidationMasterAudit].dbo.TST_USER_ACTIVITY_LOG_AUDIT
WHERE LOGOUT_DATE IS NULL and year(LOGIN_DATE) = @year
GROUP BY CAST(LOGIN_DATE AS DATE) ) AS data
 group by year(data.Date),DATENAME(month,(data.Date))),

timepermonthandday as (Select DATENAME(month,(data.Date)) As MONTH, sum(DateDiff) TimePerDay from (SELECT  COUNT(*) AS [DayCount]
      , CAST(LOGOUT_DATE AS DATE)    AS [Date],sum(DATEDIFF(SECOND, LOGIN_DATE, LOGOUT_DATE)) AS DateDiff
FROM [ValidationMasterAudit].dbo.TST_USER_ACTIVITY_LOG_AUDIT
WHERE LOGOUT_DATE IS not NULL and year(LOGOUT_DATE) = @year
GROUP BY CAST(LOGOUT_DATE AS DATE) ) AS data
 group by year(data.Date),DATENAME(month,(data.Date)))

		  select e.Month as MonthName,isnull(sum(a.ActiveCountUsers),0) NumberOfActiveAccount, isnull((sum(a.ActiveCountUsers)*100)/sum(tu.Count2),0) as ActiveUserPercentage,
		  isnull((sum(p.PerDay)) / ((e.daycount)),0) as PerDay,
		  cast(sum(p.PerDay) as float)/cast(4 as float)   PerWeek,p.PerDay PerMonth ,
		   --prday
		   RIGHT('0' + CAST((((tm.TimePerDay )/((e.daycount) ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )/((e.daycount) ))) / 60) % 60 AS VARCHAR),2) as TimePerDay,

--perweek
		   RIGHT('0' + CAST((((tm.TimePerDay )/((4) ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )/((4) ))) / 60) % 60 AS VARCHAR),2)  as TimePerWeek,
--permonth,
		   RIGHT('0' + CAST((((tm.TimePerDay ))) / 3600 AS VARCHAR),2) + ' : ' +
RIGHT('0' + CAST(((((tm.TimePerDay )) / 60)) % 60 AS VARCHAR),2) as TimePerMonth



		  from eachmonth e 
		   
		   left join PerDay p on p.MONTH = e.Month
		  left join  activeaccount a on a.MonthName = e.MONTH
		  left join timepermonthandday tm on tm.MONTH = e.Month
		  left join totaluser tu on tu.Month = e.Month
		  group by e.MONTH,p.YEAR,p.PerDay,e.dt,tm.TimePerDay,e.daycount,tu.Month  order by e.dt
		  
		  
		  
		 
END
