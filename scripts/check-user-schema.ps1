# Quick script to check User and UserDay schema
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
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
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

Write-Host "`n=== User Table ===" -ForegroundColor Cyan
Get-TableColumns "User" | Format-Table

Write-Host "`n=== UserDay Table ===" -ForegroundColor Cyan
Get-TableColumns "UserDay" | Format-Table

Write-Host "`n=== Victory Table ===" -ForegroundColor Cyan
Get-TableColumns "Victory" | Format-Table
