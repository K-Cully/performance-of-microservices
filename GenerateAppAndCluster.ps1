# Provisions cluster and required resources
# Generates an app package based on configuration files
# Deploys the app package to the provisioned cluster.

param(
   [string] [Parameter(Mandatory = $true)] $AppName,
   [string] [Parameter(Mandatory = $true)] $ClusterName,
   [string] [Parameter(Mandatory = $true)] $AppConfigFile,
   [bool] [Parameter] $ProvisionInfrastructure = $True
)

# Add REST based SF module
Import-Module -Name Microsoft.ServiceFabric.Powershell.Http

# Create variables
$PortMappingFile = "$PSScriptRoot\generated\$AppName\config\ports.json"
$AppPackagePath = "$PSScriptRoot\generated\$AppName\pkg\"
$NodeCount = 3
$ClusterLevel = "Bronze"
$Location = "northeurope"

$DeploymentStartTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# By default, provision infrastructure first so that generation can proceed while the cluster is provisioning
if ($ProvisionInfrastructure){
    # Deploy infrastructure
    try {
        .\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount `
            -Location $Location -ClusterTier $ClusterLevel
    }
    catch {
        $deploymentPause = 5
        Write-Warning -Message "First deployment attempt failed, waiting $deploymentPause seconds and retrying."
        Start-Sleep -Seconds $deploymentPause
        .\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount `
            -Location $Location -ClusterTier $ClusterLevel        
    }

    # Prompt user to update config file with App Insights key
    Write-Warning -Message "Application Insights has been provisioned.`nPlease update the application config file '$AppConfigFile' before proceeding."
    Read-Host -Prompt "Press Enter to continue..."
}

# Generate application package
.\ClusterGeneration\AppGenerator.ps1 -Name $AppName -ConfigFile $AppConfigFile

# Wait up to 20 minutes for cluster provisioning to complete
$retries = 20
$WaitTimeSeconds = 60
$ClusterEndpoint = "https://$ClusterName.$Location.cloudapp.azure.com:19080"
$ClusterCertThumbprint = Get-Content "$PSScriptRoot\Infrastructure\secrets\$ClusterName.thumb.txt"
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

# Check for in-progress upgrade by using events and 
# Note, this should really just use Get-SFClusterUpgradeProgress but at time of authoring that had a bug
$now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$events = Get-SFClusterEventList -StartTimeUtc $DeploymentStartTime -EndTimeUtc $now
$upgradeStarted = $events | Where-Object { $_.Category -eq "Upgrade" }
$SystemServiceCount = 8

# Since the cluster deployment started event can be delayed check that system services are provisioned too
if (-not $upgradeStarted) {
    $clusterHealth = Get-SFClusterHealth
    $serviceHealth = $clusterHealth.HealthStatistics.HealthStateCountList | Where-Object { $_.EntityKind -eq "Service" } 
    $upgradeStarted = $serviceHealth.HealthStateCount.OkCount -lt $SystemServiceCount
}

# Wait up to 20 minutes for cluster upgrade to complete
$retries = 20
$WaitTimeSeconds = 60
if ($upgradeStarted) {
    Write-Host "Cluster upgrade detected, waiting on completion before continuing."
    $upgradeCompleted = $null
    while ($retries -gt 0 -and -not $upgradeCompleted) {
        $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        $events = Get-SFClusterEventList -StartTimeUtc $DeploymentStartTime -EndTimeUtc $now
        $upgradeCompleted = $events | Where-Object { $_.OverallUpgradeElapsedTimeInMs }

        if (-not $upgradeCompleted) {
            $retries = $retries - 1
            Write-Warning -Message "Cluster upgrade has not completed."            
            if ($retries -gt 0 -and -not $upgradeCompleted) {
                Write-Host "Retrying again in $WaitTimeSeconds seconds. ($retries retries remaining)"
                Start-Sleep -Seconds $WaitTimeSeconds
            }  
        }        
    }
}

# Re-deploy infra with service port forwarding rules
.\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount -Location $Location `
    -PortMappingFile $PortMappingFile -ClusterTier $ClusterLevel

# Package app
.\ClusterGeneration\AppPacker.ps1 -Name $AppName

# Deploy app to cluster
.\Infrastructure\scripts\deploy_app_core.ps1 -ClusterName $ClusterName -ApplicationName $AppName -PackagePath $AppPackagePath