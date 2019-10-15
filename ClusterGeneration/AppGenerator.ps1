# Note: Requires Powershell Core 6+

param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] [Parameter(Mandatory = $true)] $ConfigFile
)

function Read-AppConfig([string]$Path)
{
    if (-Not (Test-Path -Path $Path)) {
        Write-Error -Message "File $Path not found"
        exit 1
    }
    
    $config = Get-Content $Path | ConvertFrom-Json -AsHashtable
    if ((-Not $config.services) -or $config.services.Count -eq 0)
    {
        Write-Error -Message "No services are sepcified in $Path"
        exit 1
    }

    $config
}


function Validate-ServiceConfig([string]$Config)
{
    if (-Not $Config.port) {
        Write-Error -Message "port is missing from service $serviceName"
        exit 1
    }
    elseif($usedPorts[$Config.port]) {
        $port = $Config.port
        $otherService = $usedPorts[$port]
        Write-Error -Message "port $port is already used by $otherService, cannot create $serviceName"
        exit 1
    }
    elseif ((-Not $Config.processors) -or $Config.processors.Count -eq 0) {
        Write-Error -Message "processors are missing from service $serviceName"
        exit 1
    }
    elseif ((-Not $Config.steps) -or $Config.steps.Count -eq 0) {
        Write-Error -Message "steps are missing from service $serviceName"
        exit 1
    }
}


$usedPorts = @{}

$config = Read-AppConfig -Path $ConfigFile 

$appSettingsFile = $null
if ($config.settingsPath) {
    if (-Not (Test-Path -Path $config.settingsPath)) {
        Write-Error -Message "Settings path $ConfigFile is not valid"
        exit 1
    }

    $appSettingsFile = $config.settingsPath      
}
elseif ($config.aiKey) {
    # TODO: pass to template generation
}

foreach ($serviceName in $config.services.Keys) {
    $serviceConfig = $config.services.$serviceName
    Validate-ServiceConfig -Config $serviceConfig

    # Add port to used table
    $usedPorts[$serviceConfig.port] = $serviceName

    # TODO: call dotnet new for templated service with aiKey
    
    if ($appSettingsFile)
    {
        # TODO: replace the appsettings.json file after project genreration
    }

    
    # TODO: optionally process clients and policies ()
}