# Installs the package on a local cluster running on Windows

$sdkInstallPath = (Get-ItemProperty 'HKLM:\Software\Microsoft\Service Fabric SDK').FabricSDKInstallPath
$sfSdkPsModulePath = $sdkInstallPath + "Tools\PSModule\ServiceFabricSDK"
Import-Module $sfSdkPsModulePath\ServiceFabricSDK.psm1

$applicationName = "ClusterEmulator"
$applicationType = $applicationName + "Type"
$applicationVersion = "1.0.0"
$packageLocation = "$PSScriptRoot\$applicationName"

# Connect to local cluster (default)
Connect-ServiceFabricCluster

# Test the package is valid
Test-ServiceFabricApplicationPackage -ApplicationPackagePath $packageLocation

$clusterManifest = Get-ServiceFabricClusterManifest
$connectionString = Get-ImageStoreConnectionStringFromClusterManifest -ClusterManifest $clusterManifest


# Register package through image store
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packageLocation -ApplicationPackagePathInImageStore $applicationType -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest)) -TimeoutSec 1800
Register-ServiceFabricApplicationType $applicationType

# Create application
New-ServiceFabricApplication -Applicationname "fabric:/$applicationName" -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion