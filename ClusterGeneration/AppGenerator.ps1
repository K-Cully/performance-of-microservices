# Nots:
# Requires Powershell Core 6+
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
if ($OutputFolder){
    $OutputDirectory = $OutputFolder
}
else {
    $OutputDirectory = "$ParentDirectory\generated\$Name"
}

# Read the app configuration
$AppConfig = Read-AppConfig -Path $ConfigFile 

# Set up optional override for appsettings.json files
$AppSettingsFile = $null
if ($AppConfig.settingsPath) {
    if (-Not (Test-Path -Path $AppConfig.settingsPath)) {
        Write-Error -Message "Settings path $ConfigFile is not valid"
        exit 1
    }

    $AppSettingsFile = $AppConfig.settingsPath      
}

# Ensure value is not null
$AppInsightsKey = $AppConfig.aiKey
if (-not $AppInsightsKey) {
    $AppInsightsKey = "0000"
}

# Clean build and generated folders
Clean-BuildFolders -RootPath $ParentDirectory
if (Test-Path -Path $OutputDirectory) {
    Remove-Item -Path $OutputDirectory -Recurse -Force
}

# Copy simulation engine projects
Copy-Item -Path "$EmulatorDirectory\Service.Models" -Destination "$OutputDirectory\projects\Service.Models" -Recurse -Force
Copy-Item -Path "$EmulatorDirectory\Service.Shared" -Destination "$OutputDirectory\projects\" -Recurse -Force
Copy-Item -Path "$EmulatorDirectory\Service.Simulation" -Destination "$OutputDirectory\projects\" -Recurse -Force

# Ensure service template is installed
$junk = dotnet new -i "$EmulatorDirectory\EmulationService"

# Create projects for all services
foreach ($serviceName in $AppConfig.services.Keys) {
    $serviceConfig = $AppConfig.services[$serviceName]
    Validate-ServiceConfig -Config $serviceConfig -PortAssignments $UsedPorts

    # Add port to used table
    $UsedPorts[$serviceConfig.port] = $serviceName

    # Geterate basic template
    dotnet new protoCoreSF -n $serviceName -p $serviceConfig.port -ai $AppInsightsKey -o "$OutputDirectory\projects\$serviceName\" --force
    
    if ($AppSettingsFile)
    {
        # Replace the appsettings.json file with the one specified
        Copy-Item -Path $AppSettingsFile -Destination "$OutputDirectory\projects\$serviceName\appsettings.json" -Force
    }

    foreach ($processor in $serviceConfig.processors.Keys) {
        $value = $serviceConfig.processors[$processor]
        Write-Host -Message "$processor - $value"
        # TODO: finished here - Need to remove output and fix template generation 
        Create-Setting -Name $processor -Value $serviceConfig.processors[$processor]
    }
    # TODO: process policies and steps
    
    # TODO: optionally process clients and policies ()
}

# Output service names and ports to a file
New-Item -Path "$OutputDirectory\config\" -ItemType directory
$UsedPorts | Out-File -FilePath "$OutputDirectory\config\ports.txt"