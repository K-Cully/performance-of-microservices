param(
   [string] [Parameter(Mandatory = $true)] $ClusterName,
   [string] [Parameter(Mandatory = $true)] $ApplicationName,
   [string] [Parameter(Mandatory = $true)] $PackagePath,
   [string] $Location = "northeurope"
)

# Add REST based SF module
Import-Module -Name Microsoft.ServiceFabric.Powershell.Http

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Configure application values
$applicationType = $ApplicationName + "Type"
$applicationVersion = "1.0.0"

# Configure cluster connection
$endpoint = "https://$ClusterName.$Location.cloudapp.azure.com:19080"
$thumbprint = Get-Content "$PSScriptRoot\$ClusterName.thumb.txt"

# TODO: Note demo had the endpoint port as 19080 

# Connect to cluster
Write-Host "connecting to cluster $endpoint using cert thumbprint $thumbprint..."
Connect-SFCluster -ConnectionEndpoint $endpoint `
    -X509Credential `
    -ServerCertThumbprint $thumbprint `
    -FindType FindByThumbprint -FindValue $thumbprint `
    -StoreLocation CurrentUser -StoreName My

Write-Host "Unregistering $applicationType if present..."
$registeredTypes = Get-SFApplicationType
if ($registeredTypes) {
    # TODO: Get-SFApplicationType https://github.com/microsoft/service-fabric-client-dotnet/blob/develop/docs/cmdlets/Get-SFApplicationType.md
    # TODO: Unregister-SFApplicationType -ApplicationTypeName $applicationType -ApplicationTypeVersion $applicationVersion
}

Write-Host "Uploading application package to the cluster image store..."
Copy-SFApplicationPackage -ApplicationPackagePath $PackagePath -ApplicationPackagePathInImageStore $applicationType

Write-Host "Registering application $applicationType..."
Register-SFApplicationType -ApplicationTypeBuildPath $applicationType

Write-Host "Creating application..."
New-SFApplication -Name "fabric:/$applicationType" -TypeName $applicationType -TypeVersion $applicationVersion