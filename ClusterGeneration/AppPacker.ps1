# Packages the application for installation in any Service Fabric Cluster

param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] [Parameter(Mandatory = $false)] $Source,
    [string] [Parameter(Mandatory = $false)] $Destination
)

Import-Module -Name Microsoft.PowerShell.Management
Import-Module -Name Microsoft.PowerShell.Utility

# Add utility functions
. "$PSScriptRoot\Common.ps1"

# Set relevant directory paths
$ParentDirectory = $PSScriptRoot | Split-Path
if ($Source) {
    $AppSource = $Source
}
else {
    $AppSource = "$ParentDirectory\generated\$Name"
}

if ($Destination) {
    $OutputPath = $Destination
}
else {
    $OutputPath = "$AppSource\pkg"
}

$ProjectsFolder = "$AppSource\projects"
$ConfigFolder = "$AppSource\config"

# Set control variables
$BuildFlavour = "Release"
$PortsAndServices = Get-Content -Path "$ConfigFolder\ports.txt"

if (Test-Path -Path $OutputPath)
{
    Write-Warning "Removing existing package at $OutputPath"
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Package services
foreach ($service in $PortsAndServices.Values) {
    Package-Service -Name $service -Location $ProjectsFolder -PackageRoot $OutputPath -Flavour $BuildFlavour
}


# TODO: update below code

# Package application
Write-Output "Packing application manifest`n"
$manifestSource = ".\" + $applicationName + "\ApplicationPackageRoot\ApplicationManifest.xml"
$manifestDestination = ".\" + $applicationName + "\pkg\ApplicationManifest.xml"
Copy-Item -Path $manifestSource -Destination $manifestDestination -Force

Write-Output "Package created at $packageRoot"

