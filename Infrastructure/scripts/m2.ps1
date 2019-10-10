param(
    [string] [Parameter(Mandatory = $true)] $Name
)

. "$PSScriptRoot\Common.ps1"

$GenericName = "test-delete"

$ResourceGroupName = "$GenericName-$Name"
$Location = "North Europe"
$KeyValutName = "$GenericName-vault"

CheckLoggedIn
EnsureResourceGroup $ResourceGroupName $Location

$keyVault = EnsureKeyVault $KeyValutName $ResourceGroupName $Location

$certThumbprint, $certPassword, $certPath = CreateSelfSignedCertificate $Name
$keyVaultCert = ImportCertificateIntoKeyVault $KeyValutName $Name $certPath $certPassword

