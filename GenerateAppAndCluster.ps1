# Provisions cluster and required resources
# Generates an app package based on configuration files
# Deploys the app package to the provisioned cluster.

param(
   [string] [Parameter(Mandatory = $true)] $AppName,
   [string] [Parameter(Mandatory = $true)] $ClusterName,
   [string] [Parameter(Mandatory = $true)] $AppConfigFile
)

# Add REST based SF module
Import-Module -Name Microsoft.ServiceFabric.Powershell.Http

# Create variables
$PortMappingFile = "$PSScriptRoot\generated\$AppName\config\ports.json"
$AppPackagePath = "$PSScriptRoot\generated\$AppName\pkg\"
$NodeCount = 3
$Location = "northeurope"

# Deploy infrastructure
.\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount -Location $Location

# Prompt user to update config file with App Insights key
Write-Warning -Message "Application Insights has been provisioned.`nPlease update the application config file '$AppConfigFile' before proceeding."
Read-Host -Prompt "Press Enter to continue..."

# Generate application package
.\ClusterGeneration\AppGenerator.ps1 -Name $AppName -ConfigFile $AppConfigFile

# Re-deploy infra with service port forwarding rules
.\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount -PortMappingFile $PortMappingFile

# Package app
.\ClusterGeneration\AppPacker.ps1 -Name $AppName

# Wait up to 15 minutes for cluster to become available
$ClusterEndpoint = "https://$ClusterName.$Location.cloudapp.azure.com:19080"
$ClusterCertThumbprint = Get-Content "$PSScriptRoot\$ClusterName.thumb.txt"
$WaitTimeSeconds = 30
$retries = 30
$success = $false
while ($retries -gt 0 -and -not $success) {
    try {
        Connect-SFCluster -ConnectionEndpoint $ClusterEndpoint `
            -X509Credential `
            -ServerCertThumbprint $ClusterCertThumbprint `
            -FindType FindByThumbprint -FindValue $ClusterCertThumbprint `
            -StoreLocation CurrentUser -StoreName My
        $success = $true
    }
    catch {
        $retries = $retries - 1
        Write-Warning -Message "Connection to '$ClusterEndpoint' failed."
        if ($retries -gt 0) {
            Write-Host "Retrying again in $WaitTimeSeconds seconds. ($retries retries remaining)"
            Start-Sleep -Seconds $WaitTimeSeconds
        }   
    }
}

if (-not $success) {
    Write-Error -Message "Could not access cluster, exiting without deploying application"
    Write-Host "To manually trigger application deployment run 'deploy_app_core.ps1 -ClusterName $ClusterName -ApplicationName $AppName -PackagePath $AppPackagePath'"
    exit 1
}

# Deploy app to cluster
.\Infrastructure\scripts\deploy_app_core.ps1 -ClusterName $ClusterName -ApplicationName $AppName -PackagePath $AppPackagePath