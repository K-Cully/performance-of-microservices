function Clean-BuildFolders([string]$RootPath)
{
    Get-ChildItem $RootPath -Recurse | Where-Object { $_.Name -Match "^obj$|^bin$" } | Remove-Item -Recurse
}


function Read-AppConfig([string]$Path)
{
    if (-Not (Test-Path -Path $Path)) {
        Write-Error -Message "File $Path not found"
        exit 1
    }
    
    $config = Get-Content $Path | ConvertFrom-Json -AsHashtable
    if ((-Not $config.services) -or $config.services.Count -eq 0)
    {
        Write-Error -Message "No services are sepcified in $Path"
        exit 1
    }

    $config
}


function Validate-ServiceConfig([hashtable]$Config, [hashtable]$PortAssignments)
{
    if (-Not $Config.port) {
        Write-Error -Message "port is missing from service $serviceName"
        exit 1
    }
    elseif($PortAssignments["$($Config.port)"]) {
        $port = $Config.port
        $otherService = $PortAssignments["$port"]
        Write-Error -Message "port $port is already used by $otherService, cannot create $serviceName"
        exit 1
    }
    elseif ((-Not $Config.processors) -or $Config.processors.Count -eq 0) {
        Write-Error -Message "processors are missing from service $serviceName"
        exit 1
    }
    elseif ((-Not $Config.steps) -or $Config.steps.Count -eq 0) {
        Write-Error -Message "steps are missing from service $serviceName"
        exit 1
    }
}

function Build-SettingCompatibleJson($TreeNode)
{
    if ($TreeNode -is [string]) {
        $json = @"
'$TreeNode', 
"@
    }
    elseif ($TreeNode -is [array]) {
        $inner = ""
        foreach ($item in $TreeNode) {
            $itemJson = Build-SettingCompatibleJson($item)
            $inner = @"
$inner$itemJson
"@
        }

        $inner = $inner.TrimEnd(',', ' ')
        $json = @"
[ $inner ], 
"@
    }
    elseif ($TreeNode -is [hashtable]) {
        $inner = ""
        foreach ($name in $TreeNode.Keys) {
            $value = $TreeNode[$name]
            $childJson = Build-SettingCompatibleJson($value)
            $inner = @"
$inner$name : $childJson
"@
        }
        
        $inner = $inner.TrimEnd(',', ' ')
        $json = @"
{ $inner }, 
"@
    }
    else {
        $json = "$TreeNode, "
    }

    return $json
}


function Create-Setting([string]$Name, [hashtable]$Value)
{
    $json = Build-SettingCompatibleJson($Value)
    $json = $json.TrimEnd(',', ' ')

    $template = @"
<Parameter Name="$Name" Value="$json" />
"@
    $template
}


function Replace-Placeholder([string] $File, [string] $Placeholder, [string] $Value)
{
    Write-Host -Message "Replacing $Placeholder in $File"
    ((Get-Content -path $File -Raw) -replace $Placeholder, $Value) | Set-Content -Path $File
}


function Generate-Settings([hashtable] $Section, [string] $Placeholder, [string] $File)
{
    if (-Not $Section) {
        Write-Warning -Message "No setting source was not specified, ignoring."
        return
    }

    if (-Not (Test-Path -Path $File)) {
        Write-Error -Message "No file was not found at $File"
        exit 1
    }

    # Create and append settings
    $xmlSettings = ""
    foreach ($nodeName in $Section.Keys) {
        $setting = Create-Setting -Name $nodeName -Value $Section[$nodeName]
        $xmlSettings = @"
$xmlSettings$setting
    
"@
    }

    # Trim trailing spaces, newline and replace in output file
    $xmlSettings = $xmlSettings.TrimEnd(' ', [char]0x000A, [char]0x000D)
    Replace-Placeholder -File $File -Placeholder $Placeholder -Value $xmlSettings
}


function Set-ApplicationManifestPlaceholders([string] $File, [hashtable] $PortsAndServices)
{
    Write-Host -Message "Settings application manifest service values"
    foreach ($port in $PortsAndServices.Keys) {
        $serviceName = $PortsAndServices[$port]
        Write-Warning -Message $port
        Write-Warning -Message $serviceName

        $parameters = @"
$parameters
    <Parameter Name="$($serviceName)Service_ASPNETCORE_ENVIRONMENT" DefaultValue="" />
    <Parameter Name="$($serviceName)Service_InstanceCount" DefaultValue="-1" />
"@
        $manifests = @"
$manifests
  <ServiceManifestImport>
      <ServiceManifestRef ServiceManifestName="$($serviceName)ServicePkg" ServiceManifestVersion="1.0.0" />
      <EnvironmentOverrides CodePackageRef="code">
          <EnvironmentVariable Name="ASPNETCORE_ENVIRONMENT" Value="[$($serviceName)Service_ASPNETCORE_ENVIRONMENT]" />
      </EnvironmentOverrides>
  </ServiceManifestImport>
"@
        $services = @"
$services
    <Service Name="$($serviceName)Service" ServicePackageActivationMode="ExclusiveProcess">
        <StatelessService ServiceTypeName="$($serviceName)ServiceType" InstanceCount="[$($serviceName)Service_InstanceCount]">
            <SingletonPartition />
        </StatelessService>
    </Service>
"@
    }

    $remove = ([char]0x000A, [char]0x000D)
    $parameters = $parameters.TrimEnd($remove).TrimStart($remove)
    $manifests = $manifests.TrimEnd($remove).TrimStart($remove)
    $services = $services.TrimEnd($remove).TrimStart($remove)

    Replace-Placeholder -File $File -Placeholder "<!-- Parameters_Placeholder -->" -Value $parameters
    Replace-Placeholder -File $File -Placeholder "<!-- ServiceManifests_Placeholder -->" -Value $manifests
    Replace-Placeholder -File $File -Placeholder "<!-- Services_Placeholder -->" -Value $services
}


function Package-Service([string] $Name, [string] $Location, [string] $PackageRoot, [string] $Flavour)
{
    Push-Location -Path "$Location\$Name" # Move to project

    $packagePath = "$PackageRoot\$Name" + "ServicePkg"
    Write-Host "Packing $Name to $packagePath"

    # Build and publish service code to package and suppress console output
    $packageCode = "$packagePath\Code"
    $junk = dotnet publish -c $flavour -o $packageCode

    # Copy service manifest
    Copy-Item -Path ".\PackageRoot\ServiceManifest.xml" -Destination $packagePath -Force
    
    # Create config folder and copy service settings
    $configPath = "$packagePath\Config"
    New-Item -ItemType Directory -Force -Path $configPath
    Copy-Item -Path ".\PackageRoot\Config\Settings.xml" -Destination $configPath -Force
    
    Pop-Location # Leave project
}