param(
    [string] [Parameter(Mandatory = $true)] $Name
)

. "$PSScriptRoot\Common.ps1"

$ResourceGroupName = "$Name-rg"
$Location = "North Europe"
$KeyValutName = "$Name-vault"

CheckLoggedIn
EnsureResourceGroup $ResourceGroupName $Location

$keyVault = EnsureKeyVault $KeyValutName $ResourceGroupName $Location

$certThumbprint, $certPassword, $certPath = CreateSelfSignedCertificate $Name
$keyVaultCert = ImportCertificateIntoKeyVault $KeyValutName $Name $certPath $certPassword

$keyVaultCert