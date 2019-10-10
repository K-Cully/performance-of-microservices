param(
   [string] [Parameter(Mandatory = $true)] $ClusterName,
   [string] [Parameter(Mandatory = $true)] $ApplicationName,
   [string] [Parameter(Mandatory = $true)] $PackagePath,
   [string] $Location = "northeurope"
)

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Configure application values
$applicationType = $ApplicationName + "Type"
$applicationVersion = "1.0.0"

# Configure cluster connection
$endpoint = "$ClusterName.$Location.cloudapp.azure.com:19000"
$thumbprint = Get-Content "$PSScriptRoot\$ClusterName.thumb.txt"

# Connect to cluster
Write-Host "connecting to cluster $endpoint using cert thumbprint $thumbprint..."
Connect-ServiceFabricCluster -ConnectionEndpoint $endpoint `
    -X509Credential `
    -ServerCertThumbprint $thumbprint `
    -FindType FindByThumbprint -FindValue $thumbprint `
    -StoreLocation CurrentUser -StoreName My

Write-Host "Unregistering $applicationType if present..."
Unregister-ApplicationTypeCompletely $applicationType

Write-Host "Uploading test application binary to the cluster..."
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $PackagePath -ApplicationPackagePathInImageStore $applicationType -TimeoutSec 1800 -ShowProgress

Write-Host "Registering application $applicationType..."
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $applicationType

Write-Host "Creating application..."
New-ServiceFabricApplication -ApplicationName "fabric:/$applicationType" -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion
