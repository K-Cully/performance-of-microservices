# Notes: Requires powershell 6

param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] [Parameter(Mandatory = $true)] $ConfigFile
)
 
if (-Not (Test-Path -Path $ConfigFile)) {
    Write-Error -Message "File $ConfigFile not found"
    exit 1
}

$config = Get-Content $ConfigFile | ConvertFrom-Json
if ((-Not $config.services) -or $config.services.Count -eq 0)
{
    Write-Error -Message "No services are sepcified in $ConfigFile"
    exit 1
}

if ($config.settingsPath) {
    if (-Not (Test-Path -Path $config.settingsPath)) {
        Write-Error -Message "Settings path $ConfigFile is not valid"
        exit 1
    }

    # TODO: copy settings after template generation        
}
elseif ($null -ne $config.aiKey) {
    # TODO: pass to template generation
}

foreach ($service in $config.services) {
    if (-Not $service.port) {
        Write-Error -Message "Service path $ConfigFile is not valid"
        exit 1
    }
}