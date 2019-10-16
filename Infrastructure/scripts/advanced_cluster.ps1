param(
    [string] [Parameter(Mandatory = $true)] $Name,
    [string] $TemplateName = "complete.json",   # Name of the cluster ARM template
    [int] $NodeCount = 1,                       # Number of nodes to create
    [string] $ClusterTier = "None",             # The reliability and durability tier of the cluster
    [string] $Location = "northeurope",         # Physical location of all the resources
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

# Create parameters for cluster resource deployment
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

if ($PortMappingFile) {
  # Create Service Fabric managment load balancer rules
  [array]$rules = @{name="FabricManagement"; frontendPort="19000"; backendPort="19000"; protocol="tcp";probeProtocol="tcp";probePath=$null}
  [array]$rules += @{name="FabricExplorer"; frontendPort="19080"; backendPort="19080"; protocol="tcp";probeProtocol="tcp";probePath=$null}

  # Read services and ports
  $portServiceMap = Get-Content -Path $PortMappingFile | ConvertFrom-Json -AsHashtable

  # Add service load balancer rules
  foreach ($port in $portServiceMap.Keys) {
    $serviceName = $portServiceMap[$port]
    [array]$rules += @{name="$serviceName"; frontendPort="$port"; backendPort="$port"; protocol="tcp";probeProtocol="http";probePath="/health"}
  }

  $armParameters.lbRules = $rules;
}

# Create cluster resources based on the specified ARM template
Write-Host "Applying cluster template $TemplateName..."
$creationResult = New-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile "$TemplateDirectory\$TemplateName" -Mode Incremental -TemplateParameterObject $armParameters -Verbose

# Add relevant performance counters to the OMS workspace
Add-PerformanceCounters -ResourceGroup $ResourceGroupName -WorkspaceName $OmsWorkspaceName

$creationResult