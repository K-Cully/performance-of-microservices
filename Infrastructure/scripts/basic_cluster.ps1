param(
    [string] [Parameter(Mandatory = $true)] $Name
)

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Define control variables
$ParentDirectory = $PSScriptRoot | split-path
$TemplateDirectory = "$ParentDirectory\templates"
$ResourceGroupName = "$Name-rg"
$Location = "North Europe"
$KeyValutName = "$Name-vault"

# Check that you're logged in to Azure before running anything at all, the call will
# exit the script if you're not
CheckLoggedIn

# Check the resource group we are deploying to exists or create it
EnsureResourceGroup $ResourceGroupName $Location

# Check that the Key Vault resource exists or crete it
$keyVault = EnsureKeyVault $KeyValutName $ResourceGroupName $Location

# Create a self-signed cluster certificate for development and testing 
$certThumbprint, $certPassword, $certPath = CreateSelfSignedCertificate $Name

# Add the certificate to the Key Vault
$keyVaultCert = ImportCertificateIntoKeyVault $KeyValutName $Name $certPath $certPassword

# Create parameters for cluster resource deployment
Write-Host "Deploying cluster with ARM template..."
$armParameters = @{
    namePart = $Name;
    certificateThumbprint = $certThumbprint;
    sourceVaultResourceId = $keyVault.ResourceId;
    certificateUrlValue = $keyVaultCert.SecretId;
    rdpPassword = GeneratePassword;
}

# Create cluster resources based on the specified ARM template
New-AzureRmResourceGroupDeployment `
  -ResourceGroupName $ResourceGroupName `
  -TemplateFile "$TemplateDirectory\minimal.json" `
  -Mode Incremental `
  -TemplateParameterObject $armParameters `
  -Verbose

  # If the following call (SF SDK) responds successfully, the cluser is provisioned
  # Connect-ServiceFabricCluster -ConnectionEndpoint $Name.northeurope.cloudapp.azure.com:19000 -X509Credential -FindType FindByThumbprint -FindValue $certThumbprint -StoreLocation CurrentUser -StoreName My