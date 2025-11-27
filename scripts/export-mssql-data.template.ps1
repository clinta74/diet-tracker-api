# PowerShell Script: Export SQL Server Data to CSV
# Alternative to T-SQL script - uses PowerShell with SQL client for better compatibility

param(
    [Parameter(Mandatory=$false)]
    [string]$ServerName = "your-sql-server-name",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "DietTracker",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "your-sql-username",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "your-sql-password",
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

# Create output directory if it doesn't exist
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

Write-Host "Exporting data from $DatabaseName on $ServerName to $OutputPath" -ForegroundColor Green

# Build connection string
if ($Username -and $Password) {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
} else {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"
}

# Function to export table to CSV
function Export-TableToCSV {
    param(
        [string]$TableName,
        [string]$Query,
        [string]$FilePath
    )
    
    Write-Host "Exporting $TableName..." -ForegroundColor Yellow
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        
        $dataset.Tables[0] | Export-Csv -Path $FilePath -NoTypeInformation -Encoding UTF8
        
        $connection.Close()
        
        $rowCount = $dataset.Tables[0].Rows.Count
        Write-Host "  ✓ Exported $rowCount rows to $FilePath" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error exporting $TableName : $_" -ForegroundColor Red
    }
}

# Export all tables
$tables = @(
    @{Name="User"; Query="SELECT UserId, Autosave, Created FROM [User]"},
    @{Name="Plan"; Query="SELECT PlanId, Name, MealCount, FuelingCount FROM [Plan]"},
    @{Name="Fueling"; Query="SELECT FuelingId, Name FROM Fueling"},
    @{Name="UserTracking"; Query="SELECT UserTrackingId, UserId, Title, Description, Occurrences, [Order], UseTime, Disabled FROM UserTracking"},
    @{Name="UserTrackingValue"; Query="SELECT UserTrackingValueId, UserTrackingId, Name, Description, [Order], Type, Disabled FROM UserTrackingValue"},
    @{Name="UserTrackingValueMetadata"; Query="SELECT UserTrackingValueId, [Key], Value FROM UserTrackingValueMetadata"},
    @{Name="UserDay"; Query="SELECT UserId, Day, Weight FROM UserDay"},
    @{Name="UserFueling"; Query="SELECT UserFuelingId, UserId, Day, Name, [When] FROM UserFueling"},
    @{Name="UserMeal"; Query="SELECT UserMealId, UserId, Day, Name, [When] FROM UserMeal"},
    @{Name="UserPlan"; Query="SELECT UserId, PlanId, Start FROM UserPlan"},
    @{Name="UserDailyTrackingValue"; Query="SELECT UserId, Day, UserTrackingValueId, Occurrence, Value FROM UserDailyTrackingValue"},
    @{Name="Victory"; Query="SELECT VictoryId, UserId, Name, [When], Type FROM Victory"}
)

foreach ($table in $tables) {
    $filePath = Join-Path $OutputPath "$($table.Name).csv"
    Export-TableToCSV -TableName $table.Name -Query $table.Query -FilePath $filePath
}

Write-Host "`nExport complete! All files saved to: $OutputPath" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Set up your PostgreSQL database" -ForegroundColor White
Write-Host "2. Run migrations: dotnet ef database update" -ForegroundColor White
Write-Host "3. Run import-postgresql-data.ps1 to load the data" -ForegroundColor White
