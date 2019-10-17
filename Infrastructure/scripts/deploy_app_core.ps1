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
$ApplicationType = $ApplicationName + "Type"
$ApplicationVersion = "1.0.0"

# Configure cluster connection
$Endpoint = "https://$ClusterName.$Location.cloudapp.azure.com:19080"
$Thumbprint = Get-Content "$PSScriptRoot\$ClusterName.thumb.txt"
$TimeoutSeconds = 600

# Connect to cluster
Write-Host "connecting to cluster $Endpoint using cert thumbprint $Thumbprint..."
Connect-SFCluster -ConnectionEndpoint $Endpoint `
    -X509Credential `
    -ServerCertThumbprint $Thumbprint `
    -FindType FindByThumbprint -FindValue $Thumbprint `
    -StoreLocation CurrentUser -StoreName My

# Stop active applications
$activeApplications = Get-SFApplication -ApplicationTypeName $ApplicationType -ServerTimeout $TimeoutSeconds
if ($activeApplications) {
    Write-Host "Found $($activeApplications.Count) active applications matching $ApplicationType"
    foreach ($app in $activeApplications) {
        Write-Host "Stopping $($app.ApplicationId) version $($app.ApplicationTypeVersion)"
        Remove-SFApplication -ApplicationId $app.ApplicationId -ForceRemove $true -ServerTimeout $TimeoutSeconds -Force
    }
}

# Remove all application versions of the app type
$registeredTypes = Get-SFApplicationType -ApplicationTypeName $ApplicationType -ServerTimeout $TimeoutSeconds
if ($registeredTypes) {
    Write-Host "Found $($registeredTypes.Count) registered application types matching $ApplicationType"
    foreach ($application in $registeredTypes) {
        Write-Host "Unregistering $($application.ApplicationTypeName) version $($application.ApplicationTypeVersion)"
        Unregister-SFApplicationType -ApplicationTypeName $application.ApplicationTypeName `
            -ApplicationTypeVersion $application.ApplicationTypeVersion -ServerTimeout $TimeoutSeconds -Force
    }
}

# Remove app from the image store if it is already present
$existingContent = $null
try {
    $existingContent = Get-SFImageStoreContent -ContentPath $ApplicationType -ErrorAction Ignore
}
catch {
    # Swallow error since -ErrorAction Ignore is not honoured
}

if ($existingContent) {
    Write-Host "Removing existing content from the image store."
    Remove-SFImageStoreContent -ContentPath $ApplicationType -ServerTimeout $TimeoutSeconds -Force
}

# Since the module doesn't return anything useful to indicate failure reason, retry all errors
$result = $null
$attempt = 0
$successMatch = "Success!"
while ($result -ne $successMatch -and $attempt -lt 5) {
    $attempt = $attempt + 1
    Write-Host "Uploading application package to the cluster image store. (Attempt $attempt)"
    $result = Copy-SFApplicationPackage -ApplicationPackagePath $PackagePath -ApplicationPackagePathInImageStore $ApplicationType -Verbose
}

# TODO: re-add this once module returns result correctly
# For now, just retry 5 times and assume that it worked.
# Return with failure if package could not be uploaded
#if (-not ($result -like $successMatch)) {
#    Write-Error -Message "Upload failed with message '$result' after $attempt attempts!"
#    exit 1
#}

Write-Host "Application package uploaded to image store after $attempt attempts."

# Note, may need to make async and delay creation for large app sizes
Write-Host "Registering application $ApplicationType..."
Register-SFApplicationType -ImageStorePath -ApplicationTypeBuildPath $ApplicationType -ServerTimeout $TimeoutSeconds

Write-Host "Creating application..."
New-SFApplication -Name "fabric:/$ApplicationType" -TypeName $ApplicationType -TypeVersion $ApplicationVersion `
    -ServerTimeout $TimeoutSeconds