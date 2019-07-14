# Installs the package on a local cluster running on Windows

$sdkInstallPath = (Get-ItemProperty 'HKLM:\Software\Microsoft\Service Fabric SDK').FabricSDKInstallPath
$sfSdkPsModulePath = $sdkInstallPath + "Tools\PSModule\ServiceFabricSDK"
Import-Module $sfSdkPsModulePath\ServiceFabricSDK.psm1

$applicationName = "ClusterEmulator"
$applicationType = $applicationName + "Type"
$applicationVersion = "1.0.0"
$appPath = "$PSScriptRoot\$applicationName"

# Connect to local cluster (default)
Connect-ServiceFabricCluster

# Register package through image store
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $appPath -ApplicationPackagePathInImageStore $applicationType -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest)) -TimeoutSec 1800
Register-ServiceFabricApplicationType $applicationType

# Create application
New-ServiceFabricApplication -Applicationname "fabric:/$applicationName" -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion