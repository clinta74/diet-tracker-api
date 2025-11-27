# PowerShell Script: Import CSV Data into PostgreSQL
# Imports data exported from SQL Server into PostgreSQL database

param(
    [Parameter(Mandatory=$true)]
    [string]$Host,
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 5432,
    
    [Parameter(Mandatory=$true)]
    [string]$Database,
    
    [Parameter(Mandatory=$true)]
    [string]$Username,
    
    [Parameter(Mandatory=$true)]
    [string]$Password,
    
    [Parameter(Mandatory=$true)]
    [string]$InputPath
)

Write-Host "Importing data into $Database on $Host from $InputPath" -ForegroundColor Green

# Set PostgreSQL password environment variable
$env:PGPASSWORD = $Password

# Function to import CSV into PostgreSQL table
function Import-CSVToPostgreSQL {
    param(
        [string]$TableName,
        [string]$FilePath,
        [string[]]$Columns,
        [bool]$HasIdentity = $false
    )
    
    if (!(Test-Path $FilePath)) {
        Write-Host "  ⚠ File not found: $FilePath - Skipping" -ForegroundColor Yellow
        return
    }
    
    Write-Host "Importing $TableName..." -ForegroundColor Yellow
    
    try {
        # Read CSV and skip header
        $csvContent = Get-Content $FilePath | Select-Object -Skip 1
        
        if ($csvContent.Count -eq 0) {
            Write-Host "  ⚠ No data in $FilePath - Skipping" -ForegroundColor Yellow
            return
        }
        
        # Create temporary file with data only (no header)
        $tempFile = [System.IO.Path]::GetTempFileName()
        $csvContent | Set-Content $tempFile -Encoding UTF8
        
        # Build COPY command
        $columnList = $Columns -join ", "
        $copyCommand = "COPY `"$TableName`" ($columnList) FROM STDIN WITH (FORMAT csv, DELIMITER ',', NULL '');"
        
        # Execute COPY using psql
        $copyCommand | & psql -h $Host -p $Port -U $Username -d $Database 2>&1 | Tee-Object -Variable output
        
        Get-Content $tempFile | & psql -h $Host -p $Port -U $Username -d $Database 2>&1 | Out-Null
        
        # Clean up temp file
        Remove-Item $tempFile -Force
        
        # Reset identity sequence if table has identity column
        if ($HasIdentity) {
            $seqResetCommand = "SELECT setval(pg_get_serial_sequence('`"$TableName`"', '$($Columns[0])'), (SELECT MAX($($Columns[0])) FROM `"$TableName`"`));"
            $seqResetCommand | & psql -h $Host -p $Port -U $Username -d $Database 2>&1 | Out-Null
        }
        
        $rowCount = $csvContent.Count
        Write-Host "  ✓ Imported $rowCount rows into $TableName" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error importing $TableName : $_" -ForegroundColor Red
    }
}

# Check if psql is available
try {
    & psql --version | Out-Null
}
catch {
    Write-Host "ERROR: psql command not found. Please install PostgreSQL client tools." -ForegroundColor Red
    Write-Host "Download from: https://www.postgresql.org/download/" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nStarting import process..." -ForegroundColor Cyan

# Import tables in correct order (respecting foreign key constraints)

# 1. Base tables without dependencies
Import-CSVToPostgreSQL -TableName "User" -FilePath (Join-Path $InputPath "User.csv") `
    -Columns @("UserId", "Autosave", "Created") -HasIdentity $false

Import-CSVToPostgreSQL -TableName "Plan" -FilePath (Join-Path $InputPath "Plan.csv") `
    -Columns @("PlanId", "Name", "MealCount", "FuelingCount", "CondimentCount") -HasIdentity $true

Import-CSVToPostgreSQL -TableName "Fueling" -FilePath (Join-Path $InputPath "Fueling.csv") `
    -Columns @("FuelingId", "Name", "Disabled") -HasIdentity $true

Import-CSVToPostgreSQL -TableName "Victory" -FilePath (Join-Path $InputPath "Victory.csv") `
    -Columns @("VictoryId", "UserId", "Name", "When", "Type") -HasIdentity $true

# 2. Tables with User dependency
Import-CSVToPostgreSQL -TableName "UserDay" -FilePath (Join-Path $InputPath "UserDay.csv") `
    -Columns @("UserId", "Day", "Weight") -HasIdentity $false

Import-CSVToPostgreSQL -TableName "UserPlan" -FilePath (Join-Path $InputPath "UserPlan.csv") `
    -Columns @("UserId", "PlanId", "Start") -HasIdentity $false

Import-CSVToPostgreSQL -TableName "UserTracking" -FilePath (Join-Path $InputPath "UserTracking.csv") `
    -Columns @("UserTrackingId", "UserId", "Title", "Description", "Occurrences", "Order", "UseTime", "Disabled") -HasIdentity $true

# 3. Tables with UserTracking dependency
Import-CSVToPostgreSQL -TableName "UserTrackingValue" -FilePath (Join-Path $InputPath "UserTrackingValue.csv") `
    -Columns @("UserTrackingValueId", "UserTrackingId", "Name", "Description", "Order", "Type", "Disabled") -HasIdentity $true

# 4. Tables with UserTrackingValue dependency
Import-CSVToPostgreSQL -TableName "UserTrackingValueMetadata" -FilePath (Join-Path $InputPath "UserTrackingValueMetadata.csv") `
    -Columns @("UserTrackingValueId", "Key", "Value") -HasIdentity $false

Import-CSVToPostgreSQL -TableName "UserDailyTrackingValue" -FilePath (Join-Path $InputPath "UserDailyTrackingValue.csv") `
    -Columns @("UserId", "Day", "UserTrackingValueId", "Occurrence", "Value") -HasIdentity $false

# 5. Tables with Fueling dependency
Import-CSVToPostgreSQL -TableName "UserFueling" -FilePath (Join-Path $InputPath "UserFueling.csv") `
    -Columns @("UserFuelingId", "UserId", "FuelingId", "Day", "When") -HasIdentity $true

Import-CSVToPostgreSQL -TableName "UserMeal" -FilePath (Join-Path $InputPath "UserMeal.csv") `
    -Columns @("UserMealId", "UserId", "Day", "Type", "When") -HasIdentity $true

# Clean up
$env:PGPASSWORD = ""

Write-Host "`n✅ Import complete!" -ForegroundColor Green
Write-Host "`nVerify data counts:" -ForegroundColor Cyan
Write-Host "Run this in psql to check row counts:" -ForegroundColor White
Write-Host @"
SELECT 'User' as table_name, COUNT(*) FROM "User"
UNION ALL SELECT 'Plan', COUNT(*) FROM "Plan"
UNION ALL SELECT 'Fueling', COUNT(*) FROM "Fueling"
UNION ALL SELECT 'Victory', COUNT(*) FROM "Victory"
UNION ALL SELECT 'UserDay', COUNT(*) FROM "UserDay"
UNION ALL SELECT 'UserPlan', COUNT(*) FROM "UserPlan"
UNION ALL SELECT 'UserTracking', COUNT(*) FROM "UserTracking"
UNION ALL SELECT 'UserTrackingValue', COUNT(*) FROM "UserTrackingValue"
UNION ALL SELECT 'UserTrackingValueMetadata', COUNT(*) FROM "UserTrackingValueMetadata"
UNION ALL SELECT 'UserDailyTrackingValue', COUNT(*) FROM "UserDailyTrackingValue"
UNION ALL SELECT 'UserFueling', COUNT(*) FROM "UserFueling"
UNION ALL SELECT 'UserMeal', COUNT(*) FROM "UserMeal";
"@ -ForegroundColor Gray
