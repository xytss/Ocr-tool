param(
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dotnetScript = Join-Path $PSScriptRoot 'dotnet.ps1'
$appProject = Join-Path $repositoryRoot 'src\OcrTool.App\OcrTool.App.csproj'
$apiProject = Join-Path $repositoryRoot 'src\OcrTool.Api\OcrTool.Api.csproj'
$installerProject = Join-Path $repositoryRoot 'src\OcrTool.Installer\OcrTool.Installer.wixproj'
$releaseRoot = Join-Path $repositoryRoot "artifacts\release\$Version"
$portableRoot = Join-Path $releaseRoot 'OcrTool'
$apiPublishRoot = Join-Path $portableRoot 'api'
$portableArchive = Join-Path $releaseRoot "OcrTool-$Version-win-x64-portable.zip"
$userGuide = Join-Path $repositoryRoot 'docs\使用说明.txt'
$apiGuide = Join-Path $repositoryRoot 'docs\API使用说明.txt'
$thirdPartyNotices = Join-Path $repositoryRoot 'docs\THIRD-PARTY-NOTICES.txt'
$portableMarker = Join-Path $repositoryRoot 'docs\portable.flag'
$verificationScript = Join-Path $PSScriptRoot 'verify-windows-release.ps1'

$publishArguments = @(
    'publish',
    $appProject,
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-o', $portableRoot,
    ('-p:Version={0}' -f $Version),
    ('-p:AssemblyVersion={0}.0' -f $Version),
    ('-p:FileVersion={0}.0' -f $Version),
    ('-p:InformationalVersion={0}' -f $Version),
    '-p:IncludeSourceRevisionInInformationalVersion=false',
    '-p:DebugType=None',
    '-p:DebugSymbols=false'
)
& $dotnetScript @publishArguments
if ($LASTEXITCODE -ne 0)
{
    throw '绿色版发布失败。'
}

$apiPublishArguments = @(
    'publish',
    $apiProject,
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-o', $apiPublishRoot,
    ('-p:Version={0}' -f $Version),
    ('-p:AssemblyVersion={0}.0' -f $Version),
    ('-p:FileVersion={0}.0' -f $Version),
    ('-p:InformationalVersion={0}' -f $Version),
    '-p:IncludeSourceRevisionInInformationalVersion=false',
    '-p:DebugType=None',
    '-p:DebugSymbols=false'
)
& $dotnetScript @apiPublishArguments
if ($LASTEXITCODE -ne 0)
{
    throw 'Windows API 发布失败。'
}

Copy-Item -LiteralPath $userGuide -Destination (Join-Path $portableRoot '使用说明.txt')
Copy-Item -LiteralPath $apiGuide -Destination (Join-Path $portableRoot 'API使用说明.txt')
Copy-Item -LiteralPath $thirdPartyNotices -Destination (Join-Path $portableRoot 'THIRD-PARTY-NOTICES.txt')
Copy-Item -LiteralPath $portableMarker -Destination (Join-Path $portableRoot 'portable.flag')

Compress-Archive -LiteralPath $portableRoot -DestinationPath $portableArchive -CompressionLevel 'Optimal' -Force

$installerArguments = @(
    'build',
    $installerProject,
    '-c', 'Release',
    ('-p:ProductVersion={0}' -f $Version),
    ('-p:AppPublishDir={0}' -f $portableRoot),
    ('-p:OutputPath={0}' -f $releaseRoot)
)
& $dotnetScript @installerArguments
if ($LASTEXITCODE -ne 0)
{
    throw '安装版构建失败。'
}

& $verificationScript -Version $Version
