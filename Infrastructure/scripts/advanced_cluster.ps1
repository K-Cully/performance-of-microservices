param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] $TemplateName = "complete.json",   # Name of the cluster ARM template
    [int] $NodeCount = 1,                       # Number of nodes to create
    [string] $ClusterTier = "None",             # The reliability and durability tier of the cluster
    [string] $Location = "northeurope"          # Physical location of all the resources
)

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Define control variables
$ParentDirectory = $PSScriptRoot | split-path
$TemplateDirectory = "$ParentDirectory\templates"
$ResourceGroupName = "$Name-rg"
$Location = "North Europe"
$KeyVaultName = "$Name-vault"
$durabilityLevel = If ($ClusterTier -eq "None") { "Bronze" } Else { $ClusterTier }

# Check that you're logged in to Azure before running anything at all, the call will
# exit the script if you're not
CheckLoggedIn

# Check the resource group we are deploying to exists or create it
EnsureResourceGroup $ResourceGroupName $Location

# Check that the Key Vault resource exists or crete it
$keyVault = EnsureKeyVault $KeyVaultName $ResourceGroupName $Location

# Ensure that a self-signed certificate is created and imported into Key Vault
$cert = EnsureSelfSignedCertificate $KeyVaultName $Name

# Create parameters for cluster resource deployment
Write-Host "Applying cluster template $TemplateName..."
$armParameters = @{
    namePart = $Name;
    certificateThumbprint = $cert.Thumbprint;
    sourceVaultResourceId = $keyVault.ResourceId;
    certificateUrlValue = $cert.SecretId;
    rdpPassword = GeneratePassword;
    vmInstanceCount = $NodeCount;
    reliability = $ClusterTier;
    durability = $durabilityLevel;
}

# Create cluster resources based on the specified ARM template
New-AzureRmResourceGroupDeployment `
  -ResourceGroupName $ResourceGroupName `
  -TemplateFile "$TemplateDirectory\$TemplateName" `
  -Mode Incremental `
  -TemplateParameterObject $armParameters `
  -Verbose
