# Temporary script for generating certificates and related files in the correct location
# until a suitable cross-platform (PS Core) replacement can be added to Common.ps1

param(
    [string] [Parameter(Mandatory = $true)] $Name
)

# Add utility methods
. "$PSScriptRoot\Common.ps1"

# Generate certificate
CreateSelfSignedCertificate $Name