$ErrorActionPreference = 'Stop'

$t = [Reflection.Assembly]::LoadWithPartialName("System.Web")
Write-Host "Loaded $($t.FullName)."

function CheckLoggedIn()
{
    Write-Host "Validating if you are logged in..."
    $azContext = Get-AzContext

    if($null -eq $azContext.Account) {
        Write-Host "  you are not logged into Azure. Use Connect-AzAccount to log in first and optionally select a subscription" -ForegroundColor Red
        exit
    }

    Write-Host "  account:      '$($azContext.Account.Id)'"
    Write-Host "  subscription: '$($azContext.Subscription.Name)'"
    Write-Host "  tenant:       '$($azContext.Tenant.Id)'"
}

function EnsureResourceGroup([string]$Name, [string]$Location)
{
    # Prepare resource group
    Write-Host "Checking if resource group '$Name' exists..."
    $resourceGroup = Get-AzResourceGroup -Name $Name -Location $Location -ErrorAction Ignore
    if($null -eq $resourceGroup)
    {
        Write-Host "  resource group doesn't exist, creating a new one..."
        $resourceGroup = New-AzResourceGroup -Name $Name -Location $Location
        Write-Host "  resource group created."
    }
    else
    {
        Write-Host "  resource group already exists."
    }
}

function EnsureKeyVault([string]$Name, [string]$ResourceGroupName, [string]$Location)
{
    # properly create a new Key Vault
    # KV must be enabled for deployment (last parameter)
    Write-Host "Checking if Key Vault '$Name' exists..."
    $keyVault = Get-AzKeyVault -VaultName $Name -ErrorAction Ignore
    if($null -eq $keyVault)
    {
        Write-Host "  key vault doesn't exist, creating a new one..."
        # Personal accounts are not autoamtically granted vault access so ignore warning and add access policy
        $user = Get-AzADUser
        $keyVault = New-AzKeyVault -VaultName $Name -ResourceGroupName $ResourceGroupName -Location $Location -EnabledForDeployment -WarningAction Ignore
        Set-AzKeyVaultAccessPolicy -VaultName $Name -UserPrincipalName $user.UserPrincipalName -PermissionsToCertificates get,list,delete,create,import,update -PermissionsToKeys get,list,delete,create,import,update,sign,decrypt,encrypt -PermissionsToSecrets get,list,set,delete -PassThru
        Write-Host "  Key Vault Created and enabled for deployment."
    }
    else
    {
        Write-Host "  key vault already exists."
    }

    $keyVault
}

function CreateSelfSignedCertificate([string]$DnsName)
{
    # TODO: convert cert generation to work with Powershell Core and be cross platform.
    # See https://github.com/rjmholt/SelfSignedCertificate on cert generation.

    # TODO: Import-WinModule Microsoft.

    Write-Host "Creating self-signed certificate with dns name $DnsName"
    
    $filePath = "$PSScriptRoot\$DnsName.pfx"

    Write-Host "  generating password... " -NoNewline
    $certPassword = GeneratePassword
    Write-Host "$certPassword"

    Write-Host "  generating certificate... " -NoNewline
    $securePassword = ConvertTo-SecureString $certPassword -AsPlainText -Force
    $thumbprint = (New-SelfSignedCertificate -DnsName $DnsName -CertStoreLocation Cert:\CurrentUser\My -KeySpec KeyExchange).Thumbprint
    Write-Host "$thumbprint."
    
    Write-Host "  exporting to $filePath..."
    $certContent = (Get-ChildItem -Path cert:\CurrentUser\My\$thumbprint)
    $t = Export-PfxCertificate -Cert $certContent -FilePath $filePath -Password $securePassword
    Set-Content -Path "$PSScriptRoot\$DnsName.thumb.txt" -Value $thumbprint
    Set-Content -Path "$PSScriptRoot\$DnsName.pwd.txt" -Value $certPassword
    Write-Host "  exported."

    $thumbprint
    $certPassword
    $filePath
}

function ImportCertificateIntoKeyVault([string]$KeyVaultName, [string]$CertName, [string]$CertFilePath, [string]$CertPassword)
{
    #Write-Host

    Write-Host "Importing certificate..."
    Write-Host "  generating secure password..."
    $securePassword = ConvertTo-SecureString $CertPassword -AsPlainText -Force
    Write-Host "  uploading to KeyVault..."
    Import-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $CertName -FilePath $CertFilePath -Password $securePassword
    Write-Host "  imported."
}

function GeneratePassword()
{
    $characters = [char[]]"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
    $specialCharacters = [char[]]"!@#%&^?~[]{}_+"

    $inner = $characters | Get-Random -Count 13
    $outer = $specialCharacters | Get-Random -Count 2
    $passowrd = "$inner$outer"
    $passowrd
}

function EnsureSelfSignedCertificate([string]$KeyVaultName, [string]$CertName)
{
    $localPath = "$PSScriptRoot\$CertName.pfx"
    $existsLocally = Test-Path $localPath

    # create or read certificate
    if($existsLocally) {
        Write-Host "Certificate exists locally."
        $thumbprint = Get-Content "$PSScriptRoot\$Certname.thumb.txt"
        $password = Get-Content "$PSScriptRoot\$Certname.pwd.txt"
        Write-Host "  thumb: $thumbprint, pass: $password"

    } else {
        $thumbprint, $password, $localPath = CreateSelfSignedCertificate $CertName
    }

    #import into vault if needed
    Write-Host "Checking certificate in key vault..."
    $kvCert = Get-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $CertName
    if($null -eq $kvCert) {
        Write-Host "  importing..."
        $securePassword = ConvertTo-SecureString $password -AsPlainText -Force
        $kvCert = Import-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $CertName -FilePath $localPath -Password $securePassword
    } else {
        Write-Host "  certificate already imported."
    }

    $kvCert
}

function Connect-SecureCluster([string]$ClusterName, [string]$Thumbprint)
{
    $Endpoint = "$ClusterName.westeurope.cloudapp.azure.com:19000"

    Write-Host "connecting to cluster $Endpoint using cert thumbprint $Thumbprint..."
    
    Connect-ServiceFabricCluster -ConnectionEndpoint $Endpoint `
        -X509Credential `
        -ServerCertThumbprint $Thumbprint `
        -FindType FindByThumbprint -FindValue $Thumbprint `
        -StoreLocation CurrentUser -StoreName My
}

function Unregister-ApplicationTypeCompletely([string]$ApplicationTypeName)
{
    Write-Host "Checking if application type $ApplicationTypeName is present.."
    $type = Get-ServiceFabricApplicationType -ApplicationTypeName $ApplicationTypeName
    if($null -eq $type) {
        Write-Host "  Application is not in the cluster"
    } else {
        $runningApps = Get-ServiceFabricApplication -ApplicationTypeName $ApplicationTypeName
        foreach($app in $runningApps) {
            $uri = $app.ApplicationName.AbsoluteUri
            Write-Host "    Anregistering '$uri'..."

            $t = Remove-ServiceFabricApplication -ApplicationName $uri -ForceRemove -Verbose -Force
        }

        Write-Host "  Unregistering type..."
        $t =Unregister-ServiceFabricApplicationType `
            -ApplicationTypeName $ApplicationTypeName -ApplicationTypeVersion $type.ApplicationTypeVersion `
            -Force -Confirm

    }
}

function Add-PerformanceCounters([string]$ResourceGroup, [string]$WorkspaceName)
{
    $perfCounters = @{}
    $perfCounters.LogicalDisk = ("*", "Avg. Disk sec/Read"), ("*", "Avg. Disk sec/Write"), `
        ("*", "Disk Reads/sec"), ("*", "Disk Transfers/sec"), ("*", "Disk Writes/sec")
    $perfCounters.Memory = ("*", "% Committed Bytes In Use"), ("*", "Available MBytes")
    $perfCounters.Add("Network Adapter", (("*", "Bytes Received/sec"), ("*", "Bytes Sent/sec")))
    $perfCounters.Add("Network Interface", (("*", "Bytes Total/sec"), $null))
    $perfCounters.Add("System", (("*", "Processor Queue Length"), $null))
    $perfCounters.Add("Processor", (("_Total", "% Processor Time"), $null))

    Write-Output("Adding performance counters to OMS workplace $WorkspaceName in resource group $ResourceGroup")
    foreach($perfCounter in $perfCounters.Keys)
    {
        foreach($perfCounterTuple in $perfCounters[$perfCounter])
        {
            if ($null -eq $perfCounterTuple)
            {
                continue
            }

            $instanceName = $perfCounterTuple[0]
            $counterName = $perfCounterTuple[1]
            $name = "$perfCounter-$counterName"
            $name = $name.Replace("/", "per").Replace("%", "percent")

            # Add performance counter data sources
            New-AzOperationalInsightsWindowsPerformanceCounterDataSource `
                -ResourceGroupName $ResourceGroup -WorkspaceName $WorkspaceName `
                -ObjectName $perfCounter -InstanceName $instanceName  -CounterName $counterName `
                -IntervalSeconds 15 -Name $name -Force
        }
    }
}


function Get-RdpPassword([string] $ClusterName) {
    $filePath = "$PSScriptRoot\$ClusterName.rdp.pwd.txt"

    if (Test-Path -Path $filePath) {
        Write-Warning -Message "Password file exists, retrieving existing password"
        $rdpPassword = Get-Content -Path $filePath
        Write-Host "'$rdpPassword' retrieved from $filePath"
    }
    else {
        Write-Host "  generating password... "
        $rdpPassword = GeneratePassword
        Set-Content -Path $filePath -Value $rdpPassword        
        Write-Host "'$rdpPassword' written to $filePath"
    }

    $rdpPassword
}