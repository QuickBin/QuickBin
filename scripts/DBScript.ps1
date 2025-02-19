param(
    [switch]$displayHelp,
    [switch]$runMigration,
    [switch]$recreateDatabase,
    [switch]$startDatabase,
    [switch]$stopDatabase
)

function Display-UsageInstructions
{
    Write-Host ""
    Write-Host "Usage [Windows]  : .\DatabaseScript.ps1 [-parameters]"
    Write-Host "Usage [Linux] : pwsh DatabaseScript.ps1 [-parameters]"
    Write-Host ""
    Write-Host "Powersehll v7+ is required to execute this script."
    Write-Host "https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -runMigration               Execute the migration script."
    Write-Host "  -recreateDatabase           Recreate the database."
    Write-Host "  -displayHelp                Display this help message."
    Write-Host "  -startDatabase              Start Database Containers."
    Write-Host "  -stopDatabase               Stop Database Containers."
    Write-Host ""
}

function Notification
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if ($IsLinux)
    {
        notify-send "DB-Script" $Message
    }
}


if (($PSBoundParameters.Count -eq 0) -or $displayHelp)
{
    Display-UsageInstructions
    exit
}


if ($startDatabase)
{
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    cd $scriptDir

    if ($IsWindows)
    {
        $dockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
        if (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)
        {
            Write-Host "Docker Desktop is already running."
        }
        else
        {
            Start-Process $dockerPath
            Start-Sleep -Seconds 10
        }
    }

    cd "..\database"
    docker compose pull
    docker compose up -d --timeout 60

    Notification "Database Started"
}


if ($stopDatabase)
{
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    cd $scriptDir

    cd "..\database"
    docker compose down --timeout 60

    Notification "Database Stopped"
}

if ($recreateDatabase)
{
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    cd $scriptDir

    $directoryPath = ".."
    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host "              START DELETING DATABASE"
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host ""

    cd $directoryPath
    docker compose down --timeout 60

    if (Test-Path -Path "data\db" -PathType Container)
    {
        Remove-Item -Path "data\db" -Recurse -Force
    }

    Start-Sleep -Seconds 1
    docker compose pull
    docker compose up -d --timeout 60

    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host "               DATABASE DELETED"
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host ""

    Notification "Database Recreated"
}

if ($runMigration)
{
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    cd $scriptDir

    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host "               CREATING MIGRATION"
    Write-Host "==================================================" -ForegroundColor Blue
    Write-Host ""

    $MigrationName = Read-Host "Please enter the Migration name"

    cd -Path "..\QuickBon.Infrastructure"

    dotnet ef migrations add $MigrationName -s ../QuickBon.Api/

    git add Migrations/

    Notification "Migrations created"
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Blue
Write-Host "                  COMPLETED"
Write-Host "==================================================" -ForegroundColor Blue
Write-Host ""
