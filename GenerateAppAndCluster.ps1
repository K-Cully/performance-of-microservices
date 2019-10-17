#TODO : params
$AppName = "Sample" 
$ClusterName = "emu-infra-three"
$AppConfigFile = "$PSScriptRoot\ClusterGeneration\SampleAppConfig.json"

# Create variables
$PortMappingFile = "$PSScriptRoot\generated\$AppName\config\ports.json"
$AppPackagePath = "$PSScriptRoot\generated\$AppName\pkg\"
$NodeCount = 3

# Deploy infrastructure
.\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount

# TODO: get ai key and add it to the config file

# TODO: generate app
.\ClusterGeneration\AppGenerator.ps1 -Name $AppName -ConfigFile $AppConfigFile

# Re-deploy infra with service port forwarding rules
# TODO: make $PSScriptRoot relative
.\Infrastructure\scripts\advanced_cluster.ps1 -Name $ClusterName -NodeCount $NodeCount -PortMappingFile $PortMappingFile

# TODO: package app
.\ClusterGeneration\AppPacker.ps1 -Name $AppName

# TODO: wait until cluster is available

# Deploy app to cluster
.\Infrastructure\scripts\deploy_app_core.ps1 -ClusterName $ClusterName -ApplicationName $AppName -PackagePath $AppPackagePath