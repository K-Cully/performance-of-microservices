function Clean-BuildFolders([string]$RootPath)
{
    Get-ChildItem $RootPath -Recurse | Where{$_.Name -Match "^obj$|^bin$"} | Remove-Item -Recurse
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
    elseif($PortAssignments[$Config.port]) {
        $port = $Config.port
        $otherService = $PortAssignments[$port]
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
    Write-Host -Message $template
}