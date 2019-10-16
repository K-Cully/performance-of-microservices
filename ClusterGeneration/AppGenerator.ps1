# Generates servcices and an application manifest for a prototype application.
# Requires Powershell Core 6+.
# If the ClusterEmulator project is open in another IDE, there may be issues executing this script.

param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] [Parameter(Mandatory = $true)] $ConfigFile,
    [string] [Parameter(Mandatory = $false)] $OutputFolder
)

# Add utility functions
. "$PSScriptRoot\Common.ps1"

$UsedPorts = @{}

# Set relevant folder locations
$ParentDirectory = $PSScriptRoot | Split-Path
$EmulatorDirectory = "$ParentDirectory\ClusterEmulator"
if ($OutputFolder) {
    $OutputDirectory = $OutputFolder
}
else {
    $OutputDirectory = "$ParentDirectory\generated\$Name"
}

# Read the app configuration
$AppConfig = Read-AppConfig -Path $ConfigFile 

# Set up optional override for appsettings.json files
$AppSettingsFile = $null
if ($AppConfig.appsettingsPath) {
    if (-Not (Test-Path -Path $AppConfig.appsettingsPath)) {
        Write-Error -Message "Appsettings path $ConfigFile is not valid"
        exit 1
    }

    $AppSettingsFile = $AppConfig.appsettingsPath      
}

# Ensure value is not null
$AppInsightsKey = $AppConfig.aiKey
if (-not $AppInsightsKey) {
    $AppInsightsKey = "0000"
}

Write-Host -Message "Cleaning build and generated folders"
Clean-BuildFolders -RootPath $ParentDirectory
if (Test-Path -Path $OutputDirectory) {
    Remove-Item -Path $OutputDirectory -Recurse -Force
}

Write-Host -Message "Copying project dependencies to $OutputDirectory\projects"
Copy-Item -Path "$EmulatorDirectory\Service.Models" -Destination "$OutputDirectory\projects\Service.Models" -Recurse -Force
Copy-Item -Path "$EmulatorDirectory\Service.Shared" -Destination "$OutputDirectory\projects\" -Recurse -Force
Copy-Item -Path "$EmulatorDirectory\Service.Simulation" -Destination "$OutputDirectory\projects\" -Recurse -Force

# Ensure service template is installed and suppress console output
$junk = dotnet new -i "$EmulatorDirectory\EmulationService"

Write-Host -Message "Generating services under $OutputDirectory\projects"
foreach ($serviceName in $AppConfig.services.Keys) {
    $serviceConfig = $AppConfig.services[$serviceName]
    Validate-ServiceConfig -Config $serviceConfig -PortAssignments $UsedPorts

    # Add port to used table
    $UsedPorts[$serviceConfig.port] = $serviceName

    # Geterate basic template
    Write-Host -Message "Generating $serviceName project with port $port and App Insights key $AppInsightsKey"
    dotnet new protoCoreSF -n $serviceName -p $serviceConfig.port -ai $AppInsightsKey -o "$OutputDirectory\projects\$serviceName\" --force
    
    # Optionally, replace the appsettings.json file with the one specified
    if ($AppSettingsFile)
    {
        Write-Host -Message "Replacing appsettings.json file"
        Copy-Item -Path $AppSettingsFile -Destination "$OutputDirectory\projects\$serviceName\appsettings.json" -Force
    }

    # Generate Settings    
    $serviceSettingsFile = "$OutputDirectory\projects\$serviceName\PackageRoot\Config\Settings.xml"
    Write-Host -Message "Generating processor settings"
    Generate-Settings -Section $serviceConfig.processors -Placeholder "<!--Processors_Placeholder-->" -File $serviceSettingsFile
    Write-Host -Message "Generating step settings"    
    Generate-Settings -Section $serviceConfig.steps -Placeholder "<!--Steps_Placeholder-->" -File $serviceSettingsFile
    Write-Host -Message "Generating client settings"
    Generate-Settings -Section $serviceConfig.clients -Placeholder "<!--Clients_Placeholder-->" -File $serviceSettingsFile
    Write-Host -Message "Generating policy settings"
    Generate-Settings -Section $serviceConfig.policies -Placeholder "<!--Policies_Placeholder-->" -File $serviceSettingsFile
}

# Output service names and ports to a file
New-Item -Path "$OutputDirectory\config\" -ItemType directory
$UsedPorts | Out-File -FilePath "$OutputDirectory\config\ports.txt"

# Copy AppManifest to output
Copy-Item -Path "$EmulatorDirectory\ClusterEmulator\ApplicationPackageRoot\ApplicationManifest.xml" -Destination "$OutputDirectory\config\"

# TODO: Add services and ports to app manifest

Write-Host -Message "Successfully generated application $Name in $OutputDirectory"