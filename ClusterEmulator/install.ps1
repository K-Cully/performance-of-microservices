# Installs the package on a local cluster

# Import SF SDK module
$sdkInstallPath = (Get-ItemProperty 'HKLM:\Software\Microsoft\Service Fabric SDK').FabricSDKInstallPath
$sfSdkPsModulePath = $sdkInstallPath + "Tools\PSModule\ServiceFabricSDK"
Import-Module $sfSdkPsModulePath\ServiceFabricSDK.psm1

# Configure application information
$applicationName = "ClusterEmulator"
$applicationType = $applicationName + "Type"
$applicationVersion = "1.0.0"
$packageLocation = "$PSScriptRoot\$applicationName\pkg\"

# Connect to local cluster (default)
Connect-ServiceFabricCluster

# Test the package is valid
Test-ServiceFabricApplicationPackage -ApplicationPackagePath $packageLocation

# Upload the package to the cluster's image store
$clusterManifest = Get-ServiceFabricClusterManifest
$connectionString = Get-ImageStoreConnectionStringFromClusterManifest -ClusterManifest $clusterManifest
$timeout = 1800
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packageLocation -ApplicationPackagePathInImageStore $applicationType -ImageStoreConnectionString $connectionString -TimeoutSec $timeout

# Create the application
Register-ServiceFabricApplicationType $applicationType
New-ServiceFabricApplication -Applicationname "fabric:/$applicationName" -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion