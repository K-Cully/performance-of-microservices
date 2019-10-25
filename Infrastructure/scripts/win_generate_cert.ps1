# Temporary script for generating certificates and related files in the correct location
# until a suitable cross-platform (PS Core) replacement can be added to Common.ps1

param(
    [string] [Parameter(Mandatory = $true)] $Name
)

# Set secrets folder to ..\secrets
$SecretsRoot = "$($PSScriptRoot | split-path)\secrets"

function CreateSelfSignedCertificate([string]$DnsName)
{
    # TODO: convert cert generation to work with Powershell Core and be cross platform.
    # See https://github.com/rjmholt/SelfSignedCertificate on cert generation.

    Write-Host "Creating self-signed certificate with dns name $DnsName"
    
    $filePath = "$SecretsRoot\$DnsName.pfx"

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
    Set-Content -Path "$SecretsRoot\$DnsName.thumb.txt" -Value $thumbprint
    Set-Content -Path "$SecretsRoot\$DnsName.pwd.txt" -Value $certPassword
    Write-Host "  exported."

    $thumbprint
    $certPassword
    $filePath
}


# Generate certificate
CreateSelfSignedCertificate $Name