$ErrorActionPreference = 'Stop'

# Set secrets folder to ..\secrets
$SecretsRoot = "$($PSScriptRoot | split-path)\secrets"

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
    $localPath = "$SecretsRoot\$CertName.pfx"
    $thumbPath = "$SecretsRoot\$Certname.thumb.txt"
    $passPath = "$SecretsRoot\$Certname.pwd.txt"
    $existsLocally = Test-Path $localPath

    # create or read certificate
    if($existsLocally) {
        Write-Host "Certificate exists locally."
        $thumbprint = Get-Content $thumbPath
        $password = Get-Content $passPath
        Write-Host "  thumb: $thumbprint, pass: $password"

    }
    else {
        # TODO: implement cert generation in Powershell Core script.
        # See https://github.com/rjmholt/SelfSignedCertificate on cert generation.
        # $thumbprint, $password, $localPath = CreateSelfSignedCertificate $CertName
        
        Write-Error -Message "Certificate not found. Please generate the certificate details at the following locations:`nCert: $localPath`nThumbprint: $thumbPath`nPassword: $passPath"
        return
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
    $perfCounters.Add("LogicalDisk", (("*", "Disk Transfers/sec"), $null))
    #("*", "Avg. Disk sec/Read"), ("*", "Avg. Disk sec/Write"), ("*", "Disk Reads/sec"), , ("*", "Disk Writes/sec")
    $perfCounters.Memory = ("*", "% Committed Bytes In Use"), ("*", "Available MBytes")
    #$perfCounters.Add("Network Adapter", (("*", "Bytes Received/sec"), ("*", "Bytes Sent/sec")))
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
                -IntervalSeconds 30 -Name $name -Force
        }
    }
}


function Get-RdpPassword([string] $ClusterName) {
    $filePath = "$SecretsRoot\$ClusterName.rdp.pwd.txt"

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