# Packages the application for installation in any Service Fabric Cluster

Import-Module -Name Microsoft.PowerShell.Management
Import-Module -Name Microsoft.PowerShell.Utility

function Package-Service
{
    Param ([string] $project, [string] $application)
    
    Push-Location $project
    $packagePath = "..\" + $application + "\pkg\" + $project + "Pkg\"
    Write-Output "`nPacking $project to $packagePath"

    # Build and publish code to package
    $packageCode = $packagePath + "Code"    
    dotnet publish -o $packageCode

    # Copy config to package
    Push-Location "PackageRoot"

    $packagePath = "..\" + $packagePath
    Copy-Item -Path ServiceManifest.xml -Destination $packagePath -Force
    $configPath = $packagePath + "Config\"
    #mkdir $configPath
    New-Item -ItemType Directory -Force -Path $configPath
    Copy-Item -Path Config\Settings.xml -Destination $configPath -Force
    
    Pop-Location
    Pop-Location
}


$flavour = "Debug"
$applicationName = "ClusterEmulator"

$packageRoot = ".\" + $applicationName + "\pkg"
if (Test-Path -Path $packageRoot)
{
    Write-Output "Removing existing package at $packageRoot`n"
    Remove-Item -Path $packageRoot -Recurse -Force
}

Write-Output "Building solution`n"
dotnet build -c $flavour

# Package services
Package-Service -project "CoreService" -application $applicationName
# Package application
Write-Output "Packing application manifest`n"
$manifestSource = ".\" + $applicationName + "\ApplicationPackageRoot\ApplicationManifest.xml"
$manifestDestination = ".\" + $applicationName + "\pkg\ApplicationManifest.xml"
Copy-Item -Path $manifestSource -Destination $manifestDestination -Force

Write-Output "Package created at $packageRoot"

