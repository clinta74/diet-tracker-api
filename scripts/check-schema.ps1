# Quick script to check the actual schema
param(
    [string]$ServerName = "192.168.100.12",
    [string]$DatabaseName = "DietTracker",
    [string]$Username = "diet-tracker-user",
    [string]$Password = "iKgHo1yAEpZ0BDcXgfXB"
)

$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;"

function Get-TableColumns {
    param([string]$TableName)
    
    $query = @"
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '$TableName' 
ORDER BY ORDINAL_POSITION
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    $connection.Close()
    
    return $dataset.Tables[0]
}

Write-Host "`n=== Plan Table ===" -ForegroundColor Cyan
Get-TableColumns "Plan" | Format-Table

Write-Host "`n=== Fueling Table ===" -ForegroundColor Cyan
Get-TableColumns "Fueling" | Format-Table

Write-Host "`n=== UserFueling Table ===" -ForegroundColor Cyan
Get-TableColumns "UserFueling" | Format-Table

Write-Host "`n=== UserMeal Table ===" -ForegroundColor Cyan
Get-TableColumns "UserMeal" | Format-Table
