# Removes the application from the local cluster

# Import SF SDK module
$sdkInstallPath = (Get-ItemProperty 'HKLM:\Software\Microsoft\Service Fabric SDK').FabricSDKInstallPath
$sfSdkPsModulePath = $sdkInstallPath + "Tools\PSModule\ServiceFabricSDK"
Import-Module $sfSdkPsModulePath\ServiceFabricSDK.psm1

# Configure application information
$applicationName = "ClusterEmulator"
$applicationType = $applicationName + "Type"
$applicationVersion = "1.0.0"
$applicationUri = "fabric:/$applicationName"

# Connect to local cluster (default)
Connect-ServiceFabricCluster

# Remove instance
Remove-ServiceFabricApplication -ApplicationName $applicationUri -Force

# Unregister application
Unregister-ServiceFabricApplicationType -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion -Force