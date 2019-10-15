# Nots:
# Requires Powershell Core 6+
# If Visual Studio is open in the background, debug folders may be copied to output though they will not affect execution.


param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] [Parameter(Mandatory = $true)] $ConfigFile,
    [string] [Parameter(Mandatory = $false)] $OutputFolder
)

# Add utility functions
. "$PSScriptRoot\Common.ps1"

$UsedPorts = @{}
$ParentDirectory = $PSScriptRoot | Split-Path
$EmulatorDirectory = "$ParentDirectory\ClusterEmulator"

if ($OutputFolder){
    $OutputDirectory = $OutputFolder
}
else {
    $OutputDirectory = "$ParentDirectory\generated\$Name\"
}

$AppConfig = Read-AppConfig -Path $ConfigFile 

$appSettingsFile = $null
if ($AppConfig.settingsPath) {
    if (-Not (Test-Path -Path $AppConfig.settingsPath)) {
        Write-Error -Message "Settings path $ConfigFile is not valid"
        exit 1
    }

    $appSettingsFile = $AppConfig.settingsPath      
}
elseif ($AppConfig.aiKey) {
    # TODO: pass to template generation
}

# Clean build folders and copy simulation engine projects
Clean-BuildFolders -RootPath $ParentDirectory
Copy-Item -Path "$EmulatorDirectory\Service.Models" -Destination "$OutputDirectory\projects"
Copy-Item -Path "$EmulatorDirectory\Service.Shared" -Destination "$OutputDirectory\projects"
Copy-Item -Path "$EmulatorDirectory\Service.Simulation" -Destination "$OutputDirectory\projects"

# TODO: build simulation and reference dlls for service validation
# TODO: .\ClusterEmulator\Service.Simulation\bin\Release\netcoreapp2.2\ClusterEmulator.Service.Simulation.dll

# Create projects for all services
foreach ($serviceName in $AppConfig.services.Keys) {
    $serviceConfig = $AppConfig.services[$serviceName]
    Validate-ServiceConfig -Config $serviceConfig -PortAssignments $UsedPorts

    # Add port to used table
    $UsedPorts[$serviceConfig.port] = $serviceName

    # TODO: call dotnet new for templated service with aiKey
    
    if ($appSettingsFile)
    {
        # TODO: replace the appsettings.json file after project genreration
    }

    
    # TODO: optionally process clients and policies ()
}