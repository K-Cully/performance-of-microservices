param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] $TemplateName = "complete.json",   # Name of the cluster ARM template
    [int] $NodeCount = 1,                       # Number of nodes to create
    [string] $ClusterTier = "None",             # The reliability and durability tier of the cluster
    [string] $Location = "northeurope"          # Physical location of all the resources
    [string] $PortMappingFile                   # A file containing required port to service mapping information
)

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Define control variables
$ParentDirectory = $PSScriptRoot | split-path
$TemplateDirectory = "$ParentDirectory\templates"
$ResourceGroupName = "$Name-rg"
$Location = "North Europe"
$KeyVaultName = "$Name-vault"
$DurabilityLevel = If ($ClusterTier -eq "None") { "Bronze" } Else { $ClusterTier }
$OmsWorkspaceName = "$Name-oms"

# Check that you're logged in to Azure before running anything at all, the call will
# exit the script if you're not
CheckLoggedIn

# Check the resource group we are deploying to exists or create it
EnsureResourceGroup $ResourceGroupName $Location

# Check that the Key Vault resource exists or crete it
$keyVault = EnsureKeyVault $KeyVaultName $ResourceGroupName $Location

# Ensure that a self-signed certificate is created and imported into Key Vault
$cert = EnsureSelfSignedCertificate $KeyVaultName $Name


if ($PortMappingFile) {
  # Create an array of service parameters
  $serviceObjects = foreach ($port in $a.Keys) {
    $obj = New-Object -TypeName psobject
    $obj | Add-Member -MemberType NoteProperty -Name name -Value $a[$port]
    $obj | Add-Member -MemberType NoteProperty -Name port -Value $porttion
    $obj
  }

  # Convert the array to JSON
  $serviceJson = $serviceObjects | ConvertTo-Json

  # TODO: pass Json to template

  # TODO: generate JSON with Names and Ports in an acceptable format 
  # TODO: update ports and health checks in load balancer resource

}


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
    durability = $DurabilityLevel;
    omsWorkspaceName = $OmsWorkspaceName;
}

# Create cluster resources based on the specified ARM template
$creationResult = New-AzureRmResourceGroupDeployment `
  -ResourceGroupName $ResourceGroupName `
  -TemplateFile "$TemplateDirectory\$TemplateName" `
  -Mode Incremental `
  -TemplateParameterObject $armParameters `
  -Verbose

# Add relevant performance counters to the OMS workspace
Add-PerformanceCounters -ResourceGroup $ResourceGroupName -WorkspaceName $OmsWorkspaceName

$creationResult