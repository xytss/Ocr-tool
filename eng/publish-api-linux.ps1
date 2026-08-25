param(
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectRoot 'src\OcrTool.Api\OcrTool.Api.csproj'
$output = Join-Path $projectRoot 'artifacts\publish\api-linux-x64'
$apiGuide = Join-Path $projectRoot 'docs\API使用说明.txt'

$publishArguments = @(
    'publish',
    $project,
    '--configuration', 'Release',
    '--runtime', 'linux-x64',
    '--self-contained', 'true',
    '--output', $output,
    ('-p:Version={0}' -f $Version),
    ('-p:AssemblyVersion={0}.0' -f $Version),
    ('-p:FileVersion={0}.0' -f $Version),
    ('-p:InformationalVersion={0}' -f $Version),
    '-p:IncludeSourceRevisionInInformationalVersion=false',
    '-p:DebugType=None',
    '-p:DebugSymbols=false'
)
& (Join-Path $PSScriptRoot 'dotnet.ps1') @publishArguments

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -LiteralPath $apiGuide -Destination (Join-Path $output 'API使用说明.txt')
