-- SQL Server Data Export Script
-- Exports all tables to CSV files for PostgreSQL import
-- Run this script against your MS SQL Server database

-- Set output directory (modify as needed)
DECLARE @OutputPath NVARCHAR(500) = 'C:\temp\diet-tracker-export\';

-- Ensure output directory exists before running this script

-- Export User table
DECLARE @sql NVARCHAR(MAX);
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserId, Autosave, Created FROM [' + DB_NAME() + '].dbo.[User]" queryout "' + @OutputPath + 'User.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export Plan table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT PlanId, Name, MealCount, FuelingCount, CondimentCount FROM [' + DB_NAME() + '].dbo.Plan" queryout "' + @OutputPath + 'Plan.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export Fueling table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT FuelingId, Name, Disabled FROM [' + DB_NAME() + '].dbo.Fueling" queryout "' + @OutputPath + 'Fueling.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserTracking table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserTrackingId, UserId, Title, Description, Occurrences, [Order], UseTime, Disabled FROM [' + DB_NAME() + '].dbo.UserTracking" queryout "' + @OutputPath + 'UserTracking.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserTrackingValue table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserTrackingValueId, UserTrackingId, Name, Description, [Order], Type, Disabled FROM [' + DB_NAME() + '].dbo.UserTrackingValue" queryout "' + @OutputPath + 'UserTrackingValue.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserTrackingValueMetadata table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserTrackingValueId, [Key], Value FROM [' + DB_NAME() + '].dbo.UserTrackingValueMetadata" queryout "' + @OutputPath + 'UserTrackingValueMetadata.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserDay table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserId, Day, Weight FROM [' + DB_NAME() + '].dbo.UserDay" queryout "' + @OutputPath + 'UserDay.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserFueling table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserFuelingId, UserId, FuelingId, Day, [When] FROM [' + DB_NAME() + '].dbo.UserFueling" queryout "' + @OutputPath + 'UserFueling.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserMeal table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserMealId, UserId, Day, Type, [When] FROM [' + DB_NAME() + '].dbo.UserMeal" queryout "' + @OutputPath + 'UserMeal.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserPlan table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserId, PlanId, Start FROM [' + DB_NAME() + '].dbo.UserPlan" queryout "' + @OutputPath + 'UserPlan.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export UserDailyTrackingValue table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT UserId, Day, UserTrackingValueId, Occurrence, Value FROM [' + DB_NAME() + '].dbo.UserDailyTrackingValue" queryout "' + @OutputPath + 'UserDailyTrackingValue.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

-- Export Victory table
SET @sql = '
EXEC xp_cmdshell ''bcp "SELECT VictoryId, UserId, Name, [When], Type FROM [' + DB_NAME() + '].dbo.Victory" queryout "' + @OutputPath + 'Victory.csv" -c -t"," -r"\n" -S ' + @@SERVERNAME + ' -T''
';
EXEC sp_executesql @sql;

PRINT 'Export complete! Files saved to: ' + @OutputPath;
PRINT 'Note: You may need to enable xp_cmdshell:';
PRINT 'EXEC sp_configure ''show advanced options'', 1; RECONFIGURE;';
PRINT 'EXEC sp_configure ''xp_cmdshell'', 1; RECONFIGURE;';
